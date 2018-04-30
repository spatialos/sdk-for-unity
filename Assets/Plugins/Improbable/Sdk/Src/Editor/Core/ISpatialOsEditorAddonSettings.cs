// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity.Editor.Core
{
    /// <summary>
    ///     Allows an addon to present an interface for modifying its settings.
    /// </summary>
    public interface ISpatialOsEditorAddonSettings
    {
        /// <summary>
        ///     Called by the <see cref="EditorTools.SpatialOsWindow" /> when the addon needs to render settings.
        /// </summary>
        void OnSettingsGui(Rect rect);
    }
}
