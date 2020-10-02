// <copyright file="IntN.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A class representing a fixed bit size integer.
    /// </summary>
    public abstract class IntN<T, TSign> : IEquatable<IntN<T, TSign>>
    {
        /// <summary>
        /// Gets the number of bits for the integer.
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// The bytes representing the binary of the integer in Big Endian.
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// Whether the integer is signed.
        /// </summary>
        public bool Signed { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="IntN{T, TSign}"/> class.
        /// </summary>
        /// <param name="bytes">The bytes in Big Endian.</param>
        public IntN(byte[] bytes)
        {
            var numBytes = this.NumBytes();

            if (bytes.Length > numBytes)
            {
                throw new ArgumentException($"Invalid byte[] length, expected {this.Size % 8} but got {bytes.Length}");
            }

            this.Signed = typeof(TSign) == typeof(Signed);
            this.Bytes = new byte[numBytes];

            for (int i = 0; i < bytes.Length; i++)
            {
                this.Bytes[this.Bytes.Length - 1 - i] = bytes[bytes.Length - 1 - i];
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IntN{T, TSign}"/> class.
        /// </summary>
        /// <param name="value">The value as a long.</param>
        public IntN(long value)
        {
            this.Signed = typeof(TSign) == typeof(Signed);

            var exp = this.Signed ? this.Size - 1 : this.Size;
            var min = -(int)Math.Pow(2, exp);
            var max = (int)Math.Pow(2, exp) - 1;

            if (value < min || value > max)
            {
                throw new ArgumentException($"Invalid argument: {value} exceeds bit width of integer ({this.Size}).");
            }

            this.Bytes = new byte[this.NumBytes()];

            bool negated = false;

            if (this.Signed && value < 0)
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
                Negate(this.Bytes, this.Bytes, this.Size);
            }
        }

        /// <summary>
        /// Less than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator <=(IntN<T, TSign> left, IntN<T, TSign> right)
        {
            CommonUtilities.ValidateNotNull(left);
            CommonUtilities.ValidateNotNull(right);

            return left.LessThanOrEqual(right);
        }

        /// <summary>
        /// Less than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public bool LessThanOrEqual(IntN<T, TSign> other)
        {
            var ln = GetBit(this.Bytes, this.Size, 0);
            var rn = GetBit(other.Bytes, other.Size, 0);

            if (ln && !rn)
            {
                return true;
            }

            if (!ln && rn)
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
        public static bool operator >=(IntN<T, TSign> left, IntN<T, TSign> right)
        {
            CommonUtilities.ValidateNotNull(left);
            CommonUtilities.ValidateNotNull(right);

            return left.GreaterThanOrEqual(right);
        }

        /// <summary>
        /// Greater than or equal to for fixed bit size integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns></returns>
        public bool GreaterThanOrEqual(IntN<T, TSign> other)
        {
            return !(this <= other) || this == other;
        }

        /// <summary>
        /// Equality for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator ==(IntN<T, TSign> left, IntN<T, TSign> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for fixed bit size integers.
        /// </summary>
        /// <param name="left">The first integer.</param>
        /// <param name="right">The second integer.</param>
        /// <returns></returns>
        public static bool operator !=(IntN<T, TSign> left, IntN<T, TSign> right)
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
            if (!(obj is IntN<T, TSign>))
            {
                return false;
            }

            return Equals(obj as IntN<T, TSign>);
        }

        /// <summary>
        /// Equality of fixed bit integers.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>True or false.</returns>
        public bool Equals(IntN<T, TSign> other)
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
        /// Gets the hash code.
        /// </summary>
        /// <returns>Integer hash code.</returns>
        public override int GetHashCode()
        {
            int hashCode = this.Size + (this.Signed ? 1 : 0);
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
        public T BitwiseAnd(IntN<T, TSign> other)
        {
            CommonUtilities.ValidateNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            BitwiseAnd(this.Bytes, other.Bytes, newBytes);
            return CreateResult(newBytes);
        }

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
        public T BitwiseOr(IntN<T, TSign> other)
        {
            CommonUtilities.ValidateNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            BitwiseOr(this.Bytes, other.Bytes, newBytes);
            return CreateResult(newBytes);
        }

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
        public T BitwiseXor(IntN<T, TSign> other)
        {
            CommonUtilities.ValidateNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            BitwiseXor(this.Bytes, other.Bytes, newBytes);
            return CreateResult(newBytes);
        }

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
        public T Negate()
        {
            var newBytes = new byte[this.Bytes.Length];
            Negate(this.Bytes, newBytes, this.Size);
            return CreateResult(newBytes);
        }

        private static void Negate(byte[] bytes, byte[] result, int size)
        {
            var one = new byte[bytes.Length];
            one[one.Length - 1] = 1;
            BitwiseNot(bytes, result, size);
            Add(result, one, result, size);
        }

        /// <summary>
        /// Compute the bitwise negation of an integer.
        /// </summary>
        /// <returns>The negated integer.</returns>
        public T BitwiseNot()
        {
            var newBytes = new byte[this.Bytes.Length];
            BitwiseNot(this.Bytes, newBytes, this.Size);
            return CreateResult(newBytes);
        }

        private static void BitwiseNot(byte[] bytes, byte[] result, int size)
        {
            for (int i = 0; i < size; i++)
            {
                var b = GetBit(bytes, size, i);
                SetBit(result, size, i, !b);
            }
        }

        /// <summary>
        /// Adds the integer with another of the same size.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The added integer.</returns>
        public T Add(IntN<T, TSign> other)
        {
            CommonUtilities.ValidateNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            Add(this.Bytes, other.Bytes, newBytes, this.Size);
            return CreateResult(newBytes);
        }

        private static void Add(byte[] left, byte[] right, byte[] result, int size)
        {
            var c = false;

            for (int i = size - 1; i >= 0; i--)
            {
                var b1 = GetBit(left, size, i);
                var b2 = GetBit(right, size, i);

                SetBit(result, size, i, b1 ^ b2 ^ c);
                c = (b1 && b2) || ((b1 || b2) && c);
            }
        }

        /// <summary>
        /// Subtracts an integer from this integer.
        /// Wraps around on overflow.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns>The subtracted integer.</returns>
        public T Subtract(IntN<T, TSign> other)
        {
            CommonUtilities.ValidateNotNull(other);

            var newBytes = new byte[this.Bytes.Length];
            Subtract(this.Bytes, other.Bytes, newBytes, this.Size);
            return CreateResult(newBytes);
        }

        private static void Subtract(byte[] left, byte[] right, byte[] result, int size)
        {
            var c = false;

            for (int i = size - 1; i >= 0; i--)
            {
                var b1 = GetBit(left, size, i);
                var b2 = GetBit(right, size, i);

                SetBit(result, size, i, b1 ^ b2 ^ c);
                c = (b1 && b2 && c) || (!b1 && (b2 || c));
            }
        }

        /// <summary>
        /// Create a long value from this integer, if the size permits.
        /// </summary>
        /// <returns></returns>
        public long ToLong()
        {
            if (this.Size > 64)
            {
                throw new ArgumentException($"Cannot convert integer with size: {this.Size} to a long.");
            }

            var bytes = this.Bytes;

            bool negated = GetBit(this.Bytes, this.Size, 0);

            if (negated)
            {
                bytes = new byte[this.Bytes.Length];
                Negate(this.Bytes, bytes, this.Size);
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
            var result = new bool[this.Size];

            for (int i = 0; i < this.Size; i++)
            {
                result[i] = GetBit(this.Bytes, this.Size, i);
            }

            return result;
        }

        /// <summary>
        /// Create a result of the appropriate type from the bytes.
        /// </summary>
        /// <param name="bytes">Bytes to use.</param>
        /// <returns>A result of the desired type.</returns>
        private T CreateResult(byte[] bytes)
        {
            var constructor = typeof(T).GetConstructor(new Type[] { typeof(byte[]) });
            return (T)constructor.Invoke(new object[] { bytes });
        }

        /// <summary>
        /// Returns the number of bytes needed.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        private int NumBytes()
        {
            return 1 + (this.Size - 1) / 8;
        }

        /// <summary>
        /// Gets whether the bit is set at a given position.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="size">The size of the integer.</param>
        /// <param name="position">The position.</param>
        /// <returns>True or false.</returns>
        private static bool GetBit(byte[] bytes, int size, int position)
        {
            var p = size - 1 - position;
            var whichByte = bytes.Length - 1 - (p / 8);
            return (bytes[whichByte] & (1 << (p % 8))) != 0;
        }

        /// <summary>
        /// Gets whether the bit is set at a given position.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="size">The size of the integer.</param>
        /// <param name="position">The position.</param>
        /// <param name="value">Whether to set true or false.</param>
        private static void SetBit(byte[] bytes, int size, int position, bool value)
        {
            var p = size - 1 - position;
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
    /// Signed flag type.
    /// </summary>
    public struct Signed { }

    /// <summary>
    /// Unsigned flag type.
    /// </summary>
    public struct Unsigned { }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    // signed integers

    [ExcludeFromCodeCoverage] public class Int1 : IntN<Int1, Signed> { public override int Size { get { return 1; } } public Int1(byte[] bytes) : base(bytes) { } public Int1(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int2 : IntN<Int2, Signed> { public override int Size { get { return 2; } } public Int2(byte[] bytes) : base(bytes) { } public Int2(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int3 : IntN<Int3, Signed> { public override int Size { get { return 3; } } public Int3(byte[] bytes) : base(bytes) { } public Int3(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int4 : IntN<Int4, Signed> { public override int Size { get { return 4; } } public Int4(byte[] bytes) : base(bytes) { } public Int4(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int5 : IntN<Int5, Signed> { public override int Size { get { return 5; } } public Int5(byte[] bytes) : base(bytes) { } public Int5(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int6 : IntN<Int6, Signed> { public override int Size { get { return 6; } } public Int6(byte[] bytes) : base(bytes) { } public Int6(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int7 : IntN<Int7, Signed> { public override int Size { get { return 7; } } public Int7(byte[] bytes) : base(bytes) { } public Int7(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int8 : IntN<Int8, Signed> { public override int Size { get { return 8; } } public Int8(byte[] bytes) : base(bytes) { } public Int8(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int9 : IntN<Int9, Signed> { public override int Size { get { return 9; } } public Int9(byte[] bytes) : base(bytes) { } public Int9(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int10 : IntN<Int10, Signed> { public override int Size { get { return 10; } } public Int10(byte[] bytes) : base(bytes) { } public Int10(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int11 : IntN<Int11, Signed> { public override int Size { get { return 11; } } public Int11(byte[] bytes) : base(bytes) { } public Int11(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int12 : IntN<Int12, Signed> { public override int Size { get { return 12; } } public Int12(byte[] bytes) : base(bytes) { } public Int12(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int13 : IntN<Int13, Signed> { public override int Size { get { return 13; } } public Int13(byte[] bytes) : base(bytes) { } public Int13(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int14 : IntN<Int14, Signed> { public override int Size { get { return 14; } } public Int14(byte[] bytes) : base(bytes) { } public Int14(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int15 : IntN<Int15, Signed> { public override int Size { get { return 15; } } public Int15(byte[] bytes) : base(bytes) { } public Int15(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int16 : IntN<Int16, Signed> { public override int Size { get { return 16; } } public Int16(byte[] bytes) : base(bytes) { } public Int16(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int17 : IntN<Int17, Signed> { public override int Size { get { return 17; } } public Int17(byte[] bytes) : base(bytes) { } public Int17(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int18 : IntN<Int18, Signed> { public override int Size { get { return 18; } } public Int18(byte[] bytes) : base(bytes) { } public Int18(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int19 : IntN<Int19, Signed> { public override int Size { get { return 19; } } public Int19(byte[] bytes) : base(bytes) { } public Int19(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int20 : IntN<Int20, Signed> { public override int Size { get { return 20; } } public Int20(byte[] bytes) : base(bytes) { } public Int20(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int21 : IntN<Int21, Signed> { public override int Size { get { return 21; } } public Int21(byte[] bytes) : base(bytes) { } public Int21(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int22 : IntN<Int22, Signed> { public override int Size { get { return 22; } } public Int22(byte[] bytes) : base(bytes) { } public Int22(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int23 : IntN<Int23, Signed> { public override int Size { get { return 23; } } public Int23(byte[] bytes) : base(bytes) { } public Int23(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int24 : IntN<Int24, Signed> { public override int Size { get { return 24; } } public Int24(byte[] bytes) : base(bytes) { } public Int24(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int25 : IntN<Int25, Signed> { public override int Size { get { return 25; } } public Int25(byte[] bytes) : base(bytes) { } public Int25(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int26 : IntN<Int26, Signed> { public override int Size { get { return 26; } } public Int26(byte[] bytes) : base(bytes) { } public Int26(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int27 : IntN<Int27, Signed> { public override int Size { get { return 27; } } public Int27(byte[] bytes) : base(bytes) { } public Int27(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int28 : IntN<Int28, Signed> { public override int Size { get { return 28; } } public Int28(byte[] bytes) : base(bytes) { } public Int28(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int29 : IntN<Int29, Signed> { public override int Size { get { return 29; } } public Int29(byte[] bytes) : base(bytes) { } public Int29(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int30 : IntN<Int30, Signed> { public override int Size { get { return 30; } } public Int30(byte[] bytes) : base(bytes) { } public Int30(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int31 : IntN<Int31, Signed> { public override int Size { get { return 31; } } public Int31(byte[] bytes) : base(bytes) { } public Int31(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int32 : IntN<Int32, Signed> { public override int Size { get { return 32; } } public Int32(byte[] bytes) : base(bytes) { } public Int32(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int33 : IntN<Int33, Signed> { public override int Size { get { return 33; } } public Int33(byte[] bytes) : base(bytes) { } public Int33(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int34 : IntN<Int34, Signed> { public override int Size { get { return 34; } } public Int34(byte[] bytes) : base(bytes) { } public Int34(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int35 : IntN<Int35, Signed> { public override int Size { get { return 35; } } public Int35(byte[] bytes) : base(bytes) { } public Int35(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int36 : IntN<Int36, Signed> { public override int Size { get { return 36; } } public Int36(byte[] bytes) : base(bytes) { } public Int36(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int37 : IntN<Int37, Signed> { public override int Size { get { return 37; } } public Int37(byte[] bytes) : base(bytes) { } public Int37(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int38 : IntN<Int38, Signed> { public override int Size { get { return 38; } } public Int38(byte[] bytes) : base(bytes) { } public Int38(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int39 : IntN<Int39, Signed> { public override int Size { get { return 39; } } public Int39(byte[] bytes) : base(bytes) { } public Int39(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int40 : IntN<Int40, Signed> { public override int Size { get { return 40; } } public Int40(byte[] bytes) : base(bytes) { } public Int40(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int41 : IntN<Int41, Signed> { public override int Size { get { return 41; } } public Int41(byte[] bytes) : base(bytes) { } public Int41(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int42 : IntN<Int42, Signed> { public override int Size { get { return 42; } } public Int42(byte[] bytes) : base(bytes) { } public Int42(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int43 : IntN<Int43, Signed> { public override int Size { get { return 43; } } public Int43(byte[] bytes) : base(bytes) { } public Int43(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int44 : IntN<Int44, Signed> { public override int Size { get { return 44; } } public Int44(byte[] bytes) : base(bytes) { } public Int44(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int45 : IntN<Int45, Signed> { public override int Size { get { return 45; } } public Int45(byte[] bytes) : base(bytes) { } public Int45(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int46 : IntN<Int46, Signed> { public override int Size { get { return 46; } } public Int46(byte[] bytes) : base(bytes) { } public Int46(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int47 : IntN<Int47, Signed> { public override int Size { get { return 47; } } public Int47(byte[] bytes) : base(bytes) { } public Int47(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int48 : IntN<Int48, Signed> { public override int Size { get { return 48; } } public Int48(byte[] bytes) : base(bytes) { } public Int48(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int49 : IntN<Int49, Signed> { public override int Size { get { return 49; } } public Int49(byte[] bytes) : base(bytes) { } public Int49(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int50 : IntN<Int50, Signed> { public override int Size { get { return 50; } } public Int50(byte[] bytes) : base(bytes) { } public Int50(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int51 : IntN<Int51, Signed> { public override int Size { get { return 51; } } public Int51(byte[] bytes) : base(bytes) { } public Int51(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int52 : IntN<Int52, Signed> { public override int Size { get { return 52; } } public Int52(byte[] bytes) : base(bytes) { } public Int52(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int53 : IntN<Int53, Signed> { public override int Size { get { return 53; } } public Int53(byte[] bytes) : base(bytes) { } public Int53(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int54 : IntN<Int54, Signed> { public override int Size { get { return 54; } } public Int54(byte[] bytes) : base(bytes) { } public Int54(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int55 : IntN<Int55, Signed> { public override int Size { get { return 55; } } public Int55(byte[] bytes) : base(bytes) { } public Int55(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int56 : IntN<Int56, Signed> { public override int Size { get { return 56; } } public Int56(byte[] bytes) : base(bytes) { } public Int56(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int57 : IntN<Int57, Signed> { public override int Size { get { return 57; } } public Int57(byte[] bytes) : base(bytes) { } public Int57(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int58 : IntN<Int58, Signed> { public override int Size { get { return 58; } } public Int58(byte[] bytes) : base(bytes) { } public Int58(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int59 : IntN<Int59, Signed> { public override int Size { get { return 59; } } public Int59(byte[] bytes) : base(bytes) { } public Int59(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int60 : IntN<Int60, Signed> { public override int Size { get { return 60; } } public Int60(byte[] bytes) : base(bytes) { } public Int60(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int61 : IntN<Int61, Signed> { public override int Size { get { return 61; } } public Int61(byte[] bytes) : base(bytes) { } public Int61(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int62 : IntN<Int62, Signed> { public override int Size { get { return 62; } } public Int62(byte[] bytes) : base(bytes) { } public Int62(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int63 : IntN<Int63, Signed> { public override int Size { get { return 63; } } public Int63(byte[] bytes) : base(bytes) { } public Int63(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int64 : IntN<Int64, Signed> { public override int Size { get { return 64; } } public Int64(byte[] bytes) : base(bytes) { } public Int64(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int128 : IntN<Int128, Signed> { public override int Size { get { return 128; } } public Int128(byte[] bytes) : base(bytes) { } public Int128(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class Int256 : IntN<Int256, Signed> { public override int Size { get { return 256; } } public Int256(byte[] bytes) : base(bytes) { } public Int256(long value) : base(value) { } }

    // unsigned integers

    [ExcludeFromCodeCoverage] public class UInt1 : IntN<UInt1, Unsigned> { public override int Size { get { return 1; } } public UInt1(byte[] bytes) : base(bytes) { } public UInt1(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt2 : IntN<UInt2, Unsigned> { public override int Size { get { return 2; } } public UInt2(byte[] bytes) : base(bytes) { } public UInt2(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt3 : IntN<UInt3, Unsigned> { public override int Size { get { return 3; } } public UInt3(byte[] bytes) : base(bytes) { } public UInt3(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt4 : IntN<UInt4, Unsigned> { public override int Size { get { return 4; } } public UInt4(byte[] bytes) : base(bytes) { } public UInt4(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt5 : IntN<UInt5, Unsigned> { public override int Size { get { return 5; } } public UInt5(byte[] bytes) : base(bytes) { } public UInt5(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt6 : IntN<UInt6, Unsigned> { public override int Size { get { return 6; } } public UInt6(byte[] bytes) : base(bytes) { } public UInt6(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt7 : IntN<UInt7, Unsigned> { public override int Size { get { return 7; } } public UInt7(byte[] bytes) : base(bytes) { } public UInt7(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt8 : IntN<UInt8, Unsigned> { public override int Size { get { return 8; } } public UInt8(byte[] bytes) : base(bytes) { } public UInt8(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt9 : IntN<UInt9, Unsigned> { public override int Size { get { return 9; } } public UInt9(byte[] bytes) : base(bytes) { } public UInt9(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt10 : IntN<UInt10, Unsigned> { public override int Size { get { return 10; } } public UInt10(byte[] bytes) : base(bytes) { } public UInt10(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt11 : IntN<UInt11, Unsigned> { public override int Size { get { return 11; } } public UInt11(byte[] bytes) : base(bytes) { } public UInt11(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt12 : IntN<UInt12, Unsigned> { public override int Size { get { return 12; } } public UInt12(byte[] bytes) : base(bytes) { } public UInt12(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt13 : IntN<UInt13, Unsigned> { public override int Size { get { return 13; } } public UInt13(byte[] bytes) : base(bytes) { } public UInt13(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt14 : IntN<UInt14, Unsigned> { public override int Size { get { return 14; } } public UInt14(byte[] bytes) : base(bytes) { } public UInt14(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt15 : IntN<UInt15, Unsigned> { public override int Size { get { return 15; } } public UInt15(byte[] bytes) : base(bytes) { } public UInt15(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt16 : IntN<UInt16, Unsigned> { public override int Size { get { return 16; } } public UInt16(byte[] bytes) : base(bytes) { } public UInt16(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt17 : IntN<UInt17, Unsigned> { public override int Size { get { return 17; } } public UInt17(byte[] bytes) : base(bytes) { } public UInt17(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt18 : IntN<UInt18, Unsigned> { public override int Size { get { return 18; } } public UInt18(byte[] bytes) : base(bytes) { } public UInt18(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt19 : IntN<UInt19, Unsigned> { public override int Size { get { return 19; } } public UInt19(byte[] bytes) : base(bytes) { } public UInt19(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt20 : IntN<UInt20, Unsigned> { public override int Size { get { return 20; } } public UInt20(byte[] bytes) : base(bytes) { } public UInt20(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt21 : IntN<UInt21, Unsigned> { public override int Size { get { return 21; } } public UInt21(byte[] bytes) : base(bytes) { } public UInt21(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt22 : IntN<UInt22, Unsigned> { public override int Size { get { return 22; } } public UInt22(byte[] bytes) : base(bytes) { } public UInt22(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt23 : IntN<UInt23, Unsigned> { public override int Size { get { return 23; } } public UInt23(byte[] bytes) : base(bytes) { } public UInt23(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt24 : IntN<UInt24, Unsigned> { public override int Size { get { return 24; } } public UInt24(byte[] bytes) : base(bytes) { } public UInt24(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt25 : IntN<UInt25, Unsigned> { public override int Size { get { return 25; } } public UInt25(byte[] bytes) : base(bytes) { } public UInt25(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt26 : IntN<UInt26, Unsigned> { public override int Size { get { return 26; } } public UInt26(byte[] bytes) : base(bytes) { } public UInt26(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt27 : IntN<UInt27, Unsigned> { public override int Size { get { return 27; } } public UInt27(byte[] bytes) : base(bytes) { } public UInt27(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt28 : IntN<UInt28, Unsigned> { public override int Size { get { return 28; } } public UInt28(byte[] bytes) : base(bytes) { } public UInt28(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt29 : IntN<UInt29, Unsigned> { public override int Size { get { return 29; } } public UInt29(byte[] bytes) : base(bytes) { } public UInt29(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt30 : IntN<UInt30, Unsigned> { public override int Size { get { return 30; } } public UInt30(byte[] bytes) : base(bytes) { } public UInt30(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt31 : IntN<UInt31, Unsigned> { public override int Size { get { return 31; } } public UInt31(byte[] bytes) : base(bytes) { } public UInt31(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt32 : IntN<UInt32, Unsigned> { public override int Size { get { return 32; } } public UInt32(byte[] bytes) : base(bytes) { } public UInt32(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt33 : IntN<UInt33, Unsigned> { public override int Size { get { return 33; } } public UInt33(byte[] bytes) : base(bytes) { } public UInt33(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt34 : IntN<UInt34, Unsigned> { public override int Size { get { return 34; } } public UInt34(byte[] bytes) : base(bytes) { } public UInt34(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt35 : IntN<UInt35, Unsigned> { public override int Size { get { return 35; } } public UInt35(byte[] bytes) : base(bytes) { } public UInt35(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt36 : IntN<UInt36, Unsigned> { public override int Size { get { return 36; } } public UInt36(byte[] bytes) : base(bytes) { } public UInt36(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt37 : IntN<UInt37, Unsigned> { public override int Size { get { return 37; } } public UInt37(byte[] bytes) : base(bytes) { } public UInt37(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt38 : IntN<UInt38, Unsigned> { public override int Size { get { return 38; } } public UInt38(byte[] bytes) : base(bytes) { } public UInt38(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt39 : IntN<UInt39, Unsigned> { public override int Size { get { return 39; } } public UInt39(byte[] bytes) : base(bytes) { } public UInt39(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt40 : IntN<UInt40, Unsigned> { public override int Size { get { return 40; } } public UInt40(byte[] bytes) : base(bytes) { } public UInt40(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt41 : IntN<UInt41, Unsigned> { public override int Size { get { return 41; } } public UInt41(byte[] bytes) : base(bytes) { } public UInt41(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt42 : IntN<UInt42, Unsigned> { public override int Size { get { return 42; } } public UInt42(byte[] bytes) : base(bytes) { } public UInt42(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt43 : IntN<UInt43, Unsigned> { public override int Size { get { return 43; } } public UInt43(byte[] bytes) : base(bytes) { } public UInt43(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt44 : IntN<UInt44, Unsigned> { public override int Size { get { return 44; } } public UInt44(byte[] bytes) : base(bytes) { } public UInt44(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt45 : IntN<UInt45, Unsigned> { public override int Size { get { return 45; } } public UInt45(byte[] bytes) : base(bytes) { } public UInt45(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt46 : IntN<UInt46, Unsigned> { public override int Size { get { return 46; } } public UInt46(byte[] bytes) : base(bytes) { } public UInt46(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt47 : IntN<UInt47, Unsigned> { public override int Size { get { return 47; } } public UInt47(byte[] bytes) : base(bytes) { } public UInt47(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt48 : IntN<UInt48, Unsigned> { public override int Size { get { return 48; } } public UInt48(byte[] bytes) : base(bytes) { } public UInt48(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt49 : IntN<UInt49, Unsigned> { public override int Size { get { return 49; } } public UInt49(byte[] bytes) : base(bytes) { } public UInt49(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt50 : IntN<UInt50, Unsigned> { public override int Size { get { return 50; } } public UInt50(byte[] bytes) : base(bytes) { } public UInt50(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt51 : IntN<UInt51, Unsigned> { public override int Size { get { return 51; } } public UInt51(byte[] bytes) : base(bytes) { } public UInt51(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt52 : IntN<UInt52, Unsigned> { public override int Size { get { return 52; } } public UInt52(byte[] bytes) : base(bytes) { } public UInt52(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt53 : IntN<UInt53, Unsigned> { public override int Size { get { return 53; } } public UInt53(byte[] bytes) : base(bytes) { } public UInt53(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt54 : IntN<UInt54, Unsigned> { public override int Size { get { return 54; } } public UInt54(byte[] bytes) : base(bytes) { } public UInt54(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt55 : IntN<UInt55, Unsigned> { public override int Size { get { return 55; } } public UInt55(byte[] bytes) : base(bytes) { } public UInt55(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt56 : IntN<UInt56, Unsigned> { public override int Size { get { return 56; } } public UInt56(byte[] bytes) : base(bytes) { } public UInt56(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt57 : IntN<UInt57, Unsigned> { public override int Size { get { return 57; } } public UInt57(byte[] bytes) : base(bytes) { } public UInt57(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt58 : IntN<UInt58, Unsigned> { public override int Size { get { return 58; } } public UInt58(byte[] bytes) : base(bytes) { } public UInt58(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt59 : IntN<UInt59, Unsigned> { public override int Size { get { return 59; } } public UInt59(byte[] bytes) : base(bytes) { } public UInt59(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt60 : IntN<UInt60, Unsigned> { public override int Size { get { return 60; } } public UInt60(byte[] bytes) : base(bytes) { } public UInt60(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt61 : IntN<UInt61, Unsigned> { public override int Size { get { return 61; } } public UInt61(byte[] bytes) : base(bytes) { } public UInt61(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt62 : IntN<UInt62, Unsigned> { public override int Size { get { return 62; } } public UInt62(byte[] bytes) : base(bytes) { } public UInt62(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt63 : IntN<UInt63, Unsigned> { public override int Size { get { return 63; } } public UInt63(byte[] bytes) : base(bytes) { } public UInt63(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt64 : IntN<UInt64, Unsigned> { public override int Size { get { return 64; } } public UInt64(byte[] bytes) : base(bytes) { } public UInt64(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt128 : IntN<UInt128, Unsigned> { public override int Size { get { return 128; } } public UInt128(byte[] bytes) : base(bytes) { } public UInt128(long value) : base(value) { } }
    [ExcludeFromCodeCoverage] public class UInt256 : IntN<UInt256, Unsigned> { public override int Size { get { return 256; } } public UInt256(byte[] bytes) : base(bytes) { } public UInt256(long value) : base(value) { } }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
