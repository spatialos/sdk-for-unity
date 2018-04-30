// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.Entity;
using Improbable.Unity.Metrics;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Entity pipeline block that limits the number of entity creation
    ///     ops that can be dispatched between consecutive <see cref="ProcessOps" />
    ///     calls (usually one frame). Queues the ops relevant to deferred entities
    ///     and releases them once AddEntity ops have been dispatched.
    /// </summary>
    public class ThrottledEntityDispatcher : IEntityPipelineBlock
    {
        private const string QueueSizeMetricName = "Deferred Entity Creation Queue Size";

        /// <summary>
        ///     A queue of entities that have been deferred. Order is maintained here so that entities are processed in the order
        ///     they were originally received.
        /// </summary>
        private readonly Queue<EntityId> entitiesToAdd = new Queue<EntityId>();

        /// <summary>
        ///     A mapping between an entity and all of its deferred operations.
        /// </summary>
        private readonly Dictionary<EntityId, Queue<IEntityPipelineOp>> entityActions = new Dictionary<EntityId, Queue<IEntityPipelineOp>>();

        private readonly WorkerMetrics workerMetrics;

        private readonly IUniverse universe;
        private readonly IEntitySpawnLimiter spawnLimiter;

        public ThrottledEntityDispatcher(IUniverse universe, IEntitySpawnLimiter spawnLimiter, WorkerMetrics workerMetrics)
        {
            this.universe = universe;
            this.spawnLimiter = spawnLimiter;
            this.workerMetrics = workerMetrics;
        }

        /// <inheritdoc />
        public void AddEntity(AddEntityPipelineOp addEntityOp)
        {
            if (spawnLimiter.CanSpawnEntity() && !IsEntityQueued(addEntityOp.EntityId))
            {
                NextEntityBlock.AddEntity(addEntityOp);
                spawnLimiter.EntityAdded(addEntityOp.EntityId);
            }
            else
            {
                Queue<IEntityPipelineOp> queue;
                if (!entityActions.TryGetValue(addEntityOp.EntityId, out queue))
                {
                    queue = entityActions[addEntityOp.EntityId] = new Queue<IEntityPipelineOp>();
                    workerMetrics.IncrementGauge(QueueSizeMetricName);
                    entitiesToAdd.Enqueue(addEntityOp.EntityId);
                }

                queue.Enqueue(addEntityOp);
            }
        }

        /// <inheritdoc />
        public void RemoveEntity(RemoveEntityPipelineOp removeEntityOp)
        {
            var entityId = removeEntityOp.EntityId;
            if (IsEntityQueued(entityId))
            {
                entityActions[entityId].Enqueue(removeEntityOp);
            }
            else
            {
                NextEntityBlock.RemoveEntity(removeEntityOp);
            }
        }

        /// <inheritdoc />
        public void CriticalSection(CriticalSectionPipelineOp criticalSectionOp) { }

        /// <inheritdoc />
        public void AddComponent(AddComponentPipelineOp addComponentOp)
        {
            var entityId = addComponentOp.EntityId;
            if (IsEntityQueued(entityId))
            {
                entityActions[entityId].Enqueue(addComponentOp);
            }
            else
            {
                NextEntityBlock.AddComponent(addComponentOp);
            }
        }

        /// <inheritdoc />
        public void RemoveComponent(RemoveComponentPipelineOp removeComponentOp)
        {
            var entityId = removeComponentOp.EntityId;
            if (IsEntityQueued(entityId))
            {
                entityActions[entityId].Enqueue(removeComponentOp);
            }
            else
            {
                NextEntityBlock.RemoveComponent(removeComponentOp);
            }
        }

        /// <inheritdoc />
        public void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp)
        {
            var entityId = changeAuthorityOp.EntityId;
            if (IsEntityQueued(entityId))
            {
                entityActions[entityId].Enqueue(changeAuthorityOp);
            }
            else
            {
                NextEntityBlock.ChangeAuthority(changeAuthorityOp);
            }
        }

        /// <inheritdoc />
        public void UpdateComponent(UpdateComponentPipelineOp updateComponentOp)
        {
            var entityId = updateComponentOp.EntityId;
            if (IsEntityQueued(entityId))
            {
                entityActions[entityId].Enqueue(updateComponentOp);
            }
            else
            {
                NextEntityBlock.UpdateComponent(updateComponentOp);
            }
        }

        private bool IsEntityQueued(EntityId entityId)
        {
            return entityActions.ContainsKey(entityId);
        }

        /// <inheritdoc />
        public void ProcessOps()
        {
            spawnLimiter.Reset();

            while (entitiesToAdd.Count > 0)
            {
                var entityId = entitiesToAdd.Dequeue();
                var actions = entityActions[entityId];
                while (actions.Count > 0)
                {
                    var op = actions.Dequeue();
                    try // Ensure that exceptions in user code (e.g. OnEnable) don't disrupt processing of the entire queue.
                    {
                        NextEntityBlock.DispatchOp(op);
                    }
                    catch (Exception ex)
                    {
                        OnError(entityId, ex);
                    }
                }

                entityActions.Remove(entityId);
                workerMetrics.DecrementGauge(QueueSizeMetricName);
                spawnLimiter.EntityAdded(entityId);

                if (!spawnLimiter.CanSpawnEntity())
                {
                    break;
                }
            }
        }

        private void OnError(EntityId entityId, Exception ex)
        {
            GameObject context = null;

            var entityObject = universe.Get(entityId);
            if (entityObject != null)
            {
                context = entityObject.UnderlyingGameObject;
            }

            Debug.LogException(ex, context);
        }

        /// <inheritdoc />
        public IEntityPipelineBlock NextEntityBlock { get; set; }
    }
}
