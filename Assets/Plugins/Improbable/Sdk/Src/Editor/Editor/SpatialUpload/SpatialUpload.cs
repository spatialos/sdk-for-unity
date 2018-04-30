// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Editor.Addons;
using Improbable.Unity.Editor.Core;
using Improbable.Unity.EditorTools;
using UnityEditor;
using UnityEngine;

namespace Improbable.Editor.Addons.SpatialUpload
{
    [InitializeOnLoad]
    public class SpatialUpload : ISpatialOsEditorAddon, ISpatialOsEditorAddonBuild
    {
        private string assemblyName;

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Name
        {
            get { return "Spatial upload"; }
        }

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Vendor
        {
            get { return "Improbable Worlds, Ltd."; }
        }

        /// <inheritdoc cref="ISpatialOsEditorAddonBuild" />
        public void OnDevGui(Rect rect)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Assembly name", SpatialOsWindow.SharedContent.MinimalLabelStyle);
                assemblyName = EditorGUILayout.TextField(assemblyName);

                if (GUILayout.Button("Upload"))
                {
                    SpatialOsEditor.RunPausedProcess(SpatialCommand.SpatialPath, "upload " + assemblyName, SpatialOsEditor.ApplicationRootDir);
                }
            }
        }

        static SpatialUpload()
        {
            SpatialOsEditor.RegisterAddon(new SpatialUpload());
        }
    }
}
