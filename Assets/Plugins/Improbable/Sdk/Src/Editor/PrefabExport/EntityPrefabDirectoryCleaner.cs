// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.IO;
using Improbable.Unity.EditorTools.Util;

namespace Improbable.Unity.EditorTools.PrefabExport
{
    static class EntityPrefabDirectoryCleaner
    {
        public static void CleanPrefabTargetDirectories()
        {
            var info = new DirectoryInfo(EditorPaths.PrefabExportDirectory);
            if (info.Exists)
            {
                var files = info.GetFiles();
                foreach (var fileInfo in files)
                {
                    fileInfo.Delete();
                }
            }
        }
    }
}
