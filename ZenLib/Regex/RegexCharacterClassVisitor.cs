// <copyright file="RegexCharacterClassVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace ZenLib
{
    /// <summary>
    /// A class to compute character equivalence classes.
    /// </summary>
    internal class RegexCharacterClassVisitor<T> : IRegexExprVisitor<T, Unit, Set<CharRange<T>>>
    {
        public Set<CharRange<T>> Compute(Regex<T> regex)
        {
            return regex.Accept(this, Unit.Instance);
        }

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

        public Set<CharRange<T>> Visit(RegexEmptyExpr<T> expression, Unit parameter)
        {
            var allRange = new CharRange<T>();
            return new Set<CharRange<T>>().Add(allRange);
        }

        public Set<CharRange<T>> Visit(RegexEpsilonExpr<T> expression, Unit parameter)
        {
            var allRange = new CharRange<T>();
            return new Set<CharRange<T>>().Add(allRange);
        }

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

        public Set<CharRange<T>> Visit(RegexUnopExpr<T> expression, Unit parameter)
        {
            return Compute(expression.Expr);
        }

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

        [ExcludeFromCodeCoverage]
        public Set<CharRange<T>> Visit(RegexAnchorExpr<T> expression, Unit parameter)
        {
            throw new ZenUnreachableException();
        }
    }
}
