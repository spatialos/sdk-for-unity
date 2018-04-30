// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.IO;
using Improbable.Unity.Util;

namespace Improbable.Unity.MinimalBuildSystem.Configuration
{
    public static class BuildPaths
    {
        public static readonly string PrefabResourcesDirectory =
            PathUtil.Combine("Assets", "Improbable", "Generated", "Resources", "EntityPrefabs").ToUnityPath();

        public static readonly string PrefabSourceDirectory = PathUtil.Combine("Assets", "EntityPrefabs").ToUnityPath();

        public static string BuildScratchDirectory
        {
            get { return Path.GetFullPath(Path.Combine("build", "worker")); }
        }
    }
}
