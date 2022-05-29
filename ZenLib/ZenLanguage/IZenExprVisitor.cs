// <copyright file="IZenExprVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    /// <summary>
    /// Visitor interface for ZenExpr.
    /// </summary>
    /// <typeparam name="TParam">The parameter type.</typeparam>
    /// <typeparam name="TReturn">The return type.</typeparam>
    internal interface IZenExprVisitor<TParam, TReturn>
    {
        /// <summary>
        /// Visit an AndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(ZenLogicalBinopExpr expression, TParam parameter);

        /// <summary>
        /// Visit a NotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(ZenNotExpr expression, TParam parameter);

        /// <summary>
        /// Visit an IfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenIfExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenConstantExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ArithBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenArithBinopExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenBitwiseNotExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenBitwiseBinopExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ListEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenListEmptyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a DictEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a ListAddFrontExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenListAddFrontExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a DictSetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a DictDeleteExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a DictGetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, TParam parameter);

        /// <summary>
        /// Visit a DictCombineExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TKey>(ZenDictCombineExpr<TKey> expression, TParam parameter);

        /// <summary>
        /// Visit a ListCaseExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, TParam parameter);

        /// <summary>
        /// Visit a SeqEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqEmptyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqUnitExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqUnitExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqConcatExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqConcatExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqLengthExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqLengthExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqAtExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqAtExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqContainsExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqContainsExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqIndexOfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqIndexOfExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqSliceExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqSliceExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqReplaceFirstExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqReplaceFirstExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ZenSeqRegexExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenSeqRegexExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a GetFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T1, T2>(ZenGetFieldExpr<T1, T2> expression, TParam parameter);

        /// <summary>
        /// Visit a WithFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T1, T2>(ZenWithFieldExpr<T1, T2> expression, TParam parameter);

        /// <summary>
        /// Visit a CreateObjectExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TObject>(ZenCreateObjectExpr<TObject> expression, TParam parameter);

        /// <summary>
        /// Visit a EqualityExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenEqualityExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenArithComparisonExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit an ArgumentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenArgumentExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<T>(ZenArbitraryExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a CastExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, TParam parameter);
    }
}
