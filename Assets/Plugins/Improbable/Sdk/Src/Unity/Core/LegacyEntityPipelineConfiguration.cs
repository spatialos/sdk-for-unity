// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Entity;
using Improbable.Unity.Metrics;

namespace Improbable.Unity.Core
{
#pragma warning disable 618
    class LegacyEntityPipelineConfiguration : ILegacyEntityPipelineConfiguration
    {
        private readonly IEntityComponentInterestOverridesUpdater _entityComponentInterestOverridesUpdater;
        private readonly IInterestedComponentUpdaterProvider interestedComponentUpdaterProvider;

        public LegacyEntityPipelineConfiguration(IEntityComponentInterestOverridesUpdater entityComponentInterestOverridesUpdater, IInterestedComponentUpdaterProvider interestedComponentUpdaterProvider)
        {
            this._entityComponentInterestOverridesUpdater = entityComponentInterestOverridesUpdater;
            this.interestedComponentUpdaterProvider = interestedComponentUpdaterProvider;
        }

        public IEntityTemplateProvider TemplateProvider
        {
            get { return SpatialOS.TemplateProvider; }
        }

        public bool UsePrefabPooling
        {
            get { return SpatialOS.Configuration.UsePrefabPooling; }
        }

        public IList<string> AssetsToPrecache
        {
            get { return SpatialOS.AssetsToPrecache; }
        }

        public IEnumerable<KeyValuePair<string, int>> AssetsToPrePool
        {
            get { return SpatialOS.AssetsToPrePool; }
        }

        public int MaxConcurrentPrecacheConnections
        {
            get { return SpatialOS.MaxConcurrentPrecacheConnections; }
        }

        public Action OnPrecachingCompleted
        {
            get { return SpatialOS.OnPrecachingCompleted; }
        }

        public Action<int> OnPrecacheProgress
        {
            get { return SpatialOS.OnPrecacheProgress; }
        }

        [Obsolete("Please use Improbable.Unity.Core.CountBasedSpawnLimiter and SpatialOS.EntitySpawnLimiter.")]
        public int EntityCreationLimitPerFrame
        {
            get { return SpatialOS.Configuration.EntityCreationLimitPerFrame; }
        }

        public WorkerMetrics Metrics
        {
            get { return SpatialOS.Metrics; }
        }

        public IEntityComponentInterestOverridesUpdater EntityComponentInterestOverridesUpdater
        {
            get { return _entityComponentInterestOverridesUpdater; }
        }

        public IInterestedComponentUpdaterProvider InterestedComponentUpdaterProvider
        {
            get { return interestedComponentUpdaterProvider; }
        }
    }
#pragma warning restore 618
}
