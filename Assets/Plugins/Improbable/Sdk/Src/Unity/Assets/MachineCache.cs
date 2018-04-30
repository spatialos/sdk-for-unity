// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Improbable.Assets
{
    /// <summary>
    ///     A filesystem-based cache. It imports resources of one type and returns them in another.
    ///     This implementation attempts to use atomic file operations to ensure consistency.
    /// </summary>
    /// <typeparam name="TIn">the type of resource to be stored in the cache (stored as a file).</typeparam>
    /// <typeparam name="TOut">the type of resource to be retrieved from the cache (mapped from the cached resource file).</typeparam>
    /// <remarks>
    ///     The implementation is heavily inspired by Nuget's repository cache which can be found here:
    ///     https://github.com/Haacked/NuGet/blob/9f25709fafb0d8e8fc2a3b34c1b77a1cb4b8a539/src/Core/Repositories/MachineCache.cs
    /// </remarks>
    public class MachineCache<TIn, TOut> : IMachineCache<TIn, TOut>
    {
        private static readonly TimeSpan MutexWaitTime = TimeSpan.FromMinutes(3);
        private readonly IPersistenceStrategy persistenceStrategy;

        private string CachePath { get; set; }

        public MachineCache(string cachePath, IPersistenceStrategy persistenceStrategy)
        {
            this.persistenceStrategy = persistenceStrategy;
            CachePath = cachePath;
            CreateCache();
        }

        /// <summary>
        ///     Attempt to Add an item to the cache. Will not update existing entries.
        /// </summary>
        /// <param name="key">the key used for later retrieval</param>
        /// <param name="cacheItem">the item to be cached</param>
        /// <returns>true if successful otherwise false</returns>
        public bool TryAdd(string key, TIn cacheItem)
        {
            var path = GetFilePath(key);
            if (!File.Exists(path))
            {
                AtomicWriteToCache(cacheItem, path);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Attempt to Add an item to the cache. Will update existing entries.
        /// </summary>
        /// <param name="key">the key used for later retrieval</param>
        /// <param name="cacheItem">the item to be cached</param>
        /// <returns>true if successful otherwise false</returns>
        public bool TryAddOrUpdate(string key, TIn cacheItem)
        {
            try
            {
                var path = GetFilePath(key);
                AtomicWriteToCache(cacheItem, path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Attempt to add or update an item to the cache failed.\n{0} = {1}\n{2}", key, cacheItem, ex);
                return false;
            }
        }

        /// <summary>
        ///     Attempt to get an item from the cache
        /// </summary>
        /// <param name="key">the key of the item to retrieve</param>
        /// <param name="result">the item from the cache</param>
        /// <returns>true if successful otherwise false</returns>
        public bool TryGet(string key, out TOut result)
        {
            try
            {
                var path = GetFilePath(key);
                result = persistenceStrategy.ReadFromCacheFile(path);
                return true;
            }
            catch (Exception)
            {
                result = default(TOut);
                return false;
            }
        }

        private string GetFilePath(string key)
        {
            return Path.Combine(CachePath, Uri.EscapeDataString(key));
        }

        private void CreateCache()
        {
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }
        }

        private void AtomicWriteToCache(TIn cacheItem, string path)
        {
            var tmp = Path.GetTempFileName();
            persistenceStrategy.WriteToCacheFile(tmp, cacheItem);
            TryAct(() =>
            {
                if (!File.Exists(path))
                {
                    File.Move(tmp, path);
                }
                else
                {
                    var backup = Path.GetTempFileName();
                    File.Replace(tmp, path, backup);
                    File.Delete(backup);
                }

                return true;
            }, path);
        }

        private static string GenerateUniqueToken(string caseInsensitiveKey)
        {
            // We need something that is stable across all platforms and processes.
            // Use an adaption of the mscorlib's string hash.
            // http://referencesource.microsoft.com/#mscorlib/system/string.cs,827
            var pathBytes = Encoding.UTF8.GetBytes(caseInsensitiveKey.ToUpperInvariant());

            // Disable overflow checking, we're not doing "real" math here.
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < pathBytes.Length; i++)
                {
                    var c = pathBytes[i];
                    hash1 = ((hash1 << 5) + hash1) ^ c;
                    if (i == pathBytes.Length - 1 || pathBytes[i + 1] == '\0')
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ pathBytes[i + 1];
                }

                return (hash1 + (hash2 * 1566083941)).ToString();
            }
        }

        /// <remarks>
        ///     We use this method instead of the "safe" methods in FileSystem because it attempts to retry multiple times with
        ///     delays.
        ///     In our case, if we are unable to perform IO over the machine cache, we want to quit trying immediately.
        /// </remarks>
        private static void TryAct(Func<bool> action, string path)
        {
            try
            {
                // Global: machine cache is per user across TS sessions
                var mutexName = "Global\\" + GenerateUniqueToken(Path.GetFullPath(path));
                using (var mutex = new Mutex(false, mutexName))
                {
                    bool owner = false;
                    try
                    {
                        try
                        {
                            owner = mutex.WaitOne(MutexWaitTime);
                            // ideally we should throw an exception here if !owner such as
                            // throw new TimeoutException(string.Format("Timeout waiting for Machine Cache mutex for {0}", fullPath));
                            // we decided against it: machine cache operations being "best effort" basis.
                            // this may cause "File in use" exceptions for long lasting operations such as downloading a large package on
                            // a slow network connection
                        }
                        catch (AbandonedMutexException)
                        {
                            // TODO: consider logging a warning; abandoning a mutex is an indication something wrong is going on
                            owner = true; // now mine
                        }

                        action();
                    }
                    finally
                    {
                        if (owner)
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Do nothing if this fails.
            }
        }

        /// <summary>
        ///     The strategy that describes how the cache item is persisted and retrieved from disk
        /// </summary>
        public interface IPersistenceStrategy
        {
            /// <summary>
            ///     Called when the cache is persisting the item to disk
            /// </summary>
            /// <param name="outputCacheFile">the file to write to</param>
            /// <param name="resource"></param>
            /// <remarks> Note: outputCacheFile may be a temporary file and should not be used later for any reason.</remarks>
            void WriteToCacheFile(string outputCacheFile, TIn resource);

            /// <summary>
            ///     Load and deserialize a cache item
            /// </summary>
            /// <param name="inputCacheFile">the filename to read from</param>
            /// <returns>The cache item</returns>
            TOut ReadFromCacheFile(string inputCacheFile);
        }
    }
}
