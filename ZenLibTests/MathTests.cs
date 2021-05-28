// <copyright file="MathTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Tests.Network;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MathTests
    {
        /// <summary>
        /// Test addition has the zero identity.
        /// </summary>
        [TestMethod]
        public void TestAdditionZeroIdentity()
        {
            CheckValid<byte>(x => x + 0 == x);
            CheckValid<short>(x => x + 0 == x);
            CheckValid<ushort>(x => x + 0 == x);
            CheckValid<int>(x => x + 0 == x);
            CheckValid<uint>(x => x + 0 == x);
            CheckValid<long>(x => x + 0 == x);
            CheckValid<ulong>(x => x + 0 == x);
            CheckValid<BigInteger>(x => x + new BigInteger(0) == x);
        }

        /// <summary>
        /// Test addition is commutative.
        /// </summary>
        [TestMethod]
        public void TestAdditionCommutative()
        {
            RandomBytes(x => CheckValid<byte>(y => x + y == y + x));
            RandomBytes(x => CheckValid<short>(y => x + y == y + x));
            RandomBytes(x => CheckValid<ushort>(y => x + y == y + x));
            RandomBytes(x => CheckValid<int>(y => x + y == y + x));
            RandomBytes(x => CheckValid<uint>(y => x + y == y + x));
            RandomBytes(x => CheckValid<long>(y => x + y == y + x));
            RandomBytes(x => CheckValid<ulong>(y => x + y == y + x));
            RandomBytes(x => CheckValid<BigInteger>(y => new BigInteger(x) + y == y + new BigInteger(x)));
        }

        /// <summary>
        /// Test subtraction has the zero identity.
        /// </summary>
        [TestMethod]
        public void TestSubtractionZeroIdentity()
        {
            CheckValid<byte>(x => x - 0 == x);
            CheckValid<short>(x => x - 0 == x);
            CheckValid<ushort>(x => x - 0 == x);
            CheckValid<int>(x => x - 0 == x);
            CheckValid<uint>(x => x - 0 == x);
            CheckValid<long>(x => x - 0 == x);
            CheckValid<ulong>(x => x - 0 == x);
            CheckValid<BigInteger>(x => x - new BigInteger(0) == x);
        }

        /// <summary>
        /// Test an exact value with subtraction.
        /// </summary>
        [TestMethod]
        public void TestSubtractionValue()
        {
            CheckValid<byte>(x => Implies(x == 4, x - 2 == 2));
        }

        /// <summary>
        /// Test addition and subtraction are inverses.
        /// </summary>
        [TestMethod]
        public void TestAdditionSubtraction()
        {
            RandomBytes(r => CheckValid<byte>(x => x + r - x == r));
        }

        /// <summary>
        /// Test adding twice.
        /// </summary>
        [TestMethod]
        public void TestAddTwice()
        {
            CheckAgreement<byte>(x => x + x == 10);
        }

        /// <summary>
        /// Test agreement on subtraction.
        /// </summary>
        [TestMethod]
        public void TestSubtractValue()
        {
            CheckAgreement<byte>(x => x - 10 == 3);
        }

        /// <summary>
        /// Test adding multiple values is solved correctly.
        /// </summary>
        [TestMethod]
        public void TestAddMultipleValues()
        {
            var f1 = new ZenFunction<byte, byte, bool>((w, x) => w + x == 7);
            var f2 = new ZenFunction<byte, byte, byte, bool>((w, x, y) => w + x + y == 7);
            var f3 = new ZenFunction<byte, byte, byte, byte, bool>((w, x, y, z) => w + x + y + z == 7);
            var r1 = f1.Find((i1, i2, o) => o);
            var r2 = f2.Find((i1, i2, i3, o) => o);
            var r3 = f3.Find((i1, i2, i3, i4, o) => o);

            Assert.IsTrue(r1.HasValue);
            Assert.IsTrue(r2.HasValue);
            Assert.IsTrue(r3.HasValue);

            Assert.AreEqual(7, (byte)(r1.Value.Item1 + r1.Value.Item2));
            Assert.AreEqual((byte)7, (byte)(r2.Value.Item1 + r2.Value.Item2 + r2.Value.Item3));
            Assert.AreEqual(7, (byte)(r3.Value.Item1 + r3.Value.Item2 + r3.Value.Item3 + r3.Value.Item4));

            Assert.IsTrue(f1.Evaluate(2, 5));
            Assert.IsFalse(f1.Evaluate(2, 6));

            Assert.IsTrue(f2.Evaluate(2, 1, 4));
            Assert.IsFalse(f2.Evaluate(5, 5, 0));

            Assert.IsTrue(f3.Evaluate(1, 1, 2, 3));
            Assert.IsFalse(f3.Evaluate(3, 0, 1, 1));
        }

        /// <summary>
        /// Test multply evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestMultiplyConstants()
        {
            var f1 = new ZenFunction<byte, byte>(x => x * 2);
            var f2 = new ZenFunction<short, short>(x => x * 3);
            var f3 = new ZenFunction<ushort, ushort>(x => x * 4);
            var f4 = new ZenFunction<int, int>(x => x * 5);
            var f5 = new ZenFunction<uint, uint>(x => x * 6);
            var f6 = new ZenFunction<long, long>(x => x * 7L);
            var f7 = new ZenFunction<ulong, ulong>(x => x * 8L);
            var f8 = new ZenFunction<BigInteger, BigInteger>(x => x * new BigInteger(9));

            Assert.AreEqual((byte)4, f1.Evaluate(2));
            Assert.AreEqual((short)9, f2.Evaluate(3));
            Assert.AreEqual((ushort)16, f3.Evaluate(4));
            Assert.AreEqual(25, f4.Evaluate(5));
            Assert.AreEqual(36U, f5.Evaluate(6));
            Assert.AreEqual(49L, f6.Evaluate(7));
            Assert.AreEqual(64UL, f7.Evaluate(8));
            Assert.AreEqual(new BigInteger(72), f8.Evaluate(8));
        }

        /// <summary>
        /// Test that max is at least as big as both arguments.
        /// </summary>
        [TestMethod]
        public void TestMaxGreaterThan()
        {
            CheckValid<byte, byte>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
            CheckValid<short, short>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
            CheckValid<ushort, ushort>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
            CheckValid<int, int>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
            CheckValid<uint, uint>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
            CheckValid<long, long>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
            CheckValid<ulong, ulong>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
            CheckValid<BigInteger, BigInteger>((x, y) => And(Max(x, y) >= x, Max(x, y) >= y));
        }

        /// <summary>
        /// Test that Min is less than or equal to both arguments.
        /// </summary>
        [TestMethod]
        public void TestMinLessThan()
        {
            CheckValid<byte, byte>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
            CheckValid<short, short>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
            CheckValid<ushort, ushort>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
            CheckValid<int, int>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
            CheckValid<uint, uint>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
            CheckValid<long, long>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
            CheckValid<ulong, ulong>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
            CheckValid<BigInteger, BigInteger>((x, y) => And(Min(x, y) <= x, Min(x, y) <= y));
        }

        /// <summary>
        /// Test that Max returns one of its arguments.
        /// </summary>
        [TestMethod]
        public void TestMaxSelective()
        {
            CheckValid<byte, byte>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
            CheckValid<short, short>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
            CheckValid<ushort, ushort>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
            CheckValid<int, int>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
            CheckValid<uint, uint>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
            CheckValid<long, long>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
            CheckValid<ulong, ulong>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
            CheckValid<BigInteger, BigInteger>((x, y) => Or(Max(x, y) == x, Max(x, y) == y));
        }

        /// <summary>
        /// Test that Min returns one of its arguments.
        /// </summary>
        [TestMethod]
        public void TestMinSelective()
        {
            CheckValid<byte, byte>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
            CheckValid<short, short>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
            CheckValid<ushort, ushort>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
            CheckValid<int, int>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
            CheckValid<uint, uint>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
            CheckValid<long, long>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
            CheckValid<ulong, ulong>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
            CheckValid<BigInteger, BigInteger>((x, y) => Or(Min(x, y) == x, Min(x, y) == y));
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        public void TestEqualityComposite()
        {
            CheckAgreement<(byte, byte), (byte, byte)>((x, y) => x == y);
            CheckAgreement<Pair<byte, byte>, Pair<byte, byte>>((x, y) => x == y);
            CheckAgreement<Option<byte>, Option<byte>>((x, y) => x == y);
            CheckAgreement<Packet, Packet>((x, y) => x == y);
            CheckAgreement<ObjectField1, ObjectField1>((x, y) => x == y);
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEqualityCompositeException2()
        {
            CheckAgreement<IDictionary<byte, byte>, IDictionary<byte, byte>>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddException()
        {
            _ = EmptyList<int>() + EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSubtractException()
        {
            _ = EmptyList<int>() - EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMultiplyException()
        {
            _ = EmptyList<int>() * EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGeqException()
        {
            _ = EmptyList<int>() >= EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLeqException()
        {
            _ = EmptyList<int>() <= EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGtException()
        {
            _ = EmptyList<int>() > EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLtException()
        {
            _ = EmptyList<int>() < EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseOrException()
        {
            _ = EmptyList<int>() | EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseAndException()
        {
            _ = EmptyList<int>() & EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseNotException()
        {
            _ = ~EmptyList<int>();
        }

        /// <summary>
        /// Test an exception is thrown for non-integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseXorException()
        {
            _ = EmptyList<int>() ^ EmptyList<int>();
        }
    }
}