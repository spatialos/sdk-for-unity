// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Entity.Component;
using Improbable.Worker;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     An interface to the commander that uses command descriptors to send commands.
    /// </summary>
    public interface IDescriptorCommander
    {
        /// <summary>
        ///     Invokes a command on an entity's component.
        ///     The callback may be null if you wish to 'fire and forget'.
        /// </summary>
        void SendCommand<TCommand, TRequest, TResponse>(ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor,
                                                        TRequest request, EntityId entityId, CommandCallback<TResponse> callback, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip)
            where TCommand : ICommandMetaclass, new();

        /// <summary>
        ///     Invokes a command on an entity's component.
        ///     Returns an object that allows you to specify
        ///     callbacks to be invoked in case of the command's success
        ///     or failure.
        /// </summary>
        ICommandResponseHandler<TResponse> SendCommand<TCommand, TRequest, TResponse>(ICommandDescriptor<TCommand, TRequest, TResponse> commandDescriptor,
                                                                                      TRequest request, EntityId entityId, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip)
            where TCommand : ICommandMetaclass, new();
    }
}
