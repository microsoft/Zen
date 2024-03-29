﻿// <copyright file="RegexCharacterClassVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A class to compute character equivalence classes.
    /// </summary>
    internal class RegexCharacterClassVisitor<T> : IRegexExprVisitor<T, Unit, Set<CharRange<T>>>
    {
        /// <summary>
        /// Compute the character classes.
        /// </summary>
        /// <param name="regex">The regex.</param>
        /// <returns>The character classes.</returns>
        public Set<CharRange<T>> Compute(Regex<T> regex)
        {
            return regex.Accept(this, Unit.Instance);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Character sets.</returns>
        public Set<CharRange<T>> Visit(RegexBinopExpr<T> expression, Unit parameter)
        {
            if (expression.OpType == RegexBinopExprType.Concatenation && !expression.Expr1.IsNullable())
            {
                return Compute(expression.Expr1);
            }
            else
            {
                return MakeDisjoint(Compute(expression.Expr1), Compute(expression.Expr2));
            }
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Character sets.</returns>
        public Set<CharRange<T>> Visit(RegexEmptyExpr<T> expression, Unit parameter)
        {
            var allRange = new CharRange<T>();
            return new Set<CharRange<T>>().Add(allRange);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Character sets.</returns>
        public Set<CharRange<T>> Visit(RegexEpsilonExpr<T> expression, Unit parameter)
        {
            var allRange = new CharRange<T>();
            return new Set<CharRange<T>>().Add(allRange);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Character sets.</returns>
        public Set<CharRange<T>> Visit(RegexRangeExpr<T> expression, Unit parameter)
        {
            var result = new Set<CharRange<T>>().Add(expression.CharacterRange);
            var complements = expression.CharacterRange.Complement();
            foreach (var complement in complements)
            {
                result = result.Add(complement);
            }

            return result;
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Character sets.</returns>
        public Set<CharRange<T>> Visit(RegexUnopExpr<T> expression, Unit parameter)
        {
            return Compute(expression.Expr);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Character sets.</returns>
        [ExcludeFromCodeCoverage]
        public Set<CharRange<T>> Visit(RegexAnchorExpr<T> expression, Unit parameter)
        {
            throw new ZenUnreachableException();
        }

        /// <summary>
        /// Make two sets disjoint.
        /// </summary>
        /// <param name="set1">The first set of character classes.</param>
        /// <param name="set2">The second set of character classes.</param>
        /// <returns></returns>
        private Set<CharRange<T>> MakeDisjoint(Set<CharRange<T>> set1, Set<CharRange<T>> set2)
        {
            var result = new Set<CharRange<T>>();
            foreach (var item1 in set1.Values.Values.Keys)
            {
                foreach (var item2 in set2.Values.Values.Keys)
                {
                    var inter = item1.Intersect(item2);
                    if (!inter.IsEmpty())
                    {
                        result = result.Add(inter);
                    }
                }
            }

            return result;
        }
    }
}
