// <copyright file="RegexRemoveAnchorVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A class to remove anchors from a regular expression.
    /// </summary>
    internal class RegexRemoveAnchorVisitor<T> : IRegexExprVisitor<T, (Regex<T>, Regex<T>), Regex<T>>
    {
        /// <summary>
        /// Remove anchors from a regular expression.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <returns>A derivative as a regex.</returns>
        public Regex<T> Compute(Regex<T> regex)
        {
            return regex.Accept(this, (Regex.All<T>(), Regex.All<T>()));
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        [ExcludeFromCodeCoverage]
        public Regex<T> Visit(RegexEmptyExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return expression;
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexEpsilonExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Concat(parameter.Item1, parameter.Item2);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexAnchorExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Epsilon<T>();
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexRangeExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Concat(parameter.Item1, Regex.Concat(expression, parameter.Item2));
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexUnopExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Concat(parameter.Item1, Regex.Concat(expression, parameter.Item2));
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A regex.</returns>
        public Regex<T> Visit(RegexBinopExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            switch (expression.OpType)
            {
                case RegexBinopExprType.Union:
                    return Regex.Union(expression.Expr1.Accept(this, parameter), expression.Expr2.Accept(this, parameter));
                case RegexBinopExprType.Intersection:
                    return Regex.Concat(parameter.Item1, Regex.Concat(expression, parameter.Item2));
                default:
                    Contract.Assert(expression.OpType == RegexBinopExprType.Concatenation);
                    var e1 = expression.Expr1.Accept(this, (parameter.Item1, Regex.Epsilon<T>()));
                    var e2 = expression.Expr2.Accept(this, (Regex.Epsilon<T>(), parameter.Item2));
                    return Regex.Concat(e1, e2);
            }
        }
    }
}
