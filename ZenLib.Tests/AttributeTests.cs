// <copyright file="AttributeTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            var o = Symbolic<ObjectAttribute>(depth: 3);
            var s1 = o.GetField<ObjectAttribute, FSeq<int>>("Field1").ToString();
            var s2 = o.GetField<ObjectAttribute, FSeq<int>>("Field2").ToString();
            var s3 = o.GetField<ObjectAttribute, FSeq<int>>("Field3").ToString();
            Assert.AreEqual(11, s1.Split("Cons").Length);
            Assert.AreEqual(6, s2.Split("Cons").Length);
            Assert.AreEqual(4, s3.Split("Cons").Length);
        }

        private struct ObjectAttribute
        {
            [ZenSize(depth: 10)]
            public FSeq<int> Field1;

            [ZenSize(depth: 5)]
            public FSeq<int> Field2;

            [ZenSize(depth: -1)]
            public FSeq<int> Field3;
        }
    }
}
