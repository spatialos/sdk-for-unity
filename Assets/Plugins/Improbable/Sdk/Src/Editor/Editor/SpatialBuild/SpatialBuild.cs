// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Unity.Editor.Addons;
using Improbable.Unity.Editor.Core;
using Improbable.Unity.EditorTools.Build;
using Improbable.Unity.EditorTools.PrefabExport;
using Improbable.Unity.EditorTools.Util;
using Improbable.Unity.Util;
using UnityEditor;
using UnityEngine;

namespace Improbable.Editor.Addons.SpatialBuild
{
    [InitializeOnLoad]
    public class SpatialBuild : ISpatialOsEditorAddon, ISpatialOsEditorAddonSettings, ISpatialOsEditorAddonBuild
    {
        private readonly string[] configurations = { DevelopmentTarget, DeploymentTarget };

        private int selectedConfiguration;

        private IList<string> buildTargets;
        private string selectedWorkerString;

        internal const string DeploymentTarget = "Deployment";
        internal const string DevelopmentTarget = "Development";

        private readonly string selectedWorkerTargetPath = PathUtil.Combine(SpatialOsEditor.WorkerRootDir, "build", "player_build_target.txt");

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Name
        {
            get { return "Build"; }
        }

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Vendor
        {
            get { return "Improbable Worlds, Ltd."; }
        }

        private string SelectedConfiguration
        {
            get { return configurations[selectedConfiguration]; }
        }

        internal SpatialBuild()
        {
            try
            {
                var sourceText = File.ReadAllText(selectedWorkerTargetPath);
                selectedConfiguration = System.Math.Max(0, System.Math.Min(int.Parse(sourceText), configurations.Length - 1));
            }
            catch { }

            SpatialOsEditor.WorkerSelection.OnWorkerSelectionChanged += WorkerSelectionChanged;
        }

        private void WorkerSelectionChanged(HashSet<SpatialOsWorker> spatialOsWorkers)
        {
            buildTargets = null;
        }

        private void EditorUpdate()
        {
            EditorApplication.update -= EditorUpdate;
            BuildWorkers();
        }

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public void OnDevGui(Rect rect)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                DrawWorkerTargetSelection();

                using (new EditorGUI.DisabledScope(!SpatialOsEditor.WorkerSelection.AnyWorkersSelected || buildTargets == null || buildTargets.Count == 0))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("Generate from schema", EditorStyles.boldLabel);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(new GUIContent("Build", "Generate code for the currently selected workers."), EditorStyles.toolbarButton))
                            {
                                CodegenWorkers();
                            }

