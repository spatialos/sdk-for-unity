// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Util
{
    public class WorkerTypeDisplay : MonoBehaviour
    {
        private static readonly Rect NAME_LABEL_POSITION = new Rect(10, 5, 300, 30);
        private string workerType = string.Empty;

        private void Start()
        {
            workerType = WorkerTypeUtils.ToWorkerName(SpatialOS.Configuration.WorkerPlatform);
        }

        private void OnGUI()
        {
            GUI.Label(NAME_LABEL_POSITION, workerType);
        }
    }
}
