// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity.Logging
{
    /// <summary>
    ///     Filter log messages that are sent to SpatialOS.
    /// </summary>
    public interface ILogFilterReceiver
    {
        /// <summary>
        ///     Tell Unity SDK if it should send a log message or not.
        /// </summary>
        LogAction FilterLogMessage(string logString, string stackTrace, LogType type);
    }
}
