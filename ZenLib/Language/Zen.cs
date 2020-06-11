// <copyright file="ZenExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    /// <summary>
    /// A Zen expression object parameterized over the C# type.
    /// </summary>
    /// <typeparam name="T">Return type as a C# type.</typeparam>
    public abstract class Zen<T>
    {
        /// <summary>
        /// Accept a visitor for the ZenExpr object.
        /// </summary>
        /// <returns>A value of the return type.</returns>
        internal abstract TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter);

        /// <summary>
        /// Convert a bool to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(bool x)
        {
            return ConvertIntegerConstant<T>(x ? 1L : 0L);
        }

        /// <summary>
        /// Convert a byte to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(byte x)
        {
            return ConvertIntegerConstant<T>(x);
        }

        /// <summary>
        /// Convert a short to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(short x)
        {
            return ConvertIntegerConstant<T>(x);
        }

        /// <summary>
        /// Convert a ushort to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(ushort x)
        {
            return ConvertIntegerConstant<T>(x);
        }

        /// <summary>
        /// Convert a int to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(int x)
        {
            return ConvertIntegerConstant<T>(x);
        }

        /// <summary>
        /// Convert a uint to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(uint x)
        {
            return ConvertIntegerConstant<T>(x);
        }

        /// <summary>
        /// Convert a long to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(long x)
        {
            return ConvertIntegerConstant<T>(x);
        }

        /// <summary>
        /// Convert a ulong to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(ulong x)
        {
            return ConvertIntegerConstant<T>((long)x);
        }

        /// <summary>
        /// Convert a string to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(string x)
        {
            return ConvertStringConstant<T>((string)x);
        }

        /// <summary>
        /// Equality between two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator ==(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Eq(expr1, expr2);
        }

        /// <summary>
        /// Inequality between two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator !=(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Not(Language.Eq(expr1, expr2));
        }

        /// <summary>
        /// Less than or equal for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator <=(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Leq(expr1, expr2);
        }

        /// <summary>
        /// Less than for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator <(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Lt(expr1, expr2);
        }

        /// <summary>
        /// Greater than for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator >(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Gt(expr1, expr2);
        }

        /// <summary>
        /// Greater than or equal for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator >=(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Geq(expr1, expr2);
        }

        /// <summary>
        /// Sum of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator +(Zen<T> expr1, Zen<T> expr2)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.StringType)
            {
                return Language.Concat(expr1 as Zen<string>, expr2 as Zen<string>) as Zen<T>;
            }

            return Language.Plus(expr1, expr2);
        }

        /// <summary>
        /// Subtraction of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator -(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Minus(expr1, expr2);
        }

        /// <summary>
        /// Multiplication of a Zen object with a constant.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator *(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.Multiply(expr1, expr2);
        }

        /// <summary>
        /// Bitwise and of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator &(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.BitwiseAnd(expr1, expr2);
        }

        /// <summary>
        /// Bitwise or of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator |(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.BitwiseOr(expr1, expr2);
        }

        /// <summary>
        /// Bitwise xor of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator ^(Zen<T> expr1, Zen<T> expr2)
        {
            return Language.BitwiseXor(expr1, expr2);
        }

        /// <summary>
        /// Bitwise negation of a Zen object.
        /// </summary>
        /// <param name="expr">Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator ~(Zen<T> expr)
        {
            return Language.BitwiseNot(expr);
        }

        /// <summary>
        /// Convert an input to the correct integer type.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns></returns>
        private static Zen<TZen> ConvertIntegerConstant<TZen>(long o)
        {
            var type = typeof(TZen);

            if (type == ReflectionUtilities.BoolType)
            {
                return (Zen<TZen>)(object)Language.Bool(o == 0 ? false : true);
            }

            if (type == ReflectionUtilities.ByteType)
            {
                return (Zen<TZen>)(object)Language.Byte((byte)o);
            }

            if (type == ReflectionUtilities.ShortType)
            {
                return (Zen<TZen>)(object)Language.Short((short)o);
            }

            if (type == ReflectionUtilities.UshortType)
            {
                return (Zen<TZen>)(object)Language.UShort((ushort)o);
            }

            if (type == ReflectionUtilities.IntType)
            {
                return (Zen<TZen>)(object)Language.Int((int)o);
            }

            if (type == ReflectionUtilities.UintType)
            {
                return (Zen<TZen>)(object)Language.UInt((uint)o);
            }

            if (type == ReflectionUtilities.LongType)
            {
                return (Zen<TZen>)(object)Language.Long(o);
            }

            if (type == ReflectionUtilities.UlongType)
            {
                return (Zen<TZen>)(object)Language.ULong((ulong)o);
            }

            throw new ZenException($"Invalid implicit conversion from integer to type: {type}");
        }

        /// <summary>
        /// Convert an input to the Zen string type.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns></returns>
        private static Zen<TZen> ConvertStringConstant<TZen>(string s)
        {
            var type = typeof(TZen);

            if (type == ReflectionUtilities.StringType)
            {
                return (Zen<TZen>)(object)Language.String(s);
            }

            throw new ZenException($"Invalid implicit conversion from string to type: {type}");
        }

        /// <summary>
        /// Reference equality for Zen objects.
        /// </summary>
        /// <param name="obj">Other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Hash code for a Zen object.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
