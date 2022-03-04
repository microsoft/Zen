// <copyright file="Z3ExprToObjectConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
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
            var seqSort = this.solver.Context.MkSeqSort(parameter);
            var regexSort = this.solver.Context.MkReSort(seqSort);
            return this.solver.Context.MkEmptyRe(regexSort);
        }

        public ReExpr Visit(RegexEpsilonExpr<T> expression, Sort parameter)
        {
            var seqSort = this.solver.Context.MkSeqSort(parameter);
            var seq = this.solver.Context.MkEmptySeq(seqSort);
            return this.solver.Context.MkToRe(seq);
        }

        public ReExpr Visit(RegexRangeExpr<T> expression, Sort parameter)
        {
            if (expression.CharacterRange.Low.Equals(expression.CharacterRange.High))
            {
                var low = this.solver.Context.MkUnit(GetConstant(expression.CharacterRange.Low));
                return this.solver.Context.MkToRe(low);
            }
            else
            {
                Contract.Assert(typeof(T) == typeof(ZenLib.Char), "Regex range only supported for unicode (char)");
                var charLow = GetConstant(expression.CharacterRange.Low);
                var charHigh = GetConstant(expression.CharacterRange.High);
                var seqLow = this.solver.Context.MkUnit(charLow);
                var seqHigh = this.solver.Context.MkUnit(charHigh);
                return this.solver.Context.MkRange(seqLow, seqHigh);
            }
        }

        private Expr GetConstant(object obj)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.ByteType)
                return this.solver.Context.MkBV(obj.ToString(), 8);
            if (type == ReflectionUtilities.CharType)
                return this.solver.CreateCharConst((ZenLib.Char)obj);
            if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
                return this.solver.Context.MkBV(obj.ToString(), 16);
            if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
                return this.solver.Context.MkBV(obj.ToString(), 32);
            if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
                return this.solver.Context.MkBV(obj.ToString(), 64);

            Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
            dynamic value = obj;
            var bits = new bool[value.Size];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[bits.Length - i - 1] = value.GetBit(i);
            }
            return this.solver.Context.MkBV(bits);
        }

        public ReExpr Visit(RegexUnopExpr<T> expression, Sort parameter)
        {
            var e = expression.Expr.Accept(this, parameter);

            switch (expression.OpType)
            {
                case RegexUnopExprType.Star:
                    return this.solver.Context.MkStar(e);
                default:
                    Contract.Assert(expression.OpType == RegexUnopExprType.Negation);
                    return this.solver.Context.MkComplement(e);
            }
        }

        public ReExpr Visit(RegexBinopExpr<T> expression, Sort parameter)
        {
            var l = expression.Expr1.Accept(this, parameter);
            var r = expression.Expr2.Accept(this, parameter);

            switch (expression.OpType)
            {
                case RegexBinopExprType.Union:
                    return this.solver.Context.MkUnion(l, r);
                case RegexBinopExprType.Intersection:
                    return this.solver.Context.MkIntersect(l, r);
                default:
                    Contract.Assert(expression.OpType == RegexBinopExprType.Concatenation);
                    return this.solver.Context.MkConcat(l, r);
            }
        }
    }
}
