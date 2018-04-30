// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Assets
{
    /// <summary>
    ///     Unit test wrapper for MachineCache
    ///     ///
    /// </summary>
    interface IMachineCache<TIn, TOut>
    {
        bool TryAdd(string key, TIn cacheItem);
        bool TryAddOrUpdate(string key, TIn cacheItem);
        bool TryGet(string key, out TOut result);
    }
}
