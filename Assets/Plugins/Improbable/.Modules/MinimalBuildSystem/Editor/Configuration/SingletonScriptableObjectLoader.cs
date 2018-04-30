// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;

namespace Improbable.Unity.MinimalBuildSystem.Configuration
{
    internal static class SingletonScriptableObjectLoader
    {
        internal static readonly HashSet<Type> LoadingInstances = new HashSet<Type>();
    }
}
