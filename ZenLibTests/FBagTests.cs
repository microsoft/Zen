// <copyright file="FBagTests.cs" company="Microsoft">
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
    public class FBagTests
    {
        private static FBag<int> b1 = FBag.FromRange(new List<int> { 1, 2, 3 });

        private static FBag<int> b2 = FBag.FromRange(new List<int> { 0, 0, 1 });

        private static FBag<int> b3 = FBag.FromRange(new List<int> { });

        /// <summary>
        /// Test bag evaluation with add.
        /// </summary>
        [TestMethod]
        public void TestBagAddEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, int, FBag<int>>((b, i) => b.Add(i));

            Assert.AreEqual(b1.Add(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Add(1), zf.Evaluate(b3, 1));

            zf.Compile();
            Assert.AreEqual(b1.Add(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Add(1), zf.Evaluate(b3, 1));
        }

        /// <summary>
        /// Test bag evaluation with remove.
        /// </summary>
        [TestMethod]
        public void TestBagRemoveEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, int, FBag<int>>((b, i) => b.Remove(i));

            Assert.AreEqual(b1.Remove(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Remove(1), zf.Evaluate(b3, 1));

            zf.Compile();
            Assert.AreEqual(b1.Remove(1), zf.Evaluate(b1, 1));
            Assert.AreEqual(b3.Remove(1), zf.Evaluate(b3, 1));
        }

        /// <summary>
        /// Test bag evaluation with where.
        /// </summary>
        [TestMethod]
        public void TestBagWhereEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, FBag<int>>(b => b.Where(x => x < 3));

            Assert.AreEqual(b1.Where(x => x < 3), zf.Evaluate(b1));
            Assert.AreEqual(b3.Where(x => x < 3), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Where(x => x < 3), zf.Evaluate(b1));
            Assert.AreEqual(b3.Where(x => x < 3), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test bag evaluation with select.
        /// </summary>
        [TestMethod]
        public void TestBagSelectEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, FBag<int>>(b => b.Select(x => x + 1));

            Assert.AreEqual(b1.Select(x => x + 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Select(x => x + 1), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Select(x => x + 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Select(x => x + 1), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test bag evaluation with isempty.
        /// </summary>
        [TestMethod]
        public void TestBagIsEmptyEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, bool>(b => b.IsEmpty());

            Assert.AreEqual(b1.IsEmpty(), zf.Evaluate(b1));
            Assert.AreEqual(b3.IsEmpty(), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.IsEmpty(), zf.Evaluate(b1));
            Assert.AreEqual(b3.IsEmpty(), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test bag evaluation with any.
        /// </summary>
        [TestMethod]
        public void TestBagAnyEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, bool>(b => b.Any(i => i == 1));

            Assert.AreEqual(b1.Any(i => i == 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Any(i => i == 1), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Any(i => i == 1), zf.Evaluate(b1));
            Assert.AreEqual(b3.Any(i => i == 1), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test bag evaluation with all.
        /// </summary>
        [TestMethod]
        public void TestBagAllEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, bool>(b => b.All(i => i < 3));

            Assert.AreEqual(b1.All(i => i < 3), zf.Evaluate(b1));
            Assert.AreEqual(b2.All(i => i < 3), zf.Evaluate(b2));
            Assert.AreEqual(b3.All(i => i < 3), zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.All(i => i < 3), zf.Evaluate(b1));
            Assert.AreEqual(b2.All(i => i < 3), zf.Evaluate(b2));
            Assert.AreEqual(b3.All(i => i < 3), zf.Evaluate(b3));
        }

        /// <summary>
        /// Test bag evaluation with size.
        /// </summary>
        [TestMethod]
        public void TestBagSizeEvaluation()
        {
            var zf = new ZenFunction<FBag<int>, ushort>(b => b.Size());

            Assert.AreEqual(b1.Size(), (int)zf.Evaluate(b1));
            Assert.AreEqual(b3.Size(), (int)zf.Evaluate(b3));

            zf.Compile();
            Assert.AreEqual(b1.Size(), (int)zf.Evaluate(b1));
            Assert.AreEqual(b3.Size(), (int)zf.Evaluate(b3));
        }

        /// <summary>
        /// Test that converting a bag to and from an array works.
        /// </summary>
        [TestMethod]
        public void TestBagToArray()
        {
            var a1 = new int[] { 1, 2, 3, 4 };
            var b = FBag.FromRange(a1);
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
            var zf = new ZenFunction<FBag<byte>, bool>(l => l.Size() == 5);
            var example1 = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example1.HasValue);
            Assert.AreEqual(5, example1.Value.Values.Values.Count);

            zf = new ZenFunction<FBag<byte>, bool>(l => l.Size() == 3);
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
            var zf = new ZenFunction<FBag<byte>, FBag<byte>>(l => l.Remove(100));
            var example = zf.Find((l, b) => b.Contains(100), depth: 100);
            Assert.IsFalse(example.HasValue);
        }

        /// <summary>
        /// Test the Bag add if space method is scalable.
        /// </summary>
        [TestMethod]
        public void TestBagAddIfSpaceIsScalable()
        {
            var zf2 = new ZenFunction<FBag<byte>, FBag<byte>>(l => l.AddIfSpace(100));
            var example2 = zf2.Find((l, b) => b.Size() == 4, depth: 50);
        }

        /// <summary>
        /// Test the Bag Create method works.
        /// </summary>
        [TestMethod]
        public void TestBagCreate()
        {
            var b1 = Symbolic<FBag<int>>(depth: 5);
            var b2 = FBag.Create<int>(1, 2, 3);
            var b3 = FBag.Create<int>(1, 2, 3, 4, 5);
            Assert.IsFalse((b1 == b2).Solve().IsSatisfiable());
            Assert.IsTrue((b1 == b3).Solve().IsSatisfiable());
        }

        /// <summary>
        /// Test the Bag Where method works.
        /// </summary>
        [TestMethod]
        public void TestBagWhere()
        {
            var b = Symbolic<FBag<uint>>(depth: 5);
            var c = b.Where(i => i < 10).Size() == 4;
            var solution = c.Solve();
            var r = solution.Get(b);
            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsTrue(r.ToList().Count == 4);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(r.ToList()[i] < 10);
            }
        }

        /// <summary>
        /// Test the Bag Select method works.
        /// </summary>
        [TestMethod]
        public void TestBagSelect()
        {
            var b = Symbolic<FBag<uint>>(depth: 5);
            var c = b.Select(i => If<char>(i < 10, 'a', 'b')).Where(x => x == 'a').Size() == 3;
            var solution = c.Solve();
            var r = solution.Get(b);

            Assert.IsTrue(solution.IsSatisfiable());
            int count = 0;
            foreach (var elt in r.ToList())
            {
                if (elt < 10)
                {
                    count++;
                }
            }

            Assert.IsTrue(count == 3);
        }

        /// <summary>
        /// Test the Bag All method works.
        /// </summary>
        [TestMethod]
        public void TestBagAll()
        {
            var b = Symbolic<FBag<uint>>(depth: 5);
            var r = b.All(i => i == 5).Solve().Get(b);

            foreach (var element in r.ToList())
            {
                Assert.IsTrue(element == 5);
            }
        }

        /// <summary>
        /// Test the Bag Any method works.
        /// </summary>
        [TestMethod]
        public void TestBagAny()
        {
            var b = Symbolic<FBag<uint>>(depth: 5);
            var r = And(b.Any(i => i == 5), b.Contains(4)).Solve().Get(b);
            Assert.IsTrue(r.ToList().IndexOf(5) >= 0);
            Assert.IsTrue(r.ToList().IndexOf(4) >= 0);
        }

        /// <summary>
        /// Test the Bag IsEmpty method works.
        /// </summary>
        [TestMethod]
        public void TestBagIsEmpty()
        {
            var b = Symbolic<FBag<uint>>(depth: 5);
            var r = b.IsEmpty().Solve().Get(b);
            Assert.IsTrue(r.IsEmpty());
        }

        /// <summary>
        /// Test the Bag add method works.
        /// </summary>
        [TestMethod]
        public void TestBagAddExample()
        {
            var zf = new ZenFunction<FBag<byte>, FBag<byte>>(l => l.Add(7));
            var example = zf.Find((l, b) => l.Size() == 3, depth: 5);
            Assert.IsTrue(example.HasValue);

            var output = zf.Evaluate(example.Value);
            Assert.IsTrue(output.ToArray().Contains((byte)7));
            Assert.AreEqual(4, output.ToArray().Length);
        }

        /// <summary>
        /// Test the Bag add if space method works.
        /// </summary>
        [TestMethod]
        public void TestBagAddIfSpace()
        {
            var b1 = Symbolic<FBag<int>>();
            var b2 = Symbolic<FBag<int>>();
            var b3 = b1.AddIfSpace(5);

            var sol = (b2 == b3).Solve();
            var r1 = sol.Get(b1);
            var r2 = sol.Get(b3);
            var set1 = r1.ToSet();
            var set2 = r2.ToSet();
            Assert.IsTrue(set1.Count + 1 == set2.Count);
        }

        /// <summary>
        /// Test the ToList and ToSet methods.
        /// </summary>
        [TestMethod]
        public void TestBagToListToSet()
        {
            var b = Symbolic<FBag<int>>();
            var solution = And(b.Contains(1), b.Size() == 2, b.Remove(1).Size() == 0).Solve();
            var r = solution.Get(b);
            Assert.IsTrue(r.ToSet().Count == 1);
            Assert.IsTrue(r.ToSet().Contains(1));
            Assert.IsTrue(r.ToList().Count == 2);
            Assert.IsTrue(r.ToList()[0] == 1);
            Assert.IsTrue(r.ToList()[1] == 1);
        }

        /// <summary>
        /// Test the Bag remove method works.
        /// </summary>
        [TestMethod]
        public void TestBagRemoveExample()
        {
            var zf = new ZenFunction<FBag<byte>, FBag<byte>>(l => l.Remove(7));
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
            var zf = new ZenFunction<FBag<byte>, bool>(l => And(l.Contains(7), l.Contains(4)));
            var example = zf.Find((l, b) => b, depth: 5);
            Assert.IsTrue(example.HasValue);

            Assert.IsTrue(example.Value.ToArray().Contains((byte)4));
            Assert.IsTrue(example.Value.ToArray().Contains((byte)7));
        }

        /// <summary>
        /// Test seq equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestBagEqualsHashcode()
        {
            var b1 = FBag.FromRange(new List<int> { 1, 1, 2, 3, 5 });
            var b2 = FBag.FromRange(new List<int> { 1, 2, 3, 5 });
            var b3 = new FBag<int>().Add(5).Add(3).Add(2).Add(1).Add(1);
            var b4 = FBag.FromRange(new List<int> { 1, 2, 3, 6 });

            Assert.IsTrue(b1 == b3);
            Assert.IsTrue(b1 != b2);
            Assert.IsTrue(b2 != b4);
            Assert.IsFalse(b1.Equals(0));
            Assert.IsTrue(b1.GetHashCode() == b3.GetHashCode());
        }

        /// <summary>
        /// Test that adding to the bag then contains the element.
        /// </summary>
        [TestMethod]
        public void TestBagAddContains()
        {
            RandomBytes(x => CheckValid<FBag<byte>>(l => l.Add(x).Contains(x)));
        }

        /// <summary>
        /// Test that adding to the bag works when there is space.
        /// </summary>
        [TestMethod]
        public void TestBagAddIfSpaceContains1()
        {
            RandomBytes(x => CheckValid<FBag<byte>>(l => Implies(l.Size() == 0, l.AddIfSpace(x).Contains(x))));
        }

        /// <summary>
        /// Test that adding to the bag works when there is no space.
        /// </summary>
        [TestMethod]
        public void TestBagAddIfSpaceNotContains1()
        {
            RandomBytes(x => CheckValid<FBag<byte>>(l => Implies(l.Size() >= 5, l.AddIfSpace(x) == l)));
        }

        /// <summary>
        /// Test that removing from the bag then does not contain the element.
        /// </summary>
        [TestMethod]
        public void TestBagRemoveContains()
        {
            RandomBytes(x => CheckValid<FBag<byte>>(l => Not(l.Remove(x).Contains(x))));
        }
    }
}
