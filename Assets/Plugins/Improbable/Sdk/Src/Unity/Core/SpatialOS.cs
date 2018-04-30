// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Configuration;
using Improbable.Unity.Entity;
using Improbable.Unity.Logging;
using Improbable.Unity.Metrics;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     This class provides an interface for customising a worker, connecting and disconnecting from SpatialOS.
    /// </summary>
    public static class SpatialOS
    {
        private static WorkerConfiguration configuration;
        private static ConnectionLifecycle connectionLifecycle;
        private static IEnumerable<KeyValuePair<string, int>> assetsToPrePool;
        private static IEntityTemplateProvider templateProvider;

        private static readonly List<Action<DisconnectOp>> onDisconnected = new List<Action<DisconnectOp>>();
        private static readonly List<Action> onConnected = new List<Action>();

        private static readonly List<Action> onConnectionFailed = new List<Action>();
        private static readonly List<Action<string>> onConnectionFailedWithReason = new List<Action<string>>();

        private static readonly WorkerComponentInterestOverriderImpl WorkerComponentInterestImpl;

        private static IList<string> assetsToPrecache;
        private static int maxConcurrentPrecacheConnections = 5;
        private static Action onPrecachingCompleted;
        private static Action<int> onPrecacheProgress;
        private static WorkerMetrics metrics;
        private static DisconnectOp receivedDisconnectOp;

        private static ILogFilterReceiver logFilter;
        private static IEntitySpawnLimiter entitySpawnLimiter;

        static SpatialOS()
        {
            ClientError.ExceptionCallback = Debug.LogException;

            // Set this up here so that users have flexibility in how they manage their global overrides before they've connected to SpatialOS.
            WorkerComponentInterestImpl = new WorkerComponentInterestOverriderImpl
            {
                EntityObjects = LocalEntities.Instance
            };
        }

        internal static bool ConnectionWasSuccesful { get; set; }

        /// <summary>
        ///     A callback that will be invoked when the deployment list has been retrieved from the Locator.
        ///     It is passed the list of deployments, and a callback it should call to connect to the chosen deployment.
        ///     The default callback chooses the deployment specified by the command line flag or the Unity UI; if not specified,
        ///     it chooses the first deployment in the list, and logs an error if the list contains more than one.
        /// </summary>
        public static Action<DeploymentList, Action<Deployment>> OnDeploymentListReceived;

        /// <summary>
        ///     A callback that will be invoked during the queuing process. To abort an ongoing connection attempt, it should
        ///     call SpatialOS.Disconnect().
        ///     The default callback only aborts if the platform returns an error.
        /// </summary>
        public static Action<QueueStatus> OnQueueStatus;

        /// <summary>
        ///     Gets or sets the template provider.
        ///     <see cref="IEntityTemplateProvider" /> for more details about why you may want to set a custom provider.
        /// </summary>
        /// <remarks>
        ///     It is invalid to change this after Connect is called.
        /// </remarks>
        public static IEntityTemplateProvider TemplateProvider
        {
            get { return templateProvider; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("TemplateProvider was set after Connect was called.");
                }

                templateProvider = value;
            }
        }

        /// <summary>
        ///     Gets or sets a list of prefabs to pool, and how many instances to pre-allocate.
        /// </summary>
        /// <remarks>
        ///     It is invalid to change this after Connect is called.
        /// </remarks>
        public static IEnumerable<KeyValuePair<string, int>> AssetsToPrePool
        {
            get { return assetsToPrePool; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("AssetsToPrePool was set after Connect was called.");
                }

                assetsToPrePool = value;
            }
        }

        /// <summary>
        ///     A list of assets that should be downloaded before connection proceeds.
        /// </summary>
        public static IList<string> AssetsToPrecache
        {
            get { return assetsToPrecache; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("AssetsToPrecache was set after Connect was called.");
                }

                assetsToPrecache = value;
            }
        }

        /// <summary>
        ///     The maximum number of concurrent asset downloads.
        /// </summary>
        public static int MaxConcurrentPrecacheConnections
        {
            get { return maxConcurrentPrecacheConnections; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("MaxConcurrentPrecacheConnections was set after Connect was called.");
                }

                maxConcurrentPrecacheConnections = value;
            }
        }

        /// <summary>
        ///     A callback that will be invoked when all assets have been downloaded.
        /// </summary>
        public static Action OnPrecachingCompleted
        {
            get { return onPrecachingCompleted; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("OnPrecachingCompleted was set after Connect was called.");
                }

                onPrecachingCompleted = value;
            }
        }

        /// <summary>
        ///     A callback that is invoked after each asset starts to download.
        /// </summary>
        public static Action<int> OnPrecacheProgress
        {
            get { return onPrecacheProgress; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("OnPrecacheProgress was set after Connect was called.");
                }

                onPrecacheProgress = value;
            }
        }

        /// <summary>
        ///     Provides a view of all entities that exist on this worker.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        [Obsolete("Obsolete in 10.5.0. Migrate to use LocalEntities.Instance.")]
        public static IUniverse Universe
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("Universe was accessed before Connect called.");
                }

                return connectionLifecycle.Universe;
            }
        }

        internal static DispatchEventHandler DispatchEventHandler
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("DispatchEventHandler was accessed before Connect called.");
                }

                return connectionLifecycle.DispatchEventHandler;
            }
        }

        /// <summary>
        ///     Provides access to the worker id.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        public static string WorkerId
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("WorkerId was accessed before Connect called.");
                }

                return connectionLifecycle.Connection.GetWorkerId();
            }
        }

        /// <summary>
        ///     Provides access to the worker attributes.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        public static List<string> WorkerAttributes
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("WorkerAttributes was accessed before Connect called.");
                }

                return connectionLifecycle.Connection.GetWorkerAttributes();
            }
        }

        /// <summary>
        ///     Provides access to the configured worker.
        /// </summary>
        /// <remarks>
        ///     ApplyConfiguration must be called before access to this property is valid.
        /// </remarks>
        public static WorkerConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    throw new InvalidOperationException("Configuration was accessed before ApplyConfiguration called.");
                }

                return configuration;
            }
        }

        /// <summary>
        ///     The currently-selected deployment, if any.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        public static Deployment? Deployment
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("Deployment was accessed before Connect was called.");
                }

                return connectionLifecycle.Deployment;
            }
        }

        /// <summary>
        ///     Provides callbacks for events related to the connection and updates.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        public static View Dispatcher
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("Dispatcher used while not connected");
                }

                return connectionLifecycle.View;
            }
        }

        /// <summary>
        ///     The current metrics.
        /// </summary>
        public static WorkerMetrics Metrics
        {
            get { return metrics ?? (metrics = new WorkerMetrics()); }
            set { metrics = value; }
        }

        /// <summary>
        ///     Provides the ability to modify the component interest that is automatically calculated for this worker.
        /// </summary>
        public static IWorkerComponentInterestOverrider WorkerComponentInterestModifier
        {
            get { return WorkerComponentInterestImpl; }
        }

        /// <summary>
        ///     Call this to apply any customisations to the Unity Worker.
        /// </summary>
        /// <remarks>
        ///     It is invalid to call this after this.Connect is called.
        /// </remarks>
        public static void ApplyConfiguration(WorkerConfigurationData configData, IList<string> commandLineArguments = null)
        {
            if (connectionLifecycle != null)
            {
                throw new InvalidOperationException("ApplyConfiguration was called after Connect was called.");
            }

            configuration = new WorkerConfiguration(configData, commandLineArguments);
        }

        /// <summary>
        ///     Start the bootstrap process which results in connecting to SpatialOS.
        /// </summary>
        /// <param name="gameObject">
        ///     The game object that acts as the point of initialisation for the Improbable Fabric connection
        ///     within the scene.
        /// </param>
        /// <remarks>
        ///     It is invalid to call this method before ApplyConfiguration has been called.
        ///     It is invalid to call this multiple times between matching calls to Disconnect.
        /// </remarks>
        public static void Connect(GameObject gameObject)
        {
            if (connectionLifecycle != null)
            {
                throw new InvalidOperationException("Connect called while already connected.");
            }

            if (TemplateProvider == null)
            {
                TemplateProvider = gameObject.GetComponent<IEntityTemplateProvider>() ?? gameObject.AddComponent<DefaultTemplateProvider>();
            }

            connectionLifecycle = gameObject.AddComponent<ConnectionLifecycle>();
        }

        /// <summary>
        ///     The connection to SpatialOS.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        public static Connection Connection
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("Connection used while not connected");
                }

                return connectionLifecycle.Connection;
            }
        }

        /// <summary>
        ///     Used to send worker commands. Only use if you cannot send a command from
        ///     a component, as you might send the same command from multiple workers.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        public static IComponentCommander SendWorkerCommand
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("SendWorkerCommand used while not connected");
                }

                return connectionLifecycle.ComponentCommander;
            }
        }

        /// <summary>
        ///     Send commands and entity queries from writers.
        /// </summary>
        public static ICommandSender Commands
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("Commands used while not connected");
                }

                return connectionLifecycle.Commander;
            }
        }

        /// <summary>
        ///     Send commands and entity queries when a component writer is not available.
        /// </summary>
        public static IWorkerCommandSender WorkerCommands
        {
            get
            {
                if (connectionLifecycle == null)
                {
                    throw new InvalidOperationException("WorkerCommands used while not connected");
                }

                return connectionLifecycle.Commander;
            }
        }

        /// <summary>
        ///     True if a valid connection exists to SpatialOS.
        /// </summary>
        public static bool IsConnected
        {
            get { return connectionLifecycle != null && connectionLifecycle.Connection != null && connectionLifecycle.Connection.IsConnected; }
        }

        /// <summary>
        ///     Call this to cleanly disconnect from SpatialOS. All entities that exist will be despawned.
        /// </summary>
        /// <remarks>
        ///     It is invalid to call this method if Connect has not been called.
        /// </remarks>
        public static void Disconnect()
        {
            if (connectionLifecycle == null)
            {
                throw new InvalidOperationException("Disconnect called while not connected.");
            }

            receivedDisconnectOp = new DisconnectOp { Reason = "Disconnect was called by the user." };
            Disconnecting = true;
        }

        /// <summary>
        ///     Processes messages from the SpatialOS connection and triggers callbacks.
        ///     This should be called periodically, typically every frame.
        /// </summary>
        /// <remarks>
        ///     If a connection attempt has failed, call this method to receive log messages and disconnection events.
        /// </remarks>
        public static void ProcessEvents()
        {
            if (connectionLifecycle != null)
            {
                connectionLifecycle.ProcessEvents();
            }
        }

        /// <summary>
        ///     This event is triggered when a connection to SpatialOS has been successfully made.
        /// </summary>
        public static event Action OnConnected
        {
            add { onConnected.Add(value); }
            remove { onConnected.Remove(value); }
        }

        /// <summary>
        ///     This event is triggered when an attempted connection to SpatialOS has been unsuccessful.
        /// </summary>
        [Obsolete("This signature for the OnConnectionFailed is deprecated. It will be replaced by OnConnectionFailed(string) where the string gives the reason for the connection failure.")]
        public static event Action OnConnectionFailed
        {
            add { onConnectionFailed.Add(value); }
            remove { onConnectionFailed.Remove(value); }
        }

        /// <summary>
        ///     This event is triggered when an attempted connection to SpatialOS has been unsuccessful.
        /// </summary>
        public static event Action<string> OnConnectionFailedWithReason
        {
            add { onConnectionFailedWithReason.Add(value); }
            remove { onConnectionFailedWithReason.Remove(value); }
        }

        /// <summary>
        ///     This event is triggered when the connection to SpatialOS is terminated for any reason.
        /// </summary>
        /// <remarks>
        ///     Once OnDisconnected is invoked, the SpatialOS object will not be usable until SpatialOS.Connect() is called again.
        /// </remarks>
        public static event Action<DisconnectOp> OnDisconnected
        {
            add { onDisconnected.Add(value); }
            remove { onDisconnected.Remove(value); }
        }

        /// <summary>
        ///     Returns the raw entity, if it is available on this worker, or null otherwise.
        /// </summary>
        /// <remarks>
        ///     It is invalid to call this if Connect has not been called first.
        /// </remarks>
        public static Worker.Entity GetLocalEntity(EntityId entityId)
        {
            if (connectionLifecycle == null)
            {
                throw new InvalidOperationException("GetLocalEntity called while not connected");
            }

            Worker.Entity entity;
            if (Dispatcher.Entities.TryGetValue(entityId, out entity))
            {
                return entity;
            }

            return null;
        }

        /// <summary>
        ///     Returns a component of the specified type, or null if either the entity or the component is not available on this
        ///     worker.
        /// </summary>
        /// <remarks>
        ///     It is invalid to call this if Connect has not been called first.
        /// </remarks>
        public static IComponentData<T> GetLocalEntityComponent<T>(EntityId entityId) where T : IComponentMetaclass
        {
            var entity = GetLocalEntity(entityId);
            if (entity == null)
            {
                return null;
            }

            var option = entity.Get<T>();
            if (option.HasValue)
            {
                return option.Value;
            }

            return null;
        }

        /// <summary>
        ///     Set this to a function that can be used to control which log messages are sent to SpatialOS.
        ///     <summary>
        ///         Set this to a class that can be used to control if an entity should be spawned or not.
        ///     </summary>
        ///     <remarks>
        ///         It is invalid to call this after this.Connect is called.
        ///         Use this to avoid spawning too many entities in a single frame.
        ///         (Backwards compatibility) By default, entities will be limited to a maximum number per frame, set by
        ///         <see cref="Unity.EntityCreationLimitPerFrame" />.
        ///         To disable rate limiting, use <see cref="GreedySpawnLimiter" />.
        ///         To limit based on maximum frame time, use <see cref="TimeBasedSpawnLimiter" />.
        ///     </remarks>
        public static ILogFilterReceiver LogFilter
        {
            get { return logFilter; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("LogFilter was set after Connect was called.");
                }

                logFilter = value;
            }
        }

        /// <summary>
        ///     Set this to a class that can be used to control if an entity should be spawned or not.
        /// </summary>
        /// <remarks>
        ///     It is invalid to call this after this.Connect is called.
        ///     Use this to avoid spawning too many entities in a single frame.
        ///     (Backwards compatibility) By default, entities will be limited to a maximum number per frame, set by
        ///     <see cref="Unity.EntityCreationLimitPerFrame" />.
        ///     To disable rate limiting, use <see cref="GreedySpawnLimiter" />.
        ///     To limit based on maximum frame time, use <see cref="TimeBasedSpawnLimiter" />.
        ///     You can also implement the <see cref="IEntitySpawnLimiter" /> interface to provide your own strategy.
        /// </remarks>
        public static IEntitySpawnLimiter EntitySpawnLimiter
        {
            get { return entitySpawnLimiter; }
            set
            {
                if (connectionLifecycle != null)
                {
                    throw new InvalidOperationException("EntitySpawnLimiter was set after Connect was called.");
                }

                entitySpawnLimiter = value;
            }
        }

        /// <summary>
        ///     Indicates that disconnection has been requested. Used internally to defer destruction of resources.
        /// </summary>
        internal static bool Disconnecting { get; private set; }

        internal static void OnConnectedInternal()
        {
            // Now that we're connected, wire up the invalidation handler so that changes to global interest overrides can be broadcast to entities.
            WorkerComponentInterestImpl.InterestInvalidationHandler = connectionLifecycle.DispatchEventHandler.EntityInterestedComponentsUpdater.InvalidateEntity;

            var callbacks = onConnected.ToArray();
            foreach (var callback in callbacks)
            {
                try
                {
                    callback();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal static void OnConnectionFailedInternal()
        {
            var callbacks = onConnectionFailed.ToArray();
            foreach (var callback in callbacks)
            {
                try
                {
                    callback();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal static void OnConnectionFailedWithReasonInternal()
        {
            var callbacks = onConnectionFailedWithReason.ToArray();
            foreach (var callback in callbacks)
            {
                try
                {
                    callback(receivedDisconnectOp.Reason);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal static void OnDisconnectedInternal()
        {
            // Protect against callbacks modifying the list.
            var callbacks = onDisconnected.ToArray();
            for (var i = 0; i < callbacks.Length; i++)
            {
                try
                {
                    callbacks[i](receivedDisconnectOp);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal static void Disconnected(DisconnectOp disconnectOp)
        {
            receivedDisconnectOp = disconnectOp;
            Disconnecting = true;
        }

        internal static void SignalDisconnection()
        {
            if (!ConnectionWasSuccesful)
            {
                OnConnectionFailedInternal();
                OnConnectionFailedWithReasonInternal();
            }
            else
            {
                OnDisconnectedInternal();
            }
        }

        internal static void Dispose()
        {
            if (connectionLifecycle == null)
            {
                throw new InvalidOperationException("Dispose called while not connected.");
            }

            connectionLifecycle = null;
            SignalDisconnection();

            templateProvider = null;
            Disconnecting = false;
        }
    }
}
