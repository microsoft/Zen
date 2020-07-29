// <copyright file="TestHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.ModelChecking;
    using static ZenLib.Language;

    /// <summary>
    /// Test helper functions for checking correctness.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TestHelper
    {
        /// <summary>
        /// Number of random inputs to try per test.
        /// </summary>
        private static int numRandomTests = 10;

        /// <summary>
        /// Deterministic random number generator.
        /// </summary>
        private static Random random = new Random(7);

        /// <summary>
        /// Backends to test.
        /// </summary>
        private static TestParameter[] parameters = new TestParameter[]
        {
            new TestParameter { Backend = Backend.DecisionDiagrams, Simplify = true, ListSize = 5 },
            new TestParameter { Backend = Backend.DecisionDiagrams, Simplify = false, ListSize = 1 },
            new TestParameter { Backend = Backend.Z3, Simplify = true, ListSize = 5 },
            new TestParameter { Backend = Backend.Z3, Simplify = false, ListSize = 5 },
        };

        /// <summary>
        /// Backends to test when running string tests.
        /// </summary>
        private static TestParameter[] stringParameters = new TestParameter[]
        {
            new TestParameter { Backend = Backend.Z3, Simplify = true, ListSize = 5 },
            new TestParameter { Backend = Backend.Z3, Simplify = false, ListSize = 5 },
        };

        /// <summary>
        /// Repeat an action multiple times.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void RandomBytes(Action<byte> action)
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var r = (byte)random.Next(0, 255);
                action(r);
            }
        }

        /// <summary>
        /// Repeat an action multiple times.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void RandomStrings(Action<string> action)
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                action(RandomString());
            }
        }

        private static string RandomString()
        {
            string s = string.Empty;
            var len = random.Next(0, 5);
            for (int j = 0; j < len; j++)
            {
                var c = (char)random.Next(0, 255);
                s = s + c;
            }

            return s;
        }

        private static void Start(TestParameter p)
        {
            Settings.SimplifyRecursive = p.Simplify;
        }

        private static void Stop(TestParameter p)
        {
            Settings.SimplifyRecursive = true;
        }

        /// <summary>
        /// Check a predicate is valid.
        /// </summary>
        /// <typeparam name="T1">First input type..</typeparam>
        /// <param name="function">The predicate.</param>
        public static void CheckValid<T1>(Func<Zen<T1>, Zen<bool>> function)
        {
            // If testing on strings, only use Z3 backend (DD does not support strings)
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                // prove that it is valid
                var f = Function(function);
                var result = f.Find((i1, o) => Not(o), listSize: p.ListSize, backend: p.Backend);
                Assert.IsFalse(result.HasValue);

                // compare input with evaluation
                result = f.Find((i1, o) => o, listSize: p.ListSize, backend: p.Backend);
                Assert.IsTrue(result.HasValue);

                Assert.IsTrue(f.Evaluate(result.Value));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value));

                Stop(p);
            }
        }

        /// <summary>
        /// Check a predicate is valid.
        /// </summary>
        /// <typeparam name="T1">First input type..</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <param name="function">The predicate.</param>
        public static void CheckValid<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> function)
        {
            // If testing on strings, only use Z3 backend (DD does not support strings)
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType
             || typeof(T2) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                // prove that it is valid
                var f = Function(function);
                var result = f.Find((i1, i2, o) => Not(o), listSize: p.ListSize, backend: p.Backend);
                Assert.IsFalse(result.HasValue);

                // compare input with evaluation
                result = f.Find((i1, i2, o) => o, listSize: p.ListSize, backend: p.Backend);
                Assert.IsTrue(result.HasValue);

                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));

                Stop(p);
            }
        }

        /// <summary>
        /// Check a predicate is valid.
        /// </summary>
        /// <typeparam name="T1">First input type..</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <typeparam name="T3">Third input type.</typeparam>
        /// <param name="function">The predicate.</param>
        public static void CheckValid<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> function)
        {
            // If testing on strings, only use Z3 backend (DD does not support strings)
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType
             || typeof(T2) == ReflectionUtilities.StringType
             || typeof(T3) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                // prove that it is valid
                var f = Function(function);
                var result = f.Find((i1, i2, i3, o) => Not(o), listSize: p.ListSize, backend: p.Backend);
                Assert.IsFalse(result.HasValue);

                // compare input with evaluation
                result = f.Find((i1, i2, i3, o) => o, listSize: p.ListSize, backend: p.Backend);
                Assert.IsTrue(result.HasValue);

                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3));

                Stop(p);
            }
        }

        /// <summary>
        /// Check a predicate is valid.
        /// </summary>
        /// <typeparam name="T1">First input type..</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <typeparam name="T3">Third input type.</typeparam>
        /// <typeparam name="T4">Fourth input input.</typeparam>
        /// <param name="function">The predicate.</param>
        public static void CheckValid<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> function)
        {
            // If testing on strings, only use Z3 backend (DD does not support strings)
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType
             || typeof(T2) == ReflectionUtilities.StringType
             || typeof(T3) == ReflectionUtilities.StringType
             || typeof(T4) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                // prove that it is valid
                var f = Function(function);
                var result = f.Find((i1, i2, i3, i4, o) => Not(o), listSize: p.ListSize, backend: p.Backend);
                Assert.IsFalse(result.HasValue);

                // compare input with evaluation
                result = f.Find((i1, i2, i3, i4, o) => o, listSize: p.ListSize, backend: p.Backend);
                Assert.IsTrue(result.HasValue);

                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4));

                Stop(p);
            }
        }

        /// <summary>
        /// Check that a predicate is not valid.
        /// </summary>
        /// <typeparam name="T1">First input type.</typeparam>
        /// <param name="function">The predicate.</param>
        public static void CheckNotValid<T1>(Func<Zen<T1>, Zen<bool>> function)
        {
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                // prove that it is not valid
                var f = Function(function);
                var result = f.Find((i1, o) => Not(o), listSize: p.ListSize, backend: p.Backend);
                Assert.IsTrue(result.HasValue);

                // compare input with evaluation
                Assert.IsTrue(f.Evaluate(result.Value));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value));

                Stop(p);
            }
        }

        /// <summary>
        /// Check that a predicate is not valid.
        /// </summary>
        /// <typeparam name="T1">First input type.</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <param name="function">The predicate.</param>
        public static void CheckNotValid<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> function)
        {
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType
             || typeof(T2) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                var f = Function(function);
                var result = f.Find((i1, i2, o) => Not(o), listSize: p.ListSize, backend: p.Backend);
                Assert.IsTrue(result.HasValue);

                // compare input with evaluation
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));

                Stop(p);
            }
        }

        /// <summary>
        /// Check that the backends agree on the result.
        /// </summary>
        /// <param name="function">The function.</param>
        public static void CheckAgreement(Func<Zen<bool>> function)
        {
            foreach (var p in parameters)
            {
                Start(p);

                var f = Function(function);
                var result = f.Assert(o => o, backend: p.Backend);

                Assert.AreEqual(f.Evaluate(), result);
                f.Compile();
                Assert.AreEqual(f.Evaluate(), result);

                Stop(p);
            }
        }

        /// <summary>
        /// Check that the backends agree on the result.
        /// </summary>
        /// <param name="function">The function.</param>
        public static void CheckAgreement<T1>(Func<Zen<T1>, Zen<bool>> function)
        {
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                var f = Function(function);
                var result = f.Find((i1, o) => o, listSize: p.ListSize, backend: p.Backend);

                if (result.HasValue)
                {
                    Assert.IsTrue(f.Evaluate(result.Value));
                    f.Compile();
                    Assert.IsTrue(f.Evaluate(result.Value));
                }

                Stop(p);
            }
        }

        /// <summary>
        /// Check that the backends agree on the result.
        /// </summary>
        /// <typeparam name="T1">First input type.</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <param name="function">The function.</param>
        public static void CheckAgreement<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> function)
        {
            TestParameter[] selectedParams = parameters;
            if (typeof(T1) == ReflectionUtilities.StringType
             || typeof(T2) == ReflectionUtilities.StringType)
            {
                selectedParams = stringParameters;
            }
            foreach (var p in selectedParams)
            {
                Start(p);

                var f = Function(function);
                var result = f.Find((i1, i2, o) => o, listSize: p.ListSize, backend: p.Backend);

                if (result.HasValue)
                {
                    Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                    f.Compile();
                    Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                }

                Stop(p);
            }
        }

        /// <summary>
        /// Apply a collection or rules in order.
        /// </summary>
        /// <typeparam name="TInput">Input type.</typeparam>
        /// <typeparam name="TReturn">Return type.</typeparam>
        /// <typeparam name="TRule">Rule type.</typeparam>
        /// <param name="input">Initial input.</param>
        /// <param name="deflt">Default return.</param>
        /// <param name="ruleMatch">Whether a rule matches.</param>
        /// <param name="ruleAction">What action to apply if a match.</param>
        /// <param name="ruleReturn">Whether and what to return at a rule.</param>
        /// <param name="rules">The collection of rules.</param>
        /// <returns></returns>
        public static Zen<TReturn> ApplyOrderedRules<TInput, TReturn, TRule>(
            Zen<TInput> input,
            Zen<TReturn> deflt,
            Func<TRule, Zen<TInput>, int, Zen<bool>> ruleMatch,
            Func<TRule, Zen<TInput>, int, Zen<TInput>> ruleAction,
            Func<TRule, Zen<TInput>, int, Zen<Option<TReturn>>> ruleReturn,
            TRule[] rules)
        {
            return ApplyOrderedRules(input, deflt, ruleMatch, ruleAction, ruleReturn, rules, 0);
        }

        private static Zen<TReturn> ApplyOrderedRules<TInput, TReturn, TRule>(
            Zen<TInput> input,
            Zen<TReturn> deflt,
            Func<TRule, Zen<TInput>, int, Zen<bool>> ruleMatch,
            Func<TRule, Zen<TInput>, int, Zen<TInput>> ruleAction,
            Func<TRule, Zen<TInput>, int, Zen<Option<TReturn>>> ruleReturn,
            TRule[] rules,
            int i)
        {
            if (i == rules.Length)
            {
                return deflt;
            }

            var rule = rules[i];

            // whether the current rule is a match
            var match = ruleMatch(rule, input, i + 1);

            // get the new value after modifications
            var modifiedInput = If(match, ruleAction(rule, input, i + 1), input);

            // whether or not we should return here
            var ret = ruleReturn(rule, modifiedInput, i + 1);

            var applyRest = ApplyOrderedRules(modifiedInput, deflt, ruleMatch, ruleAction, ruleReturn, rules, i + 1);

            return If(And(match, ret.HasValue()), ret.Value(), applyRest);
        }

        /// <summary>
        /// An object with one field.
        /// </summary>
        internal class Object1
        {
            public int Field1 { get; set; }
        }

        /// <summary>
        /// An object with two fields.
        /// </summary>
        internal class Object2
        {
            public int Field1 { get; set; }

            public int Field2;
        }

        /// <summary>
        /// An object with three fields.
        /// </summary>
        internal class Object3
        {
            public int Field1 { get; set; }

            public int Field2 { get; set; }

            public int Field3;
        }

        /// <summary>
        /// An object with four fields.
        /// </summary>
        internal class Object4
        {
            public int Field1 { get; set; }

            public int Field2 { get; set; }

            public int Field3 { get; set; }

            public int Field4;
        }

        /// <summary>
        /// An object with five fields.
        /// </summary>
        internal class Object5
        {
            public int Field1 { get; set; }

            public int Field2 { get; set; }

            public int Field3 { get; set; }

            public int Field4 { get; set; }

            public int Field5;
        }

        /// <summary>
        /// An object with six fields.
        /// </summary>
        internal class Object6
        {
            public int Field1 { get; set; }

            public int Field2 { get; set; }

            public int Field3 { get; set; }

            public int Field4 { get; set; }

            public int Field5 { get; set; }

            public int Field6;
        }

        /// <summary>
        /// An object with seven fields.
        /// </summary>
        internal class Object7
        {
            public int Field1 { get; set; }

            public int Field2 { get; set; }

            public int Field3 { get; set; }

            public int Field4 { get; set; }

            public int Field5 { get; set; }

            public int Field6 { get; set; }

            public int Field7;
        }

        /// <summary>
        /// An object with eight fields.
        /// </summary>
        internal class Object8
        {
            public int Field1 { get; set; }

            public int Field2 { get; set; }

            public int Field3 { get; set; }

            public int Field4 { get; set; }

            public int Field5 { get; set; }

            public int Field6 { get; set; }

            public int Field7 { get; set; }

            public int Field8;
        }

        /// <summary>
        /// An object with eight fields.
        /// </summary>
        internal class ObjectField1
        {
            public int Field1;
        }

        private class TestParameter
        {
            public Backend Backend { get; set; }

            public bool Simplify { get; set; }

            public int ListSize { get; set; }
        }
    }
}
