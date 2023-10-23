// <copyright file="RegexBinopExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Regex binary operation expression.
    /// </summary>
    internal sealed class RegexBinopExpr<T> : Regex<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Regex<T>, Regex<T>, RegexBinopExprType), Regex<T>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for Regex terms.
        /// </summary>
        private static Flyweight<(long, long, int), Regex<T>> hashConsTable = new Flyweight<(long, long, int), Regex<T>>();

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

                // simplify (a & (a & b)) = a & b
                if (e2 is RegexBinopExpr<T> w && w.OpType == RegexBinopExprType.Intersection && w.Expr1.Equals(e1))
                {
                    return e2;
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

                // simplify (a + (a + b)) = a + b
                if (e2 is RegexBinopExpr<T> w && w.OpType == RegexBinopExprType.Union && w.Expr1.Equals(e1))
                {
                    return e2;
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

                // simplify [a-b] + [c-d] = [a-b] where [a-b] contains [c-d]
                if (e1 is RegexRangeExpr<T> rng1 && e2 is RegexRangeExpr<T> rng2 &&
                    rng1.CharacterRange.Contains(rng2.CharacterRange.Low) &&
                    rng1.CharacterRange.Contains(rng2.CharacterRange.High))
                {
                    return e1;
                }

                // simplify [a-b] + [c-d] = [c-d] where [a-b] is contained by [c-d]
                if (e1 is RegexRangeExpr<T> rng3 && e2 is RegexRangeExpr<T> rng4 &&
                    rng4.CharacterRange.Contains(rng3.CharacterRange.Low) &&
                    rng4.CharacterRange.Contains(rng3.CharacterRange.High))
                {
                    return e2;
                }

                // To ensure that the regular expression derivative algorithm works
                // we need to maintain the invariant that unions are ordered such
                // that they are invariant with respect to the commutativity and
                // associativity equivalence relation. To do so, we arrange the
                // unions to be sorted and right associative: a + (b + (c + d)).
                // First, we obtain the order of all nested unions using a DFS.
                var stack = new Stack<Regex<T>>();
                var order = new List<Regex<T>>();
                stack.Push(e2);
                stack.Push(e1);
                while (stack.Count > 0)
                {
                    var e = stack.Pop();
                    if (e is RegexBinopExpr<T> z && z.OpType == RegexBinopExprType.Union)
                    {
                        stack.Push(z.Expr2);
                        stack.Push(z.Expr1);
                    }
                    else
                    {
                        order.Add(e);
                    }
                }

                // Check if the terms are already ordered. If so, then we don't
                // need to do anything and we can avoid an infinite recursion.
                var isOrdered = true;
                for (int i = 0; i < order.Count - 1; i++)
                {
                    if (order[i].Id > order[i + 1].Id)
                    {
                        isOrdered = false;
                        break;
                    }
                }

                // If the left term has no unions and the subterms are already ordered
                // then we are done. We know by construction that the second term e2
                // is already in the correct form, we we just return (e1 + e2).
                if (!(e1 is RegexBinopExpr<T> zz && zz.OpType == RegexBinopExprType.Union) && isOrdered)
                {
                    return new RegexBinopExpr<T>(e1, e2, RegexBinopExprType.Union);
                }

                // Otherwise, we will sort the subterms and then construct the regexes
                // in the correct order.
                order.Sort((r1, r2) => r2.Id.CompareTo(r1.Id));
                var acc = order[0];
                for (int j = 1; j < order.Count; j++)
                {
                    acc = Regex.Union(order[j], acc);
                }
                return acc;
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

                // simplify $ . a = \empty
                if (e1 is RegexAnchorExpr<T> a && !a.IsBegin && e2 is RegexRangeExpr<T>)
                {
                    return Regex.Empty<T>();
                }

                // simplify a . ^ = \empty
                if (e2 is RegexAnchorExpr<T> b && b.IsBegin && e1 is RegexRangeExpr<T>)
                {
                    return Regex.Empty<T>();
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
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

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
            return visitor.Visit(this, parameter);
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
