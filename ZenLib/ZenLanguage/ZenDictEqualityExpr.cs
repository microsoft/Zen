// <copyright file="ZenDictEqualityExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a dictionary equality expression.
    /// </summary>
    internal sealed class ZenDictEqualityExpr<TKey, TValue> : Zen<bool>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<IDictionary<TKey, TValue>>, Zen<IDictionary<TKey, TValue>>), ZenDictEqualityExpr<TKey, TValue>> createFunc = (v) =>
            new ZenDictEqualityExpr<TKey, TValue>(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenDictAddExpr.
        /// </summary>
        private static HashConsTable<(long, long), ZenDictEqualityExpr<TKey, TValue>> hashConsTable =
            new HashConsTable<(long, long), ZenDictEqualityExpr<TKey, TValue>>();

        /// <summary>
        /// Unroll a ZenDictEqualityExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<bool> Unroll()
        {
            return Create(this.DictExpr1.Unroll(), this.DictExpr2.Unroll());
        }

        /// <summary>
        /// Create a new ZenDictEqualityExpr.
        /// </summary>
        /// <param name="dictExpr1">The dictionary1 expr.</param>
        /// <param name="dictExpr2">The dictionary2 expr.</param>
        /// <returns>The new expr.</returns>
        public static ZenDictEqualityExpr<TKey, TValue> Create(Zen<IDictionary<TKey, TValue>> dictExpr1, Zen<IDictionary<TKey, TValue>> dictExpr2)
        {
            CommonUtilities.ValidateNotNull(dictExpr1);
            CommonUtilities.ValidateNotNull(dictExpr2);
            CommonUtilities.ValidateIsMapElementType(typeof(TKey));
            CommonUtilities.ValidateIsMapElementType(typeof(TValue));

            var k = (dictExpr1.Id, dictExpr2.Id);
            hashConsTable.GetOrAdd(k, (dictExpr1, dictExpr2), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenDictEqualityExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictExpr1">The dictionary1 expression.</param>
        /// <param name="dictExpr2">The dictionary2 expression.</param>
        private ZenDictEqualityExpr(Zen<IDictionary<TKey, TValue>> dictExpr1, Zen<IDictionary<TKey, TValue>> dictExpr2)
        {
            this.DictExpr1 = dictExpr1;
            this.DictExpr2 = dictExpr2;
        }

        /// <summary>
        /// Gets the dictionary expr.
        /// </summary>
        public Zen<IDictionary<TKey, TValue>> DictExpr1 { get; }

        /// <summary>
        /// Gets the key to add the value for.
        /// </summary>
        public Zen<IDictionary<TKey, TValue>> DictExpr2 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Eq({this.DictExpr1}, {this.DictExpr2})";
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
            return visitor.VisitZenDictEqualityExpr(this, parameter);
        }
    }
}
