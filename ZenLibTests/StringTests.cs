// <copyright file="StringTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Z3;
    using ZenLib;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for the string type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StringTests
    {
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
            var toz3 = CommonUtilities.ConvertCSharpStringToZ3(s);
            var tocs = CommonUtilities.ConvertZ3StringToCSharp(context.MkString(toz3).ToString());
            Assert.AreEqual(s, tocs);
        }

        /// <summary>
        /// Test string conversions.
        /// </summary>
        [TestMethod]
        public void TestStringConversionsRandom()
        {
            for (int i = 0; i < 1000; i++)
            {
                string s = RandomString();
                var context = new Context();
                var toz3 = CommonUtilities.ConvertCSharpStringToZ3(s);
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
        /// Test startswith and replace interactions.
        /// </summary>
        [TestMethod]
        public void TestReplaceStartsWith()
        {
            RandomStrings(sub =>
            {
                if (sub != "")
                {
                    CheckValid<string, string>((s1, s2) =>
                        Implies(s1.StartsWith(sub), s1.ReplaceFirst(sub, s2).StartsWith(s2)));
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
                CheckValid<string>(s => s.Contains(s.Substring(offset, length)))));
        }

        /// <summary>
        /// Test substring agreement.
        /// </summary>
        [TestMethod]
        public void TestSubstringAgreement()
        {
            var offset = RandomByte();
            var length = RandomByte();
            CheckAgreement<string>(s => s.Substring(offset, length).EndsWith("ab"));
        }

        /// <summary>
        /// Test concatenating multiple values is solved correctly.
        /// </summary>
        [TestMethod]
        public void TestConcatMultipleValues()
        {
            var f1 = Function<string, string, bool>((w, x) => w + x == "hello");
            var f2 = Function<string, string, string, bool>((w, x, y) => w + x + y == "hello");
            var f3 = Function<string, string, string, string, bool>((w, x, y, z) => w + x + y + z == "hello");
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
            var f = Function<string, string, bool>((s1, s2) => s1.StartsWith(s2));
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
            var f = Function<string, string, bool>((s1, s2) => s1.EndsWith(s2));
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
            var f = Function<string, string, bool>((s1, s2) => s1.Contains(s2));
            Assert.AreEqual(expected, f.Evaluate(s, sub));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s, sub));
        }

        /// <summary>
        /// Test string replace first.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", "fox", "brown fox")]
        [DataRow("aabbcc", "b", "d", "aadbcc")]
        [DataRow("hello", "ll", "rrr", "herrro")]
        [DataRow("hello", "", " abc", "hello abc")]
        [DataRow("abc", "b", "", "ac")]
        [DataRow("abcd", "e", "f", "abcd")]
        public void TestReplaceFirst(string s, string sub, string replace, string expected)
        {
            Assert.AreEqual(expected, CommonUtilities.ReplaceFirst(s, sub, replace));
        }

        /// <summary>
        /// Test contains replace first.
        /// </summary>
        [TestMethod]
        [DataRow("brown cow", "cow", "fox", "brown fox")]
        [DataRow("aabbcc", "b", "d", "aadbcc")]
        [DataRow("hello", "ll", "rrr", "herrro")]
        [DataRow("hello", "", " abc", "hello abc")]
        [DataRow("abc", "b", "", "ac")]
        public void TestReplaceEvaluation(string s, string sub, string replace, string expected)
        {
            var f = Function<string, string>(s => s.ReplaceFirst(sub, replace));
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
            var f = Function<string, string>(s => s.Substring(offset, length));
            Assert.AreEqual(expected, f.Evaluate(s));
            f.Compile();
            Assert.AreEqual(expected, f.Evaluate(s));
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
        /// Test invalid implicit conversion.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidImplicitConversion()
        {
            Zen<bool> b = true;
            Or(b, "");
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException1()
        {
            CheckAgreement<IList<string>, IList<string>>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test invalid null string literal.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidStringLiteralNull()
        {
            String(null);
        }

        /// <summary>
        /// Test invalid string literal.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidStringLiteral2()
        {
            char c = (char)960; // greek pi
            String(c.ToString());
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException2()
        {
            CheckAgreement<IDictionary<string, string>, IDictionary<string, string>>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException3()
        {
            CheckAgreement<Option<IList<string>>, Option<IList<string>>>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException4()
        {
            CheckAgreement<(string, IList<string>), (string, IList<string>)>((l1, l2) => l1 == l2);
        }

        /// <summary>
        /// Test an exception is thrown for non-strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConcatException()
        {
            _ = EmptyList<string>() + EmptyList<string>();
        }

        /// <summary>
        /// Test that DD backend does not work with strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDiagramBackendException1()
        {
            var f = Function<string, bool>(s => s == "a");
            f.Find((x, y) => y, backend: ModelChecking.Backend.DecisionDiagrams);
        }

        /// <summary>
        /// Test that DD backend does not work with strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDiagramBackendException2()
        {
            var f = Function<string, string>(s => s + "a");
            f.Find((x, y) => x == y, backend: ModelChecking.Backend.DecisionDiagrams);
        }

        /// <summary>
        /// Test that DD backend does not work with strings.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDiagramBackendException3()
        {
            var f = Function<string, string>(s => s + "a");
            f.Transformer();
        }
    }
}