                            if (GUILayout.Button(new GUIContent("Cleanup", "Generate code for the currently selected workers."), EditorStyles.toolbarButton))
                            {
                                CleanupCodegen();
                            }
                        }
                    }

                    EditorGUILayout.Separator();

                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("Entity prefabs", EditorStyles.boldLabel);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(new GUIContent("Build all", "Build entity prefabs for the selected workers and targets."), EditorStyles.toolbarButton))
                            {
                                ExportPrefabs();
                            }

                            using (new EditorGUI.DisabledScope(!EntityPrefabExporter.AnyPrefabsSelected()))
                            {
                                if (GUILayout.Button(new GUIContent("Build selected", "Build selected entity prefabs for the selected workers and targets."), EditorStyles.toolbarButton))
                                {
                                    ExportSelectedPrefabs();
                                }
                            }

                            if (GUILayout.Button("Cleanup all", EditorStyles.toolbarButton))
                            {
                                EntityPrefabExportMenus.OnCleanEntityPrefabs();
                            }
                        }
                    }

                    EditorGUILayout.Separator();

                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("Workers", EditorStyles.boldLabel);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(new GUIContent("Build", "Build the currently selected workers."), EditorStyles.toolbarButton))
                            {
                                EditorApplication.update += EditorUpdate;
                            }

                            if (GUILayout.Button(new GUIContent("Cleanup all", "Clean all workers."), EditorStyles.toolbarButton))
                            {
                                UnityPathUtil.EnsureDirectoryClean(UnityPlayerBuilder.PlayerBuildDirectory);
                                UnityPathUtil.EnsureDirectoryClean(UnityPlayerBuilder.PlayerBuildScratchDirectory);
                            }
                        }
                    }
                }
            }
        }

        public void OnSettingsGui(Rect rect)
        {
            var patchingEnabled = UnityPlayerBuilderMenu.IsAutopatchEnabled();
            if (patchingEnabled != EditorGUILayout.ToggleLeft("Autopatch workers", patchingEnabled))
            {
                UnityPlayerBuilderMenu.ToggleAutopatch();
            }
        }

        private void BuildWorkers()
        {
            var workersToBuild = GetSelectedWorkerNames();
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            try
            {
                switch (SelectedConfiguration)
                {
                    case DevelopmentTarget:
                        UnityPlayerBuilders.BuildDevelopmentPlayers(workersToBuild);
                        break;
                    case DeploymentTarget:
                        UnityPlayerBuilders.BuildDeploymentPlayers(workersToBuild);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("{0} is an unknown build target.", SelectedConfiguration));
                }
            }
            finally
            {
                sw.Stop();
            }
        }

        private static void CodegenWorkers()
        {
            var selectedWorkers = string.Join(" ", SpatialOsEditor.WorkerSelection.SelectedWorkers.Select(w => w.Name).ToArray());
            SpatialOsEditor.RunPausedProcess(SpatialCommand.SpatialPath, "codegen " + selectedWorkers, SpatialOsEditor.WorkerRootDir);
        }

        private static void CleanupCodegen()
        {
            var codegenOutputDir = PathUtil.Combine(Directory.GetCurrentDirectory(), EditorPaths.CodeGeneratorScratchDirectory).ToUnityPath();
            if (!Directory.Exists(codegenOutputDir))
            {
                Debug.LogFormat("Code generator output directory '{0}' does not exist. Nothing to clean.", codegenOutputDir);
                return;
            }

            try
            {
                Debug.LogFormat("Removing code generator output from '{0}'.", codegenOutputDir);
                Directory.Delete(codegenOutputDir, true);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Exception was thrown when cleaning codegen output.");
                Debug.LogException(e);
            }
        }

        private void ExportPrefabs()
        {
            var targets = GetWorkerBuilders();
            foreach (var target in targets)
            {
                EntityPrefabExporter.ExportAllEntityPrefabs(target.BuildTarget);
            }
        }

        private void ExportSelectedPrefabs()
        {
            var targets = GetWorkerBuilders();
            foreach (var target in targets)
            {
                EntityPrefabExporter.ExportSelectedEntityPrefabs(target.BuildTarget);
            }
        }

        private void DrawWorkerTargetSelection()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                using (new EditorGUILayout.HorizontalScope())
                {
                    selectedConfiguration = EditorGUILayout.Popup(selectedConfiguration, configurations);
                    if (check.changed)
                    {
                        buildTargets = null;
                        try
                        {
                            File.WriteAllText(selectedWorkerTargetPath, selectedConfiguration.ToString());
                        }
                        catch { }
                    }
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    var workers = SpatialOsEditor.Workers;
                    if (workers.Count == 0)
                    {
                        GUILayout.Label("No workers found.");
                        return;
                    }

                    for (var i = 0; i < workers.Count; i++)
                    {
                        var worker = workers[i];

                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            var selected = EditorGUILayout.ToggleLeft(worker.Name, SpatialOsEditor.WorkerSelection.IsWorkerSelected(worker));

                            if (check.changed)
                            {
                                SpatialOsEditor.WorkerSelection.SelectWorker(worker, selected);
                            }
                        }
                    }
                }
            }

            CalculateBuildTargets();

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Separator();

                GUILayout.Label("Output platforms", EditorStyles.boldLabel);

                if (buildTargets == null || buildTargets.Count == 0)
                {
                    if (string.IsNullOrEmpty(selectedWorkerString))
                    {
                        selectedWorkerString = string.Join(" and ", SpatialOsEditor.WorkerSelection.SelectedWorkers.Select(w => w.Name).ToArray());
                    }

                    EditorGUILayout.HelpBox(string.Format("The current selection of \"{0}\",  filtered by the worker types \"{1}\", will not output any workers or prefabs.", SelectedConfiguration, selectedWorkerString), MessageType.Warning);
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.Space();

                        using (new EditorGUILayout.VerticalScope())
                        {
                            for (var i = 0; i < buildTargets.Count; i++)
                            {
                                {
                                    GUILayout.Label(buildTargets[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        private IList<string> GetSelectedWorkerNames()
        {
            return SpatialOsEditor.WorkerSelection.SelectedWorkers.Select(w => w.Name).ToList();
        }

        private IList<UnityPlayerBuilder> GetWorkerBuilders()
        {
            var workersToBuild = GetSelectedWorkerNames();

            switch (SelectedConfiguration)
            {
                case DevelopmentTarget:
                    return UnityPlayerBuilders.DevelopmentPlayerBuilders(workersToBuild);
                case DeploymentTarget:
                    return UnityPlayerBuilders.DeploymentPlayerBuilders(workersToBuild);
                default:
                    throw new ArgumentOutOfRangeException(string.Format("{0} is an unknown build target.", SelectedConfiguration));
            }
        }

        static SpatialBuild()
        {
            SpatialOsEditor.RegisterAddon(new SpatialBuild());
        }

        /// <summary>
        ///     Calculate and cache the set of <see cref="BuildTarget" /> based on the target type (Development|Deployment) and
        ///     selected workers.
        /// </summary>
        private void CalculateBuildTargets()
        {
            if (buildTargets == null)
            {
                if (!SpatialOsEditor.WorkerSelection.AnyWorkersSelected)
                {
                    buildTargets = new List<string>();
                }
                else
                {
                    var workersToBuild = GetSelectedWorkerNames();
                    IList<UnityPlayerBuilder> builders;
                    switch (SelectedConfiguration)
                    {
                        case DevelopmentTarget:
                            builders = UnityPlayerBuilders.DevelopmentPlayerBuilders(workersToBuild);
                            break;
                        case DeploymentTarget:
                            builders = UnityPlayerBuilders.DeploymentPlayerBuilders(workersToBuild);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(string.Format("{0} is an unknown build target.", SelectedConfiguration));
                    }

                    buildTargets = builders.Select(b => b.BuildTarget.ToString()).Distinct().OrderBy(b => b).ToList();
                }
            }
        }
    }
}
