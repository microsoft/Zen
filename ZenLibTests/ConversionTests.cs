// <copyright file="ConstantTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests conversion from values to Zen values.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConversionTests
    {
        /// <summary>
        /// Test that we can convert C# values to Zen values.
        /// </summary>
        [TestMethod]
        public void TestConversions()
        {
            CheckAgreement<bool>(x => x == true);
            CheckAgreement<byte>(x => x == 3);
            CheckAgreement<short>(x => x == 4);
            CheckAgreement<ushort>(x => x == 5);
            CheckAgreement<int>(x => x == 6);
            CheckAgreement<uint>(x => x == 7);
            CheckAgreement<long>(x => x == 8);
            CheckAgreement<ulong>(x => x == 9);
            CheckAgreement<Option<int>>(x => x == Option.Some(3));
            CheckAgreement<Tuple<int, int>>(x => x == new Tuple<int, int>(1, 2));
            CheckAgreement<(int, int)>(x => x == (1, 2));
            CheckAgreement<IList<int>>(x => x == new List<int>() { 1, 2, 3 });
            CheckAgreement<IList<IList<int>>>(x => x == new List<IList<int>>() { new List<int>() { 1 } });
            CheckAgreement<FiniteString>(x => x == new FiniteString("hello"));
            CheckAgreement<Object2>(x => x == new Object2 { Field1 = 1, Field2 = 2 });

            CheckAgreement<IDictionary<int, int>>(x =>
            {
                Zen<IDictionary<int, int>> y = new Dictionary<int, int>() { { 1, 2 } };
                return y.ContainsKey(4);
            });
        }

        /// <summary>
        /// Test that we can evaluate values correctly..
        /// </summary>
        [TestMethod]
        public void TestEvaluation()
        {
            CheckEqual(true);
            CheckEqual((byte)1);
            CheckEqual((short)2);
            CheckEqual((ushort)3);
            CheckEqual(4);
            CheckEqual(5U);
            CheckEqual(6L);
            CheckEqual(7UL);
            CheckEqual(Option.None<int>());
            CheckEqual(Option.Some(8));
            CheckEqual(new Tuple<int, int>(9, 10));
            CheckEqual((9, 10));
            CheckEqual(new FiniteString("hello"));
            CheckEqualLists(new List<int>() { 1, 2, 3 });
        }

        /// <summary>
        /// Check we convert a value correctly.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckEqual<T>(T value)
        {
            var f = Function<T>(() => value);
            Assert.AreEqual(value, f.Evaluate());
        }

        /// <summary>
        /// Check we convert a list correctly.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckEqualLists<T>(IList<T> value)
        {
            var f = Function(() => Lift(value));
            var result = f.Evaluate();

            Assert.AreEqual(value.Count, result.Count);

            for (int i = 0; i < value.Count; i++)
            {
                Assert.AreEqual(value[i], result[i]);
            }
        }
    }
}