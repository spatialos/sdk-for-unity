// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Logic for handling operations related to the lifetime
    ///     of SpatialOS entity and component objects.
    /// </summary>
    public interface IEntityPipeline
    {
        /// <summary>
        ///     Adds a processing block at the end of the pipeline.
        /// </summary>
        IEntityPipeline AddBlock(IEntityPipelineBlock block);
    }
}
