// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Configuration
{
    /// <summary>
    ///     Configuration options that are only available via the command line.
    /// </summary>
    public static class CommandLineConfigNames
    {
        // AppName is deprecated. Not using the obsolete tag here to avoid spamming warning messages.
        public const string AppName = "appName";
        public const string ProjectName = "projectName";
        public const string AssetDatabaseStrategy = "assetDatabaseStrategy";
        public const string AssetLoadingRetryBackoffMilliseconds = "asstLoadingRetryBackoffMilliseconds";
        public const string LocalAssetDatabasePath = "localAssetDatabasePath";
        public const string LoginToken = "loginToken";
        public const string MaxAssetLoadingRetries = "maxAssetLoadingRetries";
        public const string RefreshToken = "refreshToken";
        public const string UseLocalPrefabs = "useLocalPrefabs";
        public const string UseExternalIpForBridge = "useExternalIpForBridge";
    }
}
