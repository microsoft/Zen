﻿// <copyright file="FSeqTests.cs" company="Microsoft">
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
        public void TestFSeqToArray()
        {
            var a1 = new int[] { 1, 2, 3, 4 };
            var s = FSeq.FromRange(a1);
            var a2 = s.ToList().ToArray();
            Assert.AreEqual(a1.Length, a2.Length);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(a1[i], a2[i]);
            }
        }

        /// <summary>
        /// Test FSeq append.
        /// </summary>
        [TestMethod]
        public void TestFSeqAppendEval()
        {
            var zf = Zen.Function<FSeq<int>, FSeq<int>, FSeq<int>>((l1, l2) => l1.Append(l2));
            Assert.AreEqual(new FSeq<int>(1, 2, 3, 4), zf.Evaluate(new FSeq<int>(1, 2), new FSeq<int>(3, 4)));
            Assert.AreEqual(new FSeq<int>(1, 2), zf.Evaluate(new FSeq<int>(1, 2), new FSeq<int>()));
            Assert.AreEqual(new FSeq<int>(3, 4), zf.Evaluate(new FSeq<int>(), new FSeq<int>(3, 4)));
        }

        /// <summary>
        /// Test FSeq length.
        /// </summary>
        [TestMethod]
        public void TestFSeqLengthEval()
        {
            var zf = Zen.Function<FSeq<int>, ushort>(l => l.Length());
            Assert.AreEqual(2, zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(1, zf.Evaluate(new FSeq<int>(1)));
            Assert.AreEqual(0, zf.Evaluate(new FSeq<int>()));
        }

        /// <summary>
        /// Test FSeq select.
        /// </summary>
        [TestMethod]
        public void TestFSeqSelectEval()
        {
            var zf = Zen.Function<FSeq<int>, FSeq<int>>(l => l.Select(x => x + 1));
            Assert.AreEqual(new FSeq<int>(2, 3), zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(new FSeq<int>(2), zf.Evaluate(new FSeq<int>(1)));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>()));
        }

        /// <summary>
        /// Test FSeq where.
        /// </summary>
        [TestMethod]
        public void TestFSeqWhereEval()
        {
            var zf = Zen.Function<FSeq<int>, FSeq<int>>(l => l.Where(x => x > 2));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(new FSeq<int>(3), zf.Evaluate(new FSeq<int>(2, 3)));
            Assert.AreEqual(new FSeq<int>(3, 4), zf.Evaluate(new FSeq<int>(3, 4)));
        }

        /// <summary>
        /// Test FSeq isempty.
        /// </summary>
        [TestMethod]
        public void TestFSeqIsEmptyEval()
        {
            var zf = Zen.Function<FSeq<int>, bool>(l => l.IsEmpty());
            Assert.AreEqual(false, zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(false, zf.Evaluate(new FSeq<int>(2)));
            Assert.AreEqual(true, zf.Evaluate(new FSeq<int>()));
        }

        /// <summary>
        /// Test FSeq contains.
        /// </summary>
        [TestMethod]
        public void TestFSeqContainsEval()
        {
            var zf = Zen.Function<FSeq<int>, int, bool>((l, x) => l.Contains(x));
            Assert.AreEqual(true, zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(true, zf.Evaluate(new FSeq<int>(1, 2), 2));
            Assert.AreEqual(false, zf.Evaluate(new FSeq<int>(1, 2), 0));
        }

        /// <summary>
        /// Test FSeq reverse.
        /// </summary>
        [TestMethod]
        public void TestFSeqReverseEval()
        {
            var zf = Zen.Function<FSeq<int>, FSeq<int>>(l => l.Reverse());
            Assert.AreEqual(new FSeq<int>(2, 1), zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(new FSeq<int>(1), zf.Evaluate(new FSeq<int>(1)));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>()));
        }

        /// <summary>
        /// Test FSeq removeall.
        /// </summary>
        [TestMethod]
        public void TestFSeqRemoveAllEval()
        {
            var zf = Zen.Function<FSeq<int>, int, FSeq<int>>((l, x) => l.RemoveAll(x));
            Assert.AreEqual(new FSeq<int>(2), zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(new FSeq<int>(1), zf.Evaluate(new FSeq<int>(1, 2), 2));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>(1, 1, 1), 1));
            Assert.AreEqual(new FSeq<int>(1, 2, 1), zf.Evaluate(new FSeq<int>(1, 2, 1), 3));
        }

        /// <summary>
        /// Test FSeq removeall.
        /// </summary>
        [TestMethod]
        public void TestFSeqAddBackEval()
        {
            var zf = Zen.Function<FSeq<int>, int, FSeq<int>>((l, x) => l.AddBack(x));
            Assert.AreEqual(new FSeq<int>(1, 2, 1), zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(new FSeq<int>(2), zf.Evaluate(new FSeq<int>(), 2));
            Assert.AreEqual(new FSeq<int>(1, 3), zf.Evaluate(new FSeq<int>(1), 3));
        }

        /// <summary>
        /// Test FSeq find.
        /// </summary>
        [TestMethod]
        public void TestFSeqFindEval()
        {
            var zf = Zen.Function<FSeq<int>, Option<int>>(l => l.Find(x => x == 4));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(new FSeq<int>()));
            Assert.AreEqual(Option.Some(4), zf.Evaluate(new FSeq<int>(1, 4)));
        }

        /// <summary>
        /// Test FSeq head.
        /// </summary>
        [TestMethod]
        public void TestFSeqHeadEval()
        {
            var zf = Zen.Function<FSeq<int>, int>(l => l.Head());
            Assert.AreEqual(1, zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(0, zf.Evaluate(new FSeq<int>()));
            Assert.AreEqual(3, zf.Evaluate(new FSeq<int>(3, 4, 5)));
        }

        /// <summary>
        /// Test FSeq tail.
        /// </summary>
        [TestMethod]
        public void TestFSeqTailEval()
        {
            var zf = Zen.Function<FSeq<int>, FSeq<int>>(l => l.Tail());
            Assert.AreEqual(new FSeq<int>(2), zf.Evaluate(new FSeq<int>(1, 2)));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>()));
            Assert.AreEqual(new FSeq<int>(4, 5), zf.Evaluate(new FSeq<int>(3, 4, 5)));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>(3)));
        }

        /// <summary>
        /// Test FSeq indexof.
        /// </summary>
        [TestMethod]
        public void TestFSeqIndexOfEval()
        {
            var zf = Zen.Function<FSeq<int>, int, short>((l, x) => l.IndexOf(x));
            Assert.AreEqual(-1, zf.Evaluate(new FSeq<int>(1, 2), 3));
            Assert.AreEqual(0, zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(1, zf.Evaluate(new FSeq<int>(1, 2), 2));
        }

        /// <summary>
        /// Test FSeq duplicates.
        /// </summary>
        [TestMethod]
        public void TestFSeqDuplicatesEval()
        {
            var zf = Zen.Function<FSeq<int>, int, ushort>((l, x) => l.Duplicates(x));
            Assert.AreEqual(0, zf.Evaluate(new FSeq<int>(1, 2), 3));
            Assert.AreEqual(1, zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(2, zf.Evaluate(new FSeq<int>(1, 2, 1), 1));
        }

        /// <summary>
        /// Test FSeq take.
        /// </summary>
        [TestMethod]
        public void TestFSeqTakeEval()
        {
            var zf = Zen.Function<FSeq<int>, ushort, FSeq<int>>((l, x) => l.Take(x));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>(1, 2), 0));
            Assert.AreEqual(new FSeq<int>(1), zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(new FSeq<int>(1, 1), zf.Evaluate(new FSeq<int>(1, 1, 2), 2));
            Assert.AreEqual(new FSeq<int>(1, 1, 2), zf.Evaluate(new FSeq<int>(1, 1, 2), 3));
            Assert.AreEqual(new FSeq<int>(1, 1, 2), zf.Evaluate(new FSeq<int>(1, 1, 2), 4));
        }

        /// <summary>
        /// Test FSeq takewhile.
        /// </summary>
        [TestMethod]
        public void TestFSeqTakeWhileEval()
        {
            var zf = Zen.Function<FSeq<int>, FSeq<int>>(l => l.TakeWhile(x => x <= 2));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>(3, 4)));
            Assert.AreEqual(new FSeq<int>(1, 2), zf.Evaluate(new FSeq<int>(1, 2, 3)));
            Assert.AreEqual(new FSeq<int>(1, 2), zf.Evaluate(new FSeq<int>(1, 2)));
        }

        /// <summary>
        /// Test FSeq drop.
        /// </summary>
        [TestMethod]
        public void TestFSeqDropEval()
        {
            var zf = Zen.Function<FSeq<int>, ushort, FSeq<int>>((l, x) => l.Drop(x));
            Assert.AreEqual(new FSeq<int>(1, 2), zf.Evaluate(new FSeq<int>(1, 2), 0));
            Assert.AreEqual(new FSeq<int>(2), zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(new FSeq<int>(2), zf.Evaluate(new FSeq<int>(1, 1, 2), 2));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>(1, 1, 2), 3));
            Assert.AreEqual(new FSeq<int>(), zf.Evaluate(new FSeq<int>(1, 1, 2), 4));
        }

        /// <summary>
        /// Test FSeq at.
        /// </summary>
        [TestMethod]
        public void TestFSeqAtEval()
        {
            var zf = Zen.Function<FSeq<int>, ushort, Option<int>>((l, x) => l.At(x));
            Assert.AreEqual(Option.Some(1), zf.Evaluate(new FSeq<int>(1, 2), 0));
            Assert.AreEqual(Option.Some(2), zf.Evaluate(new FSeq<int>(1, 2), 1));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(new FSeq<int>(1, 2), 2));
        }

        /// <summary>
        /// Test FSeq set.
        /// </summary>
        [TestMethod]
        public void TestFSeqSetEval()
        {
            var zf = Zen.Function<FSeq<int>, ushort, int, FSeq<int>>((l, x, y) => l.Set(x, y));
            Assert.AreEqual(new FSeq<int>(3, 2), zf.Evaluate(new FSeq<int>(1, 2), 0, 3));
            Assert.AreEqual(new FSeq<int>(1, 3), zf.Evaluate(new FSeq<int>(1, 2), 1, 3));
            Assert.AreEqual(new FSeq<int>(1, 2), zf.Evaluate(new FSeq<int>(1, 2), 2, 3));
        }

        /// <summary>
        /// Test FSeq contains.
        /// </summary>
        [TestMethod]
        public void TestFSeqContainsSolve()
        {
            var l = Zen.Symbolic<FSeq<int>>();
            var sol = l.Contains(1).Solve();
            Assert.IsTrue(sol.IsSatisfiable());
            Assert.IsTrue(sol.Get(l).ToList().Contains(1));
        }

        /// <summary>
        /// Test FSeq All.
        /// </summary>
        [TestMethod]
        public void TestFSeqAllSolve()
        {
            var l = Zen.Symbolic<FSeq<int>>();
            var sol = l.All(x => x == 4).Solve();
            Assert.IsTrue(sol.IsSatisfiable());
            Assert.IsTrue(sol.Get(l).ToList().All(x => x == 4));
        }

        /// <summary>
        /// Test FSeq Drop.
        /// </summary>
        [TestMethod]
        public void TestFSeqDropSolve()
        {
            var l = Zen.Symbolic<FSeq<int>>();
            Console.WriteLine((l.Drop(1) == new FSeq<int>(1, 2)).Format());
            var sol = (l.Drop(1) == new FSeq<int>(1, 2)).Solve();
            Assert.IsTrue(sol.IsSatisfiable());
            Console.WriteLine(sol.Get(l));
            // Assert.IsTrue(sol.Get(l).ToList().All(x => x == 4));
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
            var f2 = new ZenFunction<FSeq<Object2>, bool>(l => l.Any(e => e.GetField<Object2, int>("Field1") == 7));
            var f3 = new ZenFunction<FSeq<Object2>, bool>(l => l.Any(e => e.GetField<Object2, int>("Field2") == 7));

            var input1 = f1.Find((i, o) => o);
            var input2 = f2.Find((i, o) => o);
            var input3 = f3.Find((i, o) => o);

            Assert.IsTrue(input1.Value.ToList().Where(x => x.Field1 == 7).Count() > 0);
            Assert.IsTrue(input2.Value.ToList().Where(x => x.Field1 == 7).Count() > 0);
            Assert.IsTrue(input3.Value.ToList().Where(x => x.Field2 == 7).Count() > 0);
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
                Implies(l.Contains(Constant<byte>(x)), l.IndexOf(x) >= 0)));
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

            /* CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() >= 2, l.Set(1, 7).At(1).Value() == 7));

            CheckValid<FSeq<byte>>(l =>
                Implies(l.Length() == 0, l.Set(1, 7) == l)); */
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
        /// Test Seq reverse.
        /// </summary>
        [TestMethod]
        public void TestSeqReverse()
        {
            RandomBytes(x => CheckAgreement<FSeq<int>>(l => l.Reverse().Contains(x)));
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
                l => Implies(l.Contains(x), l.RemoveAll(x).Duplicates(x) != l.Duplicates(x))));
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
            Assert.IsTrue(input.Value.ToList().Contains(1));
            Assert.IsTrue(input.Value.ToList().Contains(2));
        }

        /* /// <summary>
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
        } */

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
            Assert.IsTrue(s1.Equals((object)s3));
            Assert.IsTrue(!s2.Equals(s4));
            Assert.IsFalse(s1.Equals(0));
            Assert.IsTrue(s1.GetHashCode() == s3.GetHashCode());
        }
    }
}
