// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Util;
using UnityEditor;

namespace Improbable.Unity.MinimalBuildSystem.Configuration
{
    public class WorkerBuildData
    {
        internal const BuildTarget OSXBuildTarget =
#if UNITY_2017_3_OR_NEWER
            BuildTarget.StandaloneOSX;
#else
            BuildTarget.StandaloneOSXIntel64;
#endif

        private readonly WorkerPlatform workerPlatform;
        private readonly BuildTarget buildTarget;

        private static readonly Dictionary<BuildTarget, string> BuildTargetNames =
            new Dictionary<BuildTarget, string>
            {
                { BuildTarget.StandaloneWindows, "Windows" },
                { BuildTarget.StandaloneWindows64, "Windows" },
                { BuildTarget.StandaloneLinux64, "Linux" },
                { OSXBuildTarget, "Mac" }
            };

        private static readonly Dictionary<BuildTarget, string> BuildPlatformExtensions =
            new Dictionary<BuildTarget, string>
            {
                { BuildTarget.StandaloneWindows, ".exe" },
                { BuildTarget.StandaloneWindows64, ".exe" },
                { BuildTarget.StandaloneLinux64, "" },
                { OSXBuildTarget, "" }
            };

        public WorkerBuildData(WorkerPlatform workerPlatform, BuildTarget buildTarget)
        {
            switch (workerPlatform)
            {
                case WorkerPlatform.UnityWorker:
                case WorkerPlatform.UnityClient:
                    break;
                default:
                    throw new ArgumentException("Unsupported WorkerPlatform " + workerPlatform);
            }

            if (!BuildTargetNames.ContainsKey(buildTarget))
            {
                throw new ArgumentException("Unsupported BuildPlatform " + workerPlatform);
            }

            this.workerPlatform = workerPlatform;
            this.buildTarget = buildTarget;
        }

        private string BuildTargetName
        {
            get { return BuildTargetNames[buildTarget]; }
        }

        public string BuildScratchDirectory
        {
            get { return PathUtil.Combine(BuildPaths.BuildScratchDirectory, PackageName, ExecutableName).ToUnityPath(); }
        }

        public string WorkerPlatformName
        {
            get { return workerPlatform.ToString(); }
        }

        private string ExecutableName
        {
            get { return PackageName + BuildPlatformExtensions[buildTarget]; }
        }

        public string PackageName
        {
            get { return string.Format("{0}@{1}", workerPlatform, BuildTargetName); }
        }
    }
}
