// <copyright file="ZenFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Zen extension methods for solving.
    /// </summary>
    public static class ZenExtensions
    {
        /// <summary>
        /// Solves for an assignment to Arbitrary variables in a boolean expression.
        /// </summary>
        /// <param name="expr">The boolean expression.</param>
        /// <param name="backend">The solver backend to use.</param>
        /// <returns>Mapping from arbitrary expressions to C# objects.</returns>
        public static Solution Solve(this Zen<bool> expr, Backend backend = Backend.Z3)
        {
            return new Solution(SymbolicEvaluator.Find(expr, new Dictionary<long, object>(), backend));
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSet<T> StateSet<T>(this ZenFunction<T, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(T), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<T>)stateSet;
            }

            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(function.Function, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSet<Pair<T1, T2>> StateSet<T1, T2>(this ZenFunction<T1, T2, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(Pair<T1, T2>), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<Pair<T1, T2>>)stateSet;
            }

            Func<Zen<Pair<T1, T2>>, Zen<bool>> f = p => function.Function(p.Item1(), p.Item2());
            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(f, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSet<Pair<T1, T2, T3>> StateSet<T1, T2, T3>(this ZenFunction<T1, T2, T3, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(Pair<T1, T2, T3>), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<Pair<T1, T2, T3>>)stateSet;
            }

            Func<Zen<Pair<T1, T2, T3>>, Zen<bool>> f = p => function.Function(p.Item1(), p.Item2(), p.Item3());
            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(f, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSet<Pair<T1, T2, T3, T4>> StateSet<T1, T2, T3, T4>(this ZenFunction<T1, T2, T3, T4, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(Pair<T1, T2, T3, T4>), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<Pair<T1, T2, T3, T4>>)stateSet;
            }

            Func<Zen<Pair<T1, T2, T3, T4>>, Zen<bool>> f = p => function.Function(p.Item1(), p.Item2(), p.Item3(), p.Item4());
            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(f, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }
    }
}