// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity.Editor.Core
{
    /// <summary>
    ///     Allows an addon to present an interface in the development toolbar.
    /// </summary>
    public interface ISpatialOsEditorAddonToolbar
    {
        /// <summary>
        ///     Called by the <see cref="EditorTools.SpatialOsWindow" /> when the addon needs to render to the toolbar.
        /// </summary>
        void OnToolbarGui(Rect rect);
    }
}
