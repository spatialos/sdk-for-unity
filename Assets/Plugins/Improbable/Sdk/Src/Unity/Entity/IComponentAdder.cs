// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Worker;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     An <see cref="EntityBuilder" /> that has all of its required components set.
    ///     Call <see cref="AddComponent{C}" /> to add more components, or <see cref="Build" /> to complete the
    ///     <see cref="Worker.Entity" />.
    /// </summary>
    public interface IComponentAdder
    {
        /// <summary>
        ///     Adds a component (with the specified write ACL) to the entity.
        /// </summary>
        IComponentAdder AddComponent<C>(IComponentData<C> data, WorkerRequirementSet writeAcl) where C : IComponentMetaclass;

        /// <summary>
        ///     Sets the write authority on the ACL component.
        /// </summary>
        IComponentAdder SetEntityAclComponentWriteAccess(WorkerRequirementSet writeAcl);

        /// <summary>
        ///     Builds the <see cref="Worker.Entity" />. This method cannot be called more than once.
        /// </summary>
        Worker.Entity Build();
    }
}
