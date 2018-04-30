// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Worker;

namespace Improbable.Unity.Core
{
    /// <inheritdoc cref="IEntityPipeline" />
    /// <summary>
    ///     Internal interface of the entity pipeline.
    /// </summary>
    /// <remarks>
    ///     Should only be used from within the SDK.
    /// </remarks>
    interface IEntityPipelineInternal : IEntityPipeline, IDisposable
    {
        /// <summary>
        ///     Process any buffered ops.
        /// </summary>
        /// <remarks>
        ///     Needs to be called regularly (e.g. every frame) for the ops
        ///     to progress through the pipeline.
        /// </remarks>
        void ProcessOps();

        /// <summary>
        ///     Enables the processing of ops inside the pipeline.
        /// </summary>
        void Start(ISpatialCommunicator spatialCommunicator);

        /// <summary>
        ///     Registers the pipeline as the receiver of component-related ops.
        /// </summary>
        void RegisterComponentFactories(Connection connection, Dispatcher dispatcher);

        /// <summary>
        ///     Unregisters the pipeline as the receiver of component-related ops.
        /// </summary>
        void UnregisterComponentFactories();

        /// <summary>
        ///     True if the pipeline contains any processing blocks.
        /// </summary>
        bool IsEmpty { get; }
    }
}
