// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Util
{
    class WWWRequestWrapper
    {
        public bool IsCancelled { get; private set; }

        private readonly string url;

        private readonly bool isPost;
        private readonly byte[] postData;
        private readonly Dictionary<string, string> headers;

        public WWWRequestWrapper(string url)
        {
            this.url = url;

            isPost = false;
        }

        public WWWRequestWrapper(string url, byte[] postData, Dictionary<string, string> headers)
        {
            this.url = url;
            this.postData = postData;
            this.headers = headers;

            isPost = true;
        }

        public WWW CreateWww()
        {
            if (isPost)
            {
                return new WWW(url, postData, headers);
            }

            return new WWW(url);
        }

        public void Cancel()
        {
            IsCancelled = true;
        }
    }
}
