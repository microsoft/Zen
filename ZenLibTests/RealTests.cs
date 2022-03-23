// <copyright file="RealTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for the Zen bag type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RealTests
    {
        /// <summary>
        /// Test Real implicit conversion.
        /// </summary>
        [TestMethod]
        public void TestRealImplicitConversion()
        {
            Real r = 10;
            Assert.AreEqual(r.Denominator, BigInteger.One);
            Assert.AreEqual(r.Numerator, new BigInteger(10));
        }

        /// <summary>
        /// Test Real simplifications.
        /// </summary>
        [TestMethod]
        public void TestRealSimplifications()
        {
            Assert.AreEqual(new Real(-1, -2), new Real(1, 2));
            Assert.AreEqual(new Real(6, 3), new Real(2, 1));
            Assert.AreEqual(new Real(6, -3), new Real(-2, 1));
            Assert.AreEqual(new Real(0, 4), new Real(0, 1));
        }

        /// <summary>
        /// Test Char equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestRealEqualityHashcode()
        {
            Real r1 = 10;
            Real r2 = 12;
            Real r3 = new Real(10, 3);
            Real r4 = new Real(20, 2);

            Assert.IsTrue(r1 != r2);
            Assert.IsTrue(r1 != r3);
            Assert.IsTrue(r1 == r4);
            Assert.AreNotEqual(r1, new object());
            Assert.AreEqual(r1.GetHashCode(), r4.GetHashCode());
            Assert.AreNotEqual(r1.GetHashCode(), r3.GetHashCode());
        }

        /// <summary>
        /// Test Real comparison.
        /// </summary>
        [TestMethod]
        public void TestRealComparisons()
        {
            Real r1 = 10;
            Real r2 = 11;
            Real r3 = new Real(21, 2);

            Assert.IsTrue(r1 < r2);
            Assert.IsTrue(r1 < r3);
            Assert.IsTrue(r2 > r3);
            Assert.IsTrue(r2 > r1);
            Assert.IsTrue(r1 <= r2);
            Assert.IsTrue(r1 <= r3);
            Assert.IsFalse(r1 >= r2);
            Assert.IsFalse(r1 >= r3);
            Assert.IsTrue(r2 >= r3);
            Assert.IsTrue(r2 >= r1);
        }

        /// <summary>
        /// Test Real throws exception for denominator 0.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestZeroDenominatorException()
        {
            new Real(1, 0);
        }

        /// <summary>
        /// Test Real addition.
        /// </summary>
        [TestMethod]
        [DataRow(10, 3, 13)]
        [DataRow(-4, 2, -2)]
        [DataRow(-4, -3, -7)]
        [DataRow(0, 3, 3)]
        [DataRow(3, 0, 3)]
        [DataRow(-10, 10, 0)]
        public void TestRealAdd(long x, long y, long z)
        {
            Real r1 = x;
            Real r2 = y;
            Real r3 = z;
            Assert.AreEqual(r3, r1 + r2);
        }

        /// <summary>
        /// Test Real addition.
        /// </summary>
        [TestMethod]
        public void TestRealAddFractional()
        {
            Assert.AreEqual(new Real(61, 12), new Real(10, 3) + new Real(7, 4));
        }

        /// <summary>
        /// Test Real subtraction.
        /// </summary>
        [TestMethod]
        [DataRow(10, 3, 7)]
        [DataRow(-4, 2, -6)]
        [DataRow(-4, -3, -1)]
        [DataRow(0, 3, -3)]
        [DataRow(3, 0, 3)]
        [DataRow(10, 10, 0)]
        public void TestRealSubtract(long x, long y, long z)
        {
            Real r1 = x;
            Real r2 = y;
            Real r3 = z;
            Assert.AreEqual(r3, r1 - r2);
        }

        /// <summary>
        /// Test Real subtraction.
        /// </summary>
        [TestMethod]
        public void TestRealSubtractFractional()
        {
            Assert.AreEqual(new Real(19, 12), new Real(10, 3) - new Real(7, 4));
        }

        /// <summary>
        /// Test Real multiplication.
        /// </summary>
        [TestMethod]
        [DataRow(10, 3, 30)]
        [DataRow(-4, 2, -8)]
        [DataRow(-4, -3, 12)]
        [DataRow(0, 3, 0)]
        [DataRow(3, 0, 0)]
        [DataRow(10, 1, 10)]
        public void TestRealMultiplication(long x, long y, long z)
        {
            Real r1 = x;
            Real r2 = y;
            Real r3 = z;
            Assert.AreEqual(r3, r1 * r2);
        }

        /// <summary>
        /// Test Real multiplication.
        /// </summary>
        [TestMethod]
        public void TestRealMultiplyFractional()
        {
            Assert.AreEqual(new Real(35, 6), new Real(10, 3) * new Real(7, 4));
        }
    }
}
