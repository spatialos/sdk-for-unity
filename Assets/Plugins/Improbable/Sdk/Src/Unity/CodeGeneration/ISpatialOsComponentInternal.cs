// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Core;
using Improbable.Unity.Entity;
using Improbable.Worker;

namespace Improbable.Unity.CodeGeneration
{
    public interface ISpatialOsComponentInternal : ISpatialOsComponent, IPipelineEntityComponentOpsReceiver
    {
        /// <summary>
        ///     Initializes the SpatialOsComponent
        /// </summary>
        bool Init(ISpatialCommunicator communicator, IEntityObject entityObject);

        /// <summary>
        ///     Invoked when authority changes for this component.
        /// </summary>
        event OnAuthorityChangeCallback OnAuthorityChange;
    }

    /// <summary>
    ///     The type of callback to listen for authority changes.
    /// </summary>
    public delegate void OnAuthorityChangeCallback(Authority newAuthority);

    /// <summary>
    ///     The type of callback to listen for component ready changes.
    /// </summary>
    public delegate void OnComponentReadyCallback();
}
