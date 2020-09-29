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
        /// <param name="function">The function.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T> Compile<T>(Func<Zen<T>> function, int maxUnrollingDepth)
        {
            var args = ImmutableDictionary<string, Expression>.Empty;
            var env = new ExpressionConverterEnvironment(args);
            var e = function();
            var expr = CompileToBlock(e, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T>>(expr, new ParameterExpression[] { });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2> Compile<T1, T2>(Func<Zen<T1>, Zen<T2>> function, int maxUnrollingDepth)
        {
            var args = ImmutableDictionary<string, Expression>.Empty;

            var arg1 = new ZenArgumentExpr<T1>();
            var param1 = Expression.Parameter(typeof(T1));
            args = args.Add(arg1.Id, param1);

            var env = new ExpressionConverterEnvironment(args);
            var e = function(arg1);
            var expr = CompileToBlock(e, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T1, T2>>(expr, new ParameterExpression[] { param1 });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2, T3> Compile<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> function, int maxUnrollingDepth)
        {
            var args = ImmutableDictionary<string, Expression>.Empty;

            var arg1 = new ZenArgumentExpr<T1>();
            var param1 = Expression.Parameter(typeof(T1));
            args = args.Add(arg1.Id, param1);

            var arg2 = new ZenArgumentExpr<T2>();
            var param2 = Expression.Parameter(typeof(T2));
            args = args.Add(arg2.Id, param2);

            var env = new ExpressionConverterEnvironment(args);
            var e = function(arg1, arg2);
            var expr = CompileToBlock(e, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T1, T2, T3>>(expr, new ParameterExpression[] { param1, param2 });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2, T3, T4> Compile<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function, int maxUnrollingDepth)
        {
            var args = ImmutableDictionary<string, Expression>.Empty;

            var arg1 = new ZenArgumentExpr<T1>();
            var param1 = Expression.Parameter(typeof(T1));
            args = args.Add(arg1.Id, param1);

            var arg2 = new ZenArgumentExpr<T2>();
            var param2 = Expression.Parameter(typeof(T2));
            args = args.Add(arg2.Id, param2);

            var arg3 = new ZenArgumentExpr<T3>();
            var param3 = Expression.Parameter(typeof(T3));
            args = args.Add(arg3.Id, param3);

            var env = new ExpressionConverterEnvironment(args);
            var e = function(arg1, arg2, arg3);
            var expr = CompileToBlock(e, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
            var lambda = Expression.Lambda<Func<T1, T2, T3, T4>>(expr, new ParameterExpression[] { param1, param2, param3 });
            return lambda.Compile();
        }

        /// <summary>
        /// Compile a Zen expression to native IL.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>A native IL function.</returns>
        public static Func<T1, T2, T3, T4, T5> Compile<T1, T2, T3, T4, T5>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function, int maxUnrollingDepth)
        {
            var args = ImmutableDictionary<string, Expression>.Empty;

            var arg1 = new ZenArgumentExpr<T1>();
            var param1 = Expression.Parameter(typeof(T1));
            args = args.Add(arg1.Id, param1);

            var arg2 = new ZenArgumentExpr<T2>();
            var param2 = Expression.Parameter(typeof(T2));
            args = args.Add(arg2.Id, param2);

            var arg3 = new ZenArgumentExpr<T3>();
            var param3 = Expression.Parameter(typeof(T3));
            args = args.Add(arg3.Id, param3);

            var arg4 = new ZenArgumentExpr<T4>();
            var param4 = Expression.Parameter(typeof(T4));
            args = args.Add(arg4.Id, param4);

            var env = new ExpressionConverterEnvironment(args);
            var e = function(arg1, arg2, arg3, arg4);
            var expr = CompileToBlock(e, env, ImmutableDictionary<object, Expression>.Empty, 0, maxUnrollingDepth);
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
            var converter = new ExpressionConverter(subexpressionCache, currentMatchUnrollingDepth, maxMatchUnrollingDepth);
            var expr = zenExpression.Accept(converter, env);
            converter.BlockExpressions.Add(expr);
            return Expression.Block(converter.Variables, converter.BlockExpressions.ToArray());
        }
    }
}
