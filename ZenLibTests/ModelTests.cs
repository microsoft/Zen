// <copyright file="ModelTests.cs" company="Microsoft">
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
    public class ModelTests
    {
        /// <summary>
        /// Test input generation for the PFC model.
        /// </summary>
        [TestMethod]
        public void TestInputGenerationPfc()
        {
            Assert.AreEqual(8, PfcModel.GenerateTests().Count());
        }
    }
}