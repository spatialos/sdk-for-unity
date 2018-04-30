// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Collections;
using Improbable.Worker;

namespace Improbable.Unity.Core
{
    interface ICommandCallbackWrapper
    {
        void TriggerWithError(StatusCode statusCode, string errorMessage);
        void TriggerWithAuthorityError();
    }

    class CommandCallbackWrapper<TResponse> : ICommandCallbackWrapper
    {
        private readonly CommandCallback<TResponse> commandCallback;

        public CommandCallbackWrapper(CommandCallback<TResponse> commandCallback)
        {
            this.commandCallback = commandCallback;
        }

        public void TriggerWithStatus(StatusCode statusCode, Option<TResponse> userResponse, string errorMessage = null)
        {
            if (commandCallback == null)
            {
                // Fire and forget succeeded
                return;
            }

            commandCallback(new CommandCallbackResponse<TResponse>
            {
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                Response = userResponse
            });
        }

        public void TriggerWithError(StatusCode statusCode, string errorMessage)
        {
            if (commandCallback == null)
            {
                // Fire and forget failed
                return;
            }

            commandCallback(new CommandCallbackResponse<TResponse>
            {
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                Response = new Option<TResponse>()
            });
        }

        public void TriggerWithAuthorityError()
        {
            TriggerWithError(StatusCode.AuthorityLost, "This worker does not have write authority on the given entity/component pair.");
        }
    }

    /// <summary>
    ///     A callback of this type must be passed with each command request, in order to return the response to the caller.
    ///     The callback is passed an ICommandCallbackResponse object when it is invoked.
    /// </summary>
    /// <param name="response">The response.</param>
    public delegate void CommandCallback<TResponse>(ICommandCallbackResponse<TResponse> response);

    /// <summary>
    ///     Callback invoked when a command is successfully executed.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="response"></param>
    public delegate void CommandSuccessCallback<in TResponse>(TResponse response);

    /// <summary>
    ///     Callback invoked when a command invocation fails.
    /// </summary>
    /// <param name="response"></param>
    public delegate void CommandFailureCallback(ICommandErrorDetails response);

    /// <summary>
    ///     This wraps up the response from a command call.
    /// </summary>
    public interface ICommandCallbackResponse<TResponse> : ICommandErrorDetails
    {
        /// <summary>
        ///     The value returned by the callee. Might be null in case no response was received.
        /// </summary>
        Option<TResponse> Response { get; }
    }

    /// <summary>
    ///     Contains the details of the command invocation error.
    /// </summary>
    public interface ICommandErrorDetails
    {
        /// <summary>
        ///     Whether or not the command was successful.
        /// </summary>
        StatusCode StatusCode { get; }

        /// <summary>
        ///     The error message returned in case the command was not successful.
        /// </summary>
        string ErrorMessage { get; }
    }

    /// <summary>
    ///     This wraps up the response from a command call.
    /// </summary>
    public struct CommandCallbackResponse<TResponse> : ICommandCallbackResponse<TResponse>
    {
        /// <summary>
        ///     Whether or not the command was successful.
        /// </summary>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        ///     The error message returned in case the command was not successful.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     The value returned by the callee. Might be null in case no response was received.
        /// </summary>
        public Option<TResponse> Response { get; set; }
    }
}
