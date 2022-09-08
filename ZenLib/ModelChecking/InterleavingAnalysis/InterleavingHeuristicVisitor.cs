// <copyright file="InterleavingHeuristicVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Class to conservatively estimate which variables
    /// must be interleaved to avoid exponential blowup in the BDD encoding.
    /// </summary>
    internal sealed class InterleavingHeuristicVisitor : ZenExprVisitor<ImmutableDictionary<long, object>, InterleavingResult>
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
        public List<List<object>> GetInterleavedVariables<T>(Zen<T> expr, ImmutableDictionary<long, object> arguments)
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
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitApply<TSrc, TDst>(ZenApplyExpr<TSrc, TDst> expression, ImmutableDictionary<long, object> parameter)
        {
            throw new ZenException($"FSeq type not supported in DD backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitLogicalBinop(ZenLogicalBinopExpr expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitNot(ZenNotExpr expression, ImmutableDictionary<long, object> parameter)
        {
            return this.Visit(expression.Expr, parameter);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitIf<T>(ZenIfExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitConstant<T>(ZenConstantExpr<T> expression, ImmutableDictionary<long, object> parameter)
        {
            return emptySetVisitor.Visit(typeof(T), Unit.Instance);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitArithBinop<T>(ZenArithBinopExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, ImmutableDictionary<long, object> parameter)
        {
            return this.Visit(expression.Expr, parameter);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitListAdd<T>(ZenFSeqAddFrontExpr<T> expression, ImmutableDictionary<long, object> parameter)
        {
            var x = this.Visit(expression.ElementExpr, parameter);
            var y = this.Visit(expression.ListExpr, parameter);
            return x.Union(y);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>\
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitListCase<TList, TResult>(ZenFSeqCaseExpr<TList, TResult> expression, ImmutableDictionary<long, object> parameter)
        {
            throw new ZenException($"FSeq type not supported in DD backend.");
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, ImmutableDictionary<long, object> parameter)
        {
            return this.Visit(expression.SourceExpr, parameter);
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The interleaving result.</returns>
        public override InterleavingResult VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitEquality<T>(ZenEqualityExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitParameter<T>(ZenParameterExpr<T> expression, ImmutableDictionary<long, object> parameter)
        {
            try
            {
                var expr = parameter[expression.ParameterId];
                var evaluateMethod = typeof(InterleavingHeuristicVisitor)
                    .GetMethodCached("Visit")
                    .MakeGenericMethod(typeof(T));
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
        public override InterleavingResult VisitArbitrary<T>(ZenArbitraryExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqAt<T>(ZenSeqAtExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqNth<T>(ZenSeqNthExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, ImmutableDictionary<long, object> parameter)
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
        public override InterleavingResult VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, ImmutableDictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid sequence type used with Decision Diagram backend.");
        }
    }
}
