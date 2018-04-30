// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Metrics
{
    class EndTimer : MonoBehaviour
    {
        public ScriptLifecycleAnalytics Analytics { set; private get; }

        private void FixedUpdate()
        {
            Analytics.EndFixedUpdate();
        }

        private void Update()
        {
            Analytics.EndUpdate();
        }

        private void LateUpdate()
        {
            Analytics.EndLateUpdate();
        }
    }
}
