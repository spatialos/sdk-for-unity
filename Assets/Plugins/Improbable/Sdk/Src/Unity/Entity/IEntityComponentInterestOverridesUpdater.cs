// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Calculates interested components of an entity.
    /// </summary>
    public interface IEntityComponentInterestOverridesUpdater
    {
        /// <summary>
        ///     Request recalculation of interested components for an entity.
        /// </summary>
        void InvalidateEntity(IEntityObject entityObject);

        /// <summary>
        ///     Cancels recalculation of interested components for an entity.
        /// </summary>
        void RemoveEntity(IEntityObject entityObject);

        /// <summary>
        ///     Add an IComponentInterestOverridesUpdateReceiver which listens to update events.
        /// </summary>
        void AddUpdateReceiver(IComponentInterestOverridesUpdateReceiver updateReceiver);

        /// <summary>
        ///     Remove an IComponentInterestOverridesUpdateReceiver and stop it from getting notified about update events.
        /// </summary>
        void RemoveUpdateReceiver(IComponentInterestOverridesUpdateReceiver updateReceiver);
    }
}
