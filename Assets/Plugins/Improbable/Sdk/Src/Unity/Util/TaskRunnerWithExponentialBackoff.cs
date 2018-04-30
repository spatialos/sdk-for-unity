// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections;
using UnityEngine;

namespace Improbable.Unity.Util
{
    public class TaskRunnerWithExponentialBackoff<TResult>
    {
        private bool isRunning = false;
        private string identifier;
        private MonoBehaviour corountineHost;

        private int remainingRetries;
        private float timeUntilNextRetry;

        private Action RunTask;
        private Func<TResult, TaskResult> EvaluationFunc;
        private Action<TResult> OnSuccess;
        private Action<string> OnFailure;

        public void RunTaskWithRetries(string identifier, MonoBehaviour coroutineHost, Action runTask, Func<TResult, TaskResult> evaluationFunc, Action<TResult> onSuccess, Action<string> onFailure, int remainingRetries = 3, float timeUntilNextRetry = 0.25f)
        {
            if (isRunning)
            {
                throw new InvalidOperationException("Task runner is already in use.");
            }

            isRunning = true;
            this.identifier = identifier;
            this.corountineHost = coroutineHost;
            this.remainingRetries = remainingRetries;
            this.timeUntilNextRetry = timeUntilNextRetry;
            RunTask = runTask;
            EvaluationFunc = evaluationFunc;
            OnSuccess = onSuccess;
            OnFailure = onFailure;

            runTask();
        }

        public void ProcessResult(TResult response)
        {
            var result = EvaluationFunc(response);
            if (result.IsSuccess)
            {
                isRunning = false;
                OnSuccess(response);
            }
            else
            {
                if (--remainingRetries >= 0)
                {
                    Debug.LogWarning(string.Format("Task [{0}] failed with message: {1}. Retrying with {2} retries left.", identifier, result.ErrorMessage, remainingRetries));
                    Retry();
                }
                else
                {
                    isRunning = false;
                    Debug.LogError(string.Format("Task [{0}] failed with message: {1}. No retries left. Aborting.", identifier, result.ErrorMessage));
                    OnFailure(result.ErrorMessage);
                }
            }
        }

        private void Retry()
        {
            timeUntilNextRetry *= 2;
            corountineHost.StartCoroutine(WaitAndPerform(timeUntilNextRetry, RunTask));
        }

        private IEnumerator WaitAndPerform(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }
    }

    public struct TaskResult
    {
        public bool IsSuccess;
        public string ErrorMessage;
    }
}
