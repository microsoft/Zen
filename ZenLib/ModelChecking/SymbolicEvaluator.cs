// <copyright file="SymbolicEvaluator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using ZenLib.Interpretation;

    /// <summary>
    /// Helper class to perform symbolic reasoning.
    /// </summary>
    internal static class SymbolicEvaluator
    {
        /// <summary>
        /// Determine if an expression has a satisfying assignment.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>True or false.</returns>
        public static Dictionary<object, object> Find(Zen<bool> expression, Dictionary<long, object> arguments, Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression, arguments);
            return modelChecker.ModelCheck(expression, arguments);
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="input">The Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<T> Find<T>(
            Zen<bool> expression,
            Dictionary<long, object> arguments,
            Zen<T> input,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression, arguments);
            var assignment = modelChecker.ModelCheck(expression, arguments);
            if (assignment == null)
            {
                return Option.None<T>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result = input.Accept(new ExpressionEvaluator(false), interpreterEnv);
            return Option.Some(CommonUtilities.ConvertSymbolicResultToCSharp<T>(result));
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="input1">The first Zen expression for the input to the function.</param>
        /// <param name="input2">The second Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<(T1, T2)> Find<T1, T2>(
            Zen<bool> expression,
            Dictionary<long, object> arguments,
            Zen<T1> input1,
            Zen<T2> input2,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression, arguments);

            var assignment = modelChecker.ModelCheck(expression, arguments);

            if (assignment == null)
            {
                return Option.None<(T1, T2)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(false), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(false), interpreterEnv);
            return Option.Some((CommonUtilities.ConvertSymbolicResultToCSharp<T1>(result1),
                                CommonUtilities.ConvertSymbolicResultToCSharp<T2>(result2)));
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="input1">The first Zen expression for the input to the function.</param>
        /// <param name="input2">The second Zen expression for the input to the function.</param>
        /// <param name="input3">The third Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<(T1, T2, T3)> Find<T1, T2, T3>(
            Zen<bool> expression,
            Dictionary<long, object> arguments,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression, arguments);
            var assignment = modelChecker.ModelCheck(expression, arguments);

            if (assignment == null)
            {
                return Option.None<(T1, T2, T3)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(false), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(false), interpreterEnv);
            var result3 = input3.Accept(new ExpressionEvaluator(false), interpreterEnv);
            return Option.Some((CommonUtilities.ConvertSymbolicResultToCSharp<T1>(result1),
                                CommonUtilities.ConvertSymbolicResultToCSharp<T2>(result2),
                                CommonUtilities.ConvertSymbolicResultToCSharp<T3>(result3)));
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="input1">The first Zen expression for the input to the function.</param>
        /// <param name="input2">The second Zen expression for the input to the function.</param>
        /// <param name="input3">The third Zen expression for the input to the function.</param>
        /// <param name="input4">The fourth Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<(T1, T2, T3, T4)> Find<T1, T2, T3, T4>(
            Zen<bool> expression,
            Dictionary<long, object> arguments,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression, arguments);
            var assignment = modelChecker.ModelCheck(expression, arguments);

            if (assignment == null)
            {
                return Option.None<(T1, T2, T3, T4)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(false), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(false), interpreterEnv);
            var result3 = input3.Accept(new ExpressionEvaluator(false), interpreterEnv);
            var result4 = input4.Accept(new ExpressionEvaluator(false), interpreterEnv);
            return Option.Some((CommonUtilities.ConvertSymbolicResultToCSharp<T1>(result1),
                                CommonUtilities.ConvertSymbolicResultToCSharp<T2>(result2),
                                CommonUtilities.ConvertSymbolicResultToCSharp<T3>(result3),
                                CommonUtilities.ConvertSymbolicResultToCSharp<T4>(result4)));
        }
    }
}
