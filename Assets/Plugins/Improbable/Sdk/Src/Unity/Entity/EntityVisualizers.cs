// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Visualizer;
using Improbable.Util.Injection;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Manages the enabling and disabling of visualizers
    /// </summary>
    class EntityVisualizers : IEntityVisualizers, IDisposable
    {
        private class DefaultMonobehaviourActivator : IMonobehaviourActivator
        {
            public void Enable(MonoBehaviour monoBehaviour)
            {
                monoBehaviour.enabled = true;
            }

            public void Disable(MonoBehaviour monoBehaviour)
            {
                monoBehaviour.enabled = false;
            }
        }

        private static readonly DefaultMonobehaviourActivator DefaultActivator = new DefaultMonobehaviourActivator();

        private readonly HashSet<MonoBehaviour> disabledVisualizers = new HashSet<MonoBehaviour>();
        private readonly HashSet<uint> requiredComponents = new HashSet<uint>();
        private bool requiredComponentsUpToDate;
        private bool disposing;
        private readonly HashSet<uint> authoritativeComponents = new HashSet<uint>();
        private readonly IList<IEntityInterestedComponentsInvalidator> invalidatorsList;

        private IMonobehaviourActivator activator = DefaultActivator;

        /// <summary>
        ///     Create a new instance of <c>EntityVisualizers</c>.
        /// </summary>
        /// <param name="underlyingGameObject">The GameObject associated with the entity.</param>
        internal EntityVisualizers(GameObject underlyingGameObject)
        {
            if (underlyingGameObject == null)
            {
                throw new ArgumentNullException("underlyingGameObject");
            }

            ExtractedVisualizers = VisualizerMetadataLookup.ExtractVisualizers(underlyingGameObject);


            OnUserException = Debug.LogException;
            invalidatorsList = new List<IEntityInterestedComponentsInvalidator>();
            Initialize();
        }

        /// <inheritdoc />
        public IList<MonoBehaviour> ExtractedVisualizers { get; private set; }

        /// <summary>
        ///     For testing use only, i.e. this method should not be called by user code.
        /// </summary>
        /// <returns>A copy of the required components.</returns>
        internal HashSet<uint> GetCopyOfRequiredComponents()
        {
            return new HashSet<uint>(requiredComponents);
        }

        /// <inheritdoc />
        public HashSet<uint> RequiredComponents
        {
            get
            {
                if (!requiredComponentsUpToDate)
                {
                    CalculateRequiredComponents();
                }

                return requiredComponents;
            }
        }

        /// <inheritdoc />
        public Action<Exception, UnityEngine.Object> OnUserException { get; set; }

        /// <summary>
        ///     Registers <see cref="IMonobehaviourActivator" /> to be used intead
        ///     of the simple default implementation.
        /// </summary>
        internal void RegisterActivationController(IMonobehaviourActivator activatorToUse)
        {
            this.activator = activatorToUse;
        }

        /// <summary>
        ///     Disposing of entity visualizers is a subtle process. First, the visualisers are deactivated,
        ///     which can run user code --- this user code must run in an environment where everything
        ///     generally still works as expected; the exception is that calls to public methods of this
        ///     class will be ignored, because they could interfere with the disposal. Then, all event
        ///     handlers are removed; this should not trigger any events in itself. Only then, it is safe
        ///     to null visualizer fields.
        /// </summary>
        public void Dispose()
        {
            disposing = true;
            DeactivateVisualizers();
            NullVisualizerFields();
            invalidatorsList.Clear();
            disposing = false;
        }

        /// <inheritdoc />
        public void TryEnableVisualizers(IList<MonoBehaviour> visualizersToEnable)
        {
            if (disposing)
            {
                return;
            }

            var enabledVisualizersCount = 0;
            for (var i = 0; i < visualizersToEnable.Count; i++)
            {
                var visualizer = visualizersToEnable[i];
                if (IsMarkedAsDisabled(visualizer))
                {
                    enabledVisualizersCount++;
                    EnableVisualizer(visualizer);
                }
                else
                {
                    Debug.LogWarningFormat("Visualiser {0} was not previously disabled, cannot enable.", visualizer.GetType().Name);
                }
            }

            if (enabledVisualizersCount > 0)
            {
                TriggerRequiredComponentsPotentiallyChanged();
            }
        }

        /// <inheritdoc />
        public void DisableVisualizers(IList<MonoBehaviour> visualizersToDisable)
        {
            if (disposing)
            {
                return;
            }

            var disabledVisualizersCount = 0;
            for (var i = 0; i < visualizersToDisable.Count; i++)
            {
                var visualizer = visualizersToDisable[i];
                if (!IsMarkedAsDisabled(visualizer))
                {
                    disabledVisualizersCount++;
                    DisableVisualizer(visualizer);
                }
            }

            if (disabledVisualizersCount > 0)
            {
                TriggerRequiredComponentsPotentiallyChanged();
            }
        }

        private void DeactivateVisualizers()
        {
            for (var i = 0; i < ExtractedVisualizers.Count; i++)
            {
                Deactivate(ExtractedVisualizers[i]);
            }
        }

        private void NullVisualizerFields()
        {
            for (var i = 0; i < ExtractedVisualizers.Count; i++)
            {
                NullAllFields(ExtractedVisualizers[i]);
            }
        }

        private void Activate(MonoBehaviour visualizer)
        {
            try
            {
                activator.Enable(visualizer);
            }
            catch (Exception ex)
            {
                if (OnUserException != null)
                {
                    OnUserException(ex, visualizer);
                }
            }
        }

        private void Deactivate(MonoBehaviour visualizer)
        {
            try
            {
                activator.Disable(visualizer);
            }
            catch (Exception ex)
            {
                if (OnUserException != null)
                {
                    OnUserException(ex, visualizer);
                }
            }
        }

        /// <summary>
        ///     For internal use only, i.e. this method should not be called by user code.
        /// </summary>
        internal void OnComponentAdded(IComponentMetaclass componentMetaclass, object component)
        {
            for (var i = 0; i < ExtractedVisualizers.Count; i++)
            {
                InjectReaders(ExtractedVisualizers[i], component);
            }

            TriggerRequiredComponentsPotentiallyChanged();
        }

        /// <summary>
        ///     For internal use only, i.e. this method should not be called by user code.
        /// </summary>
        internal void OnComponentRemoved(IComponentMetaclass componentMetaclass, object component)
        {
            for (var i = 0; i < ExtractedVisualizers.Count; i++)
            {
                InjectNullAndDisable(ExtractedVisualizers[i], componentMetaclass, component.GetType());
            }

            TriggerRequiredComponentsPotentiallyChanged();
        }

        /// <summary>
        ///     For internal use only, i.e. this method should not be called by user code.
        /// </summary>
        internal void OnAuthorityChanged(IComponentMetaclass componentMetaclass, Authority authority, object component)
        {
            for (var i = 0; i < ExtractedVisualizers.Count; i++)
            {
                var visualizer = ExtractedVisualizers[i];
                var field = VisualizerMetadataLookup.Instance.GetFieldInfo(component.GetType(), visualizer.GetType());
                if (field != null && VisualizerMetadataLookup.Instance.IsWriter(field))
                {
                    if (authority == Authority.Authoritative || authority == Authority.AuthorityLossImminent)
                    {
                        InjectField(visualizer, field, component);
                        authoritativeComponents.Add(componentMetaclass.ComponentId);
                        UpdateActivation(visualizer);
                    }
                    else
                    {
                        Deactivate(visualizer);
                        field.SetValue(visualizer, null);
                        authoritativeComponents.Remove(componentMetaclass.ComponentId);
                    }
                }
            }

            TriggerRequiredComponentsPotentiallyChanged();
        }

        private void DisableVisualizer(MonoBehaviour visualizer)
        {
            disabledVisualizers.Add(visualizer);
            UpdateActivation(visualizer);
        }

        private void EnableVisualizer(MonoBehaviour visualizer)
        {
            disabledVisualizers.Remove(visualizer);
            UpdateActivation(visualizer);
        }

        private void Initialize()
        {
            for (var i = 0; i < ExtractedVisualizers.Count; i++)
            {
                var visualizer = ExtractedVisualizers[i];

                if (VisualizerMetadataLookup.Instance.DontEnableOnStart(visualizer.GetType()))
                {
                    disabledVisualizers.Add(visualizer);
                }

                UpdateActivation(visualizer);
            }

            TriggerRequiredComponentsPotentiallyChanged();
        }

        private bool IsMarkedAsDisabled(MonoBehaviour visualizer)
        {
            return disabledVisualizers.Contains(visualizer);
        }

        /// <inheritdoc />
        public void AddInvalidator(IEntityInterestedComponentsInvalidator invalidator)
        {
            invalidatorsList.Add(invalidator);
        }

        /// <inheritdoc />
        public void RemoveInvalidator(IEntityInterestedComponentsInvalidator invalidator)
        {
            invalidatorsList.Remove(invalidator);
        }

        private void TriggerRequiredComponentsPotentiallyChanged()
        {
            requiredComponentsUpToDate = false;
            for (int i = 0; i < invalidatorsList.Count; i++)
            {
                invalidatorsList[i].OnInterestedComponentsPotentiallyChanged();
            }
        }

        internal void CalculateRequiredComponents()
        {
            requiredComponents.Clear();
            for (var i = 0; i < ExtractedVisualizers.Count; i++)
            {
                var visualizer = ExtractedVisualizers[i];

                if (!IsMarkedAsDisabled(visualizer) && AllFieldWritersInjected(visualizer))
                {
                    var visualizerType = visualizer.GetType();
                    var requiredReaders = VisualizerMetadataLookup.Instance.GetRequiredReaderComponentIds(visualizerType);
                    // NOTE: Using indexed for and Set.Add instead of UnionWith because UnionWith allocates memory (enumerators)
                    for (var componentIndex = 0; componentIndex < requiredReaders.Count; componentIndex++)
                    {
                        requiredComponents.Add(requiredReaders[componentIndex]);
                    }
                }
            }

            requiredComponentsUpToDate = true;
        }

        private void NullAllFields(object visualizer)
        {
            var fields = VisualizerMetadataLookup.Instance.GetRequiredReadersWriters(visualizer.GetType());
            for (var index = 0; index < fields.Count; index++)
            {
                var fieldInfo = fields[index];
                fieldInfo.SetValue(visualizer, null);
            }
        }

        private void InjectReaders(MonoBehaviour visualizer, object component)
        {
            var field = VisualizerMetadataLookup.Instance.GetFieldInfo(component.GetType(), visualizer.GetType());
            if (field != null && !VisualizerMetadataLookup.Instance.IsWriter(field))
            {
                InjectField(visualizer, field, component);
                UpdateActivation(visualizer);
            }
        }

        private void InjectField(object visualizer, IMemberAdapter field, object component)
        {
            field.SetValue(visualizer, component);
        }

        private void UpdateActivation(MonoBehaviour visualizer)
        {
            if (IsMarkedAsDisabled(visualizer))
            {
                Deactivate(visualizer);
            }
            else if (VisualizerMetadataLookup.Instance.AreAllRequiredFieldsInjectable(visualizer.GetType())
                     && AllFieldReadersAndWritersInjected(visualizer))
            {
                Activate(visualizer);
            }
        }

        private bool AllFieldReadersAndWritersInjected(object visualizer)
        {
            var required = VisualizerMetadataLookup.Instance.GetRequiredReadersWriters(visualizer.GetType());
            return AllFieldsInjected(visualizer, required);
        }

        private bool AllFieldWritersInjected(object visualizer)
        {
            var requiredWriters = VisualizerMetadataLookup.Instance.GetRequiredWriters(visualizer.GetType());
            return AllFieldsInjected(visualizer, requiredWriters);
        }

        private static bool AllFieldsInjected(object visualizer, IList<IMemberAdapter> fields)
        {
            for (var index = 0; index < fields.Count; index++)
            {
                var fieldInfo = fields[index];
                if (fieldInfo.GetValue(visualizer) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void InjectNullAndDisable(MonoBehaviour visualizer, IComponentMetaclass componentMetaclass, Type componentType)
        {
            var field = VisualizerMetadataLookup.Instance.GetFieldInfo(componentType, visualizer.GetType());
            if (field == null)
            {
                return;
            }

            Deactivate(visualizer);
            field.SetValue(visualizer, null);
        }
    }
}
