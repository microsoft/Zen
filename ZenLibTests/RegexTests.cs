// <copyright file="RegexTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for Regular expressions.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RegexTests
    {
        private static Regex<byte> zero = Regex.Char<byte>(0);

        private static Regex<byte> one = Regex.Char<byte>(1);

        private static Regex<byte> two = Regex.Char<byte>(2);

        private static Regex<byte> three = Regex.Char<byte>(3);

        /// <summary>
        /// Test that Regex simplifications are working.
        /// </summary>
        [TestMethod]
        public void TestRegexSimplifications()
        {
            var r = Regex.Char(1);
            var s = Regex.Char(2);
            var t = Regex.Char(3);
            var range = Regex.Range<byte>(0, 255);

            // unary simplifications
            Assert.AreEqual(Regex.Star(Regex.Star(r)), Regex.Star(r));
            Assert.AreEqual(Regex.Star(Regex.Epsilon<int>()), Regex.Epsilon<int>());
            Assert.AreEqual(Regex.Star(Regex.Empty<int>()), Regex.Epsilon<int>());
            Assert.AreEqual(Regex.Negation(Regex.Negation(r)), r);
            Assert.AreEqual(Regex.Negation(range), Regex.Empty<byte>());
            // concat simplifications
            Assert.AreEqual(Regex.Concat(Regex.Empty<int>(), r), Regex.Empty<int>());
            Assert.AreEqual(Regex.Empty<int>(), Regex.Concat(Regex.Empty<int>(), r));
            Assert.AreEqual(Regex.Concat(Regex.Epsilon<int>(), r), r);
            Assert.AreEqual(r, Regex.Concat(Regex.Epsilon<int>(), r));
            Assert.AreEqual(Regex.Concat(r, Regex.Concat(s, t)), Regex.Concat(Regex.Concat(r, s), t));
            // intersection simplifications
            Assert.AreEqual(Regex.Intersect(r, r), r);
            Assert.AreEqual(Regex.Intersect(s, r), Regex.Intersect(r, s));
            Assert.AreEqual(Regex.Intersect(Regex.Intersect(r, s), t), Regex.Intersect(r, Regex.Intersect(s, t)));
            Assert.AreEqual(Regex.Intersect(Regex.Empty<int>(), r), Regex.Empty<int>());
            Assert.AreEqual(Regex.Intersect(r, Regex.Empty<int>()), Regex.Empty<int>());
            Assert.AreEqual(Regex.Intersect(Regex.Negation(Regex.Empty<int>()), r), r);
            Assert.AreEqual(Regex.Intersect(r, Regex.Negation(Regex.Empty<int>())), r);
            // union simplifications
            Assert.AreEqual(Regex.Union(r, r), r);
            Assert.AreEqual(Regex.Union(r, Regex.Empty<int>()), r);
            Assert.AreEqual(Regex.Union(Regex.Empty<int>(), r), r);
            Assert.AreEqual(Regex.Union(r, Regex.Negation(Regex.Empty<int>())), Regex.Negation(Regex.Empty<int>()));
            Assert.AreEqual(Regex.Union(Regex.Negation(Regex.Empty<int>()), r), Regex.Negation(Regex.Empty<int>()));
            Assert.AreEqual(Regex.Union(s, r), Regex.Union(r, s));
            Assert.AreEqual(Regex.Union(Regex.Union(r, s), t), Regex.Union(r, Regex.Union(s, t)));
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch1()
        {
            var r = Regex.Concat(Regex.Concat(one, two), Regex.Star(three));

            Assert.IsFalse(Regex.IsMatch(r, new byte[] { }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 1 }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 1, 2 }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 1, 2, 3 }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 1, 2, 3, 3 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 1, 2, 3, 1 }));
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch2()
        {
            var r = Regex.Union(Regex.Concat(zero, one), Regex.Concat(two, three));

            Assert.IsFalse(Regex.IsMatch(r, new byte[] { }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 1 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 2 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 0, 2 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 0, 3 }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 0, 1 }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 2, 3 }));
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch3()
        {
            var r = Regex.Intersect(Regex.Star(Regex.Union(zero, one)), Regex.Star(Regex.Union(one, two)));

            Assert.IsTrue(Regex.IsMatch(r, new byte[] { }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 1 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 2 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 0 }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 1, 1, 1 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 1, 2 }));
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch4()
        {
            var r = Regex.Negation(Regex.Star(Regex.Range<byte>(0, 10)));

            Assert.IsFalse(Regex.IsMatch(r, new byte[] { }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 11, 12 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 0, 1, 2 }));
            Assert.IsFalse(Regex.IsMatch(r, new byte[] { 10, 10, 10, 10 }));
            Assert.IsTrue(Regex.IsMatch(r, new byte[] { 0, 1, 11 }));
        }

        /// <summary>
        /// Test that the character range implementation is working.
        /// </summary>
        [TestMethod]
        public void TestCharRangeOperations()
        {
            var r1 = new CharRange<byte>(1, 10);
            var r2 = new CharRange<byte>(0, 11);
            var r3 = new CharRange<byte>(100, byte.MaxValue);
            var r4 = new CharRange<byte>(0, byte.MaxValue);
            var r5 = new CharRange<byte>(10, 1);

            // test isempty
            Assert.IsFalse(r1.IsEmpty());
            Assert.IsFalse(r2.IsEmpty());
            Assert.IsFalse(r3.IsEmpty());
            Assert.IsFalse(r4.IsEmpty());
            Assert.IsTrue(r5.IsEmpty());

            // test isfull
            Assert.IsFalse(r1.IsFull());
            Assert.IsFalse(r2.IsFull());
            Assert.IsFalse(r3.IsFull());
            Assert.IsTrue(r4.IsFull());
            Assert.IsFalse(r5.IsFull());

            // test intersect
            Assert.AreEqual(r1, r1.Intersect(r2));
            Assert.AreEqual(r1, r1.Intersect(r4));
            Assert.IsTrue(r1.Intersect(r3).IsEmpty());
            Assert.AreEqual(r3, r3.Intersect(r4));
            Assert.AreEqual(r2, r2.Intersect(r2));

            // test complement
            Assert.AreEqual(0, r4.Complement().Length);
            Assert.AreEqual(1, r2.Complement().Length);
            Assert.AreEqual(new CharRange<byte>(12, 255), r2.Complement()[0]);
            Assert.AreEqual(1, r3.Complement().Length);
            Assert.AreEqual(new CharRange<byte>(0, 99), r3.Complement()[0]);
            Assert.AreEqual(2, r1.Complement().Length);
            Assert.AreEqual(new CharRange<byte>(0, 0), r1.Complement()[0]);
            Assert.AreEqual(new CharRange<byte>(11, 255), r1.Complement()[1]);

            // test contains
            Assert.IsTrue(r1.Contains(1));
            Assert.IsTrue(r1.Contains(10));
            Assert.IsTrue(r1.Contains(9));
            Assert.IsFalse(r1.Contains(0));
            Assert.IsFalse(r1.Contains(11));
            Assert.IsFalse(r1.Contains(255));
        }
    }
}
