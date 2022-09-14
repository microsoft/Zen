// <copyright file="RegexToAutomatonConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A class to check if a regex is nullable.
    /// </summary>
    internal class RegexNullableVisitor<T> : IRegexExprVisitor<T, Unit, Regex<T>>
    {
        /// <summary>
        /// Compute whether a regular expression is nullable.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <returns>A derivative as a regex.</returns>
        public Regex<T> Compute(Regex<T> regex)
        {
            return regex.Accept(this, Unit.Instance);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexBinopExpr<T> expression, Unit parameter)
        {
            var d1 = Compute(expression.Expr1);
            var d2 = Compute(expression.Expr2);
            switch (expression.OpType)
            {
                case RegexBinopExprType.Union:
                    return Regex.Union(d1, d2);
                case RegexBinopExprType.Intersection:
                    return Regex.Intersect(d1, d2);
                default:
                    Contract.Assert(expression.OpType == RegexBinopExprType.Concatenation);
                    return Regex.Intersect(d1, d2);
            }
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexRangeExpr<T> expression, Unit parameter)
        {
            return Regex.Empty<T>();
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexEmptyExpr<T> expression, Unit parameter)
        {
            return Regex.Empty<T>();
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexEpsilonExpr<T> expression, Unit parameter)
        {
            return expression;
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexUnopExpr<T> expression, Unit parameter)
        {
            switch (expression.OpType)
            {
                case RegexUnopExprType.Star:
                    return Regex.Epsilon<T>();
                default:
                    Contract.Assert(expression.OpType == RegexUnopExprType.Negation);
                    return Compute(expression.Expr) is RegexEpsilonExpr<T> ? Regex.Empty<T>() : Regex.Epsilon<T>();
            }
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        [ExcludeFromCodeCoverage]
        public Regex<T> Visit(RegexAnchorExpr<T> expression, Unit parameter)
        {
            throw new ZenUnreachableException();
        }
    }
}
