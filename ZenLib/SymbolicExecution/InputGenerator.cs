// <copyright file="InputGenerator.cs" company="Microsoft">
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
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="input">The symbolic input.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputs<T1, T2>(
            Func<Zen<T1>, Zen<T2>> function,
            Zen<T1> input,
            Backend backend)
        {
            var expression = function(input).Simplify();

            (T2, PathConstraint) interpretFunction(T1 e)
            {
                var assignment = ModelCheckerFactory.CreateModelChecker(backend, null).ModelCheck(input == e);
                var evaluator = new ExpressionEvaluator(true);
                var env = new ExpressionEvaluatorEnvironment(assignment);
                return ((T2)expression.Accept(evaluator, env), evaluator.PathConstraint);
            }

            Option<T1> findFunction(Zen<bool> e) => SymbolicEvaluator.Find(e, input, backend);

            return GenerateInputsSage(findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2)> GenerateInputs<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>> function,
            Zen<T1> input1,
            Zen<T2> input2,
            Backend backend)
        {
            var expression = function(input1, input2).Simplify();

            (T3, PathConstraint) interpretFunction((T1, T2) e)
            {
                var assignment = ModelCheckerFactory
                    .CreateModelChecker(backend, null)
                    .ModelCheck(And(input1 == e.Item1, input2 == e.Item2));
                var evaluator = new ExpressionEvaluator(true);
                var env = new ExpressionEvaluatorEnvironment(assignment);
                return ((T3)expression.Accept(evaluator, env), evaluator.PathConstraint);
            }

            Option<(T1, T2)> findFunction(Zen<bool> e) => SymbolicEvaluator.Find(e, input1, input2, backend);

            return GenerateInputsSage(findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="input3">The third symbolic input.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3)> GenerateInputs<T1, T2, T3, T4>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Backend backend)
        {
            var expression = function(input1, input2, input3).Simplify();

            (T4, PathConstraint) interpretFunction((T1, T2, T3) e)
            {
                var assignment = ModelCheckerFactory
                    .CreateModelChecker(backend, null)
                    .ModelCheck(And(input1 == e.Item1, input2 == e.Item2, input3 == e.Item3));
                var evaluator = new ExpressionEvaluator(true);
                var env = new ExpressionEvaluatorEnvironment(assignment);
                return ((T4)expression.Accept(evaluator, env), evaluator.PathConstraint);
            }

            Option<(T1, T2, T3)> findFunction(Zen<bool> e) => SymbolicEvaluator.Find(e, input1, input2, input3, backend);

            return GenerateInputsSage(findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="input3">The third symbolic input.</param>
        /// <param name="input4">The fourth symbolic input.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3, T4)> GenerateInputs<T1, T2, T3, T4, T5>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            Backend backend)
        {
            var expression = function(input1, input2, input3, input4).Simplify();

            (T5, PathConstraint) interpretFunction((T1, T2, T3, T4) e)
            {
                var assignment = ModelCheckerFactory
                    .CreateModelChecker(backend, null)
                    .ModelCheck(And(input1 == e.Item1, input2 == e.Item2, input3 == e.Item3, input4 == e.Item4));
                var evaluator = new ExpressionEvaluator(true);
                var env = new ExpressionEvaluatorEnvironment(assignment);
                return ((T5)expression.Accept(evaluator, env), evaluator.PathConstraint);
            }

            Option<(T1, T2, T3, T4)> findFunction(Zen<bool> e) => SymbolicEvaluator.Find(e, input1, input2, input3, input4, backend);

            return GenerateInputsSage(findFunction, interpretFunction);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="findFunction">Function to solve path constraints.</param>
        /// <param name="interpretFunction">The function to interpret the expression.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputsSage<T1, T2>(
            Func<Zen<bool>, Option<T1>> findFunction,
            Func<T1, (T2, PathConstraint)> interpretFunction)
        {
            var seed = Generate<T1>();
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
                    var expr = And(pc.GetExpr(), Not(pathConstraint.Conjuncts[j]));
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