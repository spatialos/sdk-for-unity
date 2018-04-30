// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity
{
    [Flags]
    public enum WorkerPlatform
    {
        UnityWorker = 0x1,
        UnityClient = 0x2,
    }
}
