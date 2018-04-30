// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections;
using System.Linq;
using Improbable.Unity.ComponentFactory;
using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Sets up the legacy entity pipeline implementation.
    /// </summary>
    class LegacyEntityPipelineSetup : IDisposable
    {
        private readonly IEntityPipeline entityPipeline;

        private readonly AssetPreloader assetPreloader;
        private readonly CriticalSectionPipelineBlock criticalSectionPipelineBlock;
        private readonly ThrottledEntityDispatcher throttledEntityDispatcher;
        private readonly LegacyEntityCreator legacyEntityCreator;
        private readonly LegacyComponentPipeline legacyComponentPipeline;
        private readonly EntityComponentUpdater entityComponentUpdater;
        private IEntityTemplateProvider templateProvider;

        public LegacyEntityPipelineSetup(MonoBehaviour hostBehaviour, IEntityPipeline entityPipeline, ISpatialCommunicator spatialCommunicator, IMutableUniverse universe, ILegacyEntityPipelineConfiguration config, IEntitySpawnLimiter spawnLimiter)
        {
            this.entityPipeline = entityPipeline;
            IPrefabFactory<GameObject> prefabFactory;

            if (!config.UsePrefabPooling && config.AssetsToPrePool != null && config.AssetsToPrePool.Any())
            {
                Debug.LogError("There are prefabs specified for pre-pooling, but prefab pooling is not enabled - pooling will occur");
            }

            bool preloaderHasFactory = false;
            if (config.UsePrefabPooling || config.AssetsToPrecache != null || config.AssetsToPrePool != null)
            {
                preloaderHasFactory = true;
#pragma warning disable 0612
                assetPreloader = new AssetPreloader(hostBehaviour,
                                                    config.TemplateProvider,
                                                    config.AssetsToPrecache,
                                                    config.AssetsToPrePool,
                                                    config.MaxConcurrentPrecacheConnections);
#pragma warning restore 0612
                assetPreloader.PrecachingCompleted += () =>
                {
                    if (config.OnPrecachingCompleted != null)
                    {
                        config.OnPrecachingCompleted();
                    }
                };

                assetPreloader.PrecachingProgress += progress =>
                {
                    if (config.OnPrecacheProgress != null)
                    {
                        config.OnPrecacheProgress(progress);
                    }
                };
            }

            if (preloaderHasFactory && config.UsePrefabPooling)
            {
                prefabFactory = assetPreloader.PrefabFactory;
            }
            else
            {
                prefabFactory = new UnityPrefabFactory();
            }

            criticalSectionPipelineBlock = new CriticalSectionPipelineBlock();

            throttledEntityDispatcher = new ThrottledEntityDispatcher(universe, spawnLimiter, config.Metrics);

            legacyEntityCreator = new LegacyEntityCreator(
                                                          config.TemplateProvider,
                                                          spatialCommunicator,
                                                          prefabFactory,
                                                          universe,
                                                          config.EntityComponentInterestOverridesUpdater,
                                                          config.InterestedComponentUpdaterProvider,
                                                          config.Metrics);

            legacyComponentPipeline = new LegacyComponentPipeline(universe);

            entityComponentUpdater = new EntityComponentUpdater(universe);

            templateProvider = config.TemplateProvider;
        }

        public void Setup()
        {
            entityPipeline
                .AddBlock(criticalSectionPipelineBlock)
                .AddBlock(throttledEntityDispatcher)
                .AddBlock(legacyEntityCreator)
                .AddBlock(legacyComponentPipeline)
                .AddBlock(entityComponentUpdater);
        }

        /// <summary>
        ///     Coroutine to prepare assets before establishing connection with SpatialOS.
        /// </summary>
        public IEnumerator PrepareAssets()
        {
            if (assetPreloader == null)
            {
                yield break;
            }

            yield return assetPreloader.PrepareAssets();
        }

        public void Dispose()
        {
            legacyEntityCreator.Dispose();

            if (assetPreloader != null)
            {
                assetPreloader.Dispose();
            }

            if (templateProvider != null)
            {
                templateProvider.CancelAllTemplatePreparations();
            }
        }
    }
}
