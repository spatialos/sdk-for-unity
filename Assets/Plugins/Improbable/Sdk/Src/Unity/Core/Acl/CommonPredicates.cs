// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;

namespace Improbable.Unity.Core.Acls
{
    /// <summary>
    ///     Provides commonly-used, well-known requirement sets.
    /// </summary>
    [Obsolete("Deprecated: Please use CommonRequirementSets instead.")]
    public static class CommonPredicates
    {
        /// <summary>
        ///     Only satisfied by a physics worker.
        /// </summary>
        public static WorkerRequirementSet PhysicsOnly = Acl.MakeRequirementSet(CommonAttributeSets.Physics);

        /// <summary>
        ///     Only satisfied by a visual worker.
        /// </summary>
        public static WorkerRequirementSet VisualOnly = Acl.MakeRequirementSet(CommonAttributeSets.Visual);

        /// <summary>
        ///     Satisfied by a physics or visual worker.
        /// </summary>
        public static WorkerRequirementSet PhysicsOrVisual = Acl.MakeRequirementSet(CommonAttributeSets.Physics, CommonAttributeSets.Visual);

        /// <summary>
        ///     Satisfied by a specific client only.
        /// </summary>
        public static WorkerRequirementSet SpecificClientOnly(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException("clientId");
            }

            return Acl.MakeRequirementSet(CommonAttributeSets.SpecificClient(clientId));
        }
    }
}
