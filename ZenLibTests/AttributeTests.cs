// <copyright file="AttributeTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Z3;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen object type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AttributeTests
    {
        /// <summary>
        /// Test that attributes work correctly for depth.
        /// </summary>
        [TestMethod]
        public void TestGenerationDepth()
        {
            // work around unused field warning.
            ObjectAttribute o1;
            o1.Field1 = new FSeq<int>();
            o1.Field2 = new FSeq<int>();
            o1.Field3 = new FSeq<int>();

            var o = Symbolic<ObjectAttribute>(depth: 3, exhaustiveDepth: true);

            var s1 = o.GetField<ObjectAttribute, FSeq<int>>("Field1").ToString();
            var s2 = o.GetField<ObjectAttribute, FSeq<int>>("Field2").ToString();
            var s3 = o.GetField<ObjectAttribute, FSeq<int>>("Field3").ToString();

            Console.WriteLine(s2);

            Assert.AreEqual(11, s1.Split("Cons").Length);
            Assert.AreEqual(6, s2.Split("==").Length);
            Assert.AreEqual(4, s3.Split("==").Length);
        }

        private struct ObjectAttribute
        {
            [ZenSize(depth: 10, EnumerationType.FixedSize)]
            public FSeq<int> Field1;

            [ZenSize(depth: 5, EnumerationType.Exhaustive)]
            public FSeq<int> Field2;

            [ZenSize(depth: -1, EnumerationType.User)]
            public FSeq<int> Field3;
        }
    }
}
