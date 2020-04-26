// <copyright file="ModelCheckerFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.ModelChecking
{
    using System;
    using DecisionDiagrams;
    using Microsoft.Z3;

    /// <summary>
    /// Model checker factory class.
    /// </summary>
    internal static class ModelCheckerFactory
    {
        /// <summary>
        /// Create a model checker.
        /// </summary>
        /// <param name="backend">The backend to use.</param>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>A new model checker.</returns>
        internal static IModelChecker CreateModelChecker(Backend backend, Zen<bool> expression)
        {
            if (backend == Backend.DecisionDiagrams)
            {
                return CreateModelCheckerDD(expression);
            }

            return CreateModelCheckerZ3();
        }

        /// <summary>
        /// Create a model checker based on decision diagrams.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static IModelChecker CreateModelCheckerDD(Zen<bool> expression)
        {
            var heuristic = new InterleavingHeuristic();
            var mustInterleave = heuristic.Compute(expression);
            var manager = new DDManager<CBDDNode>(new CBDDNodeFactory());
            var solver = new SolverDD<CBDDNode>(manager, mustInterleave);
            return new ModelChecker<Assignment<CBDDNode>, Variable<CBDDNode>, DD, BitVector<CBDDNode>>(solver);
        }

        /// <summary>
        /// Create a model checker based on SMT with Z3.
        /// </summary>
        /// <returns></returns>
        private static IModelChecker CreateModelCheckerZ3()
        {
            var solver = new SolverZ3();
            return new ModelChecker<Model, Expr, BoolExpr, BitVecExpr>(solver);
        }
    }
}
