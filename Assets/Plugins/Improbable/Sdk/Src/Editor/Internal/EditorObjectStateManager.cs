// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    ///     Utility class for storing state of Unity editor components.
    /// </summary>
    public static class EditorObjectStateManager
    {
        private static readonly IDictionary<int, IComponentEditorDataObject> EditorDataObjects =
            new Dictionary<int, IComponentEditorDataObject>();

        public static IComponentEditorDataObject GetComponentEditorData(int objectHash)
        {
            IComponentEditorDataObject value;
            return EditorDataObjects.TryGetValue(objectHash, out value) ? value : null;
        }

        public static void SetComponentEditorData(int objectHash, IComponentEditorDataObject editorData)
        {
            EditorDataObjects[objectHash] = editorData;
        }
    }
}
