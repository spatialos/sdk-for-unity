// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Implement this interface to customize how many entities the Unity SDK attempts to spawn per frame.
    /// </summary>
    public interface IEntitySpawnLimiter
    {
        /// <summary>
        ///     Return false to stop spawning entities for this frame.
        /// </summary>
        bool CanSpawnEntity();

        /// <summary>
        ///     Called after an entity has been spawned.
        /// </summary>
        void EntityAdded(EntityId entityId);

        /// <summary>
        ///     Called before spawning begins for the current frame.
        /// </summary>
        void Reset();
    }
}
