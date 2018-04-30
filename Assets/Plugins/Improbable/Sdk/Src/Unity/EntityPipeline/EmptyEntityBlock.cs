// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Empty entity block, added at the end of the pipeline to
    ///     make sure that <see cref="IEntityPipelineBlock.NextEntityBlock" />
    ///     does not need to be checked for null in all block implementations.
    /// </summary>
    sealed class EmptyEntityBlock : IEntityPipelineBlock
    {
        /// <inheritdoc />
        public void AddEntity(AddEntityPipelineOp addEntity) { }

        /// <inheritdoc />
        public void RemoveEntity(RemoveEntityPipelineOp removeEntityOp) { }

        /// <inheritdoc />
        public void CriticalSection(CriticalSectionPipelineOp criticalSectionOp) { }

        /// <inheritdoc />
        public void AddComponent(AddComponentPipelineOp addComponentOp) { }

        /// <inheritdoc />
        public void RemoveComponent(RemoveComponentPipelineOp removeComponentOp) { }

        /// <inheritdoc />
        public void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp) { }

        /// <inheritdoc />
        public void UpdateComponent(UpdateComponentPipelineOp updateComponentOp) { }

        /// <inheritdoc />
        public void ProcessOps() { }

        /// <inheritdoc />
        public IEntityPipelineBlock NextEntityBlock { get; set; }
    }
}
