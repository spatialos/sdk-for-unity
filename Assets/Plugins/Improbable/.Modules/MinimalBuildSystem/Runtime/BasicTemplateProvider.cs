// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Assets;
using Improbable.Unity;
using Improbable.Unity.Assets;
using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.MinimalBuildSystem.Prefabs
{
    public class BasicTemplateProvider : MonoBehaviour, IEntityTemplateProvider
    {
        public WorkerPlatform platform;

        // The template provider can't be instantiated during construction as Application.isEditor doesn't work.
        private IEntityTemplateProvider templateProvider;

        private IEntityTemplateProvider TemplateProvider
        {
            get
            {
                if (templateProvider == null)
                {
                    var gameObjectLoader = InitializeAssetLoader();
                    templateProvider = InitializeTemplateProvider(gameObjectLoader);
                }

                return templateProvider;
            }
        }

        private IAssetLoader<GameObject> InitializeAssetLoader()
        {
            return new ResourceGameObjectLoader();
        }

        private IEntityTemplateProvider InitializeTemplateProvider(IAssetLoader<GameObject> gameObjectLoader)
        {
            return new AssetDatabaseTemplateProvider(new CachingAssetDatabase(gameObjectLoader));
        }

        public void PrepareTemplate(string prefabName, Action<string> onSuccess, Action<Exception> onError)
        {
            TemplateProvider.PrepareTemplate(prefabName, onSuccess, onError);
        }

        public GameObject GetEntityTemplate(string prefabName)
        {
            return TemplateProvider.GetEntityTemplate(prefabName);
        }

        public void CancelAllTemplatePreparations()
        {
            TemplateProvider.CancelAllTemplatePreparations();
        }
    }
}
