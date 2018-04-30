// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System.Diagnostics;
using Improbable.Unity.Metrics;
using UnityEngine;

namespace Improbable.Metrics
{
    /// <summary>
    ///     Provides an estimation of the load of the Unity worker based on analysis of
    ///     the Unity Script Lifecycle http://docs.unity3d.com/Manual/ExecutionOrder.html
    ///     Uses targetFrameRate and target fixed frame rate (Time.fixedDeltaTime / Time.timeScale)
    ///     to estimate load.
    /// </summary>
    public class ScriptLifecycleLoadMetricProvider : MonoBehaviour, ILoadMetricProvider
    {
        private ScriptLifecycleAnalytics scriptLifecycleAnalytics;
        private long lastPhysicsLoopCount;
        private long lastRenderLoopCount;
        private float lastPhysicsLoopTime;
        private float lastRenderLoopTime;

        private float timePerPhysics;
        private float timePerRender;
        private long lastReportedTime;
        private readonly Stopwatch watch = new Stopwatch();

        // Only query the load metric once per UpdatePeriodMs milliseconds
        public long UpdateMinPeriodMillis = 2000;

        private void Start()
        {
            watch.Start();
            scriptLifecycleAnalytics = gameObject.GetComponent<ScriptLifecycleAnalytics>() ?? gameObject.AddComponent<ScriptLifecycleAnalytics>();
        }

        private void Update()
        {
            if (watch.ElapsedMilliseconds - lastReportedTime > UpdateMinPeriodMillis)
            {
                lastReportedTime = watch.ElapsedMilliseconds;
                Load = GetLoad();
            }
        }

        /// <returns>
        ///     A load estimation from the last time this method was called.
        ///     If called more frequently, will return noisier data.
        ///     If called before more analytics is present, will return the last recorded load.
        /// </returns>
        private float GetLoad()
        {
            UpdateTimePerPhysicsLoop();
            UpdateTimePerRenderLoop();

            var targetRenderCount = Application.targetFrameRate;
            var targetPhysicsTime = Time.fixedDeltaTime / Time.timeScale;

            return timePerPhysics / targetPhysicsTime + timePerRender * targetRenderCount;
        }

        private void UpdateTimePerPhysicsLoop()
        {
            var newPhysicsLoopCount = scriptLifecycleAnalytics.FixedFrameCount;
            var physicsLoopCountDelta = newPhysicsLoopCount - lastPhysicsLoopCount;

            if (physicsLoopCountDelta > 0)
            {
                var newPhysicsLoopTime = scriptLifecycleAnalytics.CumulativePhysicsLoopDuration();
                timePerPhysics = (newPhysicsLoopTime - lastPhysicsLoopTime) / physicsLoopCountDelta;

                lastPhysicsLoopTime = newPhysicsLoopTime;
                lastPhysicsLoopCount = newPhysicsLoopCount;
            }
        }

        private void UpdateTimePerRenderLoop()
        {
            var newRenderLoopCount = scriptLifecycleAnalytics.FrameCount;
            var renderLoopCountDelta = newRenderLoopCount - lastRenderLoopCount;

            if (renderLoopCountDelta > 0)
            {
                var newRenderLoopTime = scriptLifecycleAnalytics.CumulativeRenderLoopDuration();
                timePerRender = (newRenderLoopTime - lastRenderLoopTime) / renderLoopCountDelta;

                lastRenderLoopTime = newRenderLoopTime;
                lastRenderLoopCount = newRenderLoopCount;
            }
        }

        public double Load { get; private set; }
    }
}
