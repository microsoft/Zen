// <copyright file="Range.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A simple representation of a range.
    /// </summary>
    public class Range<T> : IEquatable<Range<T>> where T : IComparable<T>
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
        /// Creates a new instance of the <see cref="Range{T}"/> class.
        /// </summary>
        /// <param name="low">The low value of the range.</param>
        /// <param name="high">The high value of the range.</param>
        public Range(T low, T high)
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
            return this.Low.CompareTo(element) <= 0 && this.High.CompareTo(element) >= 0;
        }

        /// <summary>
        /// Checks if the range is full.
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return this.Low.CompareTo(min) == 0 && this.High.CompareTo(max) == 0;
        }

        /// <summary>
        /// Determines if this range is empty.
        /// </summary>
        /// <returns>True or false.</returns>
        public bool IsEmpty()
        {
            return this.High.CompareTo(this.Low) < 0;
        }

        /// <summary>
        /// Determines if this range contains a single value.
        /// </summary>
        /// <returns>True or false.</returns>
        public bool IsSingleton()
        {
            return this.High.CompareTo(this.Low) == 0;
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
            return obj is Range<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for ranges.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns>True if equal.</returns>
        public bool Equals(Range<T> other)
        {
            return other != null &&
                   this.Low.CompareTo(other.Low) == 0 &&
                   this.High.CompareTo(other.High) == 0;
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
