// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Entity;
using Improbable.Unity.Metrics;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Everything that is needed to run the legacy asset pipeline.
    /// </summary>
    /// <remarks>
    ///     This de-couples the LegacyAssetPipeline from SpatialOS utility class.
    /// </remarks>
    public interface ILegacyEntityPipelineConfiguration
    {
        /// <summary>
        ///     EntityTemplateProvider to be used.
        /// </summary>
        IEntityTemplateProvider TemplateProvider { get; }

        /// <summary>
        ///     True if prefab pooling should be used.
        /// </summary>
        bool UsePrefabPooling { get; }

        /// <summary>
        ///     List of assets to be precached.
        /// </summary>
        IList<string> AssetsToPrecache { get; }

        /// <summary>
        ///     List of assets to be prepooled.
        /// </summary>
        IEnumerable<KeyValuePair<string, int>> AssetsToPrePool { get; }

        /// <summary>
        ///     The maximum number of concurrent connections to be used for asset preaching.
        /// </summary>
        int MaxConcurrentPrecacheConnections { get; }

        /// <summary>
        ///     Delegate to be invoked when asset precaching is complete.
        /// </summary>
        Action OnPrecachingCompleted { get; }

        /// <summary>
        ///     Delegate to be invoked with asset precaching progress.
        /// </summary>
        Action<int> OnPrecacheProgress { get; }

        /// <summary>
        ///     The maximum number of entities that can be created per frame.
        /// </summary>
        int EntityCreationLimitPerFrame { get; }

        /// <summary>
        ///     Metrics object to be used for reportinhg.
        /// </summary>
        WorkerMetrics Metrics { get; }

        /// <summary>
        ///     Class updating interested components.
        /// </summary>
        IEntityComponentInterestOverridesUpdater EntityComponentInterestOverridesUpdater { get; }

        /// <summary>
        ///     Class providing interested components provider.
        /// </summary>
        IInterestedComponentUpdaterProvider InterestedComponentUpdaterProvider { get; }
    }
}
