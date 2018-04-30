// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;
using System.Collections;
using System.Diagnostics;
using Improbable.Unity.Metrics;
using UnityEngine;

namespace Improbable.Metrics
{
    /// <summary>
    ///     Provide load based on how long a frame takes to render.
    /// </summary>
    public class UnityFixedFrameLoadMetricProvider : MonoBehaviour, ILoadMetricProvider
    {
        private static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        private Coroutine endOfFrameCoroutine;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private TimeSpan? lastFixedUpdateStart;

        public double Load { get; private set; }

        public void Awake()
        {
            stopwatch.Start();
        }

        private void OnEnable()
        {
            endOfFrameCoroutine = StartCoroutine(EndOfFrame());
        }

        private void OnDisable()
        {
            if (endOfFrameCoroutine != null)
            {
                StopCoroutine(endOfFrameCoroutine);
                endOfFrameCoroutine = null;
            }
        }

        public void FixedUpdate()
        {
            SnapshotTime();
        }

        private void SnapshotTime()
        {
            if (!lastFixedUpdateStart.HasValue)
            {
                lastFixedUpdateStart = stopwatch.Elapsed;
            }
        }

        private IEnumerator EndOfFrame()
        {
            while (isActiveAndEnabled)
            {
                yield return WaitForEndOfFrame;

                if (lastFixedUpdateStart.HasValue)
                {
                    var delta = stopwatch.Elapsed - lastFixedUpdateStart.Value;
                    var maxDeltaTime = 1.0 / Application.targetFrameRate;

                    Load = delta.TotalSeconds / maxDeltaTime;

                    lastFixedUpdateStart = null;
                }
            }
        }
    }
}
