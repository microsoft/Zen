// <copyright file="FSeqTests.cs" company="Microsoft">
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
    /// Tests for the Zen sequence type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FSeqTests
    {
        /// <summary>
        /// Test that converting a sequence to and from an array works.
        /// </summary>
        [TestMethod]
        public void TestSeqToArray()
        {
            var a1 = new int[] { 1, 2, 3, 4 };
            var s = FSeq.FromRange(a1);
            var a2 = s.Values.ToArray();
            Assert.AreEqual(a1.Length, a2.Length);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(a1[i], a2[i]);
            }
        }

        /// <summary>
        /// Test List contains.
        /// </summary>
        [TestMethod]
        public void TestSeqContains()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => And(l.Contains(x), l.Contains(7))));
        }

        /// <summary>
        /// Test Seq contains.
        /// </summary>
        [TestMethod]
        public void TestSeqContainsVariable()
        {
            CheckAgreement<FSeq<byte>, byte>((l, x) => l.Contains(x), bddListSize: 2);
        }

        /// <summary>
        /// Test Seq all.
        /// </summary>
        [TestMethod]
        public void TestSeqAll()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.All(e => e == x)));
        }

        /// <summary>
        /// Test Seq any.
        /// </summary>
        [TestMethod]
        public void TestSeqAny()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.Any(e => e >= x)));
        }

        /// <summary>
        /// Test Seq any.
        /// </summary>
        [TestMethod]
        public void TestSeqAnyObjects()
        {
            var f1 = new ZenFunction<FSeq<Object2>, bool>(l => l.Any(e => e.GetField<Object2, int>("Field1") == 7));
            var f2 = new ZenFunction<FSeq<Object2>, bool>(l => l.Any(e => e.GetField<Object2, int>("Field1") == 7).Simplify());
            var f3 = new ZenFunction<FSeq<Object2>, bool>(l => l.Any(e => e.GetField<Object2, int>("Field2") == 7));

            var input1 = f1.Find((i, o) => o);
            var input2 = f2.Find((i, o) => o);
            var input3 = f3.Find((i, o) => o);

            Assert.IsTrue(input1.Value.Values.Where(x => x.Field1 == 7).Count() > 0);
            Assert.IsTrue(input2.Value.Values.Where(x => x.Field1 == 7).Count() > 0);
            Assert.IsTrue(input3.Value.Values.Where(x => x.Field2 == 7).Count() > 0);
        }

        /// <summary>
        /// Test Seq map.
        /// </summary>
        [TestMethod]
        public void TestSeqMap()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.Select(e => e + 1).Contains(x)));
        }

        /// <summary>
        /// Test Seq filter.
        /// </summary>
        [TestMethod]
        public void TestSeqFilter()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.Where(e => e < (x + 1)).Contains(x)));
        }

        /// <summary>
        /// Test Seq find implies contains.
        /// </summary>
        [TestMethod]
        public void TestSeqContainsFind()
        {
            RandomBytes(x => CheckValid<FSeq<byte>>(l =>
                Implies(l.Contains(Constant<byte>(x)), l.Find(v => v == x).IsSome())));
        }

        /// <summary>
        /// Test Seq index of.
        /// </summary>
        [TestMethod]
        public void TestSeqIndexOf()
        {
            RandomBytes(x => CheckValid<FSeq<byte>>(l =>
                Implies(l.Contains(Constant<byte>(x)), l.IndexOf(x).IsSome())));
        }

        /// <summary>
        /// Test Seq at.
        /// </summary>
        [TestMethod]
        public void TestSeqAt()
        {
            CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() >= 2, l.At(1).IsSome()));

            CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() == 0, Not(l.At(0).IsSome())));
        }

        /// <summary>
        /// Test Seq set.
        /// </summary>
        [TestMethod]
        public void TestSeqSet()
        {
            CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() >= 2, l.Set(1, 7).Contains(7)));

            CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() >= 2, l.Set(1, 7).At(1).Value() == 7));

            CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() == 0, l.Set(1, 7) == l));
        }

        /// <summary>
        /// Test Seq length.
        /// </summary>
        [TestMethod]
        public void TestSeqLength()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.AddFront(x).Length() > 0));
        }

        /// <summary>
        /// Test Seq length.
        /// </summary>
        [TestMethod]
        public void TestSeqLength2()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.AddBack(x).Length() > 0));
        }

        /// <summary>
        /// Test Seq sort.
        /// </summary>
        [TestMethod]
        public void TestSeqSort()
        {
            CheckAgreement<FSeq<int>>(l => l.Sort().IsSorted(), bddListSize: 1);
        }

        /// <summary>
        /// Test Seq reverse.
        /// </summary>
        [TestMethod]
        public void TestSeqReverse()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.Reverse().Contains(x)));
        }

        /// <summary>
        /// Test Seq intersperse.
        /// </summary>
        [TestMethod]
        public void TestSeqIntersperse()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.Intersperse(Constant<int>(x)).Contains(3)));
        }

        /// <summary>
        /// Test Seq split at.
        /// </summary>
        [TestMethod]
        public void TestSeqSplitAt()
        {
            CheckAgreement<FSeq<int>>(l => l.SplitAt(2).Item1().Length() == 1);
        }

        /// <summary>
        /// Test that Seq remove all does not contain that element.
        /// </summary>
        [TestMethod]
        public void TestSeqRemoveAllNotContains()
        {
            RandomBytes(x => CheckValid<FSeq<int>>(l => Not(l.RemoveAll(Constant<int>(x)).Contains(x))));
        }

        /// <summary>
        /// Test Seq remove all results in a smaller Seq.
        /// </summary>
        [TestMethod]
        public void TestSeqRemoveAllSmaller()
        {
            RandomBytes(x => CheckValid<FSeq<int>>(l => l.RemoveAll(x).Length() <= l.Length()));
        }

        /// <summary>
        /// Test Seq remove first.
        /// </summary>
        [TestMethod]
        public void TestSeqRemoveFirstCount()
        {
            RandomBytes(x => CheckValid<FSeq<int>>(
                l => Implies(l.Contains(x), l.RemoveFirst(x).Duplicates(x) != l.Duplicates(x))));
        }

        /// <summary>
        /// Test that Seq take results in a smaller Seq.
        /// </summary>
        [TestMethod]
        public void TestSeqTakeSmaller()
        {
            RandomBytes(x => CheckValid<FSeq<int>>(l => l.Take(x).Length() <= l.Length()));
        }

        /// <summary>
        /// Test Seq take.
        /// </summary>
        [TestMethod]
        public void TestSeqTakeExact()
        {
            RandomBytes(x =>
            {
                var len = (ushort)(x % 4);
                CheckValid<FSeq<int>>(l =>
                    If(len <= l.Length(), l.Take(len).Length() == len, true));
            });
        }

        /// <summary>
        /// Test that Seq drop results in a smaller Seq.
        /// </summary>
        [TestMethod]
        public void TestSeqDropSmaller()
        {
            RandomBytes(x => CheckValid<FSeq<int>>(l =>
                l.Drop(x).Length() <= l.Length()));
        }

        /// <summary>
        /// Test Seq drop.
        /// </summary>
        [TestMethod]
        public void TestSeqDropExact()
        {
            CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() >= 2, l.At(1).Value() == l.Drop(1).At(0).Value()));
        }

        /// <summary>
        /// Test Seq drop while.
        /// </summary>
        [TestMethod]
        public void TestDropWhile()
        {
            CheckValid<FSeq<byte>>(l =>
            {
                var dropped = l.DropWhile(b => b > 0);
                var first = dropped.At(0);
                return Implies(first.IsSome(), first.Value() == 0);
            });
        }

        /// <summary>
        /// Test Seq take while.
        /// </summary>
        [TestMethod]
        public void TestTakeWhile()
        {
            CheckValid<FSeq<byte>>(l =>
            {
                var dropped = l.TakeWhile(b => b > 0);
                var first = dropped.At(0);
                return Implies(first.IsSome(), first.Value() > 0);
            });
        }

        /// <summary>
        /// Test Seq append.
        /// </summary>
        [TestMethod]
        public void TestSeqAppend()
        {
            CheckValid<FSeq<byte>, FSeq<byte>>((l1, l2) => l1.Append(l2).Length() >= l1.Length());
            CheckValid<FSeq<byte>, FSeq<byte>>((l1, l2) => l1.Append(l2).Length() >= l2.Length());
        }

        /// <summary>
        /// Test Seq arbitrary.
        /// </summary>
        [TestMethod]
        public void TestSeqArbitrary()
        {
            var f = new ZenFunction<FSeq<int>, bool>(l => And(l.Contains(1), l.Contains(2)));
            var arbitrarySeq = FSeq.Empty<int>().AddFront(Arbitrary<int>()).AddFront(Arbitrary<int>());
            var input = f.Find((l, o) => o, arbitrarySeq);
            Assert.IsTrue(input.Value.Values.Contains(1));
            Assert.IsTrue(input.Value.Values.Contains(2));
        }

        /// <summary>
        /// Test Seq reverse and append.
        /// </summary>
        [TestMethod]
        public void TestSeqReverseAppend()
        {
            CheckValid<FSeq<byte>, FSeq<byte>>((l1, l2) =>
            {
                var l3 = l2.Reverse().Append(l1.Reverse()).Reverse();
                var l4 = l1.Append(l2);
                return Implies(Not(l3.IsEmpty()), l3.At(0).Value() == l4.At(0).Value());
            });
        }

        /// <summary>
        /// Test that size constraints are enforced correctly.
        /// </summary>
        [TestMethod]
        public void TestSeqSizeConstraints()
        {
            var zf = new ZenFunction<FSeq<byte>, bool>(l => l.Length() == 4);
            var example1 = zf.Find((l, b) => b, depth: 3);
            Assert.IsFalse(example1.HasValue);

            var zfNested = new ZenFunction<FSeq<FSeq<byte>>, bool>(l => l.Any(x => x.Length() == 4));
            var example2 = zfNested.Find((l, b) => b, depth: 3);
            Assert.IsFalse(example2.HasValue);
        }

        /// <summary>
        /// Test equality and hashcode for FSeq.
        /// </summary>
        [TestMethod]
        public void TestSeqEqualsHashCode()
        {
            var s1 = FSeq.FromRange(new List<int> { 1, 1, 2, 3, 5 });
            var s2 = FSeq.FromRange(new List<int> { 1, 2, 3, 5 });
            var s3 = new FSeq<int>().AddFront(5).AddFront(3).AddFront(2).AddFront(1).AddFront(1);
            var s4 = FSeq.FromRange(new List<int> { 1, 2, 3, 6 });

            Assert.IsTrue(s1 == s3);
            Assert.IsTrue(s1 != s2);
            Assert.IsTrue(s2 != s4);
            Assert.IsFalse(s1.Equals(0));
            Assert.IsTrue(s1.GetHashCode() == s3.GetHashCode());
        }
    }
}
