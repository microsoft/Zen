// <copyright file="RegexTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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

            // equals, hashcode
            Assert.IsTrue(r1.Equals(r1));
            Assert.IsFalse(r1.Equals(10));
            Assert.IsFalse(r1.Equals(r2));
            Assert.IsFalse(r2.Equals(r4));
        }

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

            // range simplifications
            Assert.AreEqual(Regex.Range<byte>(5, 4), Regex.Empty<byte>());
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
            Assert.AreEqual(Regex.Concat(r, Regex.Epsilon<int>()), r);
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

            CheckIsNotMatch(r, new byte[] { });
            CheckIsNotMatch(r, new byte[] { 1 });
            CheckIsMatch(r, new byte[] { 1, 2 });
            CheckIsMatch(r, new byte[] { 1, 2, 3 });
            CheckIsMatch(r, new byte[] { 1, 2, 3, 3 });
            CheckIsNotMatch(r, new byte[] { 1, 2, 3, 1 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch2()
        {
            var r = Regex.Union(Regex.Concat(zero, one), Regex.Concat(two, three));

            CheckIsNotMatch(r, new byte[] { });
            CheckIsNotMatch(r, new byte[] { 1 });
            CheckIsNotMatch(r, new byte[] { 2 });
            CheckIsNotMatch(r, new byte[] { 0, 2 });
            CheckIsNotMatch(r, new byte[] { 0, 3 });
            CheckIsMatch(r, new byte[] { 0, 1 });
            CheckIsMatch(r, new byte[] { 2, 3 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch3()
        {
            var r = Regex.Intersect(Regex.Star(Regex.Union(zero, one)), Regex.Star(Regex.Union(one, two)));

            CheckIsMatch(r, new byte[] { });
            CheckIsMatch(r, new byte[] { 1 });
            CheckIsNotMatch(r, new byte[] { 2 });
            CheckIsNotMatch(r, new byte[] { 0 });
            CheckIsMatch(r, new byte[] { 1, 1, 1 });
            CheckIsNotMatch(r, new byte[] { 1, 2 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch4()
        {
            var r = Regex.Negation(Regex.Star(Regex.Range<byte>(0, 10)));

            CheckIsNotMatch(r, new byte[] { });
            CheckIsMatch(r, new byte[] { 11, 12 });
            CheckIsNotMatch(r, new byte[] { 0, 1, 2 });
            CheckIsNotMatch(r, new byte[] { 10, 10, 10, 10 });
            CheckIsMatch(r, new byte[] { 0, 1, 11 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch5()
        {
            var r = Regex.Union(Regex.Epsilon<byte>(), Regex.Dot<byte>());

            CheckIsMatch(r, new byte[] { });
            CheckIsMatch(r, new byte[] { 1 });
            CheckIsMatch(r, new byte[] { 3 });
            CheckIsMatch(r, new byte[] { 255 });
            CheckIsNotMatch(r, new byte[] { 1, 1 });
            CheckIsNotMatch(r, new byte[] { 0, 0, 0 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch6()
        {
            var r1 = Regex.Union(one, two);
            var r = Regex.Concat(r1, Regex.Star(r1));

            CheckIsNotMatch(r, new byte[] { });
            CheckIsMatch(r, new byte[] { 1 });
            CheckIsMatch(r, new byte[] { 2 });
            CheckIsMatch(r, new byte[] { 1, 2, 1, 2 });
            CheckIsNotMatch(r, new byte[] { 1, 2, 3 });
            CheckIsNotMatch(r, new byte[] { 0, 0, 0 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch7()
        {
            var r = Regex.Concat(Regex.Star(one), two);

            CheckIsNotMatch(r, new byte[] { });
            CheckIsNotMatch(r, new byte[] { 1 });
            CheckIsMatch(r, new byte[] { 2 });
            CheckIsMatch(r, new byte[] { 1, 2 });
            CheckIsMatch(r, new byte[] { 1, 1, 2 });
            CheckIsNotMatch(r, new byte[] { 1, 2, 1 });
            CheckIsNotMatch(r, new byte[] { 3, 2 });
            CheckIsNotMatch(r, new byte[] { 2, 2 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch8()
        {
            var r = Regex.All<byte>();

            CheckIsMatch(r, new byte[] { });
            CheckIsMatch(r, new byte[] { 1 });
            CheckIsMatch(r, new byte[] { 2 });
            CheckIsMatch(r, new byte[] { 1, 2 });
            CheckIsMatch(r, new byte[] { 1, 1, 2 });
            CheckIsMatch(r, new byte[] { 1, 2, 1 });
            CheckIsMatch(r, new byte[] { 3, 2 });
            CheckIsMatch(r, new byte[] { 2, 2 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexMatch9()
        {
            var r = Regex.Opt<byte>(Regex.Char<byte>(1));

            CheckIsMatch(r, new byte[] { });
            CheckIsMatch(r, new byte[] { 1 });
            CheckIsNotMatch(r, new byte[] { 2 });
            CheckIsNotMatch(r, new byte[] { 1, 2 });
        }

        /// <summary>
        /// Test that derivatives and regex matching are working.
        /// </summary>
        [TestMethod]
        public void TestRegexScalesToAllTypes()
        {
            var r1 = Regex.Star(Regex.Range<short>(1, 100));
            var r2 = Regex.Star(Regex.Range<ushort>(1, 100));
            var r3 = Regex.Star(Regex.Range<int>(1, 100));
            var r4 = Regex.Star(Regex.Range<uint>(1, 100));
            var r5 = Regex.Star(Regex.Range<long>(1, 100));
            var r6 = Regex.Star(Regex.Range<ulong>(1, 100));

            CheckIsMatch(r1, new short[] { 10, 20, 30, 100 });
            CheckIsMatch(r2, new ushort[] { 10, 20, 30, 100 });
            CheckIsMatch(r3, new int[] { 10, 20, 30, 100 });
            CheckIsMatch(r4, new uint[] { 10, 20, 30, 100 });
            CheckIsMatch(r5, new long[] { 10, 20, 30, 100 });
            CheckIsMatch(r6, new ulong[] { 10, 20, 30, 100 });
        }

        /// <summary>
        /// Test that Regex parsing is working.
        /// </summary>
        [TestMethod]
        [DataRow("abc", true)]
        [DataRow(".bc", true)]
        [DataRow("(ab", false)]
        [DataRow("(abc)", true)]
        [DataRow("[abc]", true)]
        [DataRow("[abc", false)]
        [DataRow("[abc)", false)]
        [DataRow("a+", true)]
        [DataRow("(ab)*", true)]
        [DataRow("a|b", true)]
        [DataRow("ab|c", true)]
        [DataRow("(a|b)?", true)]
        [DataRow("a|", false)]
        [DataRow("?", false)]
        [DataRow("*", false)]
        [DataRow("(abcd*)**", true)]
        [DataRow("[abc]+", true)]
        [DataRow("[a[bc]]", false)]
        [DataRow("[0-9a-z]", true)]
        [DataRow("\\l", true)]
        [DataRow("\\(\\)", true)]
        public void TestRegexParsing(string input, bool expected)
        {
            var p = new RegexParser(input);

            try
            {
                p.Parse();
                Assert.IsTrue(expected);
            }
            catch (ZenException e)
            {
                Console.WriteLine(e.Message);
                Assert.IsFalse(expected);
            }
        }

        /// <summary>
        /// Test that Regex parsing produces the right AST.
        /// </summary>
        [TestMethod]
        [DataRow("abc", "abc", true)]
        [DataRow("abc", "ab", false)]
        [DataRow("abc", "abcd", false)]
        [DataRow(".bc", "xbc", true)]
        [DataRow("(abc)", "abc", true)]
        [DataRow("(abc)", "abcd", false)]
        [DataRow("[abc]", "a", true)]
        [DataRow("[abc]", "b", true)]
        [DataRow("[abc]", "c", true)]
        [DataRow("[abc]", "ab", false)]
        [DataRow("[0-9a-z]", "1", true)]
        [DataRow("[0-9a-z]", "g", true)]
        [DataRow("[0-9a-z]", "\n", false)]
        [DataRow("[0-9a-z]", "A", false)]
        [DataRow("[0-9a-z]", "01", false)]
        [DataRow("ab|c", "ab", true)]
        [DataRow("ab|c", "c", true)]
        [DataRow("ab|c", "a", false)]
        [DataRow("(a|b)?", "", true)]
        [DataRow("(a|b)?", "a", true)]
        [DataRow("(a|b)?", "b", true)]
        [DataRow("(a|b)?", "ab", false)]
        [DataRow("(a|b)+", "", false)]
        [DataRow("(a|b)+", "a", true)]
        [DataRow("(a|b)+", "aa", true)]
        [DataRow("(a|b)+", "abba", true)]
        [DataRow("[abc]+", "", false)]
        [DataRow("[abc]+", "ccba", true)]
        [DataRow("[abc]+", "aabd", false)]
        [DataRow("\\(\\)", "()", true)]
        [DataRow("\n", "\n", true)]
        public void TestRegexParsingAst(string regex, string input, bool expected)
        {
            var p = new RegexParser(regex);
            var r = p.Parse();
            var bytes = input.ToCharArray().Select(c => (byte)c);
            Console.WriteLine($"Regex: {r}");
            Console.WriteLine($"Input: {string.Join(",", bytes)}");
            Assert.AreEqual(expected, r.IsMatch(bytes));
        }

        private void CheckIsMatch<T>(Regex<T> regex, IEnumerable<T> sequence) where T : IComparable<T>
        {
            var a = regex.ToAutomaton();
            Assert.IsTrue(regex.IsMatch(sequence), "regex failed to match");
            Assert.IsTrue(a.IsMatch(sequence), "automaton failed to match");
        }

        private void CheckIsNotMatch<T>(Regex<T> regex, IEnumerable<T> sequence) where T : IComparable<T>
        {
            var a = regex.ToAutomaton();
            Assert.IsFalse(regex.IsMatch(sequence), "regex matched but should not have");
            Assert.IsFalse(a.IsMatch(sequence), "automaton matched but should not have");
        }
    }
}
