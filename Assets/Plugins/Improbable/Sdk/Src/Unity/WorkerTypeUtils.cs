// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;

namespace Improbable.Unity
{
    public static class WorkerTypeUtils
    {
        public const string UnityClientType = "UnityClient";
        public const string UnityWorkerType = "UnityWorker";

        private static readonly Dictionary<WorkerPlatform, string> WorkerPlatformToNameMap = new Dictionary<WorkerPlatform, string>
        {
            { WorkerPlatform.UnityClient, UnityClientType },
            { WorkerPlatform.UnityWorker, UnityWorkerType }
        };

        private static readonly Dictionary<string, WorkerPlatform> NameToWorkerPlatformMap = new Dictionary<string, WorkerPlatform>
        {
            { UnityClientType, WorkerPlatform.UnityClient },
            { UnityWorkerType, WorkerPlatform.UnityWorker }
        };

        public static string ToWorkerName(WorkerPlatform workerPlatform)
        {
            return WorkerPlatformToNameMap[workerPlatform];
        }

        public static WorkerPlatform FromWorkerName(string workerName)
        {
            return NameToWorkerPlatformMap[workerName];
        }
    }
}
