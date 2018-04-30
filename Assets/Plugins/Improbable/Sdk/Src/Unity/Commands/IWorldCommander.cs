// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Worker.Query;

namespace Improbable.Unity.Core
{
#pragma warning disable 1584
    /// <summary>
    ///     An interface to the commander that sends world commands.
    /// </summary>
    public interface IWorldCommander
    {
        /// <summary>
        ///     Reserves an entity ID for later use in creating an entity.
        /// </summary>
        void ReserveEntityId(CommandCallback<ReserveEntityIdResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Reserves a number of entity IDs for later use in creating a number of entities.
        /// </summary>
        void ReserveEntityIds(CommandCallback<ReserveEntityIdsResult> callback, uint numberOfEntityIds, TimeSpan? timeout = null);

        /// <summary>
        ///     Reserves an entity ID for later use in creating an entity.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure. <see cref="ICommandResponseHandler{EntityId}" />
        /// </summary>
        ICommandResponseHandler<ReserveEntityIdResult> ReserveEntityId(TimeSpan? timeout = null);

        /// <summary>
        ///     Reserves a number of entity IDs for later use in creating a number of entities.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure. <see cref="ICommandResponseHandler{EntityId}" />
        /// </summary>
        ICommandResponseHandler<ReserveEntityIdsResult> ReserveEntityIds(uint numberOfEntityIds, TimeSpan? timeout = null);

        /// <summary>
        ///     Creates an entity with a previously reserved entity id.
        /// </summary>
        void CreateEntity(EntityId reservedEntityId, Worker.Entity template,
                          CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Creates an entity with a previously reserved entity id.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure. <see cref="ICommandResponseHandler{EntityId}" />
        /// </summary>
        ICommandResponseHandler<CreateEntityResult> CreateEntity(EntityId reservedEntityId, Worker.Entity template, TimeSpan? timeout = null);

        /// <summary>
        ///     Creates an entity without needing to manually reserve an entity id.
        ///     May take up to 2 * timeout to complete.
        /// </summary>
        void CreateEntity(Worker.Entity template,
                          CommandCallback<CreateEntityResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Creates an entity without needing to manually reserve an entity id.
        ///     May take up to 2 * timeout to complete.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure. <see cref="ICommandResponseHandler{EntityId}" />
        /// </summary>
        ICommandResponseHandler<CreateEntityResult> CreateEntity(Worker.Entity template, TimeSpan? timeout = null);

        /// <summary>
        ///     Deletes an entity.
        /// </summary>
        void DeleteEntity(EntityId entityId,
                          CommandCallback<DeleteEntityResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Deletes an entity.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure. <see cref="ICommandResponseHandler{EntityId}" />
        /// </summary>
        ICommandResponseHandler<DeleteEntityResult> DeleteEntity(EntityId entityId, TimeSpan? timeout = null);

        /// <summary>
        ///     Sends a query and gets back a response.
        /// </summary>
        void SendQuery(EntityQuery query, CommandCallback<EntityQueryResult> callback, TimeSpan? timeout = null);

        /// <summary>
        ///     Sends a query and gets back a response.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure. <see cref="ICommandResponseHandler{EntityQueryResult}" />
        /// </summary>
        ICommandResponseHandler<EntityQueryResult> SendQuery(EntityQuery query, TimeSpan? timeout = null);
    }

#pragma warning disable 1584
}
