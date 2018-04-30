// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Assets;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    class GameObjectFromAssetBundleLoader : IAssetLoader<GameObject>
    {
        private readonly IAssetLoader<AssetBundle> assetBundleLoader;
        private HashSet<string> inFlightAssetRequests = new HashSet<string>();

        //Each call to LoadAsset will have it's Actions stored in an OnLoadCallback object and called
        //later, once the asset bundle has been loaded.
        private struct OnLoadCallback
        {
            public Action<GameObject> onSuccess;
            public Action<Exception> onFail;
        }

        //Stores a look up table of prefab names, to a list of call backs that are stored per call to LoadAsset
        private Dictionary<string, List<OnLoadCallback>> mapPrefabToLoadCallbacks = new Dictionary<string, List<OnLoadCallback>>();

        public GameObjectFromAssetBundleLoader(IAssetLoader<AssetBundle> assetBundleLoader)
        {
            this.assetBundleLoader = assetBundleLoader;
        }

        public int NumberOfInFlightAssetRequests()
        {
            return inFlightAssetRequests.Count;
        }

        public void LoadAsset(string prefabName, Action<GameObject> onGameObjectLoaded, Action<Exception> onError)
        {
            var callBackPair = new OnLoadCallback { onSuccess = onGameObjectLoaded, onFail = onError };
            if (!mapPrefabToLoadCallbacks.ContainsKey(prefabName))
            {
                mapPrefabToLoadCallbacks.Add(prefabName, new List<OnLoadCallback> { callBackPair });
            }
            else
            {
                mapPrefabToLoadCallbacks[prefabName].Add(callBackPair);
            }

            if (!inFlightAssetRequests.Contains(prefabName))
            {
                inFlightAssetRequests.Add(prefabName);
                assetBundleLoader.LoadAsset(prefabName,
                                            loadedAssetBundle => OnAssetBundleLoaded(loadedAssetBundle, prefabName),
                                            ex => OnAssetLoadFailure(prefabName, ex));
            }
        }

        public void CancelAllLoads()
        {
            assetBundleLoader.CancelAllLoads();

            mapPrefabToLoadCallbacks.Clear();

            inFlightAssetRequests.Clear();
        }

        private void OnAssetBundleLoaded(AssetBundle loadedAssetBundle, string prefabName)
        {
            inFlightAssetRequests.Remove(prefabName);

            List<OnLoadCallback> listOfCallbacks;
            if (prefabName != null && mapPrefabToLoadCallbacks.TryGetValue(prefabName, out listOfCallbacks))
            {
                for (var i = 0; i < listOfCallbacks.Count; ++i)
                {
                    try
                    {
                        var gameObject = loadedAssetBundle.LoadAsset<GameObject>(prefabName);
                        if (gameObject == null)
                        {
                            listOfCallbacks[i].onFail(new Exception(string.Format("Could not load the game object from asset '{0}'.", prefabName)));
                        }
                        else
                        {
                            listOfCallbacks[i].onSuccess(gameObject);
                        }
                    }
                    catch (Exception ex)
                    {
                        listOfCallbacks[i].onFail(ex);
                    }
                }

                mapPrefabToLoadCallbacks.Remove(prefabName);
            }
            else
            {
                Debug.LogErrorFormat("Prefab {0} not found in asset load success callback", prefabName);
            }

            //Always unload the asset bundle regardless
            loadedAssetBundle.Unload(unloadAllLoadedObjects: false);
        }

        private void OnAssetLoadFailure(string prefabName, Exception ex)
        {
            inFlightAssetRequests.Remove(prefabName);

            List<OnLoadCallback> listOfCallbacks;
            if (mapPrefabToLoadCallbacks.TryGetValue(prefabName, out listOfCallbacks))
            {
                for (var i = 0; i < listOfCallbacks.Count; ++i)
                {
                    listOfCallbacks[i].onFail(ex);
                }

                mapPrefabToLoadCallbacks.Remove(prefabName);
            }
            else
            {
                Debug.LogErrorFormat("Prefab {0} not found in asset load fail callback", prefabName);
            }
        }
    }
}
