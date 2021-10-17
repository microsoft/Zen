// <copyright file="ModelChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.Solver;

    /// <summary>
    /// Visitor that computes a symbolic representation for the function.
    /// </summary>
    internal sealed class SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TString>
        : IZenExprVisitor<SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>>
    {
        /// <summary>
        /// Gets the decision diagram manager object.
        /// </summary>
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TString> Solver { get; }

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
        private Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>> Cache { get; } = new Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>>();

        public SymbolicEvaluationVisitor(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString> solver)
        {
            this.Solver = solver;
            this.Variables = new List<TVar>();
            this.ArbitraryVariables = new Dictionary<object, TVar>();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenAndExpr(ZenAndExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr1.Accept(this, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr2.Accept(this, parameter);
            var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.And(v1.Value, v2.Value));
            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenArbitraryExpr<T1>(ZenArbitraryExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
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
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ByteType)
            {
                var (variable, expr) = this.Solver.CreateByteVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
            {
                var (variable, expr) = this.Solver.CreateShortVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
            {
                var (variable, expr) = this.Solver.CreateIntVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.StringType)
            {
                var (variable, expr) = this.Solver.CreateStringVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
            {
                var (variable, expr) = this.Solver.CreateLongVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.BigIntType)
            {
                var (variable, expr) = this.Solver.CreateBigIntegerVar(expression);
                this.Variables.Add(variable);
                var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, expr);
                this.ArbitraryVariables[expression] = variable;

                this.Cache[expression] = result;
                return result;
            }

            // Fixed width integer case
            var size = CommonUtilities.IntegerSize(type);
            var (v, e) = this.Solver.CreateBitvecVar(expression, (uint)size);
            this.Variables.Add(v);
            var r = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, e);

            this.ArbitraryVariables[expression] = v;
            this.Cache[expression] = r;
            return r;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenArgumentExpr<T1>(ZenArgumentExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
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
                            typeof(SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString>),
                            typeof(SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>));
                    var result = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>)acceptMethod.Invoke(expr, new object[] { this, parameter });
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

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenIntegerBinopExpr<T1>(ZenIntegerBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = expression.Expr1.Accept(this, parameter);
            var e2 = expression.Expr2.Accept(this, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)e2;

                switch (expression.Operation)
                {
                    case Op.Addition:
                        var result1 = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                        this.Cache[expression] = result1;
                        return result1;
                    case Op.Subtraction:
                        var result2 = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                        this.Cache[expression] = result2;
                        return result2;
                    default:
                        var result3 = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                        this.Cache[expression] = result3;
                        return result3;
                }
            }
            else
            {
                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)e2;

                switch (expression.Operation)
                {
                    case Op.BitwiseAnd:
                        var result1 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.BitwiseAnd(v1.Value, v2.Value));
                        this.Cache[expression] = result1;
                        return result1;
                    case Op.BitwiseOr:
                        var result2 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.BitwiseOr(v1.Value, v2.Value));
                        this.Cache[expression] = result2;
                        return result2;
                    case Op.BitwiseXor:
                        var result3 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.BitwiseXor(v1.Value, v2.Value));
                        this.Cache[expression] = result3;
                        return result3;
                    case Op.Addition:
                        var result4 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                        this.Cache[expression] = result4;
                        return result4;
                    case Op.Subtraction:
                        var result5 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                        this.Cache[expression] = result5;
                        return result5;
                    default:
                        var result6 = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                        this.Cache[expression] = result6;
                        return result6;
                }
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenBitwiseNotExpr<T1>(ZenBitwiseNotExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr.Accept(this, parameter);
            var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.BitwiseNot(v.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var type = typeof(T);

            if (type == ReflectionUtilities.BigIntType)
            {
                var bi = this.Solver.CreateBigIntegerConst((BigInteger)(object)expression.Value);
                var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bi);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.BoolType)
            {
                var b = (bool)(object)expression.Value ? this.Solver.True() : this.Solver.False();
                var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, b);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ByteType)
            {
                var bv = this.Solver.CreateByteConst((byte)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.ShortType)
            {
                var bv = this.Solver.CreateShortConst((short)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.UshortType)
            {
                var bv = this.Solver.CreateShortConst((short)(ushort)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.IntType)
            {
                var bv = this.Solver.CreateIntConst((int)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.UintType)
            {
                var bv = this.Solver.CreateIntConst((int)(uint)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.LongType)
            {
                var bv = this.Solver.CreateLongConst((long)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (type == ReflectionUtilities.UlongType)
            {
                var bv = this.Solver.CreateLongConst((long)(ulong)(object)expression.Value);
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var bv = this.Solver.CreateBitvecConst(((dynamic)expression.Value).GetBits());
                var result = new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, bv);
                this.Cache[expression] = result;
                return result;
            }

            // string type.
            var s = CommonUtilities.ConvertCSharpStringToZ3((string)(object)expression.Value);
            var v = this.Solver.CreateStringConst(s);
            var r = new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, v);
            this.Cache[expression] = r;
            return r;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            try
            {
                var fields = ImmutableDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>>.Empty;
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    var acceptMethod = fieldValuePair.Value.GetType()
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(
                            typeof(SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString>),
                            typeof(SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>));

                    fields = fields.Add(field, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>)acceptMethod.Invoke(fieldValuePair.Value, new object[] { this, parameter })); // fieldValue.Accept(this, parameter));
                }

                var result = new SymbolicClass<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, fields);

                this.Cache[expression] = result;
                return result;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicClass<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr.Accept(this, parameter);
            var result = v.Fields[expression.FieldName];

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenIfExpr<T1>(ZenIfExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.GuardExpr.Accept(this, parameter);
            var vtrue = expression.TrueExpr.Accept(this, parameter);
            var vfalse = expression.FalseExpr.Accept(this, parameter);
            var result = vtrue.Merge(v.Value, vfalse);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenComparisonExpr<T1>(ZenComparisonExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var e1 = expression.Expr1.Accept(this, parameter);
            var e2 = expression.Expr2.Accept(this, parameter);

            switch (expression.ComparisonType)
            {
                case ComparisonType.Geq:
                case ComparisonType.Leq:
                    if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)
                    {
                        var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)e1;
                        var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)e2;
                        var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver,
                            expression.ComparisonType == ComparisonType.Geq ?
                                this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                                this.Solver.LessThanOrEqual(v1.Value, v2.Value));

                        this.Cache[expression] = result;
                        return result;
                    }
                    else
                    {
                        var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)e1;
                        var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)e2;
                        var r = ReflectionUtilities.IsUnsignedIntegerType(typeof(T1)) ?
                                (expression.ComparisonType == ComparisonType.Geq ?
                                    this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                                    this.Solver.LessThanOrEqual(v1.Value, v2.Value)) :
                                (expression.ComparisonType == ComparisonType.Geq ?
                                    this.Solver.GreaterThanOrEqualSigned(v1.Value, v2.Value) :
                                    this.Solver.LessThanOrEqualSigned(v1.Value, v2.Value));
                        var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, r);

                        this.Cache[expression] = result;
                        return result;
                    }

                default:
                    if (e1 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString> b1 && e2 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString> b2)
                    {
                        var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Iff(b1.Value, b2.Value));
                        this.Cache[expression] = result;
                        return result;
                    }

                    if (e1 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString> s1 && e2 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString> s2)
                    {
                        var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Eq(s1.Value, s2.Value));
                        this.Cache[expression] = result;
                        return result;
                    }

                    if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString> bi1 && e2 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString> bi2)
                    {
                        var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Eq(bi1.Value, bi2.Value));
                        this.Cache[expression] = result;
                        return result;
                    }

                    var i1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)e1;
                    var i2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)e2;
                    var res = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Eq(i1.Value, i2.Value));
                    this.Cache[expression] = res;
                    return res;
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenListAddFrontExpr<T1>(ZenListAddFrontExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr.Accept(this, parameter);
            var elt = expression.Element.Accept(this, parameter);

            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TString>>.Empty;
            foreach (var kv in v.GuardedListGroup.Mapping)
            {
                var guard = kv.Value.Guard;
                var values = kv.Value.Values.Insert(0, elt);
                mapping = mapping.Add(kv.Key + 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TString>(guard, values));
            }

            var listGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TString>(mapping);
            var result = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, listGroup);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenListEmptyExpr<T1>(ZenListEmptyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TString>>.Empty;
            var list = ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>>.Empty;
            mapping = mapping.Add(0, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver.True(), list));
            var guardedListGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TString>(mapping);
            var result = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, guardedListGroup);

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            var list = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.ListExpr.Accept(this, parameter);

            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> result = null;

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
                    var tlImmutable = CommonUtilities.ToImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>>(tl);

                    // push the guard into the tail of the list
                    var map = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TString>>.Empty;
                    map = map.Add(length - 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver.True(), tlImmutable));
                    var group = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TString>(map);
                    var rest = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, group);

                    // execute the cons case with placeholder values to get a new Zen value.
                    var arg1 = new ZenArgumentExpr<TList>();
                    var arg2 = new ZenArgumentExpr<IList<TList>>();
                    var args = parameter.ArgumentsToValue.Add(arg1.ArgumentId, hd).Add(arg2.ArgumentId, rest);
                    var newEnv = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString>(parameter.ArgumentsToExpr, args);
                    var newExpression = expression.ConsCase(arg1, arg2);

                    // model check the resulting value using the computed values for the placeholders.
                    var r =  newExpression.Accept(this, newEnv);
                    result = Merge(guard, r, result);
                }
            }

            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenNotExpr(ZenNotExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr.Accept(this, parameter);
            var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Not(v.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenOrExpr(ZenOrExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr1.Accept(this, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr2.Accept(this, parameter);
            var result = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Or(v1.Value, v2.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenConcatExpr(ZenConcatExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr1.Accept(this, parameter);
            var v2 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr2.Accept(this, parameter);
            var result = new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Concat(v1.Value, v2.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.StringExpr.Accept(this, parameter);
            var v2 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.SubstringExpr.Accept(this, parameter);

            switch (expression.ContainmentType)
            {
                case ContainmentType.PrefixOf:
                    var r1 = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.PrefixOf(v1.Value, v2.Value));
                    this.Cache[expression] = r1;
                    return r1;
                case ContainmentType.SuffixOf:
                    var r2 = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.SuffixOf(v1.Value, v2.Value));
                    this.Cache[expression] = r2;
                    return r2;
                default:
                    var r3 = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Contains(v1.Value, v2.Value));
                    this.Cache[expression] = r3;
                    return r3;
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.StringExpr.Accept(this, parameter);
            var v2 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.SubstringExpr.Accept(this, parameter);
            var v3 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.ReplaceExpr.Accept(this, parameter);
            var result = new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.ReplaceFirst(v1.Value, v2.Value, v3.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.StringExpr.Accept(this, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.OffsetExpr.Accept(this, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.LengthExpr.Accept(this, parameter);
            var result = new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Substring(v1.Value, v2.Value, v3.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenStringAtExpr(ZenStringAtExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.StringExpr.Accept(this, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.IndexExpr.Accept(this, parameter);
            var result = new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.At(v1.Value, v2.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenStringLengthExpr(ZenStringLengthExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr.Accept(this, parameter);
            var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.Length(v.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.StringExpr.Accept(this, parameter);
            var v2 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.SubstringExpr.Accept(this, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.OffsetExpr.Accept(this, parameter);
            var result = new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, this.Solver.IndexOf(v1.Value, v2.Value, v3.Value));

            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var o = (SymbolicClass<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Expr.Accept(this, parameter);
            var f = expression.FieldValue.Accept(this, parameter);
            var result = new SymbolicClass<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, o.Fields.SetItem(expression.FieldName, f));

            this.Cache[expression] = result;
            return result;
        }

        [ExcludeFromCodeCoverage]
        private SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> v1,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> v2)
        {
            if (v2 == null)
            {
                return v1;
            }

            return v1.Merge(guard, v2);
        }
    }
}
