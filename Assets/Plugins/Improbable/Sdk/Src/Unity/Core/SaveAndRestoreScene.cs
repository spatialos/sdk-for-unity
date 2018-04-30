// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using UnityEngine.SceneManagement;

namespace Improbable.Unity.Core
{
    struct SaveAndRestoreScene : IDisposable
    {
        private readonly Scene oldScene;

        public SaveAndRestoreScene(Scene newScene)
        {
            oldScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(newScene);
        }

        public void Dispose()
        {
            SceneManager.SetActiveScene(oldScene);
        }
    }
}
