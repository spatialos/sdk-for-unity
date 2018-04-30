// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Worker;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Exposes events to listen for incoming SpatialOS operations such as authority changes.
    /// </summary>
    public interface IDispatchEventHandler
    {
        /// <summary>
        ///     Subscribed callbacks will be invoked whenever authority changes for any entity/component pair checked out on this
        ///     worker.
        /// </summary>
        [Obsolete("Obsolete as of 10.4.0. Please use Improbable.Unity.Core.AuthorityChangedNotifier and Improbable.Unity.Core.EntityPipeline. (see docs)")]
        event OnAuthorityChangedCallback OnAuthorityChanged;
    }

    public delegate void OnAuthorityChangedCallback(EntityId entityId, IComponentMetaclass componentId, Authority authority, object component);
}
