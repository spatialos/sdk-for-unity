// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Linq;
using Improbable.Assets;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    public class PrefabGameObjectLoader : IAssetLoader<GameObject>
    {
        private const string AssetDatabaseTypeName = "Improbable.Unity.EditorTools.Assets.EditorPrefabGameObjectLoader";
        private IAssetLoader<GameObject> gameObjectLoader;

        public PrefabGameObjectLoader()
        {
            CreateEditorPrefabAssetDatabase();
        }

        private void CreateEditorPrefabAssetDatabase()
        {
            Type assetDatabaseType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                      from type in assembly.GetTypes()
                                      where type.FullName == AssetDatabaseTypeName
                                      select type).First();

            if (assetDatabaseType == null)
            {
                throw new Exception(String.Format("Could not find required asset database type {0}", AssetDatabaseTypeName));
            }

            gameObjectLoader = Activator.CreateInstance(assetDatabaseType) as IAssetLoader<GameObject>;
        }

        public void LoadAsset(string prefabName, Action<GameObject> onGameObjectLoaded, Action<Exception> onError)
        {
            gameObjectLoader.LoadAsset(prefabName, onGameObjectLoaded, onError);
        }

        public void CancelAllLoads()
        {
            gameObjectLoader.CancelAllLoads();
        }
    }
}
