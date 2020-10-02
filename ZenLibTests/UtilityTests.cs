// <copyright file="UtilityTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for the Zen option type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UtilityTests
    {
        /// <summary>
        /// Test merging dictionaries with the same keys.
        /// </summary>
        [TestMethod]
        public void TestDictionaryMergeSameKeys()
        {
            var d1 = ImmutableDictionary<int, int>.Empty.Add(1, 1).Add(2, 4).Add(3, 9);
            var d2 = ImmutableDictionary<int, int>.Empty.Add(1, 1).Add(2, 2).Add(3, 3);
            var d3 = CommonUtilities.Merge(d1, d2, (key, v1, v2) => Option.Some((v1.Value + v2.Value) % 3));

            Assert.AreEqual(2, d3[1]);
            Assert.AreEqual(0, d3[2]);
            Assert.AreEqual(0, d3[3]);
        }

        /// <summary>
        /// Test merging dictionaries with different keys.
        /// </summary>
        [TestMethod]
        public void TestDictionaryMergeDifferentKeys()
        {
            var d1 = ImmutableDictionary<int, int>.Empty.Add(1, 1);
            var d2 = ImmutableDictionary<int, int>.Empty.Add(2, 1);
            var d3 = CommonUtilities.Merge(d1, d2, (key, v1, v2) => Option.Some(3));

            Assert.AreEqual(3, d3[1]);
            Assert.AreEqual(3, d3[2]);
        }

        /// <summary>
        /// Test mapping over a dictionary.
        /// </summary>
        [TestMethod]
        public void TestDictionaryMap()
        {
            var d1 = ImmutableDictionary<int, int>.Empty.Add(1, 1).Add(2, 4).Add(3, 9);
            var d2 = CommonUtilities.Map(d1, v => v % 2 == 0);

            Assert.AreEqual(false, d2[1]);
            Assert.AreEqual(true, d2[2]);
            Assert.AreEqual(false, d2[3]);
        }

        /// <summary>
        /// Test splitting a list at the head.
        /// </summary>
        [TestMethod]
        public void TestSplitHead()
        {
            var list = ImmutableList<int>.Empty.Add(1).Add(2);
            var (hd, tl) = CommonUtilities.SplitHead(list);
            Assert.AreEqual(1, hd);
            Assert.AreEqual(1, tl.Count);
        }

        /// <summary>
        /// Test splitting an empty list at the head.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSplitHeadEmpty()
        {
            var list = ImmutableList<int>.Empty;
            var _ = CommonUtilities.SplitHead(list);
        }

        /// <summary>
        /// Test a type not supported by Zen.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestTypeVisitorListException()
        {
            TestHelper.CheckAgreement<List<int>>(l => Language.True());
        }

        /// <summary>
        /// Test a type not supported by Zen.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestTypeVisitorDictionaryException()
        {
            TestHelper.CheckAgreement<Dictionary<int, int>>(d => Language.True());
        }

        /// <summary>
        /// Test a type not supported by Zen.
        /// </summary>
        [TestMethod]
        public void TestReflectionWith()
        {
            var s1 = new Test { Field = 0 };
            var s2 = ReflectionUtilities.WithField<Test>(s1, "Field", 1);
            Assert.AreEqual(1, s2.Field);
        }

        /// <summary>
        /// Test that we don't throw a stack overflow exception.
        /// </summary>
        [TestMethod]
        public void TestStackOverflow()
        {
            Zen<int> x = Language.Arbitrary<int>();
            Zen<int> y = Language.Arbitrary<int>();

            for (int i = 0; i < 20000; i++)
            {
                y = Language.If(x >= i, i, y);
            }

            y.Simplify();
        }

        private struct Test
        {
            public int Field { get; set; }
        }
    }
}
