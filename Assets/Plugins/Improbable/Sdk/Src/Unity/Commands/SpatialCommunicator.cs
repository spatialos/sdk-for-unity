// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Collections;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Implementation of <see cref="ISpatialCommunicator" />
    /// </summary>
    class SpatialCommunicator : ISpatialCommunicator
    {
        private Connection connection;
        private Dispatcher dispatcher;
        private IDeferredActionDispatcher deferredActionDispatcher;

        /// <summary>
        ///     Creates a new Communicator with the given connection and dispatcher.
        /// </summary>
        public SpatialCommunicator(Connection connection, Dispatcher dispatcher, IDeferredActionDispatcher deferredActionDispatcher)
        {
            this.connection = connection;
            this.dispatcher = dispatcher;
            this.deferredActionDispatcher = deferredActionDispatcher;
        }

        /// <inheritdoc />
        public void SendComponentUpdate<TComponent>(EntityId entityId, IComponentUpdate<TComponent> update, bool legacyCallbackSemantics = false) where TComponent : IComponentMetaclass
        {
            connection.SendComponentUpdate(entityId, update, legacyCallbackSemantics);
        }

        /// <inheritdoc />
        public void SendCommandResponse<TComponent>(RequestId<IncomingCommandRequest<TComponent>> requestId, ICommandResponse<TComponent> response) where TComponent : ICommandMetaclass, new()
        {
            connection.SendCommandResponse(requestId, response);
        }

        public void SendAuthorityLossImminentAcknowledgement<C>(EntityId entityId) where C : IComponentMetaclass
        {
            connection.SendAuthorityLossImminentAcknowledgement<C>(entityId);
        }

        /// <inheritdoc />
        public RequestId<OutgoingCommandRequest<TCommand>> SendCommandRequest<TCommand>(EntityId entityId, ICommandRequest<TCommand> request, Option<uint> timeout, CommandDelivery commandDelivery) where TCommand : ICommandMetaclass, new()
        {
            return connection.SendCommandRequest(entityId, request, timeout, new CommandParameters { AllowShortCircuiting = commandDelivery == CommandDelivery.ShortCircuit });
        }

        /// <inheritdoc />
        public RequestId<ReserveEntityIdRequest> SendReserveEntityIdRequest(Option<uint> timeout)
        {
#pragma warning disable 618
            return connection.SendReserveEntityIdRequest(timeout);
#pragma warning restore 618
        }

        /// <inheritdoc />
        public RequestId<ReserveEntityIdsRequest> SendReserveEntityIdsRequest(uint numberOfEntityIds, Option<uint> timeout)
        {
            return connection.SendReserveEntityIdsRequest(numberOfEntityIds, timeout);
        }

        /// <inheritdoc />
        public RequestId<CreateEntityRequest> SendCreateEntityRequest(Worker.Entity template, Option<EntityId> entityId, Option<uint> timeout)
        {
            return connection.SendCreateEntityRequest(template, entityId, timeout);
        }

        /// <inheritdoc />
        public RequestId<DeleteEntityRequest> SendDeleteEntityRequest(EntityId entityId, Option<uint> timeout)
        {
            return connection.SendDeleteEntityRequest(entityId, timeout);
        }

        /// <inheritdoc />
        public RequestId<EntityQueryRequest> SendEntityQueryRequest(EntityQuery query, Option<uint> timeout)
        {
            return connection.SendEntityQueryRequest(query, timeout);
        }

        /// <inheritdoc />
        public void Defer(Action action)
        {
            deferredActionDispatcher.DeferAction(action);
        }

        /// <inheritdoc />
        public ulong RegisterAddComponent<TComponent>(Action<AddComponentOp<TComponent>> callback) where TComponent : IComponentMetaclass
        {
            return dispatcher.OnAddComponent(callback);
        }

        /// <inheritdoc />
        public ulong RegisterRemoveComponent<TComponent>(Action<RemoveComponentOp> callback) where TComponent : IComponentMetaclass
        {
            return dispatcher.OnRemoveComponent<TComponent>(callback);
        }

        /// <inheritdoc />
        public ulong RegisterComponentUpdate<TComponent>(Action<ComponentUpdateOp<TComponent>> callback) where TComponent : IComponentMetaclass
        {
            return dispatcher.OnComponentUpdate(callback);
        }

        /// <inheritdoc />
        public ulong RegisterCommandRequest<TCommand>(Action<CommandRequestOp<TCommand>> callback) where TCommand : ICommandMetaclass, new()
        {
            return dispatcher.OnCommandRequest(callback);
        }

        /// <inheritdoc />
        public ulong RegisterAuthorityChange<TComponent>(Action<AuthorityChangeOp> callback) where TComponent : IComponentMetaclass
        {
            return dispatcher.OnAuthorityChange<TComponent>(callback);
        }

        /// <inheritdoc />
        public void RemoveDispatcherCallback(ulong callbackKey)
        {
            dispatcher.Remove(callbackKey);
        }

        /// <inheritdoc />
        public ulong RegisterCommandResponse<TCommand>(Action<CommandResponseOp<TCommand>> response) where TCommand : ICommandMetaclass, new()
        {
            return dispatcher.OnCommandResponse(response);
        }

        /// <inheritdoc />
        public ulong RegisterReserveEntityIdResponse(Action<ReserveEntityIdResponseOp> callback)
        {
#pragma warning disable 618
            return dispatcher.OnReserveEntityIdResponse(callback);
#pragma warning restore 618
        }

        /// <inheritdoc />
        public ulong RegisterReserveEntityIdsResponse(Action<ReserveEntityIdsResponseOp> callback)
        {
            return dispatcher.OnReserveEntityIdsResponse(callback);
        }

        /// <inheritdoc />
        public ulong RegisterCreateEntityResponse(Action<CreateEntityResponseOp> callback)
        {
            return dispatcher.OnCreateEntityResponse(callback);
        }

        /// <inheritdoc />
        public ulong RegisterDeleteEntityResponse(Action<DeleteEntityResponseOp> callback)
        {
            return dispatcher.OnDeleteEntityResponse(callback);
        }

        /// <inheritdoc />
        public ulong RegisterEntityQueryResponse(Action<EntityQueryResponseOp> callback)
        {
            return dispatcher.OnEntityQueryResponse(callback);
        }

        /// <inheritdoc />
        public void Remove(ulong callbackKey)
        {
            dispatcher.Remove(callbackKey);
        }

        /// <inheritdoc />
        public void RegisterComponentInterest(EntityId entityId, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides)
        {
            connection.SendComponentInterest(entityId, interestOverrides);
        }

        /// <inheritdoc />
        public Authority GetAuthority(EntityId writerEntityId, uint writerComponentId)
        {
            Map<uint, Authority> authorityForComponentsOfEntity;

            if (!SpatialOS.Dispatcher.Authority.TryGetValue(writerEntityId, out authorityForComponentsOfEntity))
            {
                throw new InvalidOperationException(
                                                    string.Format("Authority information for writer entity {0} could not be accessed.", writerEntityId));
            }

            Authority authorityForComponent;

            if (!authorityForComponentsOfEntity.TryGetValue(writerComponentId, out authorityForComponent))
            {
                // It is possible that this worker cannot even see the component.
                // That means it certainly is not authoritative over it.
                authorityForComponent = Authority.NotAuthoritative;
            }

            return authorityForComponent;
        }

        public void TriggerComponentAuthorityChanged(EntityId entityId, IComponentMetaclass componentId,
                                                     Authority authority, object component)
        {
            if (ComponentAuthorityChanged != null)
            {
                ComponentAuthorityChanged(entityId, componentId, authority, component);
            }
        }

        /// <inheritdoc />
        public event OnAuthorityChangedCallback ComponentAuthorityChanged;

        public void AttachConnection(Connection connectionToUse)
        {
            this.connection = connectionToUse;
        }

        public ulong RegisterCriticalSection(Action<CriticalSectionOp> criticalSection)
        {
            return dispatcher.OnCriticalSection(criticalSection);
        }

        public ulong RegisterAddEntity(Action<AddEntityOp> addEntity)
        {
            return dispatcher.OnAddEntity(addEntity);
        }

        public ulong RegisterRemoveEntity(Action<RemoveEntityOp> removeEntity)
        {
            return dispatcher.OnRemoveEntity(removeEntity);
        }
    }
}
