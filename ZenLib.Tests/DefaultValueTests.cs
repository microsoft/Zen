﻿// <copyright file="DefaultValueTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
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
        /// Test integer minus with constants.
        /// </summary>
        [TestMethod]
        public void TestDefaultValues1()
        {
            Assert.IsTrue(new ZenFunction<Option<bool>>(() => Option.Null<bool>()).Assert(v => v.Value() == false));
            Assert.IsTrue(new ZenFunction<Option<byte>>(() => Option.Null<byte>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(new ZenFunction<Option<char>>(() => Option.Null<char>()).Assert(v => v.Value() == '0'));
            Assert.IsTrue(new ZenFunction<Option<short>>(() => Option.Null<short>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(new ZenFunction<Option<ushort>>(() => Option.Null<ushort>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(new ZenFunction<Option<int>>(() => Option.Null<int>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(new ZenFunction<Option<uint>>(() => Option.Null<uint>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(new ZenFunction<Option<long>>(() => Option.Null<long>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(new ZenFunction<Option<ulong>>(() => Option.Null<ulong>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(new ZenFunction<Option<BigInteger>>(() => Option.Null<BigInteger>()).Assert(v => v.Value() == new BigInteger(0)));
            Assert.IsTrue(new ZenFunction<Option<Real>>(() => Option.Null<Real>()).Assert(v => v.Value() == new Real(0)));
            Assert.IsTrue(new ZenFunction<Option<Seq<bool>>>(() => Option.Null<Seq<bool>>()).Assert(v => v.Value() == new Seq<bool>()));
            Assert.IsTrue(new ZenFunction<Option<FSeq<bool>>>(() => Option.Null<FSeq<bool>>()).Assert(v => v.Value().IsEmpty()));
            Assert.IsTrue(new ZenFunction<Option<CMap<bool, bool>>>(() => Option.Null<CMap<bool, bool>>()).Assert(v => v.Value().Get(true) == false));
        }

        /// <summary>
        /// Test that default values are correct.
        /// </summary>
        [TestMethod]
        public void TestDefaultValues2()
        {
            CheckDefault<bool>(false);
            CheckDefault<byte>((byte)0);
            CheckDefault<char>('0');
            CheckDefault<short>((short)0);
            CheckDefault<ushort>((ushort)0);
            CheckDefault<int>(0);
            CheckDefault<uint>(0U);
            CheckDefault<long>(0L);
            CheckDefault<ulong>(0UL);
            CheckDefault<Int<_1>>(new Int<_1>(0));
            CheckDefault<Int<_2>>(new Int<_2>(0));
            CheckDefault<UInt<_1>>(new UInt<_1>(0));
            CheckDefault<UInt<_2>>(new UInt<_2>(0));
            CheckDefault<string>(string.Empty);
            CheckDefault<BigInteger>(new BigInteger(0));
            CheckDefault<Real>(new Real(0));
            CheckDefault<Pair<int, int>>(new Pair<int, int>(0, 0));
            CheckDefault<Option<int>>(Option.None<int>());
            CheckDefault<CMap<int, int>>(new CMap<int, int>());

            var o = ReflectionUtilities.GetDefaultValue<Object2>();
            Assert.AreEqual(o.Field1, 0);
            Assert.AreEqual(o.Field2, 0);

            var l = ReflectionUtilities.GetDefaultValue<FSeq<int>>();
            Assert.AreEqual(0, l.Count());

            var m = ReflectionUtilities.GetDefaultValue<Map<byte, byte>>();
            Assert.AreEqual(0, m.Count());
            var v = Option.Null<Map<byte, byte>>();
        }

        private void CheckDefault<T>(object o)
        {
            Assert.AreEqual(ReflectionUtilities.GetDefaultValue<T>(), o);
        }
    }
}