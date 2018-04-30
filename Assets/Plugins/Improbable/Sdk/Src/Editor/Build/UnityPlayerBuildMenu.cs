// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Build
{
    [InitializeOnLoad]
    public static class UnityPlayerBuilderMenu
    {
        private const string AutopatchMenuItem = "Improbable/Autopatch workers on editor reload";
        private const string AutoPatchEditorKey = "Improbable.Autopatch.Workers";

        [MenuItem("Improbable/Build/Build deployment workers %#&3")]
        public static void BuildDeploymentPlayers()
        {
            BuildPlayers(UnityPlayerBuilders.DeploymentTarget);
        }

        [MenuItem("Improbable/Build/Build development workers %#&2")]
        public static void BuildDevelopmentPlayers()
        {
            BuildPlayers(UnityPlayerBuilders.DevelopmentTarget);
        }

        [MenuItem("Improbable/Build/Clean all workers %#&1")]
        public static void CleanAllPlayers()
        {
            SimpleBuildSystem.CleanPlayers();
        }

        public static bool IsAutopatchEnabled()
        {
            return EditorPrefs.GetBool(AutoPatchEditorKey, defaultValue: false);
        }

        [MenuItem(AutopatchMenuItem, true)]
        public static bool ValidateAutoPatch()
        {
            // Ensure this is always up-to-date
            Menu.SetChecked(AutopatchMenuItem, IsAutopatchEnabled());
            return true;
        }

        [MenuItem(AutopatchMenuItem)]
        public static void ToggleAutopatch()
        {
            var toggledValue = !IsAutopatchEnabled();
            EditorPrefs.SetBool(AutoPatchEditorKey, toggledValue);

            Debug.LogFormat("Auto-patching workers is {0}", toggledValue ? "enabled" : "disabled");

            if (IsAutopatchEnabled())
            {
                ReloadAssemblies.OnScriptsReloaded();
            }
        }

        private static void BuildPlayers(string target)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            try
            {
                SimpleBuildSystem.WorkersToBuild = SimpleBuildSystem.AllWorkerTypes;
                SimpleBuildSystem.Target = target;
                SimpleBuildSystem.Build();
            }
            finally
            {
                SimpleBuildSystem.WorkersToBuild = null;
                SimpleBuildSystem.Target = null;

                stopWatch.Stop();
            }
        }
    }
}
