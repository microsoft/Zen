// <copyright file="InterleavingHeuristicVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class to conservatively estimate which variables
    /// must be interleaved to avoid exponential blowup in the BDD encoding.
    /// </summary>
    internal sealed class InterleavingHeuristicVisitor : ZenExprVisitor<Dictionary<long, object>, InterleavingResult>
    {
        /// <summary>
        /// Set of disjoint variable sets to track must-interleave dependencies, implemented as a union find.
        /// </summary>
        public UnionFind<object> DisjointSets { get; } = new UnionFind<object>();

        /// <summary>
        /// The empty set visitor.
        /// </summary>
        private InterleavingSetEmptyVisitor emptySetVisitor = new InterleavingSetEmptyVisitor();

        /// <summary>
        /// Computes the variable ordering requirements for the expression.
        /// </summary>
        /// <param name="expr">The Zen expression.</param>
        /// <param name="arguments">The argument to expression mapping.</param>
        /// <returns></returns>
        public List<List<object>> GetInterleavedVariables<T>(Zen<T> expr, Dictionary<long, object> arguments)
        {
            var _ = this.Visit(expr, arguments);
            return this.DisjointSets.GetDisjointSets();
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitLogicalBinop(ZenLogicalBinopExpr expression, Dictionary<long, object> parameter)
        {
            var x = this.Visit(expression.Expr1, parameter);
            var y = this.Visit(expression.Expr2, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitNot(ZenNotExpr expression, Dictionary<long, object> parameter)
        {
            return this.Visit(expression.Expr, parameter);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitIf<T>(ZenIfExpr<T> expression, Dictionary<long, object> parameter)
        {
            this.Visit(expression.GuardExpr, parameter);
            var t = this.Visit(expression.TrueExpr, parameter);
            var f = this.Visit(expression.FalseExpr, parameter);
            return t.Union(f);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitConstant<T>(ZenConstantExpr<T> expression, Dictionary<long, object> parameter)
        {
            return emptySetVisitor.Visit(typeof(T), Unit.Instance);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitArithBinop<T>(ZenArithBinopExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = this.Visit(expression.Expr1, parameter);
            var y = this.Visit(expression.Expr2, parameter);
            x.Combine(y, this.DisjointSets);
            return x.Union(y);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = this.Visit(expression.Expr1, parameter);
            var y = this.Visit(expression.Expr2, parameter);

            switch (expression.Operation)
            {
                case BitwiseOp.BitwiseAnd:
                case BitwiseOp.BitwiseXor:
                    x.Combine(y, this.DisjointSets);
                    return x.Union(y);
                default:
                    Contract.Assert(expression.Operation == BitwiseOp.BitwiseOr);
                    return x.Union(y);
            }
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, Dictionary<long, object> parameter)
        {
            return this.Visit(expression.Expr, parameter);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitListEmpty<T>(ZenListEmptyExpr<T> expression, Dictionary<long, object> parameter)
        {
            return emptySetVisitor.Visit(typeof(Option<T>), Unit.Instance);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitListAdd<T>(ZenListAddFrontExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = this.Visit(expression.ElementExpr, parameter);
            var y = this.Visit(expression.Expr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            return this.Visit(expression.SourceExpr, parameter);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitListCase<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Dictionary<long, object> parameter)
        {
            var _ = this.Visit(expression.ListExpr, parameter);
            var e = this.Visit(expression.EmptyExpr, parameter);
            return e; // no easy way to evaluate cons case.
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            var result = (InterleavingClass)this.Visit(expression.Expr, parameter);
            return result.Fields[expression.FieldName];
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            var x = (InterleavingClass)this.Visit(expression.Expr, parameter);
            var y = this.Visit(expression.FieldExpr, parameter);
            return new InterleavingClass(x.Fields.SetItem(expression.FieldName, y));
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, Dictionary<long, object> parameter)
        {
            var fieldMapping = ImmutableDictionary<string, InterleavingResult>.Empty;
            foreach (var fieldValuePair in expression.Fields)
            {
                InterleavingResult valueResult;
                try
                {
                    var valueType = fieldValuePair.Value.GetType();
                    var innerType = valueType.BaseType.GetGenericArgumentsCached()[0];
                    var evaluateMethod = typeof(InterleavingHeuristicVisitor)
                        .GetMethodCached("Visit")
                        .MakeGenericMethod(innerType);
                    valueResult = (InterleavingResult)evaluateMethod.Invoke(this, new object[] { fieldValuePair.Value, parameter });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }

                fieldMapping = fieldMapping.Add(fieldValuePair.Key, valueResult);
            }

            return new InterleavingClass(fieldMapping);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitEquality<T>(ZenEqualityExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = this.Visit(expression.Expr1, parameter);
            var y = this.Visit(expression.Expr2, parameter);
            x.Combine(y, this.DisjointSets);
            return new InterleavingSet(x.GetAllVariables().Union(y.GetAllVariables()));
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, Dictionary<long, object> parameter)
        {
            var x = this.Visit(expression.Expr1, parameter);
            var y = this.Visit(expression.Expr2, parameter);
            x.Combine(y, this.DisjointSets);
            return x.Union(y);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitArgument<T>(ZenArgumentExpr<T> expression, Dictionary<long, object> parameter)
        {
            try
            {
                var expr = parameter[expression.ArgumentId];
                var type = expr.GetType().BaseType.GetGenericArgumentsCached()[0];
                var evaluateMethod = typeof(InterleavingHeuristicVisitor)
                    .GetMethodCached("Visit")
                    .MakeGenericMethod(type);
                return (InterleavingResult)evaluateMethod.Invoke(this, new object[] { expr, parameter });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitArbitrary<T>(ZenArbitraryExpr<T> expression, Dictionary<long, object> parameter)
        {
            this.DisjointSets.Add(expression);
            return new InterleavingSet(ImmutableHashSet<object>.Empty.Add(expression));
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitMapEmpty<TKey, TValue>(ZenMapEmptyExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid map type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid map type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid map type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid map type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid map type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid map type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid map type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqEmpty<T>(ZenSeqEmptyExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqAt<T>(ZenSeqAtExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }
    }
}
