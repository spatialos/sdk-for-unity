// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Assets;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    public class CachingAssetDatabase : IAssetDatabase<GameObject>
    {
        private readonly IDictionary<string, GameObject> cachedGameObjects = new Dictionary<string, GameObject>();
        private readonly IAssetLoader<GameObject> gameObjectLoader;

        public CachingAssetDatabase(IAssetLoader<GameObject> gameObjectLoader)
        {
            this.gameObjectLoader = gameObjectLoader;
        }

        public void LoadAsset(string prefabName, Action<GameObject> onAssetLoaded, Action<Exception> onError)
        {
            GameObject cachedGameObject;
            if (cachedGameObjects.TryGetValue(prefabName, out cachedGameObject))
            {
                onAssetLoaded(cachedGameObject);
            }
            else
            {
                gameObjectLoader.LoadAsset(prefabName, gameObject =>
                {
                    if (!cachedGameObjects.ContainsKey(prefabName))
                    {
                        cachedGameObjects.Add(prefabName, gameObject);
                    }

                    onAssetLoaded(gameObject);
                }, onError);
            }
        }

        public void CancelAllLoads()
        {
            gameObjectLoader.CancelAllLoads();
        }

        public bool TryGet(string prefabName, out GameObject prefabGameObject)
        {
            return cachedGameObjects.TryGetValue(prefabName, out prefabGameObject);
        }
    }
}
