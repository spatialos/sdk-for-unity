// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Exension methods to send Acl updates.
    /// </summary>
    public static class EntityAclExtensions
    {
        /// <summary>
        ///     Sends an update based on an Acl builder.
        /// </summary>
        public static void Send(this EntityAcl.Writer writer, Acls.Acl aclUpdate)
        {
            writer.Send(aclUpdate.ToUpdate());
        }
    }
}
