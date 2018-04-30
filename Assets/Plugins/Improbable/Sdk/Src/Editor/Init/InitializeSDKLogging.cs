// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Worker;
using UnityEditor;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Init
{
    [InitializeOnLoad]
    class InitializeSDKLogging
    {
        static InitializeSDKLogging()
        {
            ClientError.ExceptionCallback = Debug.LogException;
        }
    }
}
