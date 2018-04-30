// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Editor.Addons;
using Improbable.Unity.Editor.Core;
using Improbable.Unity.EditorTools;
using UnityEditor;
using UnityEngine;

namespace Improbable.Editor.Addons.SpatialConnect
{
    [InitializeOnLoad]
    public class SpatialConnect : ISpatialOsEditorAddon, ISpatialOsEditorAddonBuild
    {
        private string deploymentName;

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Name
        {
            get { return "Connect to deployment"; }
        }

        /// <inheritdoc cref="ISpatialOsEditorAddon" />
        public string Vendor
        {
            get { return "Improbable Worlds, Ltd."; }
        }

        static SpatialConnect()
        {
            SpatialOsEditor.RegisterAddon(new SpatialConnect());
        }

        /// <inheritdoc cref="ISpatialOsEditorAddonBuild" />
        public void OnDevGui(Rect rect)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Deployment name", SpatialOsWindow.SharedContent.MinimalLabelStyle);
                deploymentName = EditorGUILayout.TextField(deploymentName);

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(deploymentName)))
                {
                    if (GUILayout.Button("Connect"))
                    {
                        SpatialOsEditor.RunPausedProcess(SpatialCommand.SpatialPath, string.Format("connect {0} {1}", SpatialOsEditor.ProjectDescriptor.Name, deploymentName), SpatialOsEditor.ApplicationRootDir);
                    }
                }
            }
        }
    }
}
