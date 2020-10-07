// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using ZenLib.SymbolicExecution;
    using static ZenLib.Language;

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
        private Dictionary<(object, ExpressionEvaluatorEnvironment), object> cache = new Dictionary<(object, ExpressionEvaluatorEnvironment), object>();

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

        /// <summary>
        /// Lookup an existing cached value or compute it and cache the result.
        /// </summary>
        /// <param name="obj">The expression object.</param>
        /// <param name="env">The environment.</param>
        /// <param name="callback">The callback to compute the result.</param>
        /// <returns>The result of the computation.</returns>
        private object LookupOrCompute(object obj, ExpressionEvaluatorEnvironment env, Func<object> callback)
        {
            var key = (obj, env);
            if (this.cache.TryGetValue(key, out var value))
            {
                return value;
            }

            var result = callback();
            this.cache[key] = result;
            return result;
        }

        public object VisitZenAdapterExpr<TTo, TFrom>(ZenAdapterExpr<TTo, TFrom> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e = expression.Expr.Accept(this, parameter);
                foreach (var converter in expression.Converters)
                {
                    e = converter(e);
                }

                return e;
            });
        }

        public object VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                if (parameter.ArbitraryAssignment == null)
                    return ReflectionUtilities.GetDefaultValue<T>();
                if (!parameter.ArbitraryAssignment.TryGetValue(expression, out var value))
                    return ReflectionUtilities.GetDefaultValue<T>();

                // the library doesn't distinguish between signed and unsigned,
                // so we must perform this conversion manually.
                var type = typeof(T);
                if (type != value.GetType())
                {
                    if (type == ReflectionUtilities.UshortType)
                        return (ushort)(short)value;
                    if (type == ReflectionUtilities.UintType)
                        return (uint)(int)value;
                    if (type == ReflectionUtilities.UlongType)
                        return (ulong)(long)value;
                }
                return value;
            });
        }

        public object VisitZenAndExpr(ZenAndExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return (bool)expression.Expr1.Accept(this, parameter) && (bool)expression.Expr2.Accept(this, parameter);
        }

        public object VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.Id];
        }

        public object VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = expression.Expr1.Accept(this, parameter);
                var e2 = expression.Expr2.Accept(this, parameter);
                var type = typeof(T);

                switch (expression.Operation)
                {
                    case Op.BitwiseAnd:
                        if (ReflectionUtilities.IsFixedIntegerType(type))
                            return ((dynamic)e1).BitwiseAnd((dynamic)e2);
                        return ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) & ReflectionUtilities.ToLong(e2));

                    case Op.BitwiseOr:
                        if (ReflectionUtilities.IsFixedIntegerType(type))
                            return ((dynamic)e1).BitwiseOr((dynamic)e2);
                        return ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) | ReflectionUtilities.ToLong(e2));

                    case Op.BitwiseXor:
                        if (ReflectionUtilities.IsFixedIntegerType(type))
                            return ((dynamic)e1).BitwiseXor((dynamic)e2);
                        return ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) ^ ReflectionUtilities.ToLong(e2));

                    case Op.Addition:
                        if (type == ReflectionUtilities.ByteType)
                            return (byte)((byte)e1 + (byte)e2);
                        if (type == ReflectionUtilities.ShortType)
                            return (short)((short)e1 + (short)e2);
                        if (type == ReflectionUtilities.UshortType)
                            return (ushort)((ushort)e1 + (ushort)e2);
                        if (type == ReflectionUtilities.IntType)
                            return (int)e1 + (int)e2;
                        if (type == ReflectionUtilities.UintType)
                            return (uint)e1 + (uint)e2;
                        if (type == ReflectionUtilities.LongType)
                            return (long)e1 + (long)e2;
                        if (type == ReflectionUtilities.UlongType)
                            return (ulong)e1 + (ulong)e2;
                        if (type == ReflectionUtilities.BigIntType)
                            return (BigInteger)e1 + (BigInteger)e2;
                        return ((dynamic)e1).Add((dynamic)e2);

                    case Op.Subtraction:
                        if (type == ReflectionUtilities.ByteType)
                            return (byte)((byte)e1 - (byte)e2);
                        if (type == ReflectionUtilities.ShortType)
                            return (short)((short)e1 - (short)e2);
                        if (type == ReflectionUtilities.UshortType)
                            return (ushort)((ushort)e1 - (ushort)e2);
                        if (type == ReflectionUtilities.IntType)
                            return (int)e1 - (int)e2;
                        if (type == ReflectionUtilities.UintType)
                            return (uint)e1 - (uint)e2;
                        if (type == ReflectionUtilities.LongType)
                            return (long)e1 - (long)e2;
                        if (type == ReflectionUtilities.UlongType)
                            return (ulong)e1 - (ulong)e2;
                        if (type == ReflectionUtilities.BigIntType)
                            return (BigInteger)e1 - (BigInteger)e2;
                        return ((dynamic)e1).Subtract((dynamic)e2);

                    default:
                        if (type == ReflectionUtilities.ByteType)
                            return (byte)((byte)e1 * (byte)e2);
                        if (type == ReflectionUtilities.ShortType)
                            return (short)((short)e1 * (short)e2);
                        if (type == ReflectionUtilities.UshortType)
                            return (ushort)((ushort)e1 * (ushort)e2);
                        if (type == ReflectionUtilities.IntType)
                            return (int)e1 * (int)e2;
                        if (type == ReflectionUtilities.UintType)
                            return (uint)e1 * (uint)e2;
                        if (type == ReflectionUtilities.LongType)
                            return (long)e1 * (long)e2;
                        if (type == ReflectionUtilities.UlongType)
                            return (ulong)e1 * (ulong)e2;
                        return (BigInteger)e1 * (BigInteger)e2;
                }
            });
        }

        public object VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var x = ReflectionUtilities.ToLong(expression.Expr.Accept(this, parameter));
                return ReflectionUtilities.Specialize<T>(~x);
            });
        }

        public object VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var fieldNames = new List<string>();
                var parameters = new List<object>();
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    dynamic value = fieldValuePair.Value;
                    var valueType = value.GetType();
                    var valueResult = value.Accept(this, parameter);
                    fieldNames.Add(field);
                    parameters.Add(valueResult);
                }

                return ReflectionUtilities.CreateInstance<TObject>(fieldNames.ToArray(), parameters.ToArray());
            });
        }

        public object VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return ImmutableList<T>.Empty;
        }

        public object VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e = (T1)expression.Expr.Accept(this, parameter);
                return ReflectionUtilities.GetFieldOrProperty<T1, T2>(e, expression.FieldName);
            });
        }

        public object VisitZenIfExpr<T>(ZenIfExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (bool)expression.GuardExpr.Accept(this, parameter);

                if (e1)
                {
                    if (trackBranches)
                    {
                        this.PathConstraint = this.PathConstraint.Add(expression.GuardExpr);
                    }

                    return (T)expression.TrueExpr.Accept(this, parameter);
                }
                else
                {
                    if (trackBranches)
                    {
                        this.PathConstraint = this.PathConstraint.Add(ZenNotExpr.Create(expression.GuardExpr));
                    }

                    return (T)expression.FalseExpr.Accept(this, parameter);
                }
            });
        }

        public object VisitZenComparisonExpr<T>(ZenComparisonExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = expression.Expr1.Accept(this, parameter);
                var e2 = expression.Expr2.Accept(this, parameter);
                var type = typeof(T);

                switch (expression.ComparisonType)
                {
                    case ComparisonType.Geq:
                        if (type == ReflectionUtilities.ByteType)
                            return (byte)e1 >= (byte)e2;
                        if (type == ReflectionUtilities.ShortType)
                            return (short)e1 >= (short)e2;
                        if (type == ReflectionUtilities.UshortType)
                            return (ushort)e1 >= (ushort)e2;
                        if (type == ReflectionUtilities.IntType)
                            return (int)e1 >= (int)e2;
                        if (type == ReflectionUtilities.UintType)
                            return (uint)e1 >= (uint)e2;
                        if (type == ReflectionUtilities.LongType)
                            return (long)e1 >= (long)e2;
                        if (type == ReflectionUtilities.UlongType)
                            return (ulong)e1 >= (ulong)e2;
                        if (type == ReflectionUtilities.BigIntType)
                            return (BigInteger)e1 >= (BigInteger)e2;
                        return ((dynamic)e1) >= ((dynamic)e2);

                    case ComparisonType.Leq:
                        if (type == ReflectionUtilities.ByteType)
                            return (byte)e1 <= (byte)e2;
                        if (type == ReflectionUtilities.ShortType)
                            return (short)e1 <= (short)e2;
                        if (type == ReflectionUtilities.UshortType)
                            return (ushort)e1 <= (ushort)e2;
                        if (type == ReflectionUtilities.IntType)
                            return (int)e1 <= (int)e2;
                        if (type == ReflectionUtilities.UintType)
                            return (uint)e1 <= (uint)e2;
                        if (type == ReflectionUtilities.LongType)
                            return (long)e1 <= (long)e2;
                        if (type == ReflectionUtilities.UlongType)
                            return (ulong)e1 <= (ulong)e2;
                        if (type == ReflectionUtilities.BigIntType)
                            return (BigInteger)e1 <= (BigInteger)e2;
                        return ((dynamic)e1) <= ((dynamic)e2);

                    default:
                        return ((T)e1).Equals((T)e2);
                }
            });
        }

        public object VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = CommonUtilities.ToImmutableList<T>(expression.Expr.Accept(this, parameter));
                var e2 = (T)expression.Element.Accept(this, parameter);
                return e1.Insert(0, e2);
            });
        }

        public object VisitZenListCaseExpr<T, TResult>(ZenListCaseExpr<T, TResult> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e = CommonUtilities.ToImmutableList<T>(expression.ListExpr.Accept(this, parameter));

                if (e.Count == 0)
                {
                    return expression.EmptyCase.Accept(this, parameter);
                }

                var (hd, tl) = CommonUtilities.SplitHead(e);
                var result = expression.ConsCase.Invoke(Constant(hd), Constant(tl));
                return (TResult)result.Accept(this, parameter);
            });
        }

        public object VisitZenNotExpr(ZenNotExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return !(bool)expression.Expr.Accept(this, parameter);
        }

        public object VisitZenOrExpr(ZenOrExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return (bool)expression.Expr1.Accept(this, parameter) || (bool)expression.Expr2.Accept(this, parameter);
        }

        public object VisitZenConcatExpr(ZenConcatExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                return (string)expression.Expr1.Accept(this, parameter) + (string)expression.Expr2.Accept(this, parameter);
            });
        }

        public object VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (string)expression.StringExpr.Accept(this, parameter);
                var e2 = (string)expression.SubstringExpr.Accept(this, parameter);

                switch (expression.ContainmentType)
                {
                    case ContainmentType.PrefixOf:
                        return e1.StartsWith(e2);
                    case ContainmentType.SuffixOf:
                        return e1.EndsWith(e2);
                    default:
                        return e1.Contains(e2);
                }
            });
        }

        public object VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (string)expression.StringExpr.Accept(this, parameter);
                var e2 = (string)expression.SubstringExpr.Accept(this, parameter);
                var e3 = (string)expression.ReplaceExpr.Accept(this, parameter);
                return CommonUtilities.ReplaceFirst(e1, e2, e3);
            });
        }

        public object VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (string)expression.StringExpr.Accept(this, parameter);
                var e2 = (BigInteger)expression.OffsetExpr.Accept(this, parameter);
                var e3 = (BigInteger)expression.LengthExpr.Accept(this, parameter);
                return CommonUtilities.Substring(e1, e2, e3);
            });
        }

        public object VisitZenStringAtExpr(ZenStringAtExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (string)expression.StringExpr.Accept(this, parameter);
                var e2 = (BigInteger)expression.IndexExpr.Accept(this, parameter);
                return CommonUtilities.At(e1, e2);
            });
        }

        public object VisitZenStringLengthExpr(ZenStringLengthExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e = (string)expression.Expr.Accept(this, parameter);
                return (BigInteger)e.Length;
            });
        }

        public object VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (string)expression.StringExpr.Accept(this, parameter);
                var e2 = (string)expression.SubstringExpr.Accept(this, parameter);
                var e3 = (BigInteger)expression.OffsetExpr.Accept(this, parameter);
                return CommonUtilities.IndexOf(e1, e2, e3);
            });
        }

        public object VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (T1)expression.Expr.Accept(this, parameter);
                var e2 = (T2)expression.FieldValue.Accept(this, parameter);
                return ReflectionUtilities.WithField<T1>(e1, expression.FieldName, e2);
            });
        }
    }
}
