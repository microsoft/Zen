// <copyright file="ConstantMapKeyVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    /// <summary>
    /// Class to trace the set of constants that can be used as keys for ConstMap
    /// for each Arbitrary expression.
    /// </summary>
    internal sealed class ConstantMapKeyVisitor : IZenExprVisitor<Dictionary<long, object>, ImmutableHashSet<object>>
    {
        /// <summary>
        /// Mapping from each arbitrary expression to the set of key constants.
        /// </summary>
        private Dictionary<object, ImmutableHashSet<object>> constants;

        /// <summary>
        /// Expression cache to avoid redundant work.
        /// </summary>
        private Dictionary<object, ImmutableHashSet<object>> cache = new Dictionary<object, ImmutableHashSet<object>>();

        /// <summary>
        /// Computes the variable ordering requirements for the expression.
        /// </summary>
        /// <param name="expr">The Zen expression.</param>
        /// <param name="arguments">The argument to expression mapping.</param>
        /// <returns></returns>
        public Dictionary<object, ImmutableHashSet<object>> Compute<T>(Zen<T> expr, Dictionary<long, object> arguments)
        {
            this.constants = new Dictionary<object, ImmutableHashSet<object>>();
            var _ = Evaluate(expr, arguments);
            return this.constants;
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public ImmutableHashSet<object> Evaluate<T>(Zen<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Accept(this, parameter);
            this.cache[expression] = result;
            return result;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit(ZenLogicalBinopExpr expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.Expr1, parameter);
            var y = Evaluate(expression.Expr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit(ZenNotExpr expression, Dictionary<long, object> parameter)
        {
            return Evaluate(expression.Expr, parameter);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenIfExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.GuardExpr, parameter);
            var y = Evaluate(expression.TrueExpr, parameter);
            var z = Evaluate(expression.FalseExpr, parameter);
            return x.Union(y).Union(z);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenConstantExpr<T> expression, Dictionary<long, object> parameter)
        {
            return ImmutableHashSet<object>.Empty;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenArithBinopExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.Expr1, parameter);
            var y = Evaluate(expression.Expr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenBitwiseNotExpr<T> expression, Dictionary<long, object> parameter)
        {
            return Evaluate(expression.Expr, parameter);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenBitwiseBinopExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.Expr1, parameter);
            var y = Evaluate(expression.Expr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenListEmptyExpr<T> expression, Dictionary<long, object> parameter)
        {
            return ImmutableHashSet<object>.Empty;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenListAddFrontExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.Expr, parameter);
            var y = Evaluate(expression.ElementExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey, TValue>(ZenMapEmptyExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            return ImmutableHashSet<object>.Empty;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.MapExpr, parameter);
            var y = Evaluate(expression.KeyExpr, parameter);
            var z = Evaluate(expression.ValueExpr, parameter);
            return x.Union(y).Union(z);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.MapExpr, parameter);
            var y = Evaluate(expression.KeyExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.MapExpr, parameter);
            var y = Evaluate(expression.KeyExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey>(ZenMapCombineExpr<TKey> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.MapExpr1, parameter);
            var y = Evaluate(expression.MapExpr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.MapExpr, parameter);
            var y = Evaluate(expression.ValueExpr, parameter);
            AddConstants(x, expression.Key);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.MapExpr, parameter);
            AddConstants(x, expression.Key);
            return x;
        }

        private void AddConstants(IEnumerable<object> arbitraryExprs, object constant)
        {
            foreach (var arbitraryExpr in arbitraryExprs)
            {
                if (!this.constants.TryGetValue(arbitraryExpr, out var constants))
                {
                    constants = ImmutableHashSet<object>.Empty;
                }

                this.constants[arbitraryExpr] = constants.Add(constant);
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Dictionary<long, object> parameter)
        {
            // TODO: need to evaluate the cons case as well in case that uses a map get or map set.
            var x = Evaluate(expression.ListExpr, parameter);
            var y = Evaluate(expression.EmptyExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqEmptyExpr<T> expression, Dictionary<long, object> parameter)
        {
            return ImmutableHashSet<object>.Empty;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqUnitExpr<T> expression, Dictionary<long, object> parameter)
        {
            return Evaluate(expression.ValueExpr, parameter);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqConcatExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.SeqExpr1, parameter);
            var y = Evaluate(expression.SeqExpr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqLengthExpr<T> expression, Dictionary<long, object> parameter)
        {
            return Evaluate(expression.SeqExpr, parameter);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqAtExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.SeqExpr, parameter);
            var y = Evaluate(expression.IndexExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqContainsExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.SeqExpr, parameter);
            var y = Evaluate(expression.SubseqExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqIndexOfExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.SeqExpr, parameter);
            var y = Evaluate(expression.SubseqExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqSliceExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.SeqExpr, parameter);
            var y = Evaluate(expression.OffsetExpr, parameter);
            var z = Evaluate(expression.LengthExpr, parameter);
            return x.Union(y).Union(z);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqReplaceFirstExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.SeqExpr, parameter);
            var y = Evaluate(expression.SubseqExpr, parameter);
            var z = Evaluate(expression.ReplaceExpr, parameter);
            return x.Union(y).Union(z);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenSeqRegexExpr<T> expression, Dictionary<long, object> parameter)
        {
            return Evaluate(expression.SeqExpr, parameter);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            return Evaluate(expression.Expr, parameter);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.Expr, parameter);
            var y = Evaluate(expression.FieldExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TObject>(ZenCreateObjectExpr<TObject> expression, Dictionary<long, object> parameter)
        {
            var objects = ImmutableHashSet<object>.Empty;
            foreach (var fieldValuePair in expression.Fields)
            {
                ImmutableHashSet<object> valueResult;
                try
                {
                    var valueType = fieldValuePair.Value.GetType();
                    var innerType = valueType.BaseType.GetGenericArgumentsCached()[0];
                    var evaluateMethod = typeof(ConstantMapKeyVisitor)
                        .GetMethodCached("Evaluate")
                        .MakeGenericMethod(innerType);
                    valueResult = (ImmutableHashSet<object>)evaluateMethod.Invoke(this, new object[] { fieldValuePair.Value, parameter });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }

                objects = objects.Union(valueResult);
            }

            return objects;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenEqualityExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.Expr1, parameter);
            var y = Evaluate(expression.Expr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenArithComparisonExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = Evaluate(expression.Expr1, parameter);
            var y = Evaluate(expression.Expr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenArgumentExpr<T> expression, Dictionary<long, object> parameter)
        {
            try
            {
                var expr = parameter[expression.ArgumentId];
                var type = expr.GetType().BaseType.GetGenericArgumentsCached()[0];
                var evaluateMethod = typeof(ConstantMapKeyVisitor)
                    .GetMethodCached("Evaluate")
                    .MakeGenericMethod(type);
                return (ImmutableHashSet<object>)evaluateMethod.Invoke(this, new object[] { expr, parameter });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<T>(ZenArbitraryExpr<T> expression, Dictionary<long, object> parameter)
        {
            var type = typeof(T);

            if (type.IsGenericType && type.GetGenericTypeDefinitionCached() == typeof(ConstMap<,>))
            {
                return ImmutableHashSet<object>.Empty.Add(expression);
            }

            return ImmutableHashSet<object>.Empty;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public ImmutableHashSet<object> Visit<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            return Evaluate(expression.SourceExpr, parameter);
        }
    }
}
