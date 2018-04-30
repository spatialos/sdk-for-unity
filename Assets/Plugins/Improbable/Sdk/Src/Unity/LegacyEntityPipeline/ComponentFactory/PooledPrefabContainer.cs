// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Improbable.Unity.ComponentFactory
{
    public class PooledPrefabContainer : MonoBehaviour, IDisposable
    {
        private const int PoolLayer = 31;

        private GameObject LoadedPrefab;
        private string prefabName;

        private int InstanceNumber = 1;

        private readonly Dictionary<GameObject, PooledObject> SpawnedObjects = new Dictionary<GameObject, PooledObject>();
        private readonly List<PooledObject> DespawnedObjects = new List<PooledObject>();

        public void Init(GameObject prefab, string prefabName)
        {
            AddSelfToPoolLayer();
            LoadedPrefab = prefab;
            this.prefabName = prefabName;
            name = string.Format("[Pool] {0}", prefabName);
        }

        public void Dispose()
        {
            DespawnAllObjects();
            DestroyDespawnedObjects();
        }

        private void DespawnAllObjects()
        {
            var reportedProblem = false;

            // Gather all spawned objects to an array as the SpawnedObjects dictionary will be modified with Despawn
            var spawnedObjects = SpawnedObjects.Values.ToArray();

            foreach (var spawnedObject in spawnedObjects)
            {
                try
                {
                    Despawn(spawnedObject.GameObject);
                }
                catch (Exception e)
                {
                    if (!reportedProblem)
                    {
                        reportedProblem = true;

                        Debug.LogErrorFormat("Failed to despawn object for prefab {0} (container {1}). This can happen when spawned objects are destroyed by user code.", prefabName, name);

                        Debug.LogException(e);
                    }
                }
            }

            if (SpawnedObjects.Count != 0)
            {
                Debug.LogWarningFormat("Not all pooled objects for prefab {0} have been despawned (container {1}).",
                                       prefabName, name);
            }
        }

        private void DestroyDespawnedObjects()
        {
            var reportedProblem = false;

            foreach (var despawnedObject in DespawnedObjects)
            {
                try
                {
                    Destroy(despawnedObject.GameObject);
                }
                catch (Exception e)
                {
                    if (!reportedProblem)
                    {
                        reportedProblem = true;

                        Debug.LogErrorFormat("Failed to destroy object for prefab {0} (container {1}). This can happen when spawned objects are destroyed by user code.", prefabName, name);

                        Debug.LogException(e);
                    }
                }
            }

            DespawnedObjects.Clear();
        }

        private void AddSelfToPoolLayer()
        {
            gameObject.layer = PoolLayer;
        }

        public static bool IsPool(GameObject obj)
        {
            return obj.layer == PoolLayer;
        }

        public void Despawn(GameObject spawnedObject)
        {
            PooledObject pooled;
            if (SpawnedObjects.TryGetValue(spawnedObject, out pooled))
            {
                pooled.DespawnedOnFrame = Time.frameCount;
                SpawnedObjects.Remove(spawnedObject);
                if (SetDespawned(spawnedObject))
                {
                    DespawnedObjects.Add(pooled);
                }
            }
            else
            {
                Debug.LogWarningFormat("Could not despawn {0} (prefab {1})", spawnedObject.name, prefabName);
            }
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            var freeObject = FindExistingObject() ?? CreateNewObject();
            InitObject(position, rotation, freeObject);
            return freeObject.GameObject;
        }

        public bool Contains(GameObject obj)
        {
            return SpawnedObjects.ContainsKey(obj);
        }

        public int ActiveCount
        {
            get { return SpawnedObjects.Count; }
        }

        private PooledObject CreateNewObject()
        {
            return new PooledObject(LoadedPrefab) { GameObject = { name = string.Format("{0} {1:#000}", prefabName, InstanceNumber++) } };
        }

        private bool SetDespawned(GameObject spawnedObject)
        {
            try
            {
                spawnedObject.transform.SetParent(transform);
                spawnedObject.SetActive(false);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Failed to despawn object for prefab {0} (container {1}). This can happen when spawned objects are destroyed by user code.", prefabName, name);
                Debug.LogException(e);
                return false;
            }
        }

        public void CreateInactiveInPool()
        {
            var pooled = CreateNewObject();
            if (SetDespawned(pooled.GameObject))
            {
                DespawnedObjects.Add(pooled);
            }
        }

        private void InitObject(Vector3 position, Quaternion rotation, PooledObject pooledObject)
        {
            pooledObject.GameObject.transform.SetPositionAndRotation(position, rotation);
            pooledObject.GameObject.transform.SetParent(transform);

            SpawnedObjects[pooledObject.GameObject] = pooledObject;

            pooledObject.GameObject.SetActive(true);
        }

        private PooledObject FindExistingObject()
        {
            // Entities that were despawned within the last 2 frames are ignored to ensure that they aren't
            // re-used before all of their scheduled cleanup operations are completely finished
            for (var index = 0; index < DespawnedObjects.Count; index++)
            {
                var obj = DespawnedObjects[index];
                if (Time.frameCount - obj.DespawnedOnFrame >= 2)
                {
                    DespawnedObjects.RemoveAt(index);
                    return obj;
                }
            }

            return null;
        }

        private class PooledObject
        {
            public PooledObject(GameObject loadedPrefab)
            {
                GameObject = (GameObject) Instantiate(loadedPrefab);
            }

            public GameObject GameObject { get; private set; }
            public int DespawnedOnFrame { get; set; }
        }
    }
}
