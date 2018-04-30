// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Entity.Component;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     An interface to the old style of sending commands from component writers.
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        ///     Invokes a command on an entity's component.
        ///     The callback may be null if you wish to 'fire and forget'.
        /// </summary>
        void SendCommand<TCommand, TRequest, TResponse>(IComponentWriter writer,
                                                        ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor, TRequest request,
                                                        EntityId entityId, CommandCallback<TResponse> callback, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip)
            where TCommand : ICommandMetaclass, new();

        /// <summary>
        ///     Invokes a command on an entity's component.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure.
        /// </summary>
        ICommandResponseHandler<TResponse> SendCommand<TCommand, TRequest, TResponse>(IComponentWriter writer,
                                                                                      ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor,
                                                                                      TRequest request, EntityId entityId, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip)
            where TCommand : ICommandMetaclass, new();

        /// <summary>
        ///     Reserves an entity ID for later use in creating an entity.
        /// </summary>
        void ReserveEntityId(IComponentWriter writer, CommandCallback<ReserveEntityIdResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Reserves a number of entity IDs for later use in creating a number of entities.
        /// </summary>
        void ReserveEntityIds(IComponentWriter writer, CommandCallback<ReserveEntityIdsResult> callback, uint numberOfEntityIds, TimeSpan? timeout = null);

        /// <summary>
        ///     Reserves an entity ID for later use in creating an entity.
        /// </summary>
        // #SNIPPET_START reserve_entity_complex_interface
        ICommandResponseHandler<ReserveEntityIdResult> ReserveEntityId(IComponentWriter writer, TimeSpan? timeout = null);
        // #SNIPPET_END reserve_entity_complex_interface

        /// <summary>
        ///     Reserves a number of entity IDs for later use in creating a number of entities.
        /// </summary>
        ICommandResponseHandler<ReserveEntityIdsResult> ReserveEntityIds(IComponentWriter writer, uint numberOfEntityIds, TimeSpan? timeout = null);

        /// <summary>
        ///     Creates an entity with a previously reserved entity id.
        /// </summary>
        void CreateEntity(IComponentWriter writer, EntityId reservedEntityId, Worker.Entity template,
                          CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Creates an entity with a previously reserved entity id.
        /// </summary>
        // #SNIPPET_START create_entity_complex_interface
        ICommandResponseHandler<CreateEntityResult> CreateEntity(IComponentWriter writer, EntityId reservedEntityId,
                                                                 Worker.Entity template, TimeSpan? timeout = null);
        // #SNIPPET_END create_entity_complex_interface

        /// <summary>
        ///     Creates an entity without needing to manually reserve an entity id.
        /// </summary>
        void CreateEntity(IComponentWriter writer, Worker.Entity template,
                          CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Creates an entity without needing to manually reserve an entity id.
        /// </summary>
        // #SNIPPET_START create_entity_simple_interface
        ICommandResponseHandler<CreateEntityResult> CreateEntity(IComponentWriter writer,
                                                                 Worker.Entity template, TimeSpan? timeout = null);
        // #SNIPPET_END create_entity_simple_interface

        /// <summary>
        ///     Deletes an entity.
        /// </summary>
        void DeleteEntity(IComponentWriter writer, EntityId entityId,
                          CommandCallback<DeleteEntityResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Deletes an entity.
        /// </summary>
        // #SNIPPET_START delete_entity_interface
        ICommandResponseHandler<DeleteEntityResult> DeleteEntity(IComponentWriter writer, EntityId entityId, TimeSpan? timeout = null);
        // #SNIPPET_END delete_entity_interface

        /// <summary>
        ///     Sends a query and gets back a response.
        /// </summary>
        void SendQuery(IComponentWriter writer, EntityQuery query,
                       CommandCallback<EntityQueryResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Sends a query and gets back a response.
        /// </summary>
        ICommandResponseHandler<EntityQueryResult> SendQuery(IComponentWriter writer, EntityQuery query, TimeSpan? timeout = null);
    }
}
