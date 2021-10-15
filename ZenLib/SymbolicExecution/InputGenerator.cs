﻿// <copyright file="InputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ZenLib.Interpretation;
    using ZenLib.ModelChecking;
    using static ZenLib.Language;

    /// <summary>
    /// Generate inputs to a function that exercise all paths.
    /// </summary>
    internal static class InputGenerator
    {
        /// <summary>
        /// Default empty arguments.
        /// </summary>
        private static Dictionary<long, object> arguments = new Dictionary<long, object>();

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="precondition">A precondition.</param>
        /// <param name="input">The symbolic input.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputs<T1, T2>(
            Func<Zen<T1>, Zen<T2>> function,
            Func<Zen<T1>, Zen<bool>> precondition,
            Zen<T1> input,
            Backend backend)
        {
            var expression = function(input);
            var assume = precondition(input);

            (T2, PathConstraint) interpretFunction(T1 e)
            {
                return CommonUtilities.RunWithLargeStack(() =>
                {
                    var assignment = ModelCheckerFactory.CreateModelChecker(backend, null, arguments).ModelCheck(input == e, arguments);
                    var evaluator = new ExpressionEvaluator(true);
                    var env = new ExpressionEvaluatorEnvironment(assignment);
                    return ((T2)expression.Accept(evaluator, env), evaluator.PathConstraint);
                });
            }

            Option<T1> findFunction(Zen<bool> e) =>
                CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(e, arguments, input, backend));

            return GenerateInputsSage(assume, findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="precondition">A precondition for inputs.</param>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2)> GenerateInputs<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>> function,
            Func<Zen<T1>, Zen<T2>, Zen<bool>> precondition,
            Zen<T1> input1,
            Zen<T2> input2,
            Backend backend)
        {
            var expression = function(input1, input2);
            var assume = precondition(input1, input2);

            (T3, PathConstraint) interpretFunction((T1, T2) e)
            {
                return CommonUtilities.RunWithLargeStack(() =>
                {
                    var assignment = ModelCheckerFactory
                        .CreateModelChecker(backend, null, arguments)
                        .ModelCheck(And(input1 == e.Item1, input2 == e.Item2), arguments);
                    var evaluator = new ExpressionEvaluator(true);
                    var env = new ExpressionEvaluatorEnvironment(assignment);
                    return ((T3)expression.Accept(evaluator, env), evaluator.PathConstraint);
                });
            }

            Option<(T1, T2)> findFunction(Zen<bool> e) =>
                CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(e, arguments, input1, input2, backend));

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
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3)> GenerateInputs<T1, T2, T3, T4>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> precondition,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Backend backend)
        {
            var expression = function(input1, input2, input3);
            var assume = precondition(input1, input2, input3);

            (T4, PathConstraint) interpretFunction((T1, T2, T3) e)
            {
                return CommonUtilities.RunWithLargeStack(() =>
                {
                    var assignment = ModelCheckerFactory
                        .CreateModelChecker(backend, null, arguments)
                        .ModelCheck(And(input1 == e.Item1, input2 == e.Item2, input3 == e.Item3), arguments);
                    var evaluator = new ExpressionEvaluator(true);
                    var env = new ExpressionEvaluatorEnvironment(assignment);
                    return ((T4)expression.Accept(evaluator, env), evaluator.PathConstraint);
                });
            }

            Option<(T1, T2, T3)> findFunction(Zen<bool> e) =>
                CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(e, arguments, input1, input2, input3, backend));

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
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3, T4)> GenerateInputs<T1, T2, T3, T4, T5>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> precondition,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            Backend backend)
        {
            var expression = function(input1, input2, input3, input4);
            var assume = precondition(input1, input2, input3, input4);

            (T5, PathConstraint) interpretFunction((T1, T2, T3, T4) e)
            {
                return CommonUtilities.RunWithLargeStack(() =>
                {
                    var assignment = ModelCheckerFactory
                        .CreateModelChecker(backend, null, arguments)
                        .ModelCheck(And(input1 == e.Item1, input2 == e.Item2, input3 == e.Item3, input4 == e.Item4), arguments);
                    var evaluator = new ExpressionEvaluator(true);
                    var env = new ExpressionEvaluatorEnvironment(assignment);
                    return ((T5)expression.Accept(evaluator, env), evaluator.PathConstraint);
                });
            }

            Option<(T1, T2, T3, T4)> findFunction(Zen<bool> e) =>
                CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(e, arguments, input1, input2, input3, input4, backend));

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
            Func<Zen<bool>, Option<T1>> findFunction,
            Func<T1, (T2, PathConstraint)> interpretFunction)
        {
            var potentialSeed = findFunction(precondition);

            if (!potentialSeed.HasValue)
            {
                yield break;
            }

            var seed = potentialSeed.Value;

            var queue = new Queue<(T1, int)>();
            queue.Enqueue((seed, 0));

            while (queue.Count > 0)
            {
                var (example, bound) = queue.Dequeue();

                // return the next example.
                yield return example;

                var (output, pathConstraint) = interpretFunction(example);

                // generate all the children by negating some path constraint.
                var childInputs = new List<(T1, int)>();
                for (int j = bound; j < pathConstraint.Conjuncts.Count; j++)
                {
                    var pc = pathConstraint.GetRange(0, j - 1);
                    var expr = And(precondition, pc.GetExpr(), Not(pathConstraint.Conjuncts[j]));
                    var found = findFunction(expr);

                    if (found.HasValue)
                    {
                        childInputs.Add((found.Value, j + 1));
                    }
                }

                // run the child inputs and order them by score (lower is better).
                var results = childInputs.Select(c => (c, interpretFunction(c.Item1))).ToList();

                // eventually, prioritize paths that uncover new more new guards
                results.Sort((x, y) => 0);

                // add all the children to the queue.
                foreach (var result in results)
                {
                    queue.Enqueue(result.Item1);
                }
            }
        }
    }
}