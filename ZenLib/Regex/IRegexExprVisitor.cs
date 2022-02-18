// <copyright file="IRegexExprVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace ZenLib
{
    /// <summary>
    /// Visitor interface for Regex.
    /// </summary>
    /// <typeparam name="T">The regex character type.</typeparam>
    /// <typeparam name="TParam">The parameter type.</typeparam>
    /// <typeparam name="TReturn">The return type.</typeparam>
    internal interface IRegexExprVisitor<T, TParam, TReturn>
        where T : IComparable<T>
    {
        /// <summary>
        /// Visit a RegexEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitRegexEmptyExpr(RegexEmptyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexAllExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitRegexAllExpr(RegexAllExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexEpsilonExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitRegexEpsilonExpr(RegexEpsilonExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexCharExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitRegexRangeExpr(RegexRangeExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexUnopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitRegexUnopExpr(RegexUnopExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn VisitRegexBinopExpr(RegexBinopExpr<T> expression, TParam parameter);
    }
}
