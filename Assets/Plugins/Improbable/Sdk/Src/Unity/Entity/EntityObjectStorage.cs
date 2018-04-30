// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Unity.CodeGeneration;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     Used to associate an Unity Object with our EntityObject.
    /// </summary>
    public class EntityObjectStorage : MonoBehaviour
    {
        /// <summary>
        ///     The associated EntityObject.
        /// </summary>
        public IEntityObject Entity { get; private set; }

        public void Initialize(IEntityObject entityObject, ISpatialCommunicator spatialCommunicator)
        {
            if (spatialCommunicator == null)
            {
                throw new ArgumentNullException("spatialCommunicator");
            }

            Entity = entityObject;
            InitializeComponents(entityObject, spatialCommunicator);
        }

        private void InitializeComponents(IEntityObject entityObject, ISpatialCommunicator spatialCommunicator)
        {
            // N.B. this one works with interfaces, GetComponents<> doesn't.
            var spatialOsComponents = GetComponents(typeof(ISpatialOsComponentInternal));
            for (var i = 0; i < spatialOsComponents.Length; i++)
            {
                var spatialOsComponent = spatialOsComponents[i] as ISpatialOsComponentInternal;
                if (spatialOsComponent == null)
                {
                    continue;
                }

                spatialOsComponent.Init(spatialCommunicator, entityObject);
            }
        }

        public void Clear()
        {
            GameObjectExtensions.RemoveFromLookupCache(Entity);
            Entity = null;
        }
    }
}
