// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.Core
{
    /// <inheritdoc cref="IEntitySpawnLimiter" />
    /// <summary>
    ///     Spawn entities within per-frame a time budget.
    /// </summary>
    public class TimeBasedSpawnLimiter : IEntitySpawnLimiter
    {
        private readonly TimeSpan limit;
        private DateTime startingTime;

        public TimeBasedSpawnLimiter(TimeSpan limit)
        {
            if (limit < TimeSpan.FromMilliseconds(1))
            {
                throw new ArgumentOutOfRangeException("limit", string.Format("Limit must be at least 1 millisecond per frame. To disable rate limiting, please use {0}", typeof(GreedySpawnLimiter).FullName));
            }

            this.limit = limit;
            Reset();
        }

        public bool CanSpawnEntity()
        {
            return DateTime.UtcNow.Subtract(startingTime) < limit;
        }

        public void EntityAdded(EntityId entityId) { }

        public void Reset()
        {
            startingTime = DateTime.UtcNow;
        }
    }
}
