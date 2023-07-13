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
            Assert.AreEqual(expected, new Int<_9>(x) < new Int<_9>(y));
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
        public void TestLessThanOrEqualSigned(long x, long y, bool expected)
        {
            Assert.AreEqual(expected, new Int<_9>(x) <= new Int<_9>(y));
        }

        /// <summary>
        /// Test that less than or equal works as expected.
        /// </summary>
        [TestMethod]
        [DataRow(3, 4, true)]
        [DataRow(4, 3, false)]
        [DataRow(0, 1, true)]
        [DataRow(10, 20, true)]
        [DataRow(20, 10, false)]
        [DataRow(1, 2, true)]
        [DataRow(255, 256, true)]
        public void TestLessThanOrEqualUnsigned(long x, long y, bool expected)
        {
            Assert.AreEqual(expected, new UInt<_9>(x) <= new UInt<_9>(y));
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
        public void TestGreaterThanOrEqualSigned(long x, long y, bool expected)
        {
            Assert.AreEqual(expected, new Int<_9>(x) >= new Int<_9>(y));
        }

        /// <summary>
        /// Test that greater than or equal works as expected.
        /// </summary>
        [TestMethod]
        [DataRow(3, 4, false)]
        [DataRow(4, 3, true)]
        [DataRow(0, 1, false)]
        [DataRow(10, 10, true)]
        [DataRow(8, 9, false)]
        [DataRow(0, 1, false)]
        [DataRow(10, 20, false)]
        [DataRow(50, 30, true)]
        [DataRow(51, 50, true)]
        [DataRow(120, 129, false)]
        [DataRow(256, 255, true)]
        public void TestGreaterThanOrEqualUnsigned(long x, long y, bool expected)
        {
            Assert.AreEqual(expected, new UInt<_9>(x) >= new UInt<_9>(y));
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
            Assert.AreEqual(!expected, new Int<_27>(x) != new Int<_27>(y));
            Assert.AreEqual(!expected, new UInt<_27>(x) != new UInt<_27>(y));
            Assert.AreEqual(expected, new Int<_27>(x).GetHashCode() == new Int<_27>(y).GetHashCode());
            Assert.AreEqual(expected, new UInt<_27>(x).GetHashCode() == new UInt<_27>(y).GetHashCode());
        }

        /// <summary>
        /// Test that less than or equal works as expected.
        /// </summary>
        [TestMethod]
        public void TestEqualityFails()
        {
            Assert.IsFalse(new Int<_1>(0).Equals(null));
            Assert.IsFalse(new Int<_1>(0).Equals(new Int<_2>(0)));
        }

        /// <summary>
        /// Test that bitwise and works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseAnd()
        {
            Assert.AreEqual(new UInt<_3>(4).BitwiseAnd(new UInt<_3>(3)), new UInt<_3>(0));
            Assert.AreEqual(new UInt<_3>(1).BitwiseAnd(new UInt<_3>(3)), new UInt<_3>(1));
            Assert.AreEqual(new UInt<_3>(2).BitwiseAnd(new UInt<_3>(3)), new UInt<_3>(2));
            Assert.AreEqual(new UInt<_10>(257).BitwiseAnd(new UInt<_10>(257)), new UInt<_10>(257));
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

                Assert.AreEqual(b1 & b2, (byte)new UInt<_8>(b1).BitwiseAnd(new UInt<_8>(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise or works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseOr()
        {
            Assert.AreEqual(new UInt<_3>(4).BitwiseOr(new UInt<_3>(3)), new UInt<_3>(7));
            Assert.AreEqual(new UInt<_3>(1).BitwiseOr(new UInt<_3>(3)), new UInt<_3>(3));
            Assert.AreEqual(new UInt<_3>(2).BitwiseOr(new UInt<_3>(3)), new UInt<_3>(3));
            Assert.AreEqual(new UInt<_10>(257).BitwiseOr(new UInt<_10>(257)), new UInt<_10>(257));
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

                Assert.AreEqual(b1 | b2, (byte)new UInt<_8>(b1).BitwiseOr(new UInt<_8>(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise xor works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseXor()
        {
            Assert.AreEqual(new UInt<_3>(4).BitwiseXor(new UInt<_3>(3)), new UInt<_3>(7));
            Assert.AreEqual(new UInt<_3>(1).BitwiseXor(new UInt<_3>(3)), new UInt<_3>(2));
            Assert.AreEqual(new UInt<_3>(2).BitwiseXor(new UInt<_3>(3)), new UInt<_3>(1));
            Assert.AreEqual(new Int<_10>(257).BitwiseXor(new Int<_10>(257)), new Int<_10>(0));
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

                Assert.AreEqual(b1 ^ b2, (byte)new UInt<_8>(b1).BitwiseXor(new UInt<_8>(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise not works as expected.
        /// </summary>
        [TestMethod]
        public void TestBitwiseNot()
        {
            Assert.AreEqual(new UInt<_3>(4).BitwiseNot(), new UInt<_3>(3));
            Assert.AreEqual(new UInt<_3>(1).BitwiseNot(), new UInt<_3>(6));
            Assert.AreEqual(new Int<_3>(-1).BitwiseNot(), new Int<_3>(0));
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
                Assert.AreEqual((byte)~b, (byte)new UInt<_8>(b).BitwiseNot().ToLong());
            }
        }

        /// <summary>
        /// Test that bitwise not works as expected.
        /// </summary>
        [TestMethod]
        public void TestNegation()
        {
            Assert.AreEqual(new Int<_6>(-1).Negate(), new Int<_6>(1));
            Assert.AreEqual(new Int<_6>(-2).Negate(), new Int<_6>(2));
            Assert.AreEqual(new Int<_6>(5).Negate(), new Int<_6>(-5));
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
                Assert.AreEqual(-b, (int)new Int<_8>(b).Negate().ToLong());
            }
        }

        /// <summary>
        /// Test that addition works as expected.
        /// </summary>
        [TestMethod]
        public void TestAddition()
        {
            Assert.AreEqual(new Int<_5>(4).Add(new Int<_5>(3)), new Int<_5>(7));
            Assert.AreEqual(new Int<_5>(1).Add(new Int<_5>(3)), new Int<_5>(4));
            Assert.AreEqual(new Int<_5>(2).Add(new Int<_5>(3)), new Int<_5>(5));
            Assert.AreEqual(new Int<_5>(-1).Add(new Int<_5>(-2)), new Int<_5>(-3));
            Assert.AreEqual(new Int<_5>(-1).Add(new Int<_5>(1)), new Int<_5>(0));
            Assert.AreEqual(new UInt<_1>(1).Add(new UInt<_1>(1)), new UInt<_1>(0));
            Assert.AreEqual(new UInt<_5>(10).Add(new UInt<_5>(31)), new UInt<_5>(9));
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

                Assert.AreEqual((byte)(b1 + b2), (byte)new UInt<_8>(b1).Add(new UInt<_8>(b2)).ToLong());
            }
        }

        /// <summary>
        /// Test that addition works as expected.
        /// </summary>
        [TestMethod]
        public void TestSubtraction()
        {
            Assert.AreEqual(new UInt<_5>(10).Subtract(new UInt<_5>(4)), new UInt<_5>(6));
            Assert.AreEqual(new UInt<_5>(0).Subtract(new UInt<_5>(1)), new UInt<_5>(31));
            Assert.AreEqual(new Int<_5>(4).Subtract(new Int<_5>(3)), new Int<_5>(1));
            Assert.AreEqual(new Int<_5>(4).Subtract(new Int<_5>(-1)), new Int<_5>(5));
            Assert.AreEqual(new Int<_5>(-1).Subtract(new Int<_5>(-1)), new Int<_5>(0));
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

                Assert.AreEqual((byte)(b1 - b2), (byte)new UInt<_8>(b1).Subtract(new UInt<_8>(b2)).ToLong());
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
                Assert.AreEqual(i,  (int)new Int<_10>(i).ToLong());
            }
        }

        /// <summary>
        /// Test that less than or equal works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestToLongException()
        {
            new Int<_128>(0).ToLong();
        }

        /// <summary>
        /// Test that a long range is invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidRange1()
        {
            new Int<_1>(100);
        }

        /// <summary>
        /// Test that a long range is invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidRange2()
        {
            new Int<_1>(-100);
        }

        /// <summary>
        /// Test that the integer size is correct.
        /// </summary>
        [TestMethod]
        public void TestSizeCorrect()
        {
            Assert.IsTrue(Int<_1>.Size == 1);
            Assert.IsTrue(Int<_2>.Size == 2);
            Assert.IsTrue(Int<_3>.Size == 3);
            Assert.IsTrue(Int<_4>.Size == 4);
            Assert.IsTrue(Int<_5>.Size == 5);
            Assert.IsTrue(Int<_6>.Size == 6);
            Assert.IsTrue(Int<_7>.Size == 7);
            Assert.IsTrue(Int<_8>.Size == 8);
            Assert.IsTrue(Int<_9>.Size == 9);

            Assert.IsTrue(Int<_10>.Size == 10);
            Assert.IsTrue(Int<_11>.Size == 11);
            Assert.IsTrue(Int<_12>.Size == 12);
            Assert.IsTrue(Int<_13>.Size == 13);
            Assert.IsTrue(Int<_14>.Size == 14);
            Assert.IsTrue(Int<_15>.Size == 15);
            Assert.IsTrue(Int<_16>.Size == 16);
            Assert.IsTrue(Int<_17>.Size == 17);
            Assert.IsTrue(Int<_18>.Size == 18);
            Assert.IsTrue(Int<_19>.Size == 19);

            Assert.IsTrue(Int<_20>.Size == 20);
            Assert.IsTrue(Int<_21>.Size == 21);
            Assert.IsTrue(Int<_22>.Size == 22);
            Assert.IsTrue(Int<_23>.Size == 23);
            Assert.IsTrue(Int<_24>.Size == 24);
            Assert.IsTrue(Int<_25>.Size == 25);
            Assert.IsTrue(Int<_26>.Size == 26);
            Assert.IsTrue(Int<_27>.Size == 27);
            Assert.IsTrue(Int<_28>.Size == 28);
            Assert.IsTrue(Int<_29>.Size == 29);

            Assert.IsTrue(Int<_30>.Size == 30);
            Assert.IsTrue(Int<_31>.Size == 31);
            Assert.IsTrue(Int<_32>.Size == 32);
            Assert.IsTrue(Int<_33>.Size == 33);
            Assert.IsTrue(Int<_34>.Size == 34);
            Assert.IsTrue(Int<_35>.Size == 35);
            Assert.IsTrue(Int<_36>.Size == 36);
            Assert.IsTrue(Int<_37>.Size == 37);
            Assert.IsTrue(Int<_38>.Size == 38);
            Assert.IsTrue(Int<_39>.Size == 39);

            Assert.IsTrue(Int<_40>.Size == 40);
            Assert.IsTrue(Int<_41>.Size == 41);
            Assert.IsTrue(Int<_42>.Size == 42);
            Assert.IsTrue(Int<_43>.Size == 43);
            Assert.IsTrue(Int<_44>.Size == 44);
            Assert.IsTrue(Int<_45>.Size == 45);
            Assert.IsTrue(Int<_46>.Size == 46);
            Assert.IsTrue(Int<_47>.Size == 47);
            Assert.IsTrue(Int<_48>.Size == 48);
            Assert.IsTrue(Int<_49>.Size == 49);

            Assert.IsTrue(Int<_50>.Size == 50);
            Assert.IsTrue(Int<_51>.Size == 51);
            Assert.IsTrue(Int<_52>.Size == 52);
            Assert.IsTrue(Int<_53>.Size == 53);
            Assert.IsTrue(Int<_54>.Size == 54);
            Assert.IsTrue(Int<_55>.Size == 55);
            Assert.IsTrue(Int<_56>.Size == 56);
            Assert.IsTrue(Int<_57>.Size == 57);
            Assert.IsTrue(Int<_58>.Size == 58);
            Assert.IsTrue(Int<_59>.Size == 59);

            Assert.IsTrue(Int<_60>.Size == 60);
            Assert.IsTrue(Int<_61>.Size == 61);
            Assert.IsTrue(Int<_62>.Size == 62);
            Assert.IsTrue(Int<_63>.Size == 63);
            Assert.IsTrue(Int<_64>.Size == 64);
            Assert.IsTrue(Int<_65>.Size == 65);
            Assert.IsTrue(Int<_66>.Size == 66);
            Assert.IsTrue(Int<_67>.Size == 67);
            Assert.IsTrue(Int<_68>.Size == 68);
            Assert.IsTrue(Int<_69>.Size == 69);

            Assert.IsTrue(Int<_70>.Size == 70);
            Assert.IsTrue(Int<_71>.Size == 71);
            Assert.IsTrue(Int<_72>.Size == 72);
            Assert.IsTrue(Int<_73>.Size == 73);
            Assert.IsTrue(Int<_74>.Size == 74);
            Assert.IsTrue(Int<_75>.Size == 75);
            Assert.IsTrue(Int<_76>.Size == 76);
            Assert.IsTrue(Int<_77>.Size == 77);
            Assert.IsTrue(Int<_78>.Size == 78);
            Assert.IsTrue(Int<_79>.Size == 79);

            Assert.IsTrue(Int<_80>.Size == 80);
            Assert.IsTrue(Int<_81>.Size == 81);
            Assert.IsTrue(Int<_82>.Size == 82);
            Assert.IsTrue(Int<_83>.Size == 83);
            Assert.IsTrue(Int<_84>.Size == 84);
            Assert.IsTrue(Int<_85>.Size == 85);
            Assert.IsTrue(Int<_86>.Size == 86);
            Assert.IsTrue(Int<_87>.Size == 87);
            Assert.IsTrue(Int<_88>.Size == 88);
            Assert.IsTrue(Int<_89>.Size == 89);

            Assert.IsTrue(Int<_90>.Size == 90);
            Assert.IsTrue(Int<_91>.Size == 91);
            Assert.IsTrue(Int<_92>.Size == 92);
            Assert.IsTrue(Int<_93>.Size == 93);
            Assert.IsTrue(Int<_94>.Size == 94);
            Assert.IsTrue(Int<_95>.Size == 95);
            Assert.IsTrue(Int<_96>.Size == 96);
            Assert.IsTrue(Int<_97>.Size == 97);
            Assert.IsTrue(Int<_98>.Size == 98);
            Assert.IsTrue(Int<_99>.Size == 99);

            Assert.IsTrue(Int<_100>.Size == 100);
            Assert.IsTrue(Int<_128>.Size == 128);
            Assert.IsTrue(Int<_256>.Size == 256);
            Assert.IsTrue(Int<_512>.Size == 512);
            Assert.IsTrue(Int<_1024>.Size == 1024);

            Assert.IsTrue(UInt<_100>.Size == 100);
            Assert.IsTrue(Int<_1>.Signed);
            Assert.IsTrue(!UInt<_1>.Signed);
        }

        /// <summary>
        /// Test that the tostring method works.
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("5", new UInt<_7>(5).ToString());
            Assert.AreEqual("-5", new Int<_7>(-5).ToString());
            Assert.AreEqual(128 + 2, "#b00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011".Length);
            Assert.AreEqual("#b00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011", new UInt<_128>(3).ToString());
        }

        /// <summary>
        /// Test interpreting fixed integers.
        /// </summary>
        [TestMethod]
        public void TestInterpretation()
        {
            var f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x + y);
            Assert.AreEqual(new Int<_3>(3), f.Evaluate(new Int<_3>(1), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x - y);
            Assert.AreEqual(new Int<_3>(1), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x | y);
            Assert.AreEqual(new Int<_3>(3), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x & y);
            Assert.AreEqual(new Int<_3>(2), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x ^ y);
            Assert.AreEqual(new Int<_3>(1), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));
        }

        /// <summary>
        /// Test compiling fixed integers.
        /// </summary>
        [TestMethod]
        public void TestCompilation()
        {
            var f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x + y);
            f.Compile();
            Assert.AreEqual(new Int<_3>(3), f.Evaluate(new Int<_3>(1), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x - y);
            f.Compile();
            Assert.AreEqual(new Int<_3>(1), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x | y);
            f.Compile();
            Assert.AreEqual(new Int<_3>(3), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x & y);
            f.Compile();
            Assert.AreEqual(new Int<_3>(2), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));

            f = new ZenFunction<Int<_3>, Int<_3>, Int<_3>>((x, y) => x ^ y);
            f.Compile();
            Assert.AreEqual(new Int<_3>(1), f.Evaluate(new Int<_3>(3), new Int<_3>(2)));
        }

        /// <summary>
        /// Test solving addition.
        /// </summary>
        [TestMethod]
        public void TestSolvingEquality()
        {
            foreach (var backend in new List<SolverType>() { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var f = new ZenFunction<Int<_5>, Int<_5>, bool>((x, y) => x == y);
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
                var f = new ZenFunction<Int<_5>, Int<_5>, Int<_5>>((x, y) => x + y);
                var inputs = f.FindAll((x, y, z) => z == new Int<_5>(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int<_5>(4), input.Item1.Add(input.Item2));
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
                var f = new ZenFunction<Int<_5>, Int<_5>, Int<_5>>((x, y) => x - y);
                var inputs = f.FindAll((x, y, z) => z == new Int<_5>(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int<_5>(4), input.Item1.Subtract(input.Item2));
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
                var f = new ZenFunction<Int<_5>, Int<_5>, Int<_5>>((x, y) => x & y);
                var inputs = f.FindAll((x, y, z) => z == new Int<_5>(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int<_5>(4), input.Item1.BitwiseAnd(input.Item2));
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
                var f = new ZenFunction<Int<_5>, Int<_5>, Int<_5>>((x, y) => x | y);
                var inputs = f.FindAll((x, y, z) => z == new Int<_5>(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int<_5>(4), input.Item1.BitwiseOr(input.Item2));
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
                var f = new ZenFunction<Int<_5>, Int<_5>, Int<_5>>((x, y) => x ^ y);
                var inputs = f.FindAll((x, y, z) => z == new Int<_5>(4)).Take(5);

                foreach (var input in inputs)
                {
                    Assert.AreEqual(new Int<_5>(4), input.Item1.BitwiseXor(input.Item2));
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
                var f = new ZenFunction<Int<_5>, Int<_5>, bool>((x, y) => x <= y);
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
                var f = new ZenFunction<Int<_5>, Int<_5>, bool>((x, y) => x >= y);
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
            var zf = new ZenConstraint<bool, int, UInt<_9>>((b, x, y) =>
            {
                var c1 = If(b, y < new UInt<_9>(10), x == 3);
                var c2 = Implies(b, (y & new UInt<_9>(1)) == new UInt<_9>(1));
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
            var f = new ZenFunction<Option<Int<_5>>, Option<Int<_5>>>(x => Option.Null<Int<_5>>());
            var input = f.Find((x, y) => true);
            Assert.IsTrue(input.HasValue);
        }

        /// <summary>
        /// Test solving with option.
        /// </summary>
        [TestMethod]
        public void Test128BitInteger()
        {
            var a = new UInt<_128>(IPAddress.Parse("1000::").GetAddressBytes());
            var b = new UInt<_128>(IPAddress.Parse("2000::").GetAddressBytes());

            var f = new ZenFunction<UInt<_128>, bool>(x => And(x >= a, x <= b));
            var input = f.Find((x, y) => y, config: new SolverConfig { SolverType = SolverType.DecisionDiagrams });

            Assert.AreEqual(a, input.Value);
        }

        /// <summary>
        /// Test integer in if condition.
        /// </summary>
        [TestMethod]
        public void TestIntegerInIf()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<UInt<_2>>();
            var e = Zen.If(b, x, new UInt<_2>(0));
            var solution = (e == new UInt<_2>(1)).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsTrue(solution.Get(b));
            Assert.AreEqual(new UInt<_2>(1), solution.Get(x));
        }

        /// <summary>
        /// Test that backends agree on semantics.
        /// </summary>
        [TestMethod]
        public void TestAgreement1()
        {
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => x < new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => x > new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => x <= new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => x >= new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => x == new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => x + y == new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => x - y == new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => (x & y) == new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => (x | y) == new Int<_5>(5));
            TestHelper.CheckAgreement<Int<_5>, Int<_5>>((x, y) => (x ^ y) == new Int<_5>(5));
        }

        /// <summary>
        /// Test that backends agree on semantics.
        /// </summary>
        [TestMethod]
        public void TestAgreement2()
        {
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => x < new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => x > new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => x <= new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => x >= new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => x == new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => x + y == new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => x - y == new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => (x & y) == new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => (x | y) == new UInt<_5>(5));
            TestHelper.CheckAgreement<UInt<_5>, UInt<_5>>((x, y) => (x ^ y) == new UInt<_5>(5));
        }

        /// <summary>
        /// Test that using an invalid size fails.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TypeInitializationException))]
        public void TestInvalidSize()
        {
            new Int<int>(0);
        }
    }
}