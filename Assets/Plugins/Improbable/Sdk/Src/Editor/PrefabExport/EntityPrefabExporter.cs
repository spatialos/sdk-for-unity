// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Assets;
using Improbable.Unity.EditorTools.Build;
using Improbable.Unity.EditorTools.Util;
using Improbable.Unity.Util;
using UnityEditor;

namespace Improbable.Unity.EditorTools.PrefabExport
{
    public class EntityPrefabExporter
    {
        private static readonly Dictionary<BuildTarget, Platform.BuildPlatform> buildTargetToBuildPlatform =
            new Dictionary<BuildTarget, Platform.BuildPlatform>
            {
                { BuildTarget.StandaloneWindows, Platform.BuildPlatform.Windows },
                { BuildTarget.StandaloneWindows64, Platform.BuildPlatform.Windows },
#if UNITY_2017_3_OR_NEWER
                { BuildTarget.StandaloneOSX, Platform.BuildPlatform.OSX },
#else
                { BuildTarget.StandaloneOSXIntel, Platform.BuildPlatform.OSX },
                { BuildTarget.StandaloneOSXIntel64, Platform.BuildPlatform.OSX },
                { BuildTarget.StandaloneOSXUniversal, Platform.BuildPlatform.OSX },
#endif
                { BuildTarget.StandaloneLinux, Platform.BuildPlatform.Linux },
                { BuildTarget.StandaloneLinux64, Platform.BuildPlatform.Linux },
                { BuildTarget.iOS, Platform.BuildPlatform.iOS }
            };

        /// <summary>
        ///     Exports all entity prefabs for Development and Deployment, for each worker type.
        /// </summary>
        private static void ExportEntityPrefabs(IEnumerable<string> prefabGuids, IEnumerable<BuildTarget> buildTargets)
        {
            EnsureDirectoriesExist();

            var guids = prefabGuids.ToList();

            foreach (var target in buildTargets)
            {
                ExportEntityPrefabs(guids, target);
            }
        }

        private static void EnsureDirectoriesExist()
        {
            PathUtil.EnsureDirectoryExists(EditorPaths.PrefabExportDirectory);
            PathUtil.EnsureDirectoryExists(EditorPaths.PrefabSourceDirectory);
        }

        public static void ExportEntityPrefabs(IEnumerable<string> prefabGuids, BuildTarget buildTarget)
        {
            RemoveAssetBundleNamesFromAllPrefabs();
            AddAssetBundleNamesToPrefabs(prefabGuids, buildTarget);
            AssetDatabase.SaveAssets();
            ExportAllNamedPrefabs(buildTarget);
            RemoveAssetBundleNamesFromAllPrefabs();
        }

        public static void ExportAllEntityPrefabs()
        {
            var buildTargets = GetAllBuildTargets();
            ExportEntityPrefabs(GetAllPrefabAssetGuids(), buildTargets);
        }

        public static void ExportAllEntityPrefabs(BuildTarget target)
        {
            ExportEntityPrefabs(GetAllPrefabAssetGuids(), new List<BuildTarget> { target });
        }

        public static void ExportDevelopmentEntityPrefabs()
        {
            var targets = UnityPlayerBuilders.DevelopmentPlayerBuilders(SimpleBuildSystem.AllWorkerTypes)
                                             .Select(b => b.BuildTarget)
                                             .Distinct()
                                             .ToList();
            ExportEntityPrefabs(GetAllPrefabAssetGuids(), targets);
        }

        public static void ExportSelectedEntityPrefabs()
        {
            var buildTargets = GetAllBuildTargets();
            ExportEntityPrefabs(GetSelectedPrefabAssetGuids(), buildTargets);
        }

        public static void ExportSelectedEntityPrefabs(BuildTarget buildTarget)
        {
            ExportEntityPrefabs(GetSelectedPrefabAssetGuids(), new List<BuildTarget> { buildTarget });
        }

        public static void ExportSelectedDevelopmentEntityPrefabs()
        {
            var targets = UnityPlayerBuilders.DevelopmentPlayerBuilders(SimpleBuildSystem.AllWorkerTypes)
                                             .Select(b => b.BuildTarget)
                                             .Distinct()
                                             .ToList();
            ExportEntityPrefabs(GetSelectedPrefabAssetGuids(), targets);
        }

        public static IEnumerable<string> GetAllPrefabAssetGuids()
        {
            List<string> dirsToSearch = new List<string> { EditorPaths.PrefabSourceDirectory };

            if (Directory.Exists(EditorPaths.PrefabResourcesDirectory))
            {
                dirsToSearch.Add(EditorPaths.PrefabResourcesDirectory);
            }

            return AssetDatabase.FindAssets("t:prefab", dirsToSearch.ToArray()).Where(IsPrefab);
        }

        public static IEnumerable<string> GetSelectedPrefabAssetGuids()
        {
            return GetAllPrefabAssetGuids().Where(s => Selection.assetGUIDs.Contains(s));
        }

        public static bool AnyPrefabsSelected()
        {
            for (var i = 0; i < Selection.assetGUIDs.Length; i++)
            {
                var guid = Selection.assetGUIDs[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".prefab") && (path.StartsWith(EditorPaths.PrefabSourceDirectory) || path.StartsWith(EditorPaths.PrefabResourcesDirectory)))
                {
                    return true;
                }
            }

            return false;
        }

        private static IList<BuildTarget> GetAllBuildTargets()
        {
            var allTargets = UnityPlayerBuilders.DeploymentPlayerBuilders(SimpleBuildSystem.AllWorkerTypes)
                                                .Union(UnityPlayerBuilders.DevelopmentPlayerBuilders(SimpleBuildSystem.AllWorkerTypes))
                                                .Select(b => b.BuildTarget)
                                                .Distinct()
                                                .ToList();

            // Only export iOS prefabs if building for iOS.
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                allTargets.Add(BuildTarget.iOS);
            }

            return allTargets;
        }

        private static bool IsPrefab(string guid)
        {
            return AssetDatabase.GUIDToAssetPath(guid).EndsWith(".prefab");
        }

        private static void ExportAllNamedPrefabs(BuildTarget buildTarget)
        {
            BuildPipeline.BuildAssetBundles(EditorPaths.PrefabExportDirectory,
                                            BuildAssetBundleOptions.UncompressedAssetBundle,
                                            buildTarget);
        }

        private static void AddAssetBundleNamesToPrefabs(IEnumerable<string> guids, BuildTarget buildTarget)
        {
            if (!buildTargetToBuildPlatform.ContainsKey(buildTarget))
            {
                throw new ArgumentException(string.Format("No build platform is associated with build target {0}.", buildTarget));
            }

            var buildPlatform = buildTargetToBuildPlatform[buildTarget];

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path);
                var name = Path.GetFileNameWithoutExtension(path);
                if (name == null)
                {
                    throw new InvalidOperationException(string.Format("Asset path is invalid {0}", path));
                }

                importer.assetBundleName = string.Format("{0}@{1}", name.ToLowerInvariant(), Platform.BuildPlatformToAssetBundleSuffix(buildPlatform));
            }
        }

        private static void RemoveAssetBundleNamesFromAllPrefabs()
        {
            var prefabGuids = GetAllPrefabAssetGuids();
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path);
                importer.assetBundleName = null;
            }
        }
    }
}
