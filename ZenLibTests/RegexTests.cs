// <copyright file="RegexTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for Regular expressions.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RegexTests
    {
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
    }
}
