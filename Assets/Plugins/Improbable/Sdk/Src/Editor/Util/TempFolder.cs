// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.IO;

namespace Improbable.Unity.EditorTools.Util
{
    public class TempFolder : IDisposable
    {
        private string path;

        public String Path
        {
            get
            {
                if (!Initialized)
                {
                    path = CreateTempPath();
                }

                return path;
            }
        }

        private bool Initialized
        {
            get { return !string.IsNullOrEmpty(path); }
        }

        public void Dispose()
        {
            if (Initialized)
            {
                Directory.Delete(path, true);
            }
        }

        public string CreateTempPath()
        {
            string tempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
