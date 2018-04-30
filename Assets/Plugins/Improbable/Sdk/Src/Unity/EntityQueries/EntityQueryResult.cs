// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Collections;

namespace Improbable.Unity.Core.EntityQueries
{
    /// <summary>
    ///     Contains the result of a query.
    /// </summary>
    public struct EntityQueryResult
    {
        private readonly int entityCount;
        private readonly Map<EntityId, Worker.Entity> entities;

        public EntityQueryResult(int entityCount, Map<EntityId, Worker.Entity> entities)
        {
            this.entityCount = entityCount;
            this.entities = entities;
        }

        /// <summary>
        ///     Returns the number of entities in the result set.
        /// </summary>
        public int EntityCount
        {
            get { return entityCount; }
        }

        /// <summary>
        ///     Returns a map of entities keyed by their entity ids.
        ///     Empty if the query was just for a count.
        /// </summary>
        public Map<EntityId, Worker.Entity> Entities
        {
            get { return entities; }
        }
    }
}
