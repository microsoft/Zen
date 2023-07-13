// <copyright file="InputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System;
    using System.Collections.Generic;
    using ZenLib.Interpretation;
    using ZenLib.ModelChecking;
    using ZenLib.Solver;
    using static ZenLib.Zen;

    /// <summary>
    /// Generate inputs to a function that exercise all paths.
    /// </summary>
    internal static class InputGenerator
    {
        /// <summary>
        /// The empty arguments.
        /// </summary>
        private static Dictionary<long, object> emptyArguments = new Dictionary<long, object>();

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="precondition">A precondition.</param>
        /// <param name="input">The symbolic input.</param>
        /// <param name="config">The solver configuration to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputs<T1, T2>(
            Func<Zen<T1>, Zen<T2>> function,
            Func<Zen<T1>, Zen<bool>> precondition,
            Zen<T1> input,
            SolverConfig config)
        {
            var expression = function(input);
            var assume = precondition(input);

            (T2, PathConstraint, Dictionary<long, object>) interpretFunction(T1 e)
            {
                var assignment = ModelCheckerFactory.CreateModelChecker(config, ModelCheckerContext.Solving, null, emptyArguments).ModelCheck(input == e, emptyArguments);
                var evaluator = new ExpressionEvaluatorVisitor(true);
                var env = new ExpressionEvaluatorEnvironment { ArbitraryAssignment = System.Collections.Immutable.ImmutableDictionary<object, object>.Empty.AddRange(assignment) };
                return ((T2)evaluator.Visit(expression, env), evaluator.PathConstraint, evaluator.PathConstraintSymbolicEnvironment);
            }

            Option<T1> findFunction(Zen<bool> e, Dictionary<long, object> env) => SymbolicEvaluator.Find(e, env, input, config);

            return GenerateInputsSage(assume, findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="precondition">A precondition for inputs.</param>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="config">The solver configuration to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2)> GenerateInputs<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>> function,
            Func<Zen<T1>, Zen<T2>, Zen<bool>> precondition,
            Zen<T1> input1,
            Zen<T2> input2,
            SolverConfig config)
        {
            var expression = function(input1, input2);
            var assume = precondition(input1, input2);

            (T3, PathConstraint, Dictionary<long, object>) interpretFunction((T1, T2) e)
            {
                var assignment = ModelCheckerFactory
                    .CreateModelChecker(config, ModelCheckerContext.Solving, null, emptyArguments)
                    .ModelCheck(And(input1 == e.Item1, input2 == e.Item2), emptyArguments);
                var evaluator = new ExpressionEvaluatorVisitor(true);
                var env = new ExpressionEvaluatorEnvironment { ArbitraryAssignment = System.Collections.Immutable.ImmutableDictionary<object, object>.Empty.AddRange(assignment) };
                return ((T3)evaluator.Visit(expression, env), evaluator.PathConstraint, evaluator.PathConstraintSymbolicEnvironment);
            }

            Option<(T1, T2)> findFunction(Zen<bool> e, Dictionary<long, object> env) => SymbolicEvaluator.Find(e, env, input1, input2, config);

            return GenerateInputsSage(assume, findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="precondition">A precondition for inputs.</param>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="input3">The third symbolic input.</param>
        /// <param name="config">The solver configuration to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3)> GenerateInputs<T1, T2, T3, T4>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> precondition,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            SolverConfig config)
        {
            var expression = function(input1, input2, input3);
            var assume = precondition(input1, input2, input3);

            (T4, PathConstraint, Dictionary<long, object>) interpretFunction((T1, T2, T3) e)
            {
                var assignment = ModelCheckerFactory
                    .CreateModelChecker(config, ModelCheckerContext.Solving, null, emptyArguments)
                    .ModelCheck(And(input1 == e.Item1, input2 == e.Item2, input3 == e.Item3), emptyArguments);
                var evaluator = new ExpressionEvaluatorVisitor(true);
                var env = new ExpressionEvaluatorEnvironment { ArbitraryAssignment = System.Collections.Immutable.ImmutableDictionary<object, object>.Empty.AddRange(assignment) };
                return ((T4)evaluator.Visit(expression, env), evaluator.PathConstraint, evaluator.PathConstraintSymbolicEnvironment);
            }

            Option<(T1, T2, T3)> findFunction(Zen<bool> e, Dictionary<long, object> env) => SymbolicEvaluator.Find(e, env, input1, input2, input3, config);

            return GenerateInputsSage(assume, findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="precondition">A precondition for inputs.</param>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="input3">The third symbolic input.</param>
        /// <param name="input4">The fourth symbolic input.</param>
        /// <param name="config">The solver configuration to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3, T4)> GenerateInputs<T1, T2, T3, T4, T5>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> precondition,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            SolverConfig config)
        {
            var expression = function(input1, input2, input3, input4);
            var assume = precondition(input1, input2, input3, input4);

            (T5, PathConstraint, Dictionary<long, object>) interpretFunction((T1, T2, T3, T4) e)
            {
                var assignment = ModelCheckerFactory
                    .CreateModelChecker(config, ModelCheckerContext.Solving, null, emptyArguments)
                    .ModelCheck(And(input1 == e.Item1, input2 == e.Item2, input3 == e.Item3, input4 == e.Item4), emptyArguments);
                var evaluator = new ExpressionEvaluatorVisitor(true);
                var env = new ExpressionEvaluatorEnvironment { ArbitraryAssignment = System.Collections.Immutable.ImmutableDictionary<object, object>.Empty.AddRange(assignment) };
                return ((T5)evaluator.Visit(expression, env), evaluator.PathConstraint, evaluator.PathConstraintSymbolicEnvironment);
            }

            Option<(T1, T2, T3, T4)> findFunction(Zen<bool> e, Dictionary<long, object> env) => SymbolicEvaluator.Find(e, env, input1, input2, input3, input4, config);

            return GenerateInputsSage(assume, findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition to satisfy for all inputs.</param>
        /// <param name="findFunction">Function to solve path constraints.</param>
        /// <param name="interpretFunction">The function to interpret the expression.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputsSage<T1, T2>(
            Zen<bool> precondition,
            Func<Zen<bool>, Dictionary<long, object>, Option<T1>> findFunction,
            Func<T1, (T2, PathConstraint, Dictionary<long, object>)> interpretFunction)
        {
            var queue = new Queue<(Zen<bool>, Dictionary<long, object>, int)>();
            queue.Enqueue((precondition, new Dictionary<long, object>(), 0));

            while (queue.Count > 0)
            {
                var (expr, environment, bound) = queue.Dequeue();
                var example = findFunction(expr, environment);

                if (!example.HasValue)
                {
                    continue;
                }

                // return the next example.
                yield return example.Value;

                var (_, pathConstraint, pathConstraintEnv) = interpretFunction(example.Value);

                // generate all the children by negating some path constraint.
                for (int j = bound; j < pathConstraint.Conjuncts.Count; j++)
                {
                    var pc = pathConstraint.GetRange(0, j - 1);
                    var e = And(precondition, pc.GetExpr(), Not(pathConstraint.Conjuncts[j]));
                    queue.Enqueue((e, pathConstraintEnv, j + 1));
                }
            }
        }
    }
}