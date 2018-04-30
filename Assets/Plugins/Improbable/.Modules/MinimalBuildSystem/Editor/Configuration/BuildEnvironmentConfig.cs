// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using UnityEditor;

namespace Improbable.Unity.MinimalBuildSystem.Configuration
{
    [Serializable]
    public class BuildEnvironmentConfig
    {
        public SpatialBuildPlatforms BuildPlatforms = SpatialBuildPlatforms.Current;
        public BuildOptions BuildOptions = 0;

        [NonSerialized] public bool ShowBuildOptions = true;
        [NonSerialized] public bool ShowBuildPlatforms = true;
    }
}
