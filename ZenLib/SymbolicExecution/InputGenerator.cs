// <copyright file="InputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System.Collections.Generic;
    using System.Linq;
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
        /// <param name="input">The symbolic input.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>A collection of inputs.</returns>
        public static IEnumerable<T1> GenerateInputs<T1, T2>(
            Zen<T1> input,
            Zen<T2> expression,
            Backend backend)
        {
            var pathConstraints = expression.Accept(new PathExplorer(), new PathConstraint());
            var seen = new HashSet<Zen<bool>>();
            foreach (var pathConstraint in pathConstraints)
            {
                var pc = And(pathConstraint.Conjuncts.ToArray());
                if (seen.Contains(pc))
                {
                    continue;
                }

                seen.Add(pc);
                var found = SymbolicEvaluator.Find(pc, input, backend);
                if (found.HasValue)
                {
                    yield return found.Value;
                }
            }
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
            var pathConstraints = expression.Accept(new PathExplorer(), new PathConstraint());
            var seen = new HashSet<Zen<bool>>();
            foreach (var pathConstraint in pathConstraints)
            {
                var pc = And(pathConstraint.Conjuncts.ToArray());
                if (seen.Contains(pc))
                {
                    continue;
                }

                seen.Add(pc);
                var found = SymbolicEvaluator.Find(pc, input1, input2, backend);
                if (found.HasValue)
                {
                    yield return found.Value;
                }
            }
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
            var pathConstraints = expression.Accept(new PathExplorer(), new PathConstraint());
            var seen = new HashSet<Zen<bool>>();
            foreach (var pathConstraint in pathConstraints)
            {
                var pc = And(pathConstraint.Conjuncts.ToArray());
                if (seen.Contains(pc))
                {
                    continue;
                }

                seen.Add(pc);
                var found = SymbolicEvaluator.Find(pc, input1, input2, input3, backend);
                if (found.HasValue)
                {
                    yield return found.Value;
                }
            }
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
            var pathConstraints = expression.Accept(new PathExplorer(), new PathConstraint());
            var seen = new HashSet<Zen<bool>>();
            foreach (var pathConstraint in pathConstraints)
            {
                var pc = And(pathConstraint.Conjuncts.ToArray());
                if (seen.Contains(pc))
                {
                    continue;
                }

                seen.Add(pc);
                var found = SymbolicEvaluator.Find(pc, input1, input2, input3, input4, backend);
                if (found.HasValue)
                {
                    yield return found.Value;
                }
            }
        }
    }
}