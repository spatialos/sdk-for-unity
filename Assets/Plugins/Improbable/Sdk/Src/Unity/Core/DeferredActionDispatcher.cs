// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Defers actions to be executed at a later time.
    /// </summary>
    public class DeferredActionDispatcher : IDeferredActionDispatcher
    {
        private Queue<Action> currentQueue = new Queue<Action>();
        private Queue<Action> emptyQueue = new Queue<Action>();

        /// <inheritdoc />
        public void DeferAction(Action action)
        {
            currentQueue.Enqueue(action);
        }

        /// <inheritdoc />
        public void ProcessEvents()
        {
            // Replace current queue with an empty one in case deferred
            // actions place more actions in the queue; these need to happen
            // on the next invocation of ProcessEvents() method.
            var capturedQueue = currentQueue;
            currentQueue = emptyQueue;

            // Process local events.
            while (capturedQueue.Count > 0)
            {
                try
                {
                    capturedQueue.Dequeue()();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // Return the queue for reuse.
            emptyQueue = capturedQueue;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            currentQueue.Clear();
        }
    }
}
