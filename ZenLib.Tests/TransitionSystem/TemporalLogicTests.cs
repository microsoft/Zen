// <copyright file="TemporalLogicTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.TransitionSystem;

    /// <summary>
    /// Tests for temporal logic.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TemporalLogicTests
    {
        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfAnd()
        {
            var p1 = LTL.Predicate<int>(s => s == 3);
            var p2 = LTL.Predicate<int>(s => s == 4);

            var a = LTL.And(p1, p2).Nnf();
            Assert.IsTrue(a is And<int>);
            Assert.AreEqual(p1, ((And<int>)a).Formula1);
            Assert.AreEqual(p2, ((And<int>)a).Formula2);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfOr()
        {
            var p1 = LTL.Predicate<int>(s => s == 3);
            var p2 = LTL.Predicate<int>(s => s == 4);

            var a = LTL.Or(p1, p2).Nnf();
            Assert.IsTrue(a is Or<int>);
            Assert.AreEqual(p1, ((Or<int>)a).Formula1);
            Assert.AreEqual(p2, ((Or<int>)a).Formula2);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfEventually()
        {
            var p = LTL.Predicate<int>(s => s == 3);

            var a = LTL.Eventually(p).Nnf();
            Assert.IsTrue(a is Eventually<int>);
            Assert.AreEqual(p, ((Eventually<int>)a).Formula);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfAlways()
        {
            var p = LTL.Predicate<int>(s => s == 3);

            var a = LTL.Always(p).Nnf();
            Assert.IsTrue(a is Always<int>);
            Assert.AreEqual(p, ((Always<int>)a).Formula);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfPredicate()
        {
            var p1 = LTL.Predicate<int>(s => s == 3);
            Assert.AreEqual(p1, p1.Nnf());
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfNotPredicate()
        {
            var p1 = LTL.Predicate<int>(s => s == 3);
            var p2 = LTL.Not(p1).Nnf();

            var x = Zen.Arbitrary<int>();
            var e1 = ((Predicate<int>)p1).Function(x);
            var e2 = ((Predicate<int>)p2).Function(x);

            Assert.AreEqual(Zen.Not(e1), e2);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfNotAnd()
        {
            var p1 = LTL.Predicate<int>(s => s == 3);
            var p2 = LTL.Predicate<int>(s => s == 4);
            var a = LTL.And(p1, p2);
            var b = LTL.Not(a).Nnf();
            Assert.IsTrue(b is Or<int>);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfNotOr()
        {
            var p1 = LTL.Predicate<int>(s => s == 3);
            var p2 = LTL.Predicate<int>(s => s == 4);
            var a = LTL.Or(p1, p2);
            var b = LTL.Not(a).Nnf();
            Assert.IsTrue(b is And<int>);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfNotAlways()
        {
            var p = LTL.Predicate<int>(s => s == 3);
            var a = LTL.Always(p);
            var b = LTL.Not(a).Nnf();
            Assert.IsTrue(b is Eventually<int>);
        }

        /// <summary>
        /// Test that NNF conversion works.
        /// </summary>
        [TestMethod]
        public void TestNnfNotEventually()
        {
            var p = LTL.Predicate<int>(s => s == 3);
            var a = LTL.Eventually(p);
            var b = LTL.Not(a).Nnf();
            Assert.IsTrue(b is Always<int>);
        }
    }
}
