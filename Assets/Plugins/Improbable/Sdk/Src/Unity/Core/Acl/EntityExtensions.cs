// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core.Acls
{
    /// <summary>
    ///     Convenience methods for managing entity snapshots.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        ///     Adds an ACL component to an entity or overwrites an existing one.
        /// </summary>
        public static void SetAcl(this Worker.Entity entity, Acl acl)
        {
            if (entity.Get<EntityAcl>().HasValue)
            {
                entity.Update(acl.ToUpdate());
                return;
            }

            entity.Add(acl.ToData());
        }

        /// <summary>
        ///     Merges an <see cref="Acl" /> into the entity's existing set of ACLs.
        /// </summary>
        public static void MergeAcl(this Worker.Entity entity, Acl newAcl)
        {
            if (entity.Get<EntityAcl>().HasValue)
            {
                entity.Update(Acl.MergeIntoAcl(entity.Get<EntityAcl>().Value.Get().Value, newAcl).ToUpdate());
                return;
            }

            entity.Add(newAcl.ToData());
        }
    }
}
