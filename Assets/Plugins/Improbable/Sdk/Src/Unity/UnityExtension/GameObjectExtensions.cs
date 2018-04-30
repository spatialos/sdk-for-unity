// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System.Collections.Generic;
using Improbable;
using Improbable.Unity.Entity;

// ReSharper disable once CheckNamespace
namespace UnityEngine
{
    /// <summary>
    ///     Helper methods for dealing with associations between Unity's GameObjects and SpatialOS entities.
    /// </summary>
    public static class GameObjectExtensions
    {
        private static readonly Dictionary<GameObject, IEntityObject> GameObjectToEntityObjectCache = new Dictionary<GameObject, IEntityObject>();
        private static readonly Dictionary<IEntityObject, List<GameObject>> EntityObjectToGameObjectCache = new Dictionary<IEntityObject, List<GameObject>>();

        /// <summary>
        ///     Returns the SpatialOS entity ID associated with this GameObject.
        /// </summary>
        /// <remarks>
        ///     Call <seealso cref="IsSpatialOsEntity" /> or <seealso cref="IsParentedBySpatialOsEntity" /> to check that there is
        ///     a SpatialOS entity associated with the GameObject.
        ///     This method searches the GameObject's parent hierarchy to find the root GameObject associated with a SpatialOS
        ///     entity.
        ///     Returns an invalid entity ID if neither the GameObject, nor any of its parents, are a SpatialOS entity.
        /// </remarks>
        public static EntityId EntityId(this GameObject gameObject)
        {
            var entityObject = GetSpatialOsEntity(gameObject);
            return entityObject == null ? new EntityId() : entityObject.EntityId;
        }

        /// <summary>
        ///     Returns true if the specific GameObject is associated with a SpatialOS entity.
        /// </summary>
        public static bool IsSpatialOsEntity(this GameObject gameObject)
        {
            var storage = gameObject.GetComponent<EntityObjectStorage>();
            return IsStorageValid(storage);
        }

        /// <summary>
        ///     Returns true if the GameObject, or any of its parents, is associated with a SpatialOS entity.
        /// </summary>
        public static bool IsParentedBySpatialOsEntity(this GameObject gameObject)
        {
            var entity = GetSpatialOsEntity(gameObject);
            return entity != null;
        }

        /// <summary>
        ///     Finds the SpatialOS entity that belongs to the GameObject , or null if there is none associated.
        /// </summary>
        /// <remarks>
        ///     This method searches the GameObject's parent hierarchy to find the root GameObject associated with a SpatialOS
        ///     entity.
        ///     The result is cached.
        ///     To clean the cache, call the <seealso cref="RemoveFromLookupCache" /> method.
        /// </remarks>
        public static IEntityObject GetSpatialOsEntity(this GameObject gameObject)
        {
            IEntityObject entityObject;
            if (GameObjectToEntityObjectCache.TryGetValue(gameObject, out entityObject))
            {
                return entityObject;
            }

            var storage = gameObject.GetComponentInParent<EntityObjectStorage>();
            if (!IsStorageValid(storage))
            {
                return null;
            }

            entityObject = storage.Entity;

            GameObjectToEntityObjectCache.Add(gameObject, entityObject);
            AddToReverseLookup(gameObject, entityObject);
            return entityObject;
        }

        /// <summary>
        ///     Removes an entityObject from the lookup cache.
        ///     It will be re-added the next time it is looked up with <seealso cref="GetSpatialOsEntity" />.
        /// </summary>
        public static void RemoveFromLookupCache(IEntityObject entityObject)
        {
            if (entityObject == null)
            {
                return;
            }

            List<GameObject> objectsToRemove;
            if (!EntityObjectToGameObjectCache.TryGetValue(entityObject, out objectsToRemove))
            {
                return;
            }

            for (int index = 0; index < objectsToRemove.Count; index++)
            {
                var invalidObject = objectsToRemove[index];
                if (invalidObject != null)
                {
                    GameObjectToEntityObjectCache.Remove(invalidObject);
                }
            }

            EntityObjectToGameObjectCache.Remove(entityObject);
        }

        private static void AddToReverseLookup(GameObject gameObject, IEntityObject entityObject)
        {
            List<GameObject> gameObjects;
            if (!EntityObjectToGameObjectCache.TryGetValue(entityObject, out gameObjects))
            {
                gameObjects = new List<GameObject>();
                EntityObjectToGameObjectCache[entityObject] = gameObjects;
            }

            gameObjects.Add(gameObject);
        }

        private static bool IsStorageValid(EntityObjectStorage storage)
        {
            return storage != null && storage.Entity != null;
        }
    }
}
