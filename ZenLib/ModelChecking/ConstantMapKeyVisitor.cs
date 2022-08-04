// <copyright file="ConstantMapKeyVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to trace the set of constants that can be used as keys for ConstMap
    /// for each Arbitrary expression.
    /// </summary>
    internal sealed class ConstantMapKeyVisitor : ZenExprActionVisitor
    {
        /// <summary>
        /// Mapping from each ConstMap type to the set of constants used.
        /// </summary>
        private Dictionary<Type, ISet<object>> constants;

        /// <summary>
        /// Creates a new instance of the <see cref="ConstantMapKeyVisitor"/> class.
        /// </summary>
        /// <param name="arguments"></param>
        public ConstantMapKeyVisitor(Dictionary<long, object> arguments = null) : base(arguments)
        {
            this.constants = new Dictionary<Type, ISet<object>>();
        }

        /// <summary>
        /// Computes the variable ordering requirements for the expression.
        /// </summary>
        /// <param name="expr">The Zen expression.</param>
        /// <returns></returns>
        public Dictionary<Type, ISet<object>> Compute<T>(Zen<T> expr)
        {
            this.VisitCached(expr);
            return this.constants;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of ConstMap variables.</returns>
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
        /// <returns>The set of ConstMap variables.</returns>
        public override void Visit<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression)
        {
            this.VisitCached(expression.MapExpr);
            AddConstant<TKey, TValue>(expression.Key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public override void Visit<T>(ZenConstantExpr<T> expression)
        {
            var type = typeof(T);

            if (type.IsGenericType && type.GetGenericTypeDefinitionCached() == typeof(ConstMap<,>))
            {
                if (!this.constants.TryGetValue(type, out var consts))
                {
                    consts = new HashSet<object>();
                    this.constants.Add(type, consts);
                }

                dynamic value = expression.Value;
                foreach (var kv in value.Values)
                {
                    consts.Add(kv.Key);
                }
            }
        }

        /// <summary>
        /// Add a constant to the constants.
        /// </summary>
        /// <param name="constant">The constant.</param>
        private void AddConstant<TKey, TValue>(object constant)
        {
            var type = typeof(ConstMap<TKey, TValue>);
            if (!this.constants.TryGetValue(type, out var result))
            {
                result = new HashSet<object>();
                this.constants[type] = result;
            }

            result.Add(constant);
        }
    }
}
