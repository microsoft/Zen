// <copyright file="RegexToAutomatonConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// A class to check if a regex is nullable.
    /// </summary>
    internal class RegexNullableVisitor<T> : IRegexExprVisitor<T, Unit, Regex<T>>
        where T : IComparable<T>
    {
        /// <summary>
        /// Compute whether a regular expression is nullable.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <returns>A derivative as a regex.</returns>
        public Regex<T> Compute(Regex<T> regex)
        {
            return regex.Accept(this, new Unit());
        }

        public Regex<T> VisitRegexAllExpr(RegexAllExpr<T> expression, Unit parameter)
        {
            throw new NotImplementedException();
        }

        public Regex<T> VisitRegexBinopExpr(RegexBinopExpr<T> expression, Unit parameter)
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

        public Regex<T> VisitRegexRangeExpr(RegexRangeExpr<T> expression, Unit parameter)
        {
            return Regex.Empty<T>();
        }

        public Regex<T> VisitRegexEmptyExpr(RegexEmptyExpr<T> expression, Unit parameter)
        {
            return Regex.Empty<T>();
        }

        public Regex<T> VisitRegexEpsilonExpr(RegexEpsilonExpr<T> expression, Unit parameter)
        {
            return expression;
        }

        public Regex<T> VisitRegexUnopExpr(RegexUnopExpr<T> expression, Unit parameter)
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
    }
}
