// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.Visualizer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WorkerTypeAttribute : Attribute
    {
        public WorkerTypeAttribute(WorkerPlatform workerPlatform)
        {
            WorkerPlatform = workerPlatform;
        }

        public WorkerPlatform WorkerPlatform { get; private set; }
    }
}
