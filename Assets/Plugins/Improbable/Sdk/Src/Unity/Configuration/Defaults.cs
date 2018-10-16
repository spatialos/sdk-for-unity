// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Worker;

namespace Improbable.Unity.Configuration
{
    public static class Defaults
    {
        public const string DeploymentTag = "prod";

        [Obsolete("Please use Improbable.Unity.Core.CountBasedSpawnLimiter and SpatialOS.EntitySpawnLimiter.")]
        public const int EntityCreationLimitPerFrame = 100;

        public const NetworkConnectionType LinkProtocol = NetworkConnectionType.RakNet;
        public const bool LogDebugToSpatialOs = false;
        public const bool LogAssertToSpatialOs = false;
        public const bool LogWarningToSpatialOs = true;
        public const bool LogErrorToSpatialOs = true;
        public const bool LogExceptionToSpatialOs = true;

        public const string InfraServiceUrl = "https://api.spatial.improbable.io";
        public const string LocatorHost = "locator.improbable.io";
        public const string ProtocolLogPrefix = "protocol-";
        public const uint ProtocolLogMaxFileBytes = 100U * 1024U * 1024U;
        public const bool ProtocolLoggingOnStartup = false;
        public const uint RaknetHeartbeatTimeoutMillis = Worker.Defaults.RakNetHeartbeatTimeoutMillis;
        public const uint ReceiveQueueCapacity = 32768U;
        public const string ReceptionistHost = "127.0.0.1";
        public const ushort ReceptionistPort = 7777;
        public const uint SendQueueCapacity = 16384U;

        [Obsolete("As of 12.1, this no longer does anything, and will be removed in a future release.")]
        public const bool ShowDebugTraces = false;

        public const byte TcpMultiplexLevel = Worker.Defaults.TcpMultiplexLevel;
        public const bool UseInstrumentation = true;

        [Obsolete("As of 12.1, this no longer does anything, and will be removed in a future release.")]
        public const bool UsePerInstanceAssetCache = true;
    }
}
