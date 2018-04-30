// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Worker;

namespace Improbable.Unity.Core
{
    public interface IComponentCommander : IWorldCommander, IDisposable
    {
        /// <summary>
        ///     Sends a command. This method should not need to be used directly, as extension methods on ICommander are generated
        ///     for each command defined in the schema.
        /// </summary>
        void SendCommand<TCommand, TResponse>(EntityId entityId, ICommandRequest<TCommand> rawRequest,
                                              Func<ICommandResponse<TCommand>, TResponse> extractResponseFunc, CommandCallback<TResponse> callback, TimeSpan? timeout = null, CommandDelivery commandDelivery = CommandDelivery.RoundTrip)
            where TCommand : ICommandMetaclass, new();
    }
}
