// <copyright file="Char.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace ZenLib
{
    /// <summary>
    /// A unicode character that is compatible with SMT-LIB.
    /// </summary>
    public sealed class Char : IEquatable<Char>, IComparable<Char>
    {
        /// <summary>
        /// The minimum char value.
        /// </summary>
        public static readonly Char MinValue = new Char(0);

        /// <summary>
        /// The maximum char value.
        /// </summary>
        public static readonly Char MaxValue = new Char(0x2ffff);

        /// <summary>
        /// The underlying value of the character supporting up to 0x2ffff.
        /// </summary>
        internal UInt18 Value;

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
            this.Value = new UInt18(value);
        }

        /// <summary>
        /// Equality for characters.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Char c && this.Equals(c);
        }

        /// <summary>
        /// Equality for chars.
        /// </summary>
        /// <param name="other">The other char.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Char other)
        {
            return this.Value.Equals(other.Value);
        }

        /// <summary>
        /// Hashcode for characters.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Escape this character using the \uXXXXX notation.
        /// </summary>
        /// <returns>The escaped character.</returns>
        public string Escape()
        {
            return @"\u{" + this.Value.ToLong().ToString("X5") + "}";
        }

        /// <summary>
        /// Convert this char to a UTF-16 string.
        /// </summary>
        /// <returns>A string that is either a single character or a surrogate pair.</returns>
        public override string ToString()
        {
            var intVal = (int)this.Value.ToLong();

            // we need to leave escaped any characters in the range d800-dfff since
            // these characters can not be represented in strings as they are part
            // of a surrogate pair used for UTF-16 encodings.
            if (intVal >= 0xd800 && intVal <= 0xdfff)
            {
                return @"\u{" + intVal.ToString("X5") + "}";
            }

            if (intVal <= 0xffff)
            {
                return ((char)intVal).ToString();
            }

            return char.ConvertFromUtf32(intVal);
        }

        /// <summary>
        /// Compare this character to another.
        /// </summary>
        /// <param name="other">The other character.</param>
        /// <returns>An int representing the result.</returns>
        public int CompareTo(Char other)
        {
            return this.Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Equality for chars.
        /// </summary>
        /// <param name="left">The left char.</param>
        /// <param name="right">The right char.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(Char left, Char right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for chars.
        /// </summary>
        /// <param name="left">The left char.</param>
        /// <param name="right">The right char.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(Char left, Char right)
        {
            return !(left == right);
        }
    }
}
