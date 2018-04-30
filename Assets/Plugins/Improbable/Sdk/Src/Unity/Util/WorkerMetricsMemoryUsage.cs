// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Util
{
    class WorkerMetricsMemoryUsage : MonoBehaviour
    {
        private void Update()
        {
            SpatialOS.Metrics.SetGauge("Unity used heap size", GC.GetTotalMemory( /* forceFullCollection */ false));
        }
    }
}
