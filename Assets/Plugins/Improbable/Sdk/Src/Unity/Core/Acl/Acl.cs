// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;
using System.Collections.Generic;
using Improbable.Collections;
using Improbable.Worker;

namespace Improbable.Unity.Core.Acls
{
    /// <summary>
    ///     Provides convenience methods for building up ACLs (Access Control Lists).
    /// </summary>
    public struct Acl
    {
        private static readonly HashSet<uint> allComponentIds = Dynamic.GetComponentIds();

        private Map<uint, WorkerRequirementSet> writePermissions;
        private WorkerRequirementSet readPermissions;

        /// <summary>
        ///     Starts building a new ACL.
        /// </summary>
        public static Acl Build()
        {
            return new Acl();
        }

        /// <summary>
        ///     Gives workers that satisfy the given requirement set write permissions over the given component.
        /// </summary>
        /// <remarks>
        ///     Calling this multiple times for the same component will overwrite the existing requirement set.
        /// </remarks>
        public Acl SetWriteAccess<TComponent>(WorkerRequirementSet requirementSet) where TComponent : IComponentMetaclass
        {
            EnsureWritePermissionsAllocated();

            var id = Dynamic.GetComponentId<TComponent>();
            writePermissions[id] = requirementSet;
            return this;
        }

        /// <summary>
        ///     Gives workers that satisfy the given requirement set write permissions over the given component.
        /// </summary>
        /// <remarks>
        ///     Calling this multiple times for the same component will overwrite the existing requirement set.
        /// </remarks>
        public Acl SetWriteAccess(uint componentId, WorkerRequirementSet requirementSet)
        {
            if (!allComponentIds.Contains(componentId))
            {
                throw new InvalidOperationException(string.Format("{0} is an unknown component id", componentId));
            }

            EnsureWritePermissionsAllocated();
            writePermissions[componentId] = requirementSet;

            return this;
        }

        /// <summary>
        ///     Gives workers that satisfy the given requirement set read permissions over the given component.
        /// </summary>
        /// <remarks>
        ///     Calling this multiple times will overwrite the existing requirement set.
        /// </remarks>
        public Acl SetReadAccess(WorkerRequirementSet requirementSet)
        {
            readPermissions = requirementSet;
            return this;
        }

        /// <summary>
        ///     Builds a new ACL component data object suitable for adding to a SnapshotEntity.
        /// </summary>
        /// <remarks>
        ///     This can be called multiple times to create variants easily.
        /// </remarks>
        public EntityAcl.Data ToData()
        {
            return new EntityAcl.Data(new EntityAclData(readPermissions,
                                                        writePermissions == null ? new Map<uint, WorkerRequirementSet>() : writePermissions));
        }

        /// <summary>
        ///     Builds an ACL component update object suitable for sending in a component update for an existing entity.
        /// </summary>
        /// <remarks>
        ///     This can be called multiple times to create variants easily.
        /// </remarks>
        public EntityAcl.Update ToUpdate()
        {
            return new EntityAcl.Update().SetReadAcl(readPermissions).SetComponentWriteAcl(
                                                                                           writePermissions == null ? new Map<uint, WorkerRequirementSet>() : writePermissions);
        }

        /// <summary>
        ///     Creates a new <see cref="Acl" /> starting with the given <see cref="EntityAclData" />, and overwriting values in
        ///     'otherAcl' with those present in 'newAcl'.
        /// </summary>
        public static Acl MergeIntoAcl(EntityAclData otherAcl, Acl newAcl)
        {
            Acl mergedAcl = new Acl();

            mergedAcl.readPermissions = otherAcl.readAcl;
            if (newAcl.readPermissions.attributeSet != null)
            {
                mergedAcl.readPermissions = newAcl.readPermissions;
            }

            Map<uint, WorkerRequirementSet> mergedWritePermissions = otherAcl.componentWriteAcl;
            if (newAcl.writePermissions != null)
            {
                foreach (var key in newAcl.writePermissions.Keys)
                {
                    mergedWritePermissions[key] = newAcl.writePermissions[key];
                }
            }

            mergedAcl.writePermissions = mergedWritePermissions;

            return mergedAcl;
        }

        /// <summary>
        ///     Creates an attribute set that a worker satisfies if and only if it has all of the attributes.
        /// </summary>
        public static WorkerAttributeSet MakeAttributeSet(string attribute1, params string[] attributes)
        {
            var list = new Collections.List<string>(attributes.Length + 1);
            foreach (var attribute in Enumerate(attribute1, attributes))
            {
                list.Add(attribute);
            }

            return new WorkerAttributeSet(list);
        }

        /// <summary>
        ///     Creates a requirement set (a set of attribute sets) that a worker satisfies if and only
        ///     it satisfies at least one of the attribute sets.
        /// </summary>
        public static WorkerRequirementSet MakeRequirementSet(WorkerAttributeSet attribute1, params WorkerAttributeSet[] attributes)
        {
            var list = new Collections.List<WorkerAttributeSet>(attributes.Length + 1);
            foreach (var attribute in Enumerate(attribute1, attributes))
            {
                list.Add(attribute);
            }

            return new WorkerRequirementSet(list);
        }

        private static IEnumerable<T> Enumerate<T>(T element1, IEnumerable<T> elements)
        {
            yield return element1;

            using (var enumerator = elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        private void EnsureWritePermissionsAllocated()
        {
            if (writePermissions == null)
            {
                writePermissions = new Map<uint, WorkerRequirementSet>();
            }
        }

        /// <summary>
        ///     Creates an ACL with with read permissions set to client or server,
        ///     and write permissions to server, for all components that exist on the given entity.
        /// </summary>
        public static Acl GenerateServerAuthoritativeAcl(Worker.Entity entity)
        {
            var acl = Acl.Build().SetReadAccess(CommonRequirementSets.PhysicsOrVisual);
            foreach (var componentId in entity.GetComponentIds())
            {
                acl.SetWriteAccess(componentId, CommonRequirementSets.PhysicsOnly);
            }

            return acl;
        }

        /// <summary>
        ///     Creates an ACL with read permissions set to client or server,
        ///     and write permissions to a client worker with the given worker ID, for all
        ///     components that exist on the given entity.
        /// </summary>
        public static Acl GenerateClientAuthoritativeAcl(Worker.Entity entity, string workerId)
        {
            if (string.IsNullOrEmpty(workerId))
            {
                throw new ArgumentNullException("workerId");
            }

            var acl = Acl.Build().SetReadAccess(CommonRequirementSets.PhysicsOrVisual);
            var specificClient = CommonRequirementSets.SpecificClientOnly(workerId);
            foreach (var componentId in entity.GetComponentIds())
            {
                acl.SetWriteAccess(componentId, specificClient);
            }

            return acl;
        }
    }
}
