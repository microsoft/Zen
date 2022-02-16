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
    internal class ModelChecker<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> : IModelChecker
    {
        private ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> solver;

        /// <summary>
        /// Create an in instance of the class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public ModelChecker(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> solver)
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
            var symbolicEvaluator = new SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(solver);
            var env = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(arguments);
            var symbolicResult =
                (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)expression.Accept(symbolicEvaluator, env);

            var model = solver.Satisfiable(symbolicResult.Value);

            if (model == null)
            {
                return null;
            }

            // compute the input given the assignment
            var arbitraryAssignment = new Dictionary<object, object>();
            foreach (var kv in symbolicEvaluator.ArbitraryVariables)
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
