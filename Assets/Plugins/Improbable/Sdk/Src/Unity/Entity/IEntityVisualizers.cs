// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Manages the enabling and disabling of visualizers
    /// </summary>
    public interface IEntityVisualizers
    {
        /// <summary>
        ///     Returns a list of MonoBehaviours that use RequireAttribute to acquire Component readers and writers.
        /// </summary>
        IList<MonoBehaviour> ExtractedVisualizers { get; }

        /// <summary>
        ///     Called when an exception is caught during the process of enabling or disabling a MonoBehaviour.
        /// </summary>
        /// <remarks>
        ///     Defaults to Debug.LogException.
        /// </remarks>
        Action<Exception, UnityEngine.Object> OnUserException { get; set; }

        /// <summary>
        ///     Returns a set of required component ids.
        /// </summary>
        HashSet<uint> RequiredComponents { get; }

        /// <summary>
        ///     Add an IEntityInterestedComponentsInvalidator which listens to potential changes in interested components.
        /// </summary>
        void AddInvalidator(IEntityInterestedComponentsInvalidator invalidator);

        /// <summary>
        ///     Remove an IEntityInterestedComponentsInvalidator and stop it from getting notified about potential changes in
        ///     interested components.
        /// </summary>
        void RemoveInvalidator(IEntityInterestedComponentsInvalidator invalidator);

        /// <summary>
        ///     Manually disable visualizers.
        /// </summary>
        void DisableVisualizers(IList<MonoBehaviour> visualizersToDisable);

        /// <summary>
        ///     Tries to manually enable visualizers.
        /// </summary>
        void TryEnableVisualizers(IList<MonoBehaviour> visualizersToEnable);
    }
}
