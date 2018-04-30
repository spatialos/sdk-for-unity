// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Collections;
using Improbable.Entity.Component;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Improbable.Unity.Core
{
    class Commander : ICommandSender, IWorkerCommandSender, IDisposable
    {
        private struct EntityComponentId
        {
            private readonly EntityId entityId;
            private readonly uint componentId;

            public EntityComponentId(EntityId entityId, uint componentId)
            {
                this.entityId = entityId;
                this.componentId = componentId;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is EntityComponentId))
                {
                    return false;
                }

                var other = (EntityComponentId) obj;
                return other.entityId == entityId && other.componentId == componentId;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private readonly Dictionary<EntityComponentId, HashSet<uint>> componentToRequestIds;
        private readonly ComponentCommander componentCommander;
        private readonly ISpatialCommunicator communicator;

        private const bool PerformAuthorityCheck = true;
        private const bool SkipAuthorityCheck = false;

        public Commander(ComponentCommander componentCommander, ISpatialCommunicator communicator)
        {
            this.componentCommander = componentCommander;
            this.communicator = communicator;

            componentToRequestIds = new Dictionary<EntityComponentId, HashSet<uint>>();

            communicator.ComponentAuthorityChanged += OnComponentAuthorityChanged;
        }

        public void Dispose()
        {
            communicator.ComponentAuthorityChanged -= OnComponentAuthorityChanged;
        }

        private void OnComponentAuthorityChanged(EntityId entityId, IComponentMetaclass componentId, Authority authority, object componentObj)
        {
            if (authority == Authority.Authoritative)
            {
                return;
            }

            var component = new EntityComponentId(entityId, componentId.ComponentId);
            HashSet<uint> requestIds;
            if (!componentToRequestIds.TryGetValue(component, out requestIds))
            {
                return;
            }

            foreach (var requestId in requestIds)
            {
                componentCommander.ForgetRequestId(requestId);
            }

            componentToRequestIds.Remove(component);
        }

        private void SendGenericCommand<TResponse>(IComponentWriter writer, bool requireAuthority, CommandCallback<TResponse> callback,
                                                   Action sendAction)
        {
            var callbackWrapper = new CommandCallbackWrapper<TResponse>(callback);
            if (requireAuthority && (writer == null || communicator.GetAuthority(writer.EntityId, writer.ComponentId) == Authority.NotAuthoritative))
            {
                // This needs to be deferred, so that all callbacks are registered
                // before they are actually called.
                communicator.Defer(() => callbackWrapper.TriggerWithAuthorityError());
                return;
            }

            sendAction();
        }

        public void SendCommand<TCommand, TRequest, TResponse>(IComponentWriter writer,
                                                               ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor, TRequest request,
                                                               EntityId entityId, CommandCallback<TResponse> callback, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip)
            where TCommand : ICommandMetaclass, new()
        {
            SendCommandInternal(writer, PerformAuthorityCheck, commandDescriptor, request, entityId, callback, timeout, commandDelivery);
        }

        public void SendCommand<TCommand, TRequest, TResponse>(
            ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor, TRequest request,
            EntityId entityId, CommandCallback<TResponse> callback, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip)
            where TCommand : ICommandMetaclass, new()
        {
            SendCommandInternal(null, SkipAuthorityCheck, commandDescriptor, request, entityId, callback, timeout, commandDelivery);
        }

        private void SendCommandInternal<TCommand, TRequest, TResponse>(IComponentWriter writer, bool requireAuthority,
                                                                        ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor, TRequest request,
                                                                        EntityId entityId, CommandCallback<TResponse> callback, TimeSpan? timeout, CommandDelivery commandDelivery)
            where TCommand : ICommandMetaclass, new()
        {
            Action sendAction = () =>
            {
                var rawRequest = commandDescriptor.CreateRequest(request);
                Func<ICommandResponse<TCommand>, TResponse> extractResponse =
                    rawResponse => ExtractResponse(commandDescriptor, rawResponse);
                var requestId = componentCommander.SendCommandInternal(entityId, rawRequest, extractResponse, callback, timeout, commandDelivery);
                TrackRequest(writer, requestId);
            };
            SendGenericCommand(writer, requireAuthority, callback, sendAction);
        }

        public static TResponse ExtractResponse<TCommand, TRequest, TResponse>
            (ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor, ICommandResponse<TCommand> rawResponse)
            where TCommand : ICommandMetaclass, new()
        {
            return commandDescriptor.ExtractResponse(rawResponse);
        }

        public void ReserveEntityId(IComponentWriter writer, CommandCallback<ReserveEntityIdResult> callback,
                                    TimeSpan? timeout = null)
        {
            ReserveEntityIdInternal(writer, PerformAuthorityCheck, callback, timeout);
        }

        public void ReserveEntityId(CommandCallback<ReserveEntityIdResult> callback, TimeSpan? timeout = null)
        {
            ReserveEntityIdInternal(null, SkipAuthorityCheck, callback, timeout);
        }

        private void ReserveEntityIdInternal(IComponentWriter writer, bool requireAuthority, CommandCallback<ReserveEntityIdResult> callback,
                                             TimeSpan? timeout = null)
        {
            Action sendAction = () =>
            {
                var requestId = componentCommander.ReserveEntityIdInternal(callback, timeout);
                TrackRequest(writer, requestId);
            };
            SendGenericCommand(writer, requireAuthority, callback, sendAction);
        }

        public void ReserveEntityIds(IComponentWriter writer, CommandCallback<ReserveEntityIdsResult> callback,
                                     uint numberOfEntityIds, TimeSpan? timeout = null)
        {
            ReserveEntityIdsInternal(writer, PerformAuthorityCheck, callback, numberOfEntityIds, timeout);
        }

        public void ReserveEntityIds(CommandCallback<ReserveEntityIdsResult> callback, uint numberOfEntityIds, TimeSpan? timeout = null)
        {
            ReserveEntityIdsInternal(null, SkipAuthorityCheck, callback, numberOfEntityIds, timeout);
        }

        private void ReserveEntityIdsInternal(IComponentWriter writer, bool requireAuthority, CommandCallback<ReserveEntityIdsResult> callback,
                                              uint numberOfEntityIds, TimeSpan? timeout = null)
        {
            Action sendAction = () =>
            {
                var requestId = componentCommander.ReserveEntityIdsInternal(callback, numberOfEntityIds, timeout);
                TrackRequest(writer, requestId);
            };
            SendGenericCommand(writer, requireAuthority, callback, sendAction);
        }

        public void CreateEntity(IComponentWriter writer, EntityId reservedEntityId, Worker.Entity template,
                                 CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            CreateEntityInternal(writer, PerformAuthorityCheck, reservedEntityId, template, callback, timeout);
        }

        public void CreateEntity(EntityId reservedEntityId, Worker.Entity template,
                                 CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            CreateEntityInternal(null, SkipAuthorityCheck, reservedEntityId, template, callback, timeout);
        }

        public void CreateEntity(IComponentWriter writer, Worker.Entity template,
                                 CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            CreateEntityInternal(writer, PerformAuthorityCheck, template, callback, timeout);
        }

        public void CreateEntity(Worker.Entity template,
                                 CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            CreateEntityInternal(null, SkipAuthorityCheck, template, callback, timeout);
        }

        private void CreateEntityInternal(IComponentWriter writer, bool requireAuthority, EntityId reservedEntityId, Worker.Entity template,
                                          CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            Action sendAction = () =>
            {
                var requestId = componentCommander.CreateEntityInternal(reservedEntityId, template, callback, timeout);
                TrackRequest(writer, requestId);
            };
            SendGenericCommand(writer, requireAuthority, callback, sendAction);
        }

        private void CreateEntityInternal(IComponentWriter writer, bool requireAuthority, Worker.Entity template,
                                          CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            Action sendAction = () =>
            {
                var requestId = componentCommander.CreateEntityInternal(template, callback, timeout);
                TrackRequest(writer, requestId);
            };
            SendGenericCommand(writer, requireAuthority, callback, sendAction);
        }

        public void DeleteEntity(IComponentWriter writer, EntityId entityId,
                                 CommandCallback<DeleteEntityResult> callback, TimeSpan? timeout = null)
        {
            DeleteEntityInternal(writer, PerformAuthorityCheck, entityId, callback, timeout);
        }

        public void DeleteEntity(EntityId entityId,
                                 CommandCallback<DeleteEntityResult> callback, TimeSpan? timeout = null)
        {
            DeleteEntityInternal(null, SkipAuthorityCheck, entityId, callback, timeout);
        }


        private void DeleteEntityInternal(IComponentWriter writer, bool requireAuthority, EntityId entityId,
                                          CommandCallback<DeleteEntityResult> callback, TimeSpan? timeout = null)
        {
            Action sendAction = () =>
            {
                var requestId = componentCommander.DeleteEntityInternal(entityId, callback, timeout);
                TrackRequest(writer, requestId);
            };
            SendGenericCommand(writer, requireAuthority, callback, sendAction);
        }

        public void SendQuery(IComponentWriter writer, EntityQuery query,
                              CommandCallback<EntityQueryResult> callback, TimeSpan? timeout = null)
        {
            SendQueryInternal(writer, PerformAuthorityCheck, query, callback, timeout);
        }

        public void SendQuery(EntityQuery query,
                              CommandCallback<EntityQueryResult> callback, TimeSpan? timeout = null)
        {
            SendQueryInternal(null, SkipAuthorityCheck, query, callback, timeout);
        }

        #region ICommandResponseHandler boilerplate

        /// <inheritdoc />
        public ICommandResponseHandler<TResponse> SendCommand<TCommand, TRequest, TResponse>(ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor,
                                                                                             TRequest request, EntityId entityId, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip) where TCommand : ICommandMetaclass, new()
        {
            return CommandResponseHandler<TResponse>.Wrap(callback => SendCommandInternal(null, SkipAuthorityCheck, commandDescriptor, request, entityId, callback, timeout, commandDelivery));
        }

        /// <inheritdoc />
        public ICommandResponseHandler<TResponse> SendCommand<TCommand, TRequest, TResponse>(IComponentWriter writer, ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor,
                                                                                             TRequest request, EntityId entityId, TimeSpan? timeout, CommandDelivery commandDelivery = CommandDelivery.RoundTrip) where TCommand : ICommandMetaclass, new()
        {
            return CommandResponseHandler<TResponse>.Wrap(callback => SendCommandInternal(writer, PerformAuthorityCheck, commandDescriptor, request, entityId, callback, timeout, commandDelivery));
        }

        public ICommandResponseHandler<ReserveEntityIdResult> ReserveEntityId(TimeSpan? timeout = null)
        {
            return CommandResponseHandler<ReserveEntityIdResult>.Wrap(callback => ReserveEntityId(callback, timeout));
        }

        public ICommandResponseHandler<ReserveEntityIdsResult> ReserveEntityIds(uint numberOfEntityIds, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<ReserveEntityIdsResult>.Wrap(callback => ReserveEntityIds(callback, numberOfEntityIds, timeout));
        }

        public ICommandResponseHandler<CreateEntityResult> CreateEntity(EntityId reservedEntityId, Worker.Entity template,
                                                                        TimeSpan? timeout = null)
        {
            return CommandResponseHandler<CreateEntityResult>.Wrap(callback => CreateEntity(reservedEntityId, template, callback, timeout));
        }

        public ICommandResponseHandler<CreateEntityResult> CreateEntity(Worker.Entity template, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<CreateEntityResult>.Wrap(callback => CreateEntity(template, callback, timeout));
        }

        public ICommandResponseHandler<EntityQueryResult> SendQuery(EntityQuery query, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<EntityQueryResult>.Wrap(callback => SendQuery(query, callback, timeout));
        }

        public ICommandResponseHandler<DeleteEntityResult> DeleteEntity(EntityId entityId, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<DeleteEntityResult>.Wrap(callback => DeleteEntity(entityId, callback, timeout));
        }

        public ICommandResponseHandler<ReserveEntityIdResult> ReserveEntityId(IComponentWriter writer, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<ReserveEntityIdResult>.Wrap(callback => ReserveEntityId(writer, callback, timeout));
        }

        public ICommandResponseHandler<ReserveEntityIdsResult> ReserveEntityIds(IComponentWriter writer, uint numberOfEntityIds, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<ReserveEntityIdsResult>.Wrap(callback => ReserveEntityIds(writer, callback, numberOfEntityIds, timeout));
        }

        public ICommandResponseHandler<CreateEntityResult> CreateEntity(IComponentWriter writer, EntityId reservedEntityId,
                                                                        Worker.Entity template, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<CreateEntityResult>.Wrap(callback => CreateEntity(writer, reservedEntityId, template, callback, timeout));
        }

        public ICommandResponseHandler<CreateEntityResult> CreateEntity(IComponentWriter writer, Worker.Entity template,
                                                                        TimeSpan? timeout = null)
        {
            return CommandResponseHandler<CreateEntityResult>.Wrap(callback => CreateEntity(writer, template, callback, timeout));
        }

        public ICommandResponseHandler<DeleteEntityResult> DeleteEntity(IComponentWriter writer, EntityId entityId, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<DeleteEntityResult>.Wrap(callback => DeleteEntity(writer, entityId, callback, timeout));
        }

        public ICommandResponseHandler<EntityQueryResult> SendQuery(IComponentWriter writer, EntityQuery query, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<EntityQueryResult>.Wrap(callback => SendQuery(writer, query, callback, timeout));
        }

        #endregion

        private void SendQueryInternal(IComponentWriter writer, bool requireAuthority, EntityQuery query,
                                       CommandCallback<EntityQueryResult> callback, TimeSpan? timeout = null)
        {
            Action sendAction = () =>
            {
                var requestId = componentCommander.SendQueryInternal(query, callback, timeout);
                TrackRequest(writer, requestId);
            };
            SendGenericCommand(writer, requireAuthority, callback, sendAction);
        }

        private void TrackRequest(IComponentWriter writer, Option<uint> requestId)
        {
            if (writer == null || !requestId.HasValue)
            {
                return;
            }

            var component = new EntityComponentId(writer.EntityId, writer.ComponentId);
            HashSet<uint> requestIds;
            if (!componentToRequestIds.TryGetValue(component, out requestIds))
            {
                requestIds = new HashSet<uint> { requestId.Value };
                componentToRequestIds.Add(component, requestIds);
            }
            else
            {
                requestIds.Add(requestId.Value);
            }
        }
    }
}
