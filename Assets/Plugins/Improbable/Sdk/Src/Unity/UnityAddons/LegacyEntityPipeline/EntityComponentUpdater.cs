// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Pipeline block for applying dispatcher ops to generated MonoBehaviour components.
    /// </summary>
    class EntityComponentUpdater : PassOpsBlock
    {
        private readonly IUniverse universe;

        public EntityComponentUpdater(IUniverse universe)
        {
            this.universe = universe;
        }

        /// <inheritdoc />
        public override void AddComponent(AddComponentPipelineOp addComponentOp)
        {
            var entityId = addComponentOp.EntityId;
            if (!universe.ContainsEntity(entityId))
            {
                Debug.LogError("EntityComponentUpdater::AddComponent: Entity not present: " + entityId);
                return;
            }

            var entity = universe.Get(entityId);
            entity.OnAddComponentPipelineOp(addComponentOp);
            NextEntityBlock.AddComponent(addComponentOp);
        }

        /// <inheritdoc />
        public override void RemoveComponent(RemoveComponentPipelineOp removeComponentOp)
        {
            var entityId = removeComponentOp.EntityId;
            if (!universe.ContainsEntity(entityId))
            {
                Debug.LogError("EntityComponentUpdater::RemoveComponent: Entity not present: " + entityId);
                return;
            }

            var entity = universe.Get(entityId);
            entity.OnRemoveComponentPipelineOp(removeComponentOp);
            NextEntityBlock.RemoveComponent(removeComponentOp);
        }

        /// <inheritdoc />
        public override void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp)
        {
            var entityId = changeAuthorityOp.EntityId;
            if (!universe.ContainsEntity(entityId))
            {
                Debug.LogError("EntityComponentUpdater::ChangeAuthority: Entity not present: " + entityId);
                return;
            }

            var entity = universe.Get(entityId);
            entity.OnAuthorityChangePipelineOp(changeAuthorityOp);
            NextEntityBlock.ChangeAuthority(changeAuthorityOp);
        }

        /// <inheritdoc />
        public override void UpdateComponent(UpdateComponentPipelineOp updateComponentOp)
        {
            var entityId = updateComponentOp.EntityId;
            if (!universe.ContainsEntity(entityId))
            {
                Debug.LogError("EntityComponentUpdater::UpdateComponent: Entity not present: " + entityId);
                return;
            }

            var entity = universe.Get(entityId);
            entity.OnComponentUpdatePipelineOp(updateComponentOp);
            NextEntityBlock.UpdateComponent(updateComponentOp);
        }
    }
}
