// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    ///     Implementing class can accept attaching of Editor Data Object.
    /// </summary>
    public interface ICanAttachEditorDataObject
    {
        /// <summary>
        ///     Attaches the supplied editor data object.
        /// </summary>
        void AttachEditorDataObject(IComponentEditorDataObject editorDataObject);

        /// <summary>
        ///     Removes attached editor data object.
        /// </summary>
        void RemoveEditorDataObject();
    }
}
