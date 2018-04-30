// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.IO;
using System.Linq;
using Improbable.Unity.Editor.Addons;
using Improbable.Unity.Editor.Core;
using Improbable.Unity.Util;
using UnityEditor;
using UnityEngine;

namespace Improbable.Editor.Addons.SpatialLocal
{
    [InitializeOnLoad]
    public class SpatialLocal : ISpatialOsEditorAddon, ISpatialOsEditorAddonBuild
    {
        public string Name
        {
            get { return "Run SpatialOS locally"; }
        }

        public string Vendor
        {
            get { return "Improbable Worlds, Ltd."; }
        }

        private int selectedConfig;

        static SpatialLocal()
        {
            SpatialOsEditor.RegisterAddon(new SpatialLocal());
        }

        public void OnDevGui(Rect rect)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                var rootPath = Path.GetFullPath(PathUtil.Combine(SpatialOsEditor.WorkerRootDir, "..", ".."));
                var files = Directory.GetFiles(rootPath, "*.json");
                var filtered = files.Where(f => !f.Contains("spatialos")).Select<string, string>(Path.GetFileName).ToArray();
                selectedConfig = GUILayout.SelectionGrid(selectedConfig, filtered, 1);

                using (new EditorGUI.DisabledScope(selectedConfig < 0 || selectedConfig >= filtered.Length))
                {
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Run"))
                    {
                        SpatialOsEditor.RunPausedProcess(SpatialCommand.SpatialPath, string.Format("local start {0}", filtered[selectedConfig]), rootPath);
                    }
                }
            }
        }
    }
}
