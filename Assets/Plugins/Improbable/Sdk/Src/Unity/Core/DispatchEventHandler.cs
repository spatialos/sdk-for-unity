// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Entity;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Responsible for responding to SDK callbacks.
    /// </summary>
    class DispatchEventHandler :
        IDispatchEventHandler,
        IDisposable,
        IComponentInterestOverridesUpdateReceiver,
        IInterestedComponentUpdaterProvider,
        IAuthorityChangedReceiver
    {
        private readonly SpatialCommunicator spatialCommunicator;
        private readonly AuthorityChangedNotifier authorityChangedNotifier;

        private readonly EntityComponentInterestOverridesUpdater entityInterestedComponentsUpdater;
        private readonly List<OnAuthorityChangedCallback> authorityChangedCallbacks = new List<OnAuthorityChangedCallback>();


        internal DispatchEventHandler(MonoBehaviour hostBehaviour, SpatialCommunicator spatialCommunicator)
        {
            entityInterestedComponentsUpdater = new EntityComponentInterestOverridesUpdater(hostBehaviour, SpatialOS.WorkerComponentInterestModifier);
            entityInterestedComponentsUpdater.AddUpdateReceiver(this);

            this.spatialCommunicator = spatialCommunicator;

            authorityChangedNotifier = new AuthorityChangedNotifier(LocalEntities.Instance);
            authorityChangedNotifier.RegisterAuthorityChangedReceiver(this);
        }

        internal void ConnectionReady()
        {
            EntityPipeline.Internal.RegisterComponentFactories(SpatialOS.Connection, SpatialOS.Dispatcher);

            SpatialOS.Dispatcher.OnLogMessage(OnLogMessage);
            SpatialOS.Dispatcher.OnMetrics(OnMetrics);

            EntityPipeline.Instance.AddBlock(authorityChangedNotifier);
            EntityPipeline.Internal.Start(spatialCommunicator);
        }

        /// <summary>
        ///     Should be called regularly (usually every frame.)
        /// </summary>
        internal void ProcessEvents()
        {
            EntityPipeline.Internal.ProcessOps();
        }

        /// <inheritdoc />
        public event OnAuthorityChangedCallback OnAuthorityChanged
        {
            add { authorityChangedCallbacks.Add(value); }
            remove { authorityChangedCallbacks.Remove(value); }
        }

        private void OnLogMessage(LogMessageOp op)
        {
            switch (op.Level)
            {
                case LogLevel.Debug:
                    Debug.Log(op.Message);
                    break;
                case LogLevel.Info:
                    Debug.Log(op.Message);
                    break;
                case LogLevel.Warn:
                    Debug.LogWarning(op.Message);
                    break;
                case LogLevel.Error:
                    Debug.LogError(op.Message);
                    break;
                case LogLevel.Fatal:
                    Debug.LogError(op.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnMetrics(MetricsOp metrics)
        {
            SpatialOS.Connection.SendMetrics(metrics.Metrics);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            authorityChangedNotifier.TryRemoveAuthorityChangedReceiver(this);

            entityInterestedComponentsUpdater.RemoveUpdateReceiver(this);
            entityInterestedComponentsUpdater.Dispose();
        }

        /// <inheritdoc />
        public void OnComponentInterestOverridesUpdated(EntityId entity, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides)
        {
            spatialCommunicator.RegisterComponentInterest(entity, interestOverrides);
        }

        /// <inheritdoc />
        public IEntityComponentInterestOverridesUpdater GetEntityInterestedComponentsUpdater()
        {
            return entityInterestedComponentsUpdater;
        }

        /// <inheritdoc />
        public void AuthorityChanged(EntityId entityId, IComponentMetaclass componentId, Authority authority, object component)
        {
            spatialCommunicator.TriggerComponentAuthorityChanged(entityId, componentId, authority, component);
            for (int i = 0; i < authorityChangedCallbacks.Count; ++i)
            {
                try
                {
                    authorityChangedCallbacks[i](entityId, componentId, authority, component);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal IEntityComponentInterestOverridesUpdater EntityInterestedComponentsUpdater
        {
            get { return entityInterestedComponentsUpdater; }
        }
    }
}
