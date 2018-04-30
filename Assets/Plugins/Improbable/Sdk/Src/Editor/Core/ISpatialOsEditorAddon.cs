// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Editor.Core
{
    /// <summary>
    ///     Implements a SpatialOS Addon for the Unity Editor.
    /// </summary>
    public interface ISpatialOsEditorAddon
    {
        /// <summary>
        ///     The name of the editor, as displayed to the user.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The name of the vendor, as displayed to the user.
        /// </summary>
        string Vendor { get; }
    }
}
