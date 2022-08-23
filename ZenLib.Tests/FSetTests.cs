// <copyright file="FSetTests.cs" company="Microsoft">
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
    /// Tests for the Zen FSet type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FSetTests
    {
        private static FSet<int> b1 = new FSet<int>(1, 2, 3);

        private static FSet<int> b2 = new FSet<int>(0, 0, 1);

        private static FSet<int> b3 = new FSet<int>();

        /// <summary>
        /// Test set evaluation with add.
        /// </summary>
        [TestMethod]
        public void TestFSetAddEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, int, FSet<int>>((b, i) => b.Add(i));

            Assert.AreEqual(b1.Add(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Add(1), zf.Evaluate(b3, 1));

            zf.Compile();
            Assert.AreEqual(b1.Add(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Add(1), zf.Evaluate(b3, 1));
        }

        /// <summary>
        /// Test set evaluation with remove.
        /// </summary>
        [TestMethod]
        public void TestFSetRemoveEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, int, FSet<int>>((b, i) => b.Remove(i));

            Assert.AreEqual(b1.Remove(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Remove(1), zf.Evaluate(b3, 1));

            zf.Compile();
            Assert.AreEqual(b1.Remove(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Remove(1), zf.Evaluate(b3, 1));
        }

        /// <summary>
        /// Test set evaluation with where.
        /// </summary>
        [TestMethod]
        public void TestFSetWhereEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, FSet<int>>(b => b.Where(x => x < 3));

            Assert.AreEqual(b1.Where(x => x < 3), zf.Evaluate(b1));
            Assert.AreEqual(b3.Where(x => x < 3), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Where(x => x < 3), zf.Evaluate(b1));
            Assert.AreEqual(b3.Where(x => x < 3), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test set evaluation with select.
        /// </summary>
        [TestMethod]
        public void TestFSetSelectEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, FSet<int>>(b => b.Select(x => x + 1));

            Assert.AreEqual(b1.Select(x => x + 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Select(x => x + 1), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Select(x => x + 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Select(x => x + 1), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test set evaluation with isempty.
        /// </summary>
        [TestMethod]
        public void TestFSetIsEmptyEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, bool>(b => b.IsEmpty());

            Assert.AreEqual(b1.IsEmpty(), zf.Evaluate(b1));
            Assert.AreEqual(b3.IsEmpty(), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.IsEmpty(), zf.Evaluate(b1));
            Assert.AreEqual(b3.IsEmpty(), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test set evaluation with any.
        /// </summary>
        [TestMethod]
        public void TestFSetAnyEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, bool>(b => b.Any(i => i == 1));

            Assert.AreEqual(b1.Any(i => i == 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Any(i => i == 1), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Any(i => i == 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Any(i => i == 1), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test set evaluation with all.
        /// </summary>
        [TestMethod]
        public void TestFSetAllEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, bool>(b => b.All(i => i < 3));

            Assert.AreEqual(b1.All(i => i < 3), zf.Evaluate(b1));
            Assert.AreEqual(b2.All(i => i < 3), zf.Evaluate(b2));
            Assert.AreEqual(b3.All(i => i < 3), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.All(i => i < 3), zf.Evaluate(b1));
            Assert.AreEqual(b2.All(i => i < 3), zf.Evaluate(b2));
            Assert.AreEqual(b3.All(i => i < 3), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test set evaluation with size.
        /// </summary>
        [TestMethod]
        public void TestFSetSizeEvaluation()
        {
            var zf = new ZenFunction<FSet<int>, ushort>(b => b.Size());

            Assert.AreEqual(b1.Size(), (int)zf.Evaluate(b1));
            Assert.AreEqual(b3.Size(), (int)zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Size(), (int)zf.Evaluate(b1));
            Assert.AreEqual(b3.Size(), (int)zf.Evaluate(b3));
        }

        /// <summary>
        /// Test that converting a set to and from an array works.
        /// </summary>
        [TestMethod]
        public void TestFSetToArray()
        {
            var a1 = new int[] { 1, 2, 3, 4 };
            var b = new FSet<int>(a1);
            var a2 = b.ToSet().ToArray();
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
        public void TestFSetSizingAnnotation()
        {
            var zf = new ZenFunction<FSet<byte>, bool>(l => l.Size() == 5);
            var example1 = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example1.HasValue);
            Assert.AreEqual(5, example1.Value.Values.Values.Count);

            zf = new ZenFunction<FSet<byte>, bool>(l => l.Size() == 3);
            var example2 = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example2.HasValue);
            Assert.AreEqual(5, example1.Value.Values.Values.Count);
        }

        /// <summary>
        /// Test the set remove method is scalable due to lack of ordering,
        /// unlike with the sequence type that must preserve order.
        /// </summary>
        [TestMethod]
        public void TestFSetRemoveIsScalable()
        {
            var zf = new ZenFunction<FSet<byte>, FSet<byte>>(l => l.Remove(100));
            var example = zf.Find((l, b) => b.Contains(100), depth: 100);
            Assert.IsFalse(example.HasValue);
        }

        /// <summary>
        /// Test the set Create method works.
        /// </summary>
        [TestMethod]
        public void TestFSetCreate()
        {
            var b1 = Symbolic<FSet<int>>(depth: 5);
            var b2 = FSet.Create<int>(1, 2, 3);
            var b3 = FSet.Create<int>(1, 2, 3, 4, 5);
            var b4 = FSet.Create<int>(1, 2, 3, 4, 5, 6);

            var s1 = (b1 == b2).Solve();
            Assert.IsTrue(s1.IsSatisfiable());
            Assert.AreEqual(new FSet<int>(1, 2, 3), s1.Get(b1));

            var s2 = (b1 == b3).Solve();
            Assert.IsTrue(s2.IsSatisfiable());
            Assert.AreEqual(new FSet<int>(1, 2, 3, 4, 5), s2.Get(b1));

            var s3 = (b1 == b4).Solve();
            Assert.IsFalse(s3.IsSatisfiable());
        }

        /// <summary>
        /// Test the set Where method works.
        /// </summary>
        [TestMethod]
        public void TestFSetWhere()
        {
            var b = Symbolic<FSet<uint>>(depth: 5);
            var c = b.Where(i => i < 10).Size() == 4;
            var solution = c.Solve();
            var r = solution.Get(b);
            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsTrue(r.ToSet().Count == 4);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(r.ToSet().ToList()[i] < 10);
            }
        }

        /// <summary>
        /// Test the set Select method works.
        /// </summary>
        [TestMethod]
        public void TestFSetSelect()
        {
            var b = Symbolic<FSet<uint>>(depth: 5);
            var c = b.Select(i => If<char>(i < 10, 'a', 'b')).Where(x => x == 'a').Size() == 3;
            var solution = c.Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test the set All method works.
        /// </summary>
        [TestMethod]
        public void TestFSetAll()
        {
            var b = Symbolic<FSet<uint>>(depth: 5);
            var r = b.All(i => i == 5).Solve().Get(b);

            foreach (var element in r.ToSet())
            {
                Assert.IsTrue(element == 5);
            }
        }

        /// <summary>
        /// Test the set Any method works.
        /// </summary>
        [TestMethod]
        public void TestFSetAny()
        {
            var b = Symbolic<FSet<uint>>(depth: 5);
            var r = And(b.Any(i => i == 5), b.Contains(4)).Solve().Get(b);
            Assert.IsTrue(r.ToSet().ToList().IndexOf(5) >= 0);
            Assert.IsTrue(r.ToSet().ToList().IndexOf(4) >= 0);
        }

        /// <summary>
        /// Test the set IsEmpty method works.
        /// </summary>
        [TestMethod]
        public void TestFSetIsEmpty()
        {
            var b = Symbolic<FSet<uint>>(depth: 5);
            var r = b.IsEmpty().Solve().Get(b);
            Assert.IsTrue(r.IsEmpty());
        }

        /// <summary>
        /// Test the set add method works.
        /// </summary>
        [TestMethod]
        public void TestFSetAddExample()
        {
            var zf = new ZenFunction<FSet<byte>, FSet<byte>>(l => l.Add(7));
            var example = zf.Find((l, b) => l.Size() == 3);
            Assert.IsTrue(example.HasValue);

            var output = zf.Evaluate(example.Value);
            Assert.IsTrue(output.ToSet().ToArray().Contains((byte)7));
            Assert.AreEqual(4, output.ToSet().ToArray().Length);
        }

        /// <summary>
        /// Test the set remove method works.
        /// </summary>
        [TestMethod]
        public void TestFSetRemoveExample()
        {
            var zf = new ZenFunction<FSet<byte>, FSet<byte>>(l => l.Remove(7));
            var example = zf.Find((l, b) => l.Contains(7), depth: 5);
            Assert.IsTrue(example.HasValue);

            var output = zf.Evaluate(example.Value);
            Assert.IsFalse(output.ToSet().ToArray().Contains((byte)7));
            Assert.IsTrue(output.ToSet().ToArray().Length < example.Value.ToSet().ToArray().Length);
        }

        /// <summary>
        /// Test the set contains method works.
        /// </summary>
        [TestMethod]
        public void TestFSetContainsExample()
        {
            var zf = new ZenFunction<FSet<byte>, bool>(l => And(l.Contains(7), l.Contains(4)));
            var example = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example.HasValue);

            Assert.IsTrue(example.Value.ToSet().ToArray().Contains((byte)4));
            Assert.IsTrue(example.Value.ToSet().ToArray().Contains((byte)7));
        }

        /// <summary>
        /// Test seq equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestFSetEqualsHashcode()
        {
            var b1 = new FSet<int>(1, 1, 2, 3, 5);
            var b2 = new FSet<int>(1, 2, 3, 5);
            var b3 = new FSet<int>().Add(5).Add(3).Add(2).Add(1).Add(1);
            var b4 = new FSet<int>(1, 2, 3, 6);

            Assert.IsTrue(b1 == b3);
            Assert.IsFalse(b1 != b2);
            Assert.IsTrue(b2 != b4);
            Assert.IsFalse(b1.Equals(0));
            Assert.IsTrue(b1.GetHashCode() == b3.GetHashCode());
        }

        /// <summary>
        /// Test that adding to the set then contains the element.
        /// </summary>
        [TestMethod]
        public void TestFSetAddContains()
        {
            RandomBytes(x => CheckValid<FSet<byte>>(l => l.Add(x).Contains(x)));
        }

        /// <summary>
        /// Test that removing from the set then does not contain the element.
        /// </summary>
        [TestMethod]
        public void TestFSetRemoveContains()
        {
            RandomBytes(x => CheckValid<FSet<byte>>(l => Not(l.Remove(x).Contains(x))));
        }
    }
}