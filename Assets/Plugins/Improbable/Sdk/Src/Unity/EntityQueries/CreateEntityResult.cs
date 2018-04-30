// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core.EntityQueries
{
    /// <summary>
    ///     Contains the result of CreateEntity command.
    /// </summary>
    public struct CreateEntityResult
    {
        private readonly EntityId createdEntityId;

        public CreateEntityResult(EntityId createdEntityId) : this()
        {
            this.createdEntityId = createdEntityId;
        }

        /// <summary>
        ///     Returns the EntityId of the created entity.
        /// </summary>
        public EntityId CreatedEntityId
        {
            get { return createdEntityId; }
        }
    }
}
