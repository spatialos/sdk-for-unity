// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


namespace Improbable.Unity.Common.Core.Math
{
    public static class Vector3dUnityExtension
    {
        public static Vector3f ToSpatialVector3f(this Vector3d unityVector3d)
        {
            return new Vector3f((float) unityVector3d.x, (float) unityVector3d.y, (float) unityVector3d.z);
        }

        public static Coordinates ToSpatialCoordinates(this Vector3d unityVector3d)
        {
            return new Coordinates(unityVector3d.x, unityVector3d.y, unityVector3d.z);
        }
    }
}
