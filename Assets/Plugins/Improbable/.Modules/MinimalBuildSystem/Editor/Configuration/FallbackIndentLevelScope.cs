// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

#if !UNITY_2017_3_OR_NEWER
using System;
using UnityEditor;

namespace Improbable.Unity.MinimalBuildSystem.Configuration
{
    /// <summary>
    /// Fallback for Unity 2017.3f1 feature: EditorGUI.IndentLevelScope
    /// </summary>
    public class FallbackIndentLevelScope : IDisposable
    {
        private readonly int indent;

        public FallbackIndentLevelScope(int increment)
        {
            indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel += indent;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = indent;
        }
    }
}
#endif
