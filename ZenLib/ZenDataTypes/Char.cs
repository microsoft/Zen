// <copyright file="Char.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace ZenLib
{
    /// <summary>
    /// A unicode character that is compatible with SMT-LIB.
    /// </summary>
    public sealed class Char
    {
        /// <summary>
        /// The underlying value of the character supporting up to 0x2ffff.
        /// </summary>
        private UInt18 value;

        /// <summary>
        /// Convert a C# char to a Char.
        /// </summary>
        /// <param name="c">The char.</param>
        public static implicit operator Char(char c)
        {
            return new Char(c);
        }

        /// <summary>
        /// Instantiate a new <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The char value as an int.</param>
        public Char(int value)
        {
            CommonUtilities.ValidateIsTrue(value >= 0, "character value can not be negative.");
            CommonUtilities.ValidateIsTrue(value <= 0x2ffff, "character value out of range.");
            this.value = new UInt18(value);
        }

        /// <summary>
        /// Equality for characters.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Char c && this.value.Equals(c.value);
        }

        /// <summary>
        /// Hashcode for characters.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// Convert this char to a UTF-16 string.
        /// </summary>
        /// <returns>A string that is either a single character or a surrogate pair.</returns>
        public override string ToString()
        {
            return char.ConvertFromUtf32((int)this.value.ToLong());
        }
    }
}
