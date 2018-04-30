// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Worker;

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    ///     Unity companion component to SpatialOS component.
    /// </summary>
    public interface ISpatialOsComponent
    {
        /// <summary>
        ///     Gets the component ID of this component.
        /// </summary>
        uint ComponentId { get; }

        /// <summary>
        ///     Returns whether or not we have authority on this component.
        /// </summary>
        [System.Obsolete("Please use \"Authority == Improbable.Worker.Authority.Authoritative || Authority == Improbable.Worker.Authority.AuthorityLossImminent\".")]
        bool HasAuthority { get; }

        /// <summary>
        ///     Returns the state of authority of this component.
        /// </summary>
        Authority Authority { get; }

        /// <summary>
        ///     Returns the entity ID of this component.
        /// </summary>
        EntityId EntityId { get; }

        /// <summary>
        ///     Returns whether or not the component has received its first set of values and is listening for updates.
        /// </summary>
        bool IsComponentReady { get; }
    }
}
