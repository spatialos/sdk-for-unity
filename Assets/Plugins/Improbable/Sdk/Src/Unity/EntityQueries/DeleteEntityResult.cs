// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core.EntityQueries
{
    /// <summary>
    ///     Contains the result of DeleteEntity command.
    /// </summary>
    public struct DeleteEntityResult
    {
        private readonly EntityId deletedEntityId;

        public DeleteEntityResult(EntityId deletedEntityId) : this()
        {
            this.deletedEntityId = deletedEntityId;
        }

        /// <summary>
        ///     Returns the EntityId that was deleted.
        /// </summary>
        public EntityId DeletedEntityId
        {
            get { return deletedEntityId; }
        }
    }
}
