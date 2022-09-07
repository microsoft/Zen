// <copyright file="SymbolicEvaluationVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using ZenLib.Solver;

    /// <summary>
    /// Visitor that computes a symbolic representation for the function.
    /// </summary>
    internal sealed class SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
        : ZenExprVisitor<
            SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>
    {
        /// <summary>
        /// Evaluate symbolic method reference.
        /// </summary>
        private static MethodInfo evaluateMethod = typeof(SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>).GetMethod("Visit");

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
        /// Mapping from ConstMap variables to their per-key expressions.
        /// </summary>
        public Dictionary<object, Dictionary<object, object>> ConstMapAssignment { get; private set; }

        /// <summary>
        /// The map constants for the ConstMap type.
        /// </summary>
        internal Dictionary<Type, ISet<object>> MapConstants;

        /// <summary>
        /// A visitor to perform merges.
        /// </summary>
        private SymbolicMergeVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> mergeVisitor;

        /// <summary>
        /// Note that we have to cache arbitrary expressions separately to ensure they always return the same symbolic
        /// regardless of the environment.
        /// </summary>
        private Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> arbitraryCache;

        /// <summary>
        /// Creates a new instance of the <see cref="SymbolicEvaluationVisitor{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public SymbolicEvaluationVisitor(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver)
        {
            this.Solver = solver;
            this.Variables = new List<TVar>();
            this.ArbitraryVariables = new Dictionary<object, TVar>();
            this.ConstMapAssignment = new Dictionary<object, Dictionary<object, object>>();
            this.mergeVisitor = new SymbolicMergeVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this);
            this.arbitraryCache = new Dictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>();
        }

        /// <summary>
        /// Compute the symbolic result.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Compute<T>(Zen<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var constantMapKeyVisitor = new CMapKeyVisitor(parameter.ArgumentsToExpr);
            this.MapConstants = constantMapKeyVisitor.Compute(expression);
            return this.Visit(expression, parameter);
        }

        /// <summary>
        /// Compute the symbolic result.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Visit<T>(Zen<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            if (this.cache.TryGetValue((expression.Id, parameter), out var value))
            {
                return value;
            }

            var result = expression.Accept(this, parameter);
            this.cache[(expression.Id, parameter)] = result;
            return result;
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitApply<TSrc, TDst>(ZenApplyExpr<TSrc, TDst> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = this.Visit(expression.ArgumentExpr, parameter);
            var args = parameter.ArgumentsToValue.SetItem(expression.Lambda.Parameter.ParameterId, v1);
            var newEnv = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(parameter.ArgumentsToExpr, args);
            return this.Visit(expression.Lambda.Body, newEnv);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitLogicalBinop(ZenLogicalBinopExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr1, parameter);
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr2, parameter);

            switch (expression.Operation)
            {
                case ZenLogicalBinopExpr.LogicalOp.And:
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.And(v1.Value, v2.Value));
                default:
                    Contract.Assert(expression.Operation == ZenLogicalBinopExpr.LogicalOp.Or);
                    return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Or(v1.Value, v2.Value));
            }
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitArbitrary<T1>(ZenArbitraryExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            if (this.arbitraryCache.TryGetValue(expression, out var result))
            {
                return result;
            }

            var arbitraryVisitor = new SymbolicArbitraryVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this, parameter);
            result = arbitraryVisitor.Visit(typeof(T1), expression);
            this.arbitraryCache[expression] = result;
            return result;
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitParameter<T1>(ZenParameterExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            if (parameter.ArgumentsToExpr.TryGetValue(expression.ParameterId, out var expr))
            {
                try
                {
                    var method = evaluateMethod.MakeGenericMethod(typeof(T1));
                    return (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)method.Invoke(this, new object[] { expr, parameter });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            else
            {
                return parameter.ArgumentsToValue[expression.ParameterId];
            }
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitArithBinop<T1>(ZenArithBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = this.Visit(expression.Expr1, parameter);
            var e2 = this.Visit(expression.Expr2, parameter);

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

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBitwiseBinop<T1>(ZenBitwiseBinopExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr1, parameter);
            var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr2, parameter);

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

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBitwiseNot<T1>(ZenBitwiseNotExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr, parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.BitwiseNot(v.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitConstant<T>(ZenConstantExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var visitor = new SymbolicConstantVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this, parameter);
            return visitor.Visit(typeof(T), expression.Value);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        [ExcludeFromCodeCoverage] // Can't trigger TargetInvocationException
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
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

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr, parameter);
            return v.Fields[expression.FieldName];
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitIf<T1>(ZenIfExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.GuardExpr, parameter);
            var vtrue = this.Visit(expression.TrueExpr, parameter);
            var vfalse = this.Visit(expression.FalseExpr, parameter);
            return this.mergeVisitor.Visit(typeof(T1), (v.Value, vtrue, vfalse));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitEquality<T>(ZenEqualityExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = this.Visit(expression.Expr1, parameter);
            var e2 = this.Visit(expression.Expr2, parameter);
            var equalityVisitor = new SymbolicEqualityVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this, parameter);
            var result = equalityVisitor.Visit(typeof(T), (e1, e2));
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitArithComparison<T1>(ZenArithComparisonExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = this.Visit(expression.Expr1, parameter);
            var e2 = this.Visit(expression.Expr2, parameter);

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

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitListAdd<T1>(ZenFSeqAddFrontExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.ListExpr, parameter);
            var elt = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.ElementExpr, parameter);
            var hasValue = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)elt.Fields["HasValue"];
            var newList = v.Value.Insert(0, (hasValue.Value, elt.Fields["Value"]));
            return new SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, newList);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitListEmpty<T1>(ZenFSeqEmptyExpr<T1> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var list = ImmutableList<(TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)>.Empty;
            return new SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, list);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitListCase<T, TResult>(ZenFSeqCaseExpr<T, TResult> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var list = (SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.ListExpr, parameter);

            // if the list is empty, evaluate the empty case.
            if (list.Value.Count == 0)
            {
                return this.Visit(expression.EmptyExpr, parameter);
            }

            // split the symbolic list
            var (hd, tl) = CommonUtilities.SplitHeadHelper(list.Value);

            // get the symbolic value for the tail.
            var symbolicTl = new SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, tl);

            // create a symbolic value for the head of the list.
            var mapping = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty
                .Add("HasValue", new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, hd.Item1))
                .Add("Value", hd.Item2);
            var symbolicHd = new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(typeof(Option<T>), this.Solver, mapping);

            // create a symbolic value for the pair of the head and tail of the list.
            var pairMapping = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty
                .Add("Item1", symbolicHd)
                .Add("Item2", symbolicTl);
            var pair = new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(typeof(Pair<Option<T>, FSeq<T>>), this.Solver, pairMapping);

            // update the arguments to assign this value to the cons lambda parameter.
            var args = parameter.ArgumentsToValue.SetItem(expression.ConsLambda.Parameter.ParameterId, pair);
            var newEnv = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(parameter.ArgumentsToExpr, args);

            // model check the resulting value using the computed values for the placeholders.
            return this.Visit(expression.ConsLambda.Body, newEnv);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitNot(ZenNotExpr expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.Not(v.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var o = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.Expr, parameter);
            var f = this.Visit(expression.FieldExpr, parameter);
            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(typeof(T1), this.Solver, o.Fields.SetItem(expression.FieldName, f));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.MapExpr, parameter);
            var e2 = this.Visit(expression.KeyExpr, parameter);
            var e3 = this.Visit(expression.ValueExpr, parameter);
            var e = this.Solver.DictSet(e1.Value, e2, e3, typeof(TKey), typeof(TValue));
            return new SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.MapExpr, parameter);
            var e2 = this.Visit(expression.KeyExpr, parameter);
            var e = this.Solver.DictDelete(e1.Value, e2, typeof(TKey), typeof(TValue));
            return new SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.MapExpr, parameter);
            var e2 = this.Visit(expression.KeyExpr, parameter);
            var (flag, e) = this.Solver.DictGet(e1.Value, e2, typeof(TKey), typeof(TValue));

            var hasValue = new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, flag);
            var optionValue = this.Solver.ConvertExprToSymbolicValue(e, typeof(TValue));

            var fields = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty
                .Add("HasValue", hasValue).Add("Value", optionValue);

            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(typeof(Option<TValue>), this.Solver, fields);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.MapExpr1, parameter);
            var e2 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.MapExpr2, parameter);

            TArray expr;
            switch (expression.CombinationType)
            {
                case ZenMapCombineExpr<TKey>.CombineType.Union:
                    expr = this.Solver.DictUnion(e1.Value, e2.Value);
                    break;
                case ZenMapCombineExpr<TKey>.CombineType.Intersect:
                    expr = this.Solver.DictIntersect(e1.Value, e2.Value);
                    break;
                default:
                    Contract.Assert(expression.CombinationType == ZenMapCombineExpr<TKey>.CombineType.Difference);
                    expr = this.Solver.DictDifference(e1.Value, e2.Value);
                    break;
            }

            return new SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, expr);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e1 = (SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.MapExpr, parameter);
            var e2 = this.Visit(expression.ValueExpr, parameter);
            var result = e1.Value.SetItem(expression.Key, e2);
            return new SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e = (SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.MapExpr, parameter);

            if (e.Value.TryGetValue(expression.Key, out var result))
            {
                return result;
            }

            return this.Visit(Zen.Default<TValue>(), parameter);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e = this.Visit(expression.ValueExpr, parameter);
            var unitSeq = this.Solver.SeqUnit(e, typeof(T));
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, unitSeq);
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr1, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr2, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqConcat(v1.Value, v2.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqLength(v.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqAt<T>(ZenSeqAtExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.IndexExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqAt(v1.Value, v2.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqNth<T>(ZenSeqNthExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.IndexExpr, parameter);
            var expr = this.Solver.SeqNth(v1.Value, v2.Value);
            return this.Solver.ConvertExprToSymbolicValue(expr, typeof(T));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SubseqExpr, parameter);

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

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SubseqExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.OffsetExpr, parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqIndexOf(v1.Value, v2.Value, v3.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.OffsetExpr, parameter);
            var v3 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.LengthExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqSlice(v1.Value, v2.Value, v3.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SubseqExpr, parameter);
            var v3 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.ReplaceExpr, parameter);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqReplaceFirst(v1.Value, v2.Value, v3.Value));
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                var e = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SourceExpr, parameter);
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e.Value);
            }
            else if (typeof(TKey) == ReflectionUtilities.UnicodeSequenceType)
            {
                var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SourceExpr, parameter);
                return new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, e.Value);
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TKey)));
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TValue)));

                var sourceSize = ReflectionUtilities.GetFiniteIntegerSize<TKey>();
                var targetSize = ReflectionUtilities.GetFiniteIntegerSize<TValue>();

                var e = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SourceExpr, parameter);
                var resized = this.Solver.Resize(e.Value, sourceSize, targetSize);
                return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, resized);
            }
        }

        /// <summary>
        /// Visit the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> parameter)
        {
            var e = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)this.Visit(expression.SeqExpr, parameter);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, this.Solver.SeqRegex(e.Value, expression.Regex));
        }
    }
}
