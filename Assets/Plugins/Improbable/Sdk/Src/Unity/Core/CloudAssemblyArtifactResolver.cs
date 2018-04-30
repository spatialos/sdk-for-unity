// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Util;
using UnityEngine;

namespace Improbable.Unity.Core
{
    class CloudAssemblyArtifactResolver
    {
        private readonly MonoBehaviour coroutineHost;
        private readonly IWWWRequest wwwRequest;
        private readonly string infraServiceUrl;
        private readonly string projectName;
        private readonly string assemblyName;
        private readonly Action<Dictionary<string, string>> onAssetsResolved;
        private readonly Action<Exception> onFailed;
        private readonly TaskRunnerWithExponentialBackoff<WWWResponse> taskRunner;
        private WWWRequestWrapper activeRequest;

        private enum State
        {
            WaitingToStart,
            Started,
            Cancelled,
            Completed
        }

        private State state;

        private CloudAssemblyArtifactResolver(MonoBehaviour coroutineHost, IWWWRequest wwwRequest, string infraServiceUrl, string projectName, string assemblyName, Action<Dictionary<string, string>> onAssetsResolved, Action<Exception> onFailed)
        {
            this.coroutineHost = coroutineHost;
            this.wwwRequest = wwwRequest;
            this.infraServiceUrl = infraServiceUrl;
            this.projectName = projectName;
            this.assemblyName = assemblyName;
            this.onAssetsResolved = onAssetsResolved;
            this.onFailed = onFailed;

            taskRunner = new TaskRunnerWithExponentialBackoff<WWWResponse>();

            state = State.WaitingToStart;
        }

        public void Cancel()
        {
            switch (state)
            {
                case State.WaitingToStart:
                    throw new Exception("Trying to cancel assembly artifact resolution before it has started.");
                case State.Started:
                    state = State.Cancelled;

                    activeRequest.Cancel();

                    taskRunner.ProcessResult(null);
                    break;
                case State.Cancelled:
                    throw new Exception("This assembly artifact resolution was already cancelled.");
                case State.Completed:
                    // Do nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Start()
        {
            Action runTask = () =>
            {
                var headers = new Dictionary<string, string>
                {
                    { "Accept", "application/json" }
                };
                var url = string.Format("{0}/assembly_content/v3/{1}/{2}/artifacts", infraServiceUrl, projectName, assemblyName);

                activeRequest = wwwRequest.SendPostRequest(coroutineHost, url, null, headers, (response) =>
                {
                    if (state == State.Cancelled)
                    {
                        // Ignore cancelled requests because the Cancel function will already call taskRunner.ProcessResult.
                        return;
                    }

                    taskRunner.ProcessResult(response);
                });

                state = State.Started;
            };
            Func<WWWResponse, TaskResult> evaluationFunc = response =>
            {
                if (state == State.Cancelled)
                {
                    // Return a success so it will finish running the task and will not be retried.

                    return new TaskResult
                    {
                        IsSuccess = true
                    };
                }

                return new TaskResult
                {
                    IsSuccess = string.IsNullOrEmpty(response.Error),
                    ErrorMessage = response.Error
                };
            };
            Action<WWWResponse> onSuccess = response =>
            {
                if (state == State.Cancelled)
                {
                    // Do not call the onAssetsResolved or onFailed here, as the owner of this instance has initiated the request be cancelled.
                    return;
                }

                state = State.Completed;

                var assetUrls = new Dictionary<string, string>();
                var jsonResponse = JsonUtility.FromJson<AssemblyResponse>(response.Text);
                if (jsonResponse.Artifacts.Count == 0)
                {
                    onFailed(new Exception(string.Format("No artifacts found at {0}", response.Url)));
                }
                else
                {
                    for (var i = 0; i < jsonResponse.Artifacts.Count; i++)
                    {
                        var artifact = jsonResponse.Artifacts[i];
                        assetUrls[artifact.ArtifactId.Name] = artifact.Url;
                    }

                    onAssetsResolved(assetUrls);
                }
            };
            Action<string> onFailure = (string errorMessage) =>
            {
                state = State.Completed;

                onFailed(new Exception("Failed to retrieve assembly list: " + errorMessage));
            };
            taskRunner.RunTaskWithRetries("CloudAssemblyArtifactResolver::ResolveAssetUrls", coroutineHost, runTask, evaluationFunc, onSuccess, onFailure);
        }

        public static CloudAssemblyArtifactResolver ResolveAssetUrls(MonoBehaviour coroutineHost, IWWWRequest wwwRequest, string infraServiceUrl, string projectName, string assemblyName, Action<Dictionary<string, string>> onAssetsResolved, Action<Exception> onFailed)
        {
            var resolver = new CloudAssemblyArtifactResolver(coroutineHost, wwwRequest, infraServiceUrl, projectName, assemblyName, onAssetsResolved, onFailed);

            resolver.Start();

            return resolver;
        }
    }

    [Serializable]
    public class AssemblyResponse
    {
        [SerializeField]
#pragma warning disable 649
        private List<AssemblyArtifact> artifacts;
#pragma warning restore 649
        public List<AssemblyArtifact> Artifacts
        {
            get { return artifacts; }
            set { artifacts = value; }
        }
    }

    [Serializable]
    public class ArtifactId
    {
        [SerializeField]
#pragma warning disable 649
        private string name;
#pragma warning restore 649
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }

    [Serializable]
    public class AssemblyArtifact
    {
        [SerializeField]
#pragma warning disable 649
        // ReSharper disable once InconsistentNaming
        private ArtifactId artifact_id;
#pragma warning restore 649
        public ArtifactId ArtifactId
        {
            get { return artifact_id; }
            set { artifact_id = value; }
        }

        [SerializeField]
#pragma warning disable 649
        private string url;
#pragma warning restore 649
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
    }
}
