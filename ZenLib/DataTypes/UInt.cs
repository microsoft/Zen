// <copyright file="UInt.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// A class representing a fixed bit size integer with unsigned semantics.
    /// </summary>
    public class UInt<TSize> : Bitvec<TSize, Unsigned>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="UInt{TSize}"/> class.
        /// </summary>
        /// <param name="bytes">The bytes in Big Endian.</param>
        public UInt(byte[] bytes) : base(bytes)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UInt{TSize}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public UInt(long value) : base(value)
        {
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public UInt<TSize> BitwiseAnd(UInt<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new UInt<TSize>(base.BitwiseAndBytes(other));
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public UInt<TSize> BitwiseOr(UInt<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new UInt<TSize>(base.BitwiseOrBytes(other));
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public UInt<TSize> BitwiseXor(UInt<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new UInt<TSize>(base.BitwiseXorBytes(other));
        }

        /// <summary>
        /// Compute the bitwise negation of an integer.
        /// </summary>
        /// <returns>The negated integer.</returns>
        public UInt<TSize> BitwiseNot()
        {
            return new UInt<TSize>(base.BitwiseNotBytes());
        }

        /// <summary>
        /// Adds the integer with another of the same size.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The added integer.</returns>
        public UInt<TSize> Add(UInt<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new UInt<TSize>(base.AddBytes(other));
        }

        /// <summary>
        /// Subtracts an integer from this integer.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The subtracted integer.</returns>
        public UInt<TSize> Subtract(UInt<TSize> other)
        {
            Contract.AssertNotNull(other);
            return new UInt<TSize>(base.SubtractBytes(other));
        }
    }
}
