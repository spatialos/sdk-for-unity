// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Assets;
using UnityEngine;

namespace Improbable.MinimalBuildSystem.Prefabs
{
    public class ResourceGameObjectLoader : IAssetLoader<GameObject>
    {
        public void LoadAsset(string prefabName, Action<GameObject> onAssetLoaded, Action<Exception> onError)
        {
            var gameObject = Resources.Load<GameObject>("EntityPrefabs/" + prefabName);
            if (gameObject == null)
            {
                onError(new Exception("Prefab not found: " + prefabName));
            }
            else
            {
                onAssetLoaded(gameObject);
            }
        }

        public void CancelAllLoads()
        {
            // LoadAsset is instantaneous, so no need to do anything here.
        }
    }
}
