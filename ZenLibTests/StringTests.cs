// <copyright file="StringTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            string s1 = (string)r1.Value.Item1 + (string)r1.Value.Item2;
            string s2 = (string)r2.Value.Item1 + (string)r2.Value.Item2 + (string)r2.Value.Item3;
            string s3 = (string)r3.Value.Item1 + (string)r3.Value.Item2 + (string)r3.Value.Item3 + (string)r3.Value.Item4;
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
        /// Test equality for composite types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestStringEqualityCompositeException1()
        {
            CheckAgreement<IList<string>, IList<string>>((l1, l2) => l1 == l2);
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
