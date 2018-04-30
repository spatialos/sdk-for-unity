// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.EditorTools.Build
{
    [Obsolete("Obsolete in 10.3.0. Please see IPlayerBuildEvents for information about customizing player packaging.")]
#pragma warning disable 0618
    public class SimplePackager : IPackager
#pragma warning restore 0618
    {
        public void Prepare(string packagePath) { }
    }
}
