// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using UnityEngine;

namespace Improbable.Unity.Logging
{
    /// <summary>
    ///     Report exceptions to the logger.
    /// </summary>
    /// <see cref="Core.SpatialOS.LogFilter" />
    /// for a method of filtering log messages.
    [Obsolete("This class no longer does anything and will be removed. Please see SpatialOS.LogFilter and SpatialOS.LogFormatter for more information.")]
    public class LogListener : MonoBehaviour
    {
        public void OnEnable()
        {
            Debug.LogWarningFormat("{0} is no longer used. Please see the documentation for information about configuring your log messages.", typeof(LogListener));
        }
    }
}
