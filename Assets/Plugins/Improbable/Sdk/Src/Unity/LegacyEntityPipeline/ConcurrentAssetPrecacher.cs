// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    class ConcurrentAssetPrecacher
    {
        private readonly MonoBehaviour hostMonoBehaviour;

        private int completedPrecachedCount;
        private int concurrentDownloads;
        private int nextAssetToPrecacheIndex;

        private readonly IList<string> assetsToPrecache;
        private readonly int maxConcurrentConnections;
        private readonly Action onCompleted;
        private readonly Action<int> onProgress;
        private readonly IEntityTemplateProvider entityTemplateProvider;

        public ConcurrentAssetPrecacher(MonoBehaviour hostMonoBehaviour, IList<string> assetsToPrecache, Action onCompleted, Action<int> onProgress, IEntityTemplateProvider entityTemplateProvider, int concurrentConnections = 5)
        {
            this.hostMonoBehaviour = hostMonoBehaviour;
            this.assetsToPrecache = assetsToPrecache;
            this.onCompleted = onCompleted;
            this.onProgress = onProgress;
            this.entityTemplateProvider = entityTemplateProvider;

            maxConcurrentConnections = concurrentConnections;
        }

        public void BeginPrecaching()
        {
            if (maxConcurrentConnections < 1)
            {
                throw new ArgumentException("Maximum concurrent connections must be greater than 0.");
            }

            hostMonoBehaviour.StartCoroutine(Precache());
        }

        private IEnumerator Precache()
        {
            ReportProgress();
            while (completedPrecachedCount < assetsToPrecache.Count)
            {
                while (nextAssetToPrecacheIndex < assetsToPrecache.Count && concurrentDownloads < maxConcurrentConnections)
                {
                    StartPrecachingAsset();
                }

                yield return null;
            }

            if (onCompleted != null)
            {
                onCompleted();
            }
        }

        private void StartPrecachingAsset()
        {
            var assetToPrecache = assetsToPrecache[nextAssetToPrecacheIndex];
            OnAssetPrecacheStarted(assetToPrecache);
            entityTemplateProvider.PrepareTemplate(assetToPrecache,
                                                   OnAssetPrecached,
                                                   ex => OnAssetPrecacheFailed(assetToPrecache, ex));
        }

        private void OnAssetPrecacheStarted(string prefabName)
        {
            ++nextAssetToPrecacheIndex;
            ++concurrentDownloads;
        }

        private void OnAssetPrecached(string prefabName)
        {
            OnPrecachingAssetCompleted();
        }

        private void OnAssetPrecacheFailed(string prefabName, Exception ex)
        {
            OnPrecachingAssetCompleted();
            Debug.LogErrorFormat("Failed to precache asset '{0}.\nException: {1}", prefabName, ex);
        }

        private void OnPrecachingAssetCompleted()
        {
            ++completedPrecachedCount;
            --concurrentDownloads;
            ReportProgress();
        }

        private void ReportProgress()
        {
            if (onProgress != null)
            {
                onProgress(completedPrecachedCount);
            }
        }
    }
}
