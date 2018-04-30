// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using UnityEngine;

namespace Improbable
{
    public partial struct Vector3f
    {
        /// <summary>
        ///     A Vector3f with 0f in each dimension.
        /// </summary>
        public static readonly Vector3f ZERO = new Vector3f(0f, 0f, 0f);

        /// <summary>
        ///     Override of the multiplication opporator. Used for multiplying the Vector3f by a float scalar.
        /// </summary>
        public static Vector3f operator *(Vector3f vector3f, float scalar)
        {
            return new Vector3f(vector3f.x * scalar, vector3f.y * scalar, vector3f.z * scalar);
        }

        /// <summary>
        ///     Override of the multiplication opporator. Used for multiplying the Vector3f by a float scalar.
        /// </summary>
        public static Vector3f operator *(float scalar, Vector3f vector3f)
        {
            return new Vector3f(vector3f.x * scalar, vector3f.y * scalar, vector3f.z * scalar);
        }

        /// <summary>
        ///     Override of the division opporator. Used for dividing the Vector3f by a float scalar.
        /// </summary>
        public static Vector3f operator /(Vector3f vector3f, float scalar)
        {
            return new Vector3f(vector3f.x / scalar, vector3f.y / scalar, vector3f.z / scalar);
        }

        /// <summary>
        ///     Override of the addition opporator. Used for adding two Vector3s.
        /// </summary>
        public static Vector3f operator +(Vector3f vector3f, Vector3f addVector3f)
        {
            return new Vector3f(vector3f.x + addVector3f.x, vector3f.y + addVector3f.y, vector3f.z + addVector3f.z);
        }

        /// <summary>
        ///     Override of the subtraction opporator. Used for subtracting one Vector3f from another.
        /// </summary>
        public static Vector3f operator -(Vector3f vector3f, Vector3f subtractVector3f)
        {
            return new Vector3f(vector3f.x - subtractVector3f.x, vector3f.y - subtractVector3f.y, vector3f.z - subtractVector3f.z);
        }

        /// <summary>
        ///     Computes the square of the magnitude of the Vector3f.
        /// </summary>
        public float SquareMagnitude()
        {
            return x * x + y * y + z * z;
        }

        /// <summary>
        ///     Returns the normal of the Vector3f (does not modify the original Vector3f).
        /// </summary>
        public Vector3f Normalized()
        {
            var magnitude = (float) System.Math.Sqrt(SquareMagnitude());
            return new Vector3f(x / magnitude, y / magnitude, z / magnitude);
        }

        public float X
        {
            get { return x; }
        }

        public float Y
        {
            get { return y; }
        }

        public float Z
        {
            get { return z; }
        }

        /// <summary>
        ///     True if all components of the Vector3f are real numbers.
        /// </summary>
        public bool IsFinite
        {
            get
            {
                return !float.IsNaN(x) && !float.IsNaN(y) && !float.IsNaN(z) &&
                       !float.IsInfinity(x) && !float.IsInfinity(y) && !float.IsInfinity(z);
            }
        }

        /// <summary>
        ///     Converts the Vector3f to a Unity Vector3.
        /// </summary>
        public Vector3 ToUnityVector()
        {
            return new Vector3(x, y, z);
        }

        /// <summary>
        ///     Converts the Vector3f to a Unity Quaternion.
        /// </summary>
        public Quaternion ToUnityQuaternion()
        {
            return Quaternion.Euler(ToUnityVector());
        }

        /// <summary>
        ///     Returns the string representation of the Vector3f.
        /// </summary>
        public override string ToString()
        {
            return "Vector3f(" + x + ", " + y + ", " + z + ")";
        }
    }
}
