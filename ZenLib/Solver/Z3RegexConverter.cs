﻿// <copyright file="Z3ExprToObjectConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Z3;

    /// <summary>
    /// Convert a Regex to a Z3 Regex.
    /// </summary>
    internal class Z3RegexConverter<T> : IRegexExprVisitor<T, Sort, ReExpr>
    {
        /// <summary>
        /// The Z3 solver.
        /// </summary>
        private SolverZ3 solver;

        /// <summary>
        /// Creates a new instance of the <see cref="Z3RegexConverter{T}"/> class.
        /// </summary>
        /// <param name="solver"></param>
        public Z3RegexConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The regex expression.</param>
        /// <param name="parameter">The Z3 sort.</param>
        /// <returns>A Z3 regex expression.</returns>
        public ReExpr Visit(RegexEmptyExpr<T> expression, Sort parameter)
        {
            var seqSort = SolverZ3.Context.MkSeqSort(parameter);
            var regexSort = SolverZ3.Context.MkReSort(seqSort);
            return SolverZ3.Context.MkEmptyRe(regexSort);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The regex expression.</param>
        /// <param name="parameter">The Z3 sort.</param>
        /// <returns>A Z3 regex expression.</returns>
        public ReExpr Visit(RegexEpsilonExpr<T> expression, Sort parameter)
        {
            var seqSort = SolverZ3.Context.MkSeqSort(parameter);
            var seq = SolverZ3.Context.MkEmptySeq(seqSort);
            return SolverZ3.Context.MkToRe(seq);
        }

        /// <summary>
        /// Visit a regex.
        /// </summary>
        /// <param name="expression">The regex expression.</param>
        /// <param name="parameter">The Z3 sort.</param>
        /// <returns>A Z3 regex expression.</returns>
        public ReExpr Visit(RegexRangeExpr<T> expression, Sort parameter)
        {
            if (expression.CharacterRange.Low.Equals(expression.CharacterRange.High))
            {
                return SolverZ3.Context.MkToRe((SeqExpr)GetSeqConstant(expression.CharacterRange.Low));
            }
            else
            {
                Contract.Assert(
                    typeof(T) == typeof(byte) ||
                    typeof(T) == typeof(ushort) ||
                    typeof(T) == typeof(uint) ||
                    typeof(T) == typeof(ulong) ||
                    typeof(T) == typeof(char) ||
                    (ReflectionUtilities.IsFixedIntegerType(typeof(T)) && typeof(T).BaseType.GetGenericArgumentsCached()[1] == typeof(Unsigned)),
                    "Regex range only supports unsigned integer types and char.");

                var charLow = GetSeqConstant(expression.CharacterRange.Low);
                var charHigh = GetSeqConstant(expression.CharacterRange.High);
                return SolverZ3.Context.MkRange((SeqExpr)charLow, (SeqExpr)charHigh);
            }
        }

        /// <summary>
        /// Get a sequence constant.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A singleton sequence.</returns>
        private Expr GetSeqConstant(object obj)
        {
            var type = typeof(T);

            // convert primitive types.
            if (type == ReflectionUtilities.ByteType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 8));
            if (type == ReflectionUtilities.CharType)
                return this.solver.CreateStringConst(CommonUtilities.EscapeChar((char)obj));
            if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 16));
            if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 32));
            if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
                return SolverZ3.Context.MkUnit(SolverZ3.Context.MkBV(obj.ToString(), 64));

            // convert Int<N> and UInt<N>
            Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
            var size = ReflectionUtilities.IntegerSize(type);
            var bits = new bool[size];
            dynamic value = obj;
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
