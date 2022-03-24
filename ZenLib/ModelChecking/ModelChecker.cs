// <copyright file="ModelCheckerDD.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using ZenLib.Solver;

    /// <summary>
    /// Implementaiton of a bounded model checker, which is
    /// parameterized over the underlying solver.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TVar">The variable type.</typeparam>
    /// <typeparam name="TBool">The boolean expression type.</typeparam>
    /// <typeparam name="TBitvec">The bitvector expression type.</typeparam>
    /// <typeparam name="TInt">The integer expression type.</typeparam>
    /// <typeparam name="TSeq">The sequence expression type.</typeparam>
    /// <typeparam name="TArray">The array expression type.</typeparam>
    /// <typeparam name="TChar">The character expression type.</typeparam>
    /// <typeparam name="TReal">The real expression type.</typeparam>
    internal class ModelChecker<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> : IModelChecker
    {
        private ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver;

        /// <summary>
        /// Create an in instance of the class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public ModelChecker(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver)
        {
            this.solver = solver;
        }

        /// <summary>
        /// Model check an expression to find inputs that lead to it being false.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        ///     Assignment to zen arbitrary variables that make the expression false.
        ///     Null if no such assignment exists.
        /// </returns>
        public Dictionary<object, object> ModelCheck(Zen<bool> expression, Dictionary<long, object> arguments)
        {
            var symbolicEvaluator = new SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(solver);
            var env = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(arguments);
            var symbolicResult = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)symbolicEvaluator.Evaluate(expression, env);
            var model = solver.Solve(symbolicResult.Value);
            return model == null ? null : GetCSharpAssignmentFromModel(model, symbolicEvaluator.ArbitraryVariables);
        }

        /// <summary>
        /// Maximize an objective subject to some constraints.
        /// </summary>
        /// <param name="maximize">The expression to maximize.</param>
        /// <param name="subjectTo">The constraints for the problem.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        ///     Mapping from zen arbitrary expression to value.
        ///     Null if there is no input.
        /// </returns>
        public Dictionary<object, object> Maximize<T>(Zen<T> maximize, Zen<bool> subjectTo, Dictionary<long, object> arguments)
        {
            // compile the constraints first.
            var symbolicEvaluator = new SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(solver);
            var env = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(arguments);

            // evaluate both the constraints and the objective.
            var symbolicResult = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)symbolicEvaluator.Evaluate(subjectTo, env);
            var symbolicResultOpt = symbolicEvaluator.Evaluate(maximize, env);

            // get the model
            TModel model;
            if (symbolicResultOpt is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> b)
            {
                model = solver.Maximize(b.Value, symbolicResult.Value);
            }
            else if (symbolicResultOpt is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> i)
            {
                model = solver.Maximize(i.Value, symbolicResult.Value);
            }
            else
            {
                Contract.Assert(symbolicResultOpt is SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>);
                var r = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)symbolicResultOpt;
                model = solver.Maximize(r.Value, symbolicResult.Value);
            }

            return model == null ? null : GetCSharpAssignmentFromModel(model, symbolicEvaluator.ArbitraryVariables);
        }

        /// <summary>
        /// Minimize an objective subject to some constraints.
        /// </summary>
        /// <param name="maximize">The expression to maximize.</param>
        /// <param name="subjectTo">The constraints for the problem.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        ///     Mapping from zen arbitrary expression to value.
        ///     Null if there is no input.
        /// </returns>
        public Dictionary<object, object> Minimize<T>(Zen<T> maximize, Zen<bool> subjectTo, Dictionary<long, object> arguments)
        {
            // compile the constraints first.
            var symbolicEvaluator = new SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(solver);
            var env = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(arguments);

            // evaluate both the constraints and the objective.
            var symbolicResult = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)symbolicEvaluator.Evaluate(subjectTo, env);
            var symbolicResultOpt = symbolicEvaluator.Evaluate(maximize, env);

            // get the model
            TModel model;
            if (symbolicResultOpt is SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> b)
            {
                model = solver.Minimize(b.Value, symbolicResult.Value);
            }
            else if (symbolicResultOpt is SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> i)
            {
                model = solver.Minimize(i.Value, symbolicResult.Value);
            }
            else
            {
                Contract.Assert(symbolicResultOpt is SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>);
                var r = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)symbolicResultOpt;
                model = solver.Minimize(r.Value, symbolicResult.Value);
            }

            return model == null ? null : GetCSharpAssignmentFromModel(model, symbolicEvaluator.ArbitraryVariables);
        }

        /// <summary>
        /// Get the C# assignment from the model assignment.
        /// </summary>
        /// <param name="model">The solver model.</param>
        /// <param name="values">The symbolic values.</param>
        /// <returns>The C# assignment.</returns>
        private Dictionary<object, object> GetCSharpAssignmentFromModel(TModel model, Dictionary<object, TVar> values)
        {
            var arbitraryAssignment = new Dictionary<object, object>();
            foreach (var kv in values)
            {
                var expr = kv.Key;
                var variable = kv.Value;
                var type = expr.GetType().GetGenericArgumentsCached()[0];
                var obj = this.solver.Get(model, variable, type);
                arbitraryAssignment.Add(expr, obj);
            }

            return arbitraryAssignment;
        }
    }
}
