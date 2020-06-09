// <copyright file="ModelChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Visitor that computes a symbolic representation for the function.
    /// </summary>
    internal sealed class SymbolicEvaluationVisitor<TModel, TVar, TBool, TInt, TString>
        : IZenExprVisitor<SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString>, SymbolicValue<TModel, TVar, TBool, TInt, TString>>
    {
        /// <summary>
        /// Gets the decision diagram manager object.
        /// </summary>
        public ISolver<TModel, TVar, TBool, TInt, TString> Solver { get; }

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
        private Dictionary<object, SymbolicValue<TModel, TVar, TBool, TInt, TString>> Cache { get; } = new Dictionary<object, SymbolicValue<TModel, TVar, TBool, TInt, TString>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicEvaluationVisitor{TModel, TVar, TBool, TInt, TString}"/> class.
        /// </summary>
        /// <param name="solver">The manager object.</param>
        public SymbolicEvaluationVisitor(ISolver<TModel, TVar, TBool, TInt, TString> solver)
        {
            this.Solver = solver;
            this.Variables = new List<TVar>();
            this.ArbitraryVariables = new Dictionary<object, TVar>();
        }

        private SymbolicValue<TModel, TVar, TBool, TInt, TString> LookupOrCompute(object expression, Func<SymbolicValue<TModel, TVar, TBool, TInt, TString>> callback)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = callback();

            this.Cache[expression] = result;
            return result;
        }

        /// <summary>
        /// Visit an AdapterExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenAdapterExpr<TTo, TFrom>(ZenAdapterExpr<TTo, TFrom> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        /// <summary>
        /// Visit an AndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenAndExpr(ZenAndExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicBool<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicBool<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.And(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit an ArbitraryExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenArbitraryExpr<T1>(ZenArbitraryExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var type = typeof(T1);

                if (type == ReflectionUtilities.BoolType)
                {
                    var (variable, expr) = this.Solver.CreateBoolVar(expression);
                    this.Variables.Add(variable);
                    var result = new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, expr);
                    this.ArbitraryVariables[expression] = variable;
                    return result;
                }

                if (type == ReflectionUtilities.ByteType)
                {
                    var (variable, expr) = this.Solver.CreateByteVar(expression);
                    this.Variables.Add(variable);
                    var result = new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, expr);
                    this.ArbitraryVariables[expression] = variable;
                    return result;
                }

                if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
                {
                    var (variable, expr) = this.Solver.CreateShortVar(expression);
                    this.Variables.Add(variable);
                    var result = new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, expr);
                    this.ArbitraryVariables[expression] = variable;
                    return result;
                }

                if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
                {
                    var (variable, expr) = this.Solver.CreateIntVar(expression);
                    this.Variables.Add(variable);
                    var result = new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, expr);
                    this.ArbitraryVariables[expression] = variable;
                    return result;
                }

                // long or ulong
                var (v, e) = this.Solver.CreateLongVar(expression);
                this.Variables.Add(v);
                var r = new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, e);
                this.ArbitraryVariables[expression] = v;
                return r;
            });
        }

        /// <summary>
        /// Visit an ArgumentExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenArgumentExpr<T1>(ZenArgumentExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return parameter.ArgumentAssignment[expression.Id];
            });
        }

        /// <summary>
        /// Visit a BitwiseAndExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenBitwiseAndExpr<T1>(ZenBitwiseAndExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.BitwiseAnd(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit a BitwiseNotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenBitwiseNotExpr<T1>(ZenBitwiseNotExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr.Accept(this, parameter);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.BitwiseNot(v.Value));
            });
        }

        /// <summary>
        /// Visit a BitwiseOrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenBitwiseOrExpr<T1>(ZenBitwiseOrExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.BitwiseOr(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit a BitwiseXorExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenBitwiseXorExpr<T1>(ZenBitwiseXorExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.BitwiseXor(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit a ConstantBoolExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var b = expression.Value ? this.Solver.True() : this.Solver.False();
                return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, b);
            });
        }

        /// <summary>
        /// Visit a ConstantByteExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantByteExpr(ZenConstantByteExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var bv = this.Solver.CreateByteConst(expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, bv);
            });
        }

        /// <summary>
        /// Visit a ConstantIntExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantIntExpr(ZenConstantIntExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var bv = this.Solver.CreateIntConst(expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, bv);
            });
        }

        /// <summary>
        /// Visit a ConstantLongExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantLongExpr(ZenConstantLongExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var bv = this.Solver.CreateLongConst(expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, bv);
            });
        }

        /// <summary>
        /// Visit a ConstantShortExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantShortExpr(ZenConstantShortExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var bv = this.Solver.CreateShortConst(expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, bv);
            });
        }

        /// <summary>
        /// Visit a ConstantUintExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantUintExpr(ZenConstantUintExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var bv = this.Solver.CreateIntConst((int)expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, bv);
            });
        }

        /// <summary>
        /// Visit a ConstantUlongExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var bv = this.Solver.CreateLongConst((long)expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, bv);
            });
        }

        /// <summary>
        /// Visit a ConstantUshortExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var bv = this.Solver.CreateShortConst((short)expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, bv);
            });
        }

        /// <summary>
        /// Visit a CreateObjectExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var fields = ImmutableDictionary<string, SymbolicValue<TModel, TVar, TBool, TInt, TString>>.Empty;
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    dynamic fieldValue = fieldValuePair.Value;
                    fields = fields.Add(field, fieldValue.Accept(this, parameter));
                }

                return new SymbolicClass<TModel, TVar, TBool, TInt, TString>(this.Solver, fields);
            });
        }

        /// <summary>
        /// Visit an EqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenEqExpr<T1>(ZenEqExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = expression.Expr1.Accept(this, parameter);
                var v2 = expression.Expr2.Accept(this, parameter);

                if (v1 is SymbolicBool<TModel, TVar, TBool, TInt, TString> b1 && v2 is SymbolicBool<TModel, TVar, TBool, TInt, TString> b2)
                {
                    return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.Iff(b1.Value, b2.Value));
                }

                var i1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)v1;
                var i2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)v2;
                return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.Eq(i1.Value, i2.Value));
            });
        }

        /// <summary>
        /// Visit a GetFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v = (SymbolicClass<TModel, TVar, TBool, TInt, TString>)expression.Expr.Accept(this, parameter);
                return v.Fields[expression.FieldName];
            });
        }

        /// <summary>
        /// Visit an IfExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenIfExpr<T1>(ZenIfExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v = (SymbolicBool<TModel, TVar, TBool, TInt, TString>)expression.GuardExpr.Accept(this, parameter);
                var vtrue = expression.TrueExpr.Accept(this, parameter);
                var vfalse = expression.FalseExpr.Accept(this, parameter);
                return vtrue.Merge(v.Value, vfalse);
            });
        }

        /// <summary>
        /// Visit a LeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenLeqExpr<T1>(ZenLeqExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                var result =
                    ReflectionUtilities.IsUnsignedIntegerType(typeof(T1)) ?
                    this.Solver.LessThanOrEqual(v1.Value, v2.Value) :
                    this.Solver.LessThanOrEqualSigned(v1.Value, v2.Value);
                return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, result);
            });
        }

        /// <summary>
        /// Visit a GeqExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenGeqExpr<T1>(ZenGeqExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                var result =
                    ReflectionUtilities.IsUnsignedIntegerType(typeof(T1)) ?
                    this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                    this.Solver.GreaterThanOrEqualSigned(v1.Value, v2.Value);
                return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, result);
            });
        }

        /// <summary>
        /// Visit a ListAddFrontExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenListAddFrontExpr<T1>(ZenListAddFrontExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v = (SymbolicList<TModel, TVar, TBool, TInt, TString>)expression.Expr.Accept(this, parameter);
                var elt = expression.Element.Accept(this, parameter);

                var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TInt, TString>>.Empty;
                foreach (var kv in v.GuardedListGroup.Mapping)
                {
                    var guard = kv.Value.Guard;
                    var values = kv.Value.Values.Insert(0, elt);
                    mapping = mapping.Add(kv.Key + 1, new GuardedList<TModel, TVar, TBool, TInt, TString>(guard, values));
                }

                var listGroup = new GuardedListGroup<TModel, TVar, TBool, TInt, TString>(mapping);
                return new SymbolicList<TModel, TVar, TBool, TInt, TString>(this.Solver, listGroup);
            });
        }

        /// <summary>
        /// Visit a ListEmptyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenListEmptyExpr<T1>(ZenListEmptyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TInt, TString>>.Empty;
                var list = ImmutableList<SymbolicValue<TModel, TVar, TBool, TInt, TString>>.Empty;
                mapping = mapping.Add(0, new GuardedList<TModel, TVar, TBool, TInt, TString>(this.Solver.True(), list));
                var guardedListGroup = new GuardedListGroup<TModel, TVar, TBool, TInt, TString>(mapping);
                return new SymbolicList<TModel, TVar, TBool, TInt, TString>(this.Solver, guardedListGroup);
            });
        }

        /// <summary>
        /// Visit a ListMatchExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            var list = (SymbolicList<TModel, TVar, TBool, TInt, TString>)expression.ListExpr.Accept(this, parameter);

            SymbolicValue<TModel, TVar, TBool, TInt, TString> result = null;

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
                    var tlImmutable = CommonUtilities.ToImmutableList<SymbolicValue<TModel, TVar, TBool, TInt, TString>>(tl);

                    // push the guard into the tail of the list
                    var map = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TInt, TString>>.Empty;
                    map = map.Add(length - 1, new GuardedList<TModel, TVar, TBool, TInt, TString>(this.Solver.True(), tlImmutable));
                    var group = new GuardedListGroup<TModel, TVar, TBool, TInt, TString>(map);
                    var rest = new SymbolicList<TModel, TVar, TBool, TInt, TString>(this.Solver, group);

                    // execute the cons case with placeholder values to get a new Zen value.
                    var arg1 = new ZenArgumentExpr<TList>();
                    var arg2 = new ZenArgumentExpr<IList<TList>>();
                    var args = parameter.ArgumentAssignment.Add(arg1.Id, hd).Add(arg2.Id, rest);
                    var newEnv = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString>(args);
                    var newExpression = expression.ConsCase(arg1, arg2);

                    // model check the resulting value using the computed values for the placeholders.
                    var r =  newExpression.Accept(this, newEnv);
                    result = Merge(guard, r, result);
                }
            }

            return result;
        }

        [ExcludeFromCodeCoverage]
        private SymbolicValue<TModel, TVar, TBool, TInt, TString> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TInt, TString> v1,
            SymbolicValue<TModel, TVar, TBool, TInt, TString> v2)
        {
            if (v2 == null)
            {
                return v1;
            }

            return v1.Merge(guard, v2);
        }

        /// <summary>
        /// Visit a MinusExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenMinusExpr<T1>(ZenMinusExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit a MultiplyExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenMultiplyExpr<T1>(ZenMultiplyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit a NotExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenNotExpr(ZenNotExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v = (SymbolicBool<TModel, TVar, TBool, TInt, TString>)expression.Expr.Accept(this, parameter);
                return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.Not(v.Value));
            });
        }

        /// <summary>
        /// Visit an OrExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenOrExpr(ZenOrExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicBool<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicBool<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicBool<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.Or(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit a SumExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenSumExpr<T1>(ZenSumExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr1.Accept(this, parameter);
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)expression.Expr2.Accept(this, parameter);
                return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
            });
        }

        /// <summary>
        /// Visit a WithFieldExpr.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The resulting symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TInt, TString> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt, TString> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var o = (SymbolicClass<TModel, TVar, TBool, TInt, TString>)expression.Expr.Accept(this, parameter);
                var f = expression.FieldValue.Accept(this, parameter);
                return new SymbolicClass<TModel, TVar, TBool, TInt, TString>(this.Solver, o.Fields.SetItem(expression.FieldName, f));
            });
        }
    }
}
