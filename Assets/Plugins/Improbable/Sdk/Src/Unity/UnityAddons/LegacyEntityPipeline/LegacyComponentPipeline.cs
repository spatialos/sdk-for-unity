// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Entity;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Pre SpatialOS 10.4 implementation of the Entity Pipeline.
    /// </summary>
    class LegacyComponentPipeline : PassOpsBlock
    {
        private readonly IMutableUniverse universe;

        public LegacyComponentPipeline(IMutableUniverse universe)
        {
            this.universe = universe;
        }

        #region EntityEventHandler

        /// <inheritdoc />
        public override void AddComponent(AddComponentPipelineOp addComponentOp)
        {
            var entity = universe.Get(addComponentOp.EntityId);
            if (entity != null)
            {
                if (entity.Visualizers is EntityVisualizers)
                {
                    ((EntityVisualizers) entity.Visualizers).OnComponentAdded(addComponentOp.ComponentMetaClass, addComponentOp.ComponentObject);
                }
            }
            else
            {
                Debug.LogErrorFormat("Entity not found: OnComponentAdded({0}, {1})", addComponentOp.EntityId, addComponentOp.ComponentMetaClass);
            }

            NextEntityBlock.AddComponent(addComponentOp);
        }

        /// <inheritdoc />
        public override void RemoveComponent(RemoveComponentPipelineOp removeComponentOp)
        {
            NextEntityBlock.RemoveComponent(removeComponentOp);
            var entity = universe.Get(removeComponentOp.EntityId);
            if (entity != null)
            {
                if (entity.Visualizers is EntityVisualizers)
                {
                    ((EntityVisualizers) entity.Visualizers).OnComponentRemoved(removeComponentOp.ComponentMetaClass, removeComponentOp.ComponentObject);
                }
            }
            else
            {
                Debug.LogErrorFormat("Entity not found: OnComponentRemoved({0}, {1})", removeComponentOp.EntityId, removeComponentOp.ComponentMetaClass);
            }
        }

        /// <inheritdoc />
        public override void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp)
        {
            NextEntityBlock.ChangeAuthority(changeAuthorityOp);
        }

        #endregion
    }
}
