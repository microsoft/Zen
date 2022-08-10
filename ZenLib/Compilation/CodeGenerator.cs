// <copyright file="CodeGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Compilation
{
    using System;
    using System.Collections.Immutable;
    using System.Linq.Expressions;

    /// <summary>
    /// Code generation for converting Zen functions
    /// to IL that executes with native C# performance.
    /// </summary>
    internal static class CodeGenerator
    {
        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="expression">The function expression.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T> Compile<T>(
            Zen<T> expression,
            ImmutableDictionary<long, Expression> arguments,
            int maxUnrollingDepth)
        {
            var env = new ExpressionConverterEnvironment(arguments);
            var expr = CompileToBlock(expression, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T>>(expr, new ParameterExpression[] { });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="expression">The function expression.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="param1">The first parameter.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2> Compile<T1, T2>(
            Zen<T2> expression,
            ImmutableDictionary<long, Expression> arguments,
            ParameterExpression param1,
            int maxUnrollingDepth)
        {
            var env = new ExpressionConverterEnvironment(arguments);
            var expr = CompileToBlock(expression, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T1, T2>>(expr, new ParameterExpression[] { param1 });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="expression">The function expression.</param>
        /// <param name="arguments">The function arguments.</param>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2, T3> Compile<T1, T2, T3>(
            Zen<T3> expression,
            ImmutableDictionary<long, Expression> arguments,
            ParameterExpression param1,
            ParameterExpression param2,
            int maxUnrollingDepth)
        {
            var env = new ExpressionConverterEnvironment(arguments);
            var expr = CompileToBlock(expression, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T1, T2, T3>>(expr, new ParameterExpression[] { param1, param2 });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="expression">The function expression.</param>
        /// <param name="arguments">The function arguments.</param>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="param3">The third parameter.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2, T3, T4> Compile<T1, T2, T3, T4>(
            Zen<T4> expression,
            ImmutableDictionary<long, Expression> arguments,
            ParameterExpression param1,
            ParameterExpression param2,
            ParameterExpression param3,
            int maxUnrollingDepth)
        {
            var env = new ExpressionConverterEnvironment(arguments);
            var expr = CompileToBlock(expression, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T1, T2, T3, T4>>(expr, new ParameterExpression[] { param1, param2, param3 });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="expression">The function.</param>
        /// <param name="arguments">The function arguments.</param>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="param3">The third parameter.</param>
        /// <param name="param4">The fourth parameter.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2, T3, T4, T5> Compile<T1, T2, T3, T4, T5>(
            Zen<T5> expression,
            ImmutableDictionary<long, Expression> arguments,
            ParameterExpression param1,
            ParameterExpression param2,
            ParameterExpression param3,
            ParameterExpression param4,
            int maxUnrollingDepth)
        {
            var env = new ExpressionConverterEnvironment(arguments);
            var expr = CompileToBlock(expression, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T1, T2, T3, T4, T5>>(expr, new ParameterExpression[] { param1, param2, param3, param4 });
            return lambda.Compile();
        }

        internal static Expression CompileToBlock<T>(
            Zen<T> zenExpression,
            ExpressionConverterEnvironment env,
            ImmutableDictionary<object, Expression> subexpressionCache,
            int currentMatchUnrollingDepth,
            int maxMatchUnrollingDepth)
        {
            var converter = new ExpressionConverterVisitor(subexpressionCache, currentMatchUnrollingDepth, maxMatchUnrollingDepth);
            var expr = converter.Convert(zenExpression, env);
            converter.BlockExpressions.Add(expr);
            return Expression.Block(converter.Variables, converter.BlockExpressions.ToArray());
        }
    }
}
