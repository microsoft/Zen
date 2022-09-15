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
        /// Hash cons table for ZenMapCombineExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<Map<TKey, SetUnit>>> hashConsTable =
            new HashConsTable<(long, long, int), Zen<Map<TKey, SetUnit>>>();

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
        /// Simplify and create a new ZenMapCombineExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Map<TKey, SetUnit>> Simplify(
            (Zen<Map<TKey, SetUnit>> map1, Zen<Map<TKey, SetUnit>> map2, CombineType combinationType) args)
        {
            // a union a = a
            // a inter a = a
            // a minus a = {}
            if (args.map1.Equals(args.map2))
            {
                return args.combinationType == CombineType.Difference ? Zen.EmptyMap<TKey, SetUnit>() : args.map1;
            }

            // {} union a == a
            // {} inter a == {}
            // {} minus a == {}
            if (args.map1 is ZenConstantExpr<Map<TKey, SetUnit>> ce1 && ce1.Value.Values.Count == 0)
            {
                return args.combinationType == CombineType.Union ? args.map2 : args.map1;
            }

            // a union {} == a
            // a inter {} == {}
            // a minus {} == a
            if (args.map2 is ZenConstantExpr<Map<TKey, SetUnit>> ce2 && ce2.Value.Values.Count == 0)
            {
                return args.combinationType == CombineType.Intersect ? args.map2 : args.map1;
            }

            // (a union b) union b == (a union b)
            // (a union b) union a == (a union b)
            // (a inter b) inter b == (a inter b)
            // (a inter b) inter a == (a inter b)
            if (args.map1 is ZenMapCombineExpr<TKey> e1 &&
                e1.CombinationType == args.combinationType &&
                args.combinationType != CombineType.Difference &&
                (e1.MapExpr1.Equals(args.map2) || e1.MapExpr2.Equals(args.map2)))
            {
                return args.map1;
            }

            // a union (a union b) == (a union b)
            // b union (a union b) == (a union b)
            // a inter (a inter b) == (a inter b)
            // b inter (a inter b) == (a inter b)
            if (args.map2 is ZenMapCombineExpr<TKey> e2 &&
                e2.CombinationType == args.combinationType &&
                args.combinationType != CombineType.Difference &&
                (e2.MapExpr1.Equals(args.map1) || e2.MapExpr2.Equals(args.map1)))
            {
                return args.map2;
            }

            // (a minus b) minus a == {}
            // (a minus b) minus b == a minus b
            if (args.map1 is ZenMapCombineExpr<TKey> e3 && args.combinationType == CombineType.Difference)
            {
                if (e3.CombinationType == CombineType.Difference && e3.MapExpr1.Equals(args.map2))
                {
                    return Zen.EmptyMap<TKey, SetUnit>();
                }

                if (e3.CombinationType == CombineType.Difference && e3.MapExpr2.Equals(args.map2))
                {
                    return args.map1;
                }
            }

            return new ZenMapCombineExpr<TKey>(args.map1, args.map2, args.combinationType);
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
            hashConsTable.GetOrAdd(k, (mapExpr1, mapExpr2, combineType), Simplify, out var v);
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
