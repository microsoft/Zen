// <copyright file="Z3ExprToObjectConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Z3;

    /// <summary>
    /// Convert a Regex to a Z3 Regex.
    /// </summary>
    internal class Z3RegexConverter<T> : IRegexExprVisitor<T, Sort, ReExpr>
    {
        private SolverZ3 solver;

        public Z3RegexConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        public ReExpr Visit(RegexEmptyExpr<T> expression, Sort parameter)
        {
            var seqSort = SolverZ3.Context.MkSeqSort(parameter);
            var regexSort = SolverZ3.Context.MkReSort(seqSort);
            return SolverZ3.Context.MkEmptyRe(regexSort);
        }

        public ReExpr Visit(RegexEpsilonExpr<T> expression, Sort parameter)
        {
            var seqSort = SolverZ3.Context.MkSeqSort(parameter);
            var seq = SolverZ3.Context.MkEmptySeq(seqSort);
            return SolverZ3.Context.MkToRe(seq);
        }

        public ReExpr Visit(RegexRangeExpr<T> expression, Sort parameter)
        {
            if (expression.CharacterRange.Low.Equals(expression.CharacterRange.High))
            {
                return SolverZ3.Context.MkToRe((SeqExpr)GetSeqConstant(expression.CharacterRange.Low));
            }
            else
            {
                Contract.Assert(typeof(T) == typeof(ZenLib.Char), "Regex range only supported for unicode (char)");
                var charLow = GetSeqConstant(expression.CharacterRange.Low);
                var charHigh = GetSeqConstant(expression.CharacterRange.High);
                return SolverZ3.Context.MkRange((SeqExpr)charLow, (SeqExpr)charHigh);
            }
        }

        private Expr GetSeqConstant(object obj)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.ByteType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 8));
            if (type == ReflectionUtilities.CharType)
                return this.solver.CreateStringConst(((ZenLib.Char)obj).Escape());
            if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 16));
            if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 32));
            if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 64));

            Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
            dynamic value = obj;
            var bits = new bool[value.Size];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[bits.Length - i - 1] = value.GetBit(i);
            }
            return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(bits));
        }

        public ReExpr Visit(RegexUnopExpr<T> expression, Sort parameter)
        {
            var e = expression.Expr.Accept(this, parameter);

            switch (expression.OpType)
            {
                case RegexUnopExprType.Star:
                    return SolverZ3.Context.MkStar(e);
                default:
                    Contract.Assert(expression.OpType == RegexUnopExprType.Negation);
                    return SolverZ3.Context.MkComplement(e);
            }
        }

        public ReExpr Visit(RegexBinopExpr<T> expression, Sort parameter)
        {
            var l = expression.Expr1.Accept(this, parameter);
            var r = expression.Expr2.Accept(this, parameter);

            switch (expression.OpType)
            {
                case RegexBinopExprType.Union:
                    return SolverZ3.Context.MkUnion(l, r);
                case RegexBinopExprType.Intersection:
                    return SolverZ3.Context.MkIntersect(l, r);
                default:
                    Contract.Assert(expression.OpType == RegexBinopExprType.Concatenation);
                    return SolverZ3.Context.MkConcat(l, r);
            }
        }

        [ExcludeFromCodeCoverage]
        public ReExpr Visit(RegexAnchorExpr<T> expression, Sort parameter)
        {
            throw new ZenUnreachableException();
        }
    }
}
