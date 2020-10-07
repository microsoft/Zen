// <copyright file="FindAllTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static ZenLib.Language;

    /// <summary>
    /// Tests for finding multiple inputs.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FindAllTests
    {
        /// <summary>
        /// Test that find all works.
        /// </summary>
        [TestMethod]
        public void TestFindAll()
        {
            Assert.AreEqual(11, Function<ushort, bool>(x => x <= 10).FindAll((i, o) => o).Count());
            Assert.AreEqual(2, Function<uint, bool>(x => And(x > 1, x <= 3)).FindAll((i, o) => o).Count());
            Assert.AreEqual(5, Function<ulong, bool>(x => x <= 4).FindAll((i, o) => o).Count());

            Assert.AreEqual(2, Function<IList<int>, bool>(
                x => And(x.Contains(1), x.Contains(2))).FindAll(listSize: 2, invariant: (i, o) => o).Count());

            Assert.AreEqual(6, Function<uint, uint, bool>(
                (x, y) => And(x >= y, x <= 2)).FindAll((i1, i2, o) => o).Count());

            Assert.AreEqual(24, Function<uint, uint, uint, bool>(
                (x, y, z) => And(x >= y, x <= 2, z <= 3)).FindAll((i1, i2, i3, o) => o).Count());

            Assert.AreEqual(24, Function<uint, uint, uint, uint, bool>(
                (x, y, z1, z2) => And(x >= y, x <= 2, z1 <= 1, z2 <= 1)).FindAll((i1, i2, i3, i4, o) => o).Count());
        }
    }
}