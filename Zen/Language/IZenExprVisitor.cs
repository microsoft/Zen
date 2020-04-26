﻿// <copyright file="IZenExprVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
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
        /// Visit an EqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenEqExpr<T>(ZenEqExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BoolConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, TParam parameter);

        /// <summary>
        /// Visit a ByteConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantByteExpr(ZenConstantByteExpr expression, TParam parameter);

        /// <summary>
        /// Visit a UshortConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, TParam parameter);

        /// <summary>
        /// Visit a ShortConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantShortExpr(ZenConstantShortExpr expression, TParam parameter);

        /// <summary>
        /// Visit a UintConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantUintExpr(ZenConstantUintExpr expression, TParam parameter);

        /// <summary>
        /// Visit a IntConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantIntExpr(ZenConstantIntExpr expression, TParam parameter);

        /// <summary>
        /// Visit a UlongConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, TParam parameter);

        /// <summary>
        /// Visit a LongConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenConstantLongExpr(ZenConstantLongExpr expression, TParam parameter);

        /// <summary>
        /// Visit a SumExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenSumExpr<T>(ZenSumExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a MultiplyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenMultiplyExpr<T>(ZenMultiplyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a MinusExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenMinusExpr<T>(ZenMinusExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseAndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenBitwiseAndExpr<T>(ZenBitwiseAndExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseOrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenBitwiseOrExpr<T>(ZenBitwiseOrExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseXorExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenBitwiseXorExpr<T>(ZenBitwiseXorExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a MaxExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenMaxExpr<T>(ZenMaxExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a MinExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenMinExpr<T>(ZenMinExpr<T> expression, TParam parameter);

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
        /// Visit a ListMatchExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenListMatchExpr<TList, TResult>(ZenListMatchExpr<TList, TResult> expression, TParam parameter);

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
        TReturn VisitZenLeqExpr<T>(ZenLeqExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitZenGeqExpr<T>(ZenGeqExpr<T> expression, TParam parameter);

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
