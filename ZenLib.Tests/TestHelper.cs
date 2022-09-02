// <copyright file="TestHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Solver;
    using static ZenLib.Zen;

    /// <summary>
    /// Test helper functions for checking correctness.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TestHelper
    {
        /// <summary>
        /// The default bdd list size.
        /// </summary>
        private static int defaultBddListSize = 4;

        /// <summary>
        /// Number of random inputs to try per test.
        /// </summary>
        private static int numRandomTests = 8;

        /// <summary>
        /// Deterministic random number generator.
        /// </summary>
        private static Random random = new Random(7);

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

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <returns>The string.</returns>
        public static string RandomString()
        {
            string s = string.Empty;
            var len = random.Next(0, 5);
            for (int j = 0; j < len; j++)
            {
                var c = (char)random.Next(0, 255);
                s += c;
            }

            return s;
        }

        /// <summary>
        /// Generates a random byte.
        /// </summary>
        /// <returns>The byte.</returns>
        public static byte RandomByte()
        {
            return (byte)random.Next(0, 255);
        }

        private static Zen<T> Flatten<T>(Zen<T> expr, TestParameter p)
        {
            return expr;
        }

        private static TestParameter[] GetBoundedParameters(int bddListSize, bool runBdds)
        {
            if (runBdds)
            {
                return new TestParameter[]
                {
                    new TestParameter { SolverType = SolverType.DecisionDiagrams, ListSize = 1 },
                    new TestParameter { SolverType = SolverType.Z3, ListSize = 5 },
                };
            }
            else
            {
                return new TestParameter[]
                {
                    new TestParameter { SolverType = SolverType.Z3, ListSize = 5 },
                };
            }
        }

        private static TestParameter[] GetUnboundedParameters()
        {
            return new TestParameter[]
            {
                new TestParameter { SolverType = SolverType.Z3, ListSize = 5 },
            };
        }

        private static TestParameter[] SetParameters(int bddListSize, bool runBdds, params Type[] types)
        {
            var ps = GetBoundedParameters(bddListSize, runBdds);

            foreach (var type in types)
            {
                if (type == ReflectionUtilities.StringType || type == ReflectionUtilities.BigIntType || type == ReflectionUtilities.RealType)
                {
                    ps = GetUnboundedParameters();
                }
            }

            return ps;
        }

        /// <summary>
        /// Check a predicate is valid.
        /// </summary>
        /// <param name="function">The predicate.</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckValid<T1>(Func<Zen<T1>, Zen<bool>> function, bool runBdds = true)
        {
            var selectedParams = SetParameters(4, runBdds, typeof(T1));

            foreach (var p in selectedParams)
            {
                // prove that it is valid
                var f = Zen.Function<T1, bool>(function);
                var result = f.Find((i1, o) => Flatten(Not(o), p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsFalse(result.HasValue);

                // compare input with evaluation
                result = f.Find((i1, o) => o, depth: p.ListSize, backend: p.SolverType);
                Assert.IsTrue(result.HasValue);

                Assert.IsTrue(f.Evaluate(result.Value));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value));
            }
        }

        /// <summary>
        /// Check a predicate is valid.
        /// </summary>
        /// <typeparam name="T1">First input type..</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <param name="function">The predicate.</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckValid<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> function, bool runBdds = true)
        {
            // If testing on strings, only use Z3 backend (DD does not support strings)
            var selectedParams = SetParameters(defaultBddListSize, runBdds, typeof(T1), typeof(T2));

            foreach (var p in selectedParams)
            {
                // prove that it is valid
                var t = System.Diagnostics.Stopwatch.StartNew();
                var f = Zen.Function<T1, T2, bool>(function);
                var result = f.Find((i1, i2, o) => Flatten(Not(o), p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsFalse(result.HasValue);
                Console.WriteLine(t.ElapsedMilliseconds);

                // compare input with evaluation
                result = f.Find((i1, i2, o) => Flatten(o, p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsTrue(result.HasValue);
                Console.WriteLine(t.ElapsedMilliseconds);

                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                Console.WriteLine(t.ElapsedMilliseconds);
                f.Compile();
                Console.WriteLine(t.ElapsedMilliseconds);
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                Console.WriteLine(t.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Check a predicate is valid.
        /// </summary>
        /// <typeparam name="T1">First input type..</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <typeparam name="T3">Third input type.</typeparam>
        /// <param name="function">The predicate.</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckValid<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> function, bool runBdds = true)
        {
            var selectedParams = SetParameters(defaultBddListSize, runBdds, typeof(T1), typeof(T2), typeof(T3));

            foreach (var p in selectedParams)
            {
                // prove that it is valid
                var f = Zen.Function<T1, T2, T3, bool>(function);
                var result = f.Find((i1, i2, i3, o) => Flatten(Not(o), p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsFalse(result.HasValue);

                // compare input with evaluation
                result = f.Find((i1, i2, i3, o) => Flatten(o, p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsTrue(result.HasValue);

                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3));
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
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckValid<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> function, bool runBdds = true)
        {
            var selectedParams = SetParameters(defaultBddListSize, runBdds, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

            foreach (var p in selectedParams)
            {
                // prove that it is valid
                var f = Zen.Function<T1, T2, T3, T4, bool>(function);
                var result = f.Find((i1, i2, i3, i4, o) => Flatten(Not(o), p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsFalse(result.HasValue);

                // compare input with evaluation
                result = f.Find((i1, i2, i3, i4, o) => Flatten(o, p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsTrue(result.HasValue);

                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4));
            }
        }

        /// <summary>
        /// Check that a predicate is not valid.
        /// </summary>
        /// <typeparam name="T1">First input type.</typeparam>
        /// <param name="function">The predicate.</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckNotValid<T1>(Func<Zen<T1>, Zen<bool>> function, bool runBdds = true)
        {
            var selectedParams = SetParameters(defaultBddListSize, runBdds, typeof(T1));

            foreach (var p in selectedParams)
            {
                // prove that it is not valid
                var f = Zen.Function<T1, bool>(function);
                var result = f.Find((i1, o) => Flatten(Not(o), p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsTrue(result.HasValue);

                // compare input with evaluation
                Assert.IsTrue(f.Evaluate(result.Value));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value));
            }
        }

        /// <summary>
        /// Check that a predicate is not valid.
        /// </summary>
        /// <typeparam name="T1">First input type.</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <param name="function">The predicate.</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckNotValid<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> function, bool runBdds = true)
        {
            var selectedParams = SetParameters(defaultBddListSize, runBdds, typeof(T1), typeof(T2));

            foreach (var p in selectedParams)
            {
                var f = Zen.Function<T1, T2, bool>(function);
                var result = f.Find((i1, i2, o) => Flatten(Not(o), p), depth: p.ListSize, backend: p.SolverType);
                Assert.IsTrue(result.HasValue);

                // compare input with evaluation
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                f.Compile();
                Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
            }
        }

        /// <summary>
        /// Check that the backends agree on the result.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckAgreement(Func<Zen<bool>> function, bool runBdds = true)
        {
            foreach (var p in GetBoundedParameters(defaultBddListSize, runBdds))
            {
                var f = Zen.Function<bool>(function);
                var result = f.Assert(o => Flatten(o, p), backend: p.SolverType);

                Assert.AreEqual(f.Evaluate(), result);
                f.Compile();
                Assert.AreEqual(f.Evaluate(), result);
            }
        }

        /// <summary>
        /// Check that the backends agree on the result.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="bddListSize">The list size for bdds.</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckAgreement<T1>(Func<Zen<T1>, Zen<bool>> function, int bddListSize = 4, bool runBdds = true)
        {
            var selectedParams = SetParameters(bddListSize, runBdds, typeof(T1));

            foreach (var p in selectedParams)
            {
                var f = Zen.Function<T1, bool>(function);
                var result = f.Find((i1, o) => Flatten(o, p), depth: p.ListSize, backend: p.SolverType);
                if (result.HasValue)
                {
                    Assert.IsTrue(f.Evaluate(result.Value));
                    f.Compile();
                    Assert.IsTrue(f.Evaluate(result.Value));
                }
            }
        }

        /// <summary>
        /// Check that the backends agree on the result.
        /// </summary>
        /// <typeparam name="T1">First input type.</typeparam>
        /// <typeparam name="T2">Second input type.</typeparam>
        /// <param name="function">The function.</param>
        /// <param name="bddListSize">The maximum .</param>
        /// <param name="runBdds">Whether to run the bdd backend.</param>
        public static void CheckAgreement<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> function, int bddListSize = 4, bool runBdds = true)
        {
            var selectedParams = SetParameters(bddListSize, runBdds, typeof(T1), typeof(T2));

            foreach (var p in selectedParams)
            {
                var f = Zen.Function<T1, T2, bool>(function);
                var result = f.Find((i1, i2, o) => Flatten(o, p), depth: p.ListSize, backend: p.SolverType);

                if (result.HasValue)
                {
                    Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                    f.Compile();
                    Assert.IsTrue(f.Evaluate(result.Value.Item1, result.Value.Item2));
                }
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

            return If(And(match, ret.IsSome()), ret.Value(), applyRest);
        }

        /// <summary>
        /// An object with no fields.
        /// </summary>
        internal class Object0
        {
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
        public class Object2 : IEquatable<Object2>
        {
            /// <summary>
            /// Field 1.
            /// </summary>
            public int Field1 { get; set; }

            /// <summary>
            /// Field 2.
            /// </summary>
            public int Field2;

            /// <summary>
            /// Equality.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return Equals(obj as Object2);
            }

            /// <summary>
            /// Equality.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(Object2 other)
            {
                return other is not null &&
                       Field1 == other.Field1 &&
                       Field2 == other.Field2;
            }

            /// <summary>
            /// Hash code.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return HashCode.Combine(Field1, Field2);
            }
        }

        /// <summary>
        /// An object with two fields.
        /// </summary>
        public class Object2Different
        {
            /// <summary>
            /// Field 1.
            /// </summary>
            public int Field1 { get; set; }

            /// <summary>
            /// Field 2.
            /// </summary>
            public short Field2;
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
        internal class ObjectField1 : IEquatable<ObjectField1>
        {
            public int Field1;

            public override bool Equals(object obj)
            {
                return Equals(obj as ObjectField1);
            }

            public bool Equals(ObjectField1 other)
            {
                return other is not null &&
                       Field1 == other.Field1;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Field1);
            }
        }

        internal class ObjectWithInt
        {
            public UInt10 Field1;
        }

        private class TestParameter
        {
            public SolverType SolverType { get; set; }

            public int ListSize { get; set; }
        }
    }
}
