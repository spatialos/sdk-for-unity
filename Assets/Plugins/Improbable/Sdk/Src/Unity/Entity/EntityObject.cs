// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Contains information about a SpatialOS entity and its associated GameObject.
    /// </summary>
    class EntityObject : IEntityObject, IEntityInterestedComponentsInvalidator, IDisposable
    {
        private readonly IInterestedComponentUpdaterProvider interestedComponentUpdaterProvider;

        /// <summary>
        ///     Creates a new instance of <c>EntityObject</c>.
        /// </summary>
        /// <param name="entityId">The associated EntityId.</param>
        /// <param name="underlyingGameObject">The GameObject associated with the Entity.</param>
        /// <param name="prefabName">The prefab name associated with the Entity.</param>
        /// <param name="interestedComponentUpdaterProvider">
        ///     IInterestedComponentUpdaterProvider for callign
        ///     OnInterestedComponentsPotentiallyChanged().
        /// </param>
        public EntityObject(EntityId entityId, GameObject underlyingGameObject, string prefabName, IInterestedComponentUpdaterProvider interestedComponentUpdaterProvider)
        {
            this.interestedComponentUpdaterProvider = interestedComponentUpdaterProvider;

            if (underlyingGameObject == null)
            {
                throw new ArgumentNullException("underlyingGameObject");
            }

            EntityId = entityId;
            UnderlyingGameObject = underlyingGameObject;
            PrefabName = prefabName;

            Visualizers = new EntityVisualizers(UnderlyingGameObject);
            Components = new EntityComponents();
            Visualizers.AddInvalidator(this);
            Components.AddInvalidator(this);
        }

        /// <inheritdoc />
        public IEntityVisualizers Visualizers { get; private set; }

        /// <inheritdoc />
        public IEntityComponents Components { get; private set; }

        /// <inheritdoc />
        public string PrefabName { get; private set; }

        /// <inheritdoc />
        public GameObject UnderlyingGameObject { get; private set; }

        /// <inheritdoc />
        public EntityId EntityId { get; private set; }

        /// <inheritdoc />
        public void OnInterestedComponentsPotentiallyChanged()
        {
            interestedComponentUpdaterProvider.GetEntityInterestedComponentsUpdater().InvalidateEntity(this);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Visualizers.RemoveInvalidator(this);
            ((EntityVisualizers) Visualizers).Dispose();
            Components.RemoveInvalidator(this);

            if (UnderlyingGameObject != null)
            {
                UnderlyingGameObject.GetComponent<EntityObjectStorage>().Clear();
            }
        }

        public override string ToString()
        {
            return string.Format("Entity: {0}, id: {1} prefab: {2}", UnderlyingGameObject.name, EntityId, PrefabName);
        }

        /// <inheritdoc />
        public void OnAddComponentPipelineOp(AddComponentPipelineOp op)
        {
            if (op.EntityId != EntityId)
            {
                Debug.LogError(string.Format("EntityObject::OnAddComponentPipelineOp: Entity {0} received pipeline op for wrong entity id {1}.", EntityId, op.EntityId));
                return;
            }

            var componentId = op.ComponentMetaClass.ComponentId;
            if (!Components.RegisteredComponents.ContainsKey(componentId))
            {
                return;
            }

            Components.RegisteredComponents[componentId].OnAddComponentPipelineOp(op);
        }

        /// <inheritdoc />
        public void OnRemoveComponentPipelineOp(RemoveComponentPipelineOp op)
        {
            if (op.EntityId != EntityId)
            {
                Debug.LogError(string.Format("EntityObject::OnRemoveComponentPipelineOp: Entity {0} received pipeline op for wrong entity id {1}.", EntityId, op.EntityId));
                return;
            }

            var componentId = op.ComponentMetaClass.ComponentId;
            if (!Components.RegisteredComponents.ContainsKey(componentId))
            {
                return;
            }

            Components.RegisteredComponents[componentId].OnRemoveComponentPipelineOp(op);
        }

        /// <inheritdoc />
        public void OnComponentUpdatePipelineOp(UpdateComponentPipelineOp op)
        {
            if (op.EntityId != EntityId)
            {
                Debug.LogError(string.Format("EntityObject::OnComponentUpdatePipelineOp: Entity {0} received pipeline op for wrong entity id {1}.", EntityId, op.EntityId));
                return;
            }

            var componentId = op.ComponentMetaClass.ComponentId;
            if (!Components.RegisteredComponents.ContainsKey(componentId))
            {
                return;
            }

            Components.RegisteredComponents[componentId].OnComponentUpdatePipelineOp(op);
        }

        /// <inheritdoc />
        public void OnAuthorityChangePipelineOp(ChangeAuthorityPipelineOp op)
        {
            if (op.EntityId != EntityId)
            {
                Debug.LogError(string.Format("EntityObject::OnAuthorityChangePipelineOp: Entity {0} received pipeline op for wrong entity id {1}.", EntityId, op.EntityId));
                return;
            }

            var componentId = op.ComponentMetaClass.ComponentId;
            if (!Components.RegisteredComponents.ContainsKey(componentId))
            {
                return;
            }

            Components.RegisteredComponents[componentId].OnAuthorityChangePipelineOp(op);
        }
    }
}
