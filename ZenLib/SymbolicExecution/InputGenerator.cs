// <copyright file="InputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System;
    using System.Collections.Generic;
    using ZenLib.Interpretation;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Generate inputs to a function that exercise all paths.
    /// </summary>
    internal static class InputGenerator
    {
        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="input">The symbolic input.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputs<T1, T2>(
            Zen<T1> input,
            Zen<T2> expression,
            Backend backend)
        {
            Option<T1> f(Zen<bool> e) => SymbolicEvaluator.Find(e, input, backend);
            return GenerateInputs(f, expression);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2)> GenerateInputs<T1, T2, T3>(
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> expression,
            Backend backend)
        {
            Option<(T1, T2)> f(Zen<bool> e) => SymbolicEvaluator.Find(e, input1, input2, backend);
            return GenerateInputs(f, expression);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="input3">The third symbolic input.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3)> GenerateInputs<T1, T2, T3, T4>(
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> expression,
            Backend backend)
        {
            Option<(T1, T2, T3)> f(Zen<bool> e) => SymbolicEvaluator.Find(e, input1, input2, input3, backend);
            return GenerateInputs(f, expression);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="input1">The first symbolic input.</param>
        /// <param name="input2">The second symbolic input.</param>
        /// <param name="input3">The third symbolic input.</param>
        /// <param name="input4">The fourth symbolic input.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<(T1, T2, T3, T4)> GenerateInputs<T1, T2, T3, T4, T5>(
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            Zen<T5> expression,
            Backend backend)
        {
            Option<(T1, T2, T3, T4)> f(Zen<bool> e) => SymbolicEvaluator.Find(e, input1, input2, input3, input4, backend);
            return GenerateInputs(f, expression);
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="f">Function to convert a path constraint to an input.</param>
        /// <param name="expression">The expression for which to generate inputs.</param>
        /// <returns>Example inputs.</returns>
        private static IEnumerable<T1> GenerateInputs<T1, T2>(Func<Zen<bool>, Option<T1>> f, Zen<T2> expression)
        {
            var pathExplorer = new PathExplorer(pc => f(pc).HasValue);
            var pathConstraints = expression.Accept(pathExplorer, new PathConstraint());
            var seen = new HashSet<Zen<bool>>();
            foreach (var pathConstraint in pathConstraints)
            {
                if (seen.Contains(pathConstraint.Expr))
                {
                    continue;
                }

                seen.Add(pathConstraint.Expr);
                var found = f(pathConstraint.Expr);
                if (found.HasValue)
                {
                    yield return found.Value;
                }
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="input">The symbolic input.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputsSage<T1, T2>(
            Zen<T1> input,
            Zen<T2> expression,
            Backend backend)
        {
            Option<T1> f(Zen<bool> e) => SymbolicEvaluator.Find(e, input, backend);

            var seed = Language.Generate<T1>();
            var queue = new Queue<(T1, int)>();
            queue.Enqueue((seed, 0));

            while (queue.Count > 0)
            {
                var (example, bound) = queue.Dequeue();
                var evaluator = new ExpressionEvaluator(true);
                expression.Accept(evaluator, new ExpressionEvaluatorEnvironment());
            }

            return GenerateInputs(f, expression);
        }
    }
}