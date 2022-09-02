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
            this.Visit(expression, Unit.Instance);
            return this.reusedSubexpressions;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit Visit<T>(Zen<T> expression, Unit parameter)
        {
            if (this.cache.ContainsKey((expression.Id, Unit.Instance)))
            {
                this.reusedSubexpressions.Add(expression);
            }
            else
            {
                this.cache.Add((expression.Id, Unit.Instance), Unit.Instance);
                expression.Accept(this, parameter);
            }

            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitApply<TSrc, TDst>(ZenApplyExpr<TSrc, TDst> expression, Unit parameter)
        {
            this.Visit(expression.Lambda.Body, parameter);
            this.Visit(expression.ArgumentExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitLogicalBinop(ZenLogicalBinopExpr expression, Unit parameter)
        {
            this.Visit(expression.Expr1, parameter);
            this.Visit(expression.Expr2, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitNot(ZenNotExpr expression, Unit parameter)
        {
            this.Visit(expression.Expr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitIf<T>(ZenIfExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.GuardExpr, parameter);
            this.Visit(expression.TrueExpr, parameter);
            this.Visit(expression.FalseExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitConstant<T>(ZenConstantExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitArithBinop<T>(ZenArithBinopExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.Expr1, parameter);
            this.Visit(expression.Expr2, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.Expr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.Expr1, parameter);
            this.Visit(expression.Expr2, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitListEmpty<T>(ZenFSeqEmptyExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitListAdd<T>(ZenFSeqAddFrontExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.Expr, parameter);
            this.Visit(expression.ElementExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, Unit parameter)
        {
            this.Visit(expression.MapExpr, parameter);
            this.Visit(expression.KeyExpr, parameter);
            this.Visit(expression.ValueExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, Unit parameter)
        {
            this.Visit(expression.MapExpr, parameter);
            this.Visit(expression.KeyExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, Unit parameter)
        {
            this.Visit(expression.MapExpr, parameter);
            this.Visit(expression.KeyExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, Unit parameter)
        {
            this.Visit(expression.MapExpr1, parameter);
            this.Visit(expression.MapExpr2, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, Unit parameter)
        {
            this.Visit(expression.MapExpr, parameter);
            this.Visit(expression.ValueExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, Unit parameter)
        {
            this.Visit(expression.MapExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitListCase<TList, TResult>(ZenFSeqCaseExpr<TList, TResult> expression, Unit parameter)
        {
            this.Visit(expression.ListExpr, parameter);
            this.Visit(expression.EmptyExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.ValueExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr1, parameter);
            this.Visit(expression.SeqExpr2, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqAt<T>(ZenSeqAtExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr, parameter);
            this.Visit(expression.IndexExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr, parameter);
            this.Visit(expression.SubseqExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr, parameter);
            this.Visit(expression.SubseqExpr, parameter);
            this.Visit(expression.OffsetExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr, parameter);
            this.Visit(expression.OffsetExpr, parameter);
            this.Visit(expression.LengthExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr, parameter);
            this.Visit(expression.SubseqExpr, parameter);
            this.Visit(expression.ReplaceExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.SeqExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Unit parameter)
        {
            this.Visit(expression.Expr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Unit parameter)
        {
            this.Visit(expression.Expr, parameter);
            this.Visit(expression.FieldExpr, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, Unit parameter)
        {
            foreach (var fieldValuePair in expression.Fields)
            {
                dynamic fieldValue = fieldValuePair.Value;
                this.Visit(fieldValue, parameter);
            }

            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitEquality<T>(ZenEqualityExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.Expr1, parameter);
            this.Visit(expression.Expr2, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, Unit parameter)
        {
            this.Visit(expression.Expr1, parameter);
            this.Visit(expression.Expr2, parameter);
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitParameter<T>(ZenParameterExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitArbitrary<T>(ZenArbitraryExpr<T> expression, Unit parameter)
        {
            return parameter;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override Unit VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, Unit parameter)
        {
            this.Visit(expression.SourceExpr, parameter);
            return parameter;
        }
    }
}
