// <copyright file="IZenExprVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
{
    using System.Collections.Generic;

    /// <summary>
    /// Transformer interface for ZenExpr.
    /// </summary>
    internal interface IZenExprTransformer
    {
        /// <summary>
        /// Visit a AdapterExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T1> VisitZenAdapterExpr<T1, T2>(ZenAdapterExpr<T1, T2> expression);

        /// <summary>
        /// Visit an AndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<bool> VisitZenAndExpr(ZenAndExpr expression);

        /// <summary>
        /// Visit an OrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<bool> VisitZenOrExpr(ZenOrExpr expression);

        /// <summary>
        /// Visit a NotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<bool> VisitZenNotExpr(ZenNotExpr expression);

        /// <summary>
        /// Visit an IfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenIfExpr<T>(ZenIfExpr<T> expression);

        /// <summary>
        /// Visit an EqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<bool> VisitZenEqExpr<T>(ZenEqExpr<T> expression);

        /// <summary>
        /// Visit a BoolConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<bool> VisitZenConstantBoolExpr(ZenConstantBoolExpr expression);

        /// <summary>
        /// Visit a ByteConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<byte> VisitZenConstantByteExpr(ZenConstantByteExpr expression);

        /// <summary>
        /// Visit a UshortConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<ushort> VisitZenConstantUshortExpr(ZenConstantUshortExpr expression);

        /// <summary>
        /// Visit a ShortConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<short> VisitZenConstantShortExpr(ZenConstantShortExpr expression);

        /// <summary>
        /// Visit a UintConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<uint> VisitZenConstantUintExpr(ZenConstantUintExpr expression);

        /// <summary>
        /// Visit a IntConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<int> VisitZenConstantIntExpr(ZenConstantIntExpr expression);

        /// <summary>
        /// Visit a UlongConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<ulong> VisitZenConstantUlongExpr(ZenConstantUlongExpr expression);

        /// <summary>
        /// Visit a LongConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<long> VisitZenConstantLongExpr(ZenConstantLongExpr expression);

        /// <summary>
        /// Visit a SumExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenSumExpr<T>(ZenSumExpr<T> expression);

        /// <summary>
        /// Visit a MultiplyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenMultiplyExpr<T>(ZenMultiplyExpr<T> expression);

        /// <summary>
        /// Visit a MinusExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenMinusExpr<T>(ZenMinusExpr<T> expression);

        /// <summary>
        /// Visit a BitwiseAndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenBitwiseAndExpr<T>(ZenBitwiseAndExpr<T> expression);

        /// <summary>
        /// Visit a BitwiseOrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenBitwiseOrExpr<T>(ZenBitwiseOrExpr<T> expression);

        /// <summary>
        /// Visit a BitwiseXorExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenBitwiseXorExpr<T>(ZenBitwiseXorExpr<T> expression);

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression);

        /// <summary>
        /// Visit a MaxExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenMaxExpr<T>(ZenMaxExpr<T> expression);

        /// <summary>
        /// Visit a MinExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenMinExpr<T>(ZenMinExpr<T> expression);

        /// <summary>
        /// Visit an EmptyListExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<IList<T>> VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression);

        /// <summary>
        /// Visit a ListAddFrontExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<IList<T>> VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression);

        /// <summary>
        /// Visit a ListCaseExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<TResult> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression);

        /// <summary>
        /// Visit a GetFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T2> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression);

        /// <summary>
        /// Visit a WithFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T1> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression);

        /// <summary>
        /// Visit a CreateObjectExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<TObject> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression);

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<bool> VisitZenLeqExpr<T>(ZenLeqExpr<T> expression);

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<bool> VisitZenGeqExpr<T>(ZenGeqExpr<T> expression);

        /// <summary>
        /// Visit an ArgumentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression);

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        Zen<T> VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression);
    }
}
