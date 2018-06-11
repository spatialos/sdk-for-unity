// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Text.RegularExpressions;
using Improbable.Editor.Core;
using Improbable.Unity.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools
{
    /// <summary>
    ///     Provides a user interface to common SpatialOS-related development commands.
    /// </summary>
    [Serializable]
    [InitializeOnLoad]
    public class SpatialOsWindow : EditorWindow
    {
        private const string MinVersion = "2017.3.0";
        private const string MaxVersion = "2018.1.3";

        private static SharedGuiContent sharedContent;
        private static string versionMessage;

        private GUIStyle boldStyle;
        private GUIStyle selectionGridStyle;

        private GUIStyle feedbackButtonStyle;

        private Rect supportButtonRect;

        [MenuItem("Window/SpatialOS")]
        [MenuItem("Improbable/SpatialOS Window")]
        private static void ShowWindow()
        {
            var instance = GetWindow<SpatialOsWindow>();
            instance.titleContent = new GUIContent("SpatialOS");
            instance.minSize = new Vector2(320, 24);
        }

        /// <summary>
        ///     Accesses common GUI system content and helpers.
        /// </summary>
        public static SharedGuiContent SharedContent
        {
            get { return sharedContent ?? (sharedContent = new SharedGuiContent()); }
        }

        public void OnEnable()
        {
            try
            {
                long version = ConvertUnityVersionToNumber(Application.unityVersion);
                long min = ConvertUnityVersionToNumber(MinVersion);
                long max = ConvertUnityVersionToNumber(MaxVersion);

                if (version < min)
                {
                    versionMessage = string.Format("Your Unity version: {0}\nEarliest tested: {1}.\nPlease use at least {1} for the best experience.", Application.unityVersion, MinVersion);
                }
                else if (version > max)
                {
                    versionMessage = string.Format("Your Unity version: {0}\nLatest tested: {1}.\nThings should work fine, but you may find issues.", Application.unityVersion, MaxVersion);
                }
            }
            catch (Exception)
            {
                versionMessage = string.Format("Unity {0} may be untested with the SpatialOS for Unity SDK.", Application.unityVersion);
            }

            if (!string.IsNullOrEmpty(versionMessage))
            {
                EditorWindow.GetWindow<SpatialOsWindow>().minSize = new Vector2(320, 72);
            }
        }

        public void OnDestroy() { }

        public void OnGUI()
        {
            InitializeOnce();

            using (new EditorGUILayout.VerticalScope())
            {
                if (!string.IsNullOrEmpty(versionMessage))
                {
                    EditorGUILayout.HelpBox(versionMessage, MessageType.Warning);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
                    {
                        EditorGUILayout.SelectableLabel(SpatialOsEditor.ProjectDescriptor.SdkVersion, SharedContent.MinimalLabelStyle);
                        EditorGUILayout.SelectableLabel(SpatialOsEditor.ProjectDescriptor.Name, SharedContent.MinimalLabelStyle);
                    }

                    if (GUILayout.Button("Build"))
                    {
                        EditorWindow.GetWindow<SpatialOsAuxWindow>().SetAddonType(typeof(ISpatialOsEditorAddonBuild), "Build");
                    }

                    if (GUILayout.Button("Settings"))
                    {
                        EditorWindow.GetWindow<SpatialOsAuxWindow>().SetAddonType(typeof(ISpatialOsEditorAddonSettings), "Settings");
                    }

                    if (GUILayout.Button("Support"))
                    {
                        PopupWindow.Show(supportButtonRect, new SupportPopup());
                    }

                    if (Event.current.type == EventType.Repaint)
                    {
                        supportButtonRect = GUILayoutUtility.GetLastRect();
                    }
                }
            }
        }

        private void InitializeOnce()
        {
            if (boldStyle == null)
            {
                boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            }

            if (selectionGridStyle == null)
            {
                selectionGridStyle = new GUIStyle(GUI.skin.button) { fixedHeight = 48, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(4, 4, 4, 4) };
            }

            if (feedbackButtonStyle == null)
            {
                feedbackButtonStyle = new GUIStyle(GUI.skin.button) { fixedHeight = 32, fixedWidth = 32, imagePosition = ImagePosition.ImageLeft };
            }
        }

        private static long ConvertUnityVersionToNumber(string version)
        {
            var matches = Regex.Matches(version, @"(\d+)\.(\d+)\.(\d+).*");

            if (matches.Count < 1)
            {
                throw new InvalidOperationException();
            }

            var groups = matches[0].Groups;

            // groups[0] is the whole string that matches; the following elements are the captured matches.
            return long.Parse(groups[1].Value) * 100 + long.Parse(groups[2].Value) * 10 + long.Parse(groups[3].Value);
        }
    }
}
