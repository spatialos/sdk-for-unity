// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using Improbable.Unity.CodeGeneration;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Manages components attached to an entity.
    /// </summary>
    public interface IEntityComponents
    {
        /// <summary>
        ///     Returns a set of interested component ids.
        /// </summary>
        IDictionary<uint, ISpatialOsComponentInternal> RegisteredComponents { get; }

        /// <summary>
        ///     Add an interested component.
        /// </summary>
        void RegisterInterestedComponent(uint componentId, ISpatialOsComponentInternal component);

        /// <summary>
        ///     Removes an interested component.
        /// </summary>
        void DeregisterInterestedComponent(uint componentId);

        /// <summary>
        ///     Add an IEntityInterestedComponentsInvalidator which listens to potential changes in interested components.
        /// </summary>
        void AddInvalidator(IEntityInterestedComponentsInvalidator invalidator);

        /// <summary>
        ///     Remove an IEntityInterestedComponentsInvalidator and stop it from getting notified about potential changes in
        ///     interested components.
        /// </summary>
        void RemoveInvalidator(IEntityInterestedComponentsInvalidator invalidator);
    }
}
