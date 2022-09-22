// <copyright file="ZenExprActionVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Visitor interface for ZenExpr that performs side effects.
    /// </summary>
    [ExcludeFromCodeCoverage] // methods overridden
    internal abstract class ZenExprActionVisitor
    {
        /// <summary>
        /// The visited nodes.
        /// </summary>
        internal ISet<long> Visited = new HashSet<long>();

        /// <summary>
        /// The argument mapping.
        /// </summary>
        internal Dictionary<long, object> Arguments;

        /// <summary>
        /// Creates a new instance of the <see cref="ZenExprActionVisitor"/> class.
        /// </summary>
        /// <param name="arguments"></param>
        protected ZenExprActionVisitor(Dictionary<long, object> arguments)
        {
            this.Arguments = arguments;
        }

        /// <summary>
        /// Execute the visitor.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public virtual void VisitCached<T>(Zen<T> expression)
        {
            if (this.Visited.Contains(expression.Id))
            {
                return;
            }

            this.Visited.Add(expression.Id);
            expression.Accept(this);
        }

        /// <summary>
        /// Visit a ApplyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TSrc, TDst>(ZenApplyExpr<TSrc, TDst> expression)
        {
            VisitCached(expression.ArgumentExpr);
            VisitCached(expression.Lambda.Body);
        }

        /// <summary>
        /// Visit a LogicalBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit(ZenLogicalBinopExpr expression)
        {
            VisitCached(expression.Expr1);
            VisitCached(expression.Expr2);
        }

        /// <summary>
        /// Visit a NotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit(ZenNotExpr expression)
        {
            VisitCached(expression.Expr);
        }

        /// <summary>
        /// Visit an IfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenIfExpr<T> expression)
        {
            VisitCached(expression.GuardExpr);
            VisitCached(expression.TrueExpr);
            VisitCached(expression.FalseExpr);
        }

        /// <summary>
        /// Visit a ConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenConstantExpr<T> expression)
        {
            return;
        }

        /// <summary>
        /// Visit a ArithBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenArithBinopExpr<T> expression)
        {
            VisitCached(expression.Expr1);
            VisitCached(expression.Expr2);
        }

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenBitwiseNotExpr<T> expression)
        {
            VisitCached(expression.Expr);
        }

        /// <summary>
        /// Visit a BitwiseBinopExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenBitwiseBinopExpr<T> expression)
        {
            VisitCached(expression.Expr1);
            VisitCached(expression.Expr2);
        }

        /// <summary>
        /// Visit a ListAddFrontExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenFSeqAddFrontExpr<T> expression)
        {
            VisitCached(expression.ListExpr);
            VisitCached(expression.ElementExpr);
        }

        /// <summary>
        /// Visit a MapSetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression)
        {
            VisitCached(expression.MapExpr);
            VisitCached(expression.KeyExpr);
            VisitCached(expression.ValueExpr);
        }

        /// <summary>
        /// Visit a MapDeleteExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression)
        {
            VisitCached(expression.MapExpr);
            VisitCached(expression.KeyExpr);
        }

        /// <summary>
        /// Visit a MapGetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression)
        {
            VisitCached(expression.MapExpr);
            VisitCached(expression.KeyExpr);
        }

        /// <summary>
        /// Visit a MapCombineExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey>(ZenMapCombineExpr<TKey> expression)
        {
            VisitCached(expression.MapExpr1);
            VisitCached(expression.MapExpr2);
        }

        /// <summary>
        /// Visit a ConstMapSetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression)
        {
            VisitCached(expression.MapExpr);
            VisitCached(expression.ValueExpr);
        }

        /// <summary>
        /// Visit a ConstMapGetExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression)
        {
            VisitCached(expression.MapExpr);
        }

        /// <summary>
        /// Visit a ConstMapCombineExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey>(ZenConstMapCombineExpr<TKey> expression)
        {
            VisitCached(expression.MapExpr1);
            VisitCached(expression.MapExpr2);
        }

        /// <summary>
        /// Visit a ListCaseExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TList, TResult>(ZenFSeqCaseExpr<TList, TResult> expression)
        {
            VisitCached(expression.ListExpr);
            VisitCached(expression.EmptyExpr);
            VisitCached(expression.ConsLambda.Body);
        }

        /// <summary>
        /// Visit a ZenSeqUnitExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqUnitExpr<T> expression)
        {
            VisitCached(expression.ValueExpr);
        }

        /// <summary>
        /// Visit a ZenSeqConcatExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqConcatExpr<T> expression)
        {
            VisitCached(expression.SeqExpr1);
            VisitCached(expression.SeqExpr2);
        }

        /// <summary>
        /// Visit a ZenSeqLengthExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqLengthExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
        }

        /// <summary>
        /// Visit a ZenSeqAtExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqAtExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
            VisitCached(expression.IndexExpr);
        }

        /// <summary>
        /// Visit a ZenSeqNthExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqNthExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
            VisitCached(expression.IndexExpr);
        }

        /// <summary>
        /// Visit a ZenSeqContainsExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqContainsExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
            VisitCached(expression.SubseqExpr);
        }

        /// <summary>
        /// Visit a ZenSeqIndexOfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqIndexOfExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
            VisitCached(expression.SubseqExpr);
            VisitCached(expression.OffsetExpr);
        }

        /// <summary>
        /// Visit a ZenSeqSliceExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqSliceExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
            VisitCached(expression.LengthExpr);
            VisitCached(expression.OffsetExpr);
        }

        /// <summary>
        /// Visit a ZenSeqReplaceFirstExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqReplaceFirstExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
            VisitCached(expression.SubseqExpr);
            VisitCached(expression.ReplaceExpr);
        }

        /// <summary>
        /// Visit a ZenSeqRegexExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenSeqRegexExpr<T> expression)
        {
            VisitCached(expression.SeqExpr);
        }

        /// <summary>
        /// Visit a GetFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T1, T2>(ZenGetFieldExpr<T1, T2> expression)
        {
            VisitCached(expression.Expr);
        }

        /// <summary>
        /// Visit a WithFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T1, T2>(ZenWithFieldExpr<T1, T2> expression)
        {
            VisitCached(expression.Expr);
            VisitCached(expression.FieldExpr);
        }

        /// <summary>
        /// Visit a CreateObjectExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TObject>(ZenCreateObjectExpr<TObject> expression)
        {
            foreach (var fieldValuePair in expression.Fields)
            {
                try
                {
                    var valueType = fieldValuePair.Value.GetType();
                    var innerType = valueType.BaseType.GetGenericArgumentsCached()[0];
                    var evaluateMethod = this.GetType()
                        .GetMethodCached("VisitCached")
                        .MakeGenericMethod(innerType);
                    evaluateMethod.Invoke(this, new object[] { fieldValuePair.Value });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
        }

        /// <summary>
        /// Visit a EqualityExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenEqualityExpr<T> expression)
        {
            VisitCached(expression.Expr1);
            VisitCached(expression.Expr2);
        }

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenArithComparisonExpr<T> expression)
        {
            VisitCached(expression.Expr1);
            VisitCached(expression.Expr2);
        }

        /// <summary>
        /// Visit an ArgumentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenParameterExpr<T> expression)
        {
            if (!this.Arguments.ContainsKey(expression.Id))
            {
                return;
            }

            try
            {
                var expr = this.Arguments[expression.ParameterId];
                var type = expr.GetType().BaseType.GetGenericArgumentsCached()[0];
                var evaluateMethod = this.GetType()
                    .GetMethodCached("VisitCached")
                    .MakeGenericMethod(type);
                evaluateMethod.Invoke(this, new object[] { expr });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<T>(ZenArbitraryExpr<T> expression)
        {
            return;
        }

        /// <summary>
        /// Visit a CastExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A return value.</returns>
        public virtual void Visit<TKey, TValue>(ZenCastExpr<TKey, TValue> expression)
        {
            VisitCached(expression.SourceExpr);
        }
    }
}
