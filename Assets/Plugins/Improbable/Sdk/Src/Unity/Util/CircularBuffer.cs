// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;

namespace Improbable.Unity.Util
{
    /// <summary>
    ///     An array based circular buffer implementation.
    /// </summary>
    /// <typeparam name="T">The type of objects stored in the buffer.</typeparam>
    public class CircularBuffer<T>
    {
        protected readonly T[] buffer;
        protected readonly int size;
        protected int index;

        /// <summary>
        ///     Create a circular buffer with the given size.
        /// </summary>
        public CircularBuffer(int size)
        {
            buffer = new T[size];
            this.size = size;
        }

        /// <summary>
        ///     Add an item to the buffer.
        /// </summary>
        public void Add(T item)
        {
            buffer[index] = item;
            index = (index + 1) % size;
        }

        /// <summary>
        ///     Populates an array with the values in the buffer with the most recently added items first.
        ///     The given array must be of the same size as the buffer.
        /// </summary>
        public void GetItemsInMostRecentOrder(ref T[] array)
        {
            if (array.Length < size)
            {
                throw new ArgumentException("Given array must be of size >= the buffer size");
            }

            for (var i = 0; i < size; i++)
            {
                array[i] = buffer[(index - 1 - i + size) % size];
            }
        }
    }

    /// <summary>
    ///     A circular buffer to hold integers.
    /// </summary>
    public class CircularIntBuffer : CircularBuffer<int>
    {
        /// <summary>
        ///     Creates a circular buffer to hold integers.
        /// </summary>
        public CircularIntBuffer(int size) : base(size) { }

        /// <summary>
        ///     Returns the average of the values in the circular buffer.
        /// </summary>
        public double GetAverage()
        {
            var sum = 0;
            for (var i = 0; i < size; i++)
            {
                sum += buffer[i];
            }

            return (double) sum / size;
        }
    }
}
