// <copyright file="ZenExprVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Visitor interface for ZenExpr.
    /// </summary>
    /// <typeparam name="TParam">The parameter type.</typeparam>
    /// <typeparam name="TReturn">The return type.</typeparam>
    internal abstract class ZenExprVisitor<TParam, TReturn>
    {
        /// <summary>
        /// The cache of results.
        /// </summary>
        protected IDictionary<(long, TParam), TReturn> cache = new Dictionary<(long, TParam), TReturn>();

        /// <summary>
        /// Visit a LogicalBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitLogicalBinop(ZenLogicalBinopExpr expression, TParam parameter);

        /// <summary>
        /// Visit a NotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitNot(ZenNotExpr expression, TParam parameter);

        /// <summary>
        /// Visit an IfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitIf<T>(ZenIfExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitConstant<T>(ZenConstantExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ArithBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitArithBinop<T>(ZenArithBinopExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ListEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitListEmpty<T>(ZenFSeqEmptyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ListAddFrontExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitListAdd<T>(ZenFSeqAddFrontExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a MapEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitMapEmpty<TKey, TValue>(ZenMapEmptyExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a MapSetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a MapDeleteExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a MapGetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a MapCombineExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, TParam parameter);

        /// <summary>
        /// Visit a ConstMapSetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a ConstMapGetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a ListCaseExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitListCase<TList, TResult>(ZenFSeqCaseExpr<TList, TResult> expression, TParam parameter);

        /// <summary>
        /// Visit a SeqEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqEmpty<T>(ZenSeqEmptyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqUnitExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqConcatExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqLengthExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqAtExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqAt<T>(ZenSeqAtExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqContainsExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqIndexOfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqSliceExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqReplaceFirstExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqRegexExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a GetFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, TParam parameter);

        /// <summary>
        /// Visit a WithFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, TParam parameter);

        /// <summary>
        /// Visit a CreateObjectExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, TParam parameter);

        /// <summary>
        /// Visit a EqualityExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitEquality<T>(ZenEqualityExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit an ArgumentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitArgument<T>(ZenArgumentExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitArbitrary<T>(ZenArbitraryExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a CastExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public abstract TReturn VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        public virtual TReturn Visit<T>(Zen<T> expression, TParam parameter)
        {
            var key = (expression.Id, parameter);
            if (this.cache.TryGetValue(key, out var result))
            {
                return result;
            }

            result = expression.Accept(this, parameter);
            this.cache[key] = result;
            return result;
        }
    }
}
