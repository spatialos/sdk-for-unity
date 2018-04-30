// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Assets;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    public class PreprocessingGameObjectLoader : IAssetLoader<GameObject>
    {
        private readonly IAssetLoader<GameObject> gameObjectLoader;
        private readonly PrefabCompiler prefabCompiler;

        public PreprocessingGameObjectLoader(IAssetLoader<GameObject> gameObjectLoader)
        {
            this.gameObjectLoader = gameObjectLoader;
            this.prefabCompiler = new PrefabCompiler(SpatialOS.Configuration.WorkerPlatform);
        }

        public void LoadAsset(string prefabName, Action<GameObject> onAssetLoaded, Action<Exception> onError)
        {
            gameObjectLoader.LoadAsset(prefabName, gameObject =>
            {
                prefabCompiler.Compile(gameObject);
                onAssetLoaded(gameObject);
            }, onError);
        }

        public void CancelAllLoads()
        {
            gameObjectLoader.CancelAllLoads();
        }
    }
}
