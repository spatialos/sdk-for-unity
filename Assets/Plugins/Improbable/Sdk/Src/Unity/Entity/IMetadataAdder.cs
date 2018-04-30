// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     An <see cref="EntityBuilder" /> that needs to have a metadata component added with
    ///     <see cref="AddMetadataComponent" />.
    /// </summary>
    public interface IMetadataAdder
    {
        /// <summary>
        ///     Adds the required <see cref="Metadata" /> component. The next step is to call
        ///     <see cref="IPersistenceSetter.SetPersistence" />.
        /// </summary>
        IPersistenceSetter AddMetadataComponent(string entityType);
    }
}
