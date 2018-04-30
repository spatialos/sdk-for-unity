// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core.EntityQueries
{
    /// <summary>
    ///     Contains the result of ReserveEntityId command.
    /// </summary>
    public struct ReserveEntityIdResult
    {
        private readonly EntityId reservedEntityId;

        public ReserveEntityIdResult(EntityId reservedEntityId) : this()
        {
            this.reservedEntityId = reservedEntityId;
        }

        /// <summary>
        ///     Returns the EntityId that was reserved.
        /// </summary>
        public EntityId ReservedEntityId
        {
            get { return reservedEntityId; }
        }
    }
}
