// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Common.Core.Math;
using Improbable.Unity.Core.Acls;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Helper class for building an <see cref="Worker.Entity" />. Call <see cref="Begin" /> to use.
    /// </summary>
    /// <remarks>
    ///     EntityBuilder provides a builder for creating <see cref="Worker.Entity" /> objects easily. Its helper
    ///     methods are structured such that all required components must be added before an entity can be
    ///     constructed.
    ///     <para />
    ///     Helpers must be called in the following order:
    ///     <see cref="Begin" />,
    ///     <see cref="AddPositionComponent" />,
    ///     <see cref="AddMetadataComponent" />,
    ///     <see cref="SetPersistence" />,
    ///     <see cref="SetReadAcl" />.
    ///     After this point, all required components will have been set. More components can be added with
    ///     <see cref="AddComponent{C}" />, and the <see cref="Worker.Entity" /> can be built with <see cref="Build" />.
    ///     You can also set write access on the ACL component using the optional
    ///     <see cref="SetEntityAclComponentWriteAccess" />.
    ///     <para />
    ///     This class is not thread-safe.
    /// </remarks>
    /// <example>
    ///     This example shows how to construct a simple entity using the EntityBuilder helpers.
    ///     <code>
    ///      var entity = EntityBuilder.Begin()
    ///          .AddPositionComponent(position, positionWriteAcl)
    ///          .AddMetadataComponent(prefabName)
    ///          .SetPersistence(persistence)
    ///          .SetReadAcl(readAcl)
    ///          .AddComponent&lt;MyComponent&gt;(myComponent.Data, myComponentWriteAcl)
    ///          .Build();
    ///      </code>
    /// </example>
    public class EntityBuilder :
        IPositionAdder,
        IMetadataAdder,
        IPersistenceSetter,
        IReadAclSetter,
        IComponentAdder
    {
        private Worker.Entity entity;
        private Acl entityAcl;
        private bool hasBuiltOnce;

        /// <summary>
        ///     Returns a new <see cref="EntityBuilder" />. Start by calling <see cref="AddPositionComponent" />.
        /// </summary>
        public static IPositionAdder Begin()
        {
            return new EntityBuilder();
        }

        protected EntityBuilder()
        {
            entity = new Worker.Entity();
            entityAcl = new Acl();
        }

        /// <inheritdoc cref="IPositionAdder.AddPositionComponent" />
        public IMetadataAdder AddPositionComponent(Vector3 position, WorkerRequirementSet writeAcl)
        {
            entity.Add(new Position.Data(position.ToSpatialCoordinates()));
            entityAcl.SetWriteAccess<Position>(writeAcl);
            return this;
        }

        /// <inheritdoc cref="IPositionAdder.AddPositionComponent" />
        public IMetadataAdder AddPositionComponent(Vector3d position, WorkerRequirementSet writeAcl)
        {
            entity.Add(new Position.Data(position.ToSpatialCoordinates()));
            entityAcl.SetWriteAccess<Position>(writeAcl);
            return this;
        }

        /// <inheritdoc cref="IPositionAdder.AddPositionComponent" />
        public IMetadataAdder AddPositionComponent(Coordinates position, WorkerRequirementSet writeAcl)
        {
            entity.Add(new Position.Data(position));
            entityAcl.SetWriteAccess<Position>(writeAcl);
            return this;
        }

        /// <inheritdoc cref="IMetadataAdder.AddMetadataComponent" />
        public IPersistenceSetter AddMetadataComponent(string entityType)
        {
            entity.Add(new Metadata.Data(entityType));
            return this;
        }

        /// <inheritdoc cref="IPersistenceSetter.SetPersistence" />
        public IReadAclSetter SetPersistence(bool persistence)
        {
            if (persistence)
            {
                entity.Add(new Persistence.Data());
            }

            return this;
        }

        /// <inheritdoc cref="IReadAclSetter.SetReadAcl" />
        public IComponentAdder SetReadAcl(WorkerRequirementSet readAcl)
        {
            entityAcl.SetReadAccess(readAcl);
            return this;
        }

        /// <inheritdoc cref="IComponentAdder.SetEntityAclComponentWriteAccess" />
        public IComponentAdder SetEntityAclComponentWriteAccess(WorkerRequirementSet writeAcl)
        {
            entityAcl.SetWriteAccess<EntityAcl>(writeAcl);
            return this;
        }

        /// <inheritdoc cref="IComponentAdder.AddComponent" />
        public IComponentAdder AddComponent<C>(IComponentData<C> data, WorkerRequirementSet writeAcl) where C : IComponentMetaclass
        {
            entity.Add(data);
            entityAcl.SetWriteAccess<C>(writeAcl);
            return this;
        }

        /// <inheritdoc cref="IComponentAdder.Build" />
        public Worker.Entity Build()
        {
            if (hasBuiltOnce)
            {
                throw new System.InvalidOperationException("Cannot call Build() multiple times on an EntityBuilder");
            }

            hasBuiltOnce = true;
            entity.Add(entityAcl.ToData());
            return entity;
        }
    }
}
