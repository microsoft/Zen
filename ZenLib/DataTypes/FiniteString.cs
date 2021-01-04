// <copyright file="FiniteString.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A class representing a finite unicode string.
    /// </summary>
    public struct FiniteString : IEquatable<FiniteString>
    {
        /// <summary>
        /// Convert a string to a FiniteString.
        /// </summary>
        /// <param name="s">The string.</param>
        public static implicit operator FiniteString(string s)
        {
            return new FiniteString(s);
        }

        /// <summary>
        /// Create a new finite string from a string.
        /// </summary>
        /// <param name="s">The string.</param>
        public FiniteString(string s)
        {
            var chars = new List<ushort>();
            foreach (var c in s)
            {
                chars.Add(c);
            }

            this.Characters = chars;
        }

        /// <summary>
        /// Gets the underlying characters.
        /// </summary>
        public IList<ushort> Characters { get; set; }

        /// <summary>
        /// Convert the finite string to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var c in this.Characters)
            {
                sb.Append((char)c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Equality between finite strings.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Whether they are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is FiniteString @string && Equals(@string);
        }

        /// <summary>
        /// Equality between finite strings.
        /// </summary>
        /// <param name="other">The other string.</param>
        /// <returns>Whether they are equal.</returns>
        public bool Equals(FiniteString other)
        {
            if (this.Characters.Count != other.Characters.Count)
            {
                return false;
            }

            return Enumerable.SequenceEqual(this.Characters, other.Characters);
        }

        /// <summary>
        /// Equality between finite strings.
        /// </summary>
        /// <param name="left">The left string.</param>
        /// <param name="right">The right string.</param>
        /// <returns>Whether they are equal.</returns>
        public static bool operator ==(FiniteString left, FiniteString right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality between finite strings.
        /// </summary>
        /// <param name="left">The left string.</param>
        /// <param name="right">The right string.</param>
        /// <returns></returns>
        public static bool operator !=(FiniteString left, FiniteString right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets a hash code for the finite string.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            int hash = 7;
            foreach (var c in this.Characters)
            {
                hash = 31 * hash + c;
            }

            return hash;
        }
    }
}
