// <copyright file="DefaultValueTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests creating default values.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DefaultValueTests
    {
        /// <summary>
        /// Test that default values are correct..
        /// </summary>
        [TestMethod]
        public void TestDefaultValues()
        {
            CheckDefault<bool>(false);
            CheckDefault<byte>((byte)0);
            CheckDefault<short>((short)0);
            CheckDefault<ushort>((ushort)0);
            CheckDefault<int>(0);
            CheckDefault<uint>(0U);
            CheckDefault<long>(0L);
            CheckDefault<ulong>(0UL);
            CheckDefault<Int1>(new Int1(0));
            CheckDefault<Int2>(new Int2(0));
            CheckDefault<UInt1>(new UInt1(0));
            CheckDefault<UInt2>(new UInt2(0));
            CheckDefault<string>(string.Empty);
            CheckDefault<BigInteger>(new BigInteger(0));
            CheckDefault<Pair<int, int>>(new Pair<int, int> { Item1 = 0, Item2 = 0 });
            CheckDefault<Option<int>>(Option.None<int>());

            var o = ReflectionUtilities.GetDefaultValue<Object2>();
            Assert.AreEqual(o.Field1, 0);
            Assert.AreEqual(o.Field2, 0);

            var l = ReflectionUtilities.GetDefaultValue<IList<int>>();
            Assert.AreEqual(0, l.Count);

            var m = ReflectionUtilities.GetDefaultValue<IDictionary<byte, byte>>();
            Assert.AreEqual(0, m.Count);
            var v = Option.Null<IDictionary<byte, byte>>();

            var d = ReflectionUtilities.GetDefaultValue<FMap<int, int>>();
        }

        private void CheckDefault<T>(object o)
        {
            Assert.AreEqual(ReflectionUtilities.GetDefaultValue<T>(), o);
        }
    }
}