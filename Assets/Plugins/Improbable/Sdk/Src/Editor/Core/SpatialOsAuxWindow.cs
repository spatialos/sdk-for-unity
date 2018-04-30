// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Linq;
using Improbable.Unity.EditorTools;
using Improbable.Unity.EditorTools.Util;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.Editor.Core
{
    [Serializable]
    class SpatialOsAuxWindow : EditorWindow
    {
        [SerializeField] private string addonTitle;
        [SerializeField] private string typeName;
        [SerializeField] private AddonUiStateDictinoary addonState;

        private ISpatialOsEditorAddon[] addons;
        private Type addonType;

        SpatialOsAuxWindow()
        {
            titleContent = new GUIContent("SpatialOS");
        }

        public void OnEnable()
        {
            if (addonState == null)
            {
                addonState = new AddonUiStateDictinoary();
            }

            if (typeName != null)
            {
                var type = Type.GetType(typeName, false);
                if (type != null)
                {
                    SetAddonType(type, addonTitle);
                }
                else
                {
                    Debug.LogErrorFormat("Could not find type {0}", typeName);
                    typeName = null;
                    addons = null;
                }

                Repaint();
            }
        }

        public void SetAddonType(Type type, string newAddonTitle)
        {
            addonType = type;
            addons = SpatialOsEditor.Addons().Where(type.IsInstanceOfType).ToArray();
            typeName = type.FullName;
            addonTitle = newAddonTitle;
            titleContent = new GUIContent(newAddonTitle);
            Repaint();
        }

        public void OnGUI()
        {
            if (SpatialOsWindow.SharedContent == null)
            {
                return;
            }

            if (addons == null || addons.Length == 0)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.HelpBox("Please select an item in the SpatialOS window.", MessageType.Info);
                    if (GUILayout.Button("Show SpatialOS window"))
                    {
                        GetWindow<SpatialOsWindow>().Show();
                    }
                }

                return;
            }

            AddonUiState s;
            if (!addonState.TryGetValue(typeName, out s))
            {
                addonState[typeName] = s = new AddonUiState();
            }

            if (s.Selected == null || s.Selected.Length != addons.Length)
            {
                s.Selected = Enumerable.Repeat<bool>(true, addons.Length).ToArray();
            }

            if (s.Rects == null || s.Rects.Length != addons.Length)
            {
                s.Rects = new Rect[addons.Length];
            }

            var rect = new Rect(0, 0, position.width, position.height);

            using (var scroller = new EditorGUILayout.ScrollViewScope(s.ScrollPosition, false, false))
            using (new EditorGUILayout.VerticalScope())
            {
                for (var index = 0; index < addons.Length; index++)
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.Separator();
                        s.Selected[index] = EditorGUILayout.Foldout(s.Selected[index], addons[index].Name,
                                                                    SpatialOsWindow.SharedContent.BoldFoldout);
                        if (s.Selected[index])
                        {
                            if (addonType.IsAssignableFrom(typeof(ISpatialOsEditorAddonBuild)))
                            {
                                ((ISpatialOsEditorAddonBuild) addons[index]).OnDevGui(rect);
                            }
                            else if (addonType.IsAssignableFrom(typeof(ISpatialOsEditorAddonSettings)))
                            {
                                ((ISpatialOsEditorAddonSettings) addons[index]).OnSettingsGui(rect);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox(
                                                        string.Format("Can't draw addons of type {0}", addons[index].GetType()),
                                                        MessageType.Warning);
                            }
                        }

                        EditorGUILayout.Separator();
                    }

                    if (Event.current.type == EventType.Repaint)
                    {
                        s.Rects[index] = GUILayoutUtility.GetLastRect();
                    }
                }

                s.ScrollPosition = scroller.scrollPosition;
            }


            Handles.BeginGUI();
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            for (var index = 1; index < addons.Length; index++)
            {
                var yMin = s.Rects[index].yMin - s.ScrollPosition.y;
                Handles.DrawLine(new Vector3(0, yMin), new Vector3(s.Rects[index].xMax, yMin));
            }

            Handles.EndGUI();
        }

        [Serializable]
        private class AddonUiState
        {
            public bool[] Selected;
            public Vector2 ScrollPosition;
            public Rect[] Rects;
        }

        /// <summary>
        ///     Unity can't serialize generics directly.
        /// </summary>
        [Serializable]
        private class AddonUiStateDictinoary : SerializableDictionary<string, AddonUiState> { }
    }
}
