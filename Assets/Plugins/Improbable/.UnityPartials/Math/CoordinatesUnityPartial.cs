// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using UnityEngine;

namespace Improbable
{
    public partial struct Coordinates
    {
        /// <summary>
        ///     A Coordinates with 0 in each dimension.
        /// </summary>
        public static readonly Coordinates ZERO = new Coordinates(0, 0, 0);

        public static Vector3d operator -(Coordinates v1, Coordinates v2)
        {
            return new Vector3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Coordinates operator -(Coordinates v1, Vector3d v2)
        {
            return new Coordinates(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Coordinates operator +(Coordinates a, Vector3d b)
        {
            return new Coordinates(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Coordinates operator -(Coordinates v1, Vector3f v2)
        {
            return new Coordinates(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Coordinates operator +(Coordinates a, Vector3f b)
        {
            return new Coordinates(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Coordinates operator -(Coordinates v1, Vector3 v2)
        {
            return new Coordinates(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Coordinates operator +(Coordinates a, Vector3 b)
        {
            return new Coordinates(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Coordinates operator -(Vector3d v1, Coordinates v2)
        {
            return new Coordinates(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Coordinates operator -(Vector3f v1, Coordinates v2)
        {
            return new Coordinates(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Coordinates operator -(Vector3 v1, Coordinates v2)
        {
            return new Coordinates(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Coordinates operator +(Vector3d a, Coordinates b)
        {
            return new Coordinates(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Coordinates operator +(Vector3f a, Coordinates b)
        {
            return new Coordinates(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Coordinates operator +(Vector3 a, Coordinates b)
        {
            return new Coordinates(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        ///     Interpolates between to global coordinates
        /// </summary>
        /// <param name="currentPosition">The starting position</param>
        /// <param name="newPosition">The position to intepolate towards</param>
        /// <param name="progressRatio">
        ///     where 0 is the currentPosition and 1 is newPosition. Note: the value is clamped between 0
        ///     and 1 to prevent extrapolation.
        /// </param>
        /// <returns>The interpolated position</returns>
        public static Coordinates Lerp(Coordinates currentPosition, Coordinates newPosition, float progressRatio)
        {
            progressRatio = progressRatio > 1 ? 1 : (progressRatio < 0 ? 0 : progressRatio); // Clamp
            var valueDelta = newPosition - currentPosition;
            return valueDelta * progressRatio + currentPosition;
        }

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        /// <summary>
        ///     Check if a coordinate is near another.
        /// </summary>
        /// <param name="other">The coordinate to test.</param>
        /// <param name="distance">The allowed range.</param>
        /// <returns>True if the other coordinate is within strictly less than the specified range.</returns>
        public bool IsWithinDistance(Coordinates other, double distance)
        {
#pragma warning disable 618
            return IsWithinSquareDistance(other, distance * distance);
#pragma warning restore 618
        }

        /// <summary>
        ///     Calculate the square-space distance between two coordinates.
        /// </summary>
        /// <returns>The square-space distance between two coordinates.</returns>
        public static double SquareDistance(Coordinates v1, Coordinates v2)
        {
            return (v1 - v2).SquareMagnitude();
        }

        /// <summary>
        ///     Check if a coordinate is near another.
        /// </summary>
        /// <param name="other">The coordinate to test.</param>
        /// <param name="sqrDistance">The allowed square-space range.</param>
        /// <returns>True if the other coordinate is within strictly less than the specified range.</returns>
        public bool IsWithinSquareDistance(Coordinates other, double sqrDistance)
        {
            return SquareDistance(this, other) < sqrDistance;
        }

        /// <summary>
        ///     True if all components of the Coordinates are real numbers.
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
        ///     Converts to a Unity Vector3.
        /// </summary>
        public Vector3 ToUnityVector()
        {
            return new Vector3((float) x, (float) y, (float) z);
        }

        /// <summary>
        ///     Converts to a Spatial Vector3d.
        /// </summary>
        public Vector3d ToSpatialVector3d()
        {
            return new Vector3d(x, y, z);
        }

        /// <summary>
        ///     Returns the string representation of the Coordinates.
        /// </summary>
        public override string ToString()
        {
            return "Coordinates(" + x + ", " + y + ", " + z + ")";
        }
    }
}
