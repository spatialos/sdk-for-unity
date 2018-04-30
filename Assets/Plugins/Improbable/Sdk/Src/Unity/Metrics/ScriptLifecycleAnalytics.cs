// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Improbable.Metrics
{
    /// <summary>
    ///     This class provides analytics of the Unity Script execution loop.
    ///     The exposed metrics report the total amount of time spent in the
    ///     relevant section since the start of measurements, and the amount
    ///     of time spent when given section was last executed. All reported
    ///     numbers are in seconds.
    ///     Rendering and its relevant metrics assume that Unity does not sleep
    ///     in between FixedFrames.
    ///     http://docs.unity3d.com/Manual/ExecutionOrder.html
    ///     Note: this class is a MonoBehaviour so that its dependencies can
    ///     be managed easily.
    /// </summary>
    class ScriptLifecycleAnalytics : MonoBehaviour
    {
        private readonly Stopwatch watch = new Stopwatch();
        private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        private long startOfFixedUpdate;
        private long startOfUpdate;
        private long startOfLateUpdate;

        private long lastFixedUpdateDuration;
        private long lastUpdateDuration;
        private long lastLateUpdateDuration;
        private bool shouldCalculatePhysicsUpdateDuration;
        private long lastPhysicsDuration;
        private long lastAnimationUpdateDuration;
        private long lastRenderDuration;

        private long cumulativeFixedUpdateDuration;
        private long cumulativeUpdateDuration;
        private long cumulativeLateUpdateDuration;
        private long cumulativePhysicsDuration;
        private long cumulativeAnimationUpdateDuration;
        private long cumulativeRenderDuration;

        public long FixedFrameCount { get; private set; }
        public long FrameCount { get; private set; }

        /// <returns>Time in seconds spent during last call to FixedUpdate.</returns>
        public float LastFixedUpdateDuration()
        {
            return (float) lastFixedUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>Time in seconds spent during all calls to FixedUpdate since start of game.</returns>
        public float CumulativeFixedUpdateDuration()
        {
            return (float) cumulativeFixedUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>Time in seconds spent during last call to Update.</returns>
        public float LastUpdateDuration()
        {
            return (float) lastUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>Time in seconds spent during all calls to Update since start of game.</returns>
        public float CumulativeUpdateDuration()
        {
            return (float) cumulativeUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>Time in seconds spent during last call to LateUpdate.</returns>
        public float LastLateUpdateDuration()
        {
            return (float) lastLateUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>Time in seconds spent during all calls to LateUpdate since start of game.</returns>
        public float CumulativeLateUpdateDuration()
        {
            return (float) cumulativeLateUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during last physics calculation.
        ///     Includes calls to WaitForFixedUpdate, OnTrigger and OnCollision.
        /// </returns>
        public float LastPhysicsDuration()
        {
            return (float) lastPhysicsDuration / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during all physics calculations since start of game.
        ///     Includes calls to WaitForFixedUpdate, OnTrigger and OnCollision.
        /// </returns>
        public float CumulativePhysicsDuration()
        {
            return (float) cumulativePhysicsDuration / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during last Internal animation update.
        ///     Includes calls to yield {null, WaitForSeconds, WWW, and StartCoroutine}.
        /// </returns>
        public float LastAnimationUpdateDuration()
        {
            return (float) lastAnimationUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during all Internal animation updates since start of game.
        ///     Includes calls to yield {null, WaitForSeconds, WWW, and StartCoroutine}.
        /// </returns>
        public float CumulativeAnimationUpdateDuration()
        {
            return (float) cumulativeAnimationUpdateDuration / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during last Scene rendering.
        ///     Includes calls to OnDrawGizmos and OnGUI.
        /// </returns>
        public float LastRenderDuration()
        {
            return (float) lastRenderDuration / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during all Scene renderings since start of game.
        ///     Includes calls to OnDrawGizmos and OnGUI.
        /// </returns>
        public float CumulativeRenderDuration()
        {
            return (float) cumulativeRenderDuration / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during last Physics calculation loop.
        ///     See http://docs.unity3d.com/Manual/ExecutionOrder.html for reference.
        /// </returns>
        public float LastPhysicsLoopDuration()
        {
            return (float) (lastFixedUpdateDuration + lastPhysicsDuration) / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during all Physics calculation loops since start of game.
        ///     See http://docs.unity3d.com/Manual/ExecutionOrder.html for reference.
        /// </returns>
        public float CumulativePhysicsLoopDuration()
        {
            return (float) (cumulativeFixedUpdateDuration + cumulativePhysicsDuration) / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during last Rendering calculation loop,
        ///     excluding the inner Physics loop.
        ///     See http://docs.unity3d.com/Manual/ExecutionOrder.html for reference.
        /// </returns>
        public float LastRenderLoopDuration()
        {
            return (float) (lastUpdateDuration + lastAnimationUpdateDuration + lastLateUpdateDuration + lastRenderDuration) / Stopwatch.Frequency;
        }

        /// <returns>
        ///     Time in seconds spent during all Rendering calculation loops since start of game,
        ///     excluding the inner Physics loop.
        ///     See http://docs.unity3d.com/Manual/ExecutionOrder.html for reference.
        /// </returns>
        public float CumulativeRenderLoopDuration()
        {
            return (float) (cumulativeUpdateDuration + cumulativeAnimationUpdateDuration + cumulativeLateUpdateDuration + cumulativeRenderDuration) / Stopwatch.Frequency;
        }

        private void OnEnable()
        {
            var startTimer = gameObject.GetComponent<StartTimer>() ?? gameObject.AddComponent<StartTimer>();
            startTimer.Analytics = this;
            startTimer.enabled = true;

            var endTimer = gameObject.GetComponent<EndTimer>() ?? gameObject.AddComponent<EndTimer>();
            endTimer.Analytics = this;
            endTimer.enabled = true;

            watch.Start();
        }

        private void OnDisable()
        {
            watch.Stop();
            gameObject.GetComponent<StartTimer>().enabled = false;
            gameObject.GetComponent<EndTimer>().enabled = false;
        }

        internal void StartFixedUpdate()
        {
            FixedFrameCount++;
            var now = watch.ElapsedTicks;

            if (shouldCalculatePhysicsUpdateDuration)
            {
                shouldCalculatePhysicsUpdateDuration = false;
                lastPhysicsDuration = now - lastFixedUpdateDuration - startOfFixedUpdate;
                cumulativePhysicsDuration += lastPhysicsDuration;
            }

            startOfFixedUpdate = now;
        }

        internal void EndFixedUpdate()
        {
            lastFixedUpdateDuration = watch.ElapsedTicks - startOfFixedUpdate;
            cumulativeFixedUpdateDuration += lastFixedUpdateDuration;
            shouldCalculatePhysicsUpdateDuration = true;
        }

        internal void StartUpdate()
        {
            startOfUpdate = watch.ElapsedTicks;
            FrameCount++;

            if (shouldCalculatePhysicsUpdateDuration)
            {
                shouldCalculatePhysicsUpdateDuration = false;
                lastPhysicsDuration = startOfUpdate - lastFixedUpdateDuration - startOfFixedUpdate;
                cumulativePhysicsDuration += lastPhysicsDuration;
            }
        }

        internal void EndUpdate()
        {
            lastUpdateDuration = watch.ElapsedTicks - startOfUpdate;
            cumulativeUpdateDuration += lastUpdateDuration;
        }

        internal void StartLateUpdate()
        {
            startOfLateUpdate = watch.ElapsedTicks;
            lastAnimationUpdateDuration = startOfLateUpdate - lastUpdateDuration - startOfUpdate;
            cumulativeAnimationUpdateDuration += lastAnimationUpdateDuration;
        }

        internal void EndLateUpdate()
        {
            lastLateUpdateDuration = watch.ElapsedTicks - startOfLateUpdate;
            cumulativeLateUpdateDuration += lastLateUpdateDuration;
            StartCoroutine(EndOfFrame());
        }

        private IEnumerator EndOfFrame()
        {
            yield return waitForEndOfFrame;

            lastRenderDuration = watch.ElapsedTicks - lastLateUpdateDuration - startOfLateUpdate;
            cumulativeRenderDuration += lastRenderDuration;
        }
    }
}
