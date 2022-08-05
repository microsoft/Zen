// <copyright file="ZenFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq.Expressions;
    using ZenLib.Compilation;
    using ZenLib.Interpretation;
    using ZenLib.ModelChecking;
    using ZenLib.SymbolicExecution;
    using static ZenLib.Zen;

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    public class ZenFunction<T>
    {
        /// <summary>
        /// The function body expression.
        /// </summary>
        internal Zen<T> FunctionBodyExpr;

        /// <summary>
        /// The compiled function as C# IL.
        /// </summary>
        internal Func<T> CompiledFunction = null;

        /// <summary>
        /// User provided function.
        /// </summary>
        public Func<Zen<T>> Function;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T>(ZenFunction<T> value)
        {
            return () => value.Evaluate();
        }

        /// <summary>
        /// Create a new instance of the <see cref="ZenFunction{T}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T>> function)
        {
            Contract.AssertNotNull(function);
            this.Function = function;
            this.FunctionBodyExpr = this.Function();
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <returns>Result.</returns>
        public T Evaluate()
        {
            if (CompiledFunction != null)
            {
                return CompiledFunction();
            }

            var args = new Dictionary<long, object>();
            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.FunctionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (CompiledFunction != null)
            {
                return;
            }

            var args = ImmutableDictionary<long, Expression>.Empty;
            this.CompiledFunction = CodeGenerator.Compile(this.FunctionBodyExpr, args, maxUnrollingDepth);
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public bool Assert(Func<Zen<T>, Zen<bool>> invariant, Backend backend = Backend.Z3)
        {
            var result = invariant(this.Function());
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, new Dictionary<long, object>(), backend) != null);
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Return type.</typeparam>
    public class ZenFunction<T1, T2>
    {
        /// <summary>
        /// Lock for creating the argument.
        /// </summary>
        private static object argumentLock = new object();

        /// <summary>
        /// First argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T1> Argument1;

        /// <summary>
        /// The expression for the function body.
        /// </summary>
        internal Zen<T2> FunctionBodyExpr;

        /// <summary>
        /// The compiled C# version of the function.
        /// </summary>
        internal Func<T1, T2> CompiledFunction = null;

        /// <summary>
        /// The callback for the Zen function.
        /// </summary>
        public Func<Zen<T1>, Zen<T2>> Function;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2>(ZenFunction<T1, T2> value)
        {
            return (v) => value.Evaluate(v);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>> function)
        {
            Contract.AssertNotNull(function);
            this.Function = function;
            lock (argumentLock)
            {
                Argument1 = Argument1 ?? new ZenArgumentExpr<T1>();
            }
            this.FunctionBodyExpr = this.Function(Argument1);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Result.</returns>
        public T2 Evaluate(T1 value)
        {
            if (CompiledFunction != null)
            {
                return CompiledFunction(value);
            }

            var args = new Dictionary<long, object> { { Argument1.ArgumentId, value } };
            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.FunctionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (CompiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(Argument1.ArgumentId, param1);
            this.CompiledFunction = CodeGenerator.Compile<T1, T2>(this.FunctionBodyExpr, args, param1, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<T1, T2> Transformer(StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof((T1, T2)), this.FunctionBodyExpr.Id);
            if (manager.TransformerCache.TryGetValue(key, out var transformer))
            {
                return (StateSetTransformer<T1, T2>)transformer;
            }

            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateTransformer(this.Function, manager));
            manager.TransformerCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input">Default input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<T1> Find(
            Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant,
            Zen<T1> input = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input = CommonUtilities.GetArbitraryIfNull(input, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input },
            };

            var result = invariant(Argument1, this.FunctionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input">Default input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<T1> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant,
            Zen<T1> input = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input = CommonUtilities.GetArbitraryIfNull(input, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input },
            };

            var result = invariant(input, this.FunctionBodyExpr);

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(expr, args, input, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, Argument1 == example.Value);
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input">Default input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<T1> GenerateInputs(
            Zen<T1> input = null,
            Func<Zen<T1>, Zen<bool>> precondition = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input = CommonUtilities.GetArbitraryIfNull(input, depth, exhaustiveDepth);
            precondition = precondition == null ? (x) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.Function, precondition, input, backend));
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Argument 2 type.</typeparam>
    /// <typeparam name="T3">Return type.</typeparam>
    public class ZenFunction<T1, T2, T3>
    {
        /// <summary>
        /// Lock for creating the argument.
        /// </summary>
        private static object argumentLock = new object();

        /// <summary>
        /// First argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T1> Argument1;

        /// <summary>
        /// Second argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T2> Argument2;

        /// <summary>
        /// Function body expression.
        /// </summary>
        internal Zen<T3> FunctionBodyExpr;

        /// <summary>
        /// Compiled function to C# IL.
        /// </summary>
        internal Func<T1, T2, T3> CompiledFunction = null;

        /// <summary>
        /// User provided function.
        /// </summary>
        public Func<Zen<T1>, Zen<T2>, Zen<T3>> Function;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3>(ZenFunction<T1, T2, T3> value)
        {
            return (v1, v2) => value.Evaluate(v1, v2);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>> function)
        {
            Contract.AssertNotNull(function);
            this.Function = function;
            lock (argumentLock)
            {
                Argument1 = Argument1 ?? new ZenArgumentExpr<T1>();
                Argument2 = Argument2 ?? new ZenArgumentExpr<T2>();
            }
            this.FunctionBodyExpr = this.Function(Argument1, Argument2);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Result.</returns>
        public T3 Evaluate(T1 value1, T2 value2)
        {
            if (CompiledFunction != null)
            {
                return CompiledFunction(value1, value2);
            }

            var args = new Dictionary<long, object>()
            {
                { Argument1.ArgumentId, value1 },
                { Argument2.ArgumentId, value2 },
            };

            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.FunctionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (CompiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(Argument1.ArgumentId, param1)
                .Add(Argument2.ArgumentId, param2);
            this.CompiledFunction = CodeGenerator.Compile<T1, T2, T3>(this.FunctionBodyExpr, args, param1, param2, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<Pair<T1, T2>, T3> Transformer(StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof((Pair<T1, T2>, T3)), this.FunctionBodyExpr.Id);
            if (manager.TransformerCache.TryGetValue(key, out var transformer))
            {
                return (StateSetTransformer<Pair<T1, T2>, T3>)transformer;
            }

            Func<Zen<Pair<T1, T2>>, Zen<T3>> f = p => this.Function(p.Item1(), p.Item2());
            var result = StateSetTransformerFactory.CreateTransformer(f, manager);
            manager.TransformerCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input1 },
                { Argument2.ArgumentId, input2 },
            };

            var result = invariant(Argument1, Argument2, this.FunctionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input1, input2, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">First input that captures structural constraints.</param>
        /// <param name="input2">Second input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2)> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input1 },
                { Argument2.ArgumentId, input2 },
            };

            var result = invariant(input1, input2, this.FunctionBodyExpr);

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(
                    () => SymbolicEvaluator.Find(expr, args, input1, input2, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, And(Argument1 == example.Value.Item1, Argument2 == example.Value.Item2));
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2)> GenerateInputs(
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Func<Zen<T1>, Zen<T2>, Zen<bool>> precondition = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            precondition = precondition == null ? (x, y) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.Function, precondition, input1, input2, backend));
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Argument 2 type.</typeparam>
    /// <typeparam name="T3">Argument 3 type.</typeparam>
    /// <typeparam name="T4">Return type.</typeparam>
    public class ZenFunction<T1, T2, T3, T4>
    {
        /// <summary>
        /// Lock for creating the argument.
        /// </summary>
        private static object argumentLock = new object();

        /// <summary>
        /// First argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T1> Argument1;

        /// <summary>
        /// Second argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T2> Argument2;

        /// <summary>
        /// Third argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T3> Argument3;

        /// <summary>
        /// Function body expression.
        /// </summary>
        internal Zen<T4> FunctionBodyExpr;

        /// <summary>
        /// Compiled function as C# IL.
        /// </summary>
        internal Func<T1, T2, T3, T4> CompiledFunction = null;

        /// <summary>
        /// User-provided function.
        /// </summary>
        public Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> Function;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3, T4>(ZenFunction<T1, T2, T3, T4> value)
        {
            return (v1, v2, v3) => value.Evaluate(v1, v2, v3);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2, T3, T4}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function)
        {
            Contract.AssertNotNull(function);
            this.Function = function;
            lock (argumentLock)
            {
                Argument1 = Argument1 ?? new ZenArgumentExpr<T1>();
                Argument2 = Argument2 ?? new ZenArgumentExpr<T2>();
                Argument3 = Argument3 ?? new ZenArgumentExpr<T3>();
            }
            this.FunctionBodyExpr = this.Function(Argument1, Argument2, Argument3);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="value3">The third value.</param>
        /// <returns>Result.</returns>
        public T4 Evaluate(T1 value1, T2 value2, T3 value3)
        {
            if (CompiledFunction != null)
            {
                return CompiledFunction(value1, value2, value3);
            }

            var args = new Dictionary<long, object>()
            {
                { Argument1.ArgumentId, value1 },
                { Argument2.ArgumentId, value2 },
                { Argument3.ArgumentId, value3 },
            };

            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.FunctionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (CompiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var param3 = Expression.Parameter(typeof(T3));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(Argument1.ArgumentId, param1)
                .Add(Argument2.ArgumentId, param2)
                .Add(Argument3.ArgumentId, param3);
            this.CompiledFunction = CodeGenerator.Compile<T1, T2, T3, T4>(this.FunctionBodyExpr, args, param1, param2, param3, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<Pair<T1, T2, T3>, T4> Transformer(StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof((Pair<T1, T2, T3>, T4)), this.FunctionBodyExpr.Id);
            if (manager.TransformerCache.TryGetValue(key, out var transformer))
            {
                return (StateSetTransformer<Pair<T1, T2, T3>, T4>)transformer;
            }

            Func<Zen<Pair<T1, T2, T3>>, Zen<T4>> f = p => this.Function(p.Item1(), p.Item2(), p.Item3());
            var result = StateSetTransformerFactory.CreateTransformer(f, manager);
            manager.TransformerCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2, T3)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input1 },
                { Argument2.ArgumentId, input2 },
                { Argument3.ArgumentId, input3 },
            };

            var result = invariant(Argument1, Argument2, Argument3, this.FunctionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input1, input2, input3, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">First input that captures structural constraints.</param>
        /// <param name="input2">Second input that captures structural constraints.</param>
        /// <param name="input3">Third input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3)> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input1 },
                { Argument2.ArgumentId, input2 },
                { Argument3.ArgumentId, input3 },
            };

            var result = invariant(input1, input2, input3, this.FunctionBodyExpr);

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(
                    () => SymbolicEvaluator.Find(expr, args, input1, input2, input3, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, And(Argument1 == example.Value.Item1, Argument2 == example.Value.Item2, Argument3 == example.Value.Item3));
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3)> GenerateInputs(
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> precondition = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, depth, exhaustiveDepth);
            precondition = precondition == null ? (x, y, z) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.Function, precondition, input1, input2, input3, backend));
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Argument 2 type.</typeparam>
    /// <typeparam name="T3">Argument 3 type.</typeparam>
    /// <typeparam name="T4">Argument 4 type.</typeparam>
    /// <typeparam name="T5">Return type.</typeparam>
    public class ZenFunction<T1, T2, T3, T4, T5>
    {
        /// <summary>
        /// Lock for creating the argument.
        /// </summary>
        private static object argumentLock = new object();

        /// <summary>
        /// First argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T1> Argument1;

        /// <summary>
        /// Second argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T2> Argument2;

        /// <summary>
        /// Third argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T3> Argument3;

        /// <summary>
        /// Fourth argument expression.
        /// </summary>
        internal static ZenArgumentExpr<T4> Argument4;

        /// <summary>
        /// Function body expression.
        /// </summary>
        internal Zen<T5> FunctionBodyExpr;

        /// <summary>
        /// Compiled function as C# IL.
        /// </summary>
        internal Func<T1, T2, T3, T4, T5> CompiledFunction = null;

        /// <summary>
        /// User-provided function.
        /// </summary>
        public Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> Function;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3, T4, T5>(ZenFunction<T1, T2, T3, T4, T5> value)
        {
            return (v1, v2, v3, v4) => value.Evaluate(v1, v2, v3, v4);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function)
        {
            Contract.AssertNotNull(function);
            this.Function = function;
            lock (argumentLock)
            {
                Argument1 = Argument1 ?? new ZenArgumentExpr<T1>();
                Argument2 = Argument2 ?? new ZenArgumentExpr<T2>();
                Argument3 = Argument3 ?? new ZenArgumentExpr<T3>();
                Argument4 = Argument4 ?? new ZenArgumentExpr<T4>();
            }
            this.FunctionBodyExpr = this.Function(Argument1, Argument2, Argument3, Argument4);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="value3">The third value.</param>
        /// <param name="value4">The fourth value.</param>
        /// <returns>Result.</returns>
        public T5 Evaluate(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            if (CompiledFunction != null)
            {
                return CompiledFunction(value1, value2, value3, value4);
            }

            var args = new Dictionary<long, object>()
            {
                { Argument1.ArgumentId, value1 },
                { Argument2.ArgumentId, value2 },
                { Argument3.ArgumentId, value3 },
                { Argument4.ArgumentId, value4 },
            };

            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.FunctionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (CompiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var param3 = Expression.Parameter(typeof(T3));
            var param4 = Expression.Parameter(typeof(T4));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(Argument1.ArgumentId, param1)
                .Add(Argument2.ArgumentId, param2)
                .Add(Argument3.ArgumentId, param3)
                .Add(Argument4.ArgumentId, param4);
            this.CompiledFunction = CodeGenerator.Compile<T1, T2, T3, T4, T5>(this.FunctionBodyExpr, args, param1, param2, param3, param4, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<Pair<T1, T2, T3, T4>, T5> Transformer(StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof((Pair<T1, T2, T3, T4>, T5)), this.FunctionBodyExpr.Id);
            if (manager.TransformerCache.TryGetValue(key, out var transformer))
            {
                return (StateSetTransformer<Pair<T1, T2, T3, T4>, T5>)transformer;
            }

            Func<Zen<Pair<T1, T2, T3, T4>>, Zen<T5>> f = p => this.Function(p.Item1(), p.Item2(), p.Item3(), p.Item4());
            var result = StateSetTransformerFactory.CreateTransformer(f, manager);
            manager.TransformerCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="input4">Default fourth input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2, T3, T4)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Zen<T4> input4 = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, depth, exhaustiveDepth);
            input4 = CommonUtilities.GetArbitraryIfNull(input4, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input1 },
                { Argument2.ArgumentId, input2 },
                { Argument3.ArgumentId, input3 },
                { Argument4.ArgumentId, input4 },
            };

            var result = invariant(Argument1, Argument2, Argument3, Argument4, this.FunctionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input1, input2, input3, input4, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">First input that captures structural constraints.</param>
        /// <param name="input2">Second input that captures structural constraints.</param>
        /// <param name="input3">Third input that captures structural constraints.</param>
        /// <param name="input4">Fourth input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3, T4)> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Zen<T4> input4 = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, depth, exhaustiveDepth);
            input4 = CommonUtilities.GetArbitraryIfNull(input4, depth, exhaustiveDepth);
            var args = new Dictionary<long, object>
            {
                { Argument1.ArgumentId, input1 },
                { Argument2.ArgumentId, input2 },
                { Argument3.ArgumentId, input3 },
                { Argument4.ArgumentId, input4 },
            };

            var result = invariant(input1, input2, input3, input4, this.FunctionBodyExpr);

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(
                    () => SymbolicEvaluator.Find(expr, args, input1, input2, input3, input4, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, And(Argument1 == example.Value.Item1, Argument2 == example.Value.Item2, Argument3 == example.Value.Item3, Argument4 == example.Value.Item4));
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="input4">Default fourth input that captures structural constraints.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3, T4)> GenerateInputs(
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Zen<T4> input4 = null,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> precondition = null,
            int depth = 5,
            bool exhaustiveDepth = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, depth, exhaustiveDepth);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, depth, exhaustiveDepth);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, depth, exhaustiveDepth);
            input4 = CommonUtilities.GetArbitraryIfNull(input4, depth, exhaustiveDepth);
            precondition = precondition == null ? (w, x, y, z) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.Function, precondition, input1, input2, input3, input4, backend));
        }
    }
}