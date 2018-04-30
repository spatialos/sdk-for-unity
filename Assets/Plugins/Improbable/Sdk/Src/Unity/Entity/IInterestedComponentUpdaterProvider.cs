// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Interface for classes able to provide IEntityComponentInterestOverridesUpdater objects.
    /// </summary>
    public interface IInterestedComponentUpdaterProvider
    {
        /// <summary>
        ///     Return an IEntityComponentInterestOverridesUpdater.
        /// </summary>
        IEntityComponentInterestOverridesUpdater GetEntityInterestedComponentsUpdater();
    }
}
