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
            CheckDefault<(int, int)>((0, 0));
            CheckDefault<Tuple<int, int>>(new Tuple<int, int>(0, 0));
            CheckDefault<Option<int>>(Option.None<int>());

            var o = ReflectionUtilities.GetDefaultValue<Object2>();
            Assert.AreEqual(o.Field1, 0);
            Assert.AreEqual(o.Field2, 0);

            var l = ReflectionUtilities.GetDefaultValue<IList<int>>();
            Assert.AreEqual(0, l.Count);

            var d = ReflectionUtilities.GetDefaultValue<IDictionary<int, int>>();
            Assert.AreEqual(0, d.Count);
        }

        private void CheckDefault<T>(object o)
        {
            Assert.AreEqual(ReflectionUtilities.GetDefaultValue<T>(), o);
        }
    }
}