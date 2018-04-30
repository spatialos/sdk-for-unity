// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    ///     Bridges SpatialOS component classes with component editors in Unity Editor.
    /// </summary>
    interface IComponentEditorDataObject<TComponent> : IComponentEditorDataObject { }

    /// <summary>
    ///     Bridges SpatialOS component classes with component editors in Unity Editor.
    /// </summary>
    public interface IComponentEditorDataObject
    {
        void LogComponentUpdate(string componentName, object componentValue);
        void LogCommandRequest(DateTime timestamp, string commandName, object payload);
        void SendUpdateLog();
    }
}
