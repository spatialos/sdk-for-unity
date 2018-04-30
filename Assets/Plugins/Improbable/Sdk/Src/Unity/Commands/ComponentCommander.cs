// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Collections;
using Improbable.Unity.CodeGeneration;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Worker;
using Improbable.Worker.Query;
using UnityEngine;

namespace Improbable.Unity.Core
{
    class ComponentCommander : IComponentCommander
    {
        private const uint DefaultCommandTimeoutMs = 1000;

        private readonly HashSet<Type> commandResponseThunksRegistered;
        private readonly Dictionary<uint, ICommandCallbackWrapper> requestIdToCallback;

        private readonly ISpatialOsComponentInternal component;
        private readonly Collections.List<ulong> dispatcherCallbackKeys;

        private readonly ISpatialCommunicator communicator;

        public ComponentCommander(ISpatialOsComponentInternal component, ISpatialCommunicator communicator)
        {
            commandResponseThunksRegistered = new HashSet<Type>();
            requestIdToCallback = new Dictionary<uint, ICommandCallbackWrapper>();

            this.component = component;
            this.communicator = communicator;

            dispatcherCallbackKeys = new Collections.List<ulong>();
            dispatcherCallbackKeys.Add(
                                       communicator.RegisterReserveEntityIdResponse(CreateOnResponseThunk<ReserveEntityIdResponseOp, ReserveEntityIdResult>(
                                                                                                                                                            op => op.RequestId.Id, op => op.StatusCode, op => op.Message, op => op.EntityId.HasValue ? new ReserveEntityIdResult(op.EntityId.Value) : new Option<ReserveEntityIdResult>())));
            dispatcherCallbackKeys.Add(
                                       communicator.RegisterCreateEntityResponse(CreateOnResponseThunk<CreateEntityResponseOp, CreateEntityResult>(
                                                                                                                                                   op => op.RequestId.Id, op => op.StatusCode, op => op.Message, op => op.EntityId.HasValue ? new CreateEntityResult(op.EntityId.Value) : new Option<CreateEntityResult>())));
            dispatcherCallbackKeys.Add(
                                       communicator.RegisterReserveEntityIdsResponse(CreateOnResponseThunk<ReserveEntityIdsResponseOp, ReserveEntityIdsResult>(
                                                                                                                                                               op => op.RequestId.Id, op => op.StatusCode, op => op.Message, op => new ReserveEntityIdsResult(op.FirstEntityId.Value, op.NumberOfEntityIds))));
            dispatcherCallbackKeys.Add(
                                       communicator.RegisterDeleteEntityResponse(CreateOnResponseThunk<DeleteEntityResponseOp, DeleteEntityResult>(
                                                                                                                                                   op => op.RequestId.Id, op => op.StatusCode, op => op.Message, op => new DeleteEntityResult(op.EntityId))));
            dispatcherCallbackKeys.Add(
                                       communicator.RegisterEntityQueryResponse(CreateOnResponseThunk<EntityQueryResponseOp, EntityQueryResult>(
                                                                                                                                                op => op.RequestId.Id, op => op.StatusCode, op => op.Message, op => new EntityQueryResult(op.ResultCount, op.Result))));

            if (component != null)
            {
                component.OnAuthorityChange += OnComponentAuthorityChange;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (component != null)
            {
                component.OnAuthorityChange -= OnComponentAuthorityChange;
            }

            for (var i = 0; i < dispatcherCallbackKeys.Count; i++)
            {
                communicator.Remove(dispatcherCallbackKeys[i]);
            }
        }

        private void OnComponentAuthorityChange(Authority authority)
        {
            if (component.Authority != Authority.Authoritative && component.Authority != Authority.AuthorityLossImminent)
            {
                requestIdToCallback.Clear();
            }
        }

        private Option<uint> SendGenericCommand<TResponse>(CommandCallback<TResponse> callback,
                                                           Func<uint, uint> sendRequestWithTimeoutMs, TimeSpan? timeout = null)
        {
            var callbackWrapper = new CommandCallbackWrapper<TResponse>(callback);
            if (component != null && component.Authority != Authority.Authoritative && component.Authority != Authority.AuthorityLossImminent)
            {
                // This needs to be deferred, so that all callbacks are registered
                // before they are actually called.
                communicator.Defer(() => callbackWrapper.TriggerWithError(StatusCode.AuthorityLost, string.Format(
                                                                                                                  "Tried to send a command from (entity ID: {0}, component: {1}) without " +
                                                                                                                  "authority on that pair.",
                                                                                                                  component.EntityId,
                                                                                                                  component.GetType()
                                                                                                                 )));

                return new Option<uint>();
            }

            var timeoutMs = timeout.HasValue ? (uint) timeout.Value.Milliseconds : DefaultCommandTimeoutMs;
            var requestId = sendRequestWithTimeoutMs(timeoutMs);
            requestIdToCallback.Add(requestId, callbackWrapper);
            return requestId;
        }

        /// <inheritdoc />
        public void ReserveEntityId(CommandCallback<ReserveEntityIdResult> callback, TimeSpan? timeout = null)
        {
            ReserveEntityIdInternal(callback, timeout);
        }

        /// <inheritdoc />
        public void ReserveEntityIds(CommandCallback<ReserveEntityIdsResult> callback, uint numberOfEntityIds, TimeSpan? timeout = null)
        {
            ReserveEntityIdsInternal(callback, numberOfEntityIds, timeout);
        }

        public Option<uint> ReserveEntityIdInternal(CommandCallback<ReserveEntityIdResult> callback, TimeSpan? timeout = null)
        {
            Func<uint, uint> sendRequestWithTimeoutMs =
                timeoutMs => communicator.SendReserveEntityIdRequest(timeoutMs).Id;
            return SendGenericCommand(callback, sendRequestWithTimeoutMs, timeout);
        }

        public Option<uint> ReserveEntityIdsInternal(CommandCallback<ReserveEntityIdsResult> callback, uint numberOfEntityIds, TimeSpan? timeout = null)
        {
            Func<uint, uint> sendRequestWithTimeoutMs =
                timeoutMs => communicator.SendReserveEntityIdsRequest(numberOfEntityIds, timeoutMs).Id;
            return SendGenericCommand(callback, sendRequestWithTimeoutMs, timeout);
        }

        /// <inheritdoc />
        public void CreateEntity(EntityId reservedEntityId, Worker.Entity template,
                                 CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            CreateEntityInternal(reservedEntityId, template, callback, timeout);
        }

        internal Option<uint> CreateEntityInternal(EntityId reservedEntityId, Worker.Entity template,
                                                   CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            Func<uint, uint> sendRequestWithTimeoutMs =
                timeoutMs => communicator.SendCreateEntityRequest(template, reservedEntityId, timeoutMs).Id;
            return SendGenericCommand(callback, sendRequestWithTimeoutMs, timeout);
        }

        /// <inheritdoc />
        public void CreateEntity(Worker.Entity template, CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            CreateEntityInternal(template, callback, timeout);
        }

        internal Option<uint> CreateEntityInternal(Worker.Entity template,
                                                   CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null)
        {
            Func<uint, uint> sendRequestWithTimeoutMs =
                timeoutMs => communicator.SendCreateEntityRequest(template, null, timeoutMs).Id;
            return SendGenericCommand(callback, sendRequestWithTimeoutMs, timeout);
        }

        /// <inheritdoc />
        public void DeleteEntity(EntityId entityId, CommandCallback<DeleteEntityResult> callback, TimeSpan? timeout = null)
        {
            DeleteEntityInternal(entityId, callback, timeout);
        }

        internal Option<uint> DeleteEntityInternal(EntityId entityId, CommandCallback<DeleteEntityResult> callback, TimeSpan? timeout = null)
        {
            Func<uint, uint> sendRequestWithTimeoutMs =
                timeoutMs => communicator.SendDeleteEntityRequest(entityId, timeoutMs).Id;
            return SendGenericCommand(callback, sendRequestWithTimeoutMs, timeout);
        }

        /// <inheritdoc />
        public void SendQuery(EntityQuery query, CommandCallback<EntityQueryResult> callback, TimeSpan? timeout = null)
        {
            SendQueryInternal(query, callback, timeout);
        }

        internal Option<uint> SendQueryInternal(EntityQuery query, CommandCallback<EntityQueryResult> callback, TimeSpan? timeout = null)
        {
            Func<uint, uint> sendRequestWithTimeoutMs =
                timeoutMs => communicator.SendEntityQueryRequest(query, timeoutMs).Id;
            return SendGenericCommand(callback, sendRequestWithTimeoutMs, timeout);
        }

        /// <inheritdoc />
        public void SendCommand<TCommand, TResponse>(EntityId entityId, ICommandRequest<TCommand> rawRequest, Func<ICommandResponse<TCommand>, TResponse> extractResponseFunc,
                                                     CommandCallback<TResponse> callback, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip) where TCommand : ICommandMetaclass, new()
        {
            SendCommandInternal(entityId, rawRequest, extractResponseFunc, callback, timeout, commandDelivery);
        }

        public Option<uint> SendCommandInternal<TCommand, TResponse>(EntityId entityId, ICommandRequest<TCommand> rawRequest,
                                                                     Func<ICommandResponse<TCommand>, TResponse> extractResponseFunc, CommandCallback<TResponse> callback,
                                                                     TimeSpan? timeout, CommandDelivery commandDelivery) where TCommand : ICommandMetaclass, new()
        {
            if (!commandResponseThunksRegistered.Contains(typeof(TCommand)))
            {
                var callbackKey = RegisterCommandResponse(extractResponseFunc);
                dispatcherCallbackKeys.Add(callbackKey);
                commandResponseThunksRegistered.Add(typeof(TCommand));
            }

            Func<uint, uint> sendRequestWithTimeoutMs =
                timeoutMs => SendCommandRequest(entityId, rawRequest, timeoutMs, commandDelivery);
            return SendGenericCommand(callback, sendRequestWithTimeoutMs, timeout);
        }

        /// <summary>
        ///     This method is required to prevent Unity compiler issues.
        /// </summary>
        private ulong RegisterCommandResponse<TCommand, TResponse>(Func<ICommandResponse<TCommand>, TResponse> extractResponseFunc)
            where TCommand : ICommandMetaclass, new()
        {
            return communicator.RegisterCommandResponse(CreateOnResponseThunk<CommandResponseOp<TCommand>, TResponse>(
                                                                                                                      op => op.RequestId.Id, op => op.StatusCode, op => op.Message,
                                                                                                                      op => ExtractResponse(extractResponseFunc, op)));
        }

        /// <summary>
        ///     This method is required to prevent Unity compiler issues.
        /// </summary>
        private Option<TResponse> ExtractResponse<TCommand, TResponse>(Func<ICommandResponse<TCommand>, TResponse> extractResponseFunc,
                                                                       CommandResponseOp<TCommand> op) where TCommand : ICommandMetaclass, new()
        {
            return op.Response.HasValue ? extractResponseFunc(op.Response.Value) : new Option<TResponse>();
        }

        /// <summary>
        ///     This method is required to prevent Unity compiler issues.
        /// </summary>
        private uint SendCommandRequest<TCommand>(EntityId entityId, ICommandRequest<TCommand> rawRequest, uint timeoutMs, CommandDelivery commandDelivery)
            where TCommand : ICommandMetaclass, new()
        {
            return communicator.SendCommandRequest(entityId, rawRequest, timeoutMs, commandDelivery).Id;
        }

        private Action<TOp> CreateOnResponseThunk<TOp, TResponse>(Func<TOp, uint> requestIdFromOp,
                                                                  Func<TOp, StatusCode> statusCodeFromOp, Func<TOp, string> errorMessageFromOp, Func<TOp, Option<TResponse>> userResponseFromOp)
        {
            return op =>
            {
                ICommandCallbackWrapper callbackWrapper;
                var requestId = requestIdFromOp(op);
                if (!requestIdToCallback.TryGetValue(requestId, out callbackWrapper))
                {
                    return;
                }

                requestIdToCallback.Remove(requestId);
                CommandCallbackWrapper<TResponse> typedCallbackWrapper;
                try
                {
                    typedCallbackWrapper = (CommandCallbackWrapper<TResponse>) callbackWrapper;
                }
                catch (InvalidCastException exception)
                {
                    // ERROR: could not cast callback to expected type
                    Debug.LogException(exception);
                    return;
                }

                typedCallbackWrapper.TriggerWithStatus(statusCodeFromOp(op), userResponseFromOp(op),
                                                       errorMessageFromOp(op));
            };
        }

        /// <summary>
        ///     Removes requestId from the dictionary of known request ids.
        /// </summary>
        /// <remarks>
        ///     This is used by the old Commander implementation, and should be removed when possible.
        /// </remarks>
        public void ForgetRequestId(uint requestId)
        {
            requestIdToCallback.Remove(requestId);
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

        public ICommandResponseHandler<DeleteEntityResult> DeleteEntity(EntityId entityId, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<DeleteEntityResult>.Wrap(callback => DeleteEntity(entityId, callback, timeout));
        }

        public ICommandResponseHandler<EntityQueryResult> SendQuery(EntityQuery query, TimeSpan? timeout = null)
        {
            return CommandResponseHandler<EntityQueryResult>.Wrap(callback => SendQuery(query, callback, timeout));
        }
    }
}
