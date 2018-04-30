// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using Improbable.Unity.ComponentFactory;
using Improbable.Unity.Entity;
using Improbable.Unity.Metrics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Pre SpatialOS 10.4 implementation of the Entity Pipeline.
    /// </summary>
    class LegacyEntityCreator : IEntityPipelineBlock, IDisposable
    {
        private class EntitySpawnData
        {
            internal string EntityType { get; set; }
            internal PositionData? Position { get; set; }
            internal readonly Queue<IEntityPipelineOp> StalledOps;
            internal bool IsLoading { get; set; }

            internal EntitySpawnData()
            {
                StalledOps = new Queue<IEntityPipelineOp>();
            }
        }

        private const string EntityCountGaugeName = "Entity Count";

        private readonly IEntityTemplateProvider templateProvider;
        private readonly IPrefabFactory<GameObject> prefabFactory;
        private readonly ISpatialCommunicator spatialCommunicator;

        private readonly IMutableUniverse universe;
        private readonly IEntityComponentInterestOverridesUpdater entityComponentInterestOverridesUpdater;
        private readonly IInterestedComponentUpdaterProvider interestedComponentUpdaterProvider;

        private readonly HashSet<EntityId> knownEntities;
        private readonly IDictionary<EntityId, EntitySpawnData> entitiesToSpawn;

        private readonly WorkerMetrics metrics;

        // The scene that Entities will always be spawned into.
        private readonly Scene entitySpawningScene = SceneManager.GetActiveScene();

        private bool disposed;

        public LegacyEntityCreator(IEntityTemplateProvider templateProvider,
                                   ISpatialCommunicator spatialCommunicator,
                                   IPrefabFactory<GameObject> prefabFactory,
                                   IMutableUniverse universe,
                                   IEntityComponentInterestOverridesUpdater entityComponentInterestOverridesUpdater,
                                   IInterestedComponentUpdaterProvider interestedComponentUpdaterProvider,
                                   WorkerMetrics metrics)
        {
            this.templateProvider = templateProvider;
            this.prefabFactory = new PrefabFactoryMetrics(prefabFactory, metrics); // Associate metrics with the factory
            this.spatialCommunicator = spatialCommunicator;
            this.universe = universe;
            this.entityComponentInterestOverridesUpdater = entityComponentInterestOverridesUpdater;
            this.interestedComponentUpdaterProvider = interestedComponentUpdaterProvider;

            entitiesToSpawn = new Dictionary<EntityId, EntitySpawnData>();
            knownEntities = new HashSet<EntityId>();

            this.metrics = metrics;
        }

        #region EntityEventHandler

        /// <inheritdoc />
        public void AddEntity(AddEntityPipelineOp addEntityOp)
        {
            if (!IsEntityTracked(addEntityOp.EntityId))
            {
                knownEntities.Add(addEntityOp.EntityId);
                var spawnData = new EntitySpawnData();
                entitiesToSpawn.Add(addEntityOp.EntityId, spawnData);
                spawnData.StalledOps.Enqueue(addEntityOp);
                metrics.IncrementGauge(EntityCountGaugeName);
            }
            else
            {
                Debug.LogErrorFormat("Trying to add the entity '{0}' that already exists.", addEntityOp.EntityId);
            }
        }

        private void MakeEntity(EntityId entityId, string prefabName, Action<EntityId, string> onSuccess)
        {
            entitiesToSpawn[entityId].IsLoading = true;
            templateProvider.PrepareTemplate(prefabName,
                                             _ => { onSuccess(entityId, prefabName); },
                                             exception => { Debug.LogError("Exception occurred when preparing " + prefabName + " template. " + exception); });
        }

        private IEntityObject InstantiateEntityObject(EntityId entityId, string prefabName)
        {
            // Always ensure that entities are spawned into the same scene that this script was started in.
            // This avoids the following case:
            //  1) Users dynamically loads scene, activates it, spawns their own entities into it.
            //  2) A new entity is spawned by SpatialOS into the new scene.
            //  3) The user destroys the scene, also destroying the SpatialOS-created GameObject.
            //  4) Errors ensue when the user tries to access the destroyed GameObject via the SpatialOS GameObject tracking layer (e.g. Universe.)
            using (new SaveAndRestoreScene(entitySpawningScene))
            {
                var loadedPrefab = templateProvider.GetEntityTemplate(prefabName);
                var underlyingGameObject = prefabFactory.MakeComponent(loadedPrefab, prefabName);

                var entityObject = new EntityObject(entityId, underlyingGameObject, prefabName,
                                                    interestedComponentUpdaterProvider);

                var entityObjectStorage =
                    underlyingGameObject.GetComponent<EntityObjectStorage>() ??
                    underlyingGameObject.AddComponent<EntityObjectStorage>();

                entityObjectStorage.Initialize(entityObject, spatialCommunicator);

                return entityObject;
            }
        }

        // Releases the entity from this stage of the pipeline
        private void ReleaseEntity(EntityId entityId)
        {
            var entitySpawnData = entitiesToSpawn[entityId];
            while (entitySpawnData.StalledOps.Count > 0)
            {
                NextEntityBlock.DispatchOp(entitySpawnData.StalledOps.Dequeue());
            }

            entitiesToSpawn.Remove(entityId);
        }

        /// <inheritdoc />
        public void RemoveEntity(RemoveEntityPipelineOp removeEntityOp)
        {
            if (!IsEntityTracked(removeEntityOp.EntityId))
            {
                Debug.LogErrorFormat("Trying to destroy an entity we don't have: {0}", removeEntityOp.EntityId);
                return;
            }

            metrics.DecrementGauge(EntityCountGaugeName);

            if (universe.ContainsEntity(removeEntityOp.EntityId))
            {
                NextEntityBlock.RemoveEntity(removeEntityOp);
                var entity = universe.Get(removeEntityOp.EntityId);
                DestroyEntity(entity);
            }

            if (entitiesToSpawn.ContainsKey(removeEntityOp.EntityId))
            {
                entitiesToSpawn.Remove(removeEntityOp.EntityId);
            }

            knownEntities.Remove(removeEntityOp.EntityId);
        }

        private void DestroyEntity(IEntityObject entity)
        {
            var disposable = entity as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            entityComponentInterestOverridesUpdater.RemoveEntity(entity);

            universe.Remove(entity.EntityId);

            prefabFactory.DespawnComponent(entity.UnderlyingGameObject, entity.PrefabName);
        }

        /// <inheritdoc />
        public void CriticalSection(CriticalSectionPipelineOp criticalSectionOp) { }

        /// <inheritdoc />
        public void AddComponent(AddComponentPipelineOp addComponentOp)
        {
            if (!IsEntityTracked(addComponentOp.EntityId))
            {
                Debug.LogErrorFormat("Entity not found: OnComponentAdded({0}, {1})", addComponentOp.EntityId,
                                     addComponentOp.ComponentMetaClass);
                return;
            }

            if (universe.ContainsEntity(addComponentOp.EntityId))
            {
                NextEntityBlock.AddComponent(addComponentOp);
                return;
            }

            if (entitiesToSpawn.ContainsKey(addComponentOp.EntityId))
            {
                var entityData = entitiesToSpawn[addComponentOp.EntityId];

                entityData.StalledOps.Enqueue(addComponentOp);

                if (addComponentOp.ComponentMetaClass.ComponentId == Position.ComponentId)
                {
                    entityData.Position = ((Position.Impl) addComponentOp.ComponentObject).Data;
                    if (entityData.EntityType != null)
                    {
                        MakeEntity(addComponentOp.EntityId, entityData.EntityType, OnMakeEntitySuccess);
                    }
                }
                else if (addComponentOp.ComponentMetaClass.ComponentId == Metadata.ComponentId)
                {
                    entityData.EntityType = ((Metadata.Impl) addComponentOp.ComponentObject).Data.entityType;
                    if (entityData.Position.HasValue)
                    {
                        MakeEntity(addComponentOp.EntityId, entityData.EntityType, OnMakeEntitySuccess);
                    }
                }
            }
        }

        private void OnMakeEntitySuccess(EntityId entityId, string prefabName)
        {
            if (entitiesToSpawn.ContainsKey(entityId))
            {
                var entityObject = InstantiateEntityObject(entityId, prefabName);
                universe.AddEntity(entityObject);
                ReleaseEntity(entityId);
            }
        }

        /// <inheritdoc />
        public void RemoveComponent(RemoveComponentPipelineOp removeComponentOp)
        {
            if (!IsEntityTracked(removeComponentOp.EntityId))
            {
                Debug.LogErrorFormat("Entity not found: OnComponentAdded({0}, {1})", removeComponentOp.EntityId,
                                     removeComponentOp.ComponentMetaClass);
                return;
            }

            if (universe.ContainsEntity(removeComponentOp.EntityId))
            {
                NextEntityBlock.RemoveComponent(removeComponentOp);
                return;
            }

            if (entitiesToSpawn.ContainsKey(removeComponentOp.EntityId))
            {
                var data = entitiesToSpawn[removeComponentOp.EntityId];
                data.StalledOps.Enqueue(removeComponentOp);

                if (removeComponentOp.ComponentMetaClass.ComponentId == Position.ComponentId)
                {
                    entitiesToSpawn[removeComponentOp.EntityId].Position = null;
                }
            }
        }

        /// <inheritdoc />
        public void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp)
        {
            if (!IsEntityTracked(changeAuthorityOp.EntityId))
            {
                Debug.LogErrorFormat("Entity not found: OnAuthorityChanged({0}, {1}) authority: {2}",
                                     changeAuthorityOp.EntityId, changeAuthorityOp.ComponentMetaClass, changeAuthorityOp.Authority);
                return;
            }

            if (universe.ContainsEntity(changeAuthorityOp.EntityId))
            {
                NextEntityBlock.ChangeAuthority(changeAuthorityOp);
                return;
            }

            if (entitiesToSpawn.ContainsKey(changeAuthorityOp.EntityId))
            {
                entitiesToSpawn[changeAuthorityOp.EntityId].StalledOps.Enqueue(changeAuthorityOp);
            }
        }

        /// <inheritdoc />
        public void UpdateComponent(UpdateComponentPipelineOp updateComponentOp)
        {
            if (!IsEntityTracked(updateComponentOp.EntityId))
            {
                Debug.LogErrorFormat("Entity not found: OnComponentUpdate({0}, {1}).",
                                     updateComponentOp.EntityId, updateComponentOp.ComponentMetaClass);
                return;
            }

            if (universe.ContainsEntity(updateComponentOp.EntityId))
            {
                NextEntityBlock.UpdateComponent(updateComponentOp);
                return;
            }

            if (entitiesToSpawn.ContainsKey(updateComponentOp.EntityId))
            {
                entitiesToSpawn[updateComponentOp.EntityId].StalledOps.Enqueue(updateComponentOp);
            }
        }

        #endregion

        /// <inheritdoc />
        public void ProcessOps() { }

        private void DestroyEntities()
        {
            foreach (var entityObject in universe.GetAll().ToList())
            {
                DestroyEntity(entityObject);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                DestroyEntities();
            }
        }

        /// <inheritdoc />
        public IEntityPipelineBlock NextEntityBlock { get; set; }

        private bool IsEntityTracked(EntityId entityId)
        {
            return knownEntities.Contains(entityId);
        }
    }
}
