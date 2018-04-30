// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Assets;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Wraps an IAssetDatabase to work as an IEntityTemplateProvider.
    /// </summary>
    public class AssetDatabaseTemplateProvider : IEntityTemplateProvider
    {
        private readonly IAssetDatabase<GameObject> assetDatabase;

        public AssetDatabaseTemplateProvider(IAssetDatabase<GameObject> assetDatabase)
        {
            this.assetDatabase = assetDatabase;
        }

        public void PrepareTemplate(string prefabName, Action<string> onSuccess, Action<Exception> onError)
        {
            assetDatabase.LoadAsset(prefabName, _ => onSuccess(prefabName), onError);
        }

        public void CancelAllTemplatePreparations()
        {
            assetDatabase.CancelAllLoads();
        }

        public GameObject GetEntityTemplate(string prefabName)
        {
            GameObject templateObject;
            if (!assetDatabase.TryGet(prefabName, out templateObject))
            {
                throw new MissingComponentException(string.Format("Prefab: {0} cannot be found.", prefabName));
            }

            return templateObject;
        }
    }
}
