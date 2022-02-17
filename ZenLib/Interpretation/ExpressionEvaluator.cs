// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.SymbolicExecution;
    using static ZenLib.Zen;

    /// <summary>
    /// Interpret a Zen expression.
    /// </summary>
    internal sealed class ExpressionEvaluator : IZenExprVisitor<ExpressionEvaluatorEnvironment, object>
    {
        /// <summary>
        /// Whether to track covered branches.
        /// </summary>
        private bool trackBranches;

        /// <summary>
        /// Path constraint for the execution.
        /// </summary>
        public PathConstraint PathConstraint { get; set; }

        /// <summary>
        /// Cache of inputs and results.
        /// </summary>
        private Dictionary<object, object> cache = new Dictionary<object, object>();

        /// <summary>
        /// Create a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="trackBranches">Whether to track branches during execution.</param>
        public ExpressionEvaluator(bool trackBranches)
        {
            this.trackBranches = trackBranches;

            if (trackBranches)
            {
                this.PathConstraint = new PathConstraint();
            }
        }

        public object Evaluate<T>(Zen<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Accept(this, parameter);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (parameter.ArbitraryAssignment == null)
                return ReflectionUtilities.GetDefaultValue<T>();
            if (!parameter.ArbitraryAssignment.TryGetValue(expression, out var value))
                return ReflectionUtilities.GetDefaultValue<T>();
            return value;
        }

        public object VisitZenAndExpr(ZenAndExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return (bool)Evaluate(expression.Expr1, parameter) && (bool)Evaluate(expression.Expr2, parameter);
        }

        public object VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.ArgumentId];
        }

        public object VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);
            var type = typeof(T);

            switch (expression.Operation)
            {
                case Op.BitwiseAnd:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        return ((dynamic)e1).BitwiseAnd((dynamic)e2);
                    else
                        return ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) & ReflectionUtilities.ToLong(e2));

                case Op.BitwiseOr:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        return ((dynamic)e1).BitwiseOr((dynamic)e2);
                    else
                        return ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) | ReflectionUtilities.ToLong(e2));

                case Op.BitwiseXor:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        return ((dynamic)e1).BitwiseXor((dynamic)e2);
                    else
                        return ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) ^ ReflectionUtilities.ToLong(e2));

                case Op.Addition:
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
                    else
                        return ((dynamic)e1).Add((dynamic)e2);

                case Op.Subtraction:
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
                    else
                        return ((dynamic)e1).Subtract((dynamic)e2);

                default:
                    Contract.Assert(expression.Operation == Op.Multiplication);
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
                    else
                        return (BigInteger)e1 * (BigInteger)e2;
            }
        }

        public object VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var x = ReflectionUtilities.ToLong(Evaluate(expression.Expr, parameter));
            return ReflectionUtilities.Specialize<T>(~x);
        }

        public object VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var fieldNames = new List<string>();
            var parameters = new List<object>();
            foreach (var fieldValuePair in expression.Fields)
            {
                var type = fieldValuePair.Value.GetType();
                var innerType = type.BaseType.GetGenericArgumentsCached()[0];
                var field = fieldValuePair.Key;
                var evaluateMethod = typeof(ExpressionEvaluator)
                    .GetMethod("Evaluate", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(innerType);
                try
                {
                    var valueResult = evaluateMethod.Invoke(this, new object[] { fieldValuePair.Value, parameter });
                    fieldNames.Add(field);
                    parameters.Add(valueResult);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            return ReflectionUtilities.CreateInstance<TObject>(fieldNames.ToArray(), parameters.ToArray());
        }

        public object VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return ImmutableList<T>.Empty;
        }

        public object VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = (T1)Evaluate(expression.Expr, parameter);
            return ReflectionUtilities.GetFieldOrProperty<T1, T2>(e, expression.FieldName);
        }

        public object VisitZenIfExpr<T>(ZenIfExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (bool)Evaluate(expression.GuardExpr, parameter);

            if (e1)
            {
                if (trackBranches)
                {
                    this.PathConstraint = this.PathConstraint.Add(expression.GuardExpr);
                }

                return (T)Evaluate(expression.TrueExpr, parameter);
            }
            else
            {
                if (trackBranches)
                {
                    this.PathConstraint = this.PathConstraint.Add(ZenNotExpr.Create(expression.GuardExpr));
                }

                return (T)Evaluate(expression.FalseExpr, parameter);
            }
        }

        public object VisitZenEqualityExpr<T>(ZenEqualityExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (ReflectionUtilities.IsIDictType(typeof(T)))
            {
                return CommonUtilities.DictionaryEquals((dynamic)e1, (dynamic)e2);
            }
            else
            {
                return ((T)e1).Equals((T)e2);
            }
        }

        public object VisitZenComparisonExpr<T>(ZenIntegerComparisonExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);
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
                    else
                        return ((dynamic)e1) >= ((dynamic)e2);

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
                    else
                        return ((dynamic)e1) <= ((dynamic)e2);
            }
        }

        public object VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (ImmutableList<T>)Evaluate(expression.Expr, parameter);
            var e2 = (T)Evaluate(expression.Element, parameter);
            return e1.Insert(0, e2);
        }

        public object VisitZenListCaseExpr<T, TResult>(ZenListCaseExpr<T, TResult> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = (ImmutableList<T>)Evaluate(expression.ListExpr, parameter);

            if (e.Count == 0)
            {
                return Evaluate(expression.EmptyCase, parameter);
            }
            else
            {
                var (hd, tl) = CommonUtilities.SplitHead(e);
                var c = expression.ConsCase.Invoke(Constant(hd), Constant(tl));
                return (TResult)Evaluate(c, parameter);
            }
        }

        public object VisitZenNotExpr(ZenNotExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return !(bool)Evaluate(expression.Expr, parameter);
        }

        public object VisitZenOrExpr(ZenOrExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return (bool)Evaluate(expression.Expr1, parameter) || (bool)Evaluate(expression.Expr2, parameter);
        }

        public object VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (T1)Evaluate(expression.Expr, parameter);
            var e2 = (T2)Evaluate(expression.FieldValue, parameter);
            return ReflectionUtilities.WithField<T1>(e1, expression.FieldName, e2);
        }

        public object VisitZenDictEmptyExpr<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return ImmutableDictionary<TKey, TValue>.Empty;
        }

        public object VisitZenDictSetExpr<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (ImmutableDictionary<TKey, TValue>)Evaluate(expression.DictExpr, parameter);
            var e2 = (TKey)Evaluate(expression.KeyExpr, parameter);
            var e3 = (TValue)Evaluate(expression.ValueExpr, parameter);
            return e1.SetItem(e2, e3);
        }

        public object VisitZenDictDeleteExpr<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (ImmutableDictionary<TKey, TValue>)Evaluate(expression.DictExpr, parameter);
            var e2 = (TKey)Evaluate(expression.KeyExpr, parameter);
            return e1.Remove(e2);
        }

        public object VisitZenDictGetExpr<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (ImmutableDictionary<TKey, TValue>)Evaluate(expression.DictExpr, parameter);
            var e2 = (TKey)Evaluate(expression.KeyExpr, parameter);
            return CommonUtilities.DictionaryGet(e1, e2);
        }

        public object VisitZenDictCombineExpr<TKey>(ZenDictCombineExpr<TKey> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (ImmutableDictionary<TKey, SetUnit>)Evaluate(expression.DictExpr1, parameter);
            var e2 = (ImmutableDictionary<TKey, SetUnit>)Evaluate(expression.DictExpr2, parameter);

            switch (expression.CombinationType)
            {
                case ZenDictCombineExpr<TKey>.CombineType.Intersect:
                    return CommonUtilities.DictionaryIntersect(e1, e2);
                default:
                    Contract.Assert(expression.CombinationType == ZenDictCombineExpr<TKey>.CombineType.Union);
                    return CommonUtilities.DictionaryUnion(e1, e2);
            }
        }

        public object VisitZenSeqEmptyExpr<T>(ZenSeqEmptyExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return new Seq<T>();
        }

        public object VisitZenSeqConcatExpr<T>(ZenSeqConcatExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)Evaluate(expression.SeqExpr1, parameter);
            var e2 = (Seq<T>)Evaluate(expression.SeqExpr2, parameter);
            return e1.Concat(e2);
        }

        public object VisitZenSeqUnitExpr<T>(ZenSeqUnitExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (T)Evaluate(expression.ValueExpr, parameter);
            return new Seq<T>(e1);
        }

        public object VisitZenSeqLengthExpr<T>(ZenSeqLengthExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = (Seq<T>)Evaluate(expression.SeqExpr, parameter);
            return new BigInteger(e.Length());
        }

        public object VisitZenSeqAtExpr<T>(ZenSeqAtExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)Evaluate(expression.SeqExpr, parameter);
            var e2 = (BigInteger)Evaluate(expression.IndexExpr, parameter);
            return e1.AtBigInteger(e2);
        }

        public object VisitZenSeqContainsExpr<T>(ZenSeqContainsExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)Evaluate(expression.SeqExpr, parameter);
            var e2 = (Seq<T>)Evaluate(expression.SubseqExpr, parameter);

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

        public object VisitZenSeqIndexOfExpr<T>(ZenSeqIndexOfExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)Evaluate(expression.SeqExpr, parameter);
            var e2 = (Seq<T>)Evaluate(expression.SubseqExpr, parameter);
            var e3 = (BigInteger)Evaluate(expression.OffsetExpr, parameter);
            return e1.IndexOfBigInteger(e2, e3);
        }

        public object VisitZenSeqSliceExpr<T>(ZenSeqSliceExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)Evaluate(expression.SeqExpr, parameter);
            var e2 = (BigInteger)Evaluate(expression.OffsetExpr, parameter);
            var e3 = (BigInteger)Evaluate(expression.LengthExpr, parameter);
            return e1.SliceBigInteger(e2, e3);
        }

        public object VisitZenSeqReplaceFirstExpr<T>(ZenSeqReplaceFirstExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (Seq<T>)Evaluate(expression.SeqExpr, parameter);
            var e2 = (Seq<T>)Evaluate(expression.SubseqExpr, parameter);
            var e3 = (Seq<T>)Evaluate(expression.ReplaceExpr, parameter);
            return e1.ReplaceFirst(e2, e3);
        }

        public object VisitZenCastExpr<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e = Evaluate(expression.SourceExpr, parameter);

            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                return Seq.FromString((string)e);
            }
            else
            {
                Contract.Assert(typeof(TKey) == ReflectionUtilities.ByteSequenceType);
                return Seq.AsString((Seq<byte>)e);
            }
        }
    }
}
