// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A simple option type to parameterize over nullable
    /// values for both structs and classes.
    /// </summary>
    public struct Option<T> : IEquatable<Option<T>>
    {
        internal Option(bool b, T value) : this()
        {
            this.HasValue = b;
            this.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether this option has a value.
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Return the underlying value or another value if none.
        /// </summary>
        /// <param name="other">The default value.</param>
        /// <returns>A value of the underlying type.</returns>
        public T ValueOr(T other)
        {
            if (this.HasValue)
            {
                return this.Value;
            }

            return other;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (this.HasValue)
            {
                return $"Some({this.Value})";
            }

            return "None";
        }

        /// <summary>
        /// Equality between option types.
        /// </summary>
        /// <param name="obj">The other option.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is Option<T> o && this.Equals(o);
        }

        /// <summary>
        /// Equality between option types.
        /// </summary>
        /// <param name="other">The other option.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Option<T> other)
        {
            if (!this.HasValue && !other.HasValue)
            {
                return true;
            }

            if (this.HasValue && other.HasValue && this.Value.Equals(other.Value))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the hashcode for the optional value.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            int hashCode = 299404170;
            hashCode = hashCode * -1521134295 + HasValue.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            return hashCode;
        }
    }

    /// <summary>
    /// Builder class for options.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// Return some value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>An option that has some value.</returns>
        public static Option<T> Some<T>(T value)
        {
            return new Option<T>(true, value);
        }

        /// <summary>
        /// Return some value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>An option that has some value.</returns>
        public static Option<T> None<T>()
        {
            return new Option<T>(false, default);
        }
    }
}
