// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Defers actions to be executed at a later time.
    /// </summary>
    public interface IDeferredActionDispatcher : IDisposable
    {
        /// <summary>
        ///     Defers the supplied action until the next call to ProcessEvents().
        /// </summary>
        void DeferAction(Action action);

        /// <summary>
        ///     Invokes all deferred actions accumulated in the queue.
        /// </summary>
        void ProcessEvents();
    }
}
