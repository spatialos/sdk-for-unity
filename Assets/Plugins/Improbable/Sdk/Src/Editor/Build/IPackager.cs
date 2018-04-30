// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.EditorTools.Build
{
    [Obsolete("Obsolete in 10.3.0. Please see IPlayerBuildEvents for information about customizing player packaging.")]
    public interface IPackager
    {
        /// <param name="packagePath">the working directory that unity has built into.</param>
        /// <remarks>
        ///     An IPackager takes a built Unity player and calls Prepare with the path where it's located.
        ///     It gives you the opportunity to copy over any extra files that the worker needs to be run,
        ///     before it is packaged, ready for consumption by SpatialOS.
        ///     To configure the IPackager in use, set the UnityPlayerBuilder.GetPackager function to return your custom packager.
        /// </remarks>
        void Prepare(string packagePath);
    }
}
