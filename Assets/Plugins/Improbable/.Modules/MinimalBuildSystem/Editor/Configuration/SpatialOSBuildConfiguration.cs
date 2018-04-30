// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.MinimalBuildSystem.Configuration
{
    [CreateAssetMenu(fileName = "SpatialOS Build Configuration", menuName = CreateMenuPath)]
    public class SpatialOSBuildConfiguration : SingletonScriptableObject<SpatialOSBuildConfiguration>
    {
        internal const string CreateMenuPath = "SpatialOS/Build Configuration";

        [SerializeField] private bool isInitialised;

        public WorkerBuildConfiguration[] WorkerBuildConfigurations;

        public override void OnEnable()
        {
            base.OnEnable();

            if (!isInitialised)
            {
                ResetToDefault();
            }

            if (IsAnAsset())
            {
                UpdateEditorScenesForBuild();
            }
        }

        private void ResetToDefault()
        {
            // Build default settings
            var client = new WorkerBuildConfiguration()
            {
                WorkerPlatform = WorkerPlatform.UnityClient,
                ScenesForWorker = AssetDatabase.FindAssets("t:Scene")
                                               .Select(AssetDatabase.GUIDToAssetPath)
                                               .Where(path => path.Contains(WorkerPlatform.UnityClient.ToString()))
                                               .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>).ToArray(),
                LocalBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = SpatialBuildPlatforms.Current,
                    BuildOptions = BuildOptions.Development
                },
                CloudBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = SpatialBuildPlatforms.Current
                }
            };

            var worker = new WorkerBuildConfiguration()
            {
                WorkerPlatform = WorkerPlatform.UnityWorker,
                ScenesForWorker = AssetDatabase.FindAssets("t:Scene")
                                               .Select(AssetDatabase.GUIDToAssetPath)
                                               .Where(path => path.Contains(WorkerPlatform.UnityWorker.ToString()))
                                               .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>).ToArray(),
                LocalBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = SpatialBuildPlatforms.Current,
                    BuildOptions = BuildOptions.EnableHeadlessMode
                },
                CloudBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = SpatialBuildPlatforms.Linux,
                    BuildOptions = BuildOptions.EnableHeadlessMode
                }
            };

            WorkerBuildConfigurations = new WorkerBuildConfiguration[]
            {
                client, worker
            };

            isInitialised = true;
        }

        private void OnValidate()
        {
            if (!isInitialised)
            {
                ResetToDefault();
            }

            if (IsAnAsset())
            {
                UpdateEditorScenesForBuild();
            }
        }

        private SceneAsset[] GetScenesForWorker(WorkerPlatform workerPlatform)
        {
            WorkerBuildConfiguration configurationForWorker = null;

            if (WorkerBuildConfigurations != null)
            {
                configurationForWorker =
                    WorkerBuildConfigurations.FirstOrDefault(config => config.WorkerPlatform == workerPlatform);
            }

            return configurationForWorker == null
                ? new SceneAsset[0]
                : configurationForWorker.ScenesForWorker;
        }

        internal void UpdateEditorScenesForBuild()
        {
            EditorApplication.delayCall += () =>
            {
                EditorBuildSettings.scenes =
                    GetScenesForWorker(WorkerPlatform.UnityClient)
                        .Union(GetScenesForWorker(WorkerPlatform.UnityWorker))
                        .Select(AssetDatabase.GetAssetPath)
                        .Select(scenePath => new EditorBuildSettingsScene(scenePath, true))
                        .ToArray();
            };
        }

        public BuildEnvironmentConfig GetEnvironmentConfigForWorker(WorkerPlatform platform,
                                                                    BuildEnvironment targetEnvironment)
        {
            var config = WorkerBuildConfigurations.FirstOrDefault(x => x.WorkerPlatform == platform);
            if (config == null)
            {
                throw new ArgumentException("Unknown WorkerPlatform " + platform);
            }

            return config.GetEnvironmentConfig(targetEnvironment);
        }

        public string[] GetScenePathsForWorker(WorkerPlatform workerType)
        {
            return GetScenesForWorker(workerType)
                   .Where(sceneAsset => sceneAsset != null)
                   .Select(AssetDatabase.GetAssetPath)
                   .ToArray();
        }
    }
}
