// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Unity.Assets;
using Improbable.Unity.MinimalBuildSystem.Configuration;
using Improbable.Unity.Util;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Improbable.Unity.MinimalBuildSystem
{
    public static class EntityPrefabs
    {
        public static void Export(WorkerPlatform platform)
        {
            Clean();

            var prefabPaths = GetAllPrefabAssetPaths();
            var copies = CopyPrefabs(prefabPaths);
            CompilePrefabs(copies, platform);

            AssetDatabase.SaveAssets();
            Resources.UnloadUnusedAssets();
        }

        public static void Clean()
        {
            FileUtil.DeleteFileOrDirectory(BuildPaths.PrefabResourcesDirectory);
            FileUtil.DeleteFileOrDirectory(BuildPaths.PrefabResourcesDirectory + ".meta");

            AssetDatabase.Refresh();
        }

        private static void CompilePrefabs(List<string> paths, WorkerPlatform platform)
        {
            var progress = 0;
            foreach (var path in paths)
            {
                EditorUtility.DisplayProgressBar("Processing EntityPrefabs", "Preparing",
                                                 progress / (float) paths.Count());
                progress++;

                var compiler = new PrefabCompiler(platform);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                compiler.Compile(prefab);
            }

            EditorUtility.ClearProgressBar();
        }

        private static List<string> CopyPrefabs(List<string> paths)
        {
            AssetDatabase.StartAssetEditing();
            PathUtil.EnsureDirectoryExists(BuildPaths.PrefabResourcesDirectory);
            List<string> newPaths = new List<string>();
            var progress = 0;

            try
            {
                foreach (var path in paths)
                {
                    EditorUtility.DisplayProgressBar("Processing EntityPrefabs", "Preparing",
                                                     progress / (float) paths.Count());
                    progress++;

                    var targetPath = PathUtil.Combine(BuildPaths.PrefabResourcesDirectory, Path.GetFileName(path));
                    if (!AssetDatabase.CopyAsset(path, targetPath))
                    {
                        throw new BuildFailedException("Failed to process EntityPrefab " + path);
                    }

                    newPaths.Add(targetPath);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            return newPaths;
        }

        private static List<string> GetAllPrefabAssetPaths()
        {
            return AssetDatabase.FindAssets("t:prefab", new[] { BuildPaths.PrefabSourceDirectory })
                                .Select(AssetDatabase.GUIDToAssetPath).ToList();
        }
    }
}
