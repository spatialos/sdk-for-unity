// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Logging
{
    /// <summary>
    /// </summary>
    public enum LogAction
    {
        /// <summary>
        ///     Send the log message to SpatialOS if the <see cref="Configuration.WorkerConfiguration" /> allows it.
        /// </summary>
        SendIfAllowed,

        /// <summary>
        ///     Send the log messsage, even if <see cref="Configuration.WorkerConfiguration" /> is configured not to send it.
        /// </summary>
        SendAlways,

        /// <summary>
        ///     Do not send the log message to SpatialOS.
        /// </summary>
        DontSend
    }
}
