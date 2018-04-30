// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Unity.CodeGeneration;
using Improbable.Unity.Entity;

namespace Improbable.Unity
{
    class EntityComponents : IEntityComponents
    {
        private readonly IList<IEntityInterestedComponentsInvalidator> invalidatorsList;

        /// <inheritdoc />
        public IDictionary<uint, ISpatialOsComponentInternal> RegisteredComponents { get; private set; }

        /// <summary>
        ///     Creates a new instance of <c>EntityComponents</c>.
        /// </summary>
        public EntityComponents()
        {
            invalidatorsList = new List<IEntityInterestedComponentsInvalidator>();
            RegisteredComponents = new Dictionary<uint, ISpatialOsComponentInternal>();
        }

        /// <inheritdoc />
        public void RegisterInterestedComponent(uint componentId, ISpatialOsComponentInternal component)
        {
            if (RegisteredComponents.ContainsKey(componentId))
            {
                throw new InvalidOperationException("Trying to add duplicate componentId to InterestedComponents");
            }

            RegisteredComponents[componentId] = component;
            TriggerInterestedComponentsPotentiallyChanged();
        }

        /// <inheritdoc />
        public void DeregisterInterestedComponent(uint componentId)
        {
            if (!RegisteredComponents.ContainsKey(componentId))
            {
                throw new InvalidOperationException("Trying to remove non-existing componentId from InterestedComponents");
            }

            RegisteredComponents.Remove(componentId);
            TriggerInterestedComponentsPotentiallyChanged();
        }

        /// <inheritdoc />
        public void AddInvalidator(IEntityInterestedComponentsInvalidator invalidator)
        {
            invalidatorsList.Add(invalidator);
        }

        /// <inheritdoc />
        public void RemoveInvalidator(IEntityInterestedComponentsInvalidator invalidator)
        {
            invalidatorsList.Remove(invalidator);
        }

        private void TriggerInterestedComponentsPotentiallyChanged()
        {
            for (int i = 0; i < invalidatorsList.Count; i++)
            {
                invalidatorsList[i].OnInterestedComponentsPotentiallyChanged();
            }
        }
    }
}
