// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Worker;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Can be registered to an IEntityInterestedComponentsReporter to listen to updates to component interest overrides.
    /// </summary>
    public interface IComponentInterestOverridesUpdateReceiver
    {
        /// <summary>
        ///     Callback to be invoked when an entity's component interest overrides have changed.
        /// </summary>
        void OnComponentInterestOverridesUpdated(EntityId entity, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides);
    }
}
