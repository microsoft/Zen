// <copyright file="TransitionSystemTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
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
        /// Test that the always specification works.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemAlways()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s <= 100,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == sOld + 1,
                Specification = Spec.Always(Spec.Predicate<uint>(s => s < 105)),
            };

            var searchResults = ts.ModelCheck(2000).ToArray();

            Console.WriteLine(string.Join(",", searchResults.Last().CounterExample.Select(x => x.ToString())));

            Assert.AreEqual(6, searchResults.Length);
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(searchResults[i].CounterExample == null);
            }
            Assert.IsTrue(searchResults[5].SearchOutcome == SearchOutcome.CounterExample);
            Assert.IsTrue(searchResults[5].CounterExample != null);
            Assert.IsTrue(searchResults[5].CounterExample.Length == 6);
        }

        /// <summary>
        /// Test that the always specification works.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemEventually()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s == 0,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == Zen.If(sOld == 1, 0, sOld + 1),
                Specification = Spec.Eventually(Spec.Predicate<uint>(s => s == 3)),
            };

            var searchResults = ts.ModelCheck(2000).ToArray();

            Assert.AreEqual(2, searchResults.Length);
            Assert.IsTrue(searchResults.Last().SearchOutcome == SearchOutcome.CounterExample);
            Assert.IsTrue(searchResults.Last().CounterExample != null);
            Assert.IsTrue(searchResults.Last().CounterExample[0] == 0);
            Assert.IsTrue(searchResults.Last().CounterExample[1] == 1);
            Assert.IsTrue(searchResults.Last().CounterExample[2] == 0);
            Assert.IsTrue(searchResults.Last().CounterExample.Length == 3);
        }

        /// <summary>
        /// Test that the always specification works.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemPredicate()
        {
            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s == 0,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == sOld + 1,
                Specification = Spec.Predicate<uint>(s => s == 0),
            };

            var searchResults = ts.ModelCheck(2000).Take(3).ToArray();
            Assert.AreEqual(3, searchResults.Length);
            Assert.IsTrue(searchResults.Last().SearchOutcome == SearchOutcome.NoCounterExample);
            Assert.IsTrue(searchResults.Last().CounterExample == null);
        }

        /// <summary>
        /// Test that the always specification works.
        /// </summary>
        [TestMethod]
        public void TestTransitionSystemAlwaysEventually()
        {
            var ts = new TransitionSystem<Pair<bool, uint>>
            {
                InitialStates = (s) => s == new Pair<bool, uint>(false, 0),
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) =>
                {
                    var item1 = Zen.Or(sOld.Item1(), sOld.Item2() >= 2);
                    var item2 = Zen.If(sOld.Item1(), 0, sOld.Item2() + 1);
                    return sNew == Pair.Create(item1, item2);
                },
                Specification = Spec.Always(Spec.Eventually(Spec.Predicate<Pair<bool, uint>>(s => s.Item2() == 1))),
            };

            var searchResults = ts.ModelCheck(2000).ToArray();
            Assert.AreEqual(5, searchResults.Length);
            Assert.IsTrue(searchResults.Last().SearchOutcome == SearchOutcome.CounterExample);

            var counterExample = searchResults.Last().CounterExample;
            Assert.AreEqual(new Pair<bool, uint>(false, 0), counterExample[0]);
            Assert.AreEqual(new Pair<bool, uint>(false, 1), counterExample[1]);
            Assert.AreEqual(new Pair<bool, uint>(false, 2), counterExample[2]);
            Assert.AreEqual(new Pair<bool, uint>(true, 3), counterExample[3]);
            Assert.AreEqual(new Pair<bool, uint>(true, 0), counterExample[4]);
            Assert.AreEqual(new Pair<bool, uint>(true, 0), counterExample[5]);
        }

        /* /// <summary>
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
                Specification = Spec.Always(Spec.Predicate<uint>(s => s <= 200)),
            };

            var searchResults = ts.ModelCheck(2000, useKInduction: true).ToArray();

            Assert.AreEqual(2, searchResults.Length);
            Assert.IsTrue(searchResults[0].SearchOutcome == SearchOutcome.NoCounterExample);
            Assert.IsTrue(searchResults[0].CounterExample == null);
            Assert.IsTrue(searchResults[1].SearchOutcome == SearchOutcome.SafetyProof);
            Assert.IsTrue(searchResults[1].CounterExample == null);
        } */

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
                Specification = Spec.Always(Spec.Predicate<uint>(s => s <= 100000)),
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
                Specification = Spec.Always(Spec.Predicate<uint>(s => s <= 100000)),
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

        /* /// <summary>
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
                Specification = Spec.Always(Spec.Predicate<uint>(s => s <= 200)),
            };

            var searchResults = ts.ModelCheck(useKInduction: true).ToArray();

            Assert.AreEqual(2, searchResults.Length);
            Assert.IsTrue(searchResults[0].SearchOutcome == SearchOutcome.NoCounterExample);
            Assert.IsTrue(searchResults[0].CounterExample == null);
            Assert.IsTrue(searchResults[1].SearchOutcome == SearchOutcome.SafetyProof);
            Assert.IsTrue(searchResults[1].CounterExample == null);
        } */
    }
}
