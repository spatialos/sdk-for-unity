// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Util
{
    /// <summary>
    ///     Utility class for running tasks with timestamp based retries. Suitable for situations where coroutines are not
    ///     available.
    /// </summary>
    internal static class EditorTaskRunnerWithRetries
    {
        public const int DefaultNumRetries = 1;
        public static readonly TimeSpan RetryPeriod = TimeSpan.FromSeconds(10);

        private static Queue<FailedEditorTask> failedTaskRetryQueue = new Queue<FailedEditorTask>();
        private static IList<Exception> bufferedExceptions = new List<Exception>();

        public static void RunTask(Action task, int retries = DefaultNumRetries, bool throwExceptions = true)
        {
            try
            {
                task();
            }
            catch
            {
                if (retries > 0)
                {
                    Retry(task, retries - 1, throwExceptions);
                }
                else
                {
                    if (throwExceptions)
                    {
                        throw;
                    }
                }
            }
        }

        private static void Retry(Action task, int remainingRetries, bool throwExceptions)
        {
            if (failedTaskRetryQueue.Count == 0)
            {
                EditorApplication.update += EditorUpdate;
            }

            var failedTask = new FailedEditorTask(task, EditorApplication.timeSinceStartup, remainingRetries, throwExceptions);
            failedTaskRetryQueue.Enqueue(failedTask);
        }

        private static void EditorUpdate()
        {
            bufferedExceptions.Clear();

            while (failedTaskRetryQueue.Count > 0 && RetryPeriodHasElapsed(failedTaskRetryQueue.Peek()))
            {
                var failedTask = failedTaskRetryQueue.Dequeue();
                try
                {
                    RunTask(failedTask.Task, failedTask.RemainingRetries, failedTask.ShouldThrowExceptions);
                }
                catch (Exception e)
                {
                    bufferedExceptions.Add(e);
                }
            }

            if (failedTaskRetryQueue.Count == 0)
            {
                EditorApplication.update -= EditorUpdate;
            }

            for (var i = 0; i < bufferedExceptions.Count; i++)
            {
                Debug.LogException(bufferedExceptions[i]);
            }
        }

        private static bool RetryPeriodHasElapsed(FailedEditorTask task)
        {
            return task.TimeStamp <= EditorApplication.timeSinceStartup - RetryPeriod.Seconds;
        }
    }
}
