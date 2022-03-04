// <copyright file="CharTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen bag type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CharTests
    {
        /// <summary>
        /// Test Char implicit conversion.
        /// </summary>
        [TestMethod]
        public void TestCharImplicitConversion()
        {
            for (int i = 0; i < 256; i++)
            {
                ZenLib.Char c = (char)i;
                Assert.AreEqual(((char)i).ToString(), c.ToString());
            }
        }

        /// <summary>
        /// Test Char implicit conversion.
        /// </summary>
        [TestMethod]
        public void TestCharSurrogatePairs1()
        {
            ZenLib.Char c = char.MaxValue;
            Assert.AreEqual(1, c.ToString().Length);
        }

        /// <summary>
        /// Test Char surrogate pairs is working.
        /// </summary>
        [TestMethod]
        public void TestCharSurrogatePairs2()
        {
            for (int i = 0x10000; i < 0x2ffff; i++)
            {
                ZenLib.Char c = new ZenLib.Char(i);
                Assert.AreEqual(2, c.ToString().Length);
            }
        }

        /// <summary>
        /// Test Char equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestCharEqualityHashcode()
        {
            ZenLib.Char c1 = 'a';
            ZenLib.Char c2 = new ZenLib.Char(97);
            ZenLib.Char c3 = char.MaxValue;

            Assert.AreEqual(c1, c2);
            Assert.AreNotEqual(c1, c3);
            Assert.AreNotEqual(c1, new object());
            Assert.AreEqual(c1.GetHashCode(), c2.GetHashCode());
            Assert.AreNotEqual(c1.GetHashCode(), c3.GetHashCode());
        }

        /// <summary>
        /// Test Char throws exception for invalid range.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidCharacterException1()
        {
            new ZenLib.Char(-1);
        }

        /// <summary>
        /// Test Char throws exception for invalid range.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidCharacterException2()
        {
            new ZenLib.Char(0x30000);
        }

        /// <summary>
        /// Test Char doesnt throw an exception for a valid range.
        /// </summary>
        [TestMethod]
        public void TestValidCharacter()
        {
            new ZenLib.Char(0x2ffff);
        }
    }
}
