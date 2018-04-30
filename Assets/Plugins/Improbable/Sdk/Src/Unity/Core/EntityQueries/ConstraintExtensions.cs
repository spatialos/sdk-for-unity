// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Improbable.Unity.Core.EntityQueries
{
    /// <summary>
    ///     Extension methods for building EntityQueries from a query constraint.
    /// </summary>
    public static class ConstraintExtensions
    {
        private static readonly HashSet<uint> allComponentIds = Dynamic.GetComponentIds();

        /// <summary>
        ///     Creates an entity query which returns only entity IDs in the result set (component snapshots of entities will not
        ///     be included).
        /// </summary>
        public static EntityQuery ReturnOnlyEntityIds(this IConstraint constraint)
        {
            return new EntityQuery { Constraint = constraint, ResultType = Query.NoSnapshotResultType };
        }

        /// <summary>
        ///     Creates an entity query which returns entities along with component snapshots, for each specified component type,
        ///     in the result set.
        /// </summary>
        public static EntityQuery ReturnComponents(this IConstraint constraint, uint componentType, params uint[] componentTypes)
        {
            foreach (var type in Enumerate(componentType, componentTypes))
            {
                if (!allComponentIds.Contains(type))
                {
                    throw new KeyNotFoundException(string.Format("{0} is an unknown component type", type));
                }
            }

            return new EntityQuery
            {
                Constraint = constraint,
                ResultType = new SnapshotResultType(new Collections.List<uint>(Enumerate(componentType, componentTypes)))
            };
        }

        /// <summary>
        ///     Creates an entity query which returns entities along with all available component snapshots in the result set.
        /// </summary>
        public static EntityQuery ReturnAllComponents(this IConstraint constraint)
        {
            return new EntityQuery
            {
                Constraint = constraint,
                ResultType = Query.AllSnapshotResultType
            };
        }

        /// <summary>
        ///     Creates an entity query which returns only the number of entities in the result set.
        /// </summary>
        public static EntityQuery ReturnCount(this IConstraint constraint)
        {
            return new EntityQuery { Constraint = constraint, ResultType = Query.CountResultType };
        }

        private static IEnumerable<T> Enumerate<T>(T element1, IEnumerable<T> elements)
        {
            yield return element1;

            var enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            enumerator.Dispose();
        }
    }
}
