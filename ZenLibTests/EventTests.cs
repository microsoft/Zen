// <copyright file="EventTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static ZenLib.Language;

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
            // use the model to generate an example input.
            foreach (var eventList in PfcModel.GenerateTests())
            {
                Console.WriteLine($"{string.Join("\n", eventList)}\n");
            }
        }
    }
}