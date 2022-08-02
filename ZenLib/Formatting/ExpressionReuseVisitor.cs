// <copyright file="ExpressionReuseVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System.Collections.Generic;

    /// <summary>
    /// Class to identify sub-expressions that are repeated more than once.
    /// </summary>
    internal class ExpressionReuseVisitor : IZenExprVisitor<Unit, Unit>
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
            Compute(expression, new Unit());
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

        public Unit Visit(ZenLogicalBinopExpr expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public Unit Visit(ZenNotExpr expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenIfExpr<T> expression, Unit parameter)
        {
            Compute(expression.GuardExpr, parameter);
            Compute(expression.TrueExpr, parameter);
            Compute(expression.FalseExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenConstantExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public Unit Visit<T>(ZenArithBinopExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenBitwiseNotExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenBitwiseBinopExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenListEmptyExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public Unit Visit<TKey, TValue>(ZenMapEmptyExpr<TKey, TValue> expression, Unit parameter)
        {
            return parameter;
        }

        public Unit Visit<T>(ZenListAddFrontExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            Compute(expression.Element, parameter);
            return parameter;
        }

        public Unit Visit<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            Compute(expression.KeyExpr, parameter);
            Compute(expression.ValueExpr, parameter);
            return parameter;
        }

        public Unit Visit<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            Compute(expression.KeyExpr, parameter);
            return parameter;
        }

        public Unit Visit<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.MapExpr, parameter);
            Compute(expression.KeyExpr, parameter);
            return parameter;
        }

        public Unit Visit<TKey>(ZenMapCombineExpr<TKey> expression, Unit parameter)
        {
            Compute(expression.MapExpr1, parameter);
            Compute(expression.MapExpr2, parameter);
            return parameter;
        }

        public Unit Visit<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Unit parameter)
        {
            Compute(expression.ListExpr, parameter);
            Compute(expression.EmptyCase, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqEmptyExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public Unit Visit<T>(ZenSeqUnitExpr<T> expression, Unit parameter)
        {
            Compute(expression.ValueExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqConcatExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr1, parameter);
            Compute(expression.SeqExpr2, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqLengthExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqAtExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.IndexExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqContainsExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.SubseqExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqIndexOfExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.SubseqExpr, parameter);
            Compute(expression.OffsetExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqSliceExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.OffsetExpr, parameter);
            Compute(expression.LengthExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqReplaceFirstExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            Compute(expression.SubseqExpr, parameter);
            Compute(expression.ReplaceExpr, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenSeqRegexExpr<T> expression, Unit parameter)
        {
            Compute(expression.SeqExpr, parameter);
            return parameter;
        }

        public Unit Visit<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            return parameter;
        }

        public Unit Visit<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Unit parameter)
        {
            Compute(expression.Expr, parameter);
            Compute(expression.FieldValue, parameter);
            return parameter;
        }

        public Unit Visit<TObject>(ZenCreateObjectExpr<TObject> expression, Unit parameter)
        {
            foreach (var fieldValuePair in expression.Fields)
            {
                dynamic fieldValue = fieldValuePair.Value;
                Compute(fieldValue, parameter);
            }

            return parameter;
        }

        public Unit Visit<T>(ZenEqualityExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenArithComparisonExpr<T> expression, Unit parameter)
        {
            Compute(expression.Expr1, parameter);
            Compute(expression.Expr2, parameter);
            return parameter;
        }

        public Unit Visit<T>(ZenArgumentExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public Unit Visit<T>(ZenArbitraryExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        public Unit Visit<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, Unit parameter)
        {
            Compute(expression.SourceExpr, parameter);
            return parameter;
        }
    }
}
