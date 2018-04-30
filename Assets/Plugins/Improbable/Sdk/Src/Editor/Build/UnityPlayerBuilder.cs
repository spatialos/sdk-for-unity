// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Unity.Assets;
using Improbable.Unity.EditorTools.Util;
using Improbable.Unity.Util;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Build
{
    public class UnityPlayerBuilder
    {
        /// <remarks>This is obsolete, please see <see cref="IPlayerBuildEvents" /> to customize player packaging.</remarks>
        [Obsolete("Obsolete in 10.3.0. Please see IPlayerBuildEvents for information about customizing player packaging.")]
#pragma warning disable 0618
        public static Func<WorkerPlatform, BuildTarget, Config, IPackager> GetPackager = GetDefaultPackager;
#pragma warning restore 0618

        /// <remarks>This is obsolete, please see <see cref="IPlayerBuildEvents" /> to customize scene processing.</remarks>
        [Obsolete("Obsolete in 10.3.0. Please see IPlayerBuildEvents for information about customizing scene loading.")]
        public static Action<string> ProcessScene = sceneName => { };

        public readonly BuildTarget BuildTarget;
        private readonly Config config;
        public readonly WorkerPlatform WorkerType;
        private readonly BuildOptions options;
        private readonly PlatformData platformData;

        internal IPlayerBuildEvents PlayerBuildEvents { get; set; }

        public UnityPlayerBuilder(WorkerPlatform workerType, string targetString, Config config)
        {
            WorkerType = workerType;
            BuildTarget = ToRuntimePlatform(targetString);
            this.config = config;
            options = GenerateFlag(config.FlagsForPlatform(targetString));
            platformData = CreatePlatformData(BuildTarget);
        }

        private static BuildTarget ToRuntimePlatform(string platform)
        {
            if (platform.Contains("?"))
            {
                platform = platform.Substring(0, platform.IndexOf("?", StringComparison.Ordinal));
            }

            if (platform.ToLower() != "current")
            {
                var value = (BuildTarget) Enum.Parse(typeof(BuildTarget), platform);
#if UNITY_2017_3_OR_NEWER
#pragma warning disable 618
                if (value == BuildTarget.StandaloneOSXIntel64)
                {
                    Debug.LogWarningFormat("{0} is deprecated and will be removed. Please update {1} to use {2} instead.", BuildTarget.StandaloneOSXIntel64, UnityPlayerBuilders.PlayerConfigurationFilePath, BuildTarget.StandaloneOSX);
                    value = BuildTarget.StandaloneOSX;
                }
#pragma warning restore 618
#endif
                return value;
            }

            return CurrentPlatform();
        }

        internal static BuildTarget CurrentPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return BuildTarget.StandaloneWindows;
                case RuntimePlatform.OSXEditor:
#if UNITY_2017_3_OR_NEWER
                    return BuildTarget.StandaloneOSX;
#else
                    return BuildTarget.StandaloneOSXIntel64;
#endif
                case RuntimePlatform.LinuxEditor:
                    return BuildTarget.StandaloneLinux64;
                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException(string.Format("Unsupported runtime platform {0}", Application.platform));
            }
        }

        public static string PlayerBuildScratchDirectory
        {
            get { return Path.GetFullPath(Path.Combine("build", "worker")); }
        }

        public static string PlayerBuildDirectory
        {
            get { return PathUtil.Combine(Directory.GetCurrentDirectory(), EditorPaths.AssetDatabaseDirectory, "worker"); }
        }

        private string ExecutableName
        {
            get
            {
                return string.Format("{0}@{1}{2}", WorkerType, platformData.BuildContext,
                                     platformData.ExecutableExtension);
            }
        }

        private string PackageName
        {
            get { return string.Format("{0}@{1}", WorkerType, platformData.BuildContext); }
        }

        private string PackagePath
        {
            get { return Path.Combine(PlayerBuildScratchDirectory, PackageName); }
        }

        internal string AssemblyDirectory
        {
            get
            {
                switch (BuildTarget)
                {
#if UNITY_2017_3_OR_NEWER
                    case BuildTarget.StandaloneOSX:
#else
                    case BuildTarget.StandaloneOSXIntel64:
#endif
                    {
                        return PathUtil.Combine(PackagePath, string.Format("{0}.app", PackageName), "Contents", "Resources", "Data", "Managed");
                    }
                    default:
                    {
                        return PathUtil.Combine(PackagePath, string.Format("{0}_Data", PackageName), "Managed");
                    }
                }
            }
        }

        private string ZipPath
        {
            get { return Path.Combine(PlayerBuildDirectory, PackageName); }
        }

        private string BuildConfigComment
        {
            get
            {
                return string.Format("WorkerType={0};BuildTarget={1};EmbedAssets={2};BuildOptions={3}", WorkerType,
                                     BuildTarget, config.Assets == AssetDatabaseStrategy.Local.ToString(), options);
            }
        }

        public bool AssemblyDirectoryEmpty()
        {
            return !Directory.Exists(AssemblyDirectory) || !Directory.GetFileSystemEntries(AssemblyDirectory).Any();
        }

        public void PatchAssembly(string sourcePath)
        {
            if (!Directory.Exists(AssemblyDirectory))
            {
                return;
            }

            var fileName = Path.GetFileName(sourcePath);
            var targetPath = Path.Combine(AssemblyDirectory, fileName);

            File.Copy(sourcePath, targetPath, overwrite: true);
        }

