// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.Core
{
    /// <inheritdoc cref="IEntitySpawnLimiter" />
    /// <summary>
    ///     Spawn at most the specified number of entities per frame.
    /// </summary>
    public class CountBasedSpawnLimiter : IEntitySpawnLimiter
    {
        private readonly int limit;
        private int toAdd;

        public CountBasedSpawnLimiter(int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException("limit", string.Format("Limit must be at least 1 entity per frame. To disable rate limiting, please use {0}", typeof(GreedySpawnLimiter).FullName));
            }

            this.limit = limit;
            Reset();
        }

        public bool CanSpawnEntity()
        {
            return toAdd > 0;
        }

        public void EntityAdded(EntityId entityId)
        {
            toAdd--;
        }

        public void Reset()
        {
            toAdd = limit;
        }
    }
}
