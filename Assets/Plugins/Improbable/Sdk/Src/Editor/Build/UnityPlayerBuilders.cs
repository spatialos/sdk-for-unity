// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Improbable.Unity.Editor.Addons;
using Improbable.Unity.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Build
{
    public static class UnityPlayerBuilders
    {
        public static readonly string PlayerConfigurationFilePath = Path.Combine(Application.dataPath, "player-build-config.json");

        internal const string DeploymentTarget = "deployment";
        internal const string DevelopmentTarget = "development";

        [Obsolete("GlobalConfig is deprecated and will be removed in an upcoming SpatialOS version.")]
        public static GlobalConfig GlobalConfig
        {
            get { return LoadConfiguation().Global; }
        }

        public static IList<UnityPlayerBuilder> DeploymentPlayerBuilders(IList<string> selectedWorkerTypes)
        {
            return ConfiguredBuilders(LoadConfiguation().Deploy, selectedWorkerTypes);
        }

        public static IList<UnityPlayerBuilder> DevelopmentPlayerBuilders(IList<string> selectedWorkerTypes)
        {
            return ConfiguredBuilders(LoadConfiguation().Develop, selectedWorkerTypes);
        }

        /// <summary> Retrieve plugin for current platform. </summary>
        /// <remarks>This is deprecated in favor of the spatialos_worker_packages.json</remarks>
        [Obsolete("RetrievePluginForCurrentPlatform is deprecated. Please use the spatialos_worker_packages.json to download the CoreSdkDll plugins.")]
        public static void RetrievePluginForCurrentPlatform() { }

        /// <summary> Retrieves plugins for deployment players. </summary>
        /// <remarks>This is deprecated in favor of the spatialos_worker_packages.json</remarks>
        [Obsolete("RetrievePlayerPlugins is deprecated. Please use the spatialos_worker_packages.json to download the CoreSdkDll plugins.")]
        public static void RetrievePlayerPlugins(IList<BuildTarget> buildTargets) { }

        public static void BuildDeploymentPlayers(IList<string> selectedWorkerTypes)
        {
            BuildPlayers(DeploymentPlayerBuilders(selectedWorkerTypes), selectedWorkerTypes, PlayerCompression.Enabled);
        }

        public static void BuildDevelopmentPlayers(IList<string> selectedWorkerTypes)
        {
            BuildPlayers(DevelopmentPlayerBuilders(selectedWorkerTypes), selectedWorkerTypes, PlayerCompression.Disabled);
        }

        private static void BuildPlayers(IList<UnityPlayerBuilder> playerBuilders, IList<string> selectedWorkerTypes, PlayerCompression compression)
        {
            PrepareWorkerAssembly(selectedWorkerTypes);

            var playerBuildEvents = SimpleBuildSystem.CreatePlayerBuildEventsAction();

            var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            try
            {
                EditorApplication.LockReloadAssemblies();
                playerBuildEvents.BeginBuild();

                var exceptions = 0;
                var threads = new List<Thread>();

                var spatialPath = SpatialCommand.SpatialPath;

                foreach (var playerBuilder in playerBuilders)
                {
                    playerBuilder.PlayerBuildEvents = playerBuildEvents;

                    playerBuilder.Clean();
                    playerBuilder.BuildPlayer();
                    var builder = playerBuilder;

                    var thread = new Thread(() =>
                    {
                        try
                        {
#pragma warning disable 0618 // Type or member is obsolete
                            builder.PackagePlayer(spatialPath, compression);
#pragma warning restore 0618 // Type or member is obsolete
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            Interlocked.Increment(ref exceptions);
                            throw;
                        }
                    });
                    thread.Start();
                    threads.Add(thread);
                }

                try
                {
                    for (var i = 0; i < threads.Count; ++i)
                    {
                        EditorUtility.DisplayProgressBar("Packaging players", "Packaging and zipping players. This may take a while.", (float) i / threads.Count);
                        threads[i].Join();
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }

                if (exceptions > 0)
                {
                    throw new Exception(string.Format("Building {0} of the players failed. Please look at logs.", exceptions));
                }

                Debug.Log("Finished building players.");
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                playerBuildEvents.EndBuild();
#pragma warning disable 618
                EditorUserBuildSettings.SwitchActiveBuildTarget(currentBuildTarget);
#pragma warning restore 618
            }
        }

        private static IList<UnityPlayerBuilder> ConfiguredBuilders(Enviroment env, IList<string> selectedWorkerTypes)
        {
            var players = new List<UnityPlayerBuilder>();

            foreach (var t in selectedWorkerTypes)
            {
                switch (t)
                {
                    case WorkerTypeUtils.UnityClientType:
                        players.AddRange(ToPlatformBuilders(WorkerPlatform.UnityClient, env.UnityClient));
                        break;
                    case WorkerTypeUtils.UnityWorkerType:
                        players.AddRange(ToPlatformBuilders(WorkerPlatform.UnityWorker, env.UnityWorker));
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unknown player type '{0}'", t));
                }
            }

            return players;
        }

        private static IList<UnityPlayerBuilder> ToPlatformBuilders(WorkerPlatform platform, Config config)
        {
            if (config == null)
            {
                return new List<UnityPlayerBuilder>();
            }

            return config.Targets.Select(configTarget => new UnityPlayerBuilder(platform, configTarget, config)).ToList();
        }

        private static PlayerBuildConfiguation LoadConfiguation()
        {
            if (File.Exists(PlayerConfigurationFilePath))
            {
                return JsonUtility.FromJson<PlayerBuildConfiguation>(File.ReadAllText(PlayerConfigurationFilePath));
            }

            return DefaultPlayerBuildConfiguration.Generate();
        }

        private static void PrepareWorkerAssembly(IList<string> selectedWorkerTypes)
        {
            var workerTypeArguments = string.Join(" ", selectedWorkerTypes.Distinct().ToArray());
            var command = SpatialCommand.SpatialPath;
            var arguments = "build build-config " + workerTypeArguments;
            var applicationRootPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
            SpatialOsEditor.RunProcess(command, arguments, applicationRootPath);
        }
    }
}
