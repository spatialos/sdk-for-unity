// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Provides an efficient storage mechanism for EntityObjects which avoids the possibility of a
    ///     mismatch between an EntityId key and the associated EntityObject value.
    /// </summary>
    class EntityObjectDictionary : KeyedCollection<EntityId, IEntityObject>
    {
        protected override EntityId GetKeyForItem(IEntityObject item)
        {
            return item.EntityId;
        }

        public bool TryGetValue(EntityId entityId, out IEntityObject storage)
        {
            return Dictionary.TryGetValue(entityId, out storage);
        }

        public IList<IEntityObject> Values
        {
            get { return Items; }
        }
    }
}
