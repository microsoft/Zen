// <copyright file="RegexToAutomatonConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// A class to compute a regex derivative.
    /// </summary>
    internal class RegexDerivativeVisitor<T> : IRegexExprVisitor<T, T, Regex<T>>
        where T : IComparable<T>
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

        public Regex<T> VisitRegexAllExpr(RegexAllExpr<T> expression, T parameter)
        {
            throw new NotImplementedException();
        }

        public Regex<T> VisitRegexBinopExpr(RegexBinopExpr<T> expression, T parameter)
        {
            var d1 = Compute(expression.Expr1, parameter);
            var d2 = Compute(expression.Expr2, parameter);
            switch (expression.OpType)
            {
                case RegexBinopExprType.Union:
                    return Regex.Union(d1, d2);
                case RegexBinopExprType.Intersection:
                    return Regex.Intersect(d1, d2);
                default:
                    Contract.Assert(expression.OpType == RegexBinopExprType.Concatenation);
                    return Regex.Union(Regex.Concat(d1, expression.Expr2), Regex.Concat(nullableVisitor.Compute(expression.Expr1), d2));
            }
        }

        public Regex<T> VisitRegexRangeExpr(RegexRangeExpr<T> expression, T parameter)
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

        public Regex<T> VisitRegexEmptyExpr(RegexEmptyExpr<T> expression, T parameter)
        {
            return expression;
        }

        public Regex<T> VisitRegexEpsilonExpr(RegexEpsilonExpr<T> expression, T parameter)
        {
            return Regex.Empty<T>();
        }

        public Regex<T> VisitRegexUnopExpr(RegexUnopExpr<T> expression, T parameter)
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
    }
}
