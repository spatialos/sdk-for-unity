// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using Improbable.Assets;
using Improbable.Unity.EditorTools.PrefabExport;
using Improbable.Unity.EditorTools.Util;
using Improbable.Unity.Util;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Assets
{
    public class EditorPrefabGameObjectLoader : IAssetLoader<GameObject>
    {
        private readonly IDictionary<string, string> prefabs = new Dictionary<string, string>();

        public EditorPrefabGameObjectLoader()
        {
            CleanOutputFolder();
            FindPrefabsInProject();
        }

        public void LoadAsset(string prefabName, Action<GameObject> onGameObjectLoaded, Action<Exception> onError)
        {
            try
            {
                string path;
                if (prefabs.TryGetValue(prefabName, out path))
                {
                    var source = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    var prefabPath = Path.Combine(EditorPaths.PrefabCompileDirectory, prefabName + ".prefab");
                    var prefabGameObject = PrefabUtility.CreatePrefab(prefabPath.ToUnityPath(), source);
                    if (prefabGameObject == null)
                    {
                        onError(new Exception(string.Format("Could not load the game object from the local prefab '{0}'.", prefabName)));
                    }
                    else
                    {
                        onGameObjectLoaded(prefabGameObject);
                    }
                }
                else
                {
                    onError(new Exception(string.Format("Could not find the local prefab '{0}'.", prefabName)));
                }
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        }

        public void CancelAllLoads()
        {
            // LoadAsset is instantaneous, so no need to do anything here.
        }

        private void FindPrefabsInProject()
        {
            var guids = EntityPrefabExporter.GetAllPrefabAssetGuids();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(name))
                {
                    Debug.LogWarningFormat("Found a prefab an empty name on path '{0}'. Please give it a name.", path);
                }
                else if (prefabs.ContainsKey(name))
                {
                    Debug.LogWarningFormat("Duplicate Prefab detected: {0}", path);
                }
                else
                {
                    prefabs.Add(name, path);
                }
            }
        }

        private static void CleanOutputFolder()
        {
            Directory.CreateDirectory(EditorPaths.PrefabCompileDirectory);
            var info = new DirectoryInfo(EditorPaths.PrefabCompileDirectory);
            var files = info.GetFiles();
            foreach (var fileInfo in files)
            {
                fileInfo.Delete();
            }
        }
    }
}
