// <copyright file="Bitvec.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Text;

    /// <summary>
    /// A class representing a fixed bit size integer.
    /// </summary>
    public abstract class Bitvec<TSize, TSign> : IEquatable<Bitvec<TSize, TSign>>, IComparable<Bitvec<TSize, TSign>>
    {
        /// <summary>
        /// Gets the number of bits for the integer.
        /// </summary>
        public static int Size { get; }

        /// <summary>
        /// Gets whether the integer is signed.
        /// </summary>
        public static bool Signed { get; }

        /// <summary>
        /// The bytes representing the binary of the integer in Big Endian.
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// Static initializer for the bitvector class.
        /// </summary>
        static Bitvec()
        {
            var sizeType = typeof(TSize).ToString();
            try
            {
                Size = int.Parse(sizeType.Split('_')[1]);
            }
            catch
            {
                throw new ZenException($"Invalid integer size type: {sizeType}");
            }

            Signed = typeof(TSign) == typeof(Signed);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Bitvec{TSize, TSign}"/> class.
        /// </summary>
        /// <param name="bytes">The bytes in Big Endian.</param>
        public Bitvec(byte[] bytes)
        {
            var numBytes = this.NumBytes();
            this.Bytes = CommonUtilities.CopyBigEndian<byte>(bytes, 0, numBytes);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Bitvec{TSize, TSign}"/> class.
        /// </summary>
        /// <param name="value">The value as a long.</param>
        public Bitvec(long value)
        {
            var exp = Signed ? Size - 1 : Size;
            var min = -(int)Math.Pow(2, exp);
            var max = (int)Math.Pow(2, exp) - 1;

            if (value < min || value > max)
            {
                throw new ArgumentException($"Invalid argument: {value} exceeds bit width of integer ({Size}).");
            }

            this.Bytes = new byte[this.NumBytes()];

            bool negated = false;

            if (Signed && value < 0)
            {
                negated = true;
                value = -value;
            }

            for (int i = 0; i < Math.Min(8, this.Bytes.Length); i++)
            {
                this.Bytes[this.Bytes.Length - 1 - i] = (byte)(value & 0x00000000000000FF);
                value >>= 8;
            }

            if (negated)
            {
                Negate(this.Bytes, this.Bytes);
            }
        }

        /// <summary>
        /// Compare this integer to another.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>An integer.</returns>
        public int CompareTo(Bitvec<TSize, TSign> other)
        {
            if (this.Equals(other))
            {
                return 0;
            }
            else if (this.LessThanOrEqual(other))
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Less than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator <=(Bitvec<TSize, TSign> left, Bitvec<TSize, TSign> right)
        {
            Contract.AssertNotNull(left);
            Contract.AssertNotNull(right);

            return left.LessThanOrEqual(right);
        }

        /// <summary>
        /// Less than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public bool LessThanOrEqual(Bitvec<TSize, TSign> other)
        {
            var ln = GetBit(this.Bytes, 0);
            var rn = GetBit(other.Bytes, 0);

            if (Signed && ln && !rn)
            {
                return true;
            }

            if (Signed && !ln && rn)
            {
                return false;
            }

            for (int i = 0; i < this.Bytes.Length; i++)
            {
                if (this.Bytes[i] < other.Bytes[i])
                {
                    return true;
                }

                if (other.Bytes[i] < this.Bytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Greater than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator >=(Bitvec<TSize, TSign> left, Bitvec<TSize, TSign> right)
        {
            Contract.AssertNotNull(left);
            Contract.AssertNotNull(right);

            return left.GreaterThanOrEqual(right);
        }

        /// <summary>
        /// Greater than for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator >(Bitvec<TSize, TSign> left, Bitvec<TSize, TSign> right)
        {
            Contract.AssertNotNull(left);
            Contract.AssertNotNull(right);

            return left.GreaterThan(right);
        }

        /// <summary>
        /// Less than for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator <(Bitvec<TSize, TSign> left, Bitvec<TSize, TSign> right)
        {
            Contract.AssertNotNull(left);
            Contract.AssertNotNull(right);

            return left.LessThan(right);
        }

        /// <summary>
        /// Greater than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public bool GreaterThanOrEqual(Bitvec<TSize, TSign> other)
        {
            return !(this <= other) || this == other;
        }

        /// <summary>
        /// Greater than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public bool GreaterThan(Bitvec<TSize, TSign> other)
        {
            return !(this <= other);
        }

        /// <summary>
        /// Greater than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public bool LessThan(Bitvec<TSize, TSign> other)
        {
            return this <= other && this != other;
        }

        /// <summary>
        /// Equality for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator ==(Bitvec<TSize, TSign> left, Bitvec<TSize, TSign> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator !=(Bitvec<TSize, TSign> left, Bitvec<TSize, TSign> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Equality of fixed bit integers.
        /// </summary>
        /// <param name="obj">The other integer.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Bitvec<TSize, TSign>))
            {
                return false;
            }

            return Equals(obj as Bitvec<TSize, TSign>);
        }

        /// <summary>
        /// Equality of fixed bit integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Bitvec<TSize, TSign> other)
        {
            if (other is null)
            {
                return false;
            }

            for (int i = 0; i < this.Bytes.Length; i++)
            {
                if (this.Bytes[i] != other.Bytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Convert the integer to a string.
        /// </summary>
        /// <returns>The integer as a string.</returns>
        public override string ToString()
        {
            if (Size <= 64)
            {
                var l = this.ToLong();
                return Signed ? l.ToString() : ((ulong)l).ToString();
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append("#b");
                for (int i = 0; i < Size; i++)
                {
                    sb.Append(this.GetBit(i) ? "1" : "0");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>Integer hash code.</returns>
        public override int GetHashCode()
        {
            int hashCode = Size + (Signed ? 1 : 0);
            for (int i = 0; i < this.Bytes.Length; i++)
            {
                hashCode = 31 * hashCode + this.Bytes[i];
            }

            return hashCode;
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        internal byte[] BitwiseAndBytes(Bitvec<TSize, TSign> other)
        {
            Contract.AssertNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            BitwiseAnd(this.Bytes, other.Bytes, newBytes);
            return newBytes;
        }

        /// <summary>
        /// Bitwise and of two bitvectors.
        /// </summary>
        /// <param name="left">The left bytes.</param>
        /// <param name="right">The right bytes.</param>
        /// <param name="result">The result bytes.</param>
        private static void BitwiseAnd(byte[] left, byte[] right, byte[] result)
        {
            for (int i = 0; i < left.Length; i++)
            {
                result[i] = (byte)(left[i] & right[i]);
            }
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        internal byte[] BitwiseOrBytes(Bitvec<TSize, TSign> other)
        {
            Contract.AssertNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            BitwiseOr(this.Bytes, other.Bytes, newBytes);
            return newBytes;
        }

        /// <summary>
        /// Bitwise or of two bitvectors.
        /// </summary>
        /// <param name="left">The left bytes.</param>
        /// <param name="right">The right bytes.</param>
        /// <param name="result">The result bytes.</param>
        private static void BitwiseOr(byte[] left, byte[] right, byte[] result)
        {
            for (int i = 0; i < left.Length; i++)
            {
                result[i] = (byte)(left[i] | right[i]);
            }
        }

        /// <summary>
        /// Compute the bitwise and of two integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        internal byte[] BitwiseXorBytes(Bitvec<TSize, TSign> other)
        {
            Contract.AssertNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            BitwiseXor(this.Bytes, other.Bytes, newBytes);
            return newBytes;
        }

        /// <summary>
        /// Bitwise xor of two bitvectors.
        /// </summary>
        /// <param name="left">The left bytes.</param>
        /// <param name="right">The right bytes.</param>
        /// <param name="result">The result bytes.</param>
        private static void BitwiseXor(byte[] left, byte[] right, byte[] result)
        {
            for (int i = 0; i < left.Length; i++)
            {
                result[i] = (byte)(left[i] ^ right[i]);
            }
        }

        /// <summary>
        /// Compute the bitwise negation of an integer.
        /// </summary>
        /// <returns>The negated integer.</returns>
        internal byte[] NegateBytes()
        {
            var newBytes = new byte[this.Bytes.Length];
            Negate(this.Bytes, newBytes);
            return newBytes;
        }

        /// <summary>
        /// Bitwise negation of a bitvectors.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="result">The result.</param>
        private static void Negate(byte[] bytes, byte[] result)
        {
            var one = new byte[bytes.Length];
            one[one.Length - 1] = 1;
            BitwiseNot(bytes, result);
            Add(result, one, result);
        }

        /// <summary>
        /// Compute the bitwise negation of an integer.
        /// </summary>
        /// <returns>The negated integer.</returns>
        internal byte[] BitwiseNotBytes()
        {
            var newBytes = new byte[this.Bytes.Length];
            BitwiseNot(this.Bytes, newBytes);
            return newBytes;
        }

        /// <summary>
        /// Bitwise not of bitvectors.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="result">The result bytes.</param>
        private static void BitwiseNot(byte[] bytes, byte[] result)
        {
            for (int i = 0; i < Size; i++)
            {
                var b = GetBit(bytes, i);
                SetBit(result, i, !b);
            }
        }

        /// <summary>
        /// Adds the integer with another of the same size.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The added integer.</returns>
        internal byte[] AddBytes(Bitvec<TSize, TSign> other)
        {
            Contract.AssertNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            Add(this.Bytes, other.Bytes, newBytes);
            return newBytes;
        }

        /// <summary>
        /// Add two bitvectors together.
        /// </summary>
        /// <param name="left">The left bytes.</param>
        /// <param name="right">The right bytes.</param>
        /// <param name="result">The result bytes.</param>
        private static void Add(byte[] left, byte[] right, byte[] result)
        {
            var c = false;

            for (int i = Size - 1; i >= 0; i--)
            {
                var b1 = GetBit(left, i);
                var b2 = GetBit(right, i);

                SetBit(result, i, b1 ^ b2 ^ c);
                c = (b1 && b2) || ((b1 || b2) && c);
            }
        }

        /// <summary>
        /// Subtracts an integer from this integer.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The subtracted integer.</returns>
        internal byte[] SubtractBytes(Bitvec<TSize, TSign> other)
        {
            Contract.AssertNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            Subtract(this.Bytes, other.Bytes, newBytes);
            return newBytes;
        }

        /// <summary>
        /// Subtraction of two bitvectors.
        /// </summary>
        /// <param name="left">The left bytes.</param>
        /// <param name="right">The right bytes.</param>
        /// <param name="result">The result bytes.</param>
        private static void Subtract(byte[] left, byte[] right, byte[] result)
        {
            var c = false;

            for (int i = Size - 1; i >= 0; i--)
            {
                var b1 = GetBit(left, i);
                var b2 = GetBit(right, i);

                SetBit(result, i, b1 ^ b2 ^ c);
                c = (b1 && b2 && c) || (!b1 && (b2 || c));
            }
        }

        /// <summary>
        /// Create a long value from this integer, if the size permits.
        /// </summary>
        /// <returns></returns>
        public long ToLong()
        {
            if (Size > 64)
            {
                throw new ArgumentException($"Cannot convert integer with size: {Size} to a long.");
            }

            var bytes = this.Bytes;

            bool negated = Signed && GetBit(this.Bytes, 0);

            if (negated)
            {
                bytes = new byte[this.Bytes.Length];
                Negate(this.Bytes, bytes);
            }

            long result = 0L;

            for (int i = 0; i < bytes.Length; i++)
            {
                result <<= 8;
                result |= bytes[i];
            }

            return negated ? -result : result;
        }

        /// <summary>
        /// Gets the bits for the integer.
        /// </summary>
        /// <returns>The bits as an array.</returns>
        public bool[] GetBits()
        {
            var result = new bool[Size];

            for (int i = 0; i < Size; i++)
            {
                result[i] = GetBit(this.Bytes, i);
            }

            return result;
        }

        /// <summary>
        /// Sets a bit at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">Whether to set true or false.</param>
        public void SetBit(int position, bool value)
        {
            Contract.Assert(position >= 0);
            Contract.Assert(position < Size);
            SetBit(this.Bytes, position, value);
        }

        /// <summary>
        /// Gets whether the bit is set at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>True or false.</returns>
        public bool GetBit(int position)
        {
            Contract.Assert(position >= 0);
            Contract.Assert(position < Size);
            return GetBit(this.Bytes, position);
        }

        /// <summary>
        /// Returns the number of bytes needed.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        private int NumBytes()
        {
            return 1 + (Size - 1) / 8;
        }

        /// <summary>
        /// Gets whether the bit is set at a given position.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="position">The position.</param>
        /// <returns>True or false.</returns>
        private static bool GetBit(byte[] bytes, int position)
        {
            var p = Size - 1 - position;
            var whichByte = bytes.Length - 1 - (p / 8);
            return (bytes[whichByte] & (1 << (p % 8))) != 0;
        }

        /// <summary>
        /// Gets whether the bit is set at a given position.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="position">The position.</param>
        /// <param name="value">Whether to set true or false.</param>
        private static void SetBit(byte[] bytes, int position, bool value)
        {
            var p = Size - 1 - position;
            var whichByte = bytes.Length - 1 - (p / 8);
            var mask = 1 << (p % 8);

            if (value)
            {
                bytes[whichByte] |= (byte)mask;
            }
            else
            {
                bytes[whichByte] &= (byte)~mask;
            }
        }
    }

    /// <summary>
    /// Static methods for fixed integers.
    /// </summary>
    public static class Bitvec
    {
        /// <summary>
        /// Cast one finite integer to another finite integer type.
        /// </summary>
        /// <param name="x">The source finite integer.</param>
        /// <returns>The resulting finite integer.</returns>
        public static TTarget CastFiniteInteger<TSource, TTarget>(TSource x)
        {
            var b1 = ReflectionUtilities.IsFixedIntegerType(typeof(TSource));
            var b2 = ReflectionUtilities.IsFixedIntegerType(typeof(TTarget));

            if (!b1 && !b2)
            {
                return (TTarget)(dynamic)x;
            }
            else if (b1 && !b2)
            {
                var result = 0L;
                byte[] bytes = ((dynamic)x).Bytes;
                for (int i = 0; i < Math.Min(bytes.Length, 4); i++)
                {
                    result <<= 8;
                    result |= bytes[bytes.Length - 1 - i];
                }

                return (TTarget)(dynamic)result;
            }
            else
            {
                byte[] bytes;
                if (b1)
                {
                    bytes = ((dynamic)x).Bytes;
                }
                else
                {
                    bytes = BitConverter.GetBytes((long)(dynamic)x);
                    if (BitConverter.IsLittleEndian)
                    {
                        System.Array.Reverse(bytes);
                    }
                }

                var c = typeof(TTarget).GetConstructor(new Type[] { typeof(byte[]) });
                return (TTarget)c.Invoke(new object[] { bytes });
            }
        }
    }
}
