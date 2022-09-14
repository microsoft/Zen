// <copyright file="IRegexExprVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    /// <summary>
    /// Visitor interface for Regex.
    /// </summary>
    /// <typeparam name="T">The regex character type.</typeparam>
    /// <typeparam name="TParam">The parameter type.</typeparam>
    /// <typeparam name="TReturn">The return type.</typeparam>
    internal interface IRegexExprVisitor<T, TParam, TReturn>
    {
        /// <summary>
        /// Visit a RegexEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(RegexEmptyExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexEpsilonExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(RegexEpsilonExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexAnchorExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(RegexAnchorExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexCharExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(RegexRangeExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexUnopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(RegexUnopExpr<T> expression, TParam parameter);

        /// <summary>
        /// Visit a RegexBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A return value.</returns>
        TReturn Visit(RegexBinopExpr<T> expression, TParam parameter);
    }
}
