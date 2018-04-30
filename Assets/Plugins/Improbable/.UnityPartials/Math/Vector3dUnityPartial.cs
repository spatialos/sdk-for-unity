// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using UnityEngine;

namespace Improbable
{
    public partial struct Vector3d
    {
        /// <summary>
        ///     A Vector3d with 0 in each dimension.
        /// </summary>
        public static readonly Vector3d ZERO = new Vector3d(0, 0, 0);

        /// <summary>
        ///     Override of the multiplication opporator. Used for multiplying the Vector3d by a float scalar.
        /// </summary>
        public static Vector3d operator *(Vector3d vector3d, double scalar)
        {
            return new Vector3d(vector3d.x * scalar, vector3d.y * scalar, vector3d.z * scalar);
        }

        /// <summary>
        ///     Override of the multiplication opporator. Used for multiplying the Vector3d by a float scalar.
        /// </summary>
        public static Vector3d operator *(double scalar, Vector3d vector3d)
        {
            return new Vector3d(vector3d.x * scalar, vector3d.y * scalar, vector3d.z * scalar);
        }

        /// <summary>
        ///     Override of the division opporator. Used for dividing the Vector3d by a float scalar.
        /// </summary>
        public static Vector3d operator /(Vector3d vector3d, double scalar)
        {
            return new Vector3d(vector3d.x / scalar, vector3d.y / scalar, vector3d.z / scalar);
        }

        /// <summary>
        ///     Override of the addition opporator. Used for adding two Vector3s.
        /// </summary>
        public static Vector3d operator +(Vector3d vector3d, Vector3d addvector3d)
        {
            return new Vector3d(vector3d.x + addvector3d.x, vector3d.y + addvector3d.y, vector3d.z + addvector3d.z);
        }

        /// <summary>
        ///     Override of the subtraction opporator. Used for subtracting one Vector3d from another.
        /// </summary>
        public static Vector3d operator -(Vector3d vector3d, Vector3d subtractVector3d)
        {
            return new Vector3d(vector3d.x - subtractVector3d.x, vector3d.y - subtractVector3d.y, vector3d.z - subtractVector3d.z);
        }

        /// <summary>
        ///     Computes the square of the magnitude of the Vector3d.
        /// </summary>
        public double SquareMagnitude()
        {
            return x * x + y * y + z * z;
        }

        /// <summary>
        ///     Returns the normal of the Vector3d (does not modify the original Vector3f).
        /// </summary>
        public Vector3d Normalized()
        {
            var magnitude = System.Math.Sqrt(SquareMagnitude());
            return new Vector3d(x / magnitude, y / magnitude, z / magnitude);
        }

        public double X
        {
            get { return x; }
        }

        public double Y
        {
            get { return y; }
        }

        public double Z
        {
            get { return z; }
        }

        /// <summary>
        ///     True if all components of the Vector3d are real numbers.
        /// </summary>
        public bool IsFinite
        {
            get
            {
                return !double.IsNaN(x) && !double.IsNaN(y) && !double.IsNaN(z) &&
                       !double.IsInfinity(x) && !double.IsInfinity(y) && !double.IsInfinity(z);
            }
        }

        /// <summary>
        ///     Converts the Vector3d to a Unity Vector3.
        /// </summary>
        public Vector3 ToUnityVector()
        {
            return new Vector3((float) x, (float) y, (float) z);
        }

        /// <summary>
        ///     Converts the Vector3d to a Unity Quaternion.
        /// </summary>
        public Quaternion ToUnityQuaternion()
        {
            return Quaternion.Euler(ToUnityVector());
        }

        /// <summary>
        ///     Returns the string representation of the Vector3d.
        /// </summary>
        public override string ToString()
        {
            return "Vector3d(" + x + ", " + y + ", " + z + ")";
        }
    }
}
