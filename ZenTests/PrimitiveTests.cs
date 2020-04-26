// <copyright file="PrimitiveTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.ZenTests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Research.Zen;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Microsoft.Research.Zen.Language;
    using static Microsoft.Research.ZenTests.TestHelper;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PrimitiveTests
    {
        /// <summary>
        /// Test integer greater than.
        /// </summary>
        [TestMethod]
        public void TestIntegerGreaterThan()
        {
            Repeat(x => CheckAgreement<byte>(i => i > x));
            Repeat(x => CheckAgreement<byte>(i => x > i));
            Repeat(x => CheckAgreement<short>(i => i > (short)x));
            Repeat(x => CheckAgreement<short>(i => (short)x > i));
            Repeat(x => CheckAgreement<ushort>(i => i > (ushort)x));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)x > i));
            Repeat(x => CheckAgreement<int>(i => i > (int)x));
            Repeat(x => CheckAgreement<int>(i => (int)x > i));
            Repeat(x => CheckAgreement<uint>(i => i > (uint)x));
            Repeat(x => CheckAgreement<uint>(i => (uint)x > i));
            Repeat(x => CheckAgreement<long>(i => i > (long)x));
            Repeat(x => CheckAgreement<long>(i => (long)x > i));
            Repeat(x => CheckAgreement<ulong>(i => i > (ulong)x));
            Repeat(x => CheckAgreement<ulong>(i => (ulong)x > i));
        }

        /// <summary>
        /// Test integer less than.
        /// </summary>
        [TestMethod]
        public void TestIntegerLessThan()
        {
            Repeat(x => CheckAgreement<byte>(i => i < x));
            Repeat(x => CheckAgreement<byte>(i => x < i));
            Repeat(x => CheckAgreement<short>(i => i < (short)x));
            Repeat(x => CheckAgreement<short>(i => (short)x < i));
            Repeat(x => CheckAgreement<ushort>(i => i < (ushort)x));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)x < i));
            Repeat(x => CheckAgreement<int>(i => i < (int)x));
            Repeat(x => CheckAgreement<int>(i => (int)x < i));
            Repeat(x => CheckAgreement<uint>(i => i < (uint)x));
            Repeat(x => CheckAgreement<uint>(i => (uint)x < i));
            Repeat(x => CheckAgreement<long>(i => i < (long)x));
            Repeat(x => CheckAgreement<long>(i => (long)x < i));
            Repeat(x => CheckAgreement<ulong>(i => i < (ulong)x));
            Repeat(x => CheckAgreement<ulong>(i => (ulong)x < i));
        }

        /// <summary>
        /// Test integer less than or equal.
        /// </summary>
        [TestMethod]
        public void TestIntegerLessThanOrEqual()
        {
            Repeat(x => CheckAgreement<byte>(i => i <= x));
            Repeat(x => CheckAgreement<byte>(i => x <= i));
            Repeat(x => CheckAgreement<short>(i => i <= (short)x));
            Repeat(x => CheckAgreement<short>(i => (short)x <= i));
            Repeat(x => CheckAgreement<ushort>(i => i <= (ushort)x));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)x <= i));
            Repeat(x => CheckAgreement<int>(i => i <= (int)x));
            Repeat(x => CheckAgreement<int>(i => (int)x <= i));
            Repeat(x => CheckAgreement<uint>(i => i <= (uint)x));
            Repeat(x => CheckAgreement<uint>(i => (uint)x <= i));
            Repeat(x => CheckAgreement<long>(i => i <= (long)x));
            Repeat(x => CheckAgreement<long>(i => (long)x <= i));
            Repeat(x => CheckAgreement<ulong>(i => i <= (ulong)x));
            Repeat(x => CheckAgreement<ulong>(i => (ulong)x <= i));
        }

        /// <summary>
        /// Test integer greater than or equal.
        /// </summary>
        [TestMethod]
        public void TestIntegerGreaterThanOrEqualByte()
        {
            Repeat(x => CheckAgreement<byte>(i => i >= x));
            Repeat(x => CheckAgreement<byte>(i => x >= i));
            Repeat(x => CheckAgreement<short>(i => i >= (short)x));
            Repeat(x => CheckAgreement<short>(i => (short)x >= i));
            Repeat(x => CheckAgreement<ushort>(i => i >= (ushort)x));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)x >= i));
            Repeat(x => CheckAgreement<int>(i => i >= (int)x));
            Repeat(x => CheckAgreement<int>(i => (int)x >= i));
            Repeat(x => CheckAgreement<uint>(i => i >= (uint)x));
            Repeat(x => CheckAgreement<uint>(i => (uint)x >= i));
            Repeat(x => CheckAgreement<long>(i => i >= (long)x));
            Repeat(x => CheckAgreement<long>(i => (long)x >= i));
            Repeat(x => CheckAgreement<ulong>(i => i >= (ulong)x));
            Repeat(x => CheckAgreement<ulong>(i => (ulong)x >= i));
        }

        /// <summary>
        /// Test boolean equality.
        /// </summary>
        [TestMethod]
        public void TestBooleanEquality()
        {
            Repeat(x => CheckAgreement<bool>(b => b == (x % 2 == 0)));
        }

        /// <summary>
        /// Test integer equality.
        /// </summary>
        [TestMethod]
        public void TestIntegerEquality()
        {
            Repeat(x => CheckAgreement<byte>(i => i == x));
            Repeat(x => CheckAgreement<byte>(i => x == i));
            Repeat(x => CheckAgreement<short>(i => i == (short)x));
            Repeat(x => CheckAgreement<short>(i => (short)x == i));
            Repeat(x => CheckAgreement<ushort>(i => i == (ushort)x));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)x == i));
            Repeat(x => CheckAgreement<int>(i => i == (int)x));
            Repeat(x => CheckAgreement<int>(i => (int)x == i));
            Repeat(x => CheckAgreement<uint>(i => i == (uint)x));
            Repeat(x => CheckAgreement<uint>(i => (uint)x == i));
            Repeat(x => CheckAgreement<long>(i => i == (long)x));
            Repeat(x => CheckAgreement<long>(i => (long)x == i));
            Repeat(x => CheckAgreement<ulong>(i => i == (ulong)x));
            Repeat(x => CheckAgreement<ulong>(i => (ulong)x == i));
        }

        /// <summary>
        /// Test boolean inequality.
        /// </summary>
        [TestMethod]
        public void TestBooleanInequality()
        {
            Repeat(x => CheckAgreement<bool>(b => b != (x % 2 == 0)));
        }

        /// <summary>
        /// Test integer inequality.
        /// </summary>
        [TestMethod]
        public void TestIntegerInequality()
        {
            Repeat(x => CheckAgreement<byte>(i => i != x));
            Repeat(x => CheckAgreement<byte>(i => x != i));
            Repeat(x => CheckAgreement<short>(i => i != (short)x));
            Repeat(x => CheckAgreement<short>(i => (short)x != i));
            Repeat(x => CheckAgreement<ushort>(i => i != (ushort)x));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)x != i));
            Repeat(x => CheckAgreement<int>(i => i != (int)x));
            Repeat(x => CheckAgreement<int>(i => (int)x != i));
            Repeat(x => CheckAgreement<uint>(i => i != (uint)x));
            Repeat(x => CheckAgreement<uint>(i => (uint)x != i));
            Repeat(x => CheckAgreement<long>(i => i != (long)x));
            Repeat(x => CheckAgreement<long>(i => (long)x != i));
            Repeat(x => CheckAgreement<ulong>(i => i != (ulong)x));
            Repeat(x => CheckAgreement<ulong>(i => (ulong)x != i));
        }

        /// <summary>
        /// Test integer bitwise or with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseOrConstant()
        {
            CheckAgreement<byte, byte>((a, b) => (Byte(1) | (byte)2) == ((byte)2 | Byte(1)));
            CheckAgreement<short, short>((a, b) => (Short(1) | (short)2) == ((short)2 | Short(1)));
            CheckAgreement<ushort, ushort>((a, b) => (UShort(1) | (ushort)2) == ((ushort)2 | UShort(1)));
            CheckAgreement<int, int>((a, b) => (Int(1) | (int)2) == ((int)2 | Int(1)));
            CheckAgreement<uint, uint>((a, b) => (UInt(1) | 2U) == (2U | UInt(1)));
            CheckAgreement<long, long>((a, b) => (Long(1) | 2L) == (2L | Long(1)));
            CheckAgreement<ulong, ulong>((a, b) => (ULong(1) | 2UL) == (2UL | ULong(1)));
        }

        /// <summary>
        /// Test integer bitwise or.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseOr()
        {
            Repeat(x => CheckAgreement<byte, byte>((a, b) => BitwiseOr(a, b) == x));
            Repeat(x => CheckAgreement<byte, byte>((a, b) => BitwiseOr(a, b, Byte(0)) == x));
            Repeat(x => CheckAgreement<short, short>((a, b) => BitwiseOr(a, b) == x));
            Repeat(x => CheckAgreement<ushort, ushort>((a, b) => BitwiseOr(a, b) == x));
            Repeat(x => CheckAgreement<int, int>((a, b) => BitwiseOr(a, b) == x));
            Repeat(x => CheckAgreement<uint, uint>((a, b) => BitwiseOr(a, b) == x));
            Repeat(x => CheckAgreement<long, long>((a, b) => BitwiseOr(a, b) == x));
            Repeat(x => CheckAgreement<ulong, ulong>((a, b) => BitwiseOr(a, b) == x));
        }

        /// <summary>
        /// Test bitwise and with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseAndConstantByte()
        {
            CheckAgreement<byte, byte>((a, b) => (Byte(1) & (byte)2) == ((byte)2 & Byte(1)));
            CheckAgreement<short, short>((a, b) => (Short(1) & (short)2) == ((short)2 & Short(1)));
            CheckAgreement<ushort, ushort>((a, b) => (UShort(1) & (ushort)2) == ((ushort)2 & UShort(1)));
            CheckAgreement<int, int>((a, b) => (Int(1) & (int)2) == ((int)2 & Int(1)));
            CheckAgreement<uint, uint>((a, b) => (UInt(1) & 2U) == (2U & UInt(1)));
            CheckAgreement<long, long>((a, b) => (Long(1) & 2L) == (2L & Long(1)));
            CheckAgreement<ulong, ulong>((a, b) => (ULong(1) & 2UL) == (2UL & ULong(1)));
        }

        /// <summary>
        /// Test integer bitwise and.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseAnd()
        {
            Repeat(x => CheckAgreement<byte, byte>((a, b) => BitwiseAnd(a, b) == x));
            Repeat(x => CheckAgreement<byte, byte>((a, b) => BitwiseAnd(a, b, Byte(255)) == x));
            Repeat(x => CheckAgreement<short, short>((a, b) => BitwiseAnd(a, b) == x));
            Repeat(x => CheckAgreement<ushort, ushort>((a, b) => BitwiseAnd(a, b) == x));
            Repeat(x => CheckAgreement<int, int>((a, b) => BitwiseAnd(a, b) == x));
            Repeat(x => CheckAgreement<uint, uint>((a, b) => BitwiseAnd(a, b) == x));
            Repeat(x => CheckAgreement<long, long>((a, b) => BitwiseAnd(a, b) == x));
            Repeat(x => CheckAgreement<ulong, ulong>((a, b) => BitwiseAnd(a, b) == x));
        }

        /// <summary>
        /// Test bitwise xor with constants.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseXorConstant()
        {
            CheckAgreement<byte, byte>((a, b) => (Byte(1) ^ (byte)2) == ((byte)2 ^ Byte(1)));
            CheckAgreement<short, short>((a, b) => (Short(1) ^ (short)2) == ((short)2 ^ Short(1)));
            CheckAgreement<ushort, ushort>((a, b) => (UShort(1) ^ (ushort)2) == ((ushort)2 ^ UShort(1)));
            CheckAgreement<int, int>((a, b) => (Int(1) ^ (int)2) == ((int)2 ^ Int(1)));
            CheckAgreement<uint, uint>((a, b) => (UInt(1) ^ 2U) == (2U ^ UInt(1)));
            CheckAgreement<long, long>((a, b) => (Long(1) ^ 2L) == (2L ^ Long(1)));
            CheckAgreement<ulong, ulong>((a, b) => (ULong(1) ^ 2UL) == (2UL ^ ULong(1)));
        }

        /// <summary>
        /// Test bitwise xor.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseXor()
        {
            Repeat(x => CheckAgreement<byte, byte>((a, b) => BitwiseXor(a, b) == x));
            Repeat(x => CheckAgreement<byte, byte>((a, b) => BitwiseXor(a, b, Byte(0)) == x));
            Repeat(x => CheckAgreement<short, short>((a, b) => BitwiseXor(a, b) == x));
            Repeat(x => CheckAgreement<ushort, ushort>((a, b) => BitwiseXor(a, b) == x));
            Repeat(x => CheckAgreement<int, int>((a, b) => BitwiseXor(a, b) == x));
            Repeat(x => CheckAgreement<uint, uint>((a, b) => BitwiseXor(a, b) == x));
            Repeat(x => CheckAgreement<long, long>((a, b) => BitwiseXor(a, b) == x));
            Repeat(x => CheckAgreement<ulong, ulong>((a, b) => BitwiseXor(a, b) == x));
        }

        /// <summary>
        /// Test bitwise not.
        /// </summary>
        [TestMethod]
        public void TestIntegerBitwiseNot()
        {
            Repeat(x => CheckAgreement<byte>(a => BitwiseNot(a) == x));
            /* Repeat(x => CheckAgreement<short>(a => BitwiseNot(a) == x));
            Repeat(x => CheckAgreement<ushort>(a => BitwiseNot(a) == x));
            Repeat(x => CheckAgreement<int>(a => BitwiseNot(a) == x));
            Repeat(x => CheckAgreement<uint>(a => BitwiseNot(a) == x));
            Repeat(x => CheckAgreement<long>(a => BitwiseNot(a) == x));
            Repeat(x => CheckAgreement<ulong>(a => BitwiseNot(a) == x)); */
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
            Repeat(x => CheckAgreement<byte>(i => i + (byte)4 == (x + 4)));
            Repeat(x => CheckAgreement<byte>(i => (byte)4 + i == (x + 4)));
            Repeat(x => CheckAgreement<short>(i => i + (short)4 == (x + 4)));
            Repeat(x => CheckAgreement<short>(i => (short)4 + i == (x + 4)));
            Repeat(x => CheckAgreement<ushort>(i => i + (ushort)4 == (x + 4)));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)4 + i == (x + 4)));
            Repeat(x => CheckAgreement<int>(i => i + (int)4 == (x + 4)));
            Repeat(x => CheckAgreement<int>(i => (int)4 + i == (x + 4)));
            Repeat(x => CheckAgreement<uint>(i => i + 4U == (x + 4)));
            Repeat(x => CheckAgreement<uint>(i => 4U + i == (x + 4)));
            Repeat(x => CheckAgreement<long>(i => i + 4L == (x + 4)));
            Repeat(x => CheckAgreement<long>(i => 4L + i == (x + 4)));
            Repeat(x => CheckAgreement<ulong>(i => i + 4UL == (x + 4)));
            Repeat(x => CheckAgreement<ulong>(i => 4UL + i == (x + 4)));
        }

        /// <summary>
        /// Test integer sum with a variable.
        /// </summary>
        [TestMethod]
        public void TestIntegerSumVariable()
        {
            Repeat(x =>
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
            Repeat(x => CheckAgreement<byte>(i => i - (byte)4 == x));
            Repeat(x => CheckAgreement<byte>(i => (byte)4 - i == x));
            Repeat(x => CheckAgreement<short>(i => i - (short)4 == x));
            Repeat(x => CheckAgreement<short>(i => (short)4 - i == x));
            Repeat(x => CheckAgreement<ushort>(i => i - (ushort)4 == x));
            Repeat(x => CheckAgreement<ushort>(i => (ushort)4 - i == x));
            Repeat(x => CheckAgreement<int>(i => i - (int)4 == x));
            Repeat(x => CheckAgreement<int>(i => (int)4 - i == x));
            Repeat(x => CheckAgreement<uint>(i => i - 4U == x));
            Repeat(x => CheckAgreement<uint>(i => 4U - i == x));
            Repeat(x => CheckAgreement<long>(i => i - 4L == x));
            Repeat(x => CheckAgreement<long>(i => 4L - i == x));
            Repeat(x => CheckAgreement<ulong>(i => i - 4UL == x));
            Repeat(x => CheckAgreement<ulong>(i => 4UL - i == x));
        }

        /// <summary>
        /// Test integer minus with constants.
        /// </summary>
        [TestMethod]
        public void TestDefaultValues()
        {
            Assert.IsTrue(Function(() => Null<bool>()).Assert(v => v.Value() == false));
            Assert.IsTrue(Function(() => Null<byte>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(Function(() => Null<short>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(Function(() => Null<ushort>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(Function(() => Null<int>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(Function(() => Null<uint>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(Function(() => Null<long>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(Function(() => Null<ulong>()).Assert(v => v.Value() == 0));
            Assert.IsTrue(Function(() => Null<IList<bool>>()).Assert(v => v.Value().IsEmpty()));
            Assert.IsTrue(Function(() => Null<IDictionary<bool, bool>>()).Assert(v => v.Value().Get(true).HasValue() == false));
        }

        /// <summary>
        /// Test that equality completes in reasonable time.
        /// </summary>
        [TestMethod]
        public void TestMultiplication()
        {
            var f = Function<int, int, bool>((a, b) => a * b == 10);
            var inputs = f.Find((a, b, res) => res, backend: Zen.ModelChecking.Backend.Z3);
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
        /// Test that equality completes in reasonable time.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMultiplicationException()
        {
            var f = Function<int, int, bool>((a, b) => a * b == 10);
            var inputs = f.Find((a, b, res) => res, backend: Zen.ModelChecking.Backend.DecisionDiagrams);
        }

        /// <summary>
        /// Test that equality completes in reasonable time.
        /// </summary>
        [TestMethod]
        public void TestEqualityDoesNotBlowup()
        {
            Repeat(x => CheckAgreement<int, int>((a, b) => And(a == b, b > Int(4))));
        }
    }
}