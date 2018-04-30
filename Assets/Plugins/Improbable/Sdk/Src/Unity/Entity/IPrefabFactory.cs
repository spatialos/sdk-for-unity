// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Provides methods to create and destroy entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPrefabFactory<T>
    {
        /// <summary>
        ///     Instantiates a GameObject from the given prefab.
        /// </summary>
        /// <param name="prefabGameObject">The prefab to instantiate.</param>
        /// <param name="prefabName">The prefab name.</param>
        /// <returns>A new instance of the given prefabGameObject.</returns>
        T MakeComponent(T prefabGameObject, string prefabName);

        /// <summary>
        ///     Destroys an existing GameObject.
        /// </summary>
        /// <param name="gameObject">The object that is in the game.</param>
        /// <param name="prefabName">The prefab name of the prefab that the object was instantiated from.</param>
        void DespawnComponent(T gameObject, string prefabName);
    }
}
