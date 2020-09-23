// <copyright file="ModelCheckerDD.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
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
    /// <typeparam name="TString">The string expression type.</typeparam>
    internal class ModelChecker<TModel, TVar, TBool, TBitvec, TInt, TString> : IModelChecker
    {
        private ISolver<TModel, TVar, TBool, TBitvec, TInt, TString> solver;

        /// <summary>
        /// Create an in instance of the class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public ModelChecker(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString> solver)
        {
            this.solver = solver;
        }

        /// <summary>
        /// Model check an expression to find inputs that lead to it being false.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        ///     Assignment to zen arbitrary variables that make the expression false.
        ///     Null if no such assignment exists.
        /// </returns>
        public Dictionary<object, object> ModelCheck(Zen<bool> expression)
        {
            var symbolicEvaluator = new SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TString>(solver);
            var env = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TString>();
            var symbolicResult =
                (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString>)expression.Accept(symbolicEvaluator, env);

            // Console.WriteLine($"[time] model checking: {watch.ElapsedMilliseconds}ms");
            // watch.Restart();

            var possibleModel = solver.Satisfiable(symbolicResult.Value);

            // Console.WriteLine($"[time] get sat: {watch.ElapsedMilliseconds}ms");

            if (!possibleModel.HasValue)
            {
                return null;
            }

            var model = possibleModel.Value;

            // compute the input given the assignment
            var arbitraryAssignment = new Dictionary<object, object>();
            foreach (var kv in symbolicEvaluator.ArbitraryVariables)
            {
                var expr = kv.Key;
                var variable = kv.Value;
                arbitraryAssignment.Add(expr, this.solver.Get(model, variable));
            }

            return arbitraryAssignment;
        }
    }
}
