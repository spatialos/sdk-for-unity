// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Improbable.Unity.EditorTools.Build
{
    /// <summary>
    ///     Opens a single Unity scene per worker type.
    /// </summary>
    public class SingleScenePlayerBuildEvents : IPlayerBuildEvents
    {
        // During the build we will change the scene - remember so we can change it back.
        private string savedScenePath;

        private Dictionary<WorkerPlatform, string> workerToScene = new Dictionary<WorkerPlatform, string>();

        public SingleScenePlayerBuildEvents()
        {
            workerToScene[WorkerPlatform.UnityClient] = Path.Combine("Assets", "ClientScene.unity");
            workerToScene[WorkerPlatform.UnityWorker] = Path.Combine("Assets", "PhysicsServerScene.unity");
        }

        /// <summary>
        ///     Provides a mapping from the worker type to a path to a scene to load.
        /// </summary>
        public Dictionary<WorkerPlatform, string> WorkerToScene
        {
            get { return workerToScene; }
            set { workerToScene = value; }
        }

        /// <inheritdoc cref="IPlayerBuildEvents" />
        public virtual void BeginBuild()
        {
            savedScenePath = EditorSceneManager.GetActiveScene().path;
        }

        /// <inheritdoc cref="IPlayerBuildEvents" />
        public virtual void EndBuild()
        {
            // Restore the previous scene.
            if (!string.IsNullOrEmpty(savedScenePath))
            {
                EditorSceneManager.OpenScene(savedScenePath, OpenSceneMode.Single);
            }
        }

        /// <inheritdoc cref="IPlayerBuildEvents" />
        public virtual string[] GetScenes(WorkerPlatform workerType)
        {
            var scenePath = workerToScene[workerType];
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ProcessScene(scene.name);

            return new[] { scenePath };
        }

        /// <inheritdoc cref="IPlayerBuildEvents" />
        public virtual void BeginPackage(WorkerPlatform workerType, BuildTarget target, Config config, string packagePath)
        {
#pragma warning disable 0618
            UnityPlayerBuilder.GetPackager(workerType, target, config).Prepare(packagePath);
#pragma warning restore 0618
        }

        /// <summary>
        ///     Override to modify the scene before building.
        /// </summary>
        /// <remarks>This is called just after the scene is loaded by <see cref="GetScenes" />.</remarks>
        /// <param name="sceneName">The name of the scene to modify</param>
        public virtual void ProcessScene(string sceneName)
        {
#pragma warning disable 0618            
            UnityPlayerBuilder.ProcessScene(sceneName);
#pragma warning restore 0618
        }
    }
}
