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
        /// Test that conversions from strings and sequences works.
        /// </summary>
        [TestMethod]
        public void TestStringConversions1()
        {
            var s1 = "abcd";
            var s2 = Seq.AsString(Seq.FromString(s1));
            Assert.AreEqual(s1, s2);
        }

        /// <summary>
        /// Test that conversions from strings and sequences works.
        /// </summary>
        [TestMethod]
        public void TestStringConversions2()
        {
            var s1 = char.ConvertFromUtf32(0xffff);
            var s2 = Seq.AsString(Seq.FromString(s1));
            Assert.AreEqual(s1, s2);
        }
    }
}
