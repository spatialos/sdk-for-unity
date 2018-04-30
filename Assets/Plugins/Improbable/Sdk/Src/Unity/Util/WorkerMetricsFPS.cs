// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Diagnostics;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Util
{
    public class WorkerMetricsFPS : MonoBehaviour
    {
        private const long UpdateMinPeriodMillis = 2000; // 1 update per 2 seconds.
        private readonly Stopwatch stopwatch = new Stopwatch();
        private FPSMetric dynamicRate;
        private FPSMetric fixedRate;

        public void Start()
        {
            dynamicRate = new FPSMetric("Dynamic");
            fixedRate = new FPSMetric("Fixed");
            stopwatch.Start();
        }

        public void FixedUpdate()
        {
            if (fixedRate != null)
            {
                fixedRate.FrameRendered(stopwatch.ElapsedMilliseconds);
            }
        }

        public void Update()
        {
            if (dynamicRate != null)
            {
                dynamicRate.FrameRendered(stopwatch.ElapsedMilliseconds);
            }
        }

        private class FPSMetric
        {
            private long startedAt;
            private int frameCount;
            private double dt;
            private double fps;

            private readonly string fpsGaugeName;
            private readonly string artGaugeName;

            internal FPSMetric(string prefix)
            {
                fpsGaugeName = prefix + ".FPS";
                artGaugeName = prefix + ".AverageRenderTime";
            }

            /* Time.deltaTime gives you the time to render the last frame.
             * dt is then the sum of frame rendering time since last fps update.
             * frameCount is the number of frame renderings since last fps update.
             */
            internal void FrameRendered(long currentTime)
            {
                frameCount++;
                dt += (1000 * Time.deltaTime);
                var elapsed = currentTime - startedAt;
                if (elapsed > UpdateMinPeriodMillis)
                {
                    fps = ((frameCount * 1000.0) / elapsed);
                    WriteMetrics();
                    frameCount = 0;
                    dt = 0;
                    startedAt = currentTime;
                }
            }

            private void WriteMetrics()
            {
                SpatialOS.Metrics.SetGauge(fpsGaugeName, fps);
                SpatialOS.Metrics.SetGauge(artGaugeName, dt / frameCount);
            }
        }
    }
}
