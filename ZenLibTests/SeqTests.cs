// <copyright file="SeqTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen seq type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SeqTests
    {
        private static Seq<int> empty = new Seq<int>();

        private static Seq<int> zero = new Seq<int>(0);

        private static Seq<int> one = new Seq<int>(1);

        private static Seq<int> two = new Seq<int>(2);

        private static Seq<int> three = new Seq<int>(3);

        /* /// <summary>
        /// Test that some basic set equations hold.
        /// </summary>
        [TestMethod]
        public void TestSetEquations()
        {
            CheckValid<Set<byte>, byte>((d, e) => d.Add(e).Delete(e) == d.Delete(e), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => d.Delete(e).Add(e) == d.Add(e), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => Implies(d.Contains(e), d.Add(e) == d), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => Implies(Not(d.Contains(e)), d.Delete(e) == d), runBdds: false);
            CheckValid<Set<UInt3>, Set<UInt3>, UInt3>((s1, s2, e) => And(s1.Contains(e), s2.Contains(e)) == s1.Intersect(s2).Contains(e), runBdds: false);
            CheckValid<Set<UInt3>, Set<UInt3>, UInt3>((s1, s2, e) => Or(s1.Contains(e), s2.Contains(e)) == s1.Union(s2).Contains(e), runBdds: false);
        } */

        /// <summary>
        /// Test seq evaluation with concat.
        /// </summary>
        [TestMethod]
        public void TestSeqConcat()
        {
            var zf = new ZenFunction<Seq<int>, Seq<int>, Seq<int>>((s1, s2) => s1.Concat(s2));

            Assert.AreEqual(empty, zf.Evaluate(empty, empty));
            Assert.AreEqual(one, zf.Evaluate(empty, one));
            Assert.AreEqual(one, zf.Evaluate(one, empty));
            Assert.AreEqual(one.Concat(two), zf.Evaluate(one, two));
            Assert.AreEqual(one.Concat(two).Concat(three), zf.Evaluate(one.Concat(two), three));

            zf.Compile();
            Assert.AreEqual(empty, zf.Evaluate(empty, empty));
            Assert.AreEqual(one, zf.Evaluate(empty, one));
            Assert.AreEqual(one, zf.Evaluate(one, empty));
            Assert.AreEqual(one.Concat(two), zf.Evaluate(one, two));
            Assert.AreEqual(one.Concat(two).Concat(three), zf.Evaluate(one.Concat(two), three));
        }

        /// <summary>
        /// Test seq evaluation with length.
        /// </summary>
        [TestMethod]
        public void TestSeqLength()
        {
            var zf = new ZenFunction<Seq<int>, BigInteger>(s => s.Length());

            Assert.AreEqual(new BigInteger(empty.Length()), zf.Evaluate(empty));
            Assert.AreEqual(new BigInteger(empty.Concat(one).Length()), zf.Evaluate(empty.Concat(one)));
            Assert.AreEqual(new BigInteger(empty.Concat(one).Concat(two).Length()), zf.Evaluate(empty.Concat(one).Concat(two)));

            zf.Compile();
            Assert.AreEqual(new BigInteger(empty.Length()), zf.Evaluate(empty));
            Assert.AreEqual(new BigInteger(empty.Concat(one).Length()), zf.Evaluate(empty.Concat(one)));
            Assert.AreEqual(new BigInteger(empty.Concat(one).Concat(two).Length()), zf.Evaluate(empty.Concat(one).Concat(two)));
        }

        /// <summary>
        /// Test seq evaluation with at.
        /// </summary>
        [TestMethod]
        public void TestSeqAt()
        {
            var zf = new ZenFunction<Seq<int>, BigInteger, Option<int>>((s, i) => s.At(i));

            Assert.AreEqual(Option.None<int>(), zf.Evaluate(empty, new BigInteger(1)));
            Assert.AreEqual(Option.Some(1), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(0)));
            Assert.AreEqual(Option.Some(2), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(1)));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(2)));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(-1)));

            zf.Compile();
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(empty, new BigInteger(1)));
            Assert.AreEqual(Option.Some(1), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(0)));
            Assert.AreEqual(Option.Some(2), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(1)));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(2)));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(-1)));

            Assert.AreEqual(Option.None<int>(), empty.At(1));
            Assert.AreEqual(Option.Some(1), empty.Concat(one).Concat(two).At(0));
            Assert.AreEqual(Option.Some(2), empty.Concat(one).Concat(two).At(1));
            Assert.AreEqual(Option.None<int>(), empty.Concat(one).Concat(two).At(2));
            Assert.AreEqual(Option.None<int>(), empty.Concat(one).Concat(two).At(-1));
        }

        /// <summary>
        /// Test seq evaluation with contains.
        /// </summary>
        [TestMethod]
        public void TestSeqContains()
        {
            var zf = new ZenFunction<Seq<int>, Seq<int>, bool>((s1, s2) => s1.Contains(s2));

            Assert.AreEqual(true, zf.Evaluate(empty, empty));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one), empty));
            Assert.AreEqual(false, zf.Evaluate(empty, empty.Concat(one)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two).Concat(three)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two)));
            Assert.AreEqual(false, zf.Evaluate(empty.Concat(one).Concat(two), empty.Concat(two).Concat(three)));

            zf.Compile();
            Assert.AreEqual(true, zf.Evaluate(empty, empty));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one), empty));
            Assert.AreEqual(false, zf.Evaluate(empty, empty.Concat(one)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two).Concat(three)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two)));
            Assert.AreEqual(false, zf.Evaluate(empty.Concat(one).Concat(two), empty.Concat(two).Concat(three)));

            Assert.AreEqual(true, empty.Contains(empty));
            Assert.AreEqual(true, empty.Concat(one).Contains(empty));
            Assert.AreEqual(false, empty.Contains(empty.Concat(one)));
            Assert.AreEqual(true, empty.Concat(one).Concat(two).Concat(three).Contains(empty.Concat(two).Concat(three)));
            Assert.AreEqual(true, empty.Concat(one).Concat(two).Concat(three).Contains(empty.Concat(two)));
            Assert.AreEqual(false, empty.Concat(one).Concat(two).Contains(empty.Concat(two).Concat(three)));
        }

        /// <summary>
        /// Test seq evaluation with hasprefix.
        /// </summary>
        [TestMethod]
        public void TestSeqHasPrefix()
        {
            var zf = new ZenFunction<Seq<int>, Seq<int>, bool>((s1, s2) => s1.HasPrefix(s2));

            Assert.AreEqual(true, zf.Evaluate(empty, empty));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one), empty));
            Assert.AreEqual(false, zf.Evaluate(empty, empty.Concat(one)));
            Assert.AreEqual(false, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two).Concat(three)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(one).Concat(two)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two), empty.Concat(one).Concat(two)));

            zf.Compile();
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one), empty));
            Assert.AreEqual(false, zf.Evaluate(empty, empty.Concat(one)));
            Assert.AreEqual(false, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two).Concat(three)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(one).Concat(two)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two), empty.Concat(one).Concat(two)));

            Assert.AreEqual(true, empty.Concat(one).HasPrefix(empty));
            Assert.AreEqual(false, empty.HasPrefix(empty.Concat(one)));
            Assert.AreEqual(false, empty.Concat(one).Concat(two).Concat(three).HasPrefix(empty.Concat(two).Concat(three)));
            Assert.AreEqual(true, empty.Concat(one).Concat(two).Concat(three).HasPrefix(empty.Concat(one).Concat(two)));
            Assert.AreEqual(true, empty.Concat(one).Concat(two).HasPrefix(empty.Concat(one).Concat(two)));
        }

        /// <summary>
        /// Test seq evaluation with hasprefix.
        /// </summary>
        [TestMethod]
        public void TestSeqHasSuffix()
        {
            var zf = new ZenFunction<Seq<int>, Seq<int>, bool>((s1, s2) => s1.HasSuffix(s2));

            Assert.AreEqual(true, zf.Evaluate(empty, empty));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one), empty));
            Assert.AreEqual(false, zf.Evaluate(empty, empty.Concat(one)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two).Concat(three)));
            Assert.AreEqual(false, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(one).Concat(two)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two), empty.Concat(one).Concat(two)));

            zf.Compile();
            Assert.AreEqual(true, zf.Evaluate(empty, empty));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one), empty));
            Assert.AreEqual(false, zf.Evaluate(empty, empty.Concat(one)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(two).Concat(three)));
            Assert.AreEqual(false, zf.Evaluate(empty.Concat(one).Concat(two).Concat(three), empty.Concat(one).Concat(two)));
            Assert.AreEqual(true, zf.Evaluate(empty.Concat(one).Concat(two), empty.Concat(one).Concat(two)));

            Assert.AreEqual(true, empty.Concat(one).HasSuffix(empty));
            Assert.AreEqual(false, empty.HasSuffix(empty.Concat(one)));
            Assert.AreEqual(true, empty.Concat(one).Concat(two).Concat(three).HasSuffix(empty.Concat(two).Concat(three)));
            Assert.AreEqual(false, empty.Concat(one).Concat(two).Concat(three).HasSuffix(empty.Concat(one).Concat(two)));
            Assert.AreEqual(true, empty.Concat(one).Concat(two).HasSuffix(empty.Concat(one).Concat(two)));
        }

        /// <summary>
        /// Test seq find with empty.
        /// </summary>
        [TestMethod]
        public void TestSeqFindEmpty()
        {
            var result = new ZenConstraint<Seq<int>>(s => s == new Seq<int>()).Find();
            Assert.AreEqual(new Seq<int>(), result.Value);
        }

        /// <summary>
        /// Test seq find with unit.
        /// </summary>
        [TestMethod]
        public void TestSeqFindUnit()
        {
            var result = new ZenConstraint<Seq<int>>(s => s == Seq.Unit<int>(0)).Find();
            Assert.AreEqual(new Seq<int>(0), result.Value);
        }

        /// <summary>
        /// Test seq find with concat.
        /// </summary>
        [TestMethod]
        public void TestSeqFindConcat()
        {
            var zero = new Seq<int>(0);
            var one = new Seq<int>(1);

            var result = new ZenConstraint<Seq<int>>(s => s == zero.Concat(one)).Find();
            Assert.AreEqual(zero.Concat(one), result.Value);
        }

        /// <summary>
        /// Test seq find with concat.
        /// </summary>
        [TestMethod]
        public void TestSeqFindLength()
        {
            var result = new ZenConstraint<Seq<int>>(s => s.Length() == new BigInteger(4)).Find();
            Assert.AreEqual(4, result.Value.Length());
        }

        /// <summary>
        /// Test seq find with at.
        /// </summary>
        [TestMethod]
        public void TestSeqFindAt()
        {
            var result = new ZenConstraint<Seq<int>>(s => s.At(new BigInteger(2)) == Option.Some(10)).Find();
            Assert.AreEqual(10, result.Value.At(2).Value);

            result = new ZenConstraint<Seq<int>>(s => s.At(new BigInteger(2)) == Option.None<int>()).Find();
            Assert.IsTrue(result.Value.Length() < 3 || result.Value.At(2).IsNone());
        }

        /// <summary>
        /// Test seq find with contains.
        /// </summary>
        [TestMethod]
        public void TestSeqFindContains()
        {
            var result = new ZenConstraint<Seq<int>>(s => s.Contains(one.Concat(two))).Find();
            Assert.IsTrue(result.Value.Contains(one.Concat(two)));
        }

        /// <summary>
        /// Test seq find with hasprefix.
        /// </summary>
        [TestMethod]
        public void TestSeqFindHasPrefix()
        {
            var result = new ZenConstraint<Seq<int>>(s => s.HasPrefix(one.Concat(two))).Find();
            Assert.IsTrue(result.Value.HasPrefix(one.Concat(two)));
        }

        /// <summary>
        /// Test seq find with hassuffix.
        /// </summary>
        [TestMethod]
        public void TestSeqFindHasSuffix()
        {
            var result = new ZenConstraint<Seq<int>>(s => s.HasSuffix(one.Concat(two))).Find();
            Assert.IsTrue(result.Value.HasSuffix(one.Concat(two)));
        }

        /// <summary>
        /// Test set equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestSeqEqualsHashcode()
        {
            var empty = new Seq<int>();
            var zero = new Seq<int>(0);
            var one = new Seq<int>(1);
            var two = new Seq<int>(2);

            var s1 = empty.Concat(zero).Concat(one);
            var s2 = zero.Concat(one);
            var s3 = zero.Concat(one).Concat(two);
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.Equals((object)s2));
            Assert.IsFalse(s1.Equals(10));
            Assert.IsFalse(s1 == s3);
            Assert.IsTrue(s1 != s3);
            Assert.IsTrue(s1.GetHashCode() != s3.GetHashCode());
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
        }
    }
}
