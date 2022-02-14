// <copyright file="ZenDictUnionExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a dictionary union expression.
    /// </summary>
    internal sealed class ZenDictCombineExpr<TKey> : Zen<IDictionary<TKey, SetUnit>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<IDictionary<TKey, SetUnit>>, Zen<IDictionary<TKey, SetUnit>>, CombineType), ZenDictCombineExpr<TKey>> createFunc = (v) =>
            new ZenDictCombineExpr<TKey>(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenDictUnionExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), ZenDictCombineExpr<TKey>> hashConsTable =
            new HashConsTable<(long, long, int), ZenDictCombineExpr<TKey>>();

        /// <summary>
        /// Unroll a ZenDictUnionExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<IDictionary<TKey, SetUnit>> Unroll()
        {
            return Create(this.DictExpr1.Unroll(), this.DictExpr2.Unroll(), this.CombinationType);
        }

        /// <summary>
        /// Create a new ZenDictUnionExpr.
        /// </summary>
        /// <param name="dictExpr1">The first dictionary expr.</param>
        /// <param name="dictExpr2">The second dictionary expr.</param>
        /// <param name="combineType">The combination type.</param>
        /// <returns>The new expr.</returns>
        public static ZenDictCombineExpr<TKey> Create(
            Zen<IDictionary<TKey, SetUnit>> dictExpr1,
            Zen<IDictionary<TKey, SetUnit>> dictExpr2,
            CombineType combineType)
        {
            CommonUtilities.ValidateNotNull(dictExpr1);
            CommonUtilities.ValidateNotNull(dictExpr2);

            var k = (dictExpr1.Id, dictExpr2.Id, (int)combineType);
            hashConsTable.GetOrAdd(k, (dictExpr1, dictExpr2, combineType), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenDictCombineExpr{TKey}"/> class.
        /// </summary>
        /// <param name="dictExpr1">The first dictionary expression.</param>
        /// <param name="dictExpr2">The second dictionary expression.</param>
        /// <param name="combineType">The combination type.</param>
        private ZenDictCombineExpr(
            Zen<IDictionary<TKey, SetUnit>> dictExpr1,
            Zen<IDictionary<TKey, SetUnit>> dictExpr2,
            CombineType combineType)
        {
            this.DictExpr1 = dictExpr1;
            this.DictExpr2 = dictExpr2;
            this.CombinationType = combineType;
        }

        /// <summary>
        /// Gets the first dictionary expr.
        /// </summary>
        public Zen<IDictionary<TKey, SetUnit>> DictExpr1 { get; }

        /// <summary>
        /// Gets the second dictionary expr.
        /// </summary>
        public Zen<IDictionary<TKey, SetUnit>> DictExpr2 { get; }

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
                    return $"Union({this.DictExpr1}, {this.DictExpr2})";
                case CombineType.Intersect:
                    return $"Intersect({this.DictExpr1}, {this.DictExpr2})";
                default:
                    return $"Difference({this.DictExpr1}, {this.DictExpr2})";
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
        internal override TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitZenDictCombineExpr(this, parameter);
        }

        internal enum CombineType
        {
            Union,
            Intersect,
        }
    }
}
