// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Worker;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     The possible types for a pipeline op object.
    /// </summary>
    public enum PipelineOpType
    {
        AddEntity,
        RemoveEntity,
        CriticalSection,
        AddComponent,
        RemoveComponent,
        ChangeAuthority,
        UpdateComponent
    }

    // <summary>
    /// Common parent interface for all pipeline ops.
    /// </summary>
    public interface IEntityPipelineOp
    {
        // <summary>
        /// Type of the pipeline op.
        /// </summary>
        PipelineOpType PipelineOpType { get; }
    }

    /// <summary>
    ///     Pipeline op emitted when an entity is added to the worker.
    /// </summary>
    public struct AddEntityPipelineOp : IEntityPipelineOp
    {
        /// <summary>
        ///     Dispatcher op corresponding to the pipeline op.
        /// </summary>
        public AddEntityOp DispatcherOp;

        /// <summary>
        ///     Entity id of corresponding entity.
        /// </summary>
        public EntityId EntityId
        {
            get { return DispatcherOp.EntityId; }
        }

        /// <inheritdoc />
        public PipelineOpType PipelineOpType
        {
            get { return PipelineOpType.AddEntity; }
        }
    }

    /// <summary>
    ///     Pipeline op emitted when an entity is removed from the worker.
    /// </summary>
    public struct RemoveEntityPipelineOp : IEntityPipelineOp
    {
        /// <summary>
        ///     Dispatcher op corresponding to the pipeline op.
        /// </summary>
        public RemoveEntityOp DispatcherOp;

        /// <summary>
        ///     Entity id of corresponding entity.
        /// </summary>
        public EntityId EntityId
        {
            get { return DispatcherOp.EntityId; }
        }

        /// <inheritdoc />
        public PipelineOpType PipelineOpType
        {
            get { return PipelineOpType.RemoveEntity; }
        }
    }

    /// <summary>
    ///     Pipeline Op emitted when the worker enters/exits a critical section.
    /// </summary>
    public struct CriticalSectionPipelineOp : IEntityPipelineOp
    {
        /// <summary>
        ///     Dispatcher op corresponding to the pipeline op.
        /// </summary>
        public CriticalSectionOp DispatcherOp;

        /// <summary>
        ///     Indicate whether worker is about to enter a critical section (true) or exit a critical section (false).
        /// </summary>
        public bool InCriticalSection
        {
            get { return DispatcherOp.InCriticalSection; }
        }

        /// <inheritdoc />
        public PipelineOpType PipelineOpType
        {
            get { return PipelineOpType.CriticalSection; }
        }
    }

    /// <summary>
    ///     Pipeline op emitted when a SpatialOS component is added to an entity.
    /// </summary>
    public struct AddComponentPipelineOp : IEntityPipelineOp
    {
        /// <summary>
        ///     Id of the SpatialOS entity the component was added to.
        /// </summary>
        public EntityId EntityId;

        /// <summary>
        ///     The object that corresponds to the affected SpatialOS component.
        /// </summary>
        public object ComponentObject;

        /// <summary>
        ///     Type of the added component.
        /// </summary>
        public IComponentMetaclass ComponentMetaClass;

        /// <inheritdoc />
        public PipelineOpType PipelineOpType
        {
            get { return PipelineOpType.AddComponent; }
        }
    }

    /// <summary>
    ///     Pipeline op emitted when a SpatialOS component is removed from an entity.
    /// </summary>
    public struct RemoveComponentPipelineOp : IEntityPipelineOp
    {
        /// <summary>
        ///     Id of the SpatialOS entity the component was removed from.
        /// </summary>
        public EntityId EntityId;

        /// <summary>
        ///     The object that corresponds to the affected SpatialOS component.
        /// </summary>
        public object ComponentObject;

        /// <summary>
        ///     Type of the removed component.
        /// </summary>
        public IComponentMetaclass ComponentMetaClass;

        /// <inheritdoc />
        public PipelineOpType PipelineOpType
        {
            get { return PipelineOpType.RemoveComponent; }
        }
    }

    /// <summary>
    ///     Pipeline op emitted when authority for a SpatialOS component changes.
    /// </summary>
    public struct ChangeAuthorityPipelineOp : IEntityPipelineOp
    {
        /// <summary>
        ///     Id of the SpatialOS entity on which the authority over a component changed.
        /// </summary>
        public EntityId EntityId;

        /// <summary>
        ///     The object that corresponds to the affected SpatialOS component.
        /// </summary>
        public object ComponentObject;

        /// <summary>
        ///     Type of the affected component.
        /// </summary>
        public IComponentMetaclass ComponentMetaClass;

        /// <summary>
        ///     Indicates whether the worker received authority over the component.
        /// </summary>
        [System.Obsolete("Please use \"Authority == Improbable.Worker.Authority.Authoritative || Authority == Improbable.Worker.Authority.AuthorityLossImminent\".")]
        public bool HasAuthority
        {
            get { return Authority == Authority.Authoritative || Authority == Authority.AuthorityLossImminent; }
        }

        /// <summary>
        ///     Indicates the state of authority over the component.
        /// </summary>
        public Authority Authority;

        /// <inheritdoc />
        public PipelineOpType PipelineOpType
        {
            get { return PipelineOpType.ChangeAuthority; }
        }
    }

    /// <summary>
    ///     Pipeline op emitted when a component update is received.
    /// </summary>
    public struct UpdateComponentPipelineOp : IEntityPipelineOp
    {
        /// <summary>
        ///     Id of the SpatialOS entity that is receiving the component update.
        /// </summary>
        public EntityId EntityId;

        /// <summary>
        ///     The update object that corresponds to the affected SpatialOS component.
        /// </summary>
        public object UpdateObject;

        /// <summary>
        ///     Type of the affected component.
        /// </summary>
        public IComponentMetaclass ComponentMetaClass;

        /// <inheritdoc />
        public PipelineOpType PipelineOpType
        {
            get { return PipelineOpType.UpdateComponent; }
        }
    }
}
