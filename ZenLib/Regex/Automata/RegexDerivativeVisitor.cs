// <copyright file="RegexDerivativeVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A class to compute a regex derivative.
    /// </summary>
    internal class RegexDerivativeVisitor<T> : IRegexExprVisitor<T, T, Regex<T>>
    {
        /// <summary>
        /// Visitor to check if a regex is nullable.
        /// </summary>
        private RegexNullableVisitor<T> nullableVisitor = new RegexNullableVisitor<T>();

        /// <summary>
        /// Compute a derivative of a regular expression.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <param name="value">The value for the derivative.</param>
        /// <returns>A derivative as a regex.</returns>
        public Regex<T> Compute(Regex<T> regex, T value)
        {
            return regex.Accept(this, value);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexBinopExpr<T> expression, T parameter)
        {
            var r = expression.Expr1;
            var s = expression.Expr2;
            var dr = Compute(r, parameter);
            var ds = Compute(s, parameter);

            switch (expression.OpType)
            {
                case RegexBinopExprType.Union:
                    return Regex.Union(dr, ds);
                case RegexBinopExprType.Intersection:
                    return Regex.Intersect(dr, ds);
                default:
                    Contract.Assert(expression.OpType == RegexBinopExprType.Concatenation);
                    var left = Regex.Concat(dr, s);
                    var right = Regex.Concat(nullableVisitor.Compute(r), ds);
                    return Regex.Union(left, right);
            }
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexRangeExpr<T> expression, T parameter)
        {
            if (expression.CharacterRange.Contains(parameter))
            {
                return Regex.Epsilon<T>();
            }
            else
            {
                return Regex.Empty<T>();
            }
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexEmptyExpr<T> expression, T parameter)
        {
            return expression;
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexEpsilonExpr<T> expression, T parameter)
        {
            return Regex.Empty<T>();
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexUnopExpr<T> expression, T parameter)
        {
            switch (expression.OpType)
            {
                case RegexUnopExprType.Star:
                    return Regex.Concat(Compute(expression.Expr, parameter), expression);
                default:
                    Contract.Assert(expression.OpType == RegexUnopExprType.Negation);
                    return Regex.Negation(Compute(expression.Expr, parameter));
            }
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        [ExcludeFromCodeCoverage]
        public Regex<T> Visit(RegexAnchorExpr<T> expression, T parameter)
        {
            throw new ZenUnreachableException();
        }
    }
}
