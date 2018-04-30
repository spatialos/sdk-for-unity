// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Unity.Editor.Core;
using Improbable.Unity.EditorTools.Build;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Core
{
    /// <summary>
    ///     Provides workers from the worker.json files scanned from the worker's folder.
    /// </summary>
    internal class DefaultWorkerProvider : IWorkerProvider
    {
        private IList<SpatialOsWorker> workers;

        private const string WorkerNamePrefix = "spatialos.";
        private const string WorkerNameSuffix = ".worker.json";

        public IList<SpatialOsWorker> GetWorkers()
        {
            if (workers != null)
            {
                return workers;
            }

            const string searchPattern = WorkerNamePrefix + "*" + WorkerNameSuffix;
            try
            {
                var workerFiles = Directory.GetFiles(SpatialOsEditor.WorkerRootDir, searchPattern).Select<string, string>(ExtractPlayerName);
                workers = workerFiles.Select(f => new SpatialOsWorker(ExtractPlayerName(f))).ToList();
                return workers;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return new List<SpatialOsWorker>();
            }
        }

        private static string ExtractPlayerName(string fileName)
        {
            var workerName = Path.GetFileName(fileName);
            if (workerName != null)
            {
                workerName = workerName.Replace(WorkerNamePrefix, string.Empty);
                workerName = workerName.Replace(WorkerNameSuffix, string.Empty);
                return workerName;
            }

            return fileName;
        }
    }
}
