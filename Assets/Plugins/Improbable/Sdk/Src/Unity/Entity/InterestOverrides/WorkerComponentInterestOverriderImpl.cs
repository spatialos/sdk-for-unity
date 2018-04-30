// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Worker;

namespace Improbable.Unity.Entity
{
    /// <inheritdoc />
    internal class WorkerComponentInterestOverriderImpl : IWorkerComponentInterestOverrider
    {
        private readonly Dictionary<uint, WorkerComponentInterest> globalInterests;
        private readonly Dictionary<EntityId, Dictionary<uint, WorkerComponentInterest>> perEntityInterests;

        public WorkerComponentInterestOverriderImpl()
        {
            globalInterests = new Dictionary<uint, WorkerComponentInterest>();
            perEntityInterests = new Dictionary<EntityId, Dictionary<uint, WorkerComponentInterest>>();
        }

        /// <summary>
        ///     Gets or sets the target to be notified when interest has changed.
        /// </summary>
        public Action<IEntityObject> InterestInvalidationHandler { get; set; }

        /// <summary>
        ///     Gets or sets the source of all EntityObjects that exist on the current worker.
        /// </summary>
        public ILocalEntities EntityObjects { get; set; }

        public void SetComponentInterest<T>(WorkerComponentInterest interest) where T : IComponentMetaclass
        {
            var shouldNotify = ApplyInterestChanges<T>(globalInterests, interest);

            if (InterestInvalidationHandler == null || EntityObjects == null || !shouldNotify)
            {
                return;
            }

            foreach (var obj in EntityObjects.GetAll())
            {
                // Only notify that a global interest has changed if it's not overridden on the entity.
                if (!perEntityInterests.ContainsKey(obj.EntityId))
                {
                    InterestInvalidationHandler(obj);
                }
            }
        }

        public void SetComponentInterest<T>(EntityId entityId, WorkerComponentInterest interest) where T : IComponentMetaclass
        {
            var shouldNotify = true;
            var componentId = Dynamic.GetComponentId<T>();
            if (perEntityInterests.ContainsKey(entityId))
            {
                // We've already setup interests, so modify what's there.
                var workerComponentInterests = perEntityInterests[entityId];
                shouldNotify = ApplyInterestChanges<T>(workerComponentInterests, interest);

                // Clean up if no overrides remain for the entity.
                if (workerComponentInterests.Count == 0)
                {
                    perEntityInterests.Remove(entityId);
                }
            }
            else if (interest == WorkerComponentInterest.Default)
            {
                // No previous interest to remove/update, nothing has changed.
                shouldNotify = false;
            }
            else
            {
                // Create the first interest override
                perEntityInterests[entityId] = new Dictionary<uint, WorkerComponentInterest> { { componentId, interest } };
            }

            if (!shouldNotify || InterestInvalidationHandler == null)
            {
                return;
            }

            var obj = EntityObjects.Get(entityId);
            if (obj != null)
            {
                InterestInvalidationHandler(obj);
            }
        }

        public IEnumerator<KeyValuePair<uint, WorkerComponentInterest>> GetComponentInterest(EntityId entityId)
        {
            // This function "merges" the global interest overrides collection with a per-entity interest overrides collection (if present.)

            Dictionary<uint, WorkerComponentInterest> specificInterests;
            perEntityInterests.TryGetValue(entityId, out specificInterests);

            // First, return all non-overridden global interests.
            foreach (var kv in globalInterests)
            {
                // Only return a global override if it's not been overridden later by an entity-specific override.
                if (specificInterests == null || !specificInterests.ContainsKey(kv.Key))
                {
                    yield return new KeyValuePair<uint, WorkerComponentInterest>(kv.Key, kv.Value);
                }
            }

            // Then, move on to entity-specific interests, if any.
            if (specificInterests == null)
            {
                yield break;
            }

            foreach (var interest in specificInterests)
            {
                yield return interest;
            }
        }

        private static bool ApplyInterestChanges<T>(IDictionary<uint, WorkerComponentInterest> dict, WorkerComponentInterest interest) where T : IComponentMetaclass
        {
            var shouldNotify = true;

            var componentId = Dynamic.GetComponentId<T>();
            switch (interest)
            {
                case WorkerComponentInterest.Default:
                    // Only notify if interest was previously overridden.
                    shouldNotify = dict.Remove(componentId);
                    break;
                case WorkerComponentInterest.Always:
                case WorkerComponentInterest.Never:
                    WorkerComponentInterest oldInterest;
                    if (dict.TryGetValue(componentId, out oldInterest) && oldInterest == interest)
                    {
                        // The value of the interest isn't changing.
                        shouldNotify = false;
                    }
                    else
                    {
                        dict[componentId] = interest;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("interest", interest, null);
            }

            return shouldNotify;
        }
    }
}
