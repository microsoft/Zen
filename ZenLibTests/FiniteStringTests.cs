// <copyright file="FiniteStringTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for the Zen FiniteString type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FiniteStringTests
    {
        /// <summary>
        /// Test FiniteString to string conversions.
        /// </summary>
        [TestMethod]
        [DataRow("hello")]
        [DataRow("abcde")]
        [DataRow("abc")]
        [DataRow("ab")]
        [DataRow("a")]
        [DataRow("")]
        [DataRow("CaPiTaLs")]
        [DataRow("endline\r\n")]
        public void TestConversions(string s)
        {
            FiniteString fs = s;
            Assert.AreEqual(fs.ToString(), s);
        }

        /// <summary>
        /// Test FiniteString to string conversions.
        /// </summary>
        /// <param name="s">The string.</param>
        [TestMethod]
        [DataRow("hello")]
        [DataRow("abcde")]
        [DataRow("abc")]
        [DataRow("ab")]
        [DataRow("a")]
        [DataRow("")]
        [DataRow("CaPiTaLs")]
        [DataRow("endline\r\n")]
        public void TestConstants(string s)
        {
            var fs = FiniteString.Constant(s);
            var f = Function(() => fs);
            Assert.IsTrue(f.Assert(x => x == fs));
        }

        /// <summary>
        /// Test the empty FiniteString.
        /// </summary>
        [TestMethod]
        public void TestEmptyString()
        {
            CheckValid<FiniteString>(fs => (fs.Length() == 0) == (fs == new FiniteString("")));
            CheckValid<FiniteString>(fs => (fs == new FiniteString("")) == fs.IsEmpty());
        }

        /// <summary>
        /// Test string contains.
        /// </summary>
        [TestMethod]
        [DataRow("hello", 5)]
        [DataRow("dog", 3)]
        [DataRow("AaaA", 4)]
        [DataRow("newline\n", 8)]
        [DataRow("", 0)]
        public void TestLength(string s, int expected)
        {
            var f = Function<FiniteString, ushort>(fs => fs.Length());
            var actual = f.Evaluate(s);
            Assert.AreEqual(expected, (int)actual);
        }

        /// <summary>
        /// Test the string length.
        /// </summary>
        [TestMethod]
        public void TestLengthRandom()
        {
            var f = Function<FiniteString, ushort>(fs => fs.Length());
            RandomStrings(s =>
            {
                var ex = f.Find((fs, l) => l == (ushort)s.Length).Value.ToString();
                Assert.AreEqual(s.Length, ex.Length);
            });
        }

        /// <summary>
        /// Test string contains.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", true)]
        [DataRow("brown cow", "brown", true)]
        [DataRow("hello", "ell", true)]
        [DataRow("hello", "", true)]
        [DataRow("hello", "b", false)]
        public void TestContains(string s, string sub, bool expected)
        {
            var f = Function<FiniteString, bool>(fs => fs.Contains(new FiniteString(sub)));
            var actual = f.Evaluate(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string containment.
        /// </summary>
        [TestMethod]
        public void TestContainsRandom()
        {
            RandomStrings(s =>
            {
                var f = Function<FiniteString, bool>(fs => fs.Contains(new FiniteString(s)));
                var ex = f.Find((fs, b) => b).Value.ToString();
                Assert.IsTrue(ex.Contains(s));
            });
        }

        /// <summary>
        /// Test string prefix of.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", false)]
        [DataRow("brown cow", "ow", false)]
        [DataRow("brown cow", "brown", true)]
        [DataRow("brown cow", "bro", true)]
        [DataRow("quick fox", "", true)]
        [DataRow("quick fox", "uick", false)]
        public void TestPrefixOf(string s, string sub, bool expected)
        {
            var f = Function<FiniteString, bool>(fs => FiniteStringExtensions.StartsWith(fs, new FiniteString(sub)));
            var actual = f.Evaluate(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string prefix.
        /// </summary>
        [TestMethod]
        public void TestPrefixOfRandom()
        {
            RandomStrings(s =>
            {
                var f = Function<FiniteString, bool>(fs => FiniteString.Constant(s).StartsWith(fs));
                var ex = f.Find((fs, b) => b).Value.ToString();
                Assert.IsTrue(s.StartsWith(ex));
            });
        }

        /// <summary>
        /// Test string suffix of.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", true)]
        [DataRow("brown cow", "ow", true)]
        [DataRow("brown cow", "brown", false)]
        [DataRow("quick fox", "", true)]
        public void TestSuffixOf(string s, string sub, bool expected)
        {
            var f = Function<FiniteString, bool>(fs => FiniteStringExtensions.EndsWith(fs, new FiniteString(sub)));
            var actual = f.Evaluate(s);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string suffix.
        /// </summary>
        [TestMethod]
        public void TestSuffixOfRandom()
        {
            RandomStrings(s =>
            {
                var f = Function<FiniteString, bool>(fs => FiniteString.Constant(s).EndsWith(fs));
                var ex = f.Find((fs, b) => b).Value.ToString();
                Assert.IsTrue(s.EndsWith(ex));
            });
        }

        /// <summary>
        /// Test string at.
        /// </summary>
        [TestMethod]
        [DataRow("abc", 0, "a")]
        [DataRow("abc", 1, "b")]
        [DataRow("abc", 2, "c")]
        [DataRow("abc", 3, "")]
        public void TestAt(string s, int idx, string expected)
        {
            var f = Function<FiniteString, FiniteString>(fs => fs.At((ushort)idx));
            var actual = f.Evaluate(s).ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string at.
        /// </summary>
        [TestMethod]
        public void TestAtRandom()
        {
            RandomStrings(s =>
            {
                var f = Function<FiniteString, FiniteString>(fs => fs.At(2));
                var fs = f.Evaluate(s);
                var expected = s.Length >= 3 ? s[2].ToString() : string.Empty;
                Assert.AreEqual(expected, fs.ToString());
            });
        }

        /// <summary>
        /// Test string indexof.
        /// </summary>
        [TestMethod]
        [DataRow("abc", "bc", 0, 1)]
        [DataRow("abc", "a", 0, 0)]
        [DataRow("abc", "d", 0, -1)]
        [DataRow("hello", "ll", 1, 2)]
        [DataRow("lllll", "l", 1, 1)]
        [DataRow("abcde", "ab", 3, -1)]
        public void TestIndexOf(string s, string sub, int offset, int expected)
        {
            var f = Function<FiniteString, Option<ushort>>(fs => fs.IndexOf(new FiniteString(sub), (ushort)offset));
            var idx = f.Evaluate(s);
            var actual = idx.HasValue ? (int)idx.Value : -1;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string indexof.
        /// </summary>
        [TestMethod]
        public void TestIndexOfRandom()
        {
            RandomStrings(s =>
            {
                var f = Function<FiniteString, Option<ushort>>(fs => fs.IndexOf(new FiniteString(s)));
                var ex = f.Find((fs, i) => i.HasValue()).Value.ToString();
                var idx = f.Evaluate(ex);
                Assert.IsTrue(ex.Contains(s));
                Assert.AreEqual(idx.Value, ex.IndexOf(s));
            });
        }

        /// <summary>
        /// Test string substring.
        /// </summary>
        [TestMethod]
        [DataRow("abc", 1, 1, "b")]
        [DataRow("abc", 1, 2, "bc")]
        [DataRow("abc", 0, 3, "abc")]
        [DataRow("abc", 10, 1, "")]
        [DataRow("", 0, 1, "")]
        [DataRow("", 0, 0, "")]
        [DataRow("abcdefg", 0, 100, "abcdefg")]
        public void TestSubstring(string s, int offset, int len, string expected)
        {
            var f = Function<FiniteString, FiniteString>(fs => fs.SubString((ushort)offset, (ushort)len));
            var actual = f.Evaluate(s).ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string replace all.
        /// </summary>
        [TestMethod]
        [DataRow("abc", 'b', 'd', "adc")]
        [DataRow("aaa", 'a', 'b', "bbb")]
        [DataRow("", 'a', 'b', "")]
        [DataRow("abcd", 'a', 'a', "abcd")]
        [DataRow("AaBb", 'A', 'a', "aaBb")]
        public void TestReplaceAll(string s, char c1, char c2, string expected)
        {
            var f = Function<FiniteString, FiniteString>(fs => fs.ReplaceAll(c1, c2));
            var actual = f.Evaluate(s).ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string replace all.
        /// </summary>
        [TestMethod]
        [DataRow("abc", 'b', "ac")]
        [DataRow("aaa", 'a', "")]
        [DataRow("abcc", 'c', "ab")]
        [DataRow("abcc", 'a', "bcc")]
        [DataRow("", 'b', "")]
        public void TestRemoveAll(string s, char c, string expected)
        {
            var f = Function<FiniteString, FiniteString>(fs => fs.RemoveAll(c));
            var actual = f.Evaluate(s).ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test string at.
        /// </summary>
        [TestMethod]
        [DataRow("abc", "a", "abca")]
        [DataRow("abc", "", "abc")]
        [DataRow("", "efg", "efg")]
        [DataRow("quick", " brown", "quick brown")]
        public void TestConcat(string s1, string s2, string expected)
        {
            var f = Function<FiniteString, FiniteString>(fs => fs.Concat(new FiniteString(s2)));
            var actual = f.Evaluate(s1).ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test prefix implies containment.
        /// </summary>
        [TestMethod]
        public void TestPrefixImpliesContains()
        {
            var f = Function<FiniteString, FiniteString, bool>(
                (fs1, fs2) => Implies(fs2.StartsWith(fs1), fs2.Contains(fs1)));

            var ex = f.Find((fs1, fs2, b) => Not(b));
            Assert.IsFalse(ex.HasValue);
        }

        /// <summary>
        /// Test suffix implies containment.
        /// </summary>
        [TestMethod]
        public void TestSuffixImpliesContains()
        {
            var f = Function<FiniteString, FiniteString, bool>(
                (fs1, fs2) => Implies(fs2.EndsWith(fs1), fs2.Contains(fs1)));

            var ex = f.Find((fs1, fs2, b) => Not(b));
            Assert.IsFalse(ex.HasValue);
        }

        /// <summary>
        /// Test suffix implies containment.
        /// </summary>
        [TestMethod]
        public void TestAtWithinBounds()
        {
            var f = Function<FiniteString, ushort, bool>(
                (fs, i) => Implies(i < fs.Length(), fs.At(i) != new FiniteString("")));

            var ex = f.Find((fs1, fs2, b) => Not(b));
            Assert.IsFalse(ex.HasValue);
        }

        /// <summary>
        /// Test index of not null implies contains.
        /// </summary>
        [TestMethod]
        public void TestIndexOfImpliesContains()
        {
            var f = Function<FiniteString, FiniteString, bool>(
                (fs1, fs2) => Implies(fs1.Contains(fs2), fs1.IndexOf(fs2).HasValue()));

            var ex = f.Find((fs1, fs2, b) => Not(b));
            Assert.IsFalse(ex.HasValue);
        }

        /// <summary>
        /// Test index of not null implies contains.
        /// </summary>
        [TestMethod]
        public void TestConcatContains()
        {
            var f = Function<FiniteString, FiniteString, FiniteString, bool>(
                (fs1, fs2, fs3) => Implies(fs1.Contains(fs3), fs1.Concat(fs2).Contains(fs3)));

            var ex = f.Find((fs1, fs2, fs3, b) => Not(b), listSize: 4, checkSmallerLists: true);
            Assert.IsFalse(ex.HasValue);
        }
    }
}
