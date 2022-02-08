// <copyright file="BagTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
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
    public class BagTests
    {
        /// <summary>
        /// Test that converting a bag to and from an array works.
        /// </summary>
        [TestMethod]
        public void TestBagToArray()
        {
            var a1 = new int[] { 1, 2, 3, 4 };
            var b = Bag.FromArray(a1);
            var a2 = b.ToArray();
            Assert.AreEqual(a1.Length, a2.Length);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(a1[i], a2[i]);
            }
        }

        /// <summary>
        /// Test that size constraints are enforced correctly.
        /// </summary>
        [TestMethod]
        public void TestBagSizingAnnotation()
        {
            var zf = new ZenFunction<Bag<byte>, bool>(l => l.Size() == 5);
            var example1 = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example1.HasValue);
            Assert.AreEqual(5, example1.Value.Values.Values.Count);

            zf = new ZenFunction<Bag<byte>, bool>(l => l.Size() == 3);
            var example2 = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example2.HasValue);
            Assert.AreEqual(5, example1.Value.Values.Values.Count);
        }

        /// <summary>
        /// Test the Bag remove method is scalable due to lack of ordering,
        /// unlike with the sequence type that must preserve order.
        /// </summary>
        [TestMethod]
        public void TestBagRemoveIsScalable()
        {
            // var zf1 = new ZenFunction<Seq<byte>, Seq<byte>>(l => l.Where(x => x != 100));
            // var example1 = zf1.Find((l, b) => b.Length() == 4, depth: 15);

            var zf2 = new ZenFunction<Bag<byte>, Bag<byte>>(l => l.Remove(100));
            var example2 = zf2.Find((l, b) => b.Size() == 4, depth: 100);
        }

        /// <summary>
        /// Test the Bag Create method works.
        /// </summary>
        [TestMethod]
        public void TestBagCreate()
        {
            var b1 = Symbolic<Bag<int>>(depth: 5);
            var b2 = Bag.Create<int>(1, 2, 3);
            var b3 = Bag.Create<int>(1, 2, 3, 4, 5);
            Assert.IsFalse((b1 == b2).Solve().IsSatisfiable());
            Assert.IsTrue((b1 == b3).Solve().IsSatisfiable());
        }

        /// <summary>
        /// Test the Bag add method works.
        /// </summary>
        [TestMethod]
        public void TestBagAddExample()
        {
            var zf = new ZenFunction<Bag<byte>, Bag<byte>>(l => l.Add(7));
            var example = zf.Find((l, b) => l.Size() == 3, depth: 5);
            Assert.IsTrue(example.HasValue);

            var output = zf.Evaluate(example.Value);
            Assert.IsTrue(output.ToArray().Contains((byte)7));
            Assert.AreEqual(4, output.ToArray().Length);
        }

        /// <summary>
        /// Test the Bag remove method works.
        /// </summary>
        [TestMethod]
        public void TestBagRemoveExample()
        {
            var zf = new ZenFunction<Bag<byte>, Bag<byte>>(l => l.Remove(7));
            var example = zf.Find((l, b) => l.Contains(7), depth: 5);
            Assert.IsTrue(example.HasValue);

            var output = zf.Evaluate(example.Value);
            Assert.IsFalse(output.ToArray().Contains((byte)7));
            Assert.IsTrue(output.ToArray().Length < example.Value.ToArray().Length);
        }

        /// <summary>
        /// Test the Bag contains method works.
        /// </summary>
        [TestMethod]
        public void TestBagContainsExample()
        {
            var zf = new ZenFunction<Bag<byte>, bool>(l => And(l.Contains(7), l.Contains(4)));
            var example = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example.HasValue);

            Assert.IsTrue(example.Value.ToArray().Contains((byte)4));
            Assert.IsTrue(example.Value.ToArray().Contains((byte)7));
        }

        /// <summary>
        /// Test that adding to the bag then contains the element.
        /// </summary>
        [TestMethod]
        public void TestBagAddContains()
        {
            RandomBytes(x => CheckValid<Bag<byte>>(l => l.Add(x).Contains(x)));
        }

        /// <summary>
        /// Test that removing from the bag then does not contain the element.
        /// </summary>
        [TestMethod]
        public void TestBagRemoveContains()
        {
            RandomBytes(x => CheckValid<Bag<byte>>(l => Not(l.Remove(x).Contains(x))));
        }
    }
}