#pragma warning disable 0618
        [Obsolete("Obsolete in 10.3.0. Please see IPlayerBuildEvents for information about customizing player packaging.")]
        public static IPackager GetDefaultPackager(WorkerPlatform workerType, BuildTarget buildTarget, Config config)
#pragma warning restore 0618
        {
            return new SimplePackager();
        }

        public void Clean()
        {
            UnityPathUtil.EnsureDirectoryRemoved(PackagePath);
            UnityPathUtil.EnsureFileRemoved(ZipPath + ".zip");
        }

        [Obsolete("Obsolete in 10.3.0. This will be removed in a future version.")]
        public void PackagePlayer(string spatialPath, PlayerCompression useCompression)
        {
            PackagePlayer(PlayerBuildEvents, spatialPath, useCompression);
        }

        internal void PackagePlayer(IPlayerBuildEvents events, string spatialPath, PlayerCompression useCompression)
        {
            events.BeginPackage(WorkerType, BuildTarget, config, PackagePath);
            SpatialZip.Zip(spatialPath, ZipPath, PackagePath, ".", "**", useCompression);
        }

        private static PlatformData CreatePlatformData(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                    return new PlatformData("Managed", "Windows", "_Data", ".exe");
                case BuildTarget.StandaloneWindows64:
                    return new PlatformData("Managed", "Windows", "_Data", ".exe");
#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:
#else
                    case BuildTarget.StandaloneOSXIntel64:
#endif
                    return new PlatformData("Contents/Data/Managed", "Mac", ".app", "");
                case BuildTarget.StandaloneLinux64:
                    return new PlatformData("Managed", "Linux", "_Data", "");
                case BuildTarget.iOS:
                    return new PlatformData("Data/Managed", "iOS", "", "");
            }

            throw new ArgumentException("Unsupported platform " + buildTarget);
        }

        internal void BuildPlayer()
        {
            PathUtil.EnsureDirectoryExists(PlayerBuildDirectory);
            PathUtil.EnsureDirectoryExists(PlayerBuildScratchDirectory);

            var scenes = PlayerBuildEvents.GetScenes(WorkerType);

            var tempExecutablePath = Path.Combine(PackagePath, ExecutableName);

            var playerOptions = new BuildPlayerOptions { target = BuildTarget, locationPathName = tempExecutablePath, options = options, scenes = scenes };
            var buildErrorMessage = BuildPipeline.BuildPlayer(playerOptions);
            if (!string.IsNullOrEmpty(buildErrorMessage))
            {
                throw new ApplicationException(string.Format("Failed to build player {0} due to {1}", BuildConfigComment,
                                                             buildErrorMessage));
            }

            Debug.LogFormat("Built player {0} into {1}", BuildConfigComment, PackagePath);
        }

        private BuildOptions GenerateFlag(IEnumerable<BuildOptions> flagList)
        {
            return flagList.Aggregate((a, b) => a | b);
        }
    }

    internal class PlatformData
    {
        public readonly string AssemblyPathWithinPackage;
        public readonly string BuildContext;
        public readonly string DataFolderExtension;
        public readonly string ExecutableExtension;

        public PlatformData(string assemblyPathWithinPackage, string buildContext, string dataFolderExtension,
                            string executableExtension)
        {
            AssemblyPathWithinPackage = assemblyPathWithinPackage;
            BuildContext = buildContext;
            DataFolderExtension = dataFolderExtension;
            ExecutableExtension = executableExtension;
        }
    }
}
