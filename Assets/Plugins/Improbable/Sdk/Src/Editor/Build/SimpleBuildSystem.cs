// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Editor.Configuration;
using Improbable.Unity.EditorTools.PrefabExport;
using Improbable.Unity.EditorTools.Util;
using Improbable.Unity.Util;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Build
{
    /// <summary>
    ///     This is a simple default build system that will compile assets and build workers.
    /// </summary>
    public static class SimpleBuildSystem
    {
        private static string target;

        /// <summary>
        ///     A list of all default types of players to build.
        /// </summary>
        public static readonly List<string> AllWorkerTypes = new Collections.List<string>
        {
            WorkerTypeUtils.UnityClientType,
            WorkerTypeUtils.UnityWorkerType
        };

        /// <summary>
        ///     Override this to customize the actions that occur during "spatial worker build".
        /// </summary>
        public static Action BuildAction = DefaultBuild;

        /// <summary>
        ///     Override this to customize the actions that occur during "spatial worker clean".
        /// </summary>
        public static Action CleanAction = DefaultClean;

        /// <summary>
        ///     Override this to customize the actions that occur during "spatial worker codegen".
        /// </summary>
        public static Action CodegenAction = DefaultCodegen;

        /// <summary>
        ///     Override this to provide custom build events when building players.
        ///     This will be called once each time <see cref="UnityPlayerBuilders.BuildDeploymentPlayers" /> and
        ///     <see cref="UnityPlayerBuilders.BuildDevelopmentPlayers" /> is called.
        /// </summary>
        public static Func<IPlayerBuildEvents> CreatePlayerBuildEventsAction = DefaultCreatePlayerBuildEvents;

        /// <summary>
        ///     The name of the build target to run. If empty (the default) all targets will run.
        ///     Can be overridden with the IMPROBABLE_BUILD_TARGET environment variable.
        /// </summary>
        public static string Target
        {
            get
            {
                var envVar = Environment.GetEnvironmentVariable("IMPROBABLE_BUILD_TARGET");
                return string.IsNullOrEmpty(envVar) ? target : envVar;
            }
            set { target = value; }
        }

        /// <summary>
        ///     A list of Worker names to build. If it is null or empty, then all available workers will be used.
        /// </summary>
        public static IList<string> WorkersToBuild { get; set; }

        /// <summary>
        ///     The name(s) of the workers to build. Specify multiple targets by separating them with a comma.
        ///     For example: "UnityClient,UnityWorker".
        /// </summary>
        /// <remarks>
        ///     Currently, the only possible values are "UnityWorker" and "UnityClient".
        ///     Defaults to AllWorkerTypes if the flag is not specified.
        ///     If commandLine is null, defaults to using <code>Environment.GetCommandLineArgs();</code>.
        /// </remarks>
        public static IList<string> GetWorkerTypesToBuild(string[] commandLine = null)
        {
            if (WorkersToBuild != null)
            {
                if (!WorkersToBuild.Any())
                {
                    return AllWorkerTypes;
                }

                return WorkersToBuild;
            }


            if (commandLine == null)
            {
                commandLine = Environment.GetCommandLineArgs();
            }

            var commandLineValue = CommandLineUtil.GetCommandLineValue(commandLine, ConfigNames.BuildWorkerTypes, string.Empty);
            if (string.IsNullOrEmpty(commandLineValue))
            {
                return AllWorkerTypes;
            }

            return ParseWorkerTypes(commandLineValue);
        }

        /// <summary>
        ///     This is meant to be invoked by external build processes.
        /// </summary>
        public static void Build()
        {
            BuildAction();
        }

        /// <summary>
        ///     This is meant to be invoked by external build processes.
        /// </summary>
        public static void Clean()
        {
            CleanAction();
        }

        /// <summary>
        ///     Cleans player build directories and assemblies.
        /// </summary>
        public static void CleanPlayers()
        {
            Debug.LogFormat("Starting SpatialOS Unity Clean");

            if (!string.IsNullOrEmpty(Target))
            {
                Debug.LogFormat("Cleaning target '{0}', available targets are '{1}', '{2}'", Target, UnityPlayerBuilders.DeploymentTarget, UnityPlayerBuilders.DevelopmentTarget);
            }

            var workersToClean = GetWorkerTypesToBuild();

            foreach (var playerTarget in workersToClean)
            {
                Debug.LogFormat("Cleaning player {0}", playerTarget);
            }

            RunIf(UnityPlayerBuilders.DevelopmentTarget, () => CleanPlayerAssemblies(UnityPlayerBuilders.DevelopmentPlayerBuilders(workersToClean)));
            RunIf(UnityPlayerBuilders.DeploymentTarget, () => CleanPlayerAssemblies(UnityPlayerBuilders.DeploymentPlayerBuilders(workersToClean)));

            Debug.LogFormat("Finished SpatialOS Unity Clean");
        }

        /// <summary>
        ///     Cleans assemblies generated by the given player builders.
        /// </summary>
        private static void CleanPlayerAssemblies(IList<UnityPlayerBuilder> playerBuilders)
        {
            foreach (var builder in playerBuilders)
            {
                builder.Clean();
            }
        }

        /// <summary>
        ///     Performs default build steps for a Unity worker.
        /// </summary>
        private static void DefaultBuild()
        {
            Debug.LogFormat("Starting SpatialOS Build");

            if (!string.IsNullOrEmpty(Target))
            {
                Debug.LogFormat(@"Building target ""{0}"", available targets are ""{1}"", ""{2}""", Target, UnityPlayerBuilders.DeploymentTarget, UnityPlayerBuilders.DevelopmentTarget);
            }

            EntityPrefabExportMenus.ExportAllEntityPrefabs();

            var workersToBuild = GetWorkerTypesToBuild();

            foreach (var playerTarget in workersToBuild)
            {
                Debug.LogFormat(@"Building player {0}", playerTarget);
            }

            RunIf(UnityPlayerBuilders.DevelopmentTarget, () => UnityPlayerBuilders.BuildDevelopmentPlayers(workersToBuild));
            RunIf(UnityPlayerBuilders.DeploymentTarget, () => UnityPlayerBuilders.BuildDeploymentPlayers(workersToBuild));

            Debug.LogFormat("Finished SpatialOS Build");
        }

        /// <summary>
        ///     Performs default clean steps for a Unity worker.
        /// </summary>
        private static void DefaultClean()
        {
            EntityPrefabExportMenus.CleanAllEntityPrefabs();

            CleanPlayers();

            CleanDeployedFramework();
        }

        /// <summary>
        ///     Performs default codegen steps for a Unity workers.
        /// </summary>
        private static void DefaultCodegen()
        {
            Debug.Log("Default codegen");
        }

        private static IPlayerBuildEvents DefaultCreatePlayerBuildEvents()
        {
            return new SingleScenePlayerBuildEvents();
        }

        /// <summary>
        ///     Removes all the elements of the framework that were deployed via spatial build.
        /// </summary>
        private static void CleanDeployedFramework()
        {
            var pluginDirectories = Directory.GetDirectories(EditorPaths.PluginDirectory, EditorPaths.OrganizationName, SearchOption.AllDirectories);
            var directoriesToClean = new[] { EditorPaths.AssetDirectory }.Union(pluginDirectories);

            foreach (var directory in directoriesToClean)
            {
                try
                {
                    UnityPathUtil.EnsureDirectoryRemoved(directory);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        private static void RunIf(string targetName, Action action)
        {
            if (string.IsNullOrEmpty(Target) || Target.ToLowerInvariant() == targetName)
            {
                Debug.LogFormat(@"Building target ""{0}"" ", targetName);
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    throw;
                }
            }
            else
            {
                Debug.LogFormat("Skipping target '{0}', as it does not match target '{1}'", targetName, Target);
            }
        }

        private static IList<string> ParseWorkerTypes(string targets)
        {
            var playerTargets = new HashSet<string>();
            if (!string.IsNullOrEmpty(targets))
            {
                var split = targets.Trim('\"').Split(',');
                foreach (var str in split)
                {
                    var trimmed = str.Trim();
                    if (trimmed != WorkerTypeUtils.UnityClientType && trimmed != WorkerTypeUtils.UnityWorkerType)
                    {
                        throw new InvalidOperationException(string.Format("'{0}' is an unknown worker type. Accepted values are '{1}' and '{2}'", trimmed, WorkerTypeUtils.UnityClientType, WorkerTypeUtils.UnityWorkerType));
                    }

                    playerTargets.Add(trimmed);
                }
            }

            return playerTargets.ToList();
        }
    }
}
