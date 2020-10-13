// <copyright file="EventTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EventTests
    {
        /// <summary>
        /// Test solving for preconditions.
        /// </summary>
        [TestMethod]
        public void TestInputGeneration()
        {
            Assert.AreEqual(8, PfcModel.GenerateTests().Count());
        }
    }
}