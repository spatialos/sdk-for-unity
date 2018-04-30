// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Setting for delivering entity commands.
    /// </summary>
    public enum CommandDelivery
    {
        // Always send commands over the network. A successful command response guarantees that the command was fully executed on the target worker.
        RoundTrip = 0,

        // Do not send commands over the network if the target entity is on the same worker as the command originator and the worker is authoritative over the target entity's component. No guarantees.
        ShortCircuit = 1
    }
}
