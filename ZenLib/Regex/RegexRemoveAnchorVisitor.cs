// <copyright file="RegexRemoveAnchorVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace ZenLib
{
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

        public Regex<T> Visit(RegexEmptyExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return expression;
        }

        public Regex<T> Visit(RegexEpsilonExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Concat(parameter.Item1, parameter.Item2);
        }

        public Regex<T> Visit(RegexAnchorExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Epsilon<T>();
        }

        public Regex<T> Visit(RegexRangeExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Concat(parameter.Item1, Regex.Concat(expression, parameter.Item2));
        }

        public Regex<T> Visit(RegexUnopExpr<T> expression, (Regex<T>, Regex<T>) parameter)
        {
            return Regex.Concat(parameter.Item1, Regex.Concat(expression, parameter.Item2));
        }

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
                    if (parameter.Item1.Equals(Regex.All<T>()) && parameter.Item2.Equals(Regex.All<T>()))
                    {
                        var param1 = (Regex.All<T>(), Regex.Epsilon<T>());
                        var param2 = (Regex.Epsilon<T>(), Regex.All<T>());
                        return Regex.Concat(expression.Expr1.Accept(this, param1), expression.Expr2.Accept(this, param2));
                    }
                    else
                    {
                        Contract.Assert(parameter.Item1.Equals(Regex.Epsilon<T>()));
                        Contract.Assert(parameter.Item2.Equals(Regex.All<T>()));
                        return Regex.Concat(expression.Expr1, expression.Expr2.Accept(this, parameter));
                    }
            }
        }
    }
}
