// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Improbable.Assets;
using Improbable.Unity.Util;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    public class AssetBundleDownloader : IAssetLoader<AssetBundle>
    {
        private IMachineCache<byte[], AssetBundle> assetCache;
        private IMachineCache<CacheEntry, CacheEntry> metaDataCache;
        private string cachePath;
        private IWWWRequest wwwRequest;
        private MonoBehaviour coroutineHost;

        private readonly HashSet<WWWRequestWrapper> pendingDownloadRequests = new HashSet<WWWRequestWrapper>();

        /// <summary>
        ///     The method to invoke to resolve a prefab name to a URL. Throws a <see cref="KeyNotFoundException" /> if the asset
        ///     is unknown.
        /// </summary>
        public Func<string, string> GetAssetUrl { get; set; }

        public AssetBundleDownloader()
        {
            GetAssetUrl = s => { throw new InvalidOperationException("GetAssetUrl has not been assigned."); };
        }

        internal AssetBundleDownloader(string cachePath, IMachineCache<byte[], AssetBundle> assetCache, IMachineCache<CacheEntry, CacheEntry> metaDataCache, IWWWRequest wwwRequest, MonoBehaviour coroutineHost)
        {
            this.cachePath = cachePath;
            this.assetCache = assetCache;
            this.metaDataCache = metaDataCache;
            this.wwwRequest = wwwRequest;
            this.coroutineHost = coroutineHost;
        }

        /// <inheritdoc />
        public void LoadAsset(string prefabName, Action<AssetBundle> onAssetLoaded, Action<Exception> onError)
        {
            try
            {
                var assetUri = GetAssetUrl(prefabName);

                Action<WWWResponse> callback = (response) => { HandleDownloadAssetResponse(assetUri, onAssetLoaded, onError, response); };

                DownloadAsync(wwwRequest, assetUri, assetUri, callback);
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        }

        public void CancelAllLoads()
        {
            foreach (var requestId in pendingDownloadRequests)
            {
                requestId.Cancel();
            }

            pendingDownloadRequests.Clear();
        }

        private void HandleDownloadAssetResponse(string url, Action<AssetBundle> onAssetLoaded, Action<Exception> onError, WWWResponse response)
        {
            if (!String.IsNullOrEmpty(response.Error))
            {
                Debug.LogWarningFormat("Requesting asset '{0}' failed. Url: {1}. Error: {2}", GetAssetName(url), url, response.Error);
                onError(new ApplicationException(response.Error));
            }
            else if (response.ResponseHeaders.ContainsKey("STATUS") && response.ResponseHeaders["STATUS"].Contains("304"))
            {
                AssetBundle assetBundle;
                if (assetCache.TryGet(GetAssetName(url), out assetBundle))
                {
                    onAssetLoaded(assetBundle);
                }
                else
                {
                    Debug.LogErrorFormat("Failed to load asset '{0}' from cache. Corrupted cache. Please delete the cache folder: '{1}'.", GetAssetName(url), cachePath);
                    onError(new ApplicationException("Cache likely corrupted."));
                }
            }
            else if (response.AssetBundle != null)
            {
                UpdateCache(url, response);
                onAssetLoaded(response.AssetBundle);
            }
            else
            {
                string responseHeadersString = "";
                foreach (var responseHeader in response.ResponseHeaders)
                {
                    responseHeadersString += " [" + responseHeader.Key + ": " + responseHeader.Value + "]";
                }

                Debug.LogWarningFormat("Unhandled response for {0}. Downloaded {1} bytes, got the following response headers: {2}", url, response.BytesDownloaded, responseHeadersString);
                onError(new ApplicationException("Unhandled response."));
            }
        }

        private static string GetAssetName(string assetUrl)
        {
            return new Uri(assetUrl).PathAndQuery;
        }

        private void UpdateCache(string url, WWWResponse response)
        {
            var added = assetCache.TryAddOrUpdate(GetAssetName(url), response.Bytes);

            if (added)
            {
                AddMetaDataCacheEntry(url, response);
            }
            else
            {
                Debug.LogWarningFormat("Failed to cache: {0}", url);
            }
        }

        private void AddMetaDataCacheEntry(string url, WWWResponse response)
        {
            if (!HasValidators(response))
            {
                Debug.LogWarningFormat("ETag and/or Last-Modified missing. Cannot cache url {0}, keys {1}", url, string.Join("\n", response.ResponseHeaders.Keys.ToArray()));
            }
            else
            {
                var entry = CreateMetaDataCacheEntry(url, response);
                if (!metaDataCache.TryAddOrUpdate(GetAssetName(url), entry))
                {
                    Debug.LogWarningFormat("Failed to add cache metadata for: {0}", url);
                }
            }
        }

        private static bool HasValidators(WWWResponse response)
        {
            return response.ResponseHeaders.ContainsKey("LAST-MODIFIED") && response.ResponseHeaders.ContainsKey("ETAG");
        }

        private static CacheEntry CreateMetaDataCacheEntry(string url, WWWResponse response)
        {
            string responseHeader = response.ResponseHeaders["LAST-MODIFIED"];
            string entityTag = response.ResponseHeaders["ETAG"];
            var cacheEntry = new CacheEntry { EntityTag = entityTag, Modified = responseHeader, Url = url, LastFetched = DateTime.UtcNow };
            return cacheEntry;
        }

        private void DownloadAsync(IWWWRequest wwwRequest, string originalUrl, string redirectedUrl, Action<WWWResponse> callback)
        {
            WWWRequestWrapper requestWrapper;

            // It is possible for 'SendGetRequest' or 'SendPostRequest' to have completed immediately.
            // If this is the case, then we don't mark the request as pending.
            var responseReceived = false;

            Action<WWWResponse> responseHandler = (response) =>
            {
                responseReceived = true;
                DownloadAsyncResponse(response, wwwRequest, originalUrl, callback);
            };

            var headers = GetCacheHeaders(originalUrl);

            if (headers == null)
            {
                requestWrapper = wwwRequest.SendGetRequest(coroutineHost, redirectedUrl, responseHandler);
            }
            else
            {
                requestWrapper = wwwRequest.SendPostRequest(coroutineHost, redirectedUrl, null, headers, responseHandler);
            }

            if (!responseReceived)
            {
                pendingDownloadRequests.Add(requestWrapper);
            }
        }

        private void DownloadAsyncResponse(WWWResponse response, IWWWRequest wwwRequest, string originalUrl, Action<WWWResponse> callback)
        {
            pendingDownloadRequests.Remove(response.Request);

            var redirectUrl = GetRedirectUrl(response);
            if (redirectUrl != null)
            {
                DownloadAsync(wwwRequest, originalUrl, redirectUrl, callback);
            }
            else
            {
                callback(response);
            }
        }

        private Dictionary<string, string> GetCacheHeaders(string url)
        {
            CacheEntry entry;
            if (metaDataCache.TryGet(GetAssetName(url), out entry))
            {
                return new Dictionary<string, string>
                {
                    { "If-Modified-Since", entry.Modified },
                    { "If-None-Match", entry.EntityTag },
                };
            }

            return null;
        }

        private static string GetRedirectUrl(WWWResponse response)
        {
            string status = "";
            if (response.ResponseHeaders.TryGetValue("STATUS", out status))
            {
                if (status.Contains("302"))
                {
                    string location = "";
                    if (response.ResponseHeaders.TryGetValue("LOCATION", out location))
                    {
                        return location;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Requested {0} and got a 302, but no location header", response.Url);
                    }
                }
            }

            return null;
        }
    }

    public class AssetBundlePersistenceStrategy : MachineCache<byte[], AssetBundle>.IPersistenceStrategy
    {
        public void WriteToCacheFile(string outputCacheFile, byte[] resource)
        {
            File.WriteAllBytes(outputCacheFile, resource);
        }

        public AssetBundle ReadFromCacheFile(string inputCacheFile)
        {
            return AssetBundle.LoadFromFile(inputCacheFile);
        }
    }

    public class AssetMetadataPersistenceStrategy : MachineCache<CacheEntry, CacheEntry>.IPersistenceStrategy
    {
        private static readonly XmlSerializer metaDataSerializer = new XmlSerializer(typeof(Container));

        public CacheEntry ReadFromCacheFile(string filename)
        {
            using (var file = File.OpenRead(filename))
            {
                var container = (Container) metaDataSerializer.Deserialize(file);
                return container.CacheEntry;
            }
        }

        public void WriteToCacheFile(string filename, CacheEntry entry)
        {
            using (var file = File.CreateText(filename))
            {
                var container = new Container
                {
                    CacheEntry = entry
                };
                metaDataSerializer.Serialize(file, container);
            }
        }
    }

    public class CacheEntry
    {
        [XmlAttribute] public string EntityTag;

        [XmlAttribute] public DateTime LastFetched;
        [XmlAttribute] public string Modified;
        [XmlAttribute] public string Url;
    }

    [XmlRoot("Cache")]
    public class Container
    {
        [XmlElement] public CacheEntry CacheEntry;
    }
}
