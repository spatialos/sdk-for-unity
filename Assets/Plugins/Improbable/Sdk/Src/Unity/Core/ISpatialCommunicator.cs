// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Collections;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Provides an interface between Unity classes such as the Commander and the generated component MonoBehaviours
    ///     and the classes used to communicate with SpatialOS: the Connection and the Dispatcher.
    /// </summary>
    public interface ISpatialCommunicator
    {
        /// <summary>
        ///     Send a component update.
        /// </summary>
        void SendComponentUpdate<C>(EntityId entityId, IComponentUpdate<C> update, bool legacyCallbackSemantics = false) where C : IComponentMetaclass;

        /// <summary>
        ///     Send a command response.
        /// </summary>
        void SendCommandResponse<C>(RequestId<IncomingCommandRequest<C>> requestId, ICommandResponse<C> response) where C : ICommandMetaclass, new();

        /// <summary>
        ///     Register what components a given entity is interested in.
        /// </summary>
        void SendAuthorityLossImminentAcknowledgement<C>(EntityId entityId) where C : IComponentMetaclass;

        /// <summary>
        ///     Send a command request.
        /// </summary>
        RequestId<OutgoingCommandRequest<TCommand>> SendCommandRequest<TCommand>(EntityId entityId, ICommandRequest<TCommand> request, Option<uint> timeout, CommandDelivery commandDelivery) where TCommand : ICommandMetaclass, new();

        /// <summary>
        ///     Send a reserve entity ID request.
        /// </summary>
        RequestId<ReserveEntityIdRequest> SendReserveEntityIdRequest(Option<uint> timeout);

        /// <summary>
        ///     Send a reserve entity IDs request.
        /// </summary>
        RequestId<ReserveEntityIdsRequest> SendReserveEntityIdsRequest(uint numberOfEntityIds, Option<uint> timeout);

        /// <summary>
        ///     Send a create entity request.
        /// </summary>
        RequestId<CreateEntityRequest> SendCreateEntityRequest(Worker.Entity template, Option<EntityId> entityId, Option<uint> timeout);

        /// <summary>
        ///     Send a delete entity request.
        /// </summary>
        RequestId<DeleteEntityRequest> SendDeleteEntityRequest(EntityId entityId, Option<uint> timeout);

        /// <summary>
        ///     Send an entity query request.
        /// </summary>
        RequestId<EntityQueryRequest> SendEntityQueryRequest(EntityQuery query, Option<uint> timeout);

        /// <summary>
        ///     Defer an action until next time ProcessEvents() is called.
        /// </summary>
        void Defer(Action action);

        /// <summary>
        ///     Register a callback to be invoked when a component is added.
        /// </summary>
        ulong RegisterAddComponent<C>(Action<AddComponentOp<C>> callback) where C : IComponentMetaclass;

        /// <summary>
        ///     Register a callback to be invoked when a component is removed.
        /// </summary>
        ulong RegisterRemoveComponent<C>(Action<RemoveComponentOp> callback) where C : IComponentMetaclass;

        /// <summary>
        ///     Register a callback to be invoked when a component is updated.
        /// </summary>
        ulong RegisterComponentUpdate<C>(Action<ComponentUpdateOp<C>> callback) where C : IComponentMetaclass;

        /// <summary>
        ///     Register a callback to be invoked when a command request is received.
        /// </summary>
        ulong RegisterCommandRequest<C>(Action<CommandRequestOp<C>> callback) where C : ICommandMetaclass, new();

        /// <summary>
        ///     Register a callback to be invoked when authority for a component changes.
        /// </summary>
        ulong RegisterAuthorityChange<C>(Action<AuthorityChangeOp> callback) where C : IComponentMetaclass;

        /// <summary>
        ///     Register a callback to be invoked whne a command response is received.
        /// </summary>
        ulong RegisterCommandResponse<TCommand>(Action<CommandResponseOp<TCommand>> response) where TCommand : ICommandMetaclass, new();

        /// <summary>
        ///     Register a callback to be invoked when a reserve entity ID response is received.
        /// </summary>
        ulong RegisterReserveEntityIdResponse(Action<ReserveEntityIdResponseOp> callback);

        /// <summary>
        ///     Register a callback to be invoked when a reserve entity IDs response is received.
        /// </summary>
        ulong RegisterReserveEntityIdsResponse(Action<ReserveEntityIdsResponseOp> callback);

        /// <summary>
        ///     Register a callback to be invoked when a create entity response is received.
        /// </summary>
        ulong RegisterCreateEntityResponse(Action<CreateEntityResponseOp> callback);

        /// <summary>
        ///     Register a callback to be invoked when a delete entity response is received.
        /// </summary>
        ulong RegisterDeleteEntityResponse(Action<DeleteEntityResponseOp> callback);

        /// <summary>
        ///     Register a callback to be invoked when an entity query response is received.
        /// </summary>
        ulong RegisterEntityQueryResponse(Action<EntityQueryResponseOp> createOnResponseThunk);

        /// <summary>
        ///     Deregister a previously registered callback with the given callback key.
        /// </summary>
        void Remove(ulong callbackKey);

        /// <summary>
        ///     Register what components a given entity is interested in.
        /// </summary>
        void RegisterComponentInterest(EntityId entityId, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides);

        /// <summary>
        ///     Called when authority over a component changes.
        /// </summary>
        event OnAuthorityChangedCallback ComponentAuthorityChanged;

        /// <summary>
        ///     Return the authority state of the worker over a given entity.
        /// </summary>
        Authority GetAuthority(EntityId writerEntityId, uint writerComponentId);

        #region Entity creation pipeline

        /// <summary>
        ///     Registers a callback for <see cref="CriticalSectionOp" />.
        /// </summary>
        ulong RegisterCriticalSection(Action<CriticalSectionOp> criticalSection);

        /// <summary>
        ///     Registers a callback for <see cref="AddEntityOp" />.
        /// </summary>
        ulong RegisterAddEntity(Action<AddEntityOp> addEntity);

        /// <summary>
        ///     Registers a callback for <see cref="RemoveEntityOp" />.
        /// </summary>
        ulong RegisterRemoveEntity(Action<RemoveEntityOp> removeEntity);

        #endregion
    }
}
