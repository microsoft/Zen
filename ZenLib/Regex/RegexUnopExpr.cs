// <copyright file="RegexUnopExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Regex unary operation expression.
    /// </summary>
    internal sealed class RegexUnopExpr<T> : Regex<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Regex<T>, RegexUnopExprType), Regex<T>> createFunc = (v) => Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for Regex terms.
        /// </summary>
        private static HashConsTable<(long, int), Regex<T>> hashConsTable = new HashConsTable<(long, int), Regex<T>>();

        /// <summary>
        /// Simplify a new RegexUnopExpr.
        /// </summary>
        /// <param name="expr">The Regex expr.</param>
        /// <param name="opType">The regex operation type.</param>
        /// <returns>The new Regex expr.</returns>
        private static Regex<T> Simplify(Regex<T> expr, RegexUnopExprType opType)
        {
            if (opType == RegexUnopExprType.Star)
            {
                // simplify (r*)* = r*
                if (expr is RegexUnopExpr<T> x && x.OpType == RegexUnopExprType.Star)
                {
                    return expr;
                }

                // simplify \epsilon* = \epsilon
                if (expr is RegexEpsilonExpr<T>)
                {
                    return expr;
                }

                // simplify \empty* = \epsilon
                if (expr is RegexEmptyExpr<T>)
                {
                    return Regex.Epsilon<T>();
                }

                // simplify .* = \not \empty
                if (expr is RegexRangeExpr<T> r && r.CharacterRange.IsFull())
                {
                    return Regex.Negation(Regex.Empty<T>());
                }
            }

            if (opType == RegexUnopExprType.Negation)
            {
                // simplify not(not(r)) = r
                if (expr is RegexUnopExpr<T> y && y.OpType == RegexUnopExprType.Negation)
                {
                    return y.Expr;
                }

                // simplify not(range(min, max)) = \empty
                if (expr is RegexRangeExpr<T> e && e.CharacterRange.IsFull())
                {
                    return Regex.Empty<T>();
                }
            }

            return new RegexUnopExpr<T>(expr, opType);
        }

        /// <summary>
        /// Creates a new RegexUnopExpr.
        /// </summary>
        /// <param name="expr">The Regex expr.</param>
        /// <param name="opType">The operation type.</param>
        /// <returns>The new Regex expr.</returns>
        public static Regex<T> Create(Regex<T> expr, RegexUnopExprType opType)
        {
            Contract.AssertNotNull(expr);

            var key = (expr.Id, (int)opType);
            hashConsTable.GetOrAdd(key, (expr, opType), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexUnopExpr{T}"/> class.
        /// </summary>
        /// <param name="expr">The Regex expression.</param>
        /// <param name="opType">The Regex operation type.</param>
        private RegexUnopExpr(Regex<T> expr, RegexUnopExprType opType)
        {
            this.Expr = expr;
            this.OpType = opType;
        }

        /// <summary>
        /// Gets the first Regex expression.
        /// </summary>
        internal Regex<T> Expr { get; }

        /// <summary>
        /// Gets the Regex operation type.
        /// </summary>
        internal RegexUnopExprType OpType { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            switch (this.OpType)
            {
                case RegexUnopExprType.Star:
                    return $"Star({this.Expr})";
                case RegexUnopExprType.Negation:
                    return $"Not({this.Expr})";
                default:
                    throw new ZenUnreachableException();
            }
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The visitor parameter.</param>
        /// <typeparam name="TParam">The visitor parameter type.</typeparam>
        /// <typeparam name="TReturn">The visitor return type.</typeparam>
        /// <returns>A return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(IRegexExprVisitor<T, TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.Visit(this, parameter);
        }
    }

    /// <summary>
    /// The regex binary operation type.
    /// </summary>
    internal enum RegexUnopExprType
    {
        /// <summary>
        /// A Kleene star of an expression.
        /// </summary>
        Star,

        /// <summary>
        /// The negation of a regular expression.
        /// </summary>
        Negation,
    }
}
