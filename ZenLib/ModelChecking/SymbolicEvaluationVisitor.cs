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
    internal sealed class SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
        : IZenExprVisitor<SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>
    {
        /// <summary>
        /// Evaluate symbolic method reference.
        /// </summary>
        private static MethodInfo evaluateMethod = typeof(SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>).GetMethodCached("Evaluate");

        /// <summary>
        /// Gets the solver.
        /// </summary>
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Solver { get; }

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
        private Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> Cache { get; } = new Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>();

        public SymbolicEvaluationVisitor(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver)
        {
            this.Solver = solver;
            this.Variables = new List<TVar>();
            this.ArbitraryVariables = new Dictionary<object, TVar>();
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Evaluate<T>(Zen<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            if (this.Cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Accept(this, parameter);
            this.Cache[expression] = result;
            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit(ZenAndExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr1, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr2, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.And(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenArbitraryExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var type = typeof(T1);

            if (type == ReflectionUtilities.BoolType)
            {
                var (variable, expr) = this.Solver.CreateBoolVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.ByteType)
            {
                var (variable, expr) = this.Solver.CreateByteVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.CharType)
            {
                var (variable, expr) = this.Solver.CreateCharVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.ShortType || type == ReflectionUtilities.UshortType)
            {
                var (variable, expr) = this.Solver.CreateShortVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.IntType || type == ReflectionUtilities.UintType)
            {
                var (variable, expr) = this.Solver.CreateIntVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.LongType || type == ReflectionUtilities.UlongType)
            {
                var (variable, expr) = this.Solver.CreateLongVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.BigIntType)
            {
                var (variable, expr) = this.Solver.CreateBigIntegerVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (type == ReflectionUtilities.RealType)
            {
                var (variable, expr) = this.Solver.CreateRealVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (ReflectionUtilities.IsMapType(type))
            {
                var (variable, expr) = this.Solver.CreateDictVar(expression);
                this.Variables.Add(variable);
                this.ArbitraryVariables[expression] = variable;
                return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
            }
            else if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var size = CommonUtilities.IntegerSize(type);
                var (v, e) = this.Solver.CreateBitvecVar(expression, (uint)size);
                this.Variables.Add(v);
                this.ArbitraryVariables[expression] = v;
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e);
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsSeqType(type));

                var (v, e) = this.Solver.CreateSeqVar(expression);
                this.Variables.Add(v);
                this.ArbitraryVariables[expression] = v;
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenArgumentExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            if (parameter.ArgumentsToExpr.TryGetValue(expression.ArgumentId, out var expr))
            {
                try
                {
                    var innerType = expr.GetType().BaseType.GetGenericArgumentsCached()[0];
                    var method = evaluateMethod.MakeGenericMethod(innerType);
                    var result = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)method.Invoke(this, new object[] { expr, parameter });
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

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenArithBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e2;

                switch (expression.Operation)
                {
                    case ArithmeticOp.Addition:
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                    case ArithmeticOp.Subtraction:
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                    default:
                        Contract.Assert(expression.Operation == ArithmeticOp.Multiplication);
                        return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                }
            }
            else if (e1 is SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)
            {
                var v1 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e1;
                var v2 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e2;

                switch (expression.Operation)
                {
                    case ArithmeticOp.Addition:
                        return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                    case ArithmeticOp.Subtraction:
                        return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                    default:
                        Contract.Assert(expression.Operation == ArithmeticOp.Multiplication);
                        return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                }
            }
            else
            {
                Contract.Assert(e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>);

                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e2;

                switch (expression.Operation)
                {
                    case ArithmeticOp.Addition:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Add(v1.Value, v2.Value));
                    case ArithmeticOp.Subtraction:
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Subtract(v1.Value, v2.Value));
                    default:
                        Contract.Assert(expression.Operation == ArithmeticOp.Multiplication);
                        return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Multiply(v1.Value, v2.Value));
                }
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenBitwiseBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr1, parameter);
            var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr2, parameter);

            switch (expression.Operation)
            {
                case BitwiseOp.BitwiseAnd:
                    return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.BitwiseAnd(v1.Value, v2.Value));
                case BitwiseOp.BitwiseOr:
                    return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.BitwiseOr(v1.Value, v2.Value));
                default:
                    Contract.Assert(expression.Operation == BitwiseOp.BitwiseXor);
                    return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.BitwiseXor(v1.Value, v2.Value));
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenBitwiseNotExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr, parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.BitwiseNot(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenConstantExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.BigIntType)
            {
                var bi = this.Solver.CreateBigIntegerConst((BigInteger)(object)expression.Value);
                return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bi);
            }
            else if (type == ReflectionUtilities.RealType)
            {
                var bi = this.Solver.CreateRealConst((Real)(object)expression.Value);
                return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bi);
            }
            else if (type == ReflectionUtilities.BoolType)
            {
                var b = (bool)(object)expression.Value ? this.Solver.True() : this.Solver.False();
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, b);
            }
            else if (type == ReflectionUtilities.ByteType)
            {
                var bv = this.Solver.CreateByteConst((byte)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.CharType)
            {
                var c = this.Solver.CreateCharConst((ZenLib.Char)(object)expression.Value);
                return new SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, c);
            }
            else if (type == ReflectionUtilities.ShortType)
            {
                var bv = this.Solver.CreateShortConst((short)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UshortType)
            {
                var bv = this.Solver.CreateShortConst((short)(ushort)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.IntType)
            {
                var bv = this.Solver.CreateIntConst((int)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UintType)
            {
                var bv = this.Solver.CreateIntConst((int)(uint)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.LongType)
            {
                var bv = this.Solver.CreateLongConst((long)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else if (type == ReflectionUtilities.UlongType)
            {
                var bv = this.Solver.CreateLongConst((long)(ulong)(object)expression.Value);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var bv = this.Solver.CreateBitvecConst(((dynamic)expression.Value).GetBits());
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, bv);
            }
            else
            {
                Contract.Assert(type == typeof(Seq<ZenLib.Char>));
                var escapedString = CommonUtilities.ConvertCShaprStringToZ3((Seq<ZenLib.Char>)(object)expression.Value);
                var s = this.Solver.CreateStringConst(escapedString);
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, s);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TObject>(ZenCreateObjectExpr<TObject> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            try
            {
                var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    var innerType = fieldValuePair.Value.GetType().BaseType.GetGenericArgumentsCached()[0];
                    var method = evaluateMethod.MakeGenericMethod(innerType);
                    var fieldValue = (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)method.Invoke(this, new object[] { fieldValuePair.Value, parameter });
                    fields = fields.Add(field, fieldValue);
                }

                return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(typeof(TObject), this.Solver, fields);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1, T2>(ZenGetFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr, parameter);
            return v.Fields[expression.FieldName];
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenIfExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.GuardExpr, parameter);
            var vtrue = Evaluate(expression.TrueExpr, parameter);
            var vfalse = Evaluate(expression.FalseExpr, parameter);
            return vtrue.Merge(v.Value, vfalse);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenEqualityExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> b1 &&
                e2 is SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> b2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Iff(b1.Value, b2.Value));
            }
            else if (e1 is SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> c1 &&
                     e2 is SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> c2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Eq(c1.Value, c2.Value));
            }
            else if (e1 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> s1 &&
                     e2 is SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> s2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Eq(s1.Value, s2.Value));
            }
            else if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> bi1 &&
                     e2 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> bi2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Eq(bi1.Value, bi2.Value));
            }
            else if (e1 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> di1 &&
                     e2 is SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> di2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Eq(di1.Value, di2.Value));
            }
            else if (e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> i1 &&
                     e2 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> i2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Eq(i1.Value, i2.Value));
            }
            else if (e1 is SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> r1 &&
                     e2 is SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> r2)
            {
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Eq(r1.Value, r2.Value));
            }
            else
            {
                Contract.Assert(e1 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>);
                Contract.Assert(e2 is SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>);
                var seq1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e1;
                var seq2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e2;
                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Eq(seq1.Value, seq2.Value));
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenArithComparisonExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = Evaluate(expression.Expr1, parameter);
            var e2 = Evaluate(expression.Expr2, parameter);

            if (e1 is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)
            {
                var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e1;
                var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e2;

                TBool result;
                switch (expression.ComparisonType)
                {
                    case ComparisonType.Geq:
                        result = this.Solver.GreaterThanOrEqual(v1.Value, v2.Value);
                        break;
                    case ComparisonType.Leq:
                        result = this.Solver.LessThanOrEqual(v1.Value, v2.Value);
                        break;
                    case ComparisonType.Gt:
                        result = this.Solver.GreaterThan(v1.Value, v2.Value);
                        break;
                    default:
                        Contract.Assert(expression.ComparisonType == ComparisonType.Lt);
                        result = this.Solver.LessThan(v1.Value, v2.Value);
                        break;
                }

                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
            }
            else if (e1 is SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)
            {
                var v1 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e1;
                var v2 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e2;

                TBool result;
                switch (expression.ComparisonType)
                {
                    case ComparisonType.Geq:
                        result = this.Solver.GreaterThanOrEqual(v1.Value, v2.Value);
                        break;
                    case ComparisonType.Leq:
                        result = this.Solver.LessThanOrEqual(v1.Value, v2.Value);
                        break;
                    case ComparisonType.Gt:
                        result = this.Solver.GreaterThan(v1.Value, v2.Value);
                        break;
                    default:
                        Contract.Assert(expression.ComparisonType == ComparisonType.Lt);
                        result = this.Solver.LessThan(v1.Value, v2.Value);
                        break;
                }

                return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
            }
            else
            {
                Contract.Assert(e1 is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>);

                var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e1;
                var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)e2;

                if (ReflectionUtilities.IsUnsignedIntegerType(typeof(T1)))
                {
                    TBool result;
                    switch (expression.ComparisonType)
                    {
                        case ComparisonType.Geq:
                            result = this.Solver.GreaterThanOrEqual(v1.Value, v2.Value);
                            break;
                        case ComparisonType.Leq:
                            result = this.Solver.LessThanOrEqual(v1.Value, v2.Value);
                            break;
                        case ComparisonType.Gt:
                            result = this.Solver.GreaterThan(v1.Value, v2.Value);
                            break;
                        default:
                            Contract.Assert(expression.ComparisonType == ComparisonType.Lt);
                            result = this.Solver.LessThan(v1.Value, v2.Value);
                            break;
                    }

                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
                }
                else
                {
                    TBool result;
                    switch (expression.ComparisonType)
                    {
                        case ComparisonType.Geq:
                            result = this.Solver.GreaterThanOrEqualSigned(v1.Value, v2.Value);
                            break;
                        case ComparisonType.Leq:
                            result = this.Solver.LessThanOrEqualSigned(v1.Value, v2.Value);
                            break;
                        case ComparisonType.Gt:
                            result = this.Solver.GreaterThanSigned(v1.Value, v2.Value);
                            break;
                        default:
                            Contract.Assert(expression.ComparisonType == ComparisonType.Lt);
                            result = this.Solver.LessThanSigned(v1.Value, v2.Value);
                            break;
                    }

                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
                }
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenListAddFrontExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr, parameter);
            var elt = Evaluate(expression.Element, parameter);

            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            foreach (var kv in v.GuardedListGroup.Mapping)
            {
                var guard = kv.Value.Guard;
                var values = kv.Value.Values.Insert(0, elt);
                mapping = mapping.Add(kv.Key + 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(guard, values));
            }

            var listGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(mapping);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, listGroup);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1>(ZenListEmptyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var mapping = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            var list = ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            mapping = mapping.Add(0, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver.True(), list));
            var guardedListGroup = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(mapping);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, guardedListGroup);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var list = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.ListExpr, parameter);

            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> result = null;

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
                    var tlImmutable = (ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>)tl;

                    // push the guard into the tail of the list
                    var map = ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
                    map = map.Add(length - 1, new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver.True(), tlImmutable));
                    var group = new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(map);
                    var rest = new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, group);

                    // execute the cons case with placeholder values to get a new Zen value.
                    var arg1 = new ZenArgumentExpr<TList>();
                    var arg2 = new ZenArgumentExpr<FSeq<TList>>();
                    var args = parameter.ArgumentsToValue.Add(arg1.ArgumentId, hd).Add(arg2.ArgumentId, rest);
                    var newEnv = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(parameter.ArgumentsToExpr, args);
                    var newExpression = expression.ConsCase(arg1, arg2);

                    // model check the resulting value using the computed values for the placeholders.
                    var r =  Evaluate(newExpression, newEnv);
                    result = Merge(guard, r, result);
                }
            }

            return result;
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit(ZenNotExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Not(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit(ZenOrExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr1, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr2, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Or(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T1, T2>(ZenWithFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var o = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.Expr, parameter);
            var f = Evaluate(expression.FieldValue, parameter);
            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(typeof(T1), this.Solver, o.Fields.SetItem(expression.FieldName, f));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var emptyDict = this.Solver.DictEmpty(typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, emptyDict);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var e3 = Evaluate(expression.ValueExpr, parameter);
            var e = this.Solver.DictSet(e1.Value, e2, e3, typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var e = this.Solver.DictDelete(e1.Value, e2, typeof(TKey), typeof(TValue));
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.DictExpr, parameter);
            var e2 = Evaluate(expression.KeyExpr, parameter);
            var (flag, e) = this.Solver.DictGet(e1.Value, e2, typeof(TKey), typeof(TValue));

            var hasValue = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, flag);
            var optionValue = this.Solver.ConvertExprToSymbolicValue(e, typeof(TValue));

            var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty
                .Add("HasValue", hasValue).Add("Value", optionValue);

            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(typeof(Option<TValue>), this.Solver, fields);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TKey>(ZenDictCombineExpr<TKey> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.DictExpr1, parameter);
            var e2 = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.DictExpr2, parameter);

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

            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqEmptyExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var emptySeq = this.Solver.SeqEmpty(typeof(T));
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, emptySeq);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqUnitExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e = Evaluate(expression.ValueExpr, parameter);
            var unitSeq = this.Solver.SeqUnit(e, typeof(T));
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, unitSeq);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqConcatExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr1, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr2, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqConcat(v1.Value, v2.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqLengthExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqLength(v.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqAtExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.IndexExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqAt(v1.Value, v2.Value));
        }

        private SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v1,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v2)
        {
            if (v2 == null)
            {
                return v1;
            }

            return v1.Merge(guard, v2);
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqContainsExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SubseqExpr, parameter);

            switch (expression.ContainmentType)
            {
                case SeqContainmentType.HasPrefix:
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqPrefixOf(v1.Value, v2.Value));
                case SeqContainmentType.HasSuffix:
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqSuffixOf(v1.Value, v2.Value));
                default:
                    Contract.Assert(expression.ContainmentType == SeqContainmentType.Contains);
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqContains(v1.Value, v2.Value));
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqIndexOfExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SubseqExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.OffsetExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqIndexOf(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqSliceExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.OffsetExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.LengthExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqSlice(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqReplaceFirstExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SubseqExpr, parameter);
            var v3 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.ReplaceExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqReplaceFirst(v1.Value, v2.Value, v3.Value));
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                var e = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SourceExpr, parameter);
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e.Value);
            }
            else if (typeof(TKey) == ReflectionUtilities.UnicodeSequenceType)
            {
                var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SourceExpr, parameter);
                return new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e.Value);
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TKey)));
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TValue)));

                var sourceSize = ReflectionUtilities.GetFiniteIntegerSize<TKey>();
                var targetSize = ReflectionUtilities.GetFiniteIntegerSize<TValue>();

                var e = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SourceExpr, parameter);
                var resized = this.Solver.Resize(e.Value, sourceSize, targetSize);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, resized);
            }
        }

        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(ZenSeqRegexExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)Evaluate(expression.SeqExpr, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqRegex(e.Value, expression.Regex));
        }
    }
}
