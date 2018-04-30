// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     This class hosts coroutines for the ConnectLifecyle to ensure that all
    ///     unmanaged resources (futures, etc.) are freed during aborted connection attempts.
    /// </summary>
    class ConnectionCoroutineHost : MonoBehaviour { }
}
