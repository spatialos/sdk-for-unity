// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Core;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Provides an interface for accepting component related entity pipeline ops.
    /// </summary>
    public interface IPipelineEntityComponentOpsReceiver
    {
        /// <summary>
        ///     Endpoint for incoming AddComponent pipeline ops.
        /// </summary>
        void OnAddComponentPipelineOp(AddComponentPipelineOp op);

        /// <summary>
        ///     Endpoint for incoming RemoveComponent pipeline ops.
        /// </summary>
        void OnRemoveComponentPipelineOp(RemoveComponentPipelineOp op);

        /// <summary>
        ///     Endpoint for incoming UpdateComponent pipeline ops.
        /// </summary>
        void OnComponentUpdatePipelineOp(UpdateComponentPipelineOp op);

        /// <summary>
        ///     Endpoint for incoming ChangeAuthority pipeline ops.
        /// </summary>
        void OnAuthorityChangePipelineOp(ChangeAuthorityPipelineOp op);
    }
}
