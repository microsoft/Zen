// <copyright file="ModelCheckerFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using DecisionDiagrams;
    using Microsoft.Z3;
    using ZenLib.Solver;

    /// <summary>
    /// Model checker factory class.
    /// </summary>
    internal static class ModelCheckerFactory
    {
        /// <summary>
        /// Create a model checker.
        /// </summary>
        /// <param name="config">The solver configuration to use.</param>
        /// <param name="context">The checking context.</param>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="arguments">The arguements.</param>
        /// <returns>A new model checker.</returns>
        internal static IModelChecker CreateModelChecker(SolverConfig config, ModelCheckerContext context, Zen<bool> expression, Dictionary<long, object> arguments)
        {
            if (config.SolverType == SolverType.DecisionDiagrams)
            {
                return CreateModelCheckerDD(expression, arguments);
            }

            return CreateModelCheckerZ3(context, config.SolverTimeout, config.Debug);
        }

        /// <summary>
        /// Create a model checker based on decision diagrams.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>A model checker.</returns>
        private static IModelChecker CreateModelCheckerDD(Zen<bool> expression, Dictionary<long, object> arguments)
        {
            var heuristic = new InterleavingHeuristicVisitor();
            var args = ImmutableDictionary<long, object>.Empty.AddRange(arguments);
            var mustInterleave = heuristic.GetInterleavedVariables(expression, args);
            var manager = new DDManager<CBDDNode>(new CBDDNodeFactory());
            var solver = new SolverDD<CBDDNode>(manager, mustInterleave);
            solver.Init();
            return new ModelChecker<Assignment<CBDDNode>, Variable<CBDDNode>, DD, BitVector<CBDDNode>, Unit, Unit, Unit, Unit, Unit>(solver);
        }

        /// <summary>
        /// Create a model checker based on SMT with Z3.
        /// </summary>
        /// <param name="context">The model checker context.</param>
        /// <param name="timeout">A solver timeout parameter.</param>
        /// <param name="debug">An optional debugging callback.</param>
        /// <returns>A model checker.</returns>
        private static IModelChecker CreateModelCheckerZ3(ModelCheckerContext context, TimeSpan? timeout, Action<SolverDebugInfo> debug)
        {
            return new ModelChecker<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(new SolverZ3(context, timeout, debug));
        }
    }
}
