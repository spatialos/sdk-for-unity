// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <inheritdoc />
    class CommandResponseHandler<TResponse> : ICommandResponseHandler<TResponse>
    {
        private readonly Queue<CommandSuccessCallback<TResponse>> successCallbacks = new Queue<CommandSuccessCallback<TResponse>>();
        private readonly Queue<CommandFailureCallback> failureCallbacks = new Queue<CommandFailureCallback>();

        /// <summary>
        ///     Utility function for wrapping callbacks into CommandResponseHandler objects.
        /// </summary>
        public static CommandResponseHandler<TResponse> Wrap(Action<CommandCallback<TResponse>> wrap)
        {
            var commandResponseHandler = new CommandResponseHandler<TResponse>();
            wrap(commandResponseHandler.Trigger);
            return commandResponseHandler;
        }

        /// <inheritdoc />
        public ICommandResponseHandler<TResponse> OnSuccess(CommandSuccessCallback<TResponse> successCallback)
        {
            this.successCallbacks.Enqueue(successCallback);
            return this;
        }

        /// <inheritdoc />
        public ICommandResponseHandler<TResponse> OnFailure(CommandFailureCallback failureCallback)
        {
            this.failureCallbacks.Enqueue(failureCallback);
            return this;
        }

        public void Trigger(ICommandCallbackResponse<TResponse> response)
        {
            if (response.StatusCode == StatusCode.Success)
            {
                TriggerSuccess(response);
            }
            else
            {
                TriggerFailure(response);
            }
        }

        private void TriggerSuccess(ICommandCallbackResponse<TResponse> response)
        {
            while (successCallbacks.Count > 0)
            {
                try
                {
                    successCallbacks.Dequeue()(response.Response.Value);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void TriggerFailure(ICommandCallbackResponse<TResponse> response)
        {
            while (failureCallbacks.Count > 0)
            {
                try
                {
                    failureCallbacks.Dequeue()(response);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
