// <copyright file="RegexBinopExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Regex binary operation expression.
    /// </summary>
    internal sealed class RegexBinopExpr<T> : Regex<T>
        where T : IComparable<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Regex<T>, Regex<T>, RegexBinopExprType), Regex<T>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for Regex terms.
        /// </summary>
        private static HashConsTable<(long, long, int), Regex<T>> hashConsTable = new HashConsTable<(long, long, int), Regex<T>>();

        /// <summary>
        /// Simplify a new RegexBinopExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <param name="opType">The regex operation type.</param>
        /// <returns>The new Regex expr.</returns>
        private static Regex<T> Simplify(Regex<T> e1, Regex<T> e2, RegexBinopExprType opType)
        {
            if (opType == RegexBinopExprType.Intersection)
            {
                // simplify (r & r) = r
                if (ReferenceEquals(e1, e2))
                {
                    return e1;
                }

                // simplify \empty & r = \empty
                if (e1 is RegexEmptyExpr<T>)
                {
                    return e1;
                }

                // simplify r & \empty = \empty
                if (e2 is RegexEmptyExpr<T>)
                {
                    return e2;
                }

                // simplify not(\empty) & r = r
                if (e1 is RegexUnopExpr<T> x && x.OpType == RegexUnopExprType.Negation && x.Expr is RegexEmptyExpr<T>)
                {
                    return e2;
                }

                // simplilfy r & not(\empty) = r
                if (e2 is RegexUnopExpr<T> y && y.OpType == RegexUnopExprType.Negation && y.Expr is RegexEmptyExpr<T>)
                {
                    return e1;
                }

                // simplify (r & s) & t = r & (s & t)
                if (e1 is RegexBinopExpr<T> z && z.OpType == RegexBinopExprType.Intersection)
                {
                    return Regex.Intersect(z.Expr1, Regex.Intersect(z.Expr2, e2));
                }

                // simplify r & s = s & r when s < r
                if (e2.Id < e1.Id)
                {
                    return Regex.Intersect(e2, e1);
                }
            }

            if (opType == RegexBinopExprType.Union)
            {
                // simplify (r + r) = r
                if (ReferenceEquals(e1, e2))
                {
                    return e1;
                }

                // simplify \empty + r = r
                if (e1 is RegexEmptyExpr<T>)
                {
                    return e2;
                }

                // simplify r + \empty = r
                if (e2 is RegexEmptyExpr<T>)
                {
                    return e1;
                }

                // simplify not(\empty) + r = not(\empty)
                if (e1 is RegexUnopExpr<T> x && x.OpType == RegexUnopExprType.Negation && x.Expr is RegexEmptyExpr<T>)
                {
                    return e1;
                }

                // simplify r + not(\empty) = not(\empty)
                if (e2 is RegexUnopExpr<T> y && y.OpType == RegexUnopExprType.Negation && y.Expr is RegexEmptyExpr<T>)
                {
                    return e2;
                }

                // simplify (r + s) + t = r + (s + t)
                if (e1 is RegexBinopExpr<T> z && z.OpType == RegexBinopExprType.Union)
                {
                    return Regex.Union(z.Expr1, Regex.Union(z.Expr2, e2));
                }

                // simplify r + s = s + r when s < r
                if (e2.Id < e1.Id)
                {
                    return Regex.Union(e2, e1);
                }
            }

            if (opType == RegexBinopExprType.Concatenation)
            {
                // simplify \empty . r = \empty
                if (e1 is RegexEmptyExpr<T> || e2 is RegexEmptyExpr<T>)
                {
                    return Regex.Empty<T>();
                }

                // simplify \epsilon . r = r
                if (e1 is RegexEpsilonExpr<T>)
                {
                    return e2;
                }

                // simplify r . \epsilon = r
                if (e2 is RegexEpsilonExpr<T>)
                {
                    return e1;
                }

                // simplify (r . s) . t = r . (s . t)
                if (e1 is RegexBinopExpr<T> z && z.OpType == RegexBinopExprType.Concatenation)
                {
                    return Regex.Concat(z.Expr1, Regex.Concat(z.Expr2, e2));
                }
            }

            return new RegexBinopExpr<T>(e1, e2, opType);
        }

        /// <summary>
        /// Creates a new RegexBinopExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <param name="opType">The operation type.</param>
        /// <returns>The new Regex expr.</returns>
        public static Regex<T> Create(Regex<T> expr1, Regex<T> expr2, RegexBinopExprType opType)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var key = (expr1.Id, expr2.Id, (int)opType);
            hashConsTable.GetOrAdd(key, (expr1, expr2, opType), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexBinopExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">First Regex expression.</param>
        /// <param name="expr2">Second Regex expression.</param>
        /// <param name="opType">The Regex operation type.</param>
        private RegexBinopExpr(Regex<T> expr1, Regex<T> expr2, RegexBinopExprType opType)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
            this.OpType = opType;
        }

        /// <summary>
        /// Gets the first Regex expression.
        /// </summary>
        internal Regex<T> Expr1 { get; }

        /// <summary>
        /// Gets the second Regex expression.
        /// </summary>
        internal Regex<T> Expr2 { get; }

        /// <summary>
        /// Gets the Regex operation type.
        /// </summary>
        internal RegexBinopExprType OpType { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            switch (this.OpType)
            {
                case RegexBinopExprType.Union:
                    return $"Union({this.Expr1}, {this.Expr2})";
                case RegexBinopExprType.Intersection:
                    return $"Inter({this.Expr1}, {this.Expr2})";
                case RegexBinopExprType.Concatenation:
                    return $"Concat({this.Expr1}, {this.Expr2})";
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
            return visitor.VisitRegexBinopExpr(this, parameter);
        }
    }

    /// <summary>
    /// The regex binary operation type.
    /// </summary>
    internal enum RegexBinopExprType
    {
        /// <summary>
        /// A union of two regular expressions.
        /// </summary>
        Union,

        /// <summary>
        /// An intersection of two regular expressions.
        /// </summary>
        Intersection,

        /// <summary>
        /// A concatentation of two regular expressions.
        /// </summary>
        Concatenation,
    }
}
