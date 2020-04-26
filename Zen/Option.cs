// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Zen
{
    /// <summary>
    /// A simple option type to parameterize over nullable
    /// values for both structs and classes.
    /// </summary>
    public struct Option<T>
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
