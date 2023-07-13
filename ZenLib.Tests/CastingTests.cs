// <copyright file="CastingTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.Solver;

    /// <summary>
    /// Tests conversion from values to Zen values.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CastingTests
    {
        /// <summary>
        /// Test that we can cast finite integer values..
        /// </summary>
        [TestMethod]
        public void TestCastingFiniteIntegers()
        {
            var b1 = new ZenFunction<byte, short>(x => Zen.Cast<byte, short>(x));
            var b2 = new ZenFunction<byte, ushort>(x => Zen.Cast<byte, ushort>(x));
            var b3 = new ZenFunction<byte, int>(x => Zen.Cast<byte, int>(x));
            var b4 = new ZenFunction<byte, uint>(x => Zen.Cast<byte, uint>(x));
            var b5 = new ZenFunction<byte, long>(x => Zen.Cast<byte, long>(x));
            var b6 = new ZenFunction<byte, ulong>(x => Zen.Cast<byte, ulong>(x));

            var s1 = new ZenFunction<short, byte>(x => Zen.Cast<short, byte>(x));
            var s2 = new ZenFunction<short, ushort>(x => Zen.Cast<short, ushort>(x));
            var s3 = new ZenFunction<short, int>(x => Zen.Cast<short, int>(x));
            var s4 = new ZenFunction<short, uint>(x => Zen.Cast<short, uint>(x));
            var s5 = new ZenFunction<short, long>(x => Zen.Cast<short, long>(x));
            var s6 = new ZenFunction<short, ulong>(x => Zen.Cast<short, ulong>(x));

            var us1 = new ZenFunction<ushort, byte>(x => Zen.Cast<ushort, byte>(x));
            var us2 = new ZenFunction<ushort, short>(x => Zen.Cast<ushort, short>(x));
            var us3 = new ZenFunction<ushort, int>(x => Zen.Cast<ushort, int>(x));
            var us4 = new ZenFunction<ushort, uint>(x => Zen.Cast<ushort, uint>(x));
            var us5 = new ZenFunction<ushort, long>(x => Zen.Cast<ushort, long>(x));
            var us6 = new ZenFunction<ushort, ulong>(x => Zen.Cast<ushort, ulong>(x));

            var i1 = new ZenFunction<int, byte>(x => Zen.Cast<int, byte>(x));
            var i2 = new ZenFunction<int, short>(x => Zen.Cast<int, short>(x));
            var i3 = new ZenFunction<int, ushort>(x => Zen.Cast<int, ushort>(x));
            var i4 = new ZenFunction<int, uint>(x => Zen.Cast<int, uint>(x));
            var i5 = new ZenFunction<int, long>(x => Zen.Cast<int, long>(x));
            var i6 = new ZenFunction<int, ulong>(x => Zen.Cast<int, ulong>(x));

            var ui1 = new ZenFunction<uint, byte>(x => Zen.Cast<uint, byte>(x));
            var ui2 = new ZenFunction<uint, short>(x => Zen.Cast<uint, short>(x));
            var ui3 = new ZenFunction<uint, ushort>(x => Zen.Cast<uint, ushort>(x));
            var ui4 = new ZenFunction<uint, int>(x => Zen.Cast<uint, int>(x));
            var ui5 = new ZenFunction<uint, long>(x => Zen.Cast<uint, long>(x));
            var ui6 = new ZenFunction<uint, ulong>(x => Zen.Cast<uint, ulong>(x));

            var l1 = new ZenFunction<long, byte>(x => Zen.Cast<long, byte>(x));
            var l2 = new ZenFunction<long, short>(x => Zen.Cast<long, short>(x));
            var l3 = new ZenFunction<long, ushort>(x => Zen.Cast<long, ushort>(x));
            var l4 = new ZenFunction<long, int>(x => Zen.Cast<long, int>(x));
            var l5 = new ZenFunction<long, uint>(x => Zen.Cast<long, uint>(x));
            var l6 = new ZenFunction<long, ulong>(x => Zen.Cast<long, ulong>(x));

            var ul1 = new ZenFunction<ulong, byte>(x => Zen.Cast<ulong, byte>(x));
            var ul2 = new ZenFunction<ulong, short>(x => Zen.Cast<ulong, short>(x));
            var ul3 = new ZenFunction<ulong, ushort>(x => Zen.Cast<ulong, ushort>(x));
            var ul4 = new ZenFunction<ulong, int>(x => Zen.Cast<ulong, int>(x));
            var ul5 = new ZenFunction<ulong, uint>(x => Zen.Cast<ulong, uint>(x));
            var ul6 = new ZenFunction<ulong, long>(x => Zen.Cast<ulong, long>(x));

            Assert.AreEqual((short)5, b1.Evaluate(5));
            Assert.AreEqual((ushort)5, b2.Evaluate(5));
            Assert.AreEqual(5, b3.Evaluate(5));
            Assert.AreEqual(5U, b4.Evaluate(5));
            Assert.AreEqual(5L, b5.Evaluate(5));
            Assert.AreEqual(5UL, b6.Evaluate(5));

            Assert.AreEqual((byte)5, s1.Evaluate(5));
            Assert.AreEqual((ushort)5, s2.Evaluate(5));
            Assert.AreEqual(5, s3.Evaluate(5));
            Assert.AreEqual(5U, s4.Evaluate(5));
            Assert.AreEqual(5L, s5.Evaluate(5));
            Assert.AreEqual(5UL, s6.Evaluate(5));

            Assert.AreEqual((byte)5, us1.Evaluate(5));
            Assert.AreEqual((short)5, us2.Evaluate(5));
            Assert.AreEqual(5, us3.Evaluate(5));
            Assert.AreEqual(5U, us4.Evaluate(5));
            Assert.AreEqual(5L, us5.Evaluate(5));
            Assert.AreEqual(5UL, us6.Evaluate(5));

            Assert.AreEqual((byte)5, i1.Evaluate(5));
            Assert.AreEqual((short)5, i2.Evaluate(5));
            Assert.AreEqual((ushort)5, i3.Evaluate(5));
            Assert.AreEqual(5U, i4.Evaluate(5));
            Assert.AreEqual(5L, i5.Evaluate(5));
            Assert.AreEqual(5UL, i6.Evaluate(5));

            Assert.AreEqual((byte)5, ui1.Evaluate(5));
            Assert.AreEqual((short)5, ui2.Evaluate(5));
            Assert.AreEqual((ushort)5, ui3.Evaluate(5));
            Assert.AreEqual(5, ui4.Evaluate(5));
            Assert.AreEqual(5L, ui5.Evaluate(5));
            Assert.AreEqual(5UL, ui6.Evaluate(5));

            Assert.AreEqual((byte)5, l1.Evaluate(5));
            Assert.AreEqual((short)5, l2.Evaluate(5));
            Assert.AreEqual((ushort)5, l3.Evaluate(5));
            Assert.AreEqual(5, l4.Evaluate(5));
            Assert.AreEqual(5U, l5.Evaluate(5));
            Assert.AreEqual(5UL, l6.Evaluate(5));

            Assert.AreEqual((byte)5, ul1.Evaluate(5));
            Assert.AreEqual((short)5, ul2.Evaluate(5));
            Assert.AreEqual((ushort)5, ul3.Evaluate(5));
            Assert.AreEqual(5, ul4.Evaluate(5));
            Assert.AreEqual(5U, ul5.Evaluate(5));
            Assert.AreEqual(5L, ul6.Evaluate(5));

            b1.Compile();
            b2.Compile();
            b3.Compile();
            b4.Compile();
            b5.Compile();
            b6.Compile();

            s1.Compile();
            s2.Compile();
            s3.Compile();
            s4.Compile();
            s5.Compile();
            s6.Compile();

            us1.Compile();
            us2.Compile();
            us3.Compile();
            us4.Compile();
            us5.Compile();
            us6.Compile();

            i1.Compile();
            i2.Compile();
            i3.Compile();
            i4.Compile();
            i5.Compile();
            i6.Compile();

            ui1.Compile();
            ui2.Compile();
            ui3.Compile();
            ui4.Compile();
            ui5.Compile();
            ui6.Compile();

            l1.Compile();
            l2.Compile();
            l3.Compile();
            l4.Compile();
            l5.Compile();
            l6.Compile();

            ul1.Compile();
            ul2.Compile();
            ul3.Compile();
            ul4.Compile();
            ul5.Compile();
            ul6.Compile();

            Assert.AreEqual((short)5, b1.Evaluate(5));
            Assert.AreEqual((ushort)5, b2.Evaluate(5));
            Assert.AreEqual(5, b3.Evaluate(5));
            Assert.AreEqual(5U, b4.Evaluate(5));
            Assert.AreEqual(5L, b5.Evaluate(5));
            Assert.AreEqual(5UL, b6.Evaluate(5));

            Assert.AreEqual((byte)5, s1.Evaluate(5));
            Assert.AreEqual((ushort)5, s2.Evaluate(5));
            Assert.AreEqual(5, s3.Evaluate(5));
            Assert.AreEqual(5U, s4.Evaluate(5));
            Assert.AreEqual(5L, s5.Evaluate(5));
            Assert.AreEqual(5UL, s6.Evaluate(5));

            Assert.AreEqual((byte)5, us1.Evaluate(5));
            Assert.AreEqual((short)5, us2.Evaluate(5));
            Assert.AreEqual(5, us3.Evaluate(5));
            Assert.AreEqual(5U, us4.Evaluate(5));
            Assert.AreEqual(5L, us5.Evaluate(5));
            Assert.AreEqual(5UL, us6.Evaluate(5));

            Assert.AreEqual((byte)5, i1.Evaluate(5));
            Assert.AreEqual((short)5, i2.Evaluate(5));
            Assert.AreEqual((ushort)5, i3.Evaluate(5));
            Assert.AreEqual(5U, i4.Evaluate(5));
            Assert.AreEqual(5L, i5.Evaluate(5));
            Assert.AreEqual(5UL, i6.Evaluate(5));

            Assert.AreEqual((byte)5, ui1.Evaluate(5));
            Assert.AreEqual((short)5, ui2.Evaluate(5));
            Assert.AreEqual((ushort)5, ui3.Evaluate(5));
            Assert.AreEqual(5, ui4.Evaluate(5));
            Assert.AreEqual(5L, ui5.Evaluate(5));
            Assert.AreEqual(5UL, ui6.Evaluate(5));

            Assert.AreEqual((byte)5, l1.Evaluate(5));
            Assert.AreEqual((short)5, l2.Evaluate(5));
            Assert.AreEqual((ushort)5, l3.Evaluate(5));
            Assert.AreEqual(5, l4.Evaluate(5));
            Assert.AreEqual(5U, l5.Evaluate(5));
            Assert.AreEqual(5UL, l6.Evaluate(5));

            Assert.AreEqual((byte)5, ul1.Evaluate(5));
            Assert.AreEqual((short)5, ul2.Evaluate(5));
            Assert.AreEqual((ushort)5, ul3.Evaluate(5));
            Assert.AreEqual(5, ul4.Evaluate(5));
            Assert.AreEqual(5U, ul5.Evaluate(5));
            Assert.AreEqual(5L, ul6.Evaluate(5));
        }

        /// <summary>
        /// Test that we can cast finite integer values..
        /// </summary>
        [TestMethod]
        public void TestCastingFixedIntegers1()
        {
            var zf = new ZenFunction<byte, UInt<_64>>(x => Zen.Cast<byte, UInt<_64>>(x));
            Assert.AreEqual(5L, zf.Evaluate(5).ToLong());
            zf.Compile();
            Assert.AreEqual(5L, zf.Evaluate(5).ToLong());
        }

        /// <summary>
        /// Test that we can cast finite integer values..
        /// </summary>
        [TestMethod]
        public void TestCastingFixedIntegers2()
        {
            var zf = new ZenFunction<UInt<_16>, UInt<_8>>(x => Zen.Cast<UInt<_16>, UInt<_8>>(x));
            Assert.AreEqual(1, zf.Evaluate(new UInt<_16>(257)).ToLong());
            zf.Compile();
            Assert.AreEqual(1, zf.Evaluate(new UInt<_16>(257)).ToLong());
        }

        /// <summary>
        /// Test that we can cast finite integer values..
        /// </summary>
        [TestMethod]
        public void TestCastingFixedIntegers3()
        {
            var zf = new ZenFunction<UInt<_16>, byte>(x => Zen.Cast<UInt<_16>, byte>(x));
            Assert.AreEqual((byte)1, zf.Evaluate(new UInt<_16>(257)));
            zf.Compile();
            Assert.AreEqual((byte)1, zf.Evaluate(new UInt<_16>(257)));
        }

        /// <summary>
        /// Test that we can cast finite integer values..
        /// </summary>
        [TestMethod]
        public void TestCastingFixedIntegers4()
        {
            var zf = new ZenFunction<byte, Int<_3>>(x => Zen.Cast<byte, Int<_3>>(x));
            Assert.AreEqual(-1, zf.Evaluate(7).ToLong());
            zf.Compile();
            Assert.AreEqual(-1, zf.Evaluate(7).ToLong());
        }

        /// <summary>
        /// Test that we can cast finite integer values..
        /// </summary>
        [TestMethod]
        public void TestCastingFixedIntegers5()
        {
            var zf = new ZenFunction<Int<_3>, UInt<_8>>(x => Zen.Cast<Int<_3>, UInt<_8>>(x));
            Assert.AreEqual(7, zf.Evaluate(new Int<_3>(-1)).ToLong());
            zf.Compile();
            Assert.AreEqual(7, zf.Evaluate(new Int<_3>(-1)).ToLong());
        }

        /// <summary>
        /// Test that casting finite integers works with overflow.
        /// </summary>
        [TestMethod]
        public void TestFiniteIntegerCastOverflow1()
        {
            var zf = new ZenFunction<short, byte>(x => Zen.Cast<short, byte>(x));

            foreach (var backend in new SolverType[] { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var example = zf.Find((s, b) => Zen.And(s == 257, b == 1), config: new SolverConfig { SolverType = backend });
                Assert.IsTrue(example.HasValue);
                Assert.AreEqual((short)257, example.Value);
                Assert.AreEqual((byte)1, zf.Evaluate(257));
            }
        }

        /// <summary>
        /// Test that casting finite integers works with overflow.
        /// </summary>
        [TestMethod]
        public void TestFiniteIntegerCastOverflow2()
        {
            var zf = new ZenFunction<int, byte>(x => Zen.Cast<int, byte>(x));

            foreach (var backend in new SolverType[] { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var example = zf.Find((s, b) => Zen.And(s == 257, b == 1), config: new SolverConfig { SolverType = backend });
                Assert.IsTrue(example.HasValue);
                Assert.AreEqual((int)257, example.Value);
                Assert.AreEqual((byte)1, zf.Evaluate(257));
            }
        }

        /// <summary>
        /// Test that casting finite integers works with overflow.
        /// </summary>
        [TestMethod]
        public void TestFiniteIntegerCastOverflow3()
        {
            var zf = new ZenFunction<long, byte>(x => Zen.Cast<long, byte>(x));

            foreach (var backend in new SolverType[] { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var example = zf.Find((s, b) => Zen.And(s == 257, b == 1), config: new SolverConfig { SolverType = backend });
                Assert.IsTrue(example.HasValue);
                Assert.AreEqual(257L, example.Value);
                Assert.AreEqual((byte)1, zf.Evaluate(257));
            }
        }

        /// <summary>
        /// Test that casting finite integers works with underflow.
        /// </summary>
        [TestMethod]
        public void TestFiniteIntegerCastUnderflow()
        {
            var zf = new ZenFunction<byte, short>(x => Zen.Cast<byte, short>(x));

            foreach (var backend in new SolverType[] { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var example = zf.Find((b, s) => Zen.And(b == 4, s == 4), config: new SolverConfig { SolverType = backend });
                Assert.IsTrue(example.HasValue);
                Assert.AreEqual((byte)4, example.Value);
                Assert.AreEqual((short)4, zf.Evaluate(4));
            }
        }

        /// <summary>
        /// Test that casting finite integers works with signs.
        /// </summary>
        [TestMethod]
        public void TestFiniteIntegerCastSignConversion()
        {
            var zf = new ZenFunction<ushort, short>(x => Zen.Cast<ushort, short>(x));

            foreach (var backend in new SolverType[] { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var example = zf.Find((u, s) => Zen.And(u == ushort.MaxValue, s == -1), config: new SolverConfig { SolverType = backend });
                Assert.IsTrue(example.HasValue);
                Assert.AreEqual(ushort.MaxValue, example.Value);
                Assert.AreEqual((short)-1, zf.Evaluate(ushort.MaxValue));
            }
        }

        /// <summary>
        /// Test that casting fixed integers works.
        /// </summary>
        [TestMethod]
        public void TestFixedIntegerCastConversion1()
        {
            var zf = new ZenFunction<UInt<_16>, ulong>(x => Zen.Cast<UInt<_16>, ulong>(x));

            foreach (var backend in new SolverType[] { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var example = zf.Find((u, s) => u == new UInt<_16>(ushort.MaxValue), config: new SolverConfig { SolverType = backend });
                Assert.IsTrue(example.HasValue);
                Assert.AreEqual(new UInt<_16>(ushort.MaxValue), example.Value);
                Assert.AreEqual((ulong)ushort.MaxValue, zf.Evaluate(new UInt<_16>(ushort.MaxValue)));
            }
        }

        /// <summary>
        /// Test that casting fixed integers works.
        /// </summary>
        [TestMethod]
        public void TestFixedIntegerCastConversion2()
        {
            var zf = new ZenConstraint<UInt<_3>>(x => Zen.Cast<UInt<_3>, Int<_3>>(x) < new Int<_3>(0));

            foreach (var backend in new SolverType[] { SolverType.Z3, SolverType.DecisionDiagrams })
            {
                var example = zf.Find(config: new SolverConfig { SolverType = backend });
                Assert.IsTrue(example.HasValue);
                Assert.IsTrue(example.Value.ToLong() >= 4);
                Assert.IsTrue(zf.Evaluate(example.Value));
            }
        }
    }
}