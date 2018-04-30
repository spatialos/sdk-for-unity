// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Util
{
    /// <summary>
    ///     Unit test wrapper for WWW requests.
    /// </summary>
    interface IWWWRequest
    {
        /// <summary>
        ///     Send a GET request.
        /// </summary>
        /// <remarks>
        ///     Do not cache the WWWResponse callback argument and do not use the WWWResponse object outside of the callback
        ///     function.
        /// </remarks>
        /// <returns>A request that can be cancelled.</returns>
        WWWRequestWrapper SendGetRequest(MonoBehaviour coroutineHost, string url, Action<WWWResponse> callback);

        /// <summary>
        ///     Send a POST request.
        /// </summary>
        /// <remarks>
        ///     Do not cache the WWWResponse callback argument and do not use the WWWResponse object outside of the callback
        ///     function.
        /// </remarks>
        /// <returns>A request that can be cancelled.</returns>
        WWWRequestWrapper SendPostRequest(MonoBehaviour coroutineHost, string url, byte[] postData, Dictionary<string, string> headers, Action<WWWResponse> callback);
    }
}
