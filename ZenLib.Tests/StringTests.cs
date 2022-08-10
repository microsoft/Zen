// <copyright file="StringTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Z3;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the string type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StringTests
    {
        /// <summary>
        /// Convert a C# string to a Z3 string.
        /// </summary>
        /// <param name="s">The C# string.</param>
        /// <returns>The Z3 string.</returns>
        private static string ConvertCSharpStringToZ3(string s)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                sb.Append(string.Format(@"\u{0:x4}", (int)s[i]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Test string conversions.
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("AaBb")]
        [DataRow("\t")]
        [DataRow("\r")]
        [DataRow("\n")]
        [DataRow("\a")]
        [DataRow("\b")]
        [DataRow("\v")]
        [DataRow("\f")]
        [DataRow("\t")]
        [DataRow("\"")]
        [DataRow("\\")]
        [DataRow("\\x5C\\x6E")]
        [DataRow("endline\n")]
        [DataRow("\x01\\x01")]
        public void TestStringConversions(string s)
        {
            var context = new Context();
            var toz3 = ConvertCSharpStringToZ3(s);
            var tocs = CommonUtilities.ConvertZ3StringToCSharp(context.MkString(toz3).ToString());
            Assert.AreEqual(s, tocs);
        }

        /// <summary>
        /// Test string conversions.
        /// </summary>
        [TestMethod]
        public void TestStringConversionsRandom()
        {
            for (int i = 0; i < 300; i++)
            {
                string s = RandomString();
                var context = new Context();
                var toz3 = ConvertCSharpStringToZ3(s);
                var tocs = CommonUtilities.ConvertZ3StringToCSharp(context.MkString(toz3).ToString());
                Assert.AreEqual(s, tocs);
            }
        }

        /// <summary>
        /// Test concatenation with empty string.
        /// </summary>
        [TestMethod]
        public void TestConcatIdentity()
        {
            CheckValid<string>(x => x + "" == x);
        }

        /// <summary>
        /// Test string concatenation is associative.
        /// </summary>
        [TestMethod]
        public void TestConcatAssociative()
        {
            CheckValid<string, string, string>((x, y, z) => (x + y) + z == x + (y + z));
        }

        /// <summary>
        /// Test concat twice.
        /// </summary>
        [TestMethod]
        public void TestConcatTwice()
        {
            CheckAgreement<string>(x => x + x == "hello");
        }

        /// <summary>
        /// Test endswith agreement.
        /// </summary>
        [TestMethod]
        public void TestEndsWithAgreement()
        {
            RandomStrings(sub =>
            {
                CheckAgreement<string>(s => s.EndsWith(sub));
            });
        }

        /// <summary>
        /// Test startswith agreement.
        /// </summary>
        [TestMethod]
        public void TestStartsWithAgreement()
        {
            RandomStrings(sub =>
            {
                CheckAgreement<string>(s => s.StartsWith(sub));
            });
        }

        /// <summary>
        /// Test contains agreement.
        /// </summary>
        [TestMethod]
        public void TestContainsAgreement()
        {
            RandomStrings(sub =>
            {
                CheckAgreement<string>(s => s.Contains(sub));
            });
        }

        /// <summary>
        /// Test startswith and contains interactions.
        /// </summary>
        [TestMethod]
        public void TestReplaceContains()
        {
            RandomStrings(sub =>
            {
                if (sub != "")
                {
                    CheckAgreement<string, string>((s1, s2) => s1.ReplaceFirst(sub, s2).Contains(s2));
                }
            });
        }

        /// <summary>
        /// Test that a string contains its substring.
        /// </summary>
        [TestMethod]
        public void TestContainsSubstring()
        {
            RandomBytes(offset =>
            RandomBytes(length =>
                CheckValid<string>(s => s.Contains(s.Slice(new BigInteger(offset), new BigInteger(length))))));
        }

        /// <summary>
        /// Test substring agreement.
        /// </summary>
        [TestMethod]
        public void TestSubstringAgreement()
        {
            var offset = new BigInteger(2);
            var length = new BigInteger(3);
            CheckAgreement<string>(s => s.Slice(offset, length).EndsWith("ab"));
        }

        /// <summary>
        /// Test length agreement.
        /// </summary>
        [TestMethod]
        public void TestLengthAgreement()
        {
            CheckAgreement<string>(s => s.Length() == new BigInteger(0));
            CheckAgreement<string>(s => s.Length() == new BigInteger(1));
            CheckAgreement<string>(s => s.Length() == new BigInteger(2));
            CheckAgreement<string>(s => s.Length() == new BigInteger(3));
            CheckAgreement<string>(s => s.Length() == new BigInteger(4));
            CheckAgreement<string>(s => s.Length() == new BigInteger(5));
            CheckAgreement<string>(s => s.Length() == new BigInteger(10));
        }

        /// <summary>
        /// Test concatenating multiple values is solved correctly.
        /// </summary>
        [TestMethod]
        public void TestConcatMultipleValues()
        {
            var f1 = Zen.Function<string, string, bool>((w, x) => w + x == "hello");
            var f2 = Zen.Function<string, string, string, bool>((w, x, y) => w + x + y == "hello");
            var f3 = Zen.Function<string, string, string, string, bool>((w, x, y, z) => w + x + y + z == "hello");
            var r1 = f1.Find((i1, i2, o) => o);
            var r2 = f2.Find((i1, i2, i3, o) => o);
            var r3 = f3.Find((i1, i2, i3, i4, o) => o);

            Assert.IsTrue(r1.HasValue);
            Assert.IsTrue(r2.HasValue);
            Assert.IsTrue(r3.HasValue);

            string s1 = r1.Value.Item1 + r1.Value.Item2;
            string s2 = r2.Value.Item1 + r2.Value.Item2 + r2.Value.Item3;
            string s3 = r3.Value.Item1 + r3.Value.Item2 + r3.Value.Item3 + r3.Value.Item4;
            Assert.AreEqual("hello", s1);
            Assert.AreEqual("hello", s2);
            Assert.AreEqual("hello", s3);

            Assert.IsTrue(f1.Evaluate("he", "llo"));
            Assert.IsFalse(f1.Evaluate("he", "ello"));

            Assert.IsTrue(f2.Evaluate("h", "e", "llo"));
            Assert.IsFalse(f2.Evaluate("hell", "l", "o"));

            Assert.IsTrue(f3.Evaluate("h", "e", "ll", "o"));
            Assert.IsFalse(f3.Evaluate("hel", "l", "l", "o"));
        }

        /// <summary>
        /// Test startswith evaluation.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", false)]
        [DataRow("brown cow", "ow", false)]
        [DataRow("brown cow", "brown", true)]
        [DataRow("brown cow", "bro", true)]
        [DataRow("quick fox", "", true)]
        [DataRow("quick fox", "uick", false)]
        public void TestStartsWithEvaluation(string s, string sub, bool expected)
        {
            var f = Zen.Function<string, string, bool>((s1, s2) => s1.StartsWith(s2));
            Assert.AreEqual(expected, f.Evaluate(s, sub));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s, sub));
        }

        /// <summary>
        /// Test endswith evaluation.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", true)]
        [DataRow("brown cow", "ow", true)]
        [DataRow("brown cow", "brown", false)]
        [DataRow("quick fox", "", true)]
        public void TestEndsWithEvaluation(string s, string sub, bool expected)
        {
            var f = Zen.Function<string, string, bool>((s1, s2) => s1.EndsWith(s2));
            Assert.AreEqual(expected, f.Evaluate(s, sub));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s, sub));
        }

        /// <summary>
        /// Test contains evaluation.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", true)]
        [DataRow("brown cow", "brown", true)]
        [DataRow("hello", "ell", true)]
        [DataRow("hello", "", true)]
        [DataRow("hello", "b", false)]
        public void TestContainsEvaluation(string s, string sub, bool expected)
        {
            var f = Zen.Function<string, string, bool>((s1, s2) => s1.Contains(s2));
            Assert.AreEqual(expected, f.Evaluate(s, sub));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s, sub));
        }

        /// <summary>
        /// Test contains replace first.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", "fox", "brown fox")]
        [DataRow("aabbcc", "b", "d", "aadbcc")]
        [DataRow("hello", "ll", "rrr", "herrro")]
        [DataRow("hello", "", " abc", " abchello")]
        [DataRow("abc", "b", "", "ac")]
        public void TestReplaceEvaluation(string s, string sub, string replace, string expected)
        {
            var f = Zen.Function<string, string>(s => s.ReplaceFirst(sub, replace));
            Assert.AreEqual(expected, f.Evaluate(s));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s));
        }

        /// <summary>
        /// Test substring evaluation.
        /// </summary>
        [TestMethod]
        [DataRow("hello", 0, 3, "hel")]
        [DataRow("hello", 1, 3, "ell")]
        [DataRow("hello", 10, 3, "")]
        [DataRow("hello", 0, 20, "hello")]
        public void TestSubstringEvaluation(string s, int offset, int length, string expected)
        {
            var f = Zen.Function<string, string>(s => s.Slice(new BigInteger(offset), new BigInteger(length)));
            Assert.AreEqual(expected, f.Evaluate(s));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s));
        }

        /// <summary>
        /// Test string at evaluation.
        /// </summary>
        [TestMethod]
        [DataRow("abcde", 0, "a")]
        [DataRow("abcde", 1, "b")]
        [DataRow("abcde", 2, "c")]
        [DataRow("abcde", 3, "d")]
        [DataRow("abcde", 4, "e")]
        [DataRow("abcde", 5, "")]
        [DataRow("", 0, "")]
        [DataRow("", 1, "")]
        [DataRow("", 2, "")]
        public void TestAtEvaluation(string s, int index, string expected)
        {
            var f = Zen.Function<string, BigInteger, string>((s, idx) => s.At(idx));
            Assert.AreEqual(expected, f.Evaluate(s, (ushort)index));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s, (ushort)index));

            if (expected != "")
            {
                var x = f.Find((str, idx, o) => And(str == s, o == expected));
                Assert.AreEqual(x.Value.Item2, new BigInteger(index));
            }
        }

        /// <summary>
        /// Test string length evaluation.
        /// </summary>
        [TestMethod]
        [DataRow("", 0)]
        [DataRow("a", 1)]
        [DataRow("ab", 2)]
        [DataRow("abc", 3)]
        [DataRow("abcd", 4)]
        [DataRow("abcde", 5)]
        [DataRow("\x01\x02", 2)]
        public void TestLengthEvaluation(string s, int expected)
        {
            var f = Zen.Function<string, BigInteger>(s => s.Length());
            Assert.AreEqual(new BigInteger(expected), f.Evaluate(s));
            f.Compile();
            Assert.AreEqual(new BigInteger(expected), f.Evaluate(s));
        }

        /// <summary>
        /// Test string indexof evaluation.
        /// </summary>
        [TestMethod]
        [DataRow("abcda", "", 0, 0)]
        [DataRow("abcda", "", 3, 3)]
        [DataRow("abcda", "", 10, -1)]
        [DataRow("abcda", "a", 0, 0)]
        [DataRow("abcda", "a", 1, 4)]
        [DataRow("abcda", "b", 0, 1)]
        [DataRow("abcda", "b", 1, 1)]
        [DataRow("abcda", "b", 2, -1)]
        [DataRow("abcda", "e", 0, -1)]
        public void TestIndexOfEvaluation(string s, string sub, int offset, int expected)
        {
            var f = Zen.Function<string, BigInteger>(s => s.IndexOf(sub, new BigInteger(offset)));
            Assert.AreEqual((short)expected, f.Evaluate(s));
            f.Compile();
            Assert.AreEqual((short)expected, f.Evaluate(s));
        }

        /// <summary>
        /// Test indexof find.
        /// </summary>
        [TestMethod]
        public void TestIndexOfFind()
        {
            var f = Zen.Function<string, BigInteger>(s => s.IndexOf("a", new BigInteger(0)));
            var input = f.Find((s, o) => o == new BigInteger(5));
            Assert.AreEqual((short)5, f.Evaluate(input.Value));
        }

        /// <summary>
        /// Test multiple operations.
        /// </summary>
        [TestMethod]
        public void TestMultipleOperations()
        {
            var f = Zen.Function<string, bool>(s =>
            {
                var c = s.At(new BigInteger(3));
                var s2 = s.Slice(new BigInteger(5), new BigInteger(2));
                return And(s.StartsWith("a"), c == "b", s2 == "cd", s.Length() == new BigInteger(10));
            });

            var x = f.Find((i, o) => o);
            Assert.IsTrue(x.HasValue);
            Assert.IsTrue(f.Evaluate(x.Value));
        }

        /// <summary>
        /// Test at is substring.
        /// </summary>
        [TestMethod]
        public void TestAtIsSubstring()
        {
            CheckValid<string>(s => s.At(new BigInteger(0)) == s.Slice(new BigInteger(0), new BigInteger(1)));
            CheckValid<string>(s => s.At(new BigInteger(1)) == s.Slice(new BigInteger(1), new BigInteger(1)));
            CheckValid<string>(s => s.At(new BigInteger(2)) == s.Slice(new BigInteger(2), new BigInteger(1)));
            CheckValid<string>(s => s.At(new BigInteger(3)) == s.Slice(new BigInteger(3), new BigInteger(1)));
            CheckValid<string>(s => s.At(new BigInteger(4)) == s.Slice(new BigInteger(4), new BigInteger(1)));
        }

        /// <summary>
        /// Test string matchesregex.
        /// </summary>
        [TestMethod]
        [DataRow("[a-z][a-z]+")]
        [DataRow("c(a|b)")]
        [DataRow("a+b+")]
        [DataRow("abc")]
        [DataRow(".bc")]
        [DataRow("(abc)")]
        [DataRow("[abc]")]
        [DataRow("[0-9a-z]")]
        [DataRow("ab|c")]
        [DataRow("(a|b)?")]
        [DataRow("(a|b)+")]
        [DataRow("[abc]+")]
        [DataRow("\\(\\)")]
        [DataRow(@"\n")]
        [DataRow("\n")]
        [DataRow("[ab\\+]")]
        [DataRow("\\\\")]
        [DataRow("[^a-zA-Z]")]
        [DataRow("abcd\\||bc")]
        [DataRow("[a-]+")]
        [DataRow("[a*]+")]
        [DataRow("[*-\\\\]+")]
        [DataRow("(a|b|c|d)")]
        [DataRow("a\t")]
        [DataRow(" ")]
        [DataRow("[ab ]")]
        [DataRow("s s")]
        [DataRow("\\u058")]
        [DataRow(".*a.*")]
        [DataRow("^8075:[9][4-9][0-9][0-9]$")]
        [DataRow(".*")]
        [DataRow("^8220:1234|8220:5678|8220:9012|8220:345$")]
        [DataRow("^806[8-9]:.*$")]
        public void TestMatchesRegex(string regex)
        {
            var r = Regex.Parse(regex);

            var examples = Zen.Constraint<string>(s => s.MatchesRegex(regex)).FindAll().Take(5);

            foreach (var example in examples)
            {
                Assert.IsTrue(r.IsMatch(Seq.FromString(example).Values));
            }

            examples = Zen.Constraint<string>(s => Not(s.MatchesRegex(regex))).FindAll().Take(5);

            foreach (var example in examples)
            {
                Assert.IsFalse(r.IsMatch(Seq.FromString(example).Values));
            }
        }

        /// <summary>
        /// Test string matchesregex.
        /// </summary>
        [TestMethod]
        public void TestNegationDot()
        {
            var example = Zen.Constraint<string>(s => Not(s.MatchesRegex(".*"))).Find();
            Assert.IsFalse(example.HasValue);
        }

        /// <summary>
        /// Test string matchesregex for empty regex.
        /// </summary>
        [TestMethod]
        public void TestMatchesRegexEmpty1()
        {
            var r = Regex.Empty<char>();
            var s = Zen.Constraint<string>(s => s.MatchesRegex(r)).Find();
            Assert.IsFalse(s.HasValue);
        }

        /// <summary>
        /// Test string matchesregex for empty regex.
        /// </summary>
        [TestMethod]
        public void TestMatchesRegexEmpty2()
        {
            var r = Regex.Empty<byte>();
            var s = Zen.Constraint<Seq<byte>>(s => s.MatchesRegex(r)).Find();
            Assert.IsFalse(s.HasValue);
        }

        /// <summary>
        /// Test string concatentation equality.
        /// </summary>
        [TestMethod]
        public void TestConcatenationEquality()
        {
            var s = Zen.Constraint<string, string>((s1, s2) => s1 == s2 + "string").Find();
            Assert.IsTrue(s.Value.Item1 == s.Value.Item2 + "string");
        }

        /// <summary>
        /// Test that char seq works.
        /// </summary>
        [TestMethod]
        public void TestCharSeqWorks()
        {
            var s = Zen.Constraint<Seq<char>>(s => s.Contains((char)70)).Find();
            Assert.AreEqual("[F]", s.Value.ToString());
        }

        /// <summary>
        /// Test that string characters in the d800-dfff range work.
        /// </summary>
        [TestMethod]
        public void TestStringInvalidUnicodeRange()
        {
            var seq = new Seq<char>().Add((char)0xd800).Add((char)0xdfff);
            var s = Zen.Constraint<string>(s => s == Seq.AsString(seq)).Find();
            Assert.AreEqual(@"\u{D800}\u{DFFF}", s.Value);
        }

        /// <summary>
        /// Test endswith implies contains.
        /// </summary>
        [TestMethod]
        public void TestEndsWithContains()
        {
            RandomStrings(sub => CheckValid<string>(s => Implies(s.EndsWith(sub), s.Contains(sub))));
        }

        /// <summary>
        /// Test startswith implies contains.
        /// </summary>
        [TestMethod]
        public void TestStartsWithContains()
        {
            RandomStrings(sub => CheckValid<string>(s => Implies(s.StartsWith(sub), s.Contains(sub))));
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException1()
        {
            CheckAgreement<FSeq<string>, FSeq<string>>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test invalid null string literal.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidStringLiteralNull()
        {
            Constant<string>(null);
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException2()
        {
            CheckAgreement<Map<string, string>, Map<string, string>>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException3()
        {
            CheckAgreement<Option<FSeq<string>>, Option<FSeq<string>>>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException4()
        {
            CheckAgreement<(string, FSeq<string>), (string, FSeq<string>)>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test an exception is thrown for non-strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConcatException()
        {
            _ = FSeq.Empty<string>() + FSeq.Empty<string>();
        }

        /// <summary>
        /// Test that DD backend does not work with strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDiagramBackendException1()
        {
            var f = new ZenFunction<string, bool>(s => s == "a");
            f.Find((x, y) => y, backend: ModelChecking.Backend.DecisionDiagrams);
        }

        /// <summary>
        /// Test that DD backend does not work with strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDiagramBackendException2()
        {
            var f = new ZenFunction<string, string>(s => s + "a");
            f.Find((x, y) => x == y, backend: ModelChecking.Backend.DecisionDiagrams);
        }

        /// <summary>
        /// Test that DD backend does not work with strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDiagramBackendException3()
        {
            var f = new ZenFunction<string, string>(s => s + "a");
            f.Transformer();
        }
    }
}
