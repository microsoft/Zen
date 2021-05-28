// <copyright file="RandomGenerationTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.Generation;

    /// <summary>
    /// Tests for random type generation.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RandomGenerationTests
    {
        /// <summary>
        /// Test that random generation returns the right type.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(bool))]
        [DataRow(typeof(byte))]
        [DataRow(typeof(short))]
        [DataRow(typeof(ushort))]
        [DataRow(typeof(int))]
        [DataRow(typeof(uint))]
        [DataRow(typeof(long))]
        [DataRow(typeof(ulong))]
        [DataRow(typeof(Int1))]
        [DataRow(typeof(UInt1))]
        [DataRow(typeof(Int2))]
        [DataRow(typeof(UInt2))]
        [DataRow(typeof(Int3))]
        [DataRow(typeof(UInt3))]
        [DataRow(typeof(Int9))]
        [DataRow(typeof(UInt9))]
        [DataRow(typeof(BigInteger))]
        [DataRow(typeof(string))]
        [DataRow(typeof(string))]
        [DataRow(typeof(FiniteString))]
        [DataRow(typeof(TestHelper.Object1))]
        [DataRow(typeof(TestHelper.Object2))]
        [DataRow(typeof(TestHelper.Object3))]
        [DataRow(typeof(TestHelper.Object4))]
        [DataRow(typeof(TestHelper.Object5))]
        [DataRow(typeof(TestHelper.Object6))]
        [DataRow(typeof(TestHelper.Object7))]
        [DataRow(typeof(TestHelper.Object8))]
        [DataRow(typeof(IList<int>))]
        [DataRow(typeof(IDictionary<int, bool>))]
        [DataRow(typeof(IDictionary<byte, short>))]
        [DataRow(typeof(Option<int>))]
        [DataRow(typeof(Option<int>))]
        [DataRow(typeof(Option<int>))]
        [DataRow(typeof(Option<int>))]
        [DataRow(typeof(Option<int>))]
        [DataRow(typeof(Option<int>))]
        [DataRow(typeof(Option<int>))]
        [DataRow(typeof(IList<Option<ulong>>))]
        [DataRow(typeof(Pair<bool, ushort>))]
        [DataRow(typeof((int, int)))]
        [DataRow(typeof(bool))]
        public void TestRandomGenerationForType(Type type)
        {
            var constants = new Dictionary<Type, ISet<object>>
            {
                [typeof(int)] = new HashSet<object>() { 1, 2, 3, 4, 5, 6 },
            };

            var generator = new RandomValueGenerator(constants);
            var result = ReflectionUtilities.ApplyTypeVisitor(generator, type);
            Assert.IsTrue(type.IsAssignableFrom(result.GetType()));
        }

        /// <summary>
        /// Test that random generation returns the right type.
        /// </summary>
        [TestMethod]
        public void TestRandomGenerationHelper()
        {
            var o2 = Language.Generate<TestHelper.Object2>();
            Assert.IsTrue(o2 is TestHelper.Object2);
        }
    }
}