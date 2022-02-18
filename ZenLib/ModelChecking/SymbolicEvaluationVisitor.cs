// <copyright file="ModelChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
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

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> Evaluate<T>(Zen<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Accept(this, parameter);
            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenAndExpr(ZenAndExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr1, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr2, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.And(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenArbitraryExpr<T1>(ZenArbitraryExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var type = typeof(T1);

            if (type == ReflectionUtilities.BoolType)
            {
                var (variable, expr) = this.Solver.CreateBoolVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.ByteType)
            {
                var (variable, expr) = this.Solver.CreateByteVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
            {
                var (variable, expr) = this.Solver.CreateShortVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
            {
                var (variable, expr) = this.Solver.CreateIntVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
            {
                var (variable, expr) = this.Solver.CreateLongVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.BigIntType)
            {
                var (variable, expr) = this.Solver.CreateBigIntegerVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
            }
            else if (ReflectionUtilities.IsIDictType(type))
            {
                var (variable, expr) = this.Solver.CreateDictVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
            }
            else if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var size = CommonUtilities.IntegerSize(type);
                var (v, e) = this.Solver.CreateBitvecVar(expression, (uint)size);
                this.Variables.Add(v);
                this.ArbitraryVariables[expression] = v;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsSeqType(type));

                var (v, e) = this.Solver.CreateSeqVar(expression);
                this.Variables.Add(v);
                this.ArbitraryVariables[expression] = v;
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenArgumentExpr<T1>(ZenArgumentExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (parameter.ArgumentsToExpr.TryGetValue(expression.ArgumentId, out var expr))
            {
                try
                {
                    var innerType = expr.GetType().BaseType.GetGenericArgumentsCached()[0];
                    var evaluateMethod = typeof(SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
                        .GetMethodCached("Evaluate")
                        .MakeGenericMethod(innerType);
                    var result = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)evaluateMethod.Invoke(this, new object[] { expr, parameter });
                    this.Cache[expression] = result;
                    return result;
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            else
            {
                return parameter.ArgumentsToValue[expression.ArgumentId];
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenIntegerBinopExpr<T1>(ZenIntegerBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;

                switch (expression.Operation)
                {
                    case Op.Addition:
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                    case Op.Subtraction:
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                    default:
                        Contract.Assert(expression.Operation == Op.Multiplication);
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                }
            }
            else
            {
                Contract.Assert(e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>);

                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;

                switch (expression.Operation)
                {
                    case Op.BitwiseAnd:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseAnd(v1.Value, v2.Value));
                    case Op.BitwiseOr:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseOr(v1.Value, v2.Value));
                    case Op.BitwiseXor:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseXor(v1.Value, v2.Value));
                    case Op.Addition:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                    case Op.Subtraction:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                    default:
                        Contract.Assert(expression.Operation == Op.Multiplication);
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                }
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenBitwiseNotExpr<T1>(ZenBitwiseNotExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr, parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.BitwiseNot(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.BigIntType)
            {
                var bi = this.Solver.CreateBigIntegerConst((BigInteger)(object)expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bi);
            }
            else if (type == ReflectionUtilities.BoolType)
            {
                var b = (bool)(object)expression.Value ? this.Solver.True() : this.Solver.False();
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, b);
            }
            else if (type == ReflectionUtilities.ByteType)
            {
                var bv = this.Solver.CreateByteConst((byte)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.ShortType)
            {
                var bv = this.Solver.CreateShortConst((short)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UshortType)
            {
                var bv = this.Solver.CreateShortConst((short)(ushort)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.IntType)
            {
                var bv = this.Solver.CreateIntConst((int)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UintType)
            {
                var bv = this.Solver.CreateIntConst((int)(uint)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.LongType)
            {
                var bv = this.Solver.CreateLongConst((long)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UlongType)
            {
                var bv = this.Solver.CreateLongConst((long)(ulong)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type));
                var bv = this.Solver.CreateBitvecConst(((dynamic)expression.Value).GetBits());
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, bv);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            try
            {
                var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    var innerType = fieldValuePair.Value.GetType().BaseType.GetGenericArgumentsCached()[0];
                    var evaluateMethod = typeof(SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
                        .GetMethodCached("Evaluate")
                        .MakeGenericMethod(innerType);
                    var fieldValue = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)evaluateMethod.Invoke(this, new object[] { fieldValuePair.Value, parameter });
                    fields = fields.Add(field, fieldValue);
                }

                return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(typeof(TObject), this.Solver, fields);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr, parameter);
            return v.Fields[expression.FieldName];
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenIfExpr<T1>(ZenIfExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.GuardExpr, parameter);
            var vtrue = Evaluate(expression.TrueExpr, parameter);
            var vfalse = Evaluate(expression.FalseExpr, parameter);
            return vtrue.Merge(v.Value, vfalse);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenEqualityExpr<T>(ZenEqualityExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> b1 &&
                e2 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> b2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Iff(b1.Value, b2.Value));
            }
            else if (e1 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> s1 &&
                     e2 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> s2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(s1.Value, s2.Value));
            }
            else if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> bi1 &&
                     e2 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> bi2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(bi1.Value, bi2.Value));
            }
            else if (e1 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> di1 &&
                     e2 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> di2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(di1.Value, di2.Value));
            }
            else if (e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> i1 &&
                     e2 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> i2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(i1.Value, i2.Value));
            }
            else
            {
                Contract.Assert(e1 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>);
                Contract.Assert(e2 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>);
                var seq1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var seq2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Eq(seq1.Value, seq2.Value));
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenComparisonExpr<T1>(ZenIntegerComparisonExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver,
                    expression.ComparisonType == ComparisonType.Geq ?
                        this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                        this.Solver.LessThanOrEqual(v1.Value, v2.Value));
            }
            else
            {
                Contract.Assert(e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>);

                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)e2;
                var r = ReflectionUtilities.IsUnsignedIntegerType(typeof(T1)) ?
                        (expression.ComparisonType == ComparisonType.Geq ?
                            this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                            this.Solver.LessThanOrEqual(v1.Value, v2.Value)) :
                        (expression.ComparisonType == ComparisonType.Geq ?
                            this.Solver.GreaterThanOrEqualSigned(v1.Value, v2.Value) :
                            this.Solver.LessThanOrEqualSigned(v1.Value, v2.Value));
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, r);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenListAddFrontExpr<T1>(ZenListAddFrontExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr, parameter);
            var elt = Evaluate(expression.Element, parameter);

            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
            foreach (var kv in v.GuardedListGroup.Mapping)
            {
                var guard = kv.Value.Guard;
                var values = kv.Value.Values.Insert(0, elt);
                mapping = mapping.Add(kv.Key + 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(guard, values));
            }

            var listGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(mapping);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, listGroup);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenListEmptyExpr<T1>(ZenListEmptyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
            var list = ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty;
            mapping = mapping.Add(0, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver.True(), list));
            var guardedListGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(mapping);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, guardedListGroup);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var list = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.ListExpr, parameter);

            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> result = null;

            foreach (var kv in list.GuardedListGroup.Mapping)
            {
                var length = kv.Key;
                var guard = kv.Value.Guard;
                var values = kv.Value.Values;

                if (values.IsEmpty)
                {
                    var r = Evaluate(expression.EmptyCase, parameter);
                    result = Merge(guard, r, result);
                }
                else
                {
                    // split the symbolic list
                    var (hd, tl) = CommonUtilities.SplitHead(values);
                    var tlImmutable = (ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>)tl;

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
                    var r =  Evaluate(newExpression, newEnv);
                    result = Merge(guard, r, result);
                }
            }

            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenNotExpr(ZenNotExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Not(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenOrExpr(ZenOrExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr1, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr2, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.Or(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var o = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.Expr, parameter);
            var f = Evaluate(expression.FieldValue, parameter);
            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(typeof(T1), this.Solver, o.Fields.SetItem(expression.FieldName, f));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictEmptyExpr<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var emptyDict = this.Solver.DictEmpty(typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, emptyDict);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictSetExpr<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var e3 = Evaluate(expression.ValueExpr, parameter);
            var e = this.Solver.DictSet(e1.Value, e2, e3, typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictDeleteExpr<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var e = this.Solver.DictDelete(e1.Value, e2, typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictGetExpr<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var (flag, e) = this.Solver.DictGet(e1.Value, e2, typeof(TKey), typeof(TValue));

            var hasValue = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, flag);
            var optionValue = this.Solver.ConvertExprToSymbolicValue(e, typeof(TValue));

            var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>>.Empty
                .Add("HasValue", hasValue).Add("Value", optionValue);

            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(typeof(Option<TValue>), this.Solver, fields);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenDictCombineExpr<TKey>(ZenDictCombineExpr<TKey> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.DictExpr1, parameter);
            var e2 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.DictExpr2, parameter);

            TArray expr;
            switch (expression.CombinationType)
            {
                case ZenDictCombineExpr<TKey>.CombineType.Union:
                    expr = this.Solver.DictUnion(e1.Value, e2.Value);
                    break;
                default:
                    Contract.Assert(expression.CombinationType == ZenDictCombineExpr<TKey>.CombineType.Intersect);
                    expr = this.Solver.DictIntersect(e1.Value, e2.Value);
                    break;
            }

            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, expr);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqEmptyExpr<T>(ZenSeqEmptyExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var emptySeq = this.Solver.SeqEmpty(typeof(T));
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, emptySeq);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqUnitExpr<T>(ZenSeqUnitExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var e = Evaluate(expression.ValueExpr, parameter);
            var unitSeq = this.Solver.SeqUnit(e, typeof(T));
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, unitSeq);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqConcatExpr<T>(ZenSeqConcatExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr1, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr2, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqConcat(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqLengthExpr<T>(ZenSeqLengthExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqLength(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqAtExpr<T>(ZenSeqAtExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.IndexExpr, parameter);
            return this.Solver.SeqGet(v1.Value, typeof(T), v2.Value);
        }

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
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SubseqExpr, parameter);

            switch (expression.ContainmentType)
            {
                case SeqContainmentType.HasPrefix:
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqPrefixOf(v1.Value, v2.Value));
                case SeqContainmentType.HasSuffix:
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqSuffixOf(v1.Value, v2.Value));
                default:
                    Contract.Assert(expression.ContainmentType == SeqContainmentType.Contains);
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqContains(v1.Value, v2.Value));
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqIndexOfExpr<T>(ZenSeqIndexOfExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SubseqExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.OffsetExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqIndexOf(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqSliceExpr<T>(ZenSeqSliceExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.OffsetExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.LengthExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqSlice(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenSeqReplaceFirstExpr<T>(ZenSeqReplaceFirstExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SubseqExpr, parameter);
            var v3 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.ReplaceExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, this.Solver.SeqReplaceFirst(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> VisitZenCastExpr<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> parameter)
        {
            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                var e = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SourceExpr, parameter);
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e.Value);
            }
            else
            {
                Contract.Assert(typeof(TKey) == ReflectionUtilities.ByteSequenceType);

                var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)Evaluate(expression.SourceExpr, parameter);
                return new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, e.Value);
            }
        }
    }
}
