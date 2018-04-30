// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     An <see cref="EntityBuilder" /> that needs to have its read ACL set with <see cref="SetReadAcl" />.
    /// </summary>
    public interface IReadAclSetter
    {
        /// <summary>
        ///     Sets the required read ACL. After this step, the entity has all of its required components.
        /// </summary>
        IComponentAdder SetReadAcl(WorkerRequirementSet readAcl);
    }
}
