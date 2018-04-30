// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Assets
{
    /// <summary>
    ///     Platform provides methods to get prefab asset bundle suffixes for different platforms and build targets.
    /// </summary>
    public static class Platform
    {
        private static readonly string windowsAssetBundleSuffix = "windows";
        private static readonly string osxAssetBundleSuffix = "osx";
        private static readonly string iosAssetBundleSuffix = "ios";
        private static readonly string linuxAssetBundleSuffix = "linux";
        private static readonly string androidAssetBundleSuffix = "android";

        private static readonly Dictionary<RuntimePlatform, string> runtimePlatformToAssetBundleSuffix = new Dictionary<RuntimePlatform, string>
        {
            { RuntimePlatform.WindowsEditor, windowsAssetBundleSuffix },
            { RuntimePlatform.WindowsPlayer, windowsAssetBundleSuffix },

            { RuntimePlatform.OSXEditor, osxAssetBundleSuffix },
            { RuntimePlatform.OSXPlayer, osxAssetBundleSuffix },

            { RuntimePlatform.LinuxPlayer, linuxAssetBundleSuffix },
            { RuntimePlatform.LinuxEditor, linuxAssetBundleSuffix },

            { RuntimePlatform.IPhonePlayer, iosAssetBundleSuffix },

            { RuntimePlatform.Android, androidAssetBundleSuffix }
        };

        /// <summary>
        ///     BuildPlatform is used instead of BuildTarget (which is defined only in UnityEditor).
        /// </summary>
        public enum BuildPlatform
        {
            Windows,
            OSX,
            Linux,
            iOS,
            Android
        }

        private static readonly Dictionary<BuildPlatform, string> buildPlatformToAssetBundleSuffix = new Dictionary
            <BuildPlatform, string>
            {
                { BuildPlatform.Windows, windowsAssetBundleSuffix },
                { BuildPlatform.OSX, osxAssetBundleSuffix },
                { BuildPlatform.Linux, linuxAssetBundleSuffix },
                { BuildPlatform.iOS, iosAssetBundleSuffix },
                { BuildPlatform.Android, androidAssetBundleSuffix },
            };

        /// <summary>
        ///     Returns the asset bundle suffix for the given platform.
        /// </summary>
        public static string RuntimePlatformToAssetBundleSuffix(RuntimePlatform platform)
        {
            string suffix;
            if (!runtimePlatformToAssetBundleSuffix.TryGetValue(platform, out suffix))
            {
                throw new ArgumentException("No asset bundle name suffix exists for that platform.");
            }

            return suffix;
        }

        /// <summary>
        ///     Returns the asset bundle suffix for the given build target.
        /// </summary>
        public static string BuildPlatformToAssetBundleSuffix(BuildPlatform buildPlatform)
        {
            string suffix;
            if (!buildPlatformToAssetBundleSuffix.TryGetValue(buildPlatform, out suffix))
            {
                throw new ArgumentException("No asset bundle name suffix exists for that build platform.");
            }

            return suffix;
        }

        /// <summary>
        ///     Gets the asset bundle name for the given prefab name, by appending the correct suffix for the platform.
        /// </summary>
        public static string PrefabNameToAssetBundleName(string prefabName)
        {
            return String.Format("{0}@{1}", prefabName, RuntimePlatformToAssetBundleSuffix(Application.platform));
        }
    }
}
