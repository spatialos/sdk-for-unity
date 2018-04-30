// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Entity.Component;
using Improbable.Worker;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     A collection of utility extension methods to use with <see cref="IEntityPipelineBlock" /> objects.
    /// </summary>
    public static class EntityPipelineExtensions
    {
        /// <summary>
        ///     Class for wrapping the (EntityId, IComponentMetaclass, object)
        ///     method argument sets into a discrete ops.
        /// </summary>
        private class FactoryCallbackWrapper
        {
            private readonly IEntityPipelineBlock block;

            internal FactoryCallbackWrapper(IEntityPipelineBlock block)
            {
                this.block = block;
            }

            internal void AddComponent(EntityId entityId, IComponentMetaclass componentMetaclass, object component)
            {
                block.AddComponent(new AddComponentPipelineOp { EntityId = entityId, ComponentMetaClass = componentMetaclass, ComponentObject = component });
            }

            internal void RemoveComponent(EntityId entityId, IComponentMetaclass componentMetaclass, object component)
            {
                block.RemoveComponent(new RemoveComponentPipelineOp { EntityId = entityId, ComponentMetaClass = componentMetaclass, ComponentObject = component });
            }

            internal void ChangeAuthority(EntityId entityId, IComponentMetaclass componentMetaclass, Authority authority, object component)
            {
                block.ChangeAuthority(new ChangeAuthorityPipelineOp { EntityId = entityId, ComponentMetaClass = componentMetaclass, ComponentObject = component, Authority = authority });
            }

            internal void UpdateComponent(EntityId entityId, IComponentMetaclass componentMetaclass, object update)
            {
                block.UpdateComponent(new UpdateComponentPipelineOp { EntityId = entityId, ComponentMetaClass = componentMetaclass, UpdateObject = update });
            }
        }

        /// <summary>
        ///     Registers the given <see cref="IEntityPipelineBlock" /> as <see cref="ComponentFactoryCallbacks" />
        ///     objects used in base SDK <see cref="IComponentFactory" /> instances in generated code.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static ComponentFactoryCallbacks ToComponentFactoryCallbacks(this IEntityPipelineBlock block)
        {
            var wrapper = new FactoryCallbackWrapper(block);
            return new ComponentFactoryCallbacks
            {
                OnComponentAdded = wrapper.AddComponent,
                OnComponentRemoved = wrapper.RemoveComponent,
                OnAuthorityChanged = wrapper.ChangeAuthority,
                OnComponentUpdated = wrapper.UpdateComponent
            };
        }

        /// <summary>
        ///     Convenience method for dispatching the supplied op to the
        ///     appropriate method in the entity pipeline block.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     Thrown when op object is of an unrecognised type.
        /// </exception>
        public static void DispatchOp(this IEntityPipelineBlock block, IEntityPipelineOp pipelineOp)
        {
            switch (pipelineOp.PipelineOpType)
            {
                case PipelineOpType.AddEntity:
                    block.AddEntity((AddEntityPipelineOp) pipelineOp);
                    break;
                case PipelineOpType.RemoveEntity:
                    block.RemoveEntity((RemoveEntityPipelineOp) pipelineOp);
                    break;
                case PipelineOpType.CriticalSection:
                    block.CriticalSection((CriticalSectionPipelineOp) pipelineOp);
                    break;
                case PipelineOpType.AddComponent:
                    block.AddComponent((AddComponentPipelineOp) pipelineOp);
                    break;
                case PipelineOpType.RemoveComponent:
                    block.RemoveComponent((RemoveComponentPipelineOp) pipelineOp);
                    break;
                case PipelineOpType.ChangeAuthority:
                    block.ChangeAuthority((ChangeAuthorityPipelineOp) pipelineOp);
                    break;
                case PipelineOpType.UpdateComponent:
                    block.UpdateComponent((UpdateComponentPipelineOp) pipelineOp);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown op type {0}, cannot dispatch.", pipelineOp.GetType().Name), "op");
            }
        }
    }
}
