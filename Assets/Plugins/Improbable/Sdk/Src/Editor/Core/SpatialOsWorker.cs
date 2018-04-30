// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.Editor.Core
{
    /// <summary>
    ///     Represents a worker in the current project.
    /// </summary>
    public class SpatialOsWorker : IEquatable<SpatialOsWorker>
    {
        /// <summary>
        ///     The name of the worker.
        /// </summary>
        public string Name { get; private set; }

        internal SpatialOsWorker(string name)
        {
            Name = name;
        }

        public bool Equals(SpatialOsWorker other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((SpatialOsWorker) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
