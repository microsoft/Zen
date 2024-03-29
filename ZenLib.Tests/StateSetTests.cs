﻿// <copyright file="StateSetTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.ModelChecking;
    using ZenLib.Tests.Network;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for state sets.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StateSetTests
    {
        /// <summary>
        /// Test a transformer fails for unbounded types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStateSetExceptionForUnboundedTypes1()
        {
            new ZenFunction<BigInteger, bool>(i => i > new BigInteger(0)).StateSet();
        }

        /// <summary>
        /// Test a stateset fails for unbounded types.
        /// </summary>
        [TestMethod]
        public void TestStateSetExceptionForUnboundedTypes2()
        {
            ZenSettings.UseLargeStack = true;

            try
            {
                new ZenFunction<BigInteger, bool>(i => i > new BigInteger(0)).StateSet();
                Assert.Fail();
            }
            catch
            {
                ZenSettings.UseLargeStack = false;
            }
        }

        /// <summary>
        /// Test a stateset with an abitrary works.
        /// </summary>
        [TestMethod]
        public void TestStateSetArbitrary()
        {
            var manager = new StateSetTransformerManager(0);
            var b = Arbitrary<bool>();
            var f1 = new ZenFunction<uint, bool>(i => Or(b, i <= 10));
            var s1 = f1.StateSet(manager);
        }

        /// <summary>
        /// Test getting an element for a stateset.
        /// </summary>
        [TestMethod]
        public void TestStateSetElements()
        {
            var manager = new StateSetTransformerManager(0);
            var f = new ZenFunction<uint, bool>(i => (i + 1U) == 10U);
            var s = f.StateSet(manager);
            Assert.AreEqual(9U, s.Element());
        }

        /// <summary>
        /// Test the variables are made canonical for state sets.
        /// </summary>
        [TestMethod]
        public void TestStateSetVariablesAlign()
        {
            var manager = new StateSetTransformerManager(0);
            var s1 = new ZenFunction<uint, bool>(i => i + 1 == 10).StateSet(manager);
            var s2 = new ZenFunction<uint, bool>(i => i + 2 >= 10).StateSet(manager);
            var s3 = s1.Intersect(s2);

            Assert.AreEqual(s1, s3);
            Assert.AreEqual(9U, s3.Element());
        }

        /// <summary>
        /// Test the variables are made canonical for state sets.
        /// </summary>
        [TestMethod]
        public void TestStateSetInterleaving()
        {
            var manager = new StateSetTransformerManager(0);
            var s1 = new ZenFunction<ushort, ushort, bool>((x, y) => x == y).StateSet(manager);
            var s2 = new ZenFunction<ushort, ushort, bool>((x, y) => And(x == 1, y <= 10)).StateSet(manager);
            var s3 = s1.Intersect(s2);

            Assert.IsFalse(s3.IsEmpty());
            Assert.AreEqual(new Pair<ushort, ushort>(1, 1), s3.Element());
        }

        /// <summary>
        /// Test checking if a set is full.
        /// </summary>
        [TestMethod]
        public void TestStateSetIsFull()
        {
            var manager = new StateSetTransformerManager(0);
            var set = Zen.FullSet<bool>(manager);
            Assert.IsTrue(set.IsFull());
        }

        /// <summary>
        /// Test checking if a set is empty.
        /// </summary>
        [TestMethod]
        public void TestStateSetIsEmpty()
        {
            var manager = new StateSetTransformerManager(0);
            var set = Zen.EmptySet<bool>(manager);
            Assert.IsTrue(set.IsEmpty());
        }

        /// <summary>
        /// Test for different types.
        /// </summary>
        [TestMethod]
        public void TestStateSetArgTypes()
        {
            var manager = new StateSetTransformerManager(0);
            Assert.IsTrue(Zen.FullSet<bool>(manager).IsFull());
            Assert.IsTrue(Zen.FullSet<byte>(manager).IsFull());
            Assert.IsTrue(Zen.FullSet<short>(manager).IsFull());
            Assert.IsTrue(Zen.FullSet<ushort>(manager).IsFull());
            Assert.IsTrue(Zen.FullSet<int>(manager).IsFull());
            Assert.IsTrue(Zen.FullSet<uint>(manager).IsFull());
            Assert.IsTrue(Zen.FullSet<long>(manager).IsFull());
            Assert.IsTrue(Zen.FullSet<ulong>(manager).IsFull());
        }

        /// <summary>
        /// Test state sets with fixed width integers.
        /// </summary>
        [TestMethod]
        public void TestStateSetFixedWidthInteger()
        {
            var manager = new StateSetTransformerManager(0);
            var set = new ZenFunction<Int<_5>, bool>(i => i <= new Int<_5>(0)).StateSet(manager);
            Assert.IsTrue(set.Element() <= new Int<_5>(0));
        }

        /// <summary>
        /// Test state set equality.
        /// </summary>
        [TestMethod]
        public void TestSetSetEquality()
        {
            var manager = new StateSetTransformerManager(0);
            var set1 = Zen.FullSet<bool>(manager).Complement();
            var set2 = Zen.FullSet<bool>(manager).Complement();
            Assert.IsTrue(set1.Equals(set2));
            Assert.IsFalse(set1.Equals(2));
        }

        /// <summary>
        /// Test getting an element for an empty set.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerNoElement()
        {
            var manager = new StateSetTransformerManager(0);
            var stateSet = new ZenFunction<uint, bool>(i => i + 2 == i + 1).StateSet(manager);
            stateSet.Element();
        }

        /// <summary>
        /// Test a transformer over an object.
        /// </summary>
        [TestMethod]
        public void TestTransformerObject()
        {
            var manager = new StateSetTransformerManager(0);
            new ZenFunction<IpHeader, bool>(p => And(p.GetDstIp().GetValue() <= 4, p.GetSrcIp().GetValue() <= 5)).StateSet(manager);
        }

        /// <summary>
        /// Test a transformer over an object with a variable width integer field.
        /// </summary>
        [TestMethod]
        public void TestTransformerObjectWithInt()
        {
            var manager = new StateSetTransformerManager(0);
            var set = new ZenFunction<TestHelper.ObjectWithInt, bool>(o => o.GetField<TestHelper.ObjectWithInt, UInt<_10>>("Field1") == new UInt<_10>(1))
                .StateSet(manager);
            Assert.AreEqual(1L, set.Element().Field1.ToLong());
        }

        /// <summary>
        /// Test packet transformations.
        /// </summary>
        [TestMethod]
        public void TestPacketSet()
        {
            var manager = new StateSetTransformerManager(0);
            var rnd = new System.Random();
            for (int j = 0; j < 2; j++)
            {
                var i = (uint)rnd.Next();
                var f = new ZenFunction<IpHeader, bool>(p =>
                {
                    return p.GetDstIp().GetValue() == i;
                });

                StateSet<IpHeader> set = f.StateSet(manager);
                Assert.AreEqual(i, set.Element().DstIp.Value);
            }
        }

        /// <summary>
        /// Test packet transformations.
        /// </summary>
        [TestMethod]
        public void TestMultipleStateSets()
        {
            var manager = new StateSetTransformerManager(0);
            var set1 = Zen.FullSet<IpHeader>(manager);
            var set2 = new ZenFunction<IpHeader, bool>(p => p.GetDstIp().GetValue() == 1).StateSet(manager);
            var set3 = new ZenFunction<uint, bool>(u => u == 2).StateSet(manager);
            Assert.IsTrue(set1.IsFull());
            Assert.AreEqual(1U, set2.Element().DstIp.Value);
            Assert.AreEqual(2U, set3.Element());
        }

        /// <summary>
        /// Test that multiple arguments works with tuples.
        /// </summary>
        [TestMethod]
        public void TestMultipleArguments()
        {
            var manager = new StateSetTransformerManager(0);
            var set = new ZenFunction<IpHeader, IpHeader, bool>((x, y) => x == y).StateSet(manager);
            var e = set.Element();
            Assert.AreEqual(e.Item1.DstIp, e.Item2.DstIp);
            Assert.AreEqual(e.Item1.SrcIp, e.Item2.SrcIp);
            Assert.AreEqual(e.Item1.SrcPort, e.Item2.DstPort);
            Assert.AreEqual(e.Item1.DstPort, e.Item2.DstPort);
        }

        /// <summary>
        /// Test that multiple arguments works with tuples.
        /// </summary>
        [TestMethod]
        public void TestMultipleArguments2()
        {
            var manager = new StateSetTransformerManager(0);
            var set = new ZenFunction<IpHeader, IpHeader, IpHeader, bool>((x, y, z) => And(x == y, y == z)).StateSet(manager);
            var e = set.Element();
            Assert.AreEqual(e.Item1.DstIp, e.Item2.DstIp);
            Assert.AreEqual(e.Item1.DstIp, e.Item3.DstIp);
            Assert.AreEqual(e.Item1.SrcIp, e.Item2.SrcIp);
            Assert.AreEqual(e.Item1.SrcIp, e.Item3.SrcIp);
            Assert.AreEqual(e.Item1.SrcPort, e.Item2.DstPort);
            Assert.AreEqual(e.Item1.SrcPort, e.Item3.DstPort);
            Assert.AreEqual(e.Item1.DstPort, e.Item2.DstPort);
            Assert.AreEqual(e.Item1.DstPort, e.Item3.DstPort);
        }

        /// <summary>
        /// Test that multiple arguments works with tuples.
        /// </summary>
        [TestMethod]
        public void TestMultipleArguments3()
        {
            var manager = new StateSetTransformerManager(0);
            var set = new ZenFunction<int, int, int, int, bool>((a, b, c, d) => And(a == 1, b == 2, c == 3, d == 4)).StateSet(manager);
            var e = set.Element();
            Assert.AreEqual(1, e.Item1);
            Assert.AreEqual(2, e.Item2);
            Assert.AreEqual(3, e.Item3);
            Assert.AreEqual(4, e.Item4);
        }

        /// <summary>
        /// Test that variabledependencies work.
        /// </summary>
        [TestMethod]
        public void TestVariableDependencies()
        {
            var manager = new StateSetTransformerManager(0);
            var set1 = new ZenFunction<int, int, bool>((a, b) => a == b).StateSet(manager);
            var set2 = new ZenFunction<int, int, bool>((a, b) => a + 1 == b + 1).StateSet(manager);
            var neither = set1.Intersect(set2.Complement());
            Assert.IsTrue(neither.IsEmpty());
        }

        /// <summary>
        /// Test that using different manager objects works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStateSetDifferentManagers()
        {
            var set1 = Zen.FullSet<uint>(new StateSetTransformerManager());
            var set2 = Zen.FullSet<uint>(new StateSetTransformerManager());
            set1.Union(set2);
        }

        /// <summary>
        /// Test that using different manager objects works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStateSetInvalidArguments2()
        {
            var set1 = Zen.FullSet<uint>(new StateSetTransformerManager());
            var set2 = Zen.FullSet<uint>(new StateSetTransformerManager());
            set1.Intersect(set2);
        }

        /// <summary>
        /// Test that using different manager objects works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStateSetInvalidType()
        {
            Zen.FullSet<string>(new StateSetTransformerManager());
        }

        /// <summary>
        /// Test that state sets are cached.
        /// </summary>
        [TestMethod]
        public void TestStateSetCaching1()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = Zen.FullSet<byte>();
            var f2 = Zen.FullSet<byte>();
            var f3 = Zen.FullSet<byte>(m1);
            var f4 = Zen.FullSet<byte>(m1);
            var f5 = Zen.FullSet<byte>(m2);
            var f6 = Zen.FullSet<byte>(m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }

        /// <summary>
        /// Test that state sets are cached.
        /// </summary>
        [TestMethod]
        public void TestStateSetCaching2()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = Zen.StateSet<byte, byte>((x, y) => true);
            var f2 = Zen.StateSet<byte, byte>((x, y) => true);
            var f3 = Zen.StateSet<byte, byte>((x, y) => true, m1);
            var f4 = Zen.StateSet<byte, byte>((x, y) => true, m1);
            var f5 = Zen.StateSet<byte, byte>((x, y) => true, m2);
            var f6 = Zen.StateSet<byte, byte>((x, y) => true, m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }

        /// <summary>
        /// Test that state sets are cached.
        /// </summary>
        [TestMethod]
        public void TestStateSetCaching3()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = Zen.StateSet<byte, byte, byte>((x, y, z) => true);
            var f2 = Zen.StateSet<byte, byte, byte>((x, y, z) => true);
            var f3 = Zen.StateSet<byte, byte, byte>((x, y, z) => true, m1);
            var f4 = Zen.StateSet<byte, byte, byte>((x, y, z) => true, m1);
            var f5 = Zen.StateSet<byte, byte, byte>((x, y, z) => true, m2);
            var f6 = Zen.StateSet<byte, byte, byte>((x, y, z) => true, m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }

        /// <summary>
        /// Test that state sets are cached.
        /// </summary>
        [TestMethod]
        public void TestStateSetCaching4()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = Zen.StateSet<byte, byte, byte, byte>((w, x, y, z) => true);
            var f2 = Zen.StateSet<byte, byte, byte, byte>((w, x, y, z) => true);
            var f3 = Zen.StateSet<byte, byte, byte, byte>((w, x, y, z) => true, m1);
            var f4 = Zen.StateSet<byte, byte, byte, byte>((w, x, y, z) => true, m1);
            var f5 = Zen.StateSet<byte, byte, byte, byte>((w, x, y, z) => true, m2);
            var f6 = Zen.StateSet<byte, byte, byte, byte>((w, x, y, z) => true, m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }
    }
}