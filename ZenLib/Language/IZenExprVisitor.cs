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
        /// Visit a AdapterExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenAdapterExpr<T1, T2>(ZenAdapterExpr<T1, T2> expression, TParam parameter);

        /// <summary>
        /// Visit an AndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenAndExpr(ZenAndExpr expression, TParam parameter);

        /// <summary>
        /// Visit an OrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenOrExpr(ZenOrExpr expression, TParam parameter);

        /// <summary>
        /// Visit a NotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenNotExpr(ZenNotExpr expression, TParam parameter);

        /// <summary>
        /// Visit an IfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenIfExpr<T>(ZenIfExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ConcatExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConcatExpr(ZenConcatExpr expression, TParam parameter);

        /// <summary>
        /// Visit a ContainmentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, TParam parameter);

        /// <summary>
        /// Visit a StringReplaceExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, TParam parameter);

        /// <summary>
        /// Visit a StringSubstringExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, TParam parameter);

        /// <summary>
        /// Visit a StringLengthExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenStringLengthExpr(ZenStringLengthExpr expression, TParam parameter);

        /// <summary>
        /// Visit a StringAtExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenStringAtExpr(ZenStringAtExpr expression, TParam parameter);

        /// <summary>
        /// Visit a StringIndexOfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseAndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit an EmptyListExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ListAddFrontExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a ListCaseExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, TParam parameter);

        /// <summary>
        /// Visit a GetFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, TParam parameter);

        /// <summary>
        /// Visit a WithFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, TParam parameter);

        /// <summary>
        /// Visit a CreateObjectExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, TParam parameter);

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenComparisonExpr<T>(ZenComparisonExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit an ArgumentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, TParam parameter);
    }
}
