// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity
{
    /// <summary>
    ///     Implementing class can enable or disable MonoBehaviours.
    /// </summary>
    /// <remarks>
    ///     Used in <see cref="Improbable.Unity.Visualizer.EntityVisualizers" />.
    /// </remarks>
    interface IMonobehaviourActivator
    {
        /// <summary>
        ///     Enable MonoBehaviour.
        /// </summary>
        void Enable(MonoBehaviour monoBehaviour);

        /// <summary>
        ///     Disable MonoBehaviour.
        /// </summary>
        void Disable(MonoBehaviour monoBehaviour);
    }
}
