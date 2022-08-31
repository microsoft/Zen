// <copyright file="ExpressionEvaluatorVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.SymbolicExecution;

    /// <summary>
    /// Interpret a Zen expression.
    /// </summary>
    internal sealed class ExpressionEvaluatorVisitor : ZenExprVisitor<ExpressionEvaluatorEnvironment, object>
    {
        /// <summary>
        /// Evaluate method reference.
        /// </summary>
        private static MethodInfo evaluateMethod = typeof(ExpressionEvaluatorVisitor).GetMethod("Visit");

        /// <summary>
        /// Whether to track covered branches.
        /// </summary>
        private bool trackBranches;

        /// <summary>
        /// Path constraint for the execution.
        /// </summary>
        public PathConstraint PathConstraint { get; set; }

        /// <summary>
        /// Track the symbolic assignment to arguments when collecting path constraints.
        /// </summary>
        public Dictionary<long, object> PathConstraintSymbolicEnvironment { get; set; }

        /// <summary>
        /// Create a new instance of the <see cref="ExpressionEvaluatorVisitor"/> class.
        /// </summary>
        /// <param name="trackBranches">Whether to track branches during execution.</param>
        public ExpressionEvaluatorVisitor(bool trackBranches)
        {
            this.trackBranches = trackBranches;

            if (this.trackBranches)
            {
                this.PathConstraint = new PathConstraint();
                this.PathConstraintSymbolicEnvironment = new Dictionary<long, object>();
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitArbitrary<T>(ZenArbitraryExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (parameter.ArbitraryAssignment == null)
                return ReflectionUtilities.GetDefaultValue<T>();
            if (!parameter.ArbitraryAssignment.TryGetValue(expression, out var value))
                return ReflectionUtilities.GetDefaultValue<T>();
            return value;
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitLogicalBinop(ZenLogicalBinopExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (bool)this.Visit(expression.Expr1, parameter);
            var e2 = (bool)this.Visit(expression.Expr2, parameter);

            switch (expression.Operation)
            {
                case ZenLogicalBinopExpr.LogicalOp.And:
                    return e1 && e2;
                default:
                    Contract.Assert(expression.Operation == ZenLogicalBinopExpr.LogicalOp.Or);
                    return e1 || e2;
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitArgument<T>(ZenArgumentExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.ArgumentId];
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitArithBinop<T>(ZenArithBinopExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = this.Visit(expression.Expr1, parameter);
            var e2 = this.Visit(expression.Expr2, parameter);
            var type = typeof(T);

            switch (expression.Operation)
            {
                case ArithmeticOp.Addition:
                    if (type == ReflectionUtilities.ByteType)
                        return (byte)((byte)e1 + (byte)e2);
                    else if (type == ReflectionUtilities.ShortType)
                        return (short)((short)e1 + (short)e2);
                    else if (type == ReflectionUtilities.UshortType)
                        return (ushort)((ushort)e1 + (ushort)e2);
                    else if (type == ReflectionUtilities.IntType)
                        return (int)e1 + (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        return (uint)e1 + (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        return (long)e1 + (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        return (ulong)e1 + (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        return (BigInteger)e1 + (BigInteger)e2;
                    else if (type == ReflectionUtilities.RealType)
                        return (Real)e1 + (Real)e2;
                    else
                    {
                        Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
                        return ((dynamic)e1).Add((dynamic)e2);
                    }

                case ArithmeticOp.Subtraction:
                    if (type == ReflectionUtilities.ByteType)
                        return (byte)((byte)e1 - (byte)e2);
                    else if (type == ReflectionUtilities.ShortType)
                        return (short)((short)e1 - (short)e2);
                    else if (type == ReflectionUtilities.UshortType)
                        return (ushort)((ushort)e1 - (ushort)e2);
                    else if (type == ReflectionUtilities.IntType)
                        return (int)e1 - (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        return (uint)e1 - (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        return (long)e1 - (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        return (ulong)e1 - (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        return (BigInteger)e1 - (BigInteger)e2;
                    else if (type == ReflectionUtilities.RealType)
                        return (Real)e1 - (Real)e2;
                    else
                    {
                        Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
                        return ((dynamic)e1).Subtract((dynamic)e2);
                    }

                default:
                    Contract.Assert(expression.Operation == ArithmeticOp.Multiplication);
                    if (type == ReflectionUtilities.ByteType)
                        return (byte)((byte)e1 * (byte)e2);
                    else if (type == ReflectionUtilities.ShortType)
                        return (short)((short)e1 * (short)e2);
                    else if (type == ReflectionUtilities.UshortType)
                        return (ushort)((ushort)e1 * (ushort)e2);
                    else if (type == ReflectionUtilities.IntType)
                        return (int)e1 * (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        return (uint)e1 * (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        return (long)e1 * (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        return (ulong)e1 * (ulong)e2;
                    else if (type == ReflectionUtilities.RealType)
                        return (Real)e1 * (Real)e2;
                    else
                    {
                        Contract.Assert(ReflectionUtilities.IsBigIntegerType(type));
                        return (BigInteger)e1 * (BigInteger)e2;
                    }
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = this.Visit(expression.Expr1, parameter);
            var e2 = this.Visit(expression.Expr2, parameter);
            var type = typeof(T);

            switch (expression.Operation)
            {
                case BitwiseOp.BitwiseAnd:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        return ((dynamic)e1).BitwiseAnd((dynamic)e2);
                    else
                        return ReflectionUtilities.FromLong<T>(ReflectionUtilities.ToLong(e1) & ReflectionUtilities.ToLong(e2));

                case BitwiseOp.BitwiseOr:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        return ((dynamic)e1).BitwiseOr((dynamic)e2);
                    else
                        return ReflectionUtilities.FromLong<T>(ReflectionUtilities.ToLong(e1) | ReflectionUtilities.ToLong(e2));

                default:
                    Contract.Assert(expression.Operation == BitwiseOp.BitwiseXor);
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        return ((dynamic)e1).BitwiseXor((dynamic)e2);
                    else
                        return ReflectionUtilities.FromLong<T>(ReflectionUtilities.ToLong(e1) ^ ReflectionUtilities.ToLong(e2));
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var x = ReflectionUtilities.ToLong(this.Visit(expression.Expr, parameter));
            return ReflectionUtilities.FromLong<T>(~x);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitConstant<T>(ZenConstantExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var fieldNames = new List<string>();
            var parameters = new List<object>();
            foreach (var fieldValuePair in expression.Fields)
            {
                var type = fieldValuePair.Value.GetType();
                var innerType = type.BaseType.GetGenericArgumentsCached()[0];
                var field = fieldValuePair.Key;
                var method = evaluateMethod.MakeGenericMethod(innerType);
                var valueResult = method.Invoke(this, new object[] { fieldValuePair.Value, parameter });
                fieldNames.Add(field);
                parameters.Add(valueResult);
            }

            return ReflectionUtilities.CreateInstance<TObject>(fieldNames.ToArray(), parameters.ToArray());
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = (T1)this.Visit(expression.Expr, parameter);
            return ReflectionUtilities.GetFieldOrProperty<T1, T2>(e, expression.FieldName);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitIf<T>(ZenIfExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (bool)this.Visit(expression.GuardExpr, parameter);

            if (e1)
            {
                if (this.trackBranches)
                {
                    this.PathConstraint = this.PathConstraint.Add(expression.GuardExpr);
                }

                return (T)this.Visit(expression.TrueExpr, parameter);
            }
            else
            {
                if (this.trackBranches)
                {
                    this.PathConstraint = this.PathConstraint.Add(ZenNotExpr.Create(expression.GuardExpr));
                }

                return (T)this.Visit(expression.FalseExpr, parameter);
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitEquality<T>(ZenEqualityExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = this.Visit(expression.Expr1, parameter);
            var e2 = this.Visit(expression.Expr2, parameter);
            return ((T)e1).Equals((T)e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = this.Visit(expression.Expr1, parameter);
            var e2 = this.Visit(expression.Expr2, parameter);
            var type = typeof(T);

            switch (expression.ComparisonType)
            {
                case ComparisonType.Geq:
                    if (type == ReflectionUtilities.ByteType)
                        return (byte)e1 >= (byte)e2;
                    else if (type == ReflectionUtilities.ShortType)
                        return (short)e1 >= (short)e2;
                    else if (type == ReflectionUtilities.UshortType)
                        return (ushort)e1 >= (ushort)e2;
                    else if (type == ReflectionUtilities.IntType)
                        return (int)e1 >= (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        return (uint)e1 >= (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        return (long)e1 >= (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        return (ulong)e1 >= (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        return (BigInteger)e1 >= (BigInteger)e2;
                    else if (type == ReflectionUtilities.RealType)
                        return (Real)e1 >= (Real)e2;
                    else
                    {
                        Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
                        return ((dynamic)e1) >= ((dynamic)e2);
                    }

                case ComparisonType.Gt:
                    if (type == ReflectionUtilities.ByteType)
                        return (byte)e1 > (byte)e2;
                    else if (type == ReflectionUtilities.ShortType)
                        return (short)e1 > (short)e2;
                    else if (type == ReflectionUtilities.UshortType)
                        return (ushort)e1 > (ushort)e2;
                    else if (type == ReflectionUtilities.IntType)
                        return (int)e1 > (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        return (uint)e1 > (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        return (long)e1 > (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        return (ulong)e1 > (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        return (BigInteger)e1 > (BigInteger)e2;
                    else if (type == ReflectionUtilities.RealType)
                        return (Real)e1 > (Real)e2;
                    else
                    {
                        Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
                        return ((dynamic)e1) > ((dynamic)e2);
                    }

                case ComparisonType.Lt:
                    if (type == ReflectionUtilities.ByteType)
                        return (byte)e1 < (byte)e2;
                    else if (type == ReflectionUtilities.ShortType)
                        return (short)e1 < (short)e2;
                    else if (type == ReflectionUtilities.UshortType)
                        return (ushort)e1 < (ushort)e2;
                    else if (type == ReflectionUtilities.IntType)
                        return (int)e1 < (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        return (uint)e1 < (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        return (long)e1 < (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        return (ulong)e1 < (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        return (BigInteger)e1 < (BigInteger)e2;
                    else if (type == ReflectionUtilities.RealType)
                        return (Real)e1 < (Real)e2;
                    else
                    {
                        Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
                        return ((dynamic)e1) < ((dynamic)e2);
                    }

                default:
                    Contract.Assert(expression.ComparisonType == ComparisonType.Leq);
                    if (type == ReflectionUtilities.ByteType)
                        return (byte)e1 <= (byte)e2;
                    else if (type == ReflectionUtilities.ShortType)
                        return (short)e1 <= (short)e2;
                    else if (type == ReflectionUtilities.UshortType)
                        return (ushort)e1 <= (ushort)e2;
                    else if (type == ReflectionUtilities.IntType)
                        return (int)e1 <= (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        return (uint)e1 <= (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        return (long)e1 <= (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        return (ulong)e1 <= (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        return (BigInteger)e1 <= (BigInteger)e2;
                    else if (type == ReflectionUtilities.RealType)
                        return (Real)e1 <= (Real)e2;
                    else
                    {
                        Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
                        return ((dynamic)e1) <= ((dynamic)e2);
                    }
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitListEmpty<T>(ZenFSeqEmptyExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return new FSeq<T>();
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitListAdd<T>(ZenFSeqAddFrontExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (FSeq<T>)this.Visit(expression.Expr, parameter);
            var e2 = (Option<T>)this.Visit(expression.ElementExpr, parameter);
            return e1.AddFrontOption(e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitListCase<T, TResult>(ZenFSeqCaseExpr<T, TResult> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = (FSeq<T>)this.Visit(expression.ListExpr, parameter);

            if (e.Count() == 0)
            {
                if (this.trackBranches)
                {
                    this.PathConstraint.Add(expression.ListExpr.IsEmpty());
                }

                return this.Visit(expression.EmptyExpr, parameter);
            }
            else
            {
                var (hd, tl) = CommonUtilities.SplitHead(e);
                var argHd = new ZenArgumentExpr<Option<T>>();
                var argTl = new ZenArgumentExpr<FSeq<T>>();
                parameter.ArgumentAssignment[argHd.ArgumentId] = hd;
                parameter.ArgumentAssignment[argTl.ArgumentId] = tl;

                if (this.trackBranches)
                {
                    this.PathConstraint.Add(Zen.Not(expression.ListExpr.IsEmpty()));
                    this.PathConstraintSymbolicEnvironment[argHd.ArgumentId] = expression.ListExpr.Head();
                    this.PathConstraintSymbolicEnvironment[argTl.ArgumentId] = expression.ListExpr.Tail();
                }

                var c = expression.ConsCase.Invoke(argHd, argTl);
                return (TResult)this.Visit(c, parameter);
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitNot(ZenNotExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return !(bool)this.Visit(expression.Expr, parameter);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (T1)this.Visit(expression.Expr, parameter);
            var e2 = (T2)this.Visit(expression.FieldExpr, parameter);
            return ReflectionUtilities.WithField<T1>(e1, expression.FieldName, e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitMapEmpty<TKey, TValue>(ZenMapEmptyExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return new Map<TKey, TValue>();
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Map<TKey, TValue>)this.Visit(expression.MapExpr, parameter);
            var e2 = (TKey)this.Visit(expression.KeyExpr, parameter);
            var e3 = (TValue)this.Visit(expression.ValueExpr, parameter);
            return e1.Set(e2, e3);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Map<TKey, TValue>)this.Visit(expression.MapExpr, parameter);
            var e2 = (TKey)this.Visit(expression.KeyExpr, parameter);
            return e1.Delete(e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Map<TKey, TValue>)this.Visit(expression.MapExpr, parameter);
            var e2 = (TKey)this.Visit(expression.KeyExpr, parameter);
            return e1.Get(e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Map<TKey, SetUnit>)this.Visit(expression.MapExpr1, parameter);
            var e2 = (Map<TKey, SetUnit>)this.Visit(expression.MapExpr2, parameter);

            switch (expression.CombinationType)
            {
                case ZenMapCombineExpr<TKey>.CombineType.Intersect:
                    return CommonUtilities.DictionaryIntersect(e1, e2);
                case ZenMapCombineExpr<TKey>.CombineType.Union:
                    return CommonUtilities.DictionaryUnion(e1, e2);
                default:
                    Contract.Assert(expression.CombinationType == ZenMapCombineExpr<TKey>.CombineType.Difference);
                    return CommonUtilities.DictionaryDifference(e1, e2);
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (CMap<TKey, TValue>)this.Visit(expression.MapExpr, parameter);
            var e2 = (TValue)this.Visit(expression.ValueExpr, parameter);
            return e1.Set(expression.Key, e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (CMap<TKey, TValue>)this.Visit(expression.MapExpr, parameter);
            return e1.Get(expression.Key);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)this.Visit(expression.SeqExpr1, parameter);
            var e2 = (Seq<T>)this.Visit(expression.SeqExpr2, parameter);
            return e1.Concat(e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (T)this.Visit(expression.ValueExpr, parameter);
            return new Seq<T>(e1);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = (Seq<T>)this.Visit(expression.SeqExpr, parameter);
            return new BigInteger(e.Length());
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqAt<T>(ZenSeqAtExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)this.Visit(expression.SeqExpr, parameter);
            var e2 = (BigInteger)this.Visit(expression.IndexExpr, parameter);
            return e1.AtBigInteger(e2);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)this.Visit(expression.SeqExpr, parameter);
            var e2 = (Seq<T>)this.Visit(expression.SubseqExpr, parameter);

            switch (expression.ContainmentType)
            {
                case SeqContainmentType.HasPrefix:
                    return e1.HasPrefix(e2);
                case SeqContainmentType.HasSuffix:
                    return e1.HasSuffix(e2);
                default:
                    Contract.Assert(expression.ContainmentType == SeqContainmentType.Contains);
                    return e1.Contains(e2);
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)this.Visit(expression.SeqExpr, parameter);
            var e2 = (Seq<T>)this.Visit(expression.SubseqExpr, parameter);
            var e3 = (BigInteger)this.Visit(expression.OffsetExpr, parameter);
            return e1.IndexOfBigInteger(e2, e3);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)this.Visit(expression.SeqExpr, parameter);
            var e2 = (BigInteger)this.Visit(expression.OffsetExpr, parameter);
            var e3 = (BigInteger)this.Visit(expression.LengthExpr, parameter);
            return e1.SliceBigInteger(e2, e3);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)this.Visit(expression.SeqExpr, parameter);
            var e2 = (Seq<T>)this.Visit(expression.SubseqExpr, parameter);
            var e3 = (Seq<T>)this.Visit(expression.ReplaceExpr, parameter);
            return e1.ReplaceFirst(e2, e3);
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = this.Visit(expression.SourceExpr, parameter);

            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                return Seq.FromString((string)e);
            }
            else if (typeof(TKey) == ReflectionUtilities.UnicodeSequenceType)
            {
                Contract.Assert(typeof(TKey) == ReflectionUtilities.UnicodeSequenceType);
                return Seq.AsString((Seq<char>)e);
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TKey)));
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TValue)));
                return IntN.CastFiniteInteger<TKey, TValue>((TKey)e);
            }
        }

        /// <summary>
        /// Visit a Zen expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>The C# object.</returns>
        public override object VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = (Seq<T>)this.Visit(expression.SeqExpr, parameter);
            return e.MatchesRegex(expression.Regex);
        }
    }
}
