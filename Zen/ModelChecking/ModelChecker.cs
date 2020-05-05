// <copyright file="ModelCheckerDD.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.ModelChecking
{
    using System.Collections.Generic;

    /// <summary>
    /// Implementaiton of a bounded model checker, which is
    /// parameterized over the underlying solver.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TVar">The variable type.</typeparam>
    /// <typeparam name="TBool">The boolean expression type.</typeparam>
    /// <typeparam name="TInt">The integer expression type.</typeparam>
    internal class ModelChecker<TModel, TVar, TBool, TInt> : IModelChecker
    {
        private ISolver<TModel, TVar, TBool, TInt> solver;

        /// <summary>
        /// Create an in instance of the <see cref="ModelChecker{TModel, TVar, TBool, TInt}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public ModelChecker(ISolver<TModel, TVar, TBool, TInt> solver)
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
            var symbolicEvaluator = new SymbolicEvaluationVisitor<TModel, TVar, TBool, TInt>(solver);
            var env = new SymbolicEvaluationEnvironment<TModel, TVar, TBool, TInt>();
            var symbolicResult =
                (SymbolicBool<TModel, TVar, TBool, TInt>)expression.Accept(symbolicEvaluator, env);

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
