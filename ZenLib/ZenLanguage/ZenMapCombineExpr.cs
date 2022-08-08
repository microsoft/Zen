// <copyright file="ZenMapCombineExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a map union expression.
    /// </summary>
    internal sealed class ZenMapCombineExpr<TKey> : Zen<Map<TKey, SetUnit>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<Map<TKey, SetUnit>>, Zen<Map<TKey, SetUnit>>, CombineType), Zen<Map<TKey, SetUnit>>> createFunc = (v) =>
            Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenMapCombineExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<Map<TKey, SetUnit>>> hashConsTable =
            new HashConsTable<(long, long, int), Zen<Map<TKey, SetUnit>>>();

        /// <summary>
        /// Unroll a ZenMapCombineExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<Map<TKey, SetUnit>> Unroll()
        {
            return Create(this.MapExpr1.Unroll(), this.MapExpr2.Unroll(), this.CombinationType);
        }

        /// <summary>
        /// Simplify and create a new ZenMapCombineExpr.
        /// </summary>
        /// <param name="map1">The map expr.</param>
        /// <param name="map2">The map expr.</param>
        /// <param name="combinationType">The combination type.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Map<TKey, SetUnit>> Simplify(
            Zen<Map<TKey, SetUnit>> map1,
            Zen<Map<TKey, SetUnit>> map2,
            CombineType combinationType)
        {
            // a union a = a
            // a inter a = a
            // a minus a = {}
            if (map1.Equals(map2))
            {
                return combinationType == CombineType.Difference ? Zen.EmptyMap<TKey, SetUnit>() : map1;
            }

            // {} union a == a
            // {} inter a == {}
            // {} minus a == {}
            if (map1 is ZenMapEmptyExpr<TKey, SetUnit>)
            {
                return combinationType == CombineType.Union ? map2 : map1;
            }

            // a union {} == a
            // a inter {} == {}
            // a minus {} == a
            if (map2 is ZenMapEmptyExpr<TKey, SetUnit>)
            {
                return combinationType == CombineType.Intersect ? map2 : map1;
            }

            // (a union b) union b == (a union b)
            // (a union b) union a == (a union b)
            // (a inter b) inter b == (a inter b)
            // (a inter b) inter a == (a inter b)
            if (map1 is ZenMapCombineExpr<TKey> e1 &&
                e1.CombinationType == combinationType &&
                combinationType != CombineType.Difference &&
                (e1.MapExpr1.Equals(map2) || e1.MapExpr2.Equals(map2)))
            {
                return map1;
            }

            // a union (a union b) == (a union b)
            // b union (a union b) == (a union b)
            // a inter (a inter b) == (a inter b)
            // b inter (a inter b) == (a inter b)
            if (map2 is ZenMapCombineExpr<TKey> e2 &&
                e2.CombinationType == combinationType &&
                combinationType != CombineType.Difference &&
                (e2.MapExpr1.Equals(map1) || e2.MapExpr2.Equals(map1)))
            {
                return map2;
            }

            // (a minus b) minus a == {}
            // (a minus b) minus b == a minus b
            if (map1 is ZenMapCombineExpr<TKey> e3 && combinationType == CombineType.Difference)
            {
                if (e3.CombinationType == CombineType.Difference && e3.MapExpr1.Equals(map2))
                {
                    return Zen.EmptyMap<TKey, SetUnit>();
                }

                if (e3.CombinationType == CombineType.Difference && e3.MapExpr2.Equals(map2))
                {
                    return map1;
                }
            }

            return new ZenMapCombineExpr<TKey>(map1, map2, combinationType);
        }

        /// <summary>
        /// Create a new ZenMapCombineExpr.
        /// </summary>
        /// <param name="mapExpr1">The first map expr.</param>
        /// <param name="mapExpr2">The second map expr.</param>
        /// <param name="combineType">The combination type.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Map<TKey, SetUnit>> Create(
            Zen<Map<TKey, SetUnit>> mapExpr1,
            Zen<Map<TKey, SetUnit>> mapExpr2,
            CombineType combineType)
        {
            Contract.AssertNotNull(mapExpr1);
            Contract.AssertNotNull(mapExpr2);

            var k = (mapExpr1.Id, mapExpr2.Id, (int)combineType);
            hashConsTable.GetOrAdd(k, (mapExpr1, mapExpr2, combineType), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenMapCombineExpr{TKey}"/> class.
        /// </summary>
        /// <param name="mapExpr1">The first map expression.</param>
        /// <param name="mapExpr2">The second map expression.</param>
        /// <param name="combineType">The combination type.</param>
        private ZenMapCombineExpr(
            Zen<Map<TKey, SetUnit>> mapExpr1,
            Zen<Map<TKey, SetUnit>> mapExpr2,
            CombineType combineType)
        {
            this.MapExpr1 = mapExpr1;
            this.MapExpr2 = mapExpr2;
            this.CombinationType = combineType;
        }

        /// <summary>
        /// Gets the first map expr.
        /// </summary>
        public Zen<Map<TKey, SetUnit>> MapExpr1 { get; }

        /// <summary>
        /// Gets the second map expr.
        /// </summary>
        public Zen<Map<TKey, SetUnit>> MapExpr2 { get; }

        /// <summary>
        /// Gets the combination type.
        /// </summary>
        public CombineType CombinationType { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            switch (this.CombinationType)
            {
                case CombineType.Union:
                    return $"Union({this.MapExpr1}, {this.MapExpr2})";
                case CombineType.Intersect:
                    return $"Intersect({this.MapExpr1}, {this.MapExpr2})";
                case CombineType.Difference:
                    return $"Difference({this.MapExpr1}, {this.MapExpr2})";
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitMapCombine(this, parameter);
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        internal override void Accept(ZenExprActionVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal enum CombineType
        {
            Union,
            Intersect,
            Difference,
        }
    }
}
