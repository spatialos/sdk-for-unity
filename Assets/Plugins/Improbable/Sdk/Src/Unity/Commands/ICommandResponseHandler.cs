// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Allows for registration of callbacks for success or failure of commands.
    /// </summary>
    public interface ICommandResponseHandler<TResponse>
    {
        /// <summary>
        ///     Registers callback to be invoked when command succeeded.
        /// </summary>
        ICommandResponseHandler<TResponse> OnSuccess(CommandSuccessCallback<TResponse> successCallback);

        /// <summary>
        ///     Registers callback to be invoked when command failed.
        /// </summary>
        ICommandResponseHandler<TResponse> OnFailure(CommandFailureCallback failureCallback);
    }
}
