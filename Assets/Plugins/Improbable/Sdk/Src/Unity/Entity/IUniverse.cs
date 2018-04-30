// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Contains all of the entities that currently exist on this worker.
    /// </summary>
    public interface IUniverse
    {
        /// <summary>
        ///     Checks if the entityId currently exists on this worker.
        /// </summary>
        bool ContainsEntity(EntityId entityId);

        /// <summary>
        ///     Get the EntityObject associated with entityId, or null if it doesn't exist.
        /// </summary>
        IEntityObject Get(EntityId entityId);

        /// <summary>
        ///     Gets all available EntityObjects for this worker.
        /// </summary>
        IEnumerable<IEntityObject> GetAll();

        /// <summary>
        ///     Applies the supplied action to every member of the Universe.
        /// </summary>
        [Obsolete("Obsolete in 10.4.0. Migrate to use DefaultEntityPipeline and use custom entity tracking.")]
        void IterateOverAllEntityObjects(Action<EntityId, IEntityObject> action);
    }
}
