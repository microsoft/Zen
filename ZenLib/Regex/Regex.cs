// <copyright file="Regex.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Threading;

    /// <summary>
    /// A regex object parameterized over a C# type.
    /// </summary>
    /// <typeparam name="T">The type of characters for the regex.</typeparam>
    public abstract class Regex<T> where T : IComparable<T>
    {
        /// <summary>
        /// The next unique id.
        /// </summary>
        private static long nextId = 0;

        /// <summary>
        /// The unique id for the given Zen expression.
        /// </summary>
        public long Id = Interlocked.Increment(ref nextId);

        /// <summary>
        /// Accept a visitor for the ZenExpr object.
        /// </summary>
        /// <returns>A value of the return type.</returns>
        internal abstract TReturn Accept<TParam, TReturn>(IRegexExprVisitor<T, TParam, TReturn> visitor, TParam parameter);

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

    /// <summary>
    /// Static factory class for building regular expressions for Zen.
    /// </summary>
    public static class Regex
    {
        /// <summary>
        /// The 'Empty' regular expression.
        /// </summary>
        /// <returns>A regular expression that accepts no strings.</returns>
        public static Regex<T> Empty<T>() where T : IComparable<T>
        {
            return RegexEmptyExpr<T>.Instance;
        }

        /// <summary>
        /// The 'All' regular expression.
        /// </summary>
        /// <returns>A regular expression that accepts all strings.</returns>
        public static Regex<T> All<T>() where T : IComparable<T>
        {
            return RegexAllExpr<T>.Instance;
        }

        /// <summary>
        /// The 'Epsilon' regular expression.
        /// </summary>
        /// <returns>A regular expression that accepts a single empty string.</returns>
        public static Regex<T> Epsilon<T>() where T : IComparable<T>
        {
            return RegexEpsilonExpr<T>.Instance;
        }

        /// <summary>
        /// The 'Range' regular expression.
        /// </summary>
        /// <param name="low">The low character value.</param>
        /// <param name="high">The high character value.</param>
        /// <returns>A regular expression that accepts a single character.</returns>
        public static Regex<T> Range<T>(T low, T high) where T : IComparable<T>
        {
            return RegexRangeExpr<T>.Create(low, high);
        }

        /// <summary>
        /// The 'Char' regular expression.
        /// </summary>
        /// <param name="value">The character value.</param>
        /// <returns>A regular expression that accepts a single character.</returns>
        public static Regex<T> Char<T>(T value) where T : IComparable<T>
        {
            return RegexRangeExpr<T>.Create(value, value);
        }

        /// <summary>
        /// The 'Union' regular expression.
        /// </summary>
        /// <param name="expr1">The first Regex expr.</param>
        /// <param name="expr2">The second Regex expr.</param>
        /// <returns>A regular expression that accepts the union of two others.</returns>
        public static Regex<T> Union<T>(Regex<T> expr1, Regex<T> expr2) where T : IComparable<T>
        {
            return RegexBinopExpr<T>.Create(expr1, expr2, RegexBinopExprType.Union);
        }

        /// <summary>
        /// The 'Intersect' regular expression.
        /// </summary>
        /// <param name="expr1">The first Regex expr.</param>
        /// <param name="expr2">The second Regex expr.</param>
        /// <returns>A regular expression that accepts the intersection of two others.</returns>
        public static Regex<T> Intersect<T>(Regex<T> expr1, Regex<T> expr2) where T : IComparable<T>
        {
            return RegexBinopExpr<T>.Create(expr1, expr2, RegexBinopExprType.Intersection);
        }

        /// <summary>
        /// The 'Concat' regular expression.
        /// </summary>
        /// <param name="expr1">The first Regex expr.</param>
        /// <param name="expr2">The second Regex expr.</param>
        /// <returns>A regular expression that accepts the concatenation two others.</returns>
        public static Regex<T> Concat<T>(Regex<T> expr1, Regex<T> expr2) where T : IComparable<T>
        {
            return RegexBinopExpr<T>.Create(expr1, expr2, RegexBinopExprType.Concatenation);
        }

        /// <summary>
        /// The 'Star' regular expression.
        /// </summary>
        /// <param name="expr">The Regex expr.</param>
        /// <returns>A regular expression that accepts zero or more iterations of another.</returns>
        public static Regex<T> Star<T>(Regex<T> expr) where T : IComparable<T>
        {
            return RegexUnopExpr<T>.Create(expr, RegexUnopExprType.Star);
        }

        /// <summary>
        /// The 'Negation' regular expression.
        /// </summary>
        /// <param name="expr">The Regex expr.</param>
        /// <returns>A regular expression that accepts any strings another doesn't.</returns>
        public static Regex<T> Negation<T>(Regex<T> expr) where T : IComparable<T>
        {
            return RegexUnopExpr<T>.Create(expr, RegexUnopExprType.Negation);
        }
    }
}
