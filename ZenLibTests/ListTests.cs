// <copyright file="ListTests.cs" company="Microsoft">
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
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for the Zen list type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ListTests
    {
        /// <summary>
        /// Test List contains.
        /// </summary>
        [TestMethod]
        public void TestListContains()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => And(l.Contains(x), l.Contains(7))));
        }

        /// <summary>
        /// Test list contains.
        /// </summary>
        [TestMethod]
        public void TestListContainsVariable()
        {
            CheckAgreement<IList<byte>, byte>((l, x) => l.Contains(x), bddListSize: 2);
        }

        /// <summary>
        /// Test List all.
        /// </summary>
        [TestMethod]
        public void TestListAll()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.All(e => e == x)));
        }

        /// <summary>
        /// Test List any.
        /// </summary>
        [TestMethod]
        public void TestListAny()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.Any(e => e >= x)));
        }

        /// <summary>
        /// Test List any.
        /// </summary>
        [TestMethod]
        public void TestListAnyObjects()
        {
            var f = new ZenFunction<IList<Object2>, bool>(l => l.Any(e => e.GetField<Object2, int>("Field1") == 7));
            var input = f.Find((i, o) => o);
            Assert.IsTrue(input.Value.Where(x => x.Field1 == 7).Count() > 0);
        }

        /// <summary>
        /// Test List map.
        /// </summary>
        [TestMethod]
        public void TestListMap()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.Select(e => e + 1).Contains(x)));
        }

        /// <summary>
        /// Test List filter.
        /// </summary>
        [TestMethod]
        public void TestListFilter()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.Where(e => e < (x + 1)).Contains(x)));
        }

        /// <summary>
        /// Test List find implies contains.
        /// </summary>
        [TestMethod]
        public void TestListContainsFind()
        {
            RandomBytes(x => CheckValid<IList<byte>>(l =>
                Implies(l.Contains(Constant<byte>(x)), l.Find(v => v == x).HasValue())));
        }

        /// <summary>
        /// Test List index of.
        /// </summary>
        [TestMethod]
        public void TestListIndexOf()
        {
            RandomBytes(x => CheckValid<IList<byte>>(l =>
                Implies(l.Contains(Constant<byte>(x)), l.IndexOf(x).HasValue())));
        }

        /// <summary>
        /// Test List at.
        /// </summary>
        [TestMethod]
        public void TestListAt()
        {
            CheckValid<IList<byte>>(l =>
                Implies(l.Length() >= 2, l.At(1).HasValue()));

            CheckValid<IList<byte>>(l =>
                Implies(l.Length() == 0, Not(l.At(0).HasValue())));
        }

        /// <summary>
        /// Test List length.
        /// </summary>
        [TestMethod]
        public void TestListLength()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.AddFront(x).Length() > 0));
        }

        /// <summary>
        /// Test List length.
        /// </summary>
        [TestMethod]
        public void TestListLength2()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.AddBack(x).Length() > 0));
        }

        /// <summary>
        /// Test List sort.
        /// </summary>
        [TestMethod]
        public void TestListSort()
        {
            CheckAgreement<IList<int>>(l => l.Sort().IsSorted(), bddListSize: 1);
        }

        /// <summary>
        /// Test List reverse.
        /// </summary>
        [TestMethod]
        public void TestListReverse()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.Reverse().Contains(x)));
        }

        /// <summary>
        /// Test List intersperse.
        /// </summary>
        [TestMethod]
        public void TestListIntersperse()
        {
            RandomBytes(x => CheckAgreement<IList<int>>(l => l.Intersperse(Constant<int>(x)).Contains(3)));
        }

        /// <summary>
        /// Test List split at.
        /// </summary>
        [TestMethod]
        public void TestListSplitAt()
        {
            CheckAgreement<IList<int>>(l => l.SplitAt(2).Item1().Length() == 1);
        }

        /// <summary>
        /// Test that List remove all does not contain that element.
        /// </summary>
        [TestMethod]
        public void TestListRemoveAllNotContains()
        {
            RandomBytes(x => CheckValid<IList<int>>(l => Not(l.RemoveAll(Constant<int>(x)).Contains(x))));
        }

        /// <summary>
        /// Test List remove all results in a smaller list.
        /// </summary>
        [TestMethod]
        public void TestListRemoveAllSmaller()
        {
            RandomBytes(x => CheckValid<IList<int>>(l => l.RemoveAll(x).Length() <= l.Length()));
        }

        /// <summary>
        /// Test List remove first.
        /// </summary>
        [TestMethod]
        public void TestListRemoveFirstCount()
        {
            RandomBytes(x => CheckValid<IList<int>>(l => l.RemoveFirst(x).Duplicates(x) == l.Duplicates(x)));
        }

        /// <summary>
        /// Test that List take results in a smaller list.
        /// </summary>
        [TestMethod]
        public void TestListTakeSmaller()
        {
            RandomBytes(x => CheckValid<IList<int>>(l =>
                l.Take(x).Length() <= l.Length()));
        }

        /// <summary>
        /// Test List take.
        /// </summary>
        [TestMethod]
        public void TestListTakeExact()
        {
            RandomBytes(x =>
            {
                var len = (ushort)(x % 4);
                CheckValid<IList<int>>(l =>
                    If(len <= l.Length(), l.Take(len).Length() == len, true));
            });
        }

        /// <summary>
        /// Test that List drop results in a smaller list.
        /// </summary>
        [TestMethod]
        public void TestListDropSmaller()
        {
            RandomBytes(x => CheckValid<IList<int>>(l =>
                l.Drop(x).Length() <= l.Length()));
        }

        /// <summary>
        /// Test List drop.
        /// </summary>
        [TestMethod]
        public void TestListDropExact()
        {
            CheckValid<IList<byte>>(l =>
                Implies(l.Length() >= 2, l.At(1).Value() == l.Drop(1).At(0).Value()));
        }

        /// <summary>
        /// Test List drop while.
        /// </summary>
        [TestMethod]
        public void TestDropWhile()
        {
            CheckValid<IList<byte>>(l =>
            {
                var dropped = l.DropWhile(b => b > 0);
                var first = dropped.At(0);
                return Implies(first.HasValue(), first.Value() == 0);
            });
        }

        /// <summary>
        /// Test List take while.
        /// </summary>
        [TestMethod]
        public void TestTakeWhile()
        {
            CheckValid<IList<byte>>(l =>
            {
                var dropped = l.TakeWhile(b => b > 0);
                var first = dropped.At(0);
                return Implies(first.HasValue(), first.Value() > 0);
            });
        }

        /// <summary>
        /// Test List append.
        /// </summary>
        [TestMethod]
        public void TestListAppend()
        {
            CheckValid<IList<byte>, IList<byte>>((l1, l2) => l1.Append(l2).Length() >= l1.Length());
            CheckValid<IList<byte>, IList<byte>>((l1, l2) => l1.Append(l2).Length() >= l2.Length());
        }

        /// <summary>
        /// Test List arbitrary.
        /// </summary>
        [TestMethod]
        public void TestListArbitrary()
        {
            var f = new ZenFunction<IList<int>, bool>(l => And(l.Contains(1), l.Contains(2)));
            var input = f.Find((l, o) => o, ArbitraryList<int>(2));
            Assert.IsTrue(input.Value.Contains(1));
            Assert.IsTrue(input.Value.Contains(2));
        }

        /// <summary>
        /// Test List reverse and append.
        /// </summary>
        [TestMethod]
        public void TestListReverseAppend()
        {
            CheckValid<IList<byte>, IList<byte>>((l1, l2) =>
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
        public void TestListSizeConstraints()
        {
            var zf = new ZenFunction<IList<byte>, bool>(l => l.Length() == 4);
            var example1 = zf.Find((l, b) => b, listSize: 3);
            Assert.IsFalse(example1.HasValue);

            var zfNested = new ZenFunction<IList<IList<byte>>, bool>(l => l.Any(x => x.Length() == 4));
            var example2 = zfNested.Find((l, b) => b, listSize: 3);
            Assert.IsFalse(example2.HasValue);
        }
    }
}
