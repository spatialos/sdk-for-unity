// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEditor;

namespace Improbable.Unity.EditorTools.Build
{
    /// <summary>
    ///     Inherit from this interface to provide custom behavior related to the SpatialOS player building process.
    /// </summary>
    public interface IPlayerBuildEvents
    {
        /// <summary>
        ///     Called before any players are built.
        /// </summary>
        void BeginBuild();

        /// <summary>
        ///     Called after all players are built.
        /// </summary>
        /// <remarks>
        ///     This will always be called, even if all players are not successfully built.
        /// </remarks>
        void EndBuild();

        /// <summary>
        ///     Called between <see cref="BeginBuild" /> and <see cref="EndBuild" />, for each worker type that is built.
        ///     Please reference <see cref="BuildPlayerOptions.scenes" /> for more information about how the returned array of
        ///     scenes is used by Unity's build process.
        /// </summary>
        /// <param name="workerType">The type of the worker being built.</param>
        /// <returns>An array of all scenes that should be included in the build.</returns>
        string[] GetScenes(WorkerPlatform workerType);

        /// <summary>
        ///     Called between <see cref="BeginBuild" /> and <see cref="EndBuild" />, for each worker type that is packaged.
        ///     Implement this to modify the contents of the <paramref name="packagePath" /> directory before packaging.
        /// </summary>
        /// <param name="workerType">The type of the worker being packaged.</param>
        /// <param name="target">The target platform for the player being packaged.</param>
        /// <param name="config">The configuration associated with the player being packaged.</param>
        /// <param name="packagePath">The path that will be packaged.</param>
        void BeginPackage(WorkerPlatform workerType, BuildTarget target, Config config, string packagePath);
    }
}
