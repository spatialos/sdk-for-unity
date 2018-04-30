// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Interface for interacting with the <see cref="IUniverse" /> instances.
    /// </summary>
    interface IMutableUniverse : IUniverse
    {
        /// <summary>
        ///     Adds the object to the universe with the given EntityId.
        /// </summary>
        void AddEntity(IEntityObject entity);

        /// <summary>
        ///     Removes the object with the given EntityId from the universe.
        /// </summary>
        /// <param name="entityId"></param>
        void Remove(EntityId entityId);

        /// <summary>
        ///     Removes all objects from the universe.
        /// </summary>
        void Clear();
    }
}
