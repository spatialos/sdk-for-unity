// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using Improbable.Unity.Editor.Core;

namespace Improbable.Unity.EditorTools.Build
{
    interface IWorkerProvider
    {
        IList<SpatialOsWorker> GetWorkers();
    }
}
