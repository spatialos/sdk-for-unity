// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEditor;
using UnityEngine;

namespace Improbable.Editor.Core
{
    /// <summary>
    ///     Gui-related content to provide a cohesive user experience.
    /// </summary>
    public class SharedGuiContent
    {
        /// <summary>
        ///     Draws a label that is sized to its content.
        /// </summary>
        public GUIStyle MinimalLabelStyle
        {
            get { return new GUIStyle(GUI.skin.label) { stretchWidth = false }; }
        }

        /// <summary>
        ///     A foldout header with bold text.
        /// </summary>
        public GUIStyle BoldFoldout
        {
            get { return new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold }; }
        }
    }
}
