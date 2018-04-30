// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Assets
{
    /// <summary>
    ///     Maintains a cache of assets
    /// </summary>
    public interface IAssetDatabase<TGameObject> : IAssetLoader<TGameObject>
    {
        /// <summary>
        ///     Returns <c>true</c> and sets the prefab game object output parameter to a valid instance if the prefab has been
        ///     successfully loaded via <see cref="LoadAsset" />.
        ///     If the prefab has not been loaded yet, then this method returns <c>false</c> and sets the prefab game object output
        ///     parameter to <c>null</c>.
        /// </summary>
        /// <param name="prefabName">the name of the prefab to obtain</param>
        /// <param name="prefabGameObject">the prefab game object</param>
        /// <returns>true if successful, otherwise false</returns>
        /// <remarks>
        ///     <para>Subsequent calls to this method will return the same prefab game object instance (due to caching).</para>
        ///     <para>You must clone the returned prefab game object to make your own instance.</para>
        /// </remarks>
        bool TryGet(string prefabName, out TGameObject prefabGameObject);
    }
}
