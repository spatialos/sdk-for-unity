// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Can be registered to an IEntityComponents or IEntityVisualizers to listen to potential changes in interested
    ///     components.
    /// </summary>
    public interface IEntityInterestedComponentsInvalidator
    {
        /// <summary>
        ///     Callback to be invoked when an entity's interested components have potentially changed.
        /// </summary>
        void OnInterestedComponentsPotentiallyChanged();
    }
}
