// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Entity pipeline block that blocks all pipeline ops from continuing
    ///     through the pipeline until the end of the critical section is reached.
    ///     At this point all of the ops within the critical section are
    ///     dispatched sequentially to the next pipeline block. Subsequent pipeline
    ///     blocks will not receive any critical section ops.
    /// </summary>
    class CriticalSectionPipelineBlock : IEntityPipelineBlock
    {
        private bool inCriticalSection;

        private readonly Queue<IEntityPipelineOp> criticalSectionEntityOps = new Queue<IEntityPipelineOp>();

        /// <inheritdoc />
        public void AddEntity(AddEntityPipelineOp addEntityOp)
        {
            if (!inCriticalSection)
            {
                Debug.LogErrorFormat("CriticalSectionPipelineBlock received AddEntityPipelineOp for entity {0} outside of critical section", addEntityOp.EntityId);
                return;
            }

            criticalSectionEntityOps.Enqueue(addEntityOp);
        }

        /// <inheritdoc />
        public void RemoveEntity(RemoveEntityPipelineOp removeEntityOp)
        {
            StallForCriticalSection(removeEntityOp);
        }

        /// <inheritdoc />
        public void CriticalSection(CriticalSectionPipelineOp criticalSectionOp)
        {
            if (inCriticalSection)
            {
                if (criticalSectionOp.InCriticalSection)
                {
                    Debug.LogError("CriticalSectionPipelineBlock received critical section start during another critical section");
                }
                else
                {
                    inCriticalSection = false;
                    ReleaseCriticalSectionOps();
                }
            }
            else
            {
                if (criticalSectionOp.InCriticalSection)
                {
                    inCriticalSection = true;
                }
                else
                {
                    Debug.LogError("CriticalSectionPipelineBlock received critical section stop outside a critical section");
                }
            }
        }

        private void ReleaseCriticalSectionOps()
        {
            while (criticalSectionEntityOps.Count > 0)
            {
                NextEntityBlock.DispatchOp(criticalSectionEntityOps.Dequeue());
            }
        }

        /// <inheritdoc />
        public void AddComponent(AddComponentPipelineOp addComponentOp)
        {
            StallForCriticalSection(addComponentOp);
        }

        /// <inheritdoc />
        public void RemoveComponent(RemoveComponentPipelineOp removeComponentOp)
        {
            StallForCriticalSection(removeComponentOp);
        }

        /// <inheritdoc />
        public void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp)
        {
            StallForCriticalSection(changeAuthorityOp);
        }

        /// <inheritdoc />
        public void UpdateComponent(UpdateComponentPipelineOp updateComponentOp)
        {
            StallForCriticalSection(updateComponentOp);
        }

        private void StallForCriticalSection(IEntityPipelineOp entityPipelineOp)
        {
            if (!inCriticalSection)
            {
                NextEntityBlock.DispatchOp(entityPipelineOp);
                return;
            }

            criticalSectionEntityOps.Enqueue(entityPipelineOp);
        }

        /// <inheritdoc />
        public void ProcessOps() { }

        public IEntityPipelineBlock NextEntityBlock { get; set; }
    }
}
