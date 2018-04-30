// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <inheritdoc />
    /// <summary>
    ///     Contains all of the entities that currently exist on this worker.
    /// </summary>
    public class LocalEntities : IMutableLocalEntities
    {
        private readonly EntityObjectDictionary entities = new EntityObjectDictionary();

        private static readonly LocalEntities LocalEntitiesInstance = new LocalEntities();

        public IEnumerable<IEntityObject> GetAll()
        {
            return entities.Values;
        }

        /// <summary>
        ///     Singleton instance of <see cref="ILocalEntities" />.
        /// </summary>
        public static ILocalEntities Instance
        {
            get { return LocalEntitiesInstance; }
        }

        /// <summary>
        ///     Mutable interface to <see cref="IMutableLocalEntities" />.
        /// </summary>
        /// <remarks>
        ///     Should only be accessed from code related to entity lifecycle.
        /// </remarks>
        internal static IMutableLocalEntities Internal
        {
            get { return LocalEntitiesInstance; }
        }

        public void IterateOverAllEntityObjects(Action<EntityId, IEntityObject> action)
        {
            foreach (var entity in entities.Values)
            {
                action(entity.EntityId, entity);
            }
        }

        public IEntityObject Get(EntityId entityId)
        {
            IEntityObject entity;
            entities.TryGetValue(entityId, out entity);
            return entity;
        }

        public bool ContainsEntity(EntityId entityId)
        {
            return entities.Contains(entityId);
        }

        public void AddEntity(IEntityObject entity)
        {
            if (entity == null)
            {
                Debug.LogError("Storing a null EntityObject");
                return;
            }

            if (entities.Contains(entity))
            {
                Debug.LogErrorFormat("Trying to store a duplicate with EntityId {0} for object {1}", entity.EntityId, entity.UnderlyingGameObject.name);
                return;
            }

            entities.Add(entity);
        }

        public void Remove(EntityId entityId)
        {
            if (!entities.Remove(entityId))
            {
                Debug.LogErrorFormat("Trying to remove an unknown EntityId {0}", entityId);
            }
        }

        public void Clear()
        {
            entities.Clear();
        }
    }
}
