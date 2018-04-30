// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Util
{
    /// <summary>
    ///     Unit test wrapper for WWW response object.
    /// </summary>
    /// <remarks>
    ///     Do not cache this object for later use as internal fields may be released spontaneously.
    /// </remarks>
    class WWWResponse
    {
        private WWW www;

        private byte[] bytes;

        public byte[] Bytes
        {
            get { return bytes ?? (bytes = www.bytes); }
            set { bytes = value; }
        }

        private string error;

        public string Error
        {
            get { return error ?? (error = www.error); }
            set { error = value; }
        }

        private int bytesDownloaded = -1;

        public int BytesDownloaded
        {
            get
            {
                if (bytesDownloaded < 0)
                {
                    bytesDownloaded = www.bytesDownloaded;
                }

                return bytesDownloaded;
            }
            set { bytesDownloaded = value; }
        }

        private string url;

        public string Url
        {
            get { return url ?? (url = www.url); }
            set { url = value; }
        }

        private string text;

        public string Text
        {
            get { return text ?? (text = www.text); }
            set { text = value; }
        }

        private AssetBundle assetBundle;

        public AssetBundle AssetBundle
        {
            get { return assetBundle ?? (assetBundle = www.assetBundle); }
            set { assetBundle = value; }
        }

        private Dictionary<string, string> responseHeaders;
        public WWWRequestWrapper Request;

        public Dictionary<string, string> ResponseHeaders
        {
            get { return responseHeaders ?? (responseHeaders = www.responseHeaders); }
            set { responseHeaders = value; }
        }

        public static WWWResponse CreateFromWWW(WWW www)
        {
            return new WWWResponse
            {
                www = www
            };
        }
    }
}
