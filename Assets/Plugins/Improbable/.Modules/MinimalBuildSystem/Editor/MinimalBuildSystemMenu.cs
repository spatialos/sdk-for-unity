// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using Improbable.Unity.MinimalBuildSystem.Configuration;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.MinimalBuildSystem
{
    static class MinimalBuildSystemMenu
    {
        private const string MinimalBuildMenu = "Improbable/Experimental Build";

        [MenuItem(MinimalBuildMenu + "/Build UnityClient for local", false, 1)]
        public static void BuildLocalClient()
        {
            MenuBuild(new[] { WorkerPlatform.UnityClient }, BuildEnvironment.Local);
        }

        [MenuItem(MinimalBuildMenu + "/Build UnityWorker for local", false, 2)]
        public static void BuildLocalWorker()
        {
            MenuBuild(new[] { WorkerPlatform.UnityWorker }, BuildEnvironment.Local);
        }

        [MenuItem(MinimalBuildMenu + "/Build all workers for local", false, 3)]
        public static void BuildLocalAll()
        {
            MenuBuild(new[] { WorkerPlatform.UnityWorker, WorkerPlatform.UnityClient }, BuildEnvironment.Local);
        }

        [MenuItem(MinimalBuildMenu + "/Build UnityClient for cloud", false, 14)]
        public static void BuildCloudClient()
        {
            MenuBuild(new[] { WorkerPlatform.UnityClient }, BuildEnvironment.Cloud);
        }

        [MenuItem(MinimalBuildMenu + "/Build UnityWorker for cloud", false, 15)]
        public static void BuildCloudWorker()
        {
            MenuBuild(new[] { WorkerPlatform.UnityWorker }, BuildEnvironment.Cloud);
        }

        [MenuItem(MinimalBuildMenu + "/Build all workers for cloud", false, 16)]
        public static void BuildCloudAll()
        {
            MenuBuild(new[] { WorkerPlatform.UnityClient, WorkerPlatform.UnityWorker }, BuildEnvironment.Cloud);
        }

        [MenuItem(MinimalBuildMenu + "/Clean all workers", false, 27)]
        public static void Clean()
        {
            WorkerBuilder.Clean();
            Debug.Log("Clean completed");
        }

        private static void MenuBuild(IEnumerable<WorkerPlatform> platforms, BuildEnvironment environment)
        {
            // Delaying build by a frame to ensure the editor has re-rendered the UI to avoid odd glitches.
            EditorApplication.delayCall += () =>
            {
                Debug.Log("Generating build configuration");
                SpatialCommands.GenerateBuildConfiguration();
                foreach (var platform in platforms)
                {
                    WorkerBuilder.BuildWorkerForEnvironment(platform, environment);
                }

                Debug.LogFormat("Completed build for {0} target", environment);
            };
        }
    }
}
