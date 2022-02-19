// <copyright file="CharRange.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A simple representation of a range.
    /// </summary>
    public class CharRange<T> : IEquatable<CharRange<T>>
    {
        /// <summary>
        /// The minimum value for a type.
        /// </summary>
        private static T min = ReflectionUtilities.MinValue<T>();

        /// <summary>
        /// The maximum value for a type.
        /// </summary>
        private static T max = ReflectionUtilities.MaxValue<T>();

        /// <summary>
        /// The low value of the range.
        /// </summary>
        public T Low { get; private set; }

        /// <summary>
        /// The high value of the range.
        /// </summary>
        public T High { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="CharRange{T}"/> class.
        /// </summary>
        public CharRange()
        {
            Low = min;
            High = max;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CharRange{T}"/> class.
        /// </summary>
        /// <param name="low">The low value of the range.</param>
        /// <param name="high">The high value of the range.</param>
        public CharRange(T low, T high)
        {
            Low = low;
            High = high;
        }

        /// <summary>
        /// Determines if the range contains the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True if the element is in the range.</returns>
        public bool Contains(T element)
        {
            return CompareTo(this.Low, element) <= 0 && CompareTo(this.High, element) >= 0;
        }

        /// <summary>
        /// Get the intersection of this range and another.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns>A new range representing the intersection.</returns>
        public CharRange<T> Intersect(CharRange<T> other)
        {
            var newLo = CompareTo(this.Low, other.Low) > 0 ? this.Low : other.Low;
            var newHi = CompareTo(this.High, other.High) < 0 ? this.High : other.High;
            return new CharRange<T>(newLo, newHi);
        }

        /// <summary>
        /// Get the complement ranges to this range that cover the whole space.
        /// </summary>
        /// <returns>Zero to two ranges that cover the rest of the space.</returns>
        public CharRange<T>[] Complement()
        {
            if (this.IsFull())
            {
                return new CharRange<T>[] { };
            }

            if (this.Low.Equals(min))
            {
                return new CharRange<T>[] { new CharRange<T>(ReflectionUtilities.Add((dynamic)this.High, 1), max) };
            }

            if (this.High.Equals(max))
            {
                return new CharRange<T>[] { new CharRange<T>(min, ReflectionUtilities.Add((dynamic)this.Low, -1)) };
            }

            var r1 = new CharRange<T>(min, ReflectionUtilities.Add((dynamic)this.Low, -1));
            var r2 = new CharRange<T>(ReflectionUtilities.Add((dynamic)this.High, 1), max);
            return new CharRange<T>[] { r1, r2 };
        }

        /// <summary>
        /// Checks if the range is full.
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return this.Low.Equals(min) && this.High.Equals(max);
        }

        /// <summary>
        /// Determines if this range is empty.
        /// </summary>
        /// <returns>True or false.</returns>
        public bool IsEmpty()
        {
            return CompareTo(this.High, this.Low) < 0;
        }

        /// <summary>
        /// Converts the range to a string.
        /// </summary>
        /// <returns>The range as a string.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"[{this.Low}-{this.High}]";
        }

        /// <summary>
        /// Equality for ranges.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is CharRange<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for ranges.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns>True if equal.</returns>
        public bool Equals(CharRange<T> other)
        {
            return other != null &&
                   this.Low.Equals(other.Low) &&
                   this.High.Equals(other.High);
        }

        private int CompareTo(dynamic l, dynamic r)
        {
            return l.CompareTo(r);
        }

        /// <summary>
        /// Hashcode for ranges.
        /// </summary>
        /// <returns>A hashcode.</returns>
        public override int GetHashCode()
        {
            return this.Low.GetHashCode() + this.High.GetHashCode();
        }
    }
}
