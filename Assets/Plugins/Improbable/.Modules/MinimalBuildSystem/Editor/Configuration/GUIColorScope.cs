// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using UnityEngine;

namespace Improbable.Unity.MinimalBuildSystem.Configuration
{
    class GUIColorScope : IDisposable
    {
        private readonly Color color;

        public GUIColorScope(Color newColor)
        {
            color = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose()
        {
            GUI.color = color;
        }
    }
}
