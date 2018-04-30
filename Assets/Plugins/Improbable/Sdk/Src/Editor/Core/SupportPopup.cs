// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools
{
    internal class SupportPopup : PopupWindowContent
    {
        public override Vector2 GetWindowSize()
        {
            return new Vector2(120, 68);
        }

        public override void OnGUI(Rect rect)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (GUILayout.Button("Forums"))
                {
                    Process.Start("https://forums.improbable.io");
                }

                if (GUILayout.Button("Documentation"))
                {
                    Process.Start("https://spatialos.improbable.io/docs");
                }

                if (GUILayout.Button("Send feedback"))
                {
                    Process.Start("https://forums.improbable.io/c/fb");
                }
            }
        }
    }
}
