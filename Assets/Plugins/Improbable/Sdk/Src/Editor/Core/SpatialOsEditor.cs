// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Improbable.Unity.Core;
using Improbable.Unity.EditorTools.Build;
using Improbable.Unity.EditorTools.Core;
using Improbable.Unity.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Improbable.Unity.Editor.Core
{
    /// <summary>
    ///     Provides SpatialOS-specific functionality for the Unity Editor.
    /// </summary>
    [InitializeOnLoad]
    public static class SpatialOsEditor
    {
        private static readonly Dictionary<Type, ISpatialOsEditorAddon> RegisteredAddons = new Dictionary<Type, ISpatialOsEditorAddon>();

        private static ProjectDescriptor projectDescriptor;

        private static IWorkerProvider workerProvider;
        private static WorkerSelection workerSelection;

        private static readonly string WorkerSelectionAssetPath;

        /// <summary>
        ///     The absolute path of the worker.
        /// </summary>
        public static string WorkerRootDir { get; private set; }

        /// <summary>
        ///     The absolute path of the SpatialOS project.
        /// </summary>
        public static string ApplicationRootDir { get; private set; }

        /// <summary>
        ///     Contains information about the SpatialOS project as a whole.
        /// </summary>
        public static ProjectDescriptor ProjectDescriptor
        {
            get { return projectDescriptor ?? (projectDescriptor = ProjectDescriptor.Load()); }
        }

        internal static IWorkerProvider WorkerProvider
        {
            get { return workerProvider ?? (workerProvider = new DefaultWorkerProvider()); }
            set { workerProvider = value; }
        }

        /// <summary>
        ///     Manages the selection of available workers.
        /// </summary>
        public static WorkerSelection WorkerSelection
        {
            get
            {
                if (workerSelection != null)
                {
                    return workerSelection;
                }

                workerSelection = new WorkerSelection();
                InitializeWorkerSelection(workerSelection);
                return workerSelection;
            }
        }

        /// <summary>
        ///     Gets an instance of a specific addon.
        /// </summary>
        /// <exception cref="KeyNotFoundException">If the addon is not registered.</exception>
        /// >
        public static TAddon GetAddon<TAddon>() where TAddon : ISpatialOsEditorAddon
        {
            return (TAddon) RegisteredAddons[typeof(TAddon)];
        }

        /// <summary>
        ///     Registers an instance
        /// </summary>
        /// <exception cref="InvalidOperationException">If the addon is already registered.</exception>
        public static void RegisterAddon(ISpatialOsEditorAddon addon)
        {
            if (RegisteredAddons.ContainsKey(addon.GetType()))
            {
                throw new InvalidOperationException(string.Format("{0} is already registered", addon.GetType()));
            }

            RegisteredAddons[addon.GetType()] = addon;
        }

        /// <summary>
        ///     Returns a list of all currently-registered addons.
        /// </summary>
        public static IList<ISpatialOsEditorAddon> Addons()
        {
            return RegisteredAddons.Select(kv => kv.Value).ToList();
        }

        /// <summary>
        ///     Workers that exist within the current Unity project.
        /// </summary>
        public static IList<SpatialOsWorker> Workers
        {
            get { return WorkerProvider.GetWorkers(); }
        }

        /// <summary>
        ///     Temporary solution for running external processes.
        /// </summary>
        public static void RunProcess(string command, string arguments, string workingDir)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = command,
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            try
            {
                process.Start();
                process.WaitForExit();

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                // This is a workaround as toolbelt's regular output gets piped to stderror for some reason.
                LogAsSeparateLines(output + "\n" + error, (process.ExitCode != 0) ? LogType.Error : LogType.Log);
                if (process.ExitCode != 0)
                {
                    throw new Exception(string.Format("Process call {0} {1} failed with exit code {2}.", command, arguments, process.ExitCode));
                }
            }
            finally
            {
                process.Close();
            }
        }

        private static void LogAsSeparateLines(string input, LogType logType)
        {
            var lines = input.Split('\n').Select(s => s.Trim()).Where(line => !string.IsNullOrEmpty(line));
            foreach (var line in lines)
            {
#pragma warning disable 618
                Debug.logger.LogFormat(logType, "{0}", line);
#pragma warning restore 618
            }
        }

        /// <summary>
        ///     Temporary solution for running external processes.
        /// </summary>
        public static void RunPausedProcess(string command, string arguments, string workingDir)
        {
            SpatialRunner.RunPausedProcess(command, arguments, workingDir);
        }

        static SpatialOsEditor()
        {
            WorkerRootDir = Path.GetFullPath(PathUtil.Combine(Application.dataPath, ".."));
            ApplicationRootDir = Path.GetFullPath(PathUtil.Combine(WorkerRootDir, "..", ".."));
            WorkerSelectionAssetPath = PathUtil.Combine(WorkerRootDir, "build", "selected_workers.txt");
        }

        /// <summary>
        ///     Read the names of workers from a text file, one worker name per line. Empty lines are ignored.
        /// </summary>
        /// <remarks>
        ///     This exists because we want to store these per-unity project, so the filesystem is the best place.
        ///     EditorPrefs is global to all Unity instances.
        /// </remarks>
        private static void InitializeWorkerSelection(WorkerSelection selection)
        {
            // Default to selecting all workers.
            IEnumerable<SpatialOsWorker> toSelect = Workers;

            // Load from disk and reconcile the previous selection against available workers.
            if (File.Exists(WorkerSelectionAssetPath))
            {
                try
                {
                    var sourceText = File.ReadAllText(WorkerSelectionAssetPath);
                    var previouslySelected = sourceText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    toSelect = Workers.Where(w => previouslySelected.Contains(w.Name));
                }
                catch { }
            }

            // Restore the selection state.
            foreach (var worker in toSelect)
            {
                selection.SelectWorker(worker, true);
            }

            // Listen for selections events so we can save the current selection to disk.
            selection.OnWorkerSelectionChanged += worker =>
            {
                try
                {
                    var text = string.Join("\n", workerSelection.SelectedWorkers.Select(w => w.Name).ToArray());
                    File.WriteAllText(WorkerSelectionAssetPath, text);
                }
                catch { }
            };
        }
    }
}
