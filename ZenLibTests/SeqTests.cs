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
            var empty = new Seq<int>();
            var one = new Seq<int>(1);
            var two = new Seq<int>(2);
            var three = new Seq<int>(3);

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
        /// Test seq evaluation with empty.
        /// </summary>
        [TestMethod]
        public void TestSeqFindEmpty()
        {
            var zf = new ZenConstraint<Seq<int>>(s => s == new Seq<int>());
            var seq = zf.Find();
            Console.WriteLine(seq);
        }

        /* /// <summary>
        /// Test set equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestSeqEqualsHashcode()
        {
            var s1 = new Seq<int>().Add(11).Add(10);
            var s2 = new Set<int>().Add(10).Add(11);
            var s3 = new Set<int>().Add(1).Add(2);
            var s4 = new Set<int>();
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.Equals((object)s2));
            Assert.IsFalse(s1.Equals(10));
            Assert.IsFalse(s1 == s3);
            Assert.IsFalse(s1 == s4);
            Assert.IsTrue(s1 != s3);
            Assert.IsTrue(s1.GetHashCode() != s3.GetHashCode());
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
            Assert.AreEqual(0, new SetUnit().GetHashCode());
        } */
    }
}
