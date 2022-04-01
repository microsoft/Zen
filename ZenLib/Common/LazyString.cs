// <copyright file="LazyString.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Text;

    /// <summary>
    /// A string that delays expensive concatenation.
    /// It represents concatenations by building tree of concatenated strings.
    /// </summary>
    internal class LazyString
    {
        /// <summary>
        /// The string value for this node.
        /// </summary>
        private string value;

        /// <summary>
        /// The left string node.
        /// </summary>
        private LazyString left;

        /// <summary>
        ///  The right string node.
        /// </summary>
        private LazyString right;

        /// <summary>
        /// The length of the string.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="LazyString"/> class.
        /// </summary>
        /// <param name="value">The string value.</param>
        public LazyString(string value)
        {
            this.value = value;
            this.left = null;
            this.right = null;
            this.Length = this.value.Length;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LazyString"/> class.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private LazyString(LazyString left, LazyString right)
        {
            this.value = null;
            this.left = left;
            this.right = right;
            this.Length = this.left.Length + this.right.Length;
        }

        /// <summary>
        /// Concatenate two strings together.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns></returns>
        public static LazyString operator +(LazyString s1, LazyString s2)
        {
            return new LazyString(s1, s2);
        }

        /// <summary>
        /// Write this value to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        public void Write(StringBuilder sb)
        {
            if (this.left != null)
            {
                this.left.Write(sb);
            }

            if (this.value != null)
            {
                sb.Append(this.value);
            }

            if (this.right != null)
            {
                this.right.Write(sb);
            }
        }
    }
}