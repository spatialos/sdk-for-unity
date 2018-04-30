// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Entity pipeline block that passes through all operations.
    /// </summary>
    /// <remarks>
    ///     You may use this as a base class to avoid boilerplate code when
    ///     writing entity pipeline blocks that only process some of the
    ///     op streams exposed via the interface.
    ///     The class is abstract, as it does nothing meaningful on its own.
    /// </remarks>
    public abstract class PassOpsBlock : IEntityPipelineBlock
    {
        /// <inheritdoc />
        public virtual void AddEntity(AddEntityPipelineOp addEntity)
        {
            NextEntityBlock.AddEntity(addEntity);
        }

        /// <inheritdoc />
        public virtual void RemoveEntity(RemoveEntityPipelineOp removeEntityOp)
        {
            NextEntityBlock.RemoveEntity(removeEntityOp);
        }

        /// <inheritdoc />
        public virtual void CriticalSection(CriticalSectionPipelineOp criticalSectionOp)
        {
            NextEntityBlock.CriticalSection(criticalSectionOp);
        }

        /// <inheritdoc />
        public virtual void AddComponent(AddComponentPipelineOp addComponentOp)
        {
            NextEntityBlock.AddComponent(addComponentOp);
        }

        /// <inheritdoc />
        public virtual void RemoveComponent(RemoveComponentPipelineOp removeComponentOp)
        {
            NextEntityBlock.RemoveComponent(removeComponentOp);
        }

        /// <inheritdoc />
        public virtual void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp)
        {
            NextEntityBlock.ChangeAuthority(changeAuthorityOp);
        }

        /// <inheritdoc />
        public virtual void UpdateComponent(UpdateComponentPipelineOp updateComponentOp)
        {
            NextEntityBlock.UpdateComponent(updateComponentOp);
        }

        /// <inheritdoc />
        public virtual void ProcessOps() { }

        /// <inheritdoc />
        public IEntityPipelineBlock NextEntityBlock { get; set; }
    }
}
