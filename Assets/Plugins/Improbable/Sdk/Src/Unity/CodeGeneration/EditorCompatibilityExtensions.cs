// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using System;
using UnityEngine;

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    ///     Extension methods on built in schema types to convert them to Unity native types
    ///     so they can be displayed properly in the editor.
    /// </summary>
    public static class EditorCompatibilityExtensions
    {
        /// <summary>
        ///     Converts <see cref="EntityId" /> to <see cref="long" /> for Unity editor compatibility.
        /// </summary>
        public static long AsUnityType(this EntityId entityId)
        {
            return entityId.Id;
        }

        /// <summary>
        ///     Converts <see cref="Vector3d" /> to <see cref="Vector3" /> for Unity editor compatibility.
        /// </summary>
        public static Vector3 AsUnityType(this Vector3d vector)
        {
            return vector.ToUnityVector();
        }

        /// <summary>
        ///     Converts <see cref="Vector3f" /> to <see cref="Vector3" /> for Unity editor compatibility.
        /// </summary>
        public static Vector3 AsUnityType(this Vector3f vector)
        {
            return vector.ToUnityVector();
        }

        /// <summary>
        ///     Converts <see cref="Coordinates" /> to <see cref="Vector3" /> for Unity editor compatibility.
        /// </summary>
        public static Vector3 AsUnityType(this Coordinates coordinates)
        {
            return new Vector3((float) coordinates.x, (float) coordinates.y, (float) coordinates.z);
        }

        public static object AsSpatialType<TSpatial>(this long longValue)
        {
            if (typeof(TSpatial) == typeof(EntityId))
            {
                return new EntityId(longValue);
            }

            throw new ArgumentOutOfRangeException(string.Format("Cannot convert long to type {0}.", typeof(TSpatial)));
        }
    }
}
