// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using Improbable.Unity.Entity;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity
{
    /// <inheritdoc cref="IEntityComponentInterestOverridesUpdater" />
    /// <summary>
    ///     Reports interested component overrides to SpatialOS.
    /// </summary>
    class EntityComponentInterestOverridesUpdater : IEntityComponentInterestOverridesUpdater, IDisposable
    {
        private readonly MonoBehaviour coroutineHostBehaviour;
        private readonly IWorkerComponentInterestOverrider interestOverrider;
        private Coroutine updateCoroutine;
        private readonly HashSet<IEntityObject> entitiesToBeUpdated;
        private readonly IDictionary<IEntityObject, HashSet<uint>> updatesSentCache;
        private readonly IList<IComponentInterestOverridesUpdateReceiver> updateReceiversList;
        private static readonly HashSet<uint> EmptyComponentSet = new HashSet<uint>();

        public EntityComponentInterestOverridesUpdater(MonoBehaviour coroutineHostBehaviour, IWorkerComponentInterestOverrider interestOverrider)
        {
            entitiesToBeUpdated = new HashSet<IEntityObject>();
            updatesSentCache = new Dictionary<IEntityObject, HashSet<uint>>();
            updateReceiversList = new List<IComponentInterestOverridesUpdateReceiver>();
            this.coroutineHostBehaviour = coroutineHostBehaviour;
            this.interestOverrider = interestOverrider;
        }

        /// <inheritdoc />
        public void InvalidateEntity(IEntityObject entityObject)
        {
            entitiesToBeUpdated.Add(entityObject);
            ScheduleInterestedComponentsUpdate();
        }

        /// <inheritdoc />
        public void RemoveEntity(IEntityObject entityObject)
        {
            entitiesToBeUpdated.Remove(entityObject);
            updatesSentCache.Remove(entityObject);
        }

        /// <inheritdoc />
        public void AddUpdateReceiver(IComponentInterestOverridesUpdateReceiver updateReceiver)
        {
            updateReceiversList.Add(updateReceiver);
        }

        /// <inheritdoc />
        public void RemoveUpdateReceiver(IComponentInterestOverridesUpdateReceiver updateReceiver)
        {
            updateReceiversList.Remove(updateReceiver);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (updateCoroutine != null)
            {
                coroutineHostBehaviour.StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }

            entitiesToBeUpdated.Clear();
            updateReceiversList.Clear();
        }

        /// <summary>
        ///     Wait until the end of the current frame and send an InterestedComponent update once per frame per entity.
        /// </summary>
        private void ScheduleInterestedComponentsUpdate()
        {
            if (updateCoroutine == null)
            {
                updateCoroutine = coroutineHostBehaviour.StartCoroutine(UpdateInterestedComponentsCoroutine());
            }
        }

        private IEnumerator UpdateInterestedComponentsCoroutine()
        {
            yield return new WaitForEndOfFrame();
            UpdateInterestedComponents();
            updateCoroutine = null;
        }

        internal void UpdateInterestedComponents()
        {
            if (entitiesToBeUpdated.Count == 0)
            {
                return;
            }

            foreach (var entityObject in entitiesToBeUpdated)
            {
                var interestedComponents = new HashSet<uint>(entityObject.Components.RegisteredComponents.Keys);
                var interestedComponentsVisualizers = entityObject.Visualizers.RequiredComponents;

                // Apply calculated component interest
                foreach (var visualizerComponent in interestedComponentsVisualizers)
                {
                    interestedComponents.Add(visualizerComponent);
                }

                // Then apply user overrides to component interest.
                using (var enumerator = interestOverrider.GetComponentInterest(entityObject.EntityId))
                {
                    while (enumerator.MoveNext())
                    {
                        ApplyComponentInterest(enumerator.Current.Key, enumerator.Current.Value, interestedComponents);
                    }
                }

                if (!EntityNeedsUpdate(entityObject, interestedComponents))
                {
                    continue;
                }

                // Now convert to the CoreSdk's format.
                var cache = updatesSentCache.ContainsKey(entityObject) ? updatesSentCache[entityObject] : EmptyComponentSet;
                var interestOverrides = new Dictionary<uint, InterestOverride>();
                foreach (var interestedComponent in interestedComponents)
                {
                    if (!cache.Contains(interestedComponent))
                    {
                        interestOverrides.Add(interestedComponent, new InterestOverride { IsInterested = true });
                    }
                }

                foreach (var cachedInterestedComponent in cache)
                {
                    if (!interestedComponents.Contains(cachedInterestedComponent))
                    {
                        interestOverrides.Add(cachedInterestedComponent, new InterestOverride { IsInterested = false });
                    }
                }

                // Then trigger callbacks.
                for (int i = 0; i < updateReceiversList.Count; i++)
                {
                    updateReceiversList[i].OnComponentInterestOverridesUpdated(entityObject.EntityId, interestOverrides);
                }

                // Finally, cache the results.
                updatesSentCache[entityObject] = interestedComponents;
            }

            entitiesToBeUpdated.Clear();
        }

        private static void ApplyComponentInterest(uint componentId, WorkerComponentInterest interest, HashSet<uint> interestedComponents)
        {
            switch (interest)
            {
                case WorkerComponentInterest.Default:
                    Debug.LogErrorFormat("Internal logic error: {0} is not valid for component interest.", Enum.GetName(typeof(WorkerComponentInterest), WorkerComponentInterest.Default));
                    break;
                case WorkerComponentInterest.Always:
                    interestedComponents.Add(componentId);
                    break;
                case WorkerComponentInterest.Never:
                    interestedComponents.Remove(componentId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("interest", interest, null);
            }
        }

        private bool EntityNeedsUpdate(IEntityObject entity, HashSet<uint> components)
        {
            return !updatesSentCache.ContainsKey(entity) || !updatesSentCache[entity].SetEquals(components);
        }
    }
}
