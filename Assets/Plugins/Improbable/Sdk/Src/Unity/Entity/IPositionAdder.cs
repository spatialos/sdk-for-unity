// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     An <see cref="EntityBuilder" /> that needs to have a <see cref="Position" /> component added with
    ///     <see cref="AddPositionComponent" />.
    ///     Multiple calls to <see cref="AddPositionComponent" /> will overwrite the previous <see cref="Position" /> value.
    /// </summary>
    public interface IPositionAdder
    {
        /// <summary>
        ///     Adds the required <see cref="Position" /> component with the specified write ACL. The next step is to call
        ///     <see cref="IMetadataAdder.AddMetadataComponent" />.
        /// </summary>
        IMetadataAdder AddPositionComponent(Vector3 position, WorkerRequirementSet writeAcl);

        /// <summary>
        ///     Adds the required <see cref="Position" /> component with the specified write ACL. The next step is to call
        ///     <see cref="IMetadataAdder.AddMetadataComponent" />.
        /// </summary>
        IMetadataAdder AddPositionComponent(Vector3d position, WorkerRequirementSet writeAcl);

        /// <summary>
        ///     Adds the required <see cref="Position" /> component with the specified write ACL. The next step is to call
        ///     <see cref="IMetadataAdder.AddMetadataComponent" />.
        /// </summary>
        IMetadataAdder AddPositionComponent(Coordinates position, WorkerRequirementSet writeAcl);
    }
}
