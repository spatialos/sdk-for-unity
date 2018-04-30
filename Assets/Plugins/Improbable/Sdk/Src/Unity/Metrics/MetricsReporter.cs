// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System.Diagnostics;
using Improbable.Metrics;
using Improbable.Unity.Core;
using Improbable.Unity.Metrics;
using UnityEngine;

namespace Improbable.Unity.Util
{
    class MetricsReporter : MonoBehaviour
    {
        private const long UpdateMinPeriodMillis = 2000;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private long startedAt;
        private ILoadMetricProvider loadMetricProvider;

        public void Start()
        {
            stopwatch.Start();
            loadMetricProvider = gameObject.GetComponent<ILoadMetricProvider>() ?? gameObject.AddComponent<UnityFixedFrameLoadMetricProvider>();
        }

        public void Update()
        {
            var currentTime = stopwatch.ElapsedMilliseconds;
            var elapsed = currentTime - startedAt;

            if (SpatialOS.IsConnected && elapsed > UpdateMinPeriodMillis)
            {
                SpatialOS.Metrics.SetLoad(loadMetricProvider.Load);
                SpatialOS.Connection.SendMetrics(SpatialOS.Metrics.RawMetrics);
                startedAt = currentTime;
            }
        }
    }
}
