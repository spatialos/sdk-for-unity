// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;
using System.Collections.Generic;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Improbable.Unity.Core.EntityQueries
{
    /// <summary>
    ///     Provides a convenient way to build SpatialOS entity queries.
    /// </summary>
    public static class Query
    {
        private static readonly HashSet<uint> allComponentIds = Dynamic.GetComponentIds();

        public static readonly IResultType NoSnapshotResultType = new SnapshotResultType(new Collections.List<uint>());
        public static readonly IResultType AllSnapshotResultType = new SnapshotResultType();
        public static readonly IResultType CountResultType = new CountResultType();

        /// <summary>
        ///     Creates a constraint that is satisfied if and only if all of the given constraints are satisfied.
        /// </summary>
        public static IConstraint And(IConstraint constraint1, IConstraint constraint2, params IConstraint[] constraints)
        {
            return new AndConstraint(Enumerate(constraint1, constraint2, constraints));
        }

        /// <summary>
        ///     Creates a constraint that is satisfied if and only if at least one of the given constraints is satisfied.
        /// </summary>
        public static IConstraint Or(IConstraint constraint1, IConstraint constraint2, params IConstraint[] constraints)
        {
            return new OrConstraint(Enumerate(constraint1, constraint2, constraints));
        }

        /// <summary>
        ///     Creates a constraint that is satisfied by an entity with the given entity ID.
        /// </summary>
        public static IConstraint HasEntityId(EntityId entityId)
        {
            return new EntityIdConstraint(entityId);
        }

        /// <summary>
        ///     Creates a constraint that is satisfied by entities with the given component.
        /// </summary>
        /// <typeparam name="TComponent">The type of the required component.</typeparam>
        public static IConstraint HasComponent<TComponent>() where TComponent : IComponentMetaclass
        {
            return new ComponentConstraint(Dynamic.GetComponentId<TComponent>());
        }

        /// <summary>
        ///     Creates a constraint that is satisfied by entities with the given component.
        /// </summary>
        public static IConstraint HasComponent(uint componentId)
        {
            if (!allComponentIds.Contains(componentId))
            {
                throw new InvalidOperationException(string.Format("Unknown componentId {0}", componentId));
            }

            return new ComponentConstraint(componentId);
        }

        /// <summary>
        ///     Creates a constraint that is satisfied by entities that are located in the specified sphere.
        /// </summary>
        public static IConstraint InSphere(double x, double y, double z, double radius)
        {
            if (radius <= 0.0)
            {
                throw new InvalidOperationException("Please specify a valid radius.");
            }

            return new SphereConstraint(x, y, z, radius);
        }

        private static IEnumerable<IConstraint> Enumerate(IConstraint constraint1, IConstraint constraint2, IEnumerable<IConstraint> constraints)
        {
            yield return constraint1;
            yield return constraint2;

            using (var enumerator = constraints.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }
    }
}
