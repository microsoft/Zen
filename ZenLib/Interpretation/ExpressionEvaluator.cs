// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
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

        public object VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
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
        }

        public object VisitZenAndExpr(ZenAndExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            return (bool)expression.Expr1.Accept(this, parameter) && (bool)expression.Expr2.Accept(this, parameter);
        }

        public object VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.ArgumentId];
        }

        public object VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = expression.Expr1.Accept(this, parameter);
            var e2 = expression.Expr2.Accept(this, parameter);
            var type = typeof(T);

            object result;
            switch (expression.Operation)
            {
                case Op.BitwiseAnd:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        result = ((dynamic)e1).BitwiseAnd((dynamic)e2);
                    else
                        result = ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) & ReflectionUtilities.ToLong(e2));
                    break;

                case Op.BitwiseOr:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        result = ((dynamic)e1).BitwiseOr((dynamic)e2);
                    else
                        result = ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) | ReflectionUtilities.ToLong(e2));
                    break;

                case Op.BitwiseXor:
                    if (ReflectionUtilities.IsFixedIntegerType(type))
                        result = ((dynamic)e1).BitwiseXor((dynamic)e2);
                    else
                        result = ReflectionUtilities.Specialize<T>(ReflectionUtilities.ToLong(e1) ^ ReflectionUtilities.ToLong(e2));
                    break;

                case Op.Addition:
                    if (type == ReflectionUtilities.ByteType)
                        result = (byte)((byte)e1 + (byte)e2);
                    else if (type == ReflectionUtilities.ShortType)
                        result = (short)((short)e1 + (short)e2);
                    else if (type == ReflectionUtilities.UshortType)
                        result = (ushort)((ushort)e1 + (ushort)e2);
                    else if (type == ReflectionUtilities.IntType)
                        result = (int)e1 + (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        result = (uint)e1 + (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        result = (long)e1 + (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        result = (ulong)e1 + (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        result = (BigInteger)e1 + (BigInteger)e2;
                    else
                        result = ((dynamic)e1).Add((dynamic)e2);
                    break;

                case Op.Subtraction:
                    if (type == ReflectionUtilities.ByteType)
                        result = (byte)((byte)e1 - (byte)e2);
                    else if (type == ReflectionUtilities.ShortType)
                        result = (short)((short)e1 - (short)e2);
                    else if (type == ReflectionUtilities.UshortType)
                        result = (ushort)((ushort)e1 - (ushort)e2);
                    else if (type == ReflectionUtilities.IntType)
                        result = (int)e1 - (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        result = (uint)e1 - (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        result = (long)e1 - (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        result = (ulong)e1 - (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        result = (BigInteger)e1 - (BigInteger)e2;
                    else
                        result = ((dynamic)e1).Subtract((dynamic)e2);
                    break;

                default:
                    if (type == ReflectionUtilities.ByteType)
                        result = (byte)((byte)e1 * (byte)e2);
                    else if (type == ReflectionUtilities.ShortType)
                        result = (short)((short)e1 * (short)e2);
                    else if (type == ReflectionUtilities.UshortType)
                        result = (ushort)((ushort)e1 * (ushort)e2);
                    else if (type == ReflectionUtilities.IntType)
                        result = (int)e1 * (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        result = (uint)e1 * (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        result = (long)e1 * (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        result = (ulong)e1 * (ulong)e2;
                    else
                        result = (BigInteger)e1 * (BigInteger)e2;
                    break;
            }

            this.cache[expression] = result;
            return result;
        }

        public object VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = ReflectionUtilities.ToLong(expression.Expr.Accept(this, parameter));
            var result = ReflectionUtilities.Specialize<T>(~x);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return expression.Value;
        }

        public object VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var fieldNames = new List<string>();
            var parameters = new List<object>();
            foreach (var fieldValuePair in expression.Fields)
            {
                var field = fieldValuePair.Key;
                dynamic fieldValue = fieldValuePair.Value;
                var valueResult = fieldValue.Accept(this, parameter);
                fieldNames.Add(field);
                parameters.Add(valueResult);
            }

            var result = ReflectionUtilities.CreateInstance<TObject>(fieldNames.ToArray(), parameters.ToArray());
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            return ImmutableList<T>.Empty;
        }

        public object VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e = (T1)expression.Expr.Accept(this, parameter);
            var result = ReflectionUtilities.GetFieldOrProperty<T1, T2>(e, expression.FieldName);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenIfExpr<T>(ZenIfExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (bool)expression.GuardExpr.Accept(this, parameter);

            if (e1)
            {
                if (trackBranches)
                {
                    this.PathConstraint = this.PathConstraint.Add(expression.GuardExpr);
                }

                var result = (T)expression.TrueExpr.Accept(this, parameter);
                this.cache[expression] = result;
                return result;
            }
            else
            {
                if (trackBranches)
                {
                    this.PathConstraint = this.PathConstraint.Add(ZenNotExpr.Create(expression.GuardExpr));
                }

                var result = (T)expression.FalseExpr.Accept(this, parameter);
                this.cache[expression] = result;
                return result;
            }
        }

        public object VisitZenComparisonExpr<T>(ZenComparisonExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = expression.Expr1.Accept(this, parameter);
            var e2 = expression.Expr2.Accept(this, parameter);
            var type = typeof(T);

            object result;
            switch (expression.ComparisonType)
            {
                case ComparisonType.Geq:
                    if (type == ReflectionUtilities.ByteType)
                        result = (byte)e1 >= (byte)e2;
                    else if (type == ReflectionUtilities.ShortType)
                        result = (short)e1 >= (short)e2;
                    else if (type == ReflectionUtilities.UshortType)
                        result = (ushort)e1 >= (ushort)e2;
                    else if (type == ReflectionUtilities.IntType)
                        result = (int)e1 >= (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        result = (uint)e1 >= (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        result = (long)e1 >= (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        result = (ulong)e1 >= (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        result = (BigInteger)e1 >= (BigInteger)e2;
                    else
                        result = ((dynamic)e1) >= ((dynamic)e2);
                    break;

                case ComparisonType.Leq:
                    if (type == ReflectionUtilities.ByteType)
                        result = (byte)e1 <= (byte)e2;
                    else if (type == ReflectionUtilities.ShortType)
                        result = (short)e1 <= (short)e2;
                    else if (type == ReflectionUtilities.UshortType)
                        result = (ushort)e1 <= (ushort)e2;
                    else if (type == ReflectionUtilities.IntType)
                        result = (int)e1 <= (int)e2;
                    else if (type == ReflectionUtilities.UintType)
                        result = (uint)e1 <= (uint)e2;
                    else if (type == ReflectionUtilities.LongType)
                        result = (long)e1 <= (long)e2;
                    else if (type == ReflectionUtilities.UlongType)
                        result = (ulong)e1 <= (ulong)e2;
                    else if (type == ReflectionUtilities.BigIntType)
                        result = (BigInteger)e1 <= (BigInteger)e2;
                    else
                        result = ((dynamic)e1) <= ((dynamic)e2);
                    break;

                default:
                    result = ((T)e1).Equals((T)e2);
                    break;
            }

            this.cache[expression] = result;
            return result;
        }

        public object VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = CommonUtilities.ToImmutableList<T>(expression.Expr.Accept(this, parameter));
            var e2 = (T)expression.Element.Accept(this, parameter);
            var result = e1.Insert(0, e2);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenListCaseExpr<T, TResult>(ZenListCaseExpr<T, TResult> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e = CommonUtilities.ToImmutableList<T>(expression.ListExpr.Accept(this, parameter));

            if (e.Count == 0)
            {
                var result = expression.EmptyCase.Accept(this, parameter);
                this.cache[expression] = result;
                return result;
            }
            else
            {
                var (hd, tl) = CommonUtilities.SplitHead(e);
                var c = expression.ConsCase.Invoke(Constant(hd), Constant(tl));
                var result = (TResult)c.Accept(this, parameter);
                this.cache[expression] = result;
                return result;
            }
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
            return (string)expression.Expr1.Accept(this, parameter) + (string)expression.Expr2.Accept(this, parameter);
        }

        public object VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (string)expression.StringExpr.Accept(this, parameter);
            var e2 = (string)expression.SubstringExpr.Accept(this, parameter);

            object result;
            switch (expression.ContainmentType)
            {
                case ContainmentType.PrefixOf:
                    result = e1.StartsWith(e2);
                    break;
                case ContainmentType.SuffixOf:
                    result = e1.EndsWith(e2);
                    break;
                default:
                    result = e1.Contains(e2);
                    break;
            }

            this.cache[expression] = result;
            return result;
        }

        public object VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (string)expression.StringExpr.Accept(this, parameter);
            var e2 = (string)expression.SubstringExpr.Accept(this, parameter);
            var e3 = (string)expression.ReplaceExpr.Accept(this, parameter);
            var result = CommonUtilities.ReplaceFirst(e1, e2, e3);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (string)expression.StringExpr.Accept(this, parameter);
            var e2 = (BigInteger)expression.OffsetExpr.Accept(this, parameter);
            var e3 = (BigInteger)expression.LengthExpr.Accept(this, parameter);
            var result = CommonUtilities.Substring(e1, e2, e3);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenStringAtExpr(ZenStringAtExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (string)expression.StringExpr.Accept(this, parameter);
            var e2 = (BigInteger)expression.IndexExpr.Accept(this, parameter);
            var result = CommonUtilities.At(e1, e2);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenStringLengthExpr(ZenStringLengthExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e = (string)expression.Expr.Accept(this, parameter);
            var result = (BigInteger)e.Length;
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (string)expression.StringExpr.Accept(this, parameter);
            var e2 = (string)expression.SubstringExpr.Accept(this, parameter);
            var e3 = (BigInteger)expression.OffsetExpr.Accept(this, parameter);
            var result = CommonUtilities.IndexOf(e1, e2, e3);
            this.cache[expression] = result;
            return result;
        }

        public object VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ExpressionEvaluatorEnvironment parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (T1)expression.Expr.Accept(this, parameter);
            var e2 = (T2)expression.FieldValue.Accept(this, parameter);
            var result = ReflectionUtilities.WithField<T1>(e1, expression.FieldName, e2);
            this.cache[expression] = result;
            return result;
        }
    }
}
