// <copyright file="Z3ExprToObjectConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
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
            return this.solver.Context.MkEmptyRe(parameter);
        }

        public ReExpr Visit(RegexEpsilonExpr<T> expression, Sort parameter)
        {
            var seq = this.solver.Context.MkEmptySeq(parameter);
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
                var low = this.solver.Context.MkUnit(GetConstant(expression.CharacterRange.Low));
                var high = this.solver.Context.MkUnit(GetConstant(expression.CharacterRange.High));
                return this.solver.Context.MkRange(low, high);
            }
        }

        private Expr GetConstant(object obj)
        {
            var type = typeof(T);
            var value = obj.ToString();

            if (type == ReflectionUtilities.ByteType)
                return this.solver.Context.MkBV(value, 8);
            if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
                return this.solver.Context.MkBV(value, 16);
            if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
                return this.solver.Context.MkBV(value, 32);
            if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
                return this.solver.Context.MkBV(value, 64);
            throw new ZenUnreachableException();
        }

        public ReExpr Visit(RegexUnopExpr<T> expression, Sort parameter)
        {
            switch (expression.OpType)
            {
                case RegexUnopExprType.Star:
                    return this.solver.Context.MkStar(expression.Expr.Accept(this, parameter));
                default:
                    throw new ZenException("Regex negation not supported in solver.");
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
