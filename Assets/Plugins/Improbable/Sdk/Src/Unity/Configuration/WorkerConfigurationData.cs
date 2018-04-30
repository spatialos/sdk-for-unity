// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Configuration
{
    /// <summary>
    ///     Settings to customise the worker.
    /// </summary>
    [Serializable]
    public class WorkerConfigurationData
    {
        public Debugging Debugging = new Debugging();

        public Networking Networking = new Networking();

        public SpatialOsApplication SpatialOsApplication = new SpatialOsApplication();

        public Unity Unity = new Unity();
    }

    [Serializable]
    public class SpatialOsApplication
    {
        /// <summary>
        ///     The name of the assembly to use. There's no default value for this.
        ///     NOTE: This is only used for using the client against deployed games.
        /// </summary>
        public string AssemblyName;

        /// <summary>
        ///     The name of the deployment to connect to.
        ///     If empty, a local version of SpatialOS will be connected to.
        /// </summary>
        public string DeploymentId;

        /// <summary>
        ///     Used for fetching deployments with the given tag to allow the users to choose the shard to connect to.
        ///     NOTE: This is only used for running client for deployed games.
        /// </summary>
        public string DeploymentTag = "prod";

        public WorkerPlatform WorkerPlatform = WorkerPlatform.UnityClient;

        /// <summary>
        ///     Login token used to talk to the Locator. There's no default values for this.
        ///     NOTE: This is only used for using the client against deployed games.
        /// </summary>
        public string LoginToken;
    }

    [Serializable]
    public class Unity
    {
        /// <summary>
        ///     Limit the number of entities that are created in a single frame. Tune this to avoid stalls during large checkouts.
        /// </summary>
        [Range(0, 10000000)] [Obsolete("Please use Improbable.Unity.Core.CountBasedSpawnLimiter and SpatialOS.EntitySpawnLimiter.")]
        public int EntityCreationLimitPerFrame = Defaults.EntityCreationLimitPerFrame;

        /// <summary>
        ///     If true, downloaded AssetBundles will not be shared between multiple instances of Unity.
        /// </summary>
        [Obsolete("As of 12.1, this no longer does anything, and will be removed in a future release.")]
        public bool UsePerInstanceAssetCache = Defaults.UsePerInstanceAssetCache;

        /// <summary>
        ///     If enabled, GameObjects will be allocated from a pool and re-used.
        /// </summary>
        /// <remarks>
        ///     Care must be taken when writing MonoBehaviours (Visualizers) for use on pooled GameObjects.
        ///     All callbacks, local state and other data must be reset when the MonoBehaviour is disabled.
        /// </remarks>
        public bool UsePrefabPooling;
    }

    [Serializable]
    public class Networking
    {
        public NetworkConnectionType LinkProtocol = Defaults.LinkProtocol;

        public string LocatorHost = Defaults.LocatorHost;

        public string ReceptionistHost = Defaults.ReceptionistHost;

        public ushort ReceptionistPort = Defaults.ReceptionistPort;

        public uint RaknetHeartbeatTimeoutMillis = Defaults.RaknetHeartbeatTimeoutMillis;

        public uint ReceiveQueueCapacity = Defaults.ReceiveQueueCapacity;

        public uint SendQueueCapacity = Defaults.SendQueueCapacity;

        public string SteamToken = null;

        public byte TcpMultiplexLevel = Defaults.TcpMultiplexLevel;
    }

    [Serializable]
    public class Debugging
    {
        public string InfraServiceUrl = Defaults.InfraServiceUrl;

        public bool LogDebugToSpatialOs = Defaults.LogDebugToSpatialOs;

        public bool LogAssertToSpatialOs = Defaults.LogAssertToSpatialOs;

        public bool LogWarningToSpatialOs = Defaults.LogWarningToSpatialOs;

        public bool LogErrorToSpatialOs = Defaults.LogErrorToSpatialOs;

        public bool LogExceptionToSpatialOs = Defaults.LogExceptionToSpatialOs;

        public bool ProtocolLoggingOnStartup = Defaults.ProtocolLoggingOnStartup;

        public uint ProtocolLogMaxFileBytes = Defaults.ProtocolLogMaxFileBytes;

        public string ProtocolLogPrefix = Defaults.ProtocolLogPrefix;

        [Obsolete("As of 12.1, this no longer does anything, and will be removed in a future release.")]
        public bool ShowDebugTraces = Defaults.ShowDebugTraces;

        public bool UseInstrumentation = Defaults.UseInstrumentation;
    }
}
