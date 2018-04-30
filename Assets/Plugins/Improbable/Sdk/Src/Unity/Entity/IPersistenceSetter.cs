// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     An <see cref="EntityBuilder" /> that needs to have its persistence value set with <see cref="SetPersistence" />.
    /// </summary>
    public interface IPersistenceSetter
    {
        /// <summary>
        ///     If true, adds the <see cref="Persistence" /> component to the entity. The next step is to call
        ///     <see cref="IReadAclSetter.SetReadAcl" />.
        /// </summary>
        IReadAclSetter SetPersistence(bool persistence);
    }
}
