// <copyright file="ExpressionReuseVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System.Collections.Generic;

    /// <summary>
    /// Class to identify sub-expressions that are repeated more than once.
    /// </summary>
    internal class ExpressionReuseVisitor : ZenExprVisitor<Unit, Unit>
    {
        /// <summary>
        /// The subexpressions in the expression.
        /// </summary>
        private ISet<object> subexpressions = new HashSet<object>();

        /// <summary>
        /// The subexpressions that are reused in this expression.
        /// </summary>
        private ISet<object> reusedSubexpressions = new HashSet<object>();

        /// <summary>
        /// The set of reused subexpressions in this expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <returns>The expression as a formatted string.</returns>
        public ISet<object> GetReusedSubExpressions<T>(Zen<T> expression)
        {
            Compute(expression, Unit.Instance);
            return this.reusedSubexpressions;
        }

        private Unit Compute<T>(Zen<T> expression, Unit parameter)
        {
            if (subexpressions.Contains(expression))
            {
                this.reusedSubexpressions.Add(expression);
            }
            else
            {
                subexpressions.Add(expression);
                expression.Accept(this, parameter);
            }

            return parameter;
        }

        public override Unit VisitLogicalBinop(ZenLogicalBinopExpr expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public override Unit VisitNot(ZenNotExpr expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            return parameter;
        }

        public override Unit VisitIf<T>(ZenIfExpr<T> expression, Unit parameter)
        {
            Compute(expression.GuardExpr, parameter);
            Compute(expression.TrueExpr, parameter);
            Compute(expression.FalseExpr, parameter);
            return parameter;
        }

        public override Unit VisitConstant<T>(ZenConstantExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public override Unit VisitArithBinop<T>(ZenArithBinopExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public override Unit VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            return parameter;
        }

        public override Unit VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public override Unit VisitListEmpty<T>(ZenListEmptyExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public override Unit VisitMapEmpty<TKey, TValue>(ZenMapEmptyExpr<TKey, TValue> expression, Unit parameter)
        {
            return parameter;
        }

        public override Unit VisitListAdd<T>(ZenListAddFrontExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            Compute(expression.ElementExpr, parameter);
            return parameter;
        }

        public override Unit VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            Compute(expression.KeyExpr, parameter);
            Compute(expression.ValueExpr, parameter);
            return parameter;
        }

        public override Unit VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            Compute(expression.KeyExpr, parameter);
            return parameter;
        }

        public override Unit VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            Compute(expression.KeyExpr, parameter);
            return parameter;
        }

        public override Unit VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, Unit parameter)
        {
            Compute(expression.MapExpr1, parameter);
            Compute(expression.MapExpr2, parameter);
            return parameter;
        }

        public override Unit VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            Compute(expression.ValueExpr, parameter);
            return parameter;
        }

        public override Unit VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            return parameter;
        }

        public override Unit VisitListCase<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Unit parameter)
        {
            Compute(expression.ListExpr, parameter);
            Compute(expression.EmptyExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqEmpty<T>(ZenSeqEmptyExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public override Unit VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, Unit parameter)
        {
            Compute(expression.ValueExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr1, parameter);
            Compute(expression.SeqExpr2, parameter);
            return parameter;
        }

        public override Unit VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqAt<T>(ZenSeqAtExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.IndexExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.SubseqExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.SubseqExpr, parameter);
            Compute(expression.OffsetExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.OffsetExpr, parameter);
            Compute(expression.LengthExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.SubseqExpr, parameter);
            Compute(expression.ReplaceExpr, parameter);
            return parameter;
        }

        public override Unit VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            return parameter;
        }

        public override Unit VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            return parameter;
        }

        public override Unit VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            Compute(expression.FieldExpr, parameter);
            return parameter;
        }

        public override Unit VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, Unit parameter)
        {
            foreach (var fieldValuePair in expression.Fields)
            {
                dynamic fieldValue = fieldValuePair.Value;
                Compute(fieldValue, parameter);
            }

            return parameter;
        }

        public override Unit VisitEquality<T>(ZenEqualityExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public override Unit VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public override Unit VisitArgument<T>(ZenArgumentExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public override Unit VisitArbitrary<T>(ZenArbitraryExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public override Unit VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.SourceExpr, parameter);
            return parameter;
        }
    }
}
