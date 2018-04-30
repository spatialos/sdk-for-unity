// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Entity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Improbable.Unity.ComponentFactory
{
    public class PooledPrefabFactory : IPrefabFactory<GameObject>, IDisposable
    {
        private readonly Dictionary<string, List<GameObject>> OutOfDatePools = new Dictionary<string, List<GameObject>>();
        private readonly Dictionary<string, GameObject> Pools = new Dictionary<string, GameObject>();
        public static readonly Vector3 InstantiationPoint = new Vector3(-9999, -9999, -9999);

        public GameObject MakeComponent(GameObject loadedPrefab, string prefabName)
        {
            return Spawn(loadedPrefab, prefabName, InstantiationPoint, Quaternion.identity);
        }

        public GameObject MakeComponent(GameObject prefab, string prefabName, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, prefabName, position, rotation);
        }

        public void DespawnComponent(GameObject gameObject, string prefabName)
        {
            Despawn(gameObject, prefabName);
        }

        public void InvalidatePool(string prefabName)
        {
            if (!Pools.ContainsKey(prefabName))
            {
                return;
            }

            var currentPool = Pools[prefabName];
            Pools.Remove(prefabName);

            MarkPoolAsInvalid(prefabName, currentPool);
        }

        public void PreparePool(GameObject loadedPrefab, string prefabName, int count)
        {
            var poolComponent = GetOrCreatePool(loadedPrefab, prefabName);

            for (var i = 0; i < count; ++i)
            {
                poolComponent.CreateInactiveInPool();
            }
        }

        private GameObject Spawn(GameObject loadedPrefab, string prefabName, Vector3 position, Quaternion rotation)
        {
            var pool = GetOrCreatePool(loadedPrefab, prefabName);
            return pool.Spawn(position, rotation);
        }

        private void Despawn(GameObject pooledGameObject, string prefabName)
        {
            GameObject pool;
            if (Pools.TryGetValue(prefabName, out pool))
            {
                var container = pool.GetComponent<PooledPrefabContainer>();
                if (container.Contains(pooledGameObject))
                {
                    container.Despawn(pooledGameObject);
                    return;
                }
            }

            DespawnFromOldPools(pooledGameObject, prefabName);
        }

        private void DespawnFromOldPools(GameObject pooledGameObject, string prefabName)
        {
            List<GameObject> oldPools;
            if (!OutOfDatePools.TryGetValue(prefabName, out oldPools))
            {
                return;
            }

            foreach (var pool in oldPools)
            {
                var container = pool.GetComponent<PooledPrefabContainer>();
                if (container.Contains(pooledGameObject))
                {
                    container.Despawn(pooledGameObject);
                    if (container.ActiveCount == 0)
                    {
                        oldPools.Remove(pool);
                        Object.Destroy(pool);
                    }

                    return;
                }
            }
        }

        private void MarkPoolAsInvalid(string prefabName, GameObject pool)
        {
            List<GameObject> oldPools;
            if (!OutOfDatePools.TryGetValue(prefabName, out oldPools))
            {
                oldPools = new List<GameObject>();
                OutOfDatePools.Add(prefabName, oldPools);
            }

            oldPools.Add(pool);
        }

        private PooledPrefabContainer GetOrCreatePool(GameObject loadedPrefab, string prefabName)
        {
            GameObject pool;
            if (Pools.TryGetValue(prefabName, out pool))
            {
                return pool.GetComponent<PooledPrefabContainer>();
            }

            return CreatePool(loadedPrefab, prefabName);
        }

        private PooledPrefabContainer CreatePool(GameObject loadedPrefab, string prefabName)
        {
            var pool = new GameObject();
            Pools[prefabName] = pool;

            var poolComponent = pool.AddComponent<PooledPrefabContainer>();
            poolComponent.Init(loadedPrefab, prefabName);

            return poolComponent;
        }

        private static void DestroyPool(GameObject pool)
        {
            var poolPrefabContainer = pool.GetComponent<PooledPrefabContainer>();
            if (poolPrefabContainer != null)
            {
                poolPrefabContainer.Dispose();
            }

            Object.Destroy(pool);
        }

        private void DisposeOfActivePools()
        {
            var reportedProblem = false;

            // Destroy all active pools and spawned objects
            foreach (var pool in Pools.Values)
            {
                try
                {
                    DestroyPool(pool);
                }
                catch (Exception e)
                {
                    if (!reportedProblem)
                    {
                        reportedProblem = true;
                        Debug.LogError("Failed to destroy active prefab pools. This can happen when pool objects are destroyed by user code.");

                        Debug.LogException(e);
                    }
                }
            }

            Pools.Clear();
        }

        private void DisposeOfOutOfDatePools()
        {
            // Prevent repetition of errors if
            var reportedProblem = false;

            // Destroy all inactive pools, as they can still contain spawned objects
            foreach (var poolList in OutOfDatePools.Values)
            {
                foreach (var pool in poolList)
                {
                    try
                    {
                        DestroyPool(pool);
                    }
                    catch (Exception e)
                    {
                        if (!reportedProblem)
                        {
                            reportedProblem = true;
                            Debug.LogError("Failed to destroy inactive prefab pools. This can happen when pool objects are destroyed by user code.");

                            Debug.LogException(e);
                        }
                    }
                }

                poolList.Clear();
            }

            OutOfDatePools.Clear();
        }

        public void Dispose()
        {
            DisposeOfActivePools();

            DisposeOfOutOfDatePools();
        }
    }
}
