// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     An IEntityTemplateProvider can look up a GameObject to use as a template for a prefabName.
    /// </summary>
    public interface IEntityTemplateProvider
    {
        /// <summary>
        ///     PrepareTemplate is an asynchronous method guaranteed to be called at least once before the GameObject template
        ///     required for a particular prefabName is requested.
        ///     Implementors must call onSuccess once the IEntityTemplateProvider is ready to accept GetEntityTemplate calls, and
        ///     onError if it was unable to get ready.
        /// </summary>
        /// <param name="prefabName">The prefabName of the entity.</param>
        /// <param name="onSuccess">the continuation to call if preparation for the entity asset was successful.</param>
        /// <param name="onError">the continuation to call if preparation for the entity asset failed.</param>
        void PrepareTemplate(string prefabName, Action<string> onSuccess, Action<Exception> onError);

        /// <summary>
        ///     CancelAllTemplatePreparations cancels all active <see cref="PrepareTemplate" /> operations.
        ///     The onSuccess and onError callbacks for these operations should not be called.
        /// </summary>
        void CancelAllTemplatePreparations();

        /// <summary>
        ///     GetEntityTemplate must return a template GameObject that will be instantiated to make new instances of entities
        ///     with the same prefabName.
        ///     Subsequent calls with the same prefabName should return the same GameObject.
        ///     PrepareTemplate will always have been called at least once with this prefabName.
        /// </summary>
        /// <param name="prefabName">The prefab name of the entity.</param>
        /// <returns>A GameObject respresenting the prefab with the given prefabName.</returns>
        GameObject GetEntityTemplate(string prefabName);
    }
}
