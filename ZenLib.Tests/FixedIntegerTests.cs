// <copyright file="FixedIntegerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.Solver;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests fixed integer operations.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FixedIntegerTests
    {
        /// <summary>
        /// How many random tests to run.
        /// </summary>
        private static int numRandomTests = 30;

        /// <summary>
        /// Test that less than or equal works as expected.
        /// </summary>
        [TestMethod]
        [DataRow(3, 4, true)]
        [DataRow(4, 3, false)]
        [DataRow(0, 1, true)]
        [DataRow(-1, -1, false)]
        [DataRow(1, -1, false)]
        [DataRow(-1, 1, true)]
        [DataRow(-1, 0, true)]
        [DataRow(-30, -20, true)]
        [DataRow(-20, -30, false)]
        [DataRow(120, 129, true)]
        public void TestLessThan(long x, long y, bool expected)
        {
            Assert.AreEqual(expected, new Int9(x) < new Int9(y));
        }

        /// <summary>
        /// Test that less than or equal works as expected.
        /// </summary>
        [TestMethod]
        [DataRow(3, 4, true)]
        [DataRow(4, 3, false)]
        [DataRow(0, 1, true)]
        [DataRow(-1, -1, true)]
        [DataRow(1, -1, false)]
        [DataRow(-1, 1, true)]
        [DataRow(-1, 0, true)]
        [DataRow(-30, -20, true)]
        [DataRow(-20, -30, false)]
        [DataRow(120, 129, true)]
        public void TestLessThanOrEqual(long x, long y, bool expected)
        {
            Assert.AreEqual(expected, new Int9(x) <= new Int9(y));
        }

        /// <summary>
        /// Test that greater than or equal works as expected.
        /// </summary>
        [TestMethod]
        [DataRow(3, 4, false)]
        [DataRow(4, 3, true)]
        [DataRow(0, 1, false)]
        [DataRow(-1, -1, true)]
        [DataRow(-1, 1, false)]
        [DataRow(-1, 0, false)]
        [DataRow(-30, -20, false)]
        [DataRow(-20, -30, true)]
        [DataRow(-20, 10, false)]
        [DataRow(120, 129, false)]
        public void TestGreaterThanOrEqual(long x, long y, bool expected)
        {
            Assert.AreEqual(expected, new Int9(x) >= new Int9(y));
        }

        /// <summary>
        /// Test equals works as expected.
        /// </summary>
        [TestMethod]
        [DataRow(0, 1, false)]
        [DataRow(-1, 1, false)]
        [DataRow(1, 1, true)]
        [DataRow(255, 255, true)]
        [DataRow(255, 256, false)]
        [DataRow(255, 257, false)]
        [DataRow(-10, -9, false)]
        [DataRow(-10, -10, true)]
        public void TestEquality(long x, long y, bool expected)
        {
            Assert.AreEqual(!expected, new Int27(x) != new Int27(y));
            Assert.AreEqual(expected, new Int27(x).GetHashCode() == new Int27(y).GetHashCode());
            Assert.AreEqual(expected, new UInt27(x).GetHashCode() == new UInt27(y).GetHashCode());
        }

        /// <summary>
        /// Test that less than or equal works as expected.
        /// </summary>
        [TestMethod]
        public void TestEqualityFails()
        {
            Assert.IsFalse(new Int1(0).Equals(null));
            Assert.IsFalse(new Int1(0).Equals(new Int2(0)));
        }

        /// <summary>
        /// Test that bitwise and works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseAnd()
        {
            Assert.AreEqual(new UInt3(4).BitwiseAnd(new UInt3(3)), new UInt3(0));
            Assert.AreEqual(new UInt3(1).BitwiseAnd(new UInt3(3)), new UInt3(1));
            Assert.AreEqual(new UInt3(2).BitwiseAnd(new UInt3(3)), new UInt3(2));
            Assert.AreEqual(new UInt10(257).BitwiseAnd(new UInt10(257)), new UInt10(257));
        }

        /// <summary>
        /// Test that bitwise and works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseAndRandom()
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var b1 = TestHelper.RandomByte();
                var b2 = TestHelper.RandomByte();

                Assert.AreEqual(b1 & b2, (byte)new UInt8(b1).BitwiseAnd(new UInt8(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise or works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseOr()
        {
            Assert.AreEqual(new UInt3(4).BitwiseOr(new UInt3(3)), new UInt3(7));
            Assert.AreEqual(new UInt3(1).BitwiseOr(new UInt3(3)), new UInt3(3));
            Assert.AreEqual(new UInt3(2).BitwiseOr(new UInt3(3)), new UInt3(3));
            Assert.AreEqual(new UInt10(257).BitwiseOr(new UInt10(257)), new UInt10(257));
        }

        /// <summary>
        /// Test that bitwise or works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseOrRandom()
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var b1 = TestHelper.RandomByte();
                var b2 = TestHelper.RandomByte();

                Assert.AreEqual(b1 | b2, (byte)new UInt8(b1).BitwiseOr(new UInt8(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise xor works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseXor()
        {
            Assert.AreEqual(new UInt3(4).BitwiseXor(new UInt3(3)), new UInt3(7));
            Assert.AreEqual(new UInt3(1).BitwiseXor(new UInt3(3)), new UInt3(2));
            Assert.AreEqual(new UInt3(2).BitwiseXor(new UInt3(3)), new UInt3(1));
            Assert.AreEqual(new Int10(257).BitwiseXor(new Int10(257)), new Int10(0));
        }

        /// <summary>
        /// Test that bitwise xor works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseXorRandom()
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var b1 = TestHelper.RandomByte();
                var b2 = TestHelper.RandomByte();

                Assert.AreEqual(b1 ^ b2, (byte)new UInt8(b1).BitwiseXor(new UInt8(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise not works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseNot()
        {
            Assert.AreEqual(new UInt3(4).BitwiseNot(), new UInt3(3));
            Assert.AreEqual(new UInt3(1).BitwiseNot(), new UInt3(6));
        }

        /// <summary>
        /// Test that bitwise not works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseNotRandom()
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var b = TestHelper.RandomByte();
                Assert.AreEqual((byte)~b, (byte)new UInt8(b).BitwiseNot().ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise not works as expected.
        /// </summary>
        [TestMethod]
        public void TestNegation()
        {
            Assert.AreEqual(new Int6(-1).Negate(), new Int6(1));
            Assert.AreEqual(new Int6(-2).Negate(), new Int6(2));
            Assert.AreEqual(new Int6(5).Negate(), new Int6(-5));
        }

        /// <summary>
        /// Test that bitwise not works as expected.
        /// </summary>
        [TestMethod]
        public void TestNegateRandom()
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var b = TestHelper.RandomByte() % 127;
                Assert.AreEqual(-b, (int)new Int8(b).Negate().ToLong());
            }
        }

        /// <summary>
        /// Test that addition works as expected.
        /// </summary>
        [TestMethod]
        public void TestAddition()
        {
            Assert.AreEqual(new Int5(4).Add(new Int5(3)), new Int5(7));
            Assert.AreEqual(new Int5(1).Add(new Int5(3)), new Int5(4));
            Assert.AreEqual(new Int5(2).Add(new Int5(3)), new Int5(5));
            Assert.AreEqual(new Int5(-1).Add(new Int5(-2)), new Int5(-3));
            Assert.AreEqual(new Int5(-1).Add(new Int5(1)), new Int5(0));
            Assert.AreEqual(new UInt1(1).Add(new UInt1(1)), new UInt1(0));
            Assert.AreEqual(new UInt5(10).Add(new UInt5(31)), new UInt5(9));
        }

        /// <summary>
        /// Test that addition works as expected.
        /// </summary>
        [TestMethod]
        public void TestAdditionRandom()
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var b1 = TestHelper.RandomByte();
                var b2 = TestHelper.RandomByte();

                Assert.AreEqual((byte)(b1 + b2), (byte)new UInt8(b1).Add(new UInt8(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that addition works as expected.
        /// </summary>
        [TestMethod]
        public void TestSubtraction()
        {
            Assert.AreEqual(new UInt5(10).Subtract(new UInt5(4)), new UInt5(6));
            Assert.AreEqual(new UInt5(0).Subtract(new UInt5(1)), new UInt5(31));
            Assert.AreEqual(new Int5(4).Subtract(new Int5(3)), new Int5(1));
            Assert.AreEqual(new Int5(4).Subtract(new Int5(-1)), new Int5(5));
            Assert.AreEqual(new Int5(-1).Subtract(new Int5(-1)), new Int5(0));
        }

        /// <summary>
        /// Test that addition works as expected.
        /// </summary>
        [TestMethod]
        public void TestSubtractionRandom()
        {
            for (int i = 0; i < numRandomTests; i++)
            {
                var b1 = TestHelper.RandomByte();
                var b2 = TestHelper.RandomByte();

                Assert.AreEqual((byte)(b1 - b2), (byte)new UInt8(b1).Subtract(new UInt8(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test long conversion.
        /// </summary>
        [TestMethod]
        public void TestLongConversion()
        {
            for (int i = -258; i < 259; i++)
            {
                Assert.AreEqual(i,  (int)new Int10(i).ToLong());
            }
        }

        /// <summary>
        /// Test that less than or equal works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestToLongException()
        {
            new Int128(0).ToLong();
        }

        /// <summary>
        /// Test that a long range is invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidRange1()
        {
            new Int1(100);
        }

        /// <summary>
        /// Test that a long range is invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidRange2()
        {
            new Int1(-100);
        }

        /// <summary>
        /// Test that the integer size is correct.
        /// </summary>
        [TestMethod]
        public void TestSizeCorrect()
        {
            Assert.IsTrue(new Int1(0).Size == 1);
            Assert.IsTrue(new Int2(0).Size == 2);
            Assert.IsTrue(new Int3(0).Size == 3);
            Assert.IsTrue(new Int4(0).Size == 4);
            Assert.IsTrue(new Int5(0).Size == 5);
            Assert.IsTrue(new Int6(0).Size == 6);
            Assert.IsTrue(new Int7(0).Size == 7);
            Assert.IsTrue(new Int8(0).Size == 8);
            Assert.IsTrue(new Int9(0).Size == 9);
            Assert.IsTrue(new Int10(0).Size == 10);
            Assert.IsTrue(new Int11(0).Size == 11);
            Assert.IsTrue(new Int12(0).Size == 12);
            Assert.IsTrue(new Int13(0).Size == 13);
            Assert.IsTrue(new Int14(0).Size == 14);
            Assert.IsTrue(new Int15(0).Size == 15);
            Assert.IsTrue(new ZenLib.Int16(0).Size == 16);
            Assert.IsTrue(new Int17(0).Size == 17);
            Assert.IsTrue(new Int18(0).Size == 18);
            Assert.IsTrue(new Int19(0).Size == 19);
            Assert.IsTrue(new Int20(0).Size == 20);
            Assert.IsTrue(new Int21(0).Size == 21);
            Assert.IsTrue(new Int22(0).Size == 22);
            Assert.IsTrue(new Int23(0).Size == 23);
            Assert.IsTrue(new Int24(0).Size == 24);
            Assert.IsTrue(new Int25(0).Size == 25);
            Assert.IsTrue(new Int26(0).Size == 26);
            Assert.IsTrue(new Int27(0).Size == 27);
            Assert.IsTrue(new Int28(0).Size == 28);
            Assert.IsTrue(new Int29(0).Size == 29);
            Assert.IsTrue(new Int30(0).Size == 30);
            Assert.IsTrue(new Int31(0).Size == 31);
            Assert.IsTrue(new ZenLib.Int32(0).Size == 32);
            Assert.IsTrue(new Int33(0).Size == 33);
            Assert.IsTrue(new Int34(0).Size == 34);
            Assert.IsTrue(new Int35(0).Size == 35);
            Assert.IsTrue(new Int36(0).Size == 36);
            Assert.IsTrue(new Int37(0).Size == 37);
            Assert.IsTrue(new Int38(0).Size == 38);
            Assert.IsTrue(new Int39(0).Size == 39);
            Assert.IsTrue(new Int40(0).Size == 40);
            Assert.IsTrue(new Int41(0).Size == 41);
            Assert.IsTrue(new Int42(0).Size == 42);
            Assert.IsTrue(new Int43(0).Size == 43);
            Assert.IsTrue(new Int44(0).Size == 44);
            Assert.IsTrue(new Int45(0).Size == 45);
            Assert.IsTrue(new Int46(0).Size == 46);
            Assert.IsTrue(new Int47(0).Size == 47);
            Assert.IsTrue(new Int48(0).Size == 48);
            Assert.IsTrue(new Int49(0).Size == 49);
            Assert.IsTrue(new Int50(0).Size == 50);
            Assert.IsTrue(new Int51(0).Size == 51);
            Assert.IsTrue(new Int52(0).Size == 52);
            Assert.IsTrue(new Int53(0).Size == 53);
            Assert.IsTrue(new Int54(0).Size == 54);
            Assert.IsTrue(new Int55(0).Size == 55);
            Assert.IsTrue(new Int56(0).Size == 56);
            Assert.IsTrue(new Int57(0).Size == 57);
            Assert.IsTrue(new Int58(0).Size == 58);
            Assert.IsTrue(new Int59(0).Size == 59);
            Assert.IsTrue(new Int60(0).Size == 60);
            Assert.IsTrue(new Int61(0).Size == 61);
            Assert.IsTrue(new Int62(0).Size == 62);
            Assert.IsTrue(new Int63(0).Size == 63);
            Assert.IsTrue(new ZenLib.Int64(0).Size == 64);
            Assert.IsTrue(new Int128(0).Size == 128);
            Assert.IsTrue(new Int256(0).Size == 256);
        }

        /// <summary>
        /// Test that the tostring method works.
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("5", new UInt7(5).ToString());
            Assert.AreEqual("-5", new Int7(-5).ToString());
            Assert.AreEqual(128 + 2, "#b00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011".Length);
            Assert.AreEqual("#b00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011", new UInt128(3).ToString());
        }

        /// <summary>
        /// Test interpreting fixed integers.
        /// </summary>
        [TestMethod]
        public void TestInterpretation()
        {
            var f = new ZenFunction<Int3, Int3, Int3>((x, y) => x + y);
            Assert.AreEqual(new Int3(3), f.Evaluate(new Int3(1), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x - y);
            Assert.AreEqual(new Int3(1), f.Evaluate(new Int3(3), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x | y);
            Assert.AreEqual(new Int3(3), f.Evaluate(new Int3(3), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x & y);
            Assert.AreEqual(new Int3(2), f.Evaluate(new Int3(3), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x ^ y);
            Assert.AreEqual(new Int3(1), f.Evaluate(new Int3(3), new Int3(2)));
        }

        /// <summary>
        /// Test compiling fixed integers.
        /// </summary>
        [TestMethod]
        public void TestCompilation()
        {
            var f = new ZenFunction<Int3, Int3, Int3>((x, y) => x + y);
            f.Compile();
            Assert.AreEqual(new Int3(3), f.Evaluate(new Int3(1), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x - y);
            f.Compile();
            Assert.AreEqual(new Int3(1), f.Evaluate(new Int3(3), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x | y);
            f.Compile();
            Assert.AreEqual(new Int3(3), f.Evaluate(new Int3(3), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x & y);
            f.Compile();
            Assert.AreEqual(new Int3(2), f.Evaluate(new Int3(3), new Int3(2)));

            f = new ZenFunction<Int3, Int3, Int3>((x, y) => x ^ y);
            f.Compile();
            Assert.AreEqual(new Int3(1), f.Evaluate(new Int3(3), new Int3(2)));
        }

        /// <summary>
        /// Test solving addition.
        /// </summary>
        [TestMethod]
        public void TestSolvingEquality()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, bool>((x, y) => x == y);
                var inputs = f.FindAll((x, y, z) => z).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(input.Item1, input.Item2);
                }
            }
        }

        /// <summary>
        /// Test solving addition.
        /// </summary>
        [TestMethod]
        public void TestSolvingAddition()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, Int5>((x, y) => x + y);
                var inputs = f.FindAll((x, y, z) => z == new Int5(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int5(4), input.Item1.Add(input.Item2));
                }
            }
        }

        /// <summary>
        /// Test solving subtraction.
        /// </summary>
        [TestMethod]
        public void TestSolvingSubtraction()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, Int5>((x, y) => x - y);
                var inputs = f.FindAll((x, y, z) => z == new Int5(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int5(4), input.Item1.Subtract(input.Item2));
                }
            }
        }

        /// <summary>
        /// Test solving bitwise and.
        /// </summary>
        [TestMethod]
        public void TestSolvingBitwiseAnd()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, Int5>((x, y) => x & y);
                var inputs = f.FindAll((x, y, z) => z == new Int5(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int5(4), input.Item1.BitwiseAnd(input.Item2));
                }
            }
        }

        /// <summary>
        /// Test solving bitwise or.
        /// </summary>
        [TestMethod]
        public void TestSolvingBitwiseOr()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, Int5>((x, y) => x | y);
                var inputs = f.FindAll((x, y, z) => z == new Int5(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int5(4), input.Item1.BitwiseOr(input.Item2));
                }
            }
        }

        /// <summary>
        /// Test solving bitwise xor.
        /// </summary>
        [TestMethod]
        public void TestSolvingBitwiseXor()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, Int5>((x, y) => x ^ y);
                var inputs = f.FindAll((x, y, z) => z == new Int5(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int5(4), input.Item1.BitwiseXor(input.Item2));
                }
            }
        }

        /// <summary>
        /// Test solving less than or equal.
        /// </summary>
        [TestMethod]
        public void TestSolvingLeq()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, bool>((x, y) => x <= y);
                var inputs = f.FindAll((x, y, z) => z).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(true, input.Item1 <= input.Item2);
                }
            }
        }

        /// <summary>
        /// Test solving greater than or equal.
        /// </summary>
        [TestMethod]
        public void TestSolvingGeq()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int5, Int5, bool>((x, y) => x >= y);
                var inputs = f.FindAll((x, y, z) => z).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(true, input.Item1 >= input.Item2);
                }
            }
        }

        /// <summary>
        /// Test solving fixed integers with other constraints.
        /// </summary>
        [TestMethod]
        public void TestSolve()
        {
            var zf = new ZenConstraint<bool, int, UInt9>((b, x, y) =>
            {
                var c1 = If(b, y < new UInt9(10), x == 3);
                var c2 = Implies(b, (y & new UInt9(1)) == new UInt9(1));
                return And(c1, c2, x == 5);
            });

            Assert.IsTrue((zf.Find().Value.Item3.ToLong() & 1L) == 1L);
        }

        /// <summary>
        /// Test solving with option.
        /// </summary>
        [TestMethod]
        public void TestConstructingOptions()
        {
            var f = new ZenFunction<Option<Int5>, Option<Int5>>(x => Option.Null<Int5>());
            var input = f.Find((x, y) => true);
            Assert.IsTrue(input.HasValue);
        }

        /// <summary>
        /// Test solving with option.
        /// </summary>
        [TestMethod]
        public void Test128BitInteger()
        {
            var a = new UInt128(IPAddress.Parse("1000::").GetAddressBytes());
            var b = new UInt128(IPAddress.Parse("2000::").GetAddressBytes());

            var f = new ZenFunction<UInt128, bool>(x => And(x >= a, x <= b));
            var input = f.Find((x, y) => y, backend: SolverType.DecisionDiagrams);

            Assert.AreEqual(a, input.Value);
        }

        /// <summary>
        /// Test integer in if condition.
        /// </summary>
        [TestMethod]
        public void TestIntegerInIf()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<UInt2>();
            var e = Zen.If(b, x, new UInt2(0));
            var solution = (e == new UInt2(1)).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsTrue(solution.Get(b));
            Assert.AreEqual(new UInt2(1), solution.Get(x));
        }

        /// <summary>
        /// Test that backends agree on semantics.
        /// </summary>
        [TestMethod]
        public void TestAgreement()
        {
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => x < new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => x > new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => x <= new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => x >= new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => x == new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => x + y == new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => x - y == new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => (x & y) == new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => (x | y) == new Int5(5));
            TestHelper.CheckAgreement<Int5, Int5>((x, y) => (x ^ y) == new Int5(5));
        }
    }
}