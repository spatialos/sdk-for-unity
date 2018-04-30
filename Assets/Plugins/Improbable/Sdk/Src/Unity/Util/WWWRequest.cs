// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Util
{
    class WWWRequest : IWWWRequest
    {
        /// <inheritdoc />
        public WWWRequestWrapper SendGetRequest(MonoBehaviour coroutineHost, string url, Action<WWWResponse> callback)
        {
            var requestWrapper = new WWWRequestWrapper(url);

            coroutineHost.StartCoroutine(SendRequestCoroutine(requestWrapper, callback));

            return requestWrapper;
        }

        /// <inheritdoc />
        public WWWRequestWrapper SendPostRequest(MonoBehaviour coroutineHost, string url, byte[] postData, Dictionary<string, string> headers, Action<WWWResponse> callback)
        {
            var requestWrapper = new WWWRequestWrapper(url, postData, headers);

            coroutineHost.StartCoroutine(SendRequestCoroutine(requestWrapper, callback));

            return requestWrapper;
        }

        private IEnumerator SendRequestCoroutine(WWWRequestWrapper requestWrapper, Action<WWWResponse> callback)
        {
            if (requestWrapper.IsCancelled)
            {
                yield break;
            }

            using (var www = requestWrapper.CreateWww())
            {
                bool isCancelled;

                while (true)
                {
                    if (requestWrapper.IsCancelled)
                    {
                        isCancelled = true;
                        break;
                    }

                    if (www.isDone)
                    {
                        isCancelled = false;
                        break;
                    }

                    // Wait one frame
                    yield return null;
                }

                if (!isCancelled)
                {
                    var wwwResponse = WWWResponse.CreateFromWWW(www);
                    wwwResponse.Request = requestWrapper;
                    callback(wwwResponse);
                }
            }
        }
    }
}
