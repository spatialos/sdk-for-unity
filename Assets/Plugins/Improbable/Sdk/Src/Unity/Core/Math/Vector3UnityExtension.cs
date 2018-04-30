// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using UnityEngine;

namespace Improbable.Unity.Common.Core.Math
{
    public static class Vector3UnityExtension
    {
        public static Vector3f ToSpatialVector3f(this Vector3 unityVector3)
        {
            return new Vector3f(unityVector3.x, unityVector3.y, unityVector3.z);
        }

        public static Vector3d ToSpatialVector3d(this Vector3 unityVector3)
        {
            return new Vector3d(unityVector3.x, unityVector3.y, unityVector3.z);
        }

        public static Coordinates ToSpatialCoordinates(this Vector3 unityVector3)
        {
            return new Coordinates(unityVector3.x, unityVector3.y, unityVector3.z);
        }

        public static bool IsFinite(this Vector3 unityVector3)
        {
            return !float.IsInfinity(unityVector3.x) && !float.IsInfinity(unityVector3.y) && !float.IsInfinity(unityVector3.z) &&
                   !float.IsNaN(unityVector3.x) && !float.IsNaN(unityVector3.y) && !float.IsNaN(unityVector3.z);
        }
    }
}
