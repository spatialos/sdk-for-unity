// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Assets;
using Improbable.Unity.Util;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    class ExponentialBackoffRetryAssetLoader : MonoBehaviour, IAssetLoader<AssetBundle>
    {
        [Range(0, 100)] public int MaxRetries = 3;

        [Range(250, 10000)] public int StartBackoffTimeoutMilliseconds = 250;

        private IAssetLoader<AssetBundle> assetLoader;

        public void Init(IAssetLoader<AssetBundle> assetLoader, int maxRetries = -1, int startBackoffTimeoutMilliseconds = -1)
        {
            this.assetLoader = assetLoader;
            MaxRetries = maxRetries != -1 ? maxRetries : MaxRetries;
            StartBackoffTimeoutMilliseconds = startBackoffTimeoutMilliseconds != -1 ? startBackoffTimeoutMilliseconds : StartBackoffTimeoutMilliseconds;
        }

        /// <inheritdoc />
        public void LoadAsset(string prefabName, Action<AssetBundle> onAssetLoaded, Action<Exception> onError)
        {
            var taskRunner = new TaskRunnerWithExponentialBackoff<Exception>();
            Action runTask = () => { assetLoader.LoadAsset(prefabName, onAssetLoaded, taskRunner.ProcessResult); };
            Func<Exception, TaskResult> evaluationFunc = (Exception e) =>
            {
                return new TaskResult
                {
                    IsSuccess = false, // This function should never be called if the load request was successful.
                    ErrorMessage = e.Message
                };
            };
            Action<Exception> onSuccess = (Exception e) => { throw new InvalidOperationException("ExponentialBackoffRetryAssetLoader: TaskRunnerWithExponentialBackoff::onSuccess was called. This code is not supposed to be reachable."); };
            Action<string> onFailure = (string errorMessage) => { onError(new Exception(errorMessage)); };
            taskRunner.RunTaskWithRetries("ExponentialBackoffRetryAssetLoader::LoadAsset", this, runTask, evaluationFunc, onSuccess, onFailure, MaxRetries, StartBackoffTimeoutMilliseconds / 1000f);
        }

        public void CancelAllLoads()
        {
            assetLoader.CancelAllLoads();
        }
    }
}
