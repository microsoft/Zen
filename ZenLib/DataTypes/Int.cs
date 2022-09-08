// <copyright file="Int.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// A class representing a fixed bit size integer with signed semantics.
    /// </summary>
    public class Int<TSize> : Bitvec<TSize, Signed>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Int{TSize}"/> class.
        /// </summary>
        /// <param name="bytes">The bytes in Big Endian.</param>
        public Int(byte[] bytes) : base(bytes)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Int{TSize}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Int(long value) : base(value)
        {
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public Int<TSize> BitwiseAnd(Int<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new Int<TSize>(base.BitwiseAndBytes(other));
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public Int<TSize> BitwiseOr(Int<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new Int<TSize>(base.BitwiseOrBytes(other));
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public Int<TSize> BitwiseXor(Int<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new Int<TSize>(base.BitwiseXorBytes(other));
        }

        /// <summary>
        /// Compute the bitwise negation of an integer.
        /// </summary>
        /// <returns>The negated integer.</returns>
        public Int<TSize> Negate()
        {
            return new Int<TSize>(base.NegateBytes());
        }

        /// <summary>
        /// Compute the bitwise negation of an integer.
        /// </summary>
        /// <returns>The negated integer.</returns>
        public Int<TSize> BitwiseNot()
        {
            return new Int<TSize>(base.BitwiseNotBytes());
        }

        /// <summary>
        /// Adds the integer with another of the same size.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The added integer.</returns>
        public Int<TSize> Add(Int<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new Int<TSize>(base.AddBytes(other));
        }

        /// <summary>
        /// Subtracts an integer from this integer.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The subtracted integer.</returns>
        public Int<TSize> Subtract(Int<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new Int<TSize>(base.SubtractBytes(other));
        }
    }
}
