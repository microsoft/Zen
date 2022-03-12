// <copyright file="ModelChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.Solver;

    /// <summary>
    /// Visitor that computes a symbolic representation for the function.
    /// </summary>
    internal sealed class SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>
        : IZenExprVisitor<SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>
    {
        /// <summary>
        /// Evaluate symbolic method reference.
        /// </summary>
        private static MethodInfo evaluateMethod = typeof(SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>).GetMethodCached("Evaluate");

        /// <summary>
        /// Gets the solver.
        /// </summary>
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Solver { get; }

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
        private Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>> Cache { get; } = new Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>();

        public SymbolicEvaluationVisitor(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> solver)
        {
            this.Solver = solver;
            this.Variables = new List<TVar>();
            this.ArbitraryVariables = new Dictionary<object, TVar>();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Evaluate<T>(Zen<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Accept(this, parameter);
            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit(ZenAndExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr1, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr2, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.And(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenArbitraryExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var type = typeof(T1);

            if (type == ReflectionUtilities.BoolType)
            {
                var (variable, expr) = this.Solver.CreateBoolVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.ByteType)
            {
                var (variable, expr) = this.Solver.CreateByteVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.CharType)
            {
                var (variable, expr) = this.Solver.CreateCharVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
            {
                var (variable, expr) = this.Solver.CreateShortVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
            {
                var (variable, expr) = this.Solver.CreateIntVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
            {
                var (variable, expr) = this.Solver.CreateLongVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.BigIntType)
            {
                var (variable, expr) = this.Solver.CreateBigIntegerVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (ReflectionUtilities.IsMapType(type))
            {
                var (variable, expr) = this.Solver.CreateDictVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
            }
            else if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var size = CommonUtilities.IntegerSize(type);
                var (v, e) = this.Solver.CreateBitvecVar(expression, (uint)size);
                this.Variables.Add(v);
                this.ArbitraryVariables[expression] = v;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, e);
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsSeqType(type));

                var (v, e) = this.Solver.CreateSeqVar(expression);
                this.Variables.Add(v);
                this.ArbitraryVariables[expression] = v;
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, e);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenArgumentExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            if (parameter.ArgumentsToExpr.TryGetValue(expression.ArgumentId, out var expr))
            {
                try
                {
                    var innerType = expr.GetType().BaseType.GetGenericArgumentsCached()[0];
                    var method = evaluateMethod.MakeGenericMethod(innerType);
                    var result = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)method.Invoke(this, new object[] { expr, parameter });
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

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenIntegerBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e2;

                switch (expression.Operation)
                {
                    case Op.Addition:
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                    case Op.Subtraction:
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                    default:
                        Contract.Assert(expression.Operation == Op.Multiplication);
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                }
            }
            else
            {
                Contract.Assert(e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>);

                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e2;

                switch (expression.Operation)
                {
                    case Op.BitwiseAnd:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.BitwiseAnd(v1.Value, v2.Value));
                    case Op.BitwiseOr:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.BitwiseOr(v1.Value, v2.Value));
                    case Op.BitwiseXor:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.BitwiseXor(v1.Value, v2.Value));
                    case Op.Addition:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                    case Op.Subtraction:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                    default:
                        Contract.Assert(expression.Operation == Op.Multiplication);
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                }
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenBitwiseNotExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr, parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.BitwiseNot(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenConstantExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.BigIntType)
            {
                var bi = this.Solver.CreateBigIntegerConst((BigInteger)(object)expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bi);
            }
            else if (type == ReflectionUtilities.BoolType)
            {
                var b = (bool)(object)expression.Value ? this.Solver.True() : this.Solver.False();
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, b);
            }
            else if (type == ReflectionUtilities.ByteType)
            {
                var bv = this.Solver.CreateByteConst((byte)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.CharType)
            {
                var c = this.Solver.CreateCharConst((ZenLib.Char)(object)expression.Value);
                return new SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, c);
            }
            else if (type == ReflectionUtilities.ShortType)
            {
                var bv = this.Solver.CreateShortConst((short)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UshortType)
            {
                var bv = this.Solver.CreateShortConst((short)(ushort)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.IntType)
            {
                var bv = this.Solver.CreateIntConst((int)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UintType)
            {
                var bv = this.Solver.CreateIntConst((int)(uint)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.LongType)
            {
                var bv = this.Solver.CreateLongConst((long)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UlongType)
            {
                var bv = this.Solver.CreateLongConst((long)(ulong)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var bv = this.Solver.CreateBitvecConst(((dynamic)expression.Value).GetBits());
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, bv);
            }
            else
            {
                Contract.Assert(type == typeof(Seq<ZenLib.Char>));
                var escapedString = CommonUtilities.ConvertCShaprStringToZ3((Seq<ZenLib.Char>)(object)expression.Value);
                var s = this.Solver.CreateStringConst(escapedString);
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, s);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TObject>(ZenCreateObjectExpr<TObject> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            try
            {
                var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>.Empty;
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    var innerType = fieldValuePair.Value.GetType().BaseType.GetGenericArgumentsCached()[0];
                    var method = evaluateMethod.MakeGenericMethod(innerType);
                    var fieldValue = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)method.Invoke(this, new object[] { fieldValuePair.Value, parameter });
                    fields = fields.Add(field, fieldValue);
                }

                return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(typeof(TObject), this.Solver, fields);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1, T2>(ZenGetFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr, parameter);
            return v.Fields[expression.FieldName];
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenIfExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.GuardExpr, parameter);
            var vtrue = Evaluate(expression.TrueExpr, parameter);
            var vfalse = Evaluate(expression.FalseExpr, parameter);
            return vtrue.Merge(v.Value, vfalse);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenEqualityExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> b1 &&
                e2 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> b2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Iff(b1.Value, b2.Value));
            }
            else if (e1 is SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> c1 &&
                     e2 is SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> c2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Eq(c1.Value, c2.Value));
            }
            else if (e1 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> s1 &&
                     e2 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> s2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Eq(s1.Value, s2.Value));
            }
            else if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> bi1 &&
                     e2 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> bi2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Eq(bi1.Value, bi2.Value));
            }
            else if (e1 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> di1 &&
                     e2 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> di2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Eq(di1.Value, di2.Value));
            }
            else if (e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> i1 &&
                     e2 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> i2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Eq(i1.Value, i2.Value));
            }
            else
            {
                Contract.Assert(e1 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>);
                Contract.Assert(e2 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>);
                var seq1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e1;
                var seq2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e2;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Eq(seq1.Value, seq2.Value));
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenIntegerComparisonExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e2;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver,
                    expression.ComparisonType == ComparisonType.Geq ?
                        this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                        this.Solver.LessThanOrEqual(v1.Value, v2.Value));
            }
            else
            {
                Contract.Assert(e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>);

                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)e2;
                var r = ReflectionUtilities.IsUnsignedIntegerType(typeof(T1)) ?
                        (expression.ComparisonType == ComparisonType.Geq ?
                            this.Solver.GreaterThanOrEqual(v1.Value, v2.Value) :
                            this.Solver.LessThanOrEqual(v1.Value, v2.Value)) :
                        (expression.ComparisonType == ComparisonType.Geq ?
                            this.Solver.GreaterThanOrEqualSigned(v1.Value, v2.Value) :
                            this.Solver.LessThanOrEqualSigned(v1.Value, v2.Value));
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, r);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenListAddFrontExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr, parameter);
            var elt = Evaluate(expression.Element, parameter);

            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>.Empty;
            foreach (var kv in v.GuardedListGroup.Mapping)
            {
                var guard = kv.Value.Guard;
                var values = kv.Value.Values.Insert(0, elt);
                mapping = mapping.Add(kv.Key + 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(guard, values));
            }

            var listGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(mapping);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, listGroup);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1>(ZenListEmptyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>.Empty;
            var list = ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>.Empty;
            mapping = mapping.Add(0, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver.True(), list));
            var guardedListGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(mapping);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, guardedListGroup);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var list = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.ListExpr, parameter);

            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> result = null;

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
                    var (hd, tl) = CommonUtilities.SplitHeadHelper(values);
                    var tlImmutable = (ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>)tl;

                    // push the guard into the tail of the list
                    var map = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>.Empty;
                    map = map.Add(length - 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver.True(), tlImmutable));
                    var group = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(map);
                    var rest = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, group);

                    // execute the cons case with placeholder values to get a new Zen value.
                    var arg1 = new ZenArgumentExpr<TList>();
                    var arg2 = new ZenArgumentExpr<FSeq<TList>>();
                    var args = parameter.ArgumentsToValue.Add(arg1.ArgumentId, hd).Add(arg2.ArgumentId, rest);
                    var newEnv = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(parameter.ArgumentsToExpr, args);
                    var newExpression = expression.ConsCase(arg1, arg2);

                    // model check the resulting value using the computed values for the placeholders.
                    var r =  Evaluate(newExpression, newEnv);
                    result = Merge(guard, r, result);
                }
            }

            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit(ZenNotExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Not(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit(ZenOrExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr1, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr2, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.Or(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T1, T2>(ZenWithFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var o = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.Expr, parameter);
            var f = Evaluate(expression.FieldValue, parameter);
            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(typeof(T1), this.Solver, o.Fields.SetItem(expression.FieldName, f));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var emptyDict = this.Solver.DictEmpty(typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, emptyDict);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var e3 = Evaluate(expression.ValueExpr, parameter);
            var e = this.Solver.DictSet(e1.Value, e2, e3, typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, e);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var e = this.Solver.DictDelete(e1.Value, e2, typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, e);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var (flag, e) = this.Solver.DictGet(e1.Value, e2, typeof(TKey), typeof(TValue));

            var hasValue = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, flag);
            var optionValue = this.Solver.ConvertExprToSymbolicValue(e, typeof(TValue));

            var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>.Empty
                .Add("HasValue", hasValue).Add("Value", optionValue);

            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(typeof(Option<TValue>), this.Solver, fields);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TKey>(ZenDictCombineExpr<TKey> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.DictExpr1, parameter);
            var e2 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.DictExpr2, parameter);

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

            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, expr);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqEmptyExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var emptySeq = this.Solver.SeqEmpty(typeof(T));
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, emptySeq);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqUnitExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e = Evaluate(expression.ValueExpr, parameter);
            var unitSeq = this.Solver.SeqUnit(e, typeof(T));
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, unitSeq);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqConcatExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr1, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr2, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqConcat(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqLengthExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqLength(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqAtExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.IndexExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqAt(v1.Value, v2.Value));
        }

        private SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> v1,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> v2)
        {
            if (v2 == null)
            {
                return v1;
            }

            return v1.Merge(guard, v2);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqContainsExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SubseqExpr, parameter);

            switch (expression.ContainmentType)
            {
                case SeqContainmentType.HasPrefix:
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqPrefixOf(v1.Value, v2.Value));
                case SeqContainmentType.HasSuffix:
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqSuffixOf(v1.Value, v2.Value));
                default:
                    Contract.Assert(expression.ContainmentType == SeqContainmentType.Contains);
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqContains(v1.Value, v2.Value));
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqIndexOfExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SubseqExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.OffsetExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqIndexOf(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqSliceExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.OffsetExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.LengthExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqSlice(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqReplaceFirstExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SubseqExpr, parameter);
            var v3 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.ReplaceExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqReplaceFirst(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                var e = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SourceExpr, parameter);
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, e.Value);
            }
            else
            {
                Contract.Assert(typeof(TKey) == ReflectionUtilities.UnicodeSequenceType);

                var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SourceExpr, parameter);
                return new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, e.Value);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Visit<T>(ZenSeqRegexExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> parameter)
        {
            var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>)Evaluate(expression.SeqExpr, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>(this.Solver, this.Solver.SeqRegex(e.Value, expression.Regex));
        }
    }
}
