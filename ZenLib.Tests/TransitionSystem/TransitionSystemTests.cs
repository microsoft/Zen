// <copyright file="TransitionSystemTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.TransitionSystem;

    /// <summary>
    /// Tests for symbolic transition systems in Zen.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TransitionSystemTests
    {
        /// <summary>
        /// Test that a basic transition system works.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemBasic()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s <= 100,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == sOld + 1,
                SafetyChecks = (s) => s < 105,
            };

            var searchResults = ts.ModelCheck(2000).ToArray();

            Assert.AreEqual(6, searchResults.Length);
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(searchResults[i].CounterExample == null);
            }
            Assert.IsTrue(searchResults[5].SearchOutcome == SearchOutcome.CounterExample);
            Assert.IsTrue(searchResults[5].CounterExample != null);
        }

        /// <summary>
        /// Test that k-induction works.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemInduction()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s <= 100,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == Zen.If(sOld < 200, sOld + 1, 0),
                SafetyChecks = (s) => s <= 200,
            };

            var searchResults = ts.ModelCheck(2000, useKInduction: true).ToArray();

            Assert.AreEqual(2, searchResults.Length);
            Assert.IsTrue(searchResults[0].SearchOutcome == SearchOutcome.NoCounterExample);
            Assert.IsTrue(searchResults[0].CounterExample == null);
            Assert.IsTrue(searchResults[1].SearchOutcome == SearchOutcome.SafetyProof);
            Assert.IsTrue(searchResults[1].CounterExample == null);
        }

        /// <summary>
        /// Test that timeouts work.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemTimeout()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s <= 100,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == sOld + 1,
                SafetyChecks = (s) => s <= 100000,
            };

            var searchResults = ts.ModelCheck(timeoutMs: 10).ToArray();

            Assert.IsTrue(searchResults.Last().SearchOutcome == SearchOutcome.Timeout);
            Assert.IsTrue(searchResults.Last().Stats.Time <= 100);
        }

        /// <summary>
        /// Test that yields work.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemYield()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s <= 100,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == sOld + 1,
                SafetyChecks = (s) => s <= 100000,
            };

            var searchResults = ts.ModelCheck().Take(5).ToArray();

            Assert.AreEqual(5, searchResults.Length);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(i + 1, searchResults[i].Depth);
            }

            foreach (var result in searchResults)
            {
                Assert.IsTrue(result.SearchOutcome == SearchOutcome.NoCounterExample);
                Assert.IsTrue(result.CounterExample == null);
            }
        }

        /// <summary>
        /// Test that invariants work.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemInvariants()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => true,
                Invariants = (s) => s <= 100,
                NextRelation = (sOld, sNew) => sNew == sOld + 1,
                SafetyChecks = (s) => s <= 200,
            };

            var searchResults = ts.ModelCheck(useKInduction: true).ToArray();

            Assert.AreEqual(2, searchResults.Length);
            Assert.IsTrue(searchResults[0].SearchOutcome == SearchOutcome.NoCounterExample);
            Assert.IsTrue(searchResults[0].CounterExample == null);
            Assert.IsTrue(searchResults[1].SearchOutcome == SearchOutcome.SafetyProof);
            Assert.IsTrue(searchResults[1].CounterExample == null);
        }
    }
}
