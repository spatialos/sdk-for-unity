// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Collections;
using Improbable.Unity.Configuration;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Logging
{
    /// <summary>
    ///     Helper for IoC in tests.
    /// </summary>
    internal delegate void MessageSender(LogLevel level, string loggerName, string message, Option<EntityId> entityId);

    internal class LogSender : IDisposable, ILogFilterReceiver
    {
        private readonly Func<bool> isConnected;
        private readonly MessageSender sender;
        private readonly WorkerConfiguration configuration;
        private readonly ILogFilterReceiver filter;

        public LogSender(Func<bool> isConnected, MessageSender sender, WorkerConfiguration configuration, ILogFilterReceiver filter)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            if (isConnected == null)
            {
                throw new ArgumentNullException("isConnected");
            }

            this.isConnected = isConnected;
            this.sender = sender;
            this.configuration = configuration;
            this.filter = filter ?? this;

            Application.logMessageReceived += ApplicationOnLogMessageReceived;
        }

        public LogAction FilterLogMessage(string logstring, string stacktrace, LogType type)
        {
            return LogAction.SendIfAllowed;
        }

        public void Dispose()
        {
            Application.logMessageReceived -= ApplicationOnLogMessageReceived;
        }

        private void ApplicationOnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (!isConnected())
            {
                return;
            }

            var action = filter.FilterLogMessage(logString, stackTrace, type);

            LogLevel logLevel;
            bool shouldSendLog;
            switch (type)
            {
                case LogType.Log:
                    shouldSendLog = configuration.LogDebugToSpatialOs;
                    logLevel = LogLevel.Info;
                    break;
                case LogType.Assert:
                    shouldSendLog = configuration.LogAssertToSpatialOs;
                    logLevel = LogLevel.Error;
                    break;
                case LogType.Warning:
                    shouldSendLog = configuration.LogWarningToSpatialOs;
                    logLevel = LogLevel.Warn;
                    break;
                case LogType.Error:
                    shouldSendLog = configuration.LogErrorToSpatialOs;
                    logLevel = LogLevel.Error;
                    break;
                case LogType.Exception:
                    shouldSendLog = configuration.LogExceptionToSpatialOs;
                    logLevel = LogLevel.Error;
                    break;
                default:
                    shouldSendLog = configuration.LogDebugToSpatialOs;
                    logLevel = LogLevel.Info;
                    break;
            }

            switch (action)
            {
                case LogAction.SendIfAllowed:
                    break;
                case LogAction.SendAlways:
                    // Override the blanket setting from the configuration and send it onwards.
                    shouldSendLog = true;
                    break;
                case LogAction.DontSend:
                    shouldSendLog = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!shouldSendLog)
            {
                return;
            }

            sender(logLevel, "Unity", string.Format("{0}\n in {1}", logString, stackTrace), null);
        }
    }
}
