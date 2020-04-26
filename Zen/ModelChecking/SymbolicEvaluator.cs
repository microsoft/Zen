// <copyright file="SymbolicEvaluator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.ModelChecking
{
    using Microsoft.Research.Zen.Interpretation;

    /// <summary>
    /// Helper class to perform symbolic reasoning.
    /// </summary>
    internal static class SymbolicEvaluator
    {
        public static bool Find(Zen<bool> expression, Backend backend, bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);
            return assignment != null;
        }

        public static Option<T> Find<T>(
            Zen<bool> expression,
            Zen<T> input,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);

            if (assignment == null)
            {
                return Option.None<T>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result = input.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some((T)result);
        }

        public static Option<(T1, T2)> Find<T1, T2>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);

            var assignment = modelChecker.ModelCheck(expression);

            if (assignment == null)
            {
                return Option.None<(T1, T2)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some(((T1)result1, (T2)result2));
        }

        public static Option<(T1, T2, T3)> Find<T1, T2, T3>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);

            if (assignment == null)
            {
                return Option.None<(T1, T2, T3)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result3 = input3.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some(((T1)result1, (T2)result2, (T3)result3));
        }

        public static Option<(T1, T2, T3, T4)> Find<T1, T2, T3, T4>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);

            if (assignment == null)
            {
                return Option.None<(T1, T2, T3, T4)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result3 = input3.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result4 = input4.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some(((T1)result1, (T2)result2, (T3)result3, (T4)result4));
        }
    }
}
