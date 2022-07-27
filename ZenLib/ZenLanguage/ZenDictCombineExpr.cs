// <copyright file="ZenDictUnionExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a dictionary union expression.
    /// </summary>
    internal sealed class ZenDictCombineExpr<TKey> : Zen<Map<TKey, SetUnit>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<Map<TKey, SetUnit>>, Zen<Map<TKey, SetUnit>>, CombineType), Zen<Map<TKey, SetUnit>>> createFunc = (v) =>
            Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenDictUnionExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<Map<TKey, SetUnit>>> hashConsTable =
            new HashConsTable<(long, long, int), Zen<Map<TKey, SetUnit>>>();

        /// <summary>
        /// Unroll a ZenDictUnionExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<Map<TKey, SetUnit>> Unroll()
        {
            return Create(this.DictExpr1.Unroll(), this.DictExpr2.Unroll(), this.CombinationType);
        }

        /// <summary>
        /// Simplify and create a new ZenDictCombineExpr.
        /// </summary>
        /// <param name="dict1">The dictionary expr.</param>
        /// <param name="dict2">The dictionary expr.</param>
        /// <param name="combinationType">The combination type.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Map<TKey, SetUnit>> Simplify(
            Zen<Map<TKey, SetUnit>> dict1,
            Zen<Map<TKey, SetUnit>> dict2,
            CombineType combinationType)
        {
            // a union a = a
            // a inter a = a
            // a minus a = {}
            if (dict1.Equals(dict2))
            {
                return combinationType == CombineType.Difference ? Zen.EmptyDict<TKey, SetUnit>() : dict1;
            }

            // {} union a == a
            // {} inter a == {}
            // {} minus a == {}
            if (dict1 is ZenDictEmptyExpr<TKey, SetUnit>)
            {
                return combinationType == CombineType.Union ? dict2 : dict1;
            }

            // a union {} == a
            // a inter {} == {}
            // a minus {} == a
            if (dict2 is ZenDictEmptyExpr<TKey, SetUnit>)
            {
                return combinationType == CombineType.Intersect ? dict2 : dict1;
            }

            // (a union b) union b == (a union b)
            // (a union b) union a == (a union b)
            // (a inter b) inter b == (a inter b)
            // (a inter b) inter a == (a inter b)
            if (dict1 is ZenDictCombineExpr<TKey> e1 &&
                e1.CombinationType == combinationType &&
                combinationType != CombineType.Difference &&
                (e1.DictExpr1.Equals(dict2) || e1.DictExpr2.Equals(dict2)))
            {
                return dict1;
            }

            // a union (a union b) == (a union b)
            // b union (a union b) == (a union b)
            // a inter (a inter b) == (a inter b)
            // b inter (a inter b) == (a inter b)
            if (dict2 is ZenDictCombineExpr<TKey> e2 &&
                e2.CombinationType == combinationType &&
                combinationType != CombineType.Difference &&
                (e2.DictExpr1.Equals(dict1) || e2.DictExpr2.Equals(dict1)))
            {
                return dict2;
            }

            // (a minus b) minus a == {}
            // (a minus b) minus b == a minus b
            if (dict1 is ZenDictCombineExpr<TKey> e3 && combinationType == CombineType.Difference)
            {
                if (e3.CombinationType == CombineType.Difference && e3.DictExpr1.Equals(dict2))
                {
                    return Zen.EmptyDict<TKey, SetUnit>();
                }

                if (e3.CombinationType == CombineType.Difference && e3.DictExpr2.Equals(dict2))
                {
                    return dict1;
                }
            }

            return new ZenDictCombineExpr<TKey>(dict1, dict2, combinationType);
        }

        /// <summary>
        /// Create a new ZenDictUnionExpr.
        /// </summary>
        /// <param name="dictExpr1">The first dictionary expr.</param>
        /// <param name="dictExpr2">The second dictionary expr.</param>
        /// <param name="combineType">The combination type.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Map<TKey, SetUnit>> Create(
            Zen<Map<TKey, SetUnit>> dictExpr1,
            Zen<Map<TKey, SetUnit>> dictExpr2,
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
            Zen<Map<TKey, SetUnit>> dictExpr1,
            Zen<Map<TKey, SetUnit>> dictExpr2,
            CombineType combineType)
        {
            this.DictExpr1 = dictExpr1;
            this.DictExpr2 = dictExpr2;
            this.CombinationType = combineType;
        }

        /// <summary>
        /// Gets the first dictionary expr.
        /// </summary>
        public Zen<Map<TKey, SetUnit>> DictExpr1 { get; }

        /// <summary>
        /// Gets the second dictionary expr.
        /// </summary>
        public Zen<Map<TKey, SetUnit>> DictExpr2 { get; }

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
                case CombineType.Difference:
                    return $"Difference({this.DictExpr1}, {this.DictExpr2})";
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
        internal override TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.Visit(this, parameter);
        }

        internal enum CombineType
        {
            Union,
            Intersect,
            Difference,
        }
    }
}
