// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Operational block of the
    /// </summary>
    public interface IEntityPipelineBlock
    {
        /// <summary>
        ///     Called when <see cref="Improbable.Worker.AddEntityOp" /> is received by the worker.
        /// </summary>
        /// <remarks>
        ///     The op will not be automatically passed to the next block.
        ///     Use <see cref="NextEntityBlock" />.<see cref="AddEntity" />(addEntityOp)
        ///     to pass the op to the next block in the pipeline.
        /// </remarks>
        void AddEntity(AddEntityPipelineOp addEntityOp);

        /// <summary>
        ///     Called when <see cref="Improbable.Worker.RemoveEntityOp" /> is received by the worker.
        /// </summary>
        /// <remarks>
        ///     The op will not be automatically passed to the next block.
        ///     Use <see cref="NextEntityBlock" />.<see cref="RemoveEntity" />(removeEntityOp)
        ///     to pass the op to the next block in the pipeline.
        /// </remarks>
        void RemoveEntity(RemoveEntityPipelineOp removeEntityOp);

        /// <summary>
        ///     Called when <see cref="Improbable.Worker.CriticalSectionOp" /> is received by the worker.
        /// </summary>
        /// <remarks>
        ///     The op will not be automatically passed to the next block.
        ///     Use <see cref="NextEntityBlock" />.<see cref="CriticalSection" />(criticalSectionOp)
        ///     to pass the op to the next block in the pipeline.
        /// </remarks>
        void CriticalSection(CriticalSectionPipelineOp criticalSectionOp);

        /// <summary>
        ///     Called when <see cref="AddComponentPipelineOp" /> is received by the worker.
        /// </summary>
        /// <remarks>
        ///     The op will not be automatically passed to the next block.
        ///     Use <see cref="NextEntityBlock" />.<see cref="AddComponent" />(addComponentOp)
        ///     to pass the op to the next block in the pipeline.
        /// </remarks>
        void AddComponent(AddComponentPipelineOp addComponentOp);

        /// <summary>
        ///     Called when <see cref="RemoveComponentPipelineOp" /> is received by the worker.
        /// </summary>
        /// <remarks>
        ///     The op will not be automatically passed to the next block.
        ///     Use <see cref="NextEntityBlock" />.<see cref="RemoveComponentPipelineOp" />(removeComponentOp)
        ///     to pass the op to the next block in the pipeline.
        /// </remarks>
        void RemoveComponent(RemoveComponentPipelineOp removeComponentOp);

        /// <summary>
        ///     Called when <see cref="ChangeAuthorityPipelineOp" /> is received by the worker.
        /// </summary>
        /// <remarks>
        ///     The op will not be automatically passed to the next block.
        ///     Use <see cref="NextEntityBlock" />.<see cref="ChangeAuthority" />(changeAuthorityOp)
        ///     to pass the op to the next block in the pipeline.
        /// </remarks>
        void ChangeAuthority(ChangeAuthorityPipelineOp changeAuthorityOp);

        /// <summary>
        ///     Called when <see cref="UpdateComponentPipelineOp" /> is received by the worker.
        /// </summary>
        /// <remarks>
        ///     The op will not be automatically passed to the next block.
        ///     Use <see cref="NextEntityBlock" />.<see cref="UpdateComponent" />(updateComponentOp)
        ///     to pass the op to the next block in the pipeline.
        /// </remarks>
        void UpdateComponent(UpdateComponentPipelineOp updateComponentOp);

        /// <summary>
        ///     Method called every frame that allows the block to execute logic dependent
        ///     on its internal state.
        /// </summary>
        void ProcessOps();

        /// <summary>
        ///     Reference to the next block in the pipeline.
        /// </summary>
        /// <remarks>
        ///     Guaranteed to be non-null when block is registered with <see cref="IEntityPipeline" />
        ///     and the worker is connected.
        /// </remarks>
        IEntityPipelineBlock NextEntityBlock { get; set; }
    }
}
