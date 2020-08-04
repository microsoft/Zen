// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    /// <summary>
    /// Interpret a Zen expression.
    /// </summary>
    internal sealed class ExpressionEvaluator : IZenExprVisitor<ExpressionEvaluatorEnvironment, object>
    {
        private Dictionary<(object, ExpressionEvaluatorEnvironment), object> cache = new Dictionary<(object, ExpressionEvaluatorEnvironment), object>();

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
                    return default(T);
                if (!parameter.ArbitraryAssignment.TryGetValue(expression, out var value))
                    return default(T);
                // the library doesn't distinguish between signed and unsigned,
                // so we must perform this conversion manually.
                var type = typeof(T);
                if (type == ReflectionUtilities.UshortType)
                    return (ushort)(short)value;
                if (type == ReflectionUtilities.UintType)
                    return (uint)(int)value;
                if (type == ReflectionUtilities.UlongType)
                    return (ulong)(long)value;
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
                var x = ReflectionUtilities.ToLong(e1);
                var y = ReflectionUtilities.ToLong(e2);
                var type = typeof(T);

                switch (expression.Operation)
                {
                    case Op.BitwiseAnd:
                        return ReflectionUtilities.Specialize<T>(x & y);
                    case Op.BitwiseOr:
                        return ReflectionUtilities.Specialize<T>(x | y);
                    case Op.BitwiseXor:
                        return ReflectionUtilities.Specialize<T>(x ^ y);
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
                        return (ulong)e1 + (ulong)e2;
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
                        return (ulong)e1 - (ulong)e2;
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
                        return (ulong)e1 * (ulong)e2;
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

        public object VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenConstantByteExpr(ZenConstantByteExpr expression, ExpressionEvaluatorEnvironment parameter)
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
                    var value = fieldValuePair.Value;
                    var valueType = value.GetType();
                    var acceptMethod = valueType
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(ExpressionEvaluatorEnvironment), typeof(object));
                    var valueResult = acceptMethod.Invoke(value, new object[] { this, parameter });
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
                var branch = e1 ? expression.TrueExpr : expression.FalseExpr;
                return (T)branch.Accept(this, parameter);
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
                        return (ulong)e1 >= (ulong)e2;

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
                        return (ulong)e1 <= (ulong)e2;

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

                // create a new environment and evaluate the function
                var arg1 = new ZenArgumentExpr<T>();
                var arg2 = new ZenArgumentExpr<IList<T>>();
                var args = parameter.ArgumentAssignment.Add(arg1.Id, hd).Add(arg2.Id, tl);
                var newEnv = new ExpressionEvaluatorEnvironment(args);
                var result = expression.ConsCase.Invoke(arg1, arg2);

                return (TResult)result.Accept(this, newEnv);
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
                var e2 = (ushort)expression.OffsetExpr.Accept(this, parameter);
                var e3 = (ushort)expression.LengthExpr.Accept(this, parameter);
                return CommonUtilities.Substring(e1, e2, e3);
            });
        }

        public object VisitZenStringAtExpr(ZenStringAtExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (string)expression.StringExpr.Accept(this, parameter);
                var e2 = (ushort)expression.IndexExpr.Accept(this, parameter);
                return CommonUtilities.At(e1, e2);
            });
        }

        public object VisitZenStringLengthExpr(ZenStringLengthExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e = (string)expression.Expr.Accept(this, parameter);
                return (ushort)e.Length;
            });
        }

        public object VisitZenConstantIntExpr(ZenConstantIntExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenConstantUintExpr(ZenConstantUintExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenConstantLongExpr(ZenConstantLongExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenConstantShortExpr(ZenConstantShortExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenConstantStringExpr(ZenConstantStringExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.UnescapedValue;
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
