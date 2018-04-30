// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Assets;
using Improbable.Unity.Assets;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.MinimalBuildSystem.Prefabs
{
    public class BasicTemplateProvider : MonoBehaviour, IEntityTemplateProvider
    {
        // These can be overridden on the command line.
        public bool UseLocalPrefabs = true;

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
#if UNITY_EDITOR
            UseLocalPrefabs =
                SpatialOS.Configuration.GetCommandLineValue(CommandLineConfigNames.UseLocalPrefabs, UseLocalPrefabs);
            if (UseLocalPrefabs)
            {
                return new PrefabGameObjectLoader();
            }
#endif
            return new ResourceGameObjectLoader();
        }

        private IEntityTemplateProvider InitializeTemplateProvider(IAssetLoader<GameObject> gameObjectLoader)
        {
#if UNITY_EDITOR
            if (UseLocalPrefabs)
            {
                return new AssetDatabaseTemplateProvider(
                                                         new CachingAssetDatabase(new PreprocessingGameObjectLoader(gameObjectLoader)));
            }
#endif
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
