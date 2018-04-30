// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Improbable.Unity.EditorTools.Build
{
    /// <summary>
    ///     The class that represents the structure of the whole player-build-config.json file, i.e.
    ///     the configuration for building Unity players.
    /// </summary>
    [Serializable]
    public class PlayerBuildConfiguation
    {
        // This field is always initialised with a new instance of
        // GlobalConfig. By convention, we do not serialise it to
        // the JSON output.
        [NonSerialized] [Obsolete("Field Global is deprecated and will be removed in an upcoming SpatialOS version.")]
        public GlobalConfig Global = new GlobalConfig();

        public Enviroment Deploy;
        public Enviroment Develop;
    }

    /// <summary>
    ///     The global build configuration that applies to all built players.
    /// </summary>
    [Serializable]
    [Obsolete("GlobalConfig is deprecated and will be removed in an upcoming SpatialOS version.")]
    public class GlobalConfig
    {
        public PluginConfig Plugin = new PluginConfig();
    }

    /// <summary>
    ///     The global build configuration for Unity plugins.
    /// </summary>
    [Serializable]
    [Obsolete("PluginConfig is deprecated and will be removed in an upcoming SpatialOS version.")]
    public class PluginConfig
    {
        public bool UsePlatformDirectories = true;
    }

    /// <summary>
    ///     The configuration for a particular build environment when building Unity players.
    /// </summary>
    [Serializable]
    public class Enviroment
    {
        public Config UnityWorker;
        public Config UnityClient;
    }

    /// <summary>
    ///     The build configuration for a particular Unity player.
    /// </summary>
    [Serializable]
    public class Config
    {
        private static readonly List<BuildOptions> NoBuildOptions = new List<BuildOptions> { BuildOptions.None };
        public List<string> Targets = new Collections.List<string>();

        public string Assets;

        public IEnumerable<BuildOptions> FlagsForPlatform(string buildTarget)
        {
            return default(string) == buildTarget ? NoBuildOptions : ParseFlags(buildTarget);
        }

        internal IEnumerable<BuildOptions> ParseFlags(string target)
        {
            var index = target.IndexOf('?');
            if (index == -1)
            {
                return NoBuildOptions;
            }

            var flags = target.Substring(index + 1).Split(',').Select<string, string>(s => s.Trim()).Select<string, BuildOptions>(ToBuildOptions);
            return flags;
        }

        private static BuildOptions ToBuildOptions(string value)
        {
            return (BuildOptions) Enum.Parse(typeof(BuildOptions), value);
        }
    }
}
