// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Contains information about a SpatialOS entity and its associated GameObject.
    /// </summary>
    public interface IEntityObject : IPipelineEntityComponentOpsReceiver
    {
        /// <summary>
        ///     Returns the entity's entity id.
        /// </summary>
        EntityId EntityId { get; }

        /// <summary>
        ///     Returns the entity's prefab name.
        /// </summary>
        string PrefabName { get; }

        /// <summary>
        ///     Returns the GameObject associated with this entity.
        /// </summary>
        GameObject UnderlyingGameObject { get; }

        /// <summary>
        ///     Object for managing visualizers.
        /// </summary>
        IEntityVisualizers Visualizers { get; }

        /// <summary>
        ///     Object for managing components.
        /// </summary>
        IEntityComponents Components { get; }
    }
}
