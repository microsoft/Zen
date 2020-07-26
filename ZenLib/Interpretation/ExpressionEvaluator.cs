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

        /// <summary>
        /// Visit an AdapterExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
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

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
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

        /// <summary>
        /// Visit an AndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenAndExpr(ZenAndExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return (bool)expression.Expr1.Accept(this, parameter) && (bool)expression.Expr2.Accept(this, parameter);
        }

        /// <summary>
        /// Visit an ArgumentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.Id];
        }

        /// <summary>
        /// Visit a BitwiseAndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
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
                    case Op.Multiplication:
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
                    default:
                        throw new ZenException($"Invalid operation: {expression.Operation}");
                }
            });
        }

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var x = ReflectionUtilities.ToLong(expression.Expr.Accept(this, parameter));
                return ReflectionUtilities.Specialize<T>(~x);
            });
        }

        /// <summary>
        /// Visit a BoolConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a ByteConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantByteExpr(ZenConstantByteExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a CreateObjectExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
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

        /// <summary>
        /// Visit an EmptyListExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return ImmutableList<T>.Empty;
        }

        /// <summary>
        /// Visit a GetFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e = (T1)expression.Expr.Accept(this, parameter);
                return ReflectionUtilities.GetFieldOrProperty<T1, T2>(e, expression.FieldName);
            });
        }

        /// <summary>
        /// Visit an IfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenIfExpr<T>(ZenIfExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = (bool)expression.GuardExpr.Accept(this, parameter);
                var branch = e1 ? expression.TrueExpr : expression.FalseExpr;
                return (T)branch.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit a ComparisonExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
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

                    case ComparisonType.Eq:
                        return ((T)e1).Equals((T)e2);

                    default:
                        throw new ZenException($"Invalid comparison type: {expression.ComparisonType}");
                }
            });
        }

        /// <summary>
        /// Visit a ListAddFrontExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var e1 = CommonUtilities.ToImmutableList<T>(expression.Expr.Accept(this, parameter));
                var e2 = (T)expression.Element.Accept(this, parameter);
                return e1.Insert(0, e2);
            });
        }

        /// <summary>
        /// Visit a ListMatchExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
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

        /// <summary>
        /// Visit a NotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenNotExpr(ZenNotExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return !(bool)expression.Expr.Accept(this, parameter);
        }

        /// <summary>
        /// Visit an OrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenOrExpr(ZenOrExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return (bool)expression.Expr1.Accept(this, parameter) || (bool)expression.Expr2.Accept(this, parameter);
        }

        /// <summary>
        /// Visit a ConcatExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConcatExpr(ZenConcatExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                return (string)expression.Expr1.Accept(this, parameter) + (string)expression.Expr2.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit a IntConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantIntExpr(ZenConstantIntExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a UintConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantUintExpr(ZenConstantUintExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a LongConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantLongExpr(ZenConstantLongExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a UlongConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a ShortConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantShortExpr(ZenConstantShortExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a UshortConstantExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a ConstantStringExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenConstantStringExpr(ZenConstantStringExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        /// <summary>
        /// Visit a WithFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
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
