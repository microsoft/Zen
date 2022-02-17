// <copyright file="ModelChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.Solver;

    /// <summary>
    /// Visitor that computes a symbolic representation for the function.
    /// </summary>
    internal sealed class SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>
        : IZenExprVisitor<SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>
    {
        /// <summary>
        /// Gets the solver.
        /// </summary>
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> Solver { get; }

        /// <summary>
        /// Gets the set of variables.
        /// </summary>
        public List<TVar> Variables { get; }

        /// <summary>
        /// Mapping from ZenArbitraryExpr to its allocated variable.
        /// </summary>
        public Dictionary<object, TVar> ArbitraryVariables { get; private set; }

        /// <summary>
        /// Cache of results to avoid the cost of common subexpressions.
        /// </summary>
        private Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>> Cache { get; } = new Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>();

        public SymbolicEvaluationVisitor(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> solver)
        {
            this.Solver = solver;
            this.Variables = new List<TVar>();
            this.ArbitraryVariables = new Dictionary<object, TVar>();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenAndExpr(ZenAndExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr1.Accept(this, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr2.Accept(this, parameter);
            var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.And(v1.Value, v2.Value));
            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenArbitraryExpr<T1>(ZenArbitraryExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var type = typeof(T1);

            if (type == ReflectionUtilities.BoolType)
            {
                var (variable, expr) = this.Solver.CreateBoolVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ByteType)
            {
                var (variable, expr) = this.Solver.CreateByteVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
            {
                var (variable, expr) = this.Solver.CreateShortVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
            {
                var (variable, expr) = this.Solver.CreateIntVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
            {
                var (variable, expr) = this.Solver.CreateLongVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.BigIntType)
            {
                var (variable, expr) = this.Solver.CreateBigIntegerVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (ReflectionUtilities.IsIDictType(type))
            {
                var (variable, expr) = this.Solver.CreateDictVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var size = CommonUtilities.IntegerSize(type);
                var (v, e) = this.Solver.CreateBitvecVar(expression, (uint)size);
                this.Variables.Add(v);
                var r = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);

                this.ArbitraryVariables[expression] = v;
                this.Cache[expression] = r;
                return r;
            }

            if (ReflectionUtilities.IsSeqType(type))
            {
                var (v, e) = this.Solver.CreateSeqVar(expression);
                this.Variables.Add(v);
                var r = new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);

                this.ArbitraryVariables[expression] = v;
                this.Cache[expression] = r;
                return r;
            }

            throw new ZenUnreachableException();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenArgumentExpr<T1>(ZenArgumentExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            if (parameter.ArgumentsToExpr.TryGetValue(expression.ArgumentId, out var expr))
            {
                try
                {
                    var acceptMethod = expr.GetType()
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(
                            typeof(SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>),
                            typeof(SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>));
                    var result = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)acceptMethod.Invoke(expr, new object[] { this, parameter });
                    this.Cache[expression] = result;
                    return result;
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            var res = parameter.ArgumentsToValue[expression.ArgumentId];
            this.Cache[expression] = res;
            return res;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenIntegerBinopExpr<T1>(ZenIntegerBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = expression.Expr1.Accept(this, parameter);
            var e2 = expression.Expr2.Accept(this, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;

                switch (expression.Operation)
                {
                    case Op.Addition:
                        var result1 = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                        this.Cache[expression] = result1;
                        return result1;
                    case Op.Subtraction:
                        var result2 = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                        this.Cache[expression] = result2;
                        return result2;
                    case Op.Multiplication:
                        var result3 = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                        this.Cache[expression] = result3;
                        return result3;
                    default:
                        throw new ZenUnreachableException();
                }
            }

            if (e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
            {
                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;

                switch (expression.Operation)
                {
                    case Op.BitwiseAnd:
                        var result1 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseAnd(v1.Value, v2.Value));
                        this.Cache[expression] = result1;
                        return result1;
                    case Op.BitwiseOr:
                        var result2 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseOr(v1.Value, v2.Value));
                        this.Cache[expression] = result2;
                        return result2;
                    case Op.BitwiseXor:
                        var result3 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseXor(v1.Value, v2.Value));
                        this.Cache[expression] = result3;
                        return result3;
                    case Op.Addition:
                        var result4 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                        this.Cache[expression] = result4;
                        return result4;
                    case Op.Subtraction:
                        var result5 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                        this.Cache[expression] = result5;
                        return result5;
                    case Op.Multiplication:
                        var result6 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                        this.Cache[expression] = result6;
                        return result6;
                    default:
                        throw new ZenUnreachableException();
                }
            }

            throw new ZenUnreachableException();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenBitwiseNotExpr<T1>(ZenBitwiseNotExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr.Accept(this, parameter);
            var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseNot(v.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var type = typeof(T);

            if (type == ReflectionUtilities.BigIntType)
            {
                var bi = this.Solver.CreateBigIntegerConst((BigInteger)(object)expression.Value);
                var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bi);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.BoolType)
            {
                var b = (bool)(object)expression.Value ? this.Solver.True() : this.Solver.False();
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, b);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ByteType)
            {
                var bv = this.Solver.CreateByteConst((byte)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ShortType)
            {
                var bv = this.Solver.CreateShortConst((short)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.UshortType)
            {
                var bv = this.Solver.CreateShortConst((short)(ushort)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.IntType)
            {
                var bv = this.Solver.CreateIntConst((int)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.UintType)
            {
                var bv = this.Solver.CreateIntConst((int)(uint)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.LongType)
            {
                var bv = this.Solver.CreateLongConst((long)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.UlongType)
            {
                var bv = this.Solver.CreateLongConst((long)(ulong)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var bv = this.Solver.CreateBitvecConst(((dynamic)expression.Value).GetBits());
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            throw new ZenUnreachableException();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            try
            {
                var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    var acceptMethod = fieldValuePair.Value.GetType()
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(
                            typeof(SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>),
                            typeof(SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>));

                    fields = fields.Add(field, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)acceptMethod.Invoke(fieldValuePair.Value, new object[] { this, parameter })); // fieldValue.Accept(this, parameter));
                }

                var result = new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(typeof(TObject), this.Solver, fields);

                this.Cache[expression] = result;
                return result;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr.Accept(this, parameter);
            var result = v.Fields[expression.FieldName];

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenIfExpr<T1>(ZenIfExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.GuardExpr.Accept(this, parameter);
            var vtrue = expression.TrueExpr.Accept(this, parameter);
            var vfalse = expression.FalseExpr.Accept(this, parameter);
            var result = vtrue.Merge(v.Value, vfalse);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenEqualityExpr<T>(ZenEqualityExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = expression.Expr1.Accept(this, parameter);
            var e2 = expression.Expr2.Accept(this, parameter);

            if (e1 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> b1 &&
                e2 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> b2)
            {
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Iff(b1.Value, b2.Value));
                this.Cache[expression] = result;
                return result;
            }

            if (e1 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> s1 &&
                e2 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> s2)
            {
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(s1.Value, s2.Value));
                this.Cache[expression] = result;
                return result;
            }

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> bi1 &&
                e2 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> bi2)
            {
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(bi1.Value, bi2.Value));
                this.Cache[expression] = result;
                return result;
            }

            if (e1 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> di1 &&
                e2 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> di2)
            {
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(di1.Value, di2.Value));
                this.Cache[expression] = result;
                return result;
            }

            if (e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> i1 &&
                e2 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> i2)
            {
                var res = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(i1.Value, i2.Value));
                this.Cache[expression] = res;
                return res;
            }

            if (e1 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> seq1 &&
                e2 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> seq2)
            {
                var res = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(seq1.Value, seq2.Value));
                this.Cache[expression] = res;
                return res;
            }

            throw new ZenUnreachableException();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenComparisonExpr<T1>(ZenIntegerComparisonExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = expression.Expr1.Accept(this, parameter);
            var e2 = expression.Expr2.Accept(this, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver,
                    expression.ComparisonType == ComparisonType.Geq ?
                        this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                        this.Solver.LessThanOrEqual(v1.Value, v2.Value));

                this.Cache[expression] = result;
                return result;
            }

            if (e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
            {
                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;
                var r = ReflectionUtilities.IsUnsignedIntegerType(typeof(T1)) ?
                        (expression.ComparisonType == ComparisonType.Geq ?
                            this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                            this.Solver.LessThanOrEqual(v1.Value, v2.Value)) :
                        (expression.ComparisonType == ComparisonType.Geq ?
                            this.Solver.GreaterThanOrEqualSigned(v1.Value, v2.Value) :
                            this.Solver.LessThanOrEqualSigned(v1.Value, v2.Value));
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, r);

                this.Cache[expression] = result;
                return result;
            }

            throw new ZenUnreachableException();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenListAddFrontExpr<T1>(ZenListAddFrontExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr.Accept(this, parameter);
            var elt = expression.Element.Accept(this, parameter);

            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
            foreach (var kv in v.GuardedListGroup.Mapping)
            {
                var guard = kv.Value.Guard;
                var values = kv.Value.Values.Insert(0, elt);
                mapping = mapping.Add(kv.Key + 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(guard, values));
            }

            var listGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(mapping);
            var result = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, listGroup);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenListEmptyExpr<T1>(ZenListEmptyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
            var list = ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
            mapping = mapping.Add(0, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver.True(), list));
            var guardedListGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(mapping);
            var result = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, guardedListGroup);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var list = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.ListExpr.Accept(this, parameter);

            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> result = null;

            foreach (var kv in list.GuardedListGroup.Mapping)
            {
                var length = kv.Key;
                var guard = kv.Value.Guard;
                var values = kv.Value.Values;

                if (values.IsEmpty)
                {
                    var r = expression.EmptyCase.Accept(this, parameter);
                    result = Merge(guard, r, result);
                }
                else
                {
                    // split the symbolic list
                    var (hd, tl) = CommonUtilities.SplitHead(values);
                    var tlImmutable = CommonUtilities.ToImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>(tl);

                    // push the guard into the tail of the list
                    var map = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
                    map = map.Add(length - 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver.True(), tlImmutable));
                    var group = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(map);
                    var rest = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, group);

                    // execute the cons case with placeholder values to get a new Zen value.
                    var arg1 = new ZenArgumentExpr<TList>();
                    var arg2 = new ZenArgumentExpr<IList<TList>>();
                    var args = parameter.ArgumentsToValue.Add(arg1.ArgumentId, hd).Add(arg2.ArgumentId, rest);
                    var newEnv = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(parameter.ArgumentsToExpr, args);
                    var newExpression = expression.ConsCase(arg1, arg2);

                    // model check the resulting value using the computed values for the placeholders.
                    var r =  newExpression.Accept(this, newEnv);
                    result = Merge(guard, r, result);
                }
            }

            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenNotExpr(ZenNotExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr.Accept(this, parameter);
            var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Not(v.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenOrExpr(ZenOrExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr1.Accept(this, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr2.Accept(this, parameter);
            var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Or(v1.Value, v2.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var o = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Expr.Accept(this, parameter);
            var f = expression.FieldValue.Accept(this, parameter);
            var result = new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(typeof(T1), this.Solver, o.Fields.SetItem(expression.FieldName, f));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictEmptyExpr<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var emptyDict = this.Solver.DictEmpty(typeof(TKey), typeof(TValue));
            var result = new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, emptyDict);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictSetExpr<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.DictExpr.Accept(this, parameter);
            var e2 = expression.KeyExpr.Accept(this, parameter);
            var e3 = expression.ValueExpr.Accept(this, parameter);
            var e = this.Solver.DictSet(e1.Value, e2, e3, typeof(TKey), typeof(TValue));
            var result = new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictDeleteExpr<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.DictExpr.Accept(this, parameter);
            var e2 = expression.KeyExpr.Accept(this, parameter);
            var e = this.Solver.DictDelete(e1.Value, e2, typeof(TKey), typeof(TValue));
            var result = new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictGetExpr<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.DictExpr.Accept(this, parameter);
            var e2 = expression.KeyExpr.Accept(this, parameter);
            var (flag, e) = this.Solver.DictGet(e1.Value, e2, typeof(TKey), typeof(TValue));

            var hasValue = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, flag);
            var optionValue = this.Solver.ConvertExprToSymbolicValue(e, typeof(TValue));

            var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty
                .Add("HasValue", hasValue).Add("Value", optionValue);

            var result = new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(typeof(Option<TValue>), this.Solver, fields);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictCombineExpr<TKey>(ZenDictCombineExpr<TKey> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.DictExpr1.Accept(this, parameter);
            var e2 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.DictExpr2.Accept(this, parameter);

            TArray expr;
            switch (expression.CombinationType)
            {
                case ZenDictCombineExpr<TKey>.CombineType.Union:
                    expr = this.Solver.DictUnion(e1.Value, e2.Value);
                    break;
                case ZenDictCombineExpr<TKey>.CombineType.Intersect:
                    expr = this.Solver.DictIntersect(e1.Value, e2.Value);
                    break;
                default:
                    throw new ZenUnreachableException();
            }

            var result = new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqEmptyExpr<T>(ZenSeqEmptyExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var emptySeq = this.Solver.SeqEmpty(typeof(T));
            var result = new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, emptySeq);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqUnitExpr<T>(ZenSeqUnitExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e = expression.ValueExpr.Accept(this, parameter);
            var unitSeq = this.Solver.SeqUnit(e, typeof(T));
            var result = new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, unitSeq);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqConcatExpr<T>(ZenSeqConcatExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr1.Accept(this, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr2.Accept(this, parameter);
            var result = new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqConcat(v1.Value, v2.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqLengthExpr<T>(ZenSeqLengthExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr.Accept(this, parameter);
            var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqLength(v.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqAtExpr<T>(ZenSeqAtExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr.Accept(this, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.IndexExpr.Accept(this, parameter);
            var result = this.Solver.SeqGet(v1.Value, typeof(T), v2.Value);

            this.Cache[expression] = result;
            return result;
        }

        [ExcludeFromCodeCoverage]
        private SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v1,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v2)
        {
            if (v2 == null)
            {
                return v1;
            }

            return v1.Merge(guard, v2);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqContainsExpr<T>(ZenSeqContainsExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr.Accept(this, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SubseqExpr.Accept(this, parameter);

            switch (expression.ContainmentType)
            {
                case SeqContainmentType.HasPrefix:
                    var r1 = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqPrefixOf(v1.Value, v2.Value));
                    this.Cache[expression] = r1;
                    return r1;
                case SeqContainmentType.HasSuffix:
                    var r2 = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqSuffixOf(v1.Value, v2.Value));
                    this.Cache[expression] = r2;
                    return r2;
                case SeqContainmentType.Contains:
                    var r3 = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqContains(v1.Value, v2.Value));
                    this.Cache[expression] = r3;
                    return r3;
                default:
                    throw new ZenUnreachableException();
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqIndexOfExpr<T>(ZenSeqIndexOfExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr.Accept(this, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SubseqExpr.Accept(this, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.OffsetExpr.Accept(this, parameter);
            var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqIndexOf(v1.Value, v2.Value, v3.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqSliceExpr<T>(ZenSeqSliceExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr.Accept(this, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.OffsetExpr.Accept(this, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.LengthExpr.Accept(this, parameter);
            var result = new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqSlice(v1.Value, v2.Value, v3.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqReplaceFirstExpr<T>(ZenSeqReplaceFirstExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SeqExpr.Accept(this, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SubseqExpr.Accept(this, parameter);
            var v3 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.ReplaceExpr.Accept(this, parameter);
            var result = new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqReplaceFirst(v1.Value, v2.Value, v3.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenCastExpr<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                var e = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SourceExpr.Accept(this, parameter);
                var result = new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e.Value);
                this.Cache[expression] = result;
                return result;
            }

            if (typeof(TKey) == ReflectionUtilities.ByteSequenceType)
            {
                var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.SourceExpr.Accept(this, parameter);
                var result = new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e.Value);
                this.Cache[expression] = result;
                return result;
            }

            throw new ZenUnreachableException();
        }
    }
}
