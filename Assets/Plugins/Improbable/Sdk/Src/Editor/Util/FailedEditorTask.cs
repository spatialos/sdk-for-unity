// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.EditorTools.Util
{
    /// <summary>
    ///     Utility companion class for EditorTaskRunnerWithRetries.
    /// </summary>
    internal class FailedEditorTask
    {
        public readonly Action Task;
        public readonly double TimeStamp;
        public readonly int RemainingRetries;
        public readonly bool ShouldThrowExceptions;

        public FailedEditorTask(Action task, double timeStamp, int remainingRetries, bool shouldThrowExceptions)
        {
            Task = task;
            TimeStamp = timeStamp;
            RemainingRetries = remainingRetries;
            ShouldThrowExceptions = shouldThrowExceptions;
        }
    }
}
