// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using Improbable.Worker;

namespace Improbable.Unity.Entity
{
    // #SNIPPET_START interface
    /// <summary>
    ///     Provides the ability to override calculated component interests for the current worker.
    ///     These interests are calculated for a GameObject associated with a SpatialOS Entity based on:
    ///     a) The presence of the [Require] attribute on a field referencing a SpatialOS component on a MonoBehaviour.
    ///     b) The presence of a generated SpatialOS MonoBehaviour component.
    /// </summary>
    public interface IWorkerComponentInterestOverrider
    {
        /// <summary>
        ///     Sets an interest override for a component on ALL entities on the current worker.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="interest">The interest override to apply.</param>
        void SetComponentInterest<T>(WorkerComponentInterest interest) where T : IComponentMetaclass;

        /// <summary>
        ///     Sets an interest override for a component on a specific entity on the current worker.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="entityId">The entity that the override applies to.</param>
        /// <param name="interest">The interest override to apply.</param>
        void SetComponentInterest<T>(EntityId entityId, WorkerComponentInterest interest) where T : IComponentMetaclass;

        /// <summary>
        ///     Returns the set of component interest overrides active on the current worker for a specific entity, including any
        ///     global
        ///     overrides that have been set. Global overrides are themselves overridden by entity-specific overrides.
        /// </summary>
        /// <param name="entityId">The entity to retrieve.</param>
        /// <returns>A mapping of componentId to its override status.</returns>
        IEnumerator<KeyValuePair<uint, WorkerComponentInterest>> GetComponentInterest(EntityId entityId);
    }

    // #SNIPPET_END interface
}
