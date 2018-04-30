// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Improbable.Unity.Editor.Core;
using Improbable.Unity.Util;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.Editor.Addons
{
    /// <summary>
    ///     Allow each user to specify the location of the spatial command.
    /// </summary>
    [InitializeOnLoad]
    public sealed class SpatialCommand : ISpatialOsEditorAddon, ISpatialOsEditorAddonSettings
    {
        private static readonly SpatialCommand instance;

        internal const string usrLocalBin = "/usr/local/bin";

        private string spatialLocation;
        private string oldSpatialLocation;
        private GUIContent buttonContent;
        private GUIStyle iconStyle;
        private string discoveredLocation;
        internal Func<IList<string>> GetCommandLine { get; set; }
        internal Func<string, string, string> GetUserString { get; set; }
        internal Func<string, bool> FileExists { get; set; }
        internal Func<string, string> GetEnvironmentVariable { get; set; }

        public const string SpatialPathArgument = "spatialCommandPath";


        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Name
        {
            get { return "Spatial CLI [Built-in]"; }
        }

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Vendor
        {
            get { return "Improbable Worlds, Ltd."; }
        }

        /// <summary>
        ///     Returns the user-configured location of spatial[.exe], or simply "spatial" if it's not set.
        /// </summary>
        /// <remarks>
        ///     If +spatialCommandPath "/path/to/spatial" is specified on the command line, it is used in preference to any user
        ///     settings.
        ///     By default, it is assumed that "spatial" will be on the system PATH.
        /// </remarks>
        public static string SpatialPath
        {
            get { return instance.GetSpatialPath(); }
        }

        /// <summary>
        ///     Starts a process and ensures that fullPathToSpatial is available in the PATH environment variable.
        /// </summary>
        public static Process RunCommandWithSpatialInThePath(string fullPathToSpatial, ProcessStartInfo startInfo)
        {
            return SpatialRunner.RunCommandWithSpatialInThePath(fullPathToSpatial, startInfo);
        }

        /// <inheritdoc cref="ISpatialOsEditorAddonSettings" />
        public void OnSettingsGui(Rect rect)
        {
            InitializeOnce();

            using (new EditorGUILayout.VerticalScope())
            {
                DrawSpatialLocationInput();

                DrawDiscoveredLocation();

                DrawUpdateButton();
            }
        }

        private void DrawUpdateButton()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Update Spatial CLI Version");
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(discoveredLocation)))
                {
                    if (GUILayout.Button(buttonContent))
                    {
                        SpatialOsEditor.RunPausedProcess(SpatialPath, "update", "");
                    }
                }
            }
        }

        private void InitializeOnce()
        {
            if (spatialLocation == null)
            {
                spatialLocation = GetUserString(SpatialRunner.CommandLocationKey, string.Empty);
                discoveredLocation = DiscoverSpatialLocationForDisplay(spatialLocation);
            }

            if (buttonContent == null)
            {
                buttonContent = new GUIContent("Update") { tooltip = "Update spatial to the latest version" };
            }

            if (iconStyle == null)
            {
                iconStyle = new GUIStyle() { fixedWidth = 24, fixedHeight = 24 };
            }
        }

        private void DrawSpatialLocationInput()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.PrefixLabel("Spatial CLI location");

                oldSpatialLocation = spatialLocation;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(spatialLocation, MessageType.None);
                    if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
                    {
                        BrowseForSpatial();
                    }

                    if (GUILayout.Button("Reset to default", GUILayout.ExpandWidth(false)))
                    {
                        spatialLocation = string.Empty;
                    }
                }

                if (check.changed && oldSpatialLocation != spatialLocation)
                {
                    CommitAndCheckSpatialLocation();
                }
            }
        }

        private void BrowseForSpatial()
        {
            var extension = string.Empty;
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                extension = "exe";
            }

            spatialLocation = EditorUtility.OpenFilePanel("Find spatial", spatialLocation, extension);
        }

        private void CommitAndCheckSpatialLocation()
        {
            if (string.IsNullOrEmpty(spatialLocation))
            {
                EditorPrefs.DeleteKey(SpatialRunner.CommandLocationKey);
            }
            else
            {
                EditorPrefs.SetString(SpatialRunner.CommandLocationKey, spatialLocation);
            }

            discoveredLocation = DiscoverSpatialLocationForDisplay(spatialLocation);
        }

        private void DrawDiscoveredLocation()
        {
            if (string.IsNullOrEmpty(discoveredLocation))
            {
                EditorGUILayout.HelpBox("Could not find spatial.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(string.Format("Found {0}.", discoveredLocation), MessageType.Info);
            }
        }

        private string DiscoverSpatialLocationForDisplay(string location)
        {
            bool viaPath;
            var path = DiscoverSpatialLocation(location, out viaPath);
            if (viaPath)
            {
                return path + " (via PATH)";
            }
            else
            {
                return path;
            }
        }

        private string DiscoverSpatialLocation(string location)
        {
            bool viaPath;
            return DiscoverSpatialLocation(location, out viaPath);
        }

        private string DiscoverSpatialLocation(string location, out bool viaPath)
        {
            viaPath = false;
            if (string.IsNullOrEmpty(location))
            {
                var pathValue = GetEnvironmentVariable("PATH");
                if (pathValue == null)
                {
                    return string.Empty;
                }

                var fileName = SpatialRunner.DefaultSpatialCommand;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    fileName = Path.ChangeExtension(fileName, ".exe");
                }

                var splitPath = pathValue.Split(Path.PathSeparator);

                if (Application.platform == RuntimePlatform.OSXEditor && !splitPath.Contains(usrLocalBin))
                {
                    splitPath = splitPath.Union(new[] { usrLocalBin }).ToArray();
                }

                foreach (var path in splitPath)
                {
                    var testPath = Path.Combine(path, fileName);
                    if (FileExists(testPath))
                    {
                        viaPath = true;
                        return testPath;
                    }
                }
            }
            else
            {
                var fullLocation = location;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    fullLocation = Path.ChangeExtension(fullLocation, ".exe");
                }

                if (FileExists(fullLocation))
                {
                    return fullLocation;
                }
            }

            return string.Empty;
        }

        static SpatialCommand()
        {
            instance = new SpatialCommand();
            SpatialOsEditor.RegisterAddon(instance);
        }

        internal SpatialCommand()
        {
            GetCommandLine = Environment.GetCommandLineArgs;
            GetUserString = EditorPrefs.GetString;
            FileExists = File.Exists;
            GetEnvironmentVariable = Environment.GetEnvironmentVariable;
        }

        internal string GetSpatialPath()
        {
            string path;
            // The command line overrides everything.
            if (!CommandLineUtil.TryGetCommandLineValue(GetCommandLine(), SpatialPathArgument, out path))
            {
                // Then try the user-specific preferences
                path = GetUserString(SpatialRunner.CommandLocationKey, string.Empty);
            }

            // If nothing has been configured, assume it's on the system PATH, and use a sensible default of "spatial"
            if (string.IsNullOrEmpty(path))
            {
                path = DiscoverSpatialLocation(null);
            }

            return path;
        }
    }
}
