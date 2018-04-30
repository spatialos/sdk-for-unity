// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <inheritdoc />
    /// <summary>
    ///     Spawn as many entities per frame as there are available.
    /// </summary>
    public class GreedySpawnLimiter : IEntitySpawnLimiter
    {
        public bool CanSpawnEntity()
        {
            return true;
        }

        public void EntityAdded(EntityId entityId) { }

        public void Reset() { }
    }
}
