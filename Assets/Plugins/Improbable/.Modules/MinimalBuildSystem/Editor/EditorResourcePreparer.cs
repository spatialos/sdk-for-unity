// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.MinimalBuildSystem.Prefabs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Improbable.Unity.MinimalBuildSystem
{
    public static class EditorResourcePreparer
    {
        [InitializeOnLoadMethod]
        static void SetupPlaymodeCallbacks()
        {
            EditorApplication.playModeStateChanged += PrepareEntityPrefabs;
            EditorSceneManager.sceneOpened += SafetyCheckForPlatform;
        }

        private static void SafetyCheckForPlatform(Scene scene, OpenSceneMode mode)
        {
            // As the platform is not set by default, show a warning to the user when they still need to change this
            var provider = Object.FindObjectOfType<BasicTemplateProvider>();
            if (provider != null)
            {
                var workerPlatform = provider.platform;
                if (workerPlatform == 0)
                {
                    Debug.LogWarning("The BasicTemplateProvider instance has an invalid WorkerPlatform set.", provider);
                }
            }
        }

        private static void PrepareEntityPrefabs(PlayModeStateChange state)
        {
            // Only do this when we go from edit mode to play mode
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            // Only export if the scene uses a BasicTemplateProvider
            var provider = Object.FindObjectOfType<BasicTemplateProvider>();
            if (provider != null)
            {

                // Get worker platform
                var workerPlatform = provider.platform;
                Debug.LogFormat("Preparing EntityPrefabs for {0}", workerPlatform.ToString());

                // Start prefab export
                EntityPrefabs.Export(workerPlatform);
            }
        }
    }
}
