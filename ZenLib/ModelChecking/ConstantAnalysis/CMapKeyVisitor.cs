// <copyright file="CMapKeyVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to trace the set of constants that can be used as keys for CMap
    /// for each Arbitrary expression.
    /// </summary>
    internal sealed class CMapKeyVisitor : ZenExprActionVisitor
    {
        /// <summary>
        /// Mapping from each CMap type to the set of constants used.
        /// </summary>
        public Dictionary<Type, ISet<object>> Constants;

        /// <summary>
        /// The value visitor.
        /// </summary>
        private CMapValueVisitor valueVisitor;

        /// <summary>
        /// Creates a new instance of the <see cref="CMapKeyVisitor"/> class.
        /// </summary>
        /// <param name="arguments"></param>
        public CMapKeyVisitor(Dictionary<long, object> arguments = null) : base(arguments)
        {
            this.Constants = new Dictionary<Type, ISet<object>>();
            this.valueVisitor = new CMapValueVisitor(this);
        }

        /// <summary>
        /// Computes the variable ordering requirements for the expression.
        /// </summary>
        /// <param name="expr">The Zen expression.</param>
        /// <returns></returns>
        public Dictionary<Type, ISet<object>> Compute<T>(Zen<T> expr)
        {
            this.VisitCached(expr);
            return this.Constants;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of CMap variables.</returns>
        public override void Visit<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression)
        {
            this.VisitCached(expression.MapExpr);
            this.VisitCached(expression.ValueExpr);
            AddConstant<TKey, TValue>(expression.Key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of CMap variables.</returns>
        public override void Visit<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression)
        {
            this.VisitCached(expression.MapExpr);
            AddConstant<TKey, TValue>(expression.Key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of CMap variables.</returns>
        public override void Visit<T>(ZenConstantExpr<T> expression)
        {
            this.valueVisitor.Visit(typeof(T), expression.Value);
        }

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public override void Visit<T>(ZenArbitraryExpr<T> expression)
        {
            this.GetOrCreate(typeof(T));
        }

        /// <summary>
        /// Add a constant to the constants.
        /// </summary>
        /// <param name="constant">The constant.</param>
        private void AddConstant<TKey, TValue>(object constant)
        {
            var type = typeof(CMap<TKey, TValue>);
            var result = this.GetOrCreate(type);
            result.Add(constant);
        }

        /// <summary>
        /// Get or add the constants for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The set of constants.</returns>
        internal ISet<object> GetOrCreate(Type type)
        {
            if (!this.Constants.TryGetValue(type, out var consts))
            {
                consts = new HashSet<object>();
                this.Constants.Add(type, consts);
            }

            return consts;
        }
    }
}
