// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Improbable.Unity.Core.EntityQueries
{
    /// <summary>
    ///     Contains the result of ReserveEntityIds command.
    /// </summary>
    public struct ReserveEntityIdsResult
    {
        private readonly ReadOnlyCollection<EntityId> reservedEntityIds;

        public ReserveEntityIdsResult(EntityId firstEntityId, int numberOfEntityIds) : this()
        {
            var newReservedEntityIds = new List<EntityId>(numberOfEntityIds);

            for (var i = 0; i < numberOfEntityIds; ++i)
            {
                newReservedEntityIds.Add(new EntityId(firstEntityId.Id + i));
            }

            reservedEntityIds = newReservedEntityIds.AsReadOnly();
        }

        /// <summary>
        ///     Returns a contiguous range of newly allocated entity IDs which are guaranteed to be unused in the current
        ///     deployment.
        /// </summary>
        public ReadOnlyCollection<EntityId> ReservedEntityIds
        {
            get { return reservedEntityIds; }
        }
    }
}
