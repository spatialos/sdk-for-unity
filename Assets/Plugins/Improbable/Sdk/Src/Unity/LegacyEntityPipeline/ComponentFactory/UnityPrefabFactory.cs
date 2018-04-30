// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.Unity.ComponentFactory
{
    public class UnityPrefabFactory : IPrefabFactory<GameObject>
    {
        private static readonly Vector3 InstantiationPoint = new Vector3(-9999, -9999, -9999);

        public GameObject MakeComponent(GameObject loadedPrefab, string prefabName)
        {
            return Object.Instantiate(loadedPrefab, InstantiationPoint, Quaternion.identity);
        }

        public void DespawnComponent(GameObject gameObject, string prefabName)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                Object.DestroyImmediate(gameObject);
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }
    }
}
