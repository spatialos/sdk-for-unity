// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Unity.Editor.Addons;
using Improbable.Unity.EditorTools.Util;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Build
{
    internal class ReloadAssemblies
    {
        private class CompareBuilders : IEqualityComparer<UnityPlayerBuilder>
        {
            public bool Equals(UnityPlayerBuilder x, UnityPlayerBuilder y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                return x.AssemblyDirectory == y.AssemblyDirectory;
            }

            public int GetHashCode(UnityPlayerBuilder obj)
            {
                return obj.AssemblyDirectory.GetHashCode();
            }
        }

        [DidReloadScripts]
        internal static void OnScriptsReloaded()
        {
            if (!UnityPlayerBuilderMenu.IsAutopatchEnabled())
            {
                return;
            }

            if (!Directory.Exists(EditorPaths.AssetDatabaseDirectory))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // Skip repackaging during command line builds, since users are explicitly building a set of players anyway.
            if (Environment.GetCommandLineArgs().Select(s => s.ToLowerInvariant()).Contains("-batchmode"))
            {
                Debug.Log("Skipping auto-patching in batchmode");
                return;
            }

            try
            {
                EditorApplication.LockReloadAssemblies();

                var generatedCodeSourcePaths = Directory.GetFiles(EditorPaths.AssetDirectory, "*Generated.Code.dll", SearchOption.AllDirectories);
                var scriptPaths = Directory.GetFiles(EditorPaths.ScriptAssembliesDirectory, "*Assembly-*.dll", SearchOption.AllDirectories).Where(p => !p.Contains("-Editor"));

                var allPaths = generatedCodeSourcePaths.Union(scriptPaths).Select<string, string>(Path.GetFullPath).ToList();

                var developmentPlayerBuilders =
                    UnityPlayerBuilders.DevelopmentPlayerBuilders(SimpleBuildSystem.GetWorkerTypesToBuild());

                var deploymentPlayerBuilders =
                    UnityPlayerBuilders.DeploymentPlayerBuilders(SimpleBuildSystem.GetWorkerTypesToBuild());

                var allPlayerBuilders = developmentPlayerBuilders
                                        .Union(deploymentPlayerBuilders)
                                        .Distinct(new CompareBuilders()).ToList();

                var playerBuildEvents = SimpleBuildSystem.CreatePlayerBuildEventsAction();

                foreach (var builder in allPlayerBuilders)
                {
                    // No point in patching + packaging players that have not been built yet.
                    if (builder.AssemblyDirectoryEmpty())
                    {
                        continue;
                    }

                    Debug.LogFormat("Auto-patching {0} files in {1} {2}", allPaths.Count, builder.BuildTarget, builder.WorkerType);

                    foreach (var sourcePath in allPaths)
                    {
                        builder.PatchAssembly(sourcePath);
                    }

                    builder.PackagePlayer(playerBuildEvents, SpatialCommand.SpatialPath, PlayerCompression.Disabled);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
        }
    }
}
