// <copyright file="PrimitiveTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.ModelChecking;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PrimitiveTests
    {
        /// <summary>
        /// Test negative numbers.
        /// </summary>
        [TestMethod]
        public void TestNegatives()
        {
            var f = new ZenFunction<short, bool>(x => -2 > -1);
            var result = f.Find((i, o) => o, backend: Backend.DecisionDiagrams);
            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Test integer greater than.
        /// </summary>
        [TestMethod]
        public void TestIntegerGreaterThan()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i > x));
            RandomBytes(x => CheckAgreement<byte>(i => x > i));
            RandomBytes(x => CheckAgreement<short>(i => i > (short)x));
            RandomBytes(x => CheckAgreement<short>(i => (short)x > i));
            RandomBytes(x => CheckAgreement<ushort>(i => i > (ushort)x));
            RandomBytes(x => CheckAgreement<ushort>(i => (ushort)x > i));
            RandomBytes(x => CheckAgreement<int>(i => i > (int)x));
            RandomBytes(x => CheckAgreement<int>(i => (int)x > i));
            RandomBytes(x => CheckAgreement<uint>(i => i > (uint)x));
            RandomBytes(x => CheckAgreement<uint>(i => (uint)x > i));
            RandomBytes(x => CheckAgreement<long>(i => i > (long)x));
            RandomBytes(x => CheckAgreement<long>(i => (long)x > i));
            RandomBytes(x => CheckAgreement<ulong>(i => i > (ulong)x));
            RandomBytes(x => CheckAgreement<ulong>(i => (ulong)x > i));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i > new BigInteger(x)));
            RandomBytes(x => CheckAgreement<BigInteger>(i => new BigInteger(x) > i));
        }

        /// <summary>
        /// Test integer less than.
        /// </summary>
        [TestMethod]
        public void TestIntegerLessThan()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i < x));
            RandomBytes(x => CheckAgreement<byte>(i => x < i));
            RandomBytes(x => CheckAgreement<short>(i => i < (short)x));
            RandomBytes(x => CheckAgreement<short>(i => (short)x < i));
            RandomBytes(x => CheckAgreement<ushort>(i => i < (ushort)x));
            RandomBytes(x => CheckAgreement<ushort>(i => (ushort)x < i));
            RandomBytes(x => CheckAgreement<int>(i => i < (int)x));
            RandomBytes(x => CheckAgreement<int>(i => (int)x < i));
            RandomBytes(x => CheckAgreement<uint>(i => i < (uint)x));
            RandomBytes(x => CheckAgreement<uint>(i => (uint)x < i));
            RandomBytes(x => CheckAgreement<long>(i => i < (long)x));
            RandomBytes(x => CheckAgreement<long>(i => (long)x < i));
            RandomBytes(x => CheckAgreement<ulong>(i => i < (ulong)x));
            RandomBytes(x => CheckAgreement<ulong>(i => (ulong)x < i));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i < new BigInteger(x)));
            RandomBytes(x => CheckAgreement<BigInteger>(i => new BigInteger(x) < i));
        }

        /// <summary>
        /// Test integer comparison for unsigned values.
        /// </summary>
        [TestMethod]
        public void TestIntegerComparisonUnsignedLong()
        {
            Zen<ulong> a = 0xffffffffffffffff;
            Zen<ulong> b = 0x1000000000000000;
            Assert.AreEqual(True(), a > b);

            ulong c = 0xffffffffffffffff;
            ulong d = 0x1000000000000000;
            Assert.IsTrue(c > d);

            var zf = new ZenFunction<ulong, ulong, bool>((x, y) => x > y);
            Assert.IsTrue(zf.Evaluate(c, d));

            zf.Compile();
            Assert.IsTrue(zf.Evaluate(c, d));
        }

        /// <summary>
        /// Test integer comparison for unsigned values.
        /// </summary>
        [TestMethod]
        public void TestIntegerComparisonUnsignedInt()
        {
            Zen<uint> a = 0xffffffff;
            Zen<uint> b = 0x10000000;
            Assert.AreEqual(True(), a > b);

            uint c = 0xffffffff;
            uint d = 0x10000000;
            Assert.IsTrue(c > d);

            var zf = new ZenFunction<uint, uint, bool>((x, y) => x > y);
            Assert.IsTrue(zf.Evaluate(c, d));

            zf.Compile();
            Assert.IsTrue(zf.Evaluate(c, d));
        }

        /// <summary>
        /// Test integer comparison for unsigned values.
        /// </summary>
        [TestMethod]
        public void TestIntegerComparisonUnsignedShort()
        {
            Zen<ushort> a = 0xffff;
            Zen<ushort> b = 0x1000;
            Assert.AreEqual(True(), a > b);

            ushort c = 0xffff;
            ushort d = 0x1000;
            Assert.IsTrue(c > d);

            var zf = new ZenFunction<ushort, ushort, bool>((x, y) => x > y);
            Assert.IsTrue(zf.Evaluate(c, d));

            zf.Compile();
            Assert.IsTrue(zf.Evaluate(c, d));
        }

        /// <summary>
        /// Test integer less than or equal.
        /// </summary>
        [TestMethod]
        public void TestIntegerLessThanOrEqual()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i <= x));
            RandomBytes(x => CheckAgreement<byte>(i => x <= i));
            RandomBytes(x => CheckAgreement<short>(i => i <= (short)x));
            RandomBytes(x => CheckAgreement<short>(i => (short)x <= i));
            RandomBytes(x => CheckAgreement<ushort>(i => i <= (ushort)x));
            RandomBytes(x => CheckAgreement<ushort>(i => (ushort)x <= i));
            RandomBytes(x => CheckAgreement<int>(i => i <= (int)x));
            RandomBytes(x => CheckAgreement<int>(i => (int)x <= i));
            RandomBytes(x => CheckAgreement<uint>(i => i <= (uint)x));
            RandomBytes(x => CheckAgreement<uint>(i => (uint)x <= i));
            RandomBytes(x => CheckAgreement<long>(i => i <= (long)x));
            RandomBytes(x => CheckAgreement<long>(i => (long)x <= i));
            RandomBytes(x => CheckAgreement<ulong>(i => i <= (ulong)x));
            RandomBytes(x => CheckAgreement<ulong>(i => (ulong)x <= i));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i <= new BigInteger(x)));
            RandomBytes(x => CheckAgreement<BigInteger>(i => new BigInteger(x) <= i));
        }

        /// <summary>
        /// Test integer greater than or equal.
        /// </summary>
        [TestMethod]
        public void TestIntegerGreaterThanOrEqualByte()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i >= x));
            RandomBytes(x => CheckAgreement<byte>(i => x >= i));
            RandomBytes(x => CheckAgreement<short>(i => i >= (short)x));
            RandomBytes(x => CheckAgreement<short>(i => (short)x >= i));
            RandomBytes(x => CheckAgreement<ushort>(i => i >= (ushort)x));
            RandomBytes(x => CheckAgreement<ushort>(i => (ushort)x >= i));
            RandomBytes(x => CheckAgreement<int>(i => i >= (int)x));
            RandomBytes(x => CheckAgreement<int>(i => (int)x >= i));
            RandomBytes(x => CheckAgreement<uint>(i => i >= (uint)x));
            RandomBytes(x => CheckAgreement<uint>(i => (uint)x >= i));
            RandomBytes(x => CheckAgreement<long>(i => i >= (long)x));
            RandomBytes(x => CheckAgreement<long>(i => (long)x >= i));
            RandomBytes(x => CheckAgreement<ulong>(i => i >= (ulong)x));
            RandomBytes(x => CheckAgreement<ulong>(i => (ulong)x >= i));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i >= new BigInteger(x)));
            RandomBytes(x => CheckAgreement<BigInteger>(i => new BigInteger(x) >= i));
        }

        /// <summary>
        /// Test boolean equality.
        /// </summary>
        [TestMethod]
        public void TestBooleanEquality()
        {
            RandomBytes(x => CheckAgreement<bool>(b => b == (x % 2 == 0)));
        }

        /// <summary>
        /// Test integer equality.
        /// </summary>
        [TestMethod]
        public void TestIntegerEquality()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i == x));
            RandomBytes(x => CheckAgreement<byte>(i => x == i));
            RandomBytes(x => CheckAgreement<char>(i => (char)x == i));
            RandomBytes(x => CheckAgreement<short>(i => i == (short)x));
            RandomBytes(x => CheckAgreement<short>(i => (short)x == i));
            RandomBytes(x => CheckAgreement<ushort>(i => i == (ushort)x));
            RandomBytes(x => CheckAgreement<ushort>(i => (ushort)x == i));
            RandomBytes(x => CheckAgreement<int>(i => i == (int)x));
            RandomBytes(x => CheckAgreement<int>(i => (int)x == i));
            RandomBytes(x => CheckAgreement<uint>(i => i == (uint)x));
            RandomBytes(x => CheckAgreement<uint>(i => (uint)x == i));
            RandomBytes(x => CheckAgreement<long>(i => i == (long)x));
            RandomBytes(x => CheckAgreement<long>(i => (long)x == i));
            RandomBytes(x => CheckAgreement<ulong>(i => i == (ulong)x));
            RandomBytes(x => CheckAgreement<ulong>(i => (ulong)x == i));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i == new BigInteger(x)));
            RandomBytes(x => CheckAgreement<BigInteger>(i => new BigInteger(x) == i));
        }

        /// <summary>
        /// Test boolean inequality.
        /// </summary>
        [TestMethod]
        public void TestBooleanInequality()
        {
            RandomBytes(x => CheckAgreement<bool>(b => b != (x % 2 == 0)));
        }

        /// <summary>
        /// Test char inequality.
        /// </summary>
        [TestMethod]
        public void TestCharInequality()
        {
            RandomBytes(x => CheckAgreement<char>(c => c != 'x'));
        }

        /// <summary>
        /// Test integer inequality.
        /// </summary>
        [TestMethod]
        public void TestIntegerInequality()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i != x));
            RandomBytes(x => CheckAgreement<byte>(i => x != i));
            RandomBytes(x => CheckAgreement<short>(i => i != (short)x));
            RandomBytes(x => CheckAgreement<short>(i => (short)x != i));
            RandomBytes(x => CheckAgreement<ushort>(i => i != (ushort)x));
            RandomBytes(x => CheckAgreement<ushort>(i => (ushort)x != i));
            RandomBytes(x => CheckAgreement<int>(i => i != (int)x));
            RandomBytes(x => CheckAgreement<int>(i => (int)x != i));
            RandomBytes(x => CheckAgreement<uint>(i => i != (uint)x));
            RandomBytes(x => CheckAgreement<uint>(i => (uint)x != i));
            RandomBytes(x => CheckAgreement<long>(i => i != (long)x));
            RandomBytes(x => CheckAgreement<long>(i => (long)x != i));
            RandomBytes(x => CheckAgreement<ulong>(i => i != (ulong)x));
            RandomBytes(x => CheckAgreement<ulong>(i => (ulong)x != i));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i != new BigInteger(x)));
            RandomBytes(x => CheckAgreement<BigInteger>(i => new BigInteger(x) != i));
        }

        /// <summary>
        /// Test integer bitwise or with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseOrConstant()
        {
            CheckAgreement<byte, byte>((a, b) => ((byte)1 | (byte)2) == ((byte)2 | (byte)1));
            CheckAgreement<short, short>((a, b) => ((short)1 | (short)2) == ((short)2 | (short)1));
            CheckAgreement<ushort, ushort>((a, b) => ((ushort)1 | (ushort)2) == ((ushort)2 | (ushort)1));
            CheckAgreement<int, int>((a, b) => ((int)1 | (int)2) == ((int)2 | (int)1));
            CheckAgreement<uint, uint>((a, b) => (1U | 2U) == (2U | 1U));
            CheckAgreement<long, long>((a, b) => (1L | 2L) == (2L | 1L));
            CheckAgreement<ulong, ulong>((a, b) => (1UL | 2UL) == (2UL | 1UL));
        }

        /// <summary>
        /// Test integer bitwise or.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseOr()
        {
            RandomBytes(x => CheckAgreement<byte, byte>((a, b) => BitwiseOr(a, b) == x));
            RandomBytes(x => CheckAgreement<byte, byte>((a, b) => BitwiseOr(a, b, (byte)0) == x));
            RandomBytes(x => CheckAgreement<short, short>((a, b) => BitwiseOr(a, b) == x));
            RandomBytes(x => CheckAgreement<ushort, ushort>((a, b) => BitwiseOr(a, b) == x));
            RandomBytes(x => CheckAgreement<int, int>((a, b) => BitwiseOr(a, b) == x));
            RandomBytes(x => CheckAgreement<uint, uint>((a, b) => BitwiseOr(a, b) == x));
            RandomBytes(x => CheckAgreement<long, long>((a, b) => BitwiseOr(a, b) == x));
            RandomBytes(x => CheckAgreement<ulong, ulong>((a, b) => BitwiseOr(a, b) == x));
        }

        /// <summary>
        /// Test bitwise and with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseAndConstantByte()
        {
            CheckAgreement<byte, byte>((a, b) => ((byte)1 & (byte)2) == ((byte)2 & (byte)1));
            CheckAgreement<short, short>((a, b) => ((short)1 & (short)2) == ((short)2 & (short)1));
            CheckAgreement<ushort, ushort>((a, b) => ((ushort)1 & (ushort)2) == ((ushort)2 & (ushort)1));
            CheckAgreement<int, int>((a, b) => (1 & (int)2) == ((int)2 & 1));
            CheckAgreement<uint, uint>((a, b) => (1U & 2U) == (2U & 1U));
            CheckAgreement<long, long>((a, b) => (1L & 2L) == (2L & 1L));
            CheckAgreement<ulong, ulong>((a, b) => (1UL & 2UL) == (2UL & 1UL));
        }

        /// <summary>
        /// Test integer bitwise and.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseAnd()
        {
            RandomBytes(x => CheckAgreement<byte, byte>((a, b) => BitwiseAnd(a, b) == x));
            RandomBytes(x => CheckAgreement<byte, byte>((a, b) => BitwiseAnd(a, b, (byte)255) == x));
            RandomBytes(x => CheckAgreement<short, short>((a, b) => BitwiseAnd(a, b) == x));
            RandomBytes(x => CheckAgreement<ushort, ushort>((a, b) => BitwiseAnd(a, b) == x));
            RandomBytes(x => CheckAgreement<int, int>((a, b) => BitwiseAnd(a, b) == x));
            RandomBytes(x => CheckAgreement<uint, uint>((a, b) => BitwiseAnd(a, b) == x));
            RandomBytes(x => CheckAgreement<long, long>((a, b) => BitwiseAnd(a, b) == x));
            RandomBytes(x => CheckAgreement<ulong, ulong>((a, b) => BitwiseAnd(a, b) == x));
        }

        /// <summary>
        /// Test bitwise xor with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseXorConstant()
        {
            CheckAgreement<byte, byte>((a, b) => ((byte)1 ^ (byte)2) == ((byte)2 ^ (byte)1));
            CheckAgreement<short, short>((a, b) => ((short)1 ^ (short)2) == ((short)2 ^ (short)1));
            CheckAgreement<ushort, ushort>((a, b) => ((ushort)1 ^ (ushort)2) == ((ushort)2 ^ (ushort)1));
            CheckAgreement<int, int>((a, b) => (1 ^ (int)2) == ((int)2 ^ 1));
            CheckAgreement<uint, uint>((a, b) => (1U ^ 2U) == (2U ^ 1U));
            CheckAgreement<long, long>((a, b) => (1L ^ 2L) == (2L ^ 1L));
            CheckAgreement<ulong, ulong>((a, b) => (1UL ^ 2UL) == (2UL ^ 1UL));
        }

        /// <summary>
        /// Test bitwise xor.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseXor()
        {
            RandomBytes(x => CheckAgreement<byte, byte>((a, b) => BitwiseXor(a, b) == x));
            RandomBytes(x => CheckAgreement<byte, byte>((a, b) => BitwiseXor(a, b, (byte)0) == x));
            RandomBytes(x => CheckAgreement<short, short>((a, b) => BitwiseXor(a, b) == x));
            RandomBytes(x => CheckAgreement<ushort, ushort>((a, b) => BitwiseXor(a, b) == x));
            RandomBytes(x => CheckAgreement<int, int>((a, b) => BitwiseXor(a, b) == x));
            RandomBytes(x => CheckAgreement<uint, uint>((a, b) => BitwiseXor(a, b) == x));
            RandomBytes(x => CheckAgreement<long, long>((a, b) => BitwiseXor(a, b) == x));
            RandomBytes(x => CheckAgreement<ulong, ulong>((a, b) => BitwiseXor(a, b) == x));
        }

        /// <summary>
        /// Test bitwise not.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseNot()
        {
            RandomBytes(x => CheckAgreement<byte>(a => BitwiseNot(a) == x));
            RandomBytes(x => CheckAgreement<short>(a => BitwiseNot(a) == x));
            RandomBytes(x => CheckAgreement<ushort>(a => BitwiseNot(a) == x));
            RandomBytes(x => CheckAgreement<int>(a => BitwiseNot(a) == x));
            RandomBytes(x => CheckAgreement<uint>(a => BitwiseNot(a) == x));
            RandomBytes(x => CheckAgreement<long>(a => BitwiseNot(a) == x));
            RandomBytes(x => CheckAgreement<ulong>(a => BitwiseNot(a) == x));
        }

        /// <summary>
        /// Test bitwise not.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseNotNot()
        {
            CheckValid<byte>(a => ~~a == a);
            CheckValid<short>(a => ~~a == a);
            CheckValid<ushort>(a => ~~a == a);
            CheckValid<int>(a => ~~a == a);
            CheckValid<uint>(a => ~~a == a);
            CheckValid<long>(a => ~~a == a);
            CheckValid<ulong>(a => ~~a == a);
        }

        /// <summary>
        /// Test integer addition with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerSumConstant()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i + Constant<byte>(4) == (x + Constant<byte>(4))));
            RandomBytes(x => CheckAgreement<byte>(i => Constant<byte>(4) + i == (x + Constant<byte>(4))));
            RandomBytes(x => CheckAgreement<short>(i => i + Constant<short>(4) == (x + Constant<short>(4))));
            RandomBytes(x => CheckAgreement<short>(i => Constant<short>(4) + i == (x + Constant<short>(4))));
            RandomBytes(x => CheckAgreement<ushort>(i => i + Constant<ushort>(4) == (x + Constant<ushort>(4))));
            RandomBytes(x => CheckAgreement<ushort>(i => Constant<ushort>(4) + i == (x + Constant<ushort>(4))));
            RandomBytes(x => CheckAgreement<int>(i => i + Constant<int>(4) == (x + Constant<int>(4))));
            RandomBytes(x => CheckAgreement<int>(i => Constant<int>(4) + i == (x + Constant<int>(4))));
            RandomBytes(x => CheckAgreement<uint>(i => i + Constant<uint>(4) == (x + Constant<uint>(4))));
            RandomBytes(x => CheckAgreement<uint>(i => Constant<uint>(4) + i == (x + Constant<uint>(4))));
            RandomBytes(x => CheckAgreement<long>(i => i + Constant<long>(4) == (x + Constant<long>(4))));
            RandomBytes(x => CheckAgreement<long>(i => Constant<long>(4) + i == (x + Constant<long>(4))));
            RandomBytes(x => CheckAgreement<ulong>(i => i + Constant<ulong>(4) == (x + Constant<ulong>(4))));
            RandomBytes(x => CheckAgreement<ulong>(i => Constant<ulong>(4) + i == (x + Constant<ulong>(4))));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i + Constant<BigInteger>(4) == (new BigInteger(x) + Constant<BigInteger>(4))));
            RandomBytes(x => CheckAgreement<BigInteger>(i => Constant<BigInteger>(4) + i == (new BigInteger(x) + Constant<BigInteger>(4))));
        }

        /// <summary>
        /// Test integer sum with a variable.
        /// </summary>
        [TestMethod]
        public void TestIntegerSumVariable()
        {
            RandomBytes(x =>
            {
                if (x % 2 == 0)
                {
                    CheckAgreement<uint>(i => i + i == x);
                }
            });
        }

        /// <summary>
        /// Test integer minus with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerMinusConstant()
        {
            RandomBytes(x => CheckAgreement<byte>(i => i - (byte)4 == x));
            RandomBytes(x => CheckAgreement<byte>(i => (byte)4 - i == x));
            RandomBytes(x => CheckAgreement<short>(i => i - (short)4 == x));
            RandomBytes(x => CheckAgreement<short>(i => (short)4 - i == x));
            RandomBytes(x => CheckAgreement<ushort>(i => i - (ushort)4 == x));
            RandomBytes(x => CheckAgreement<ushort>(i => (ushort)4 - i == x));
            RandomBytes(x => CheckAgreement<int>(i => i - (int)4 == x));
            RandomBytes(x => CheckAgreement<int>(i => (int)4 - i == x));
            RandomBytes(x => CheckAgreement<uint>(i => i - 4U == x));
            RandomBytes(x => CheckAgreement<uint>(i => 4U - i == x));
            RandomBytes(x => CheckAgreement<long>(i => i - 4L == x));
            RandomBytes(x => CheckAgreement<long>(i => 4L - i == x));
            RandomBytes(x => CheckAgreement<ulong>(i => i - 4UL == x));
            RandomBytes(x => CheckAgreement<ulong>(i => 4UL - i == x));
            RandomBytes(x => CheckAgreement<BigInteger>(i => i - new BigInteger(4) == new BigInteger(x)));
            RandomBytes(x => CheckAgreement<BigInteger>(i => new BigInteger(4) - i == new BigInteger(x)));
        }

        /// <summary>
        /// Test multiplication of integers.
        /// </summary>
        [TestMethod]
        public void TestMultiplication()
        {
            var f = new ZenFunction<int, int, bool>((a, b) => a * b == 10);
            var inputs = f.Find((a, b, res) => res, backend: ModelChecking.Backend.Z3);
            Assert.IsTrue(inputs.HasValue);
            Assert.AreEqual(10, inputs.Value.Item1 * inputs.Value.Item2);

            Assert.IsTrue(f.Evaluate(5, 2));
            Assert.IsTrue(f.Evaluate(1, 10));
            Assert.IsFalse(f.Evaluate(4, 3));

            f.Compile();

            Assert.IsTrue(f.Evaluate(5, 2));
            Assert.IsTrue(f.Evaluate(1, 10));
            Assert.IsFalse(f.Evaluate(4, 3));
        }

        /// <summary>
        /// Test multiplication of big integers works.
        /// </summary>
        [TestMethod]
        public void TestMultiplySolve()
        {
            var f = new ZenFunction<BigInteger, BigInteger, bool>((x, y) => x * y == new BigInteger(4));
            var inputs = f.Find((x, y, result) => result);
            Assert.AreEqual(4, inputs.Value.Item1 * inputs.Value.Item2);
        }

        /// <summary>
        /// Test that equality completes in reasonable time.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMultiplicationException()
        {
            var f = new ZenFunction<int, int, bool>((a, b) => a * b == 10);
            var inputs = f.Find((a, b, res) => res, backend: Backend.DecisionDiagrams);
        }

        /// <summary>
        /// Test that equality completes in reasonable time.
        /// </summary>
        [TestMethod]
        public void TestEqualityDoesNotBlowup()
        {
            RandomBytes(x => CheckAgreement<int, int>((a, b) => And(a == b, b > 4)));
        }
    }
}