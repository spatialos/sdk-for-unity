// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using System.IO;
using Improbable.Unity.Assets;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Build
{
    static class DefaultPlayerBuildConfiguration
    {
        private static readonly List<string> CurrentPlatformTargetList = new List<string> { "Current" };

        internal static PlayerBuildConfiguation Generate()
        {
            var config = new PlayerBuildConfiguation
            {
                Deploy = new Enviroment
                {
                    UnityWorker = new Config
                    {
                        Assets = AssetDatabaseStrategy.Streaming.ToString(),
                        Targets = new List<string>
                        {
                            BuildTarget.StandaloneLinux64 + "?" + BuildOptions.EnableHeadlessMode
                        }
                    },
                    UnityClient = new Config
                    {
                        Assets = AssetDatabaseStrategy.Streaming.ToString(),
                        Targets = new List<string>
                        {
                            BuildTarget.StandaloneWindows.ToString(),
                            BuildTarget.StandaloneOSX.ToString()
                        }
                    }
                },
                Develop = new Enviroment
                {
                    UnityWorker = new Config
                    {
                        Assets = AssetDatabaseStrategy.Streaming.ToString(),
                        Targets = CurrentPlatformTargetList
                    },
                    UnityClient = new Config
                    {
                        Assets = AssetDatabaseStrategy.Streaming.ToString(),
                        Targets = CurrentPlatformTargetList
                    },
                }
            };
            var json = JsonUtility.ToJson(config, prettyPrint: true);
            File.WriteAllText(UnityPlayerBuilders.PlayerConfigurationFilePath, json);
            return config;
        }
    }
}
