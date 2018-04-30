// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;

namespace Improbable.Unity.Core.Acls
{
    /// <summary>
    ///     Provides commonly-used, well-known attribute sets.
    /// </summary>
    public static class CommonAttributeSets
    {
        /// <summary>
        ///     Identifies a physics worker.
        /// </summary>
        public static WorkerAttributeSet Physics = Acl.MakeAttributeSet("physics");

        /// <summary>
        ///     Identifies a client worker.
        /// </summary>
        public static WorkerAttributeSet Visual = Acl.MakeAttributeSet("visual");

        /// <summary>
        ///     Identifies a specific client worker.
        /// </summary>
        public static WorkerAttributeSet SpecificClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException("clientId");
            }

            return Acl.MakeAttributeSet(string.Format("workerId:{0}", clientId));
        }
    }
}
