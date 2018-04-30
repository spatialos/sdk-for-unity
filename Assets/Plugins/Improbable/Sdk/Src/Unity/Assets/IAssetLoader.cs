// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Assets
{
    public interface IAssetLoader<TAsset>
    {
        /// <summary>
        ///     Downloads an asset and invokes the callback when succeeded.
        /// </summary>
        /// <param name="prefabName">the name of the prefab for which to load the asset.</param>
        /// <param name="onAssetLoaded">called after the asset has been loaded. This callback is called in the calling thread.</param>
        /// <param name="onError">called if for any reason the asset loading failed. This callback is called in the calling thread.</param>
        /// <remarks>
        ///     <para>This method is not blocking.</para>
        /// </remarks>
        void LoadAsset(string prefabName, Action<TAsset> onAssetLoaded, Action<Exception> onError);

        /// <summary>
        ///     Cancels all ongoing asset loading requests.
        /// </summary>
        void CancelAllLoads();
    }
}
