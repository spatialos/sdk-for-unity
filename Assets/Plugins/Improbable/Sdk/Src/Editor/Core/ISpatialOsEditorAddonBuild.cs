// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity.Editor.Core
{
    /// <summary>
    ///     Allows an addon to present a user interface.
    /// </summary>
    public interface ISpatialOsEditorAddonBuild
    {
        /// <summary>
        ///     Called by the <see cref="EditorTools.SpatialOsWindow" /> when the addon is visible.
        /// </summary>
        void OnDevGui(Rect rect);
    }
}
