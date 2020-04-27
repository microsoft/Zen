// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.Interpretation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

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
                {
                    return default(T);
                }

                if (!parameter.ArbitraryAssignment.TryGetValue(expression, out var value))
                {
                    return default(T);
                }

                // the library doesn't distinguish between signed and unsigned,
                // so we must perform this conversion manually.

                var type = typeof(T);

                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)(short)value;
                }

                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)(int)value;
                }

                if (type == ReflectionUtilities.UlongType)
                {
                    return (ulong)(long)value;
                }

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
        public object VisitZenBitwiseAndExpr<T>(ZenBitwiseAndExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)((byte)expression.Expr1.Accept(this, parameter) & (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)((short)expression.Expr1.Accept(this, parameter) & (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)((ushort)expression.Expr1.Accept(this, parameter) & (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) & (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) & (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) & (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) & (ulong)expression.Expr2.Accept(this, parameter);
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
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)~(byte)expression.Expr.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)~(short)expression.Expr.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)~(ushort)expression.Expr.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return ~(int)expression.Expr.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return ~(uint)expression.Expr.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return ~(long)expression.Expr.Accept(this, parameter);
                }

                return ~(ulong)expression.Expr.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit a BitwiseOrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenBitwiseOrExpr<T>(ZenBitwiseOrExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)((byte)expression.Expr1.Accept(this, parameter) | (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)((short)expression.Expr1.Accept(this, parameter) | (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)((ushort)expression.Expr1.Accept(this, parameter) | (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) | (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) | (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) | (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) | (ulong)expression.Expr2.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit a BitwiseXorExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenBitwiseXorExpr<T>(ZenBitwiseXorExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)((byte)expression.Expr1.Accept(this, parameter) ^ (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)((short)expression.Expr1.Accept(this, parameter) ^ (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)((ushort)expression.Expr1.Accept(this, parameter) ^ (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) ^ (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) ^ (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) ^ (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) ^ (ulong)expression.Expr2.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit a MaxExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenMaxExpr<T>(ZenMaxExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return Math.Max((byte)expression.Expr1.Accept(this, parameter), (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return Math.Max((short)expression.Expr1.Accept(this, parameter), (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return Math.Max((ushort)expression.Expr1.Accept(this, parameter), (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return Math.Max((int)expression.Expr1.Accept(this, parameter), (int)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return Math.Max((uint)expression.Expr1.Accept(this, parameter), (uint)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return Math.Max((long)expression.Expr1.Accept(this, parameter), (long)expression.Expr2.Accept(this, parameter));
                }

                return Math.Max((ulong)expression.Expr1.Accept(this, parameter), (ulong)expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Visit a MinExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenMinExpr<T>(ZenMinExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return Math.Min((byte)expression.Expr1.Accept(this, parameter), (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return Math.Min((short)expression.Expr1.Accept(this, parameter), (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return Math.Min((ushort)expression.Expr1.Accept(this, parameter), (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return Math.Min((int)expression.Expr1.Accept(this, parameter), (int)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return Math.Min((uint)expression.Expr1.Accept(this, parameter), (uint)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return Math.Min((long)expression.Expr1.Accept(this, parameter), (long)expression.Expr2.Accept(this, parameter));
                }

                return Math.Min((ulong)expression.Expr1.Accept(this, parameter), (ulong)expression.Expr2.Accept(this, parameter));
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
                    dynamic value = fieldValuePair.Value;
                    fieldNames.Add(field);
                    parameters.Add(value.Accept(this, parameter));
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
        /// Visit an EqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenEqExpr<T>(ZenEqExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            var e1 = (T)expression.Expr1.Accept(this, parameter);
            var e2 = (T)expression.Expr2.Accept(this, parameter);
            return e1.Equals(e2);
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
                var e2 = (T)expression.TrueExpr.Accept(this, parameter);
                var e3 = (T)expression.FalseExpr.Accept(this, parameter);
                return e1 ? e2 : e3;
            });
        }

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenLeqExpr<T>(ZenLeqExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)expression.Expr1.Accept(this, parameter) <= (byte)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)expression.Expr1.Accept(this, parameter) <= (short)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)expression.Expr1.Accept(this, parameter) <= (ushort)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) <= (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) <= (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) <= (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) <= (ulong)expression.Expr2.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit a GeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenGeqExpr<T>(ZenGeqExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)expression.Expr1.Accept(this, parameter) >= (byte)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)expression.Expr1.Accept(this, parameter) >= (short)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)expression.Expr1.Accept(this, parameter) >= (ushort)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) >= (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) >= (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) >= (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) >= (ulong)expression.Expr2.Accept(this, parameter);
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
        /// Visit a MinusExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenMinusExpr<T>(ZenMinusExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)((byte)expression.Expr1.Accept(this, parameter) - (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)((short)expression.Expr1.Accept(this, parameter) - (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)((ushort)expression.Expr1.Accept(this, parameter) - (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) - (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) - (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) - (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) - (ulong)expression.Expr2.Accept(this, parameter);
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
        /// Visit a SumExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenSumExpr<T>(ZenSumExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);

                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)((byte)expression.Expr1.Accept(this, parameter) + (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)((short)expression.Expr1.Accept(this, parameter) + (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)((ushort)expression.Expr1.Accept(this, parameter) + (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) + (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) + (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) + (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) + (ulong)expression.Expr2.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit a MultiplyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting C# value.</returns>
        public object VisitZenMultiplyExpr<T>(ZenMultiplyExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return LookupOrCompute(expression, parameter, () =>
            {
                var type = typeof(T);
                if (type == ReflectionUtilities.ByteType)
                {
                    return (byte)((byte)expression.Expr1.Accept(this, parameter) * (byte)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.ShortType)
                {
                    return (short)((short)expression.Expr1.Accept(this, parameter) * (short)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.UshortType)
                {
                    return (ushort)((ushort)expression.Expr1.Accept(this, parameter) * (ushort)expression.Expr2.Accept(this, parameter));
                }
                if (type == ReflectionUtilities.IntType)
                {
                    return (int)expression.Expr1.Accept(this, parameter) * (int)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.UintType)
                {
                    return (uint)expression.Expr1.Accept(this, parameter) * (uint)expression.Expr2.Accept(this, parameter);
                }
                if (type == ReflectionUtilities.LongType)
                {
                    return (long)expression.Expr1.Accept(this, parameter) * (long)expression.Expr2.Accept(this, parameter);
                }

                return (ulong)expression.Expr1.Accept(this, parameter) * (ulong)expression.Expr2.Accept(this, parameter);
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
