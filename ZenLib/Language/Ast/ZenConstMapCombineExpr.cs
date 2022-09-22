// <copyright file="ZenConstMapCombineExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a cmap union expression.
    /// </summary>
    internal sealed class ZenConstMapCombineExpr<TKey> : Zen<CMap<TKey, bool>>
    {
        /// <summary>
        /// Gets the first map expr.
        /// </summary>
        public Zen<CMap<TKey, bool>> MapExpr1 { get; }

        /// <summary>
        /// Gets the second map expr.
        /// </summary>
        public Zen<CMap<TKey, bool>> MapExpr2 { get; }

        /// <summary>
        /// Gets the combination type.
        /// </summary>
        public CSetCombineType CombinationType { get; }

        /// <summary>
        /// Simplify and create a new ZenMapCombineExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<CMap<TKey, bool>> Simplify(
            (Zen<CMap<TKey, bool>> map1, Zen<CMap<TKey, bool>> map2, CSetCombineType combinationType) args)
        {
            // a union a = a
            // a inter a = a
            // a minus a = {}
            if (args.map1.Equals(args.map2))
            {
                return args.combinationType == CSetCombineType.Difference ? Zen.EmptyCMap<TKey, bool>() : args.map1;
            }

            // {} union a == a
            // {} inter a == {}
            // {} minus a == {}
            if (args.map1 is ZenConstantExpr<CMap<TKey, bool>> ce1 && ce1.Value.Values.Count == 0)
            {
                return args.combinationType == CSetCombineType.Union ? args.map2 : args.map1;
            }

            // a union {} == a
            // a inter {} == {}
            // a minus {} == a
            if (args.map2 is ZenConstantExpr<CMap<TKey, bool>> ce2 && ce2.Value.Values.Count == 0)
            {
                return args.combinationType == CSetCombineType.Intersect ? args.map2 : args.map1;
            }

            // (a union b) union b == (a union b)
            // (a union b) union a == (a union b)
            // (a inter b) inter b == (a inter b)
            // (a inter b) inter a == (a inter b)
            if (args.map1 is ZenConstMapCombineExpr<TKey> e1 &&
                e1.CombinationType == args.combinationType &&
                args.combinationType != CSetCombineType.Difference &&
                (e1.MapExpr1.Equals(args.map2) || e1.MapExpr2.Equals(args.map2)))
            {
                return args.map1;
            }

            // a union (a union b) == (a union b)
            // b union (a union b) == (a union b)
            // a inter (a inter b) == (a inter b)
            // b inter (a inter b) == (a inter b)
            if (args.map2 is ZenConstMapCombineExpr<TKey> e2 &&
                e2.CombinationType == args.combinationType &&
                args.combinationType != CSetCombineType.Difference &&
                (e2.MapExpr1.Equals(args.map1) || e2.MapExpr2.Equals(args.map1)))
            {
                return args.map2;
            }

            // (a minus b) minus a == {}
            // (a minus b) minus b == a minus b
            if (args.map1 is ZenConstMapCombineExpr<TKey> e3 && args.combinationType == CSetCombineType.Difference)
            {
                if (e3.CombinationType == CSetCombineType.Difference && e3.MapExpr1.Equals(args.map2))
                {
                    return Zen.EmptyCMap<TKey, bool>();
                }

                if (e3.CombinationType == CSetCombineType.Difference && e3.MapExpr2.Equals(args.map2))
                {
                    return args.map1;
                }
            }

            return new ZenConstMapCombineExpr<TKey>(args.map1, args.map2, args.combinationType);
        }

        /// <summary>
        /// Create a new ZenMapCombineExpr.
        /// </summary>
        /// <param name="mapExpr1">The first map expr.</param>
        /// <param name="mapExpr2">The second map expr.</param>
        /// <param name="combineType">The combination type.</param>
        /// <returns>The new expr.</returns>
        public static Zen<CMap<TKey, bool>> Create(
            Zen<CMap<TKey, bool>> mapExpr1,
            Zen<CMap<TKey, bool>> mapExpr2,
            CSetCombineType combineType)
        {
            Contract.AssertNotNull(mapExpr1);
            Contract.AssertNotNull(mapExpr2);

            var k = (mapExpr1.Id, mapExpr2.Id, (int)combineType);
            var flyweight = ZenAstCache<ZenConstMapCombineExpr<TKey>, (long, long, int), Zen<CMap<TKey, bool>>>.Flyweight;
            flyweight.GetOrAdd(k, (mapExpr1, mapExpr2, combineType), Simplify, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenMapCombineExpr{TKey}"/> class.
        /// </summary>
        /// <param name="mapExpr1">The first map expression.</param>
        /// <param name="mapExpr2">The second map expression.</param>
        /// <param name="combineType">The combination type.</param>
        private ZenConstMapCombineExpr(
            Zen<CMap<TKey, bool>> mapExpr1,
            Zen<CMap<TKey, bool>> mapExpr2,
            CSetCombineType combineType)
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
                case CSetCombineType.Union:
                    return $"Union({this.MapExpr1}, {this.MapExpr2})";
                case CSetCombineType.Intersect:
                    return $"Intersect({this.MapExpr1}, {this.MapExpr2})";
                case CSetCombineType.Difference:
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
            return visitor.VisitConstMapCombine(this, parameter);
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        internal override void Accept(ZenExprActionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <summary>
        /// A set combination type.
        /// </summary>
        internal enum CSetCombineType
        {
            Union,
            Intersect,
            Difference,
        }
    }
}
