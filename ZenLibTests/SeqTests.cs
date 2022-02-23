// <copyright file="SeqTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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

        private static Seq<int> one = new Seq<int>(1);

        private static Seq<int> two = new Seq<int>(2);

        private static Seq<int> three = new Seq<int>(3);

        /// <summary>
        /// Test that some basic set equations hold.
        /// </summary>
        [TestMethod]
        public void TestSeqEquations()
        {
            CheckValid<Seq<int>, Seq<int>>((s1, s2) => Implies(Not(s1.Contains(s2)), s1.IndexOf(s2) < BigInteger.Zero), runBdds: false);
            CheckValid<Seq<int>, int>((s, x) => Implies(s.Contains(Seq.Unit(x)), s.IndexOf(Seq.Unit(x)) >= BigInteger.Zero), runBdds: false);
            Console.WriteLine(new Seq<int>().IndexOf(new Seq<int>()));
            CheckValid<Seq<int>>(s => s.IndexOf(Seq.Empty<int>()) == BigInteger.Zero, runBdds: false);
            CheckValid<Seq<int>, Seq<int>, Seq<int>>((s1, s2, s3) => s1.Concat(s2).Concat(s3) == s1.Concat(s2.Concat(s3)), runBdds: false);
            CheckValid<Seq<int>, Seq<int>>((s1, s2) => s1.Length() + s2.Length() == s1.Concat(s2).Length(), runBdds: false);
            CheckValid<Seq<int>, BigInteger>((s, i) => Implies(s == Seq.Empty<int>(), s.At(i) == Seq.Empty<int>()), runBdds: false);
            CheckValid<Seq<int>, BigInteger>((s, i) => Implies(And(i >= BigInteger.Zero, i < s.Length()), s.At(i) != Seq.Empty<int>()), runBdds: false);
            // CheckValid<Seq<int>, int>((s, x) => Implies(s == Seq.Unit(x), s.At(BigInteger.Zero).Value() == x), runBdds: false);
            CheckValid<Seq<int>, Seq<int>>((s1, s2) => Implies(s1.StartsWith(s2), s1.Contains(s2)), runBdds: false);
            CheckValid<Seq<int>, Seq<int>>((s1, s2) => Implies(s1.EndsWith(s2), s1.Contains(s2)), runBdds: false);
            CheckValid<Seq<int>, BigInteger, BigInteger>((s, o, l) => Implies(l < BigInteger.Zero, s.Slice(o, l) == Seq.Empty<int>()), runBdds: false);
            CheckValid<Seq<int>, BigInteger, BigInteger>((s, o, l) => Implies(o < BigInteger.Zero, s.Slice(o, l) == Seq.Empty<int>()), runBdds: false);
            CheckValid<Seq<int>, BigInteger, BigInteger>((s, o, l) => Implies(o > s.Length(), s.Slice(o, l) == Seq.Empty<int>()), runBdds: false);
            CheckValid<Seq<int>, BigInteger, BigInteger>((s, o, l) => s.Slice(o, l).Length() <= s.Length(), runBdds: false);
            CheckValid<Seq<int>, BigInteger, BigInteger>((s, o, l) => Implies(o < s.Length(), s.Slice(o, l).Length() <= s.Length() - o), runBdds: false);
            CheckValid<Seq<int>, Seq<int>>((s1, s2) => s1.ReplaceFirst(Seq.Empty<int>(), s2) == s2.Concat(s1), runBdds: false);
        }

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
            var zf = new ZenFunction<Seq<int>, BigInteger, Seq<int>>((s, i) => s.At(i));

            Assert.AreEqual(new Seq<int>(), zf.Evaluate(empty, new BigInteger(1)));
            Assert.AreEqual(new Seq<int>(1), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(0)));
            Assert.AreEqual(new Seq<int>(2), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(1)));
            Assert.AreEqual(new Seq<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(2)));
            Assert.AreEqual(new Seq<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(-1)));

            zf.Compile();
            Assert.AreEqual(new Seq<int>(), zf.Evaluate(empty, new BigInteger(1)));
            Assert.AreEqual(new Seq<int>(1), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(0)));
            Assert.AreEqual(new Seq<int>(2), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(1)));
            Assert.AreEqual(new Seq<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(2)));
            Assert.AreEqual(new Seq<int>(), zf.Evaluate(empty.Concat(one).Concat(two), new BigInteger(-1)));

            Assert.AreEqual(new Seq<int>(), empty.At(1));
            Assert.AreEqual(new Seq<int>(1), empty.Concat(one).Concat(two).At(0));
            Assert.AreEqual(new Seq<int>(2), empty.Concat(one).Concat(two).At(1));
            Assert.AreEqual(new Seq<int>(), empty.Concat(one).Concat(two).At(2));
            Assert.AreEqual(new Seq<int>(), empty.Concat(one).Concat(two).At(-1));
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
            var zf = new ZenFunction<Seq<int>, Seq<int>, bool>((s1, s2) => s1.StartsWith(s2));

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
            var zf = new ZenFunction<Seq<int>, Seq<int>, bool>((s1, s2) => s1.EndsWith(s2));

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
        /// Test seq evaluation with indexof.
        /// </summary>
        [TestMethod]
        public void TestSeqIndexOf()
        {
            var zf = new ZenFunction<Seq<int>, Seq<int>, BigInteger, BigInteger>((s1, s2, o) => s1.IndexOf(s2, o));

            Assert.AreEqual(new BigInteger(0), zf.Evaluate(one, one, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(0), zf.Evaluate(one, empty, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(0), zf.Evaluate(empty, empty, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(-1), zf.Evaluate(one.Concat(two), three, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(-1), zf.Evaluate(one.Concat(two), one, BigInteger.One));
            Assert.AreEqual(new BigInteger(2), zf.Evaluate(one.Concat(two).Concat(three), three, BigInteger.One));
            Assert.AreEqual(new BigInteger(1), zf.Evaluate(one.Concat(two).Concat(three), two.Concat(three), BigInteger.One));
            Assert.AreEqual(new BigInteger(1), zf.Evaluate(one.Concat(one), one, BigInteger.One));

            zf.Compile();
            Assert.AreEqual(new BigInteger(0), zf.Evaluate(one, one, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(0), zf.Evaluate(one, empty, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(0), zf.Evaluate(empty, empty, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(-1), zf.Evaluate(one.Concat(two), three, BigInteger.Zero));
            Assert.AreEqual(new BigInteger(-1), zf.Evaluate(one.Concat(two), one, BigInteger.One));
            Assert.AreEqual(new BigInteger(2), zf.Evaluate(one.Concat(two).Concat(three), three, BigInteger.One));
            Assert.AreEqual(new BigInteger(1), zf.Evaluate(one.Concat(two).Concat(three), two.Concat(three), BigInteger.One));
            Assert.AreEqual(new BigInteger(1), zf.Evaluate(one.Concat(one), one, BigInteger.One));

            Assert.AreEqual(0, one.IndexOf(one, 0));
            Assert.AreEqual(0, one.IndexOf(empty, 0));
            Assert.AreEqual(0, empty.IndexOf(empty, 0));
            Assert.AreEqual(-1, one.Concat(two).IndexOf(three, 0));
            Assert.AreEqual(-1, one.Concat(two).IndexOf(one, 1));
            Assert.AreEqual(2, one.Concat(two).Concat(three).IndexOf(three, 1));
            Assert.AreEqual(1, one.Concat(two).Concat(three).IndexOf(two.Concat(three), 1));
            Assert.AreEqual(1, one.Concat(one).IndexOf(one, 1));
        }

        /// <summary>
        /// Test seq evaluation with slice.
        /// </summary>
        [TestMethod]
        public void TestSeqSlice()
        {
            var zf = new ZenFunction<Seq<int>, BigInteger, BigInteger, Seq<int>>((s1, o, l) => s1.Slice(o, l));

            Assert.AreEqual(0, zf.Evaluate(empty, BigInteger.Zero, BigInteger.Zero).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.Zero, BigInteger.Zero).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.Zero, BigInteger.MinusOne).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.MinusOne, BigInteger.One).Length());
            Assert.AreEqual(1, zf.Evaluate(one, BigInteger.Zero, BigInteger.One).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.One, BigInteger.One).Length());
            Assert.AreEqual(1, zf.Evaluate(one.Concat(two), BigInteger.Zero, BigInteger.One).Length());
            Assert.AreEqual(2, zf.Evaluate(one.Concat(two), BigInteger.Zero, new BigInteger(2)).Length());
            Assert.AreEqual(1, zf.Evaluate(one.Concat(two), BigInteger.One, new BigInteger(2)).Length());

            zf.Compile();
            Assert.AreEqual(0, zf.Evaluate(empty, BigInteger.Zero, BigInteger.Zero).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.Zero, BigInteger.Zero).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.Zero, BigInteger.MinusOne).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.MinusOne, BigInteger.One).Length());
            Assert.AreEqual(1, zf.Evaluate(one, BigInteger.Zero, BigInteger.One).Length());
            Assert.AreEqual(0, zf.Evaluate(one, BigInteger.One, BigInteger.One).Length());
            Assert.AreEqual(1, zf.Evaluate(one.Concat(two), BigInteger.Zero, BigInteger.One).Length());
            Assert.AreEqual(2, zf.Evaluate(one.Concat(two), BigInteger.Zero, new BigInteger(2)).Length());
            Assert.AreEqual(1, zf.Evaluate(one.Concat(two), BigInteger.One, new BigInteger(2)).Length());

            Assert.AreEqual(0, empty.Slice(0, 0).Length());
            Assert.AreEqual(0, one.Slice(0, 0).Length());
            Assert.AreEqual(0, one.Slice(0, -1).Length());
            Assert.AreEqual(0, one.Slice(-1, 1).Length());
            Assert.AreEqual(1, one.Slice(0, 1).Length());
            Assert.AreEqual(0, one.Slice(1, 2).Length());
            Assert.AreEqual(1, one.Concat(two).Slice(0, 1).Length());
            Assert.AreEqual(2, one.Concat(two).Slice(0, 2).Length());
            Assert.AreEqual(1, one.Concat(two).Slice(1, 2).Length());
        }

        /// <summary>
        /// Test seq evaluation with replacefirst.
        /// </summary>
        [TestMethod]
        public void TestSeqReplaceFirst()
        {
            var zf = new ZenFunction<Seq<int>, Seq<int>, Seq<int>, Seq<int>>((s1, s2, s3) => s1.ReplaceFirst(s2, s3));

            Assert.AreEqual(empty, zf.Evaluate(empty, empty, empty));
            Assert.AreEqual(empty, zf.Evaluate(empty, one, two));
            Assert.AreEqual(two.Concat(one), zf.Evaluate(one, empty, two));
            Assert.AreEqual(one, zf.Evaluate(one.Concat(two), two, empty));
            Assert.AreEqual(two, zf.Evaluate(one.Concat(two), one, empty));
            Assert.AreEqual(three.Concat(two), zf.Evaluate(one.Concat(two), one, three));
            Assert.AreEqual(one.Concat(three), zf.Evaluate(one.Concat(two), two, three));
            Assert.AreEqual(three.Concat(two).Concat(one), zf.Evaluate(one.Concat(two).Concat(one), one, three));

            zf.Compile();
            Assert.AreEqual(empty, zf.Evaluate(empty, empty, empty));
            Assert.AreEqual(empty, zf.Evaluate(empty, one, two));
            Assert.AreEqual(two.Concat(one), zf.Evaluate(one, empty, two));
            Assert.AreEqual(one, zf.Evaluate(one.Concat(two), two, empty));
            Assert.AreEqual(two, zf.Evaluate(one.Concat(two), one, empty));
            Assert.AreEqual(three.Concat(two), zf.Evaluate(one.Concat(two), one, three));
            Assert.AreEqual(one.Concat(three), zf.Evaluate(one.Concat(two), two, three));
            Assert.AreEqual(three.Concat(two).Concat(one), zf.Evaluate(one.Concat(two).Concat(one), one, three));

            Assert.AreEqual(empty, empty.ReplaceFirst(empty, empty));
            Assert.AreEqual(empty, empty.ReplaceFirst(one, two));
            Assert.AreEqual(two.Concat(one), one.ReplaceFirst(empty, two));
            Assert.AreEqual(one, one.Concat(two).ReplaceFirst(two, empty));
            Assert.AreEqual(two, one.Concat(two).ReplaceFirst(one, empty));
            Assert.AreEqual(three.Concat(two), one.Concat(two).ReplaceFirst(one, three));
            Assert.AreEqual(one.Concat(three), one.Concat(two).ReplaceFirst(two, three));
            Assert.AreEqual(three.Concat(two).Concat(one), one.Concat(two).Concat(one).ReplaceFirst(one, three));
        }

        /// <summary>
        /// Test seq evaluation with replacefirst.
        /// </summary>
        [TestMethod]
        public void TestSeqRegex()
        {
            var r = Regex.Concat(Regex.Char<byte>(1), Regex.Char<byte>(2));

            var zf = new ZenFunction<Seq<byte>, bool>(s => s.MatchesRegex(r));

            Assert.AreEqual(false, zf.Evaluate(new Seq<byte>()));
            Assert.AreEqual(false, zf.Evaluate(new Seq<byte>(1)));
            Assert.AreEqual(true, zf.Evaluate(new Seq<byte>(1).Concat(new Seq<byte>(2))));
            Assert.AreEqual(false, zf.Evaluate(new Seq<byte>(1).Concat(new Seq<byte>(2)).Concat(new Seq<byte>(3))));

            zf.Compile();
            Assert.AreEqual(false, zf.Evaluate(new Seq<byte>()));
            Assert.AreEqual(false, zf.Evaluate(new Seq<byte>(1)));
            Assert.AreEqual(true, zf.Evaluate(new Seq<byte>(1).Concat(new Seq<byte>(2))));
            Assert.AreEqual(false, zf.Evaluate(new Seq<byte>(1).Concat(new Seq<byte>(2)).Concat(new Seq<byte>(3))));

            Assert.AreEqual(false, new Seq<byte>().MatchesRegex(r));
            Assert.AreEqual(false, new Seq<byte>(1).MatchesRegex(r));
            Assert.AreEqual(true, new Seq<byte>(1).Concat(new Seq<byte>(2)).MatchesRegex(r));
            Assert.AreEqual(false, new Seq<byte>(1).Concat(new Seq<byte>(2)).Concat(new Seq<byte>(3)).MatchesRegex(r));
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
            var result = new ZenConstraint<Seq<int>>(s => s.At(new BigInteger(2)) == new Seq<int>(10)).Find();
            Assert.AreEqual(new Seq<int>(10), result.Value.At(2));

            result = new ZenConstraint<Seq<int>>(s => s.At(new BigInteger(2)) == Seq.Empty<int>()).Find();
            Assert.IsTrue(result.Value.Length() < 3 || result.Value.At(2) == new Seq<int>());
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
            var result = new ZenConstraint<Seq<int>>(s => s.StartsWith(one.Concat(two))).Find();
            Assert.IsTrue(result.Value.HasPrefix(one.Concat(two)));
        }

        /// <summary>
        /// Test seq find with hassuffix.
        /// </summary>
        [TestMethod]
        public void TestSeqFindHasSuffix()
        {
            var result = new ZenConstraint<Seq<int>>(s => s.EndsWith(one.Concat(two))).Find();
            Assert.IsTrue(result.Value.HasSuffix(one.Concat(two)));
        }

        /// <summary>
        /// Test seq find with indexof.
        /// </summary>
        [TestMethod]
        public void TestSeqFindIndexOf1()
        {
            var result = new ZenConstraint<Seq<int>, Seq<int>, BigInteger>((s1, s2, i) => s1.IndexOf(s2, i) == new BigInteger(2)).Find();
            Assert.AreEqual(new BigInteger(2), result.Value.Item1.IndexOfBigInteger(result.Value.Item2, result.Value.Item3));
        }

        /// <summary>
        /// Test seq find with indexof.
        /// </summary>
        [TestMethod]
        public void TestSeqFindIndexOf2()
        {
            var result = new ZenConstraint<Seq<int>, Seq<int>>((s1, s2) => s1.IndexOf(s2) == new BigInteger(2)).Find();
            Assert.AreEqual(new BigInteger(2), result.Value.Item1.IndexOf(result.Value.Item2));
        }

        /// <summary>
        /// Test seq find with slice.
        /// </summary>
        [TestMethod]
        public void TestSeqFindSlice()
        {
            var result = new ZenConstraint<Seq<int>>(
                s => s.Slice(BigInteger.One, new BigInteger(2)) == Seq.Unit<int>(1).Concat(Seq.Unit<int>(2))).Find();

            Assert.AreEqual(new Seq<int>(1).Concat(new Seq<int>(2)), result.Value.Slice(1, 2));
        }

        /// <summary>
        /// Test seq find with replacefirst.
        /// </summary>
        [TestMethod]
        public void TestSeqFindReplaceFirst()
        {
            var result = new ZenConstraint<Seq<int>>(
                s => And(s.Length() == new BigInteger(2),
                     And(s.Contains(new Seq<int>(3)),
                         s.ReplaceFirst(new Seq<int>(3), new Seq<int>(4)) == Seq.Unit<int>(5).Concat(Seq.Unit<int>(4))))).Find();

            Console.WriteLine(result);
            Assert.AreEqual(new Seq<int>(5), result.Value.At(0));
            Assert.AreEqual(new Seq<int>(3), result.Value.At(1));
        }

        /// <summary>
        /// Test seq find with objects.
        /// </summary>
        [TestMethod]
        public void TestSeqFindWithObjects()
        {
            var result = new ZenConstraint<Seq<Pair<int, int>>>(s =>
            {
                var elt2 = s.At(new BigInteger(2));
                return elt2 == new Seq<Pair<int, int>>(new Pair<int, int> { Item1 = 4, Item2 = 2 });
            }).Find();

            Assert.IsTrue(result.Value.At(2).Values.Count > 0);
            Assert.AreEqual(4, result.Value.At(2).Values[0].Item1);
        }

        /// <summary>
        /// Test seq find with strings in sets.
        /// </summary>
        [TestMethod]
        public void TestSeqFindInSet1()
        {
            var result = new ZenConstraint<Set<string>>(s => s.Contains("ab" + "bc")).Find();

            Console.WriteLine(result);
            Assert.IsTrue(result.Value.Contains("abbc"));
        }

        /// <summary>
        /// Test seq find with seqs in sets.
        /// </summary>
        [TestMethod]
        public void TestSeqFindInSet2()
        {
            var result = new ZenConstraint<Set<Seq<int>>>(s => s.Contains(Seq.Empty<int>().Concat(one).Concat(two))).Find();

            Assert.IsTrue(result.Value.Contains(one.Concat(two)));
        }

        /// <summary>
        /// Test seq find with ite.
        /// </summary>
        [TestMethod]
        public void TestSeqFindWithIte()
        {
            var result = new ZenConstraint<Seq<int>, bool>((s, b) => If(b, Seq.Empty<int>(), Seq.Empty<int>() + new Seq<int>(10)).Contains(new Seq<int>(11))).Find();

            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Test seq find with regex.
        /// </summary>
        [TestMethod]
        public void TestSeqFindWithRegex()
        {
            Func<Regex<byte>, Option<Seq<byte>>> solve =
                (r) => new ZenConstraint<Seq<byte>>(s => s.MatchesRegex(r)).Find();

            var c1 = Regex.Char<byte>(1);
            var c2 = Regex.Char<byte>(2);
            var any = Regex.Dot<byte>();

            var r1 = Regex.Negation(Regex.Star(c1));
            var r2 = Regex.Star(Regex.Union(c1, c2));
            var r3 = Regex.Concat(c1, Regex.Star(c2));
            var r4 = Regex.Epsilon<byte>();
            var r5 = Regex.Empty<byte>();
            var r6 = Regex.Intersect(Regex.Star(c1), Regex.Concat(c1, c1));

            Assert.IsTrue(solve(r1).Value.MatchesRegex(r1));
            Assert.IsTrue(solve(r2).Value.MatchesRegex(r2));
            Assert.IsTrue(solve(r3).Value.MatchesRegex(r3));
            Assert.IsTrue(solve(r4).Value.MatchesRegex(r4));
            Assert.IsFalse(solve(r5).HasValue);
            Assert.IsTrue(solve(r6).Value.MatchesRegex(r6));
        }

        /// <summary>
        /// Test seq find with regex.
        /// </summary>
        [TestMethod]
        public void TestSeqFindWithRegexTypes()
        {
            var c1 = Regex.Char<byte>(1);
            var c2 = Regex.Char<short>(1);
            var c3 = Regex.Char<ushort>(1);
            var c4 = Regex.Char<int>(1);
            var c5 = Regex.Char<uint>(1);
            var c6 = Regex.Char<long>(1);
            var c7 = Regex.Char<ulong>(1);

            Assert.IsTrue(new ZenConstraint<Seq<byte>>(s => s.MatchesRegex(c1)).Find().Value.MatchesRegex(c1));
            Assert.IsTrue(new ZenConstraint<Seq<short>>(s => s.MatchesRegex(c2)).Find().Value.MatchesRegex(c2));
            Assert.IsTrue(new ZenConstraint<Seq<ushort>>(s => s.MatchesRegex(c3)).Find().Value.MatchesRegex(c3));
            Assert.IsTrue(new ZenConstraint<Seq<int>>(s => s.MatchesRegex(c4)).Find().Value.MatchesRegex(c4));
            Assert.IsTrue(new ZenConstraint<Seq<uint>>(s => s.MatchesRegex(c5)).Find().Value.MatchesRegex(c5));
            Assert.IsTrue(new ZenConstraint<Seq<long>>(s => s.MatchesRegex(c6)).Find().Value.MatchesRegex(c6));
            Assert.IsTrue(new ZenConstraint<Seq<ulong>>(s => s.MatchesRegex(c7)).Find().Value.MatchesRegex(c7));
        }

        /// <summary>
        /// Test seq with update and regex.
        /// </summary>
        [TestMethod]
        public void TestSeqFindUpdateRegex()
        {
            var r = Regex.Concat(Regex.Star(Regex.Char<byte>(1)), Regex.Char<byte>(2));
            var zf = new ZenFunction<Seq<byte>, Seq<byte>>(s =>
            {
                return If(s.MatchesRegex(r), s.Concat(Seq.Unit<byte>(3)), Seq.Empty<byte>());
            });

            var result = zf.Find((s1, s2) => s2.Contains(Seq.Unit<byte>(3)));
            Assert.IsTrue(result.HasValue);
            Assert.IsTrue(result.Value.ToList().Count >= 2);
            Assert.IsTrue(result.Value.ToList().Last() == (byte)2);
        }

        /// <summary>
        /// Test seq find with regex range exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSeqFindWithRegexRangeException()
        {
            Func<Regex<byte>, Option<Seq<byte>>> solve =
                (r) => new ZenConstraint<Seq<byte>>(s => s.MatchesRegex(r)).Find();

            solve(Regex.Range<byte>(1, 4));
        }

        /// <summary>
        /// Test seq equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestSeqEqualsHashcode()
        {
            var zero = new Seq<int>(0);

            var s1 = empty.Concat(zero).Concat(one);
            var s2 = zero.Concat(one);
            var s3 = zero.Concat(one).Concat(two);
            var s4 = empty.Concat(zero).Concat(two);
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.Equals((object)s2));
            Assert.IsFalse(s1.Equals(10));
            Assert.IsFalse(s1 == s3);
            Assert.IsTrue(s1 != s3);
            Assert.IsFalse(s1 == s4);
            Assert.IsTrue(s1.GetHashCode() != s3.GetHashCode());
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
        }

        /// <summary>
        /// Test seq tolist.
        /// </summary>
        [TestMethod]
        public void TestSeqToList()
        {
            var zero = new Seq<int>(0);

            Assert.AreEqual(0, empty.ToList().Count);
            Assert.AreEqual(2, zero.Concat(one).ToList().Count);
        }
    }
}
