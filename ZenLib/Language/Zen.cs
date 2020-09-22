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
        /// Simplify an expression by unrolling.
        /// </summary>
        /// <returns></returns>
        internal abstract Zen<T> Unroll();

        /// <summary>
        /// Accept a visitor for the ZenExpr object.
        /// </summary>
        /// <returns>A value of the return type.</returns>
        internal abstract TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter);

        /// <summary>
        /// Simplify an expression recursively.
        /// </summary>
        /// <returns></returns>
        public Zen<T> Simplify()
        {
            return CommonUtilities.RunWithLargeStack(() => this.Unroll());
        }

        /// <summary>
        /// Convert a bool to the appropriate Zen type.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(T x)
        {
            return (Zen<T>)ReflectionUtilities.CreateZenConstant(x);
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
