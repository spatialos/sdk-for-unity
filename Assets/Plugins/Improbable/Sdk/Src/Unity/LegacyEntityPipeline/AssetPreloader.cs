// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Improbable.Unity.Assets;
using Improbable.Unity.ComponentFactory;
using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.Unity.Core
{
    class AssetPreloader : IDisposable
    {
        private readonly PooledPrefabFactory prefabPool;
        private readonly IEntityTemplateProvider templateProvider;
        private readonly IList<string> assetsToPrecache;

        private readonly IEnumerable<KeyValuePair<string, int>> assetsToPrePool;

        private readonly ConcurrentAssetPrecacher downloader;

        private bool downloadCompleted;

        public event Action PrecachingCompleted;
        public event Action<int> PrecachingProgress;

        public AssetPreloader(MonoBehaviour hostBehaviour, IEntityTemplateProvider templateProvider, IList<string> assetsToPrecache, IEnumerable<KeyValuePair<string, int>> assetsToPrePool, int maxConcurrentPrecacheConnections)
        {
            this.templateProvider = templateProvider;

            this.prefabPool = new PooledPrefabFactory();
            this.assetsToPrecache = assetsToPrecache;
            this.assetsToPrePool = assetsToPrePool;

            downloader = new ConcurrentAssetPrecacher(
                                                      hostBehaviour,
                                                      assetsToPrecache,
                                                      OnPrecachingCompleted,
                                                      OnPrecachingProgress,
                                                      templateProvider,
                                                      maxConcurrentPrecacheConnections);
        }

        private void OnPrecachingCompleted()
        {
            if (PrecachingCompleted != null)
            {
                PrecachingCompleted();
            }

            downloadCompleted = true;
        }

        private void OnPrecachingProgress(int progress)
        {
            if (PrecachingProgress != null)
            {
                PrecachingProgress(progress);
            }
        }

        public IEnumerator PrepareAssets()
        {
            PrePoolAssets();
            if (assetsToPrecache != null && assetsToPrecache.Count > 0)
            {
                var precache = PrecacheAssets();
                yield return precache;
            }

            yield return null;
        }

        private IEnumerator PrecacheAssets()
        {
            downloader.BeginPrecaching();

            var wait = new WaitUntil(() => downloadCompleted);
            yield return wait;
        }

        private void PrePoolAssets()
        {
            if (assetsToPrePool == null || !assetsToPrePool.Any())
            {
                return;
            }

            foreach (var prefabNameToCount in assetsToPrePool)
            {
                // Prepooling only supports default context at the moment.  Ultimately all this code should be pulled out into user code.
                var requestedCountInPool = prefabNameToCount.Value;
                templateProvider.PrepareTemplate(prefabNameToCount.Key, prefabName =>
                                                 {
                                                     var prefab = templateProvider.GetEntityTemplate(prefabName);
                                                     prefabPool.PreparePool(prefab, prefabName, requestedCountInPool);
                                                 },
                                                 exception => Debug.LogErrorFormat("Problem initialising pool for entity {0}: {1}",
                                                                                   prefabNameToCount.Key, exception.Message));
            }
        }

        public IPrefabFactory<GameObject> PrefabFactory
        {
            get { return prefabPool; }
        }

        public void Dispose()
        {
            if (prefabPool != null)
            {
                prefabPool.Dispose();
            }
        }
    }
}
