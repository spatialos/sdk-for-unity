// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;

namespace Improbable.Unity.Editor.Core
{
    /// <summary>
    ///     Manages the selection of the available workers in the Unity project.
    /// </summary>
    public class WorkerSelection
    {
        private readonly HashSet<SpatialOsWorker> selectedWorkers = new HashSet<SpatialOsWorker>();

        /// <summary>
        ///     Invoked when the set of selected workers changes.
        /// </summary>
        public event Action<HashSet<SpatialOsWorker>> OnWorkerSelectionChanged;

        /// <summary>
        ///     Selects or deselects a worker.
        /// </summary>
        public void SelectWorker(SpatialOsWorker worker, bool selectWorker)
        {
            var changed = selectWorker ? selectedWorkers.Add(worker) : selectedWorkers.Remove(worker);

            if (changed && OnWorkerSelectionChanged != null)
            {
                OnWorkerSelectionChanged(selectedWorkers);
            }
        }

        /// <summary>
        ///     A set of selected worker instances.
        /// </summary>
        public HashSet<SpatialOsWorker> SelectedWorkers
        {
            get { return new HashSet<SpatialOsWorker>(selectedWorkers); }
        }

        /// <summary>
        ///     Returns true if any workers are selected.
        /// </summary>
        public bool AnyWorkersSelected
        {
            get { return selectedWorkers.Count > 0; }
        }

        /// <summary>
        ///     Returns true if the worker is selected.
        /// </summary>
        public bool IsWorkerSelected(SpatialOsWorker worker)
        {
            return selectedWorkers.Contains(worker);
        }
    }
}
