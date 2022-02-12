// <copyright file="SetTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen set type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SetTests
    {
        /// <summary>
        /// Test set symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestSetDelete()
        {
            var zf = new ZenFunction<Set<int>, Set<int>>(d => d.Delete(10).Add(10));

            var d = zf.Evaluate(new Set<int>().Add(10));
            Assert.AreEqual(1, d.Count());

            d = zf.Evaluate(new Set<int>());
            Assert.AreEqual(1, d.Count());

            zf.Compile();
            d = zf.Evaluate(new Set<int>().Add(10));
            Assert.AreEqual(1, d.Count());

            d = zf.Evaluate(new Set<int>());
            Assert.AreEqual(1, d.Count());
        }

        /// <summary>
        /// Test set symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestSetDeleteImplementation()
        {
            Assert.AreEqual(0, new Set<int>().Delete(10).Count());
            Assert.AreEqual(1, new Set<int>().Add(1).Delete(10).Count());
            Assert.AreEqual(0, new Set<int>().Add(1).Delete(1).Count());
            Assert.AreEqual(1, new Set<int>().Add(1).Add(10).Delete(1).Count());
        }

        /// <summary>
        /// Test that some basic set equations hold.
        /// </summary>
        [TestMethod]
        public void TestSetEquations()
        {
            CheckValid<Set<byte>, byte>((d, e) => d.Add(e).Delete(e) == d.Delete(e), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => d.Delete(e).Add(e) == d.Add(e), runBdds: false);
            // TODO: add this back when switching to unit type for map values!
            // CheckValid<Set<byte>, byte>((d, e) => Implies(d.Contains(e), d.Add(e) == d), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => Implies(Not(d.Contains(e)), d.Delete(e) == d), runBdds: false);
        }

        /// <summary>
        /// Test that set evaluation works.
        /// </summary>
        [TestMethod]
        public void TestSetEvaluation1()
        {
            var zf1 = new ZenFunction<Set<int>, Set<int>>(d => d.Add(10));
            var zf2 = new ZenFunction<Set<int>, bool>(d => d.Contains(10));
            var zf3 = new ZenFunction<Set<int>>(() => Set.Empty<int>());

            var result1 = zf1.Evaluate(new Set<int>().Add(5));
            Assert.AreEqual(2, result1.Count());
            Assert.IsTrue(result1.Contains(5));
            Assert.IsTrue(result1.Contains(10));

            zf1.Compile();
            result1 = zf1.Evaluate(new Set<int>().Add(5));
            Assert.AreEqual(2, result1.Count());
            Assert.IsTrue(result1.Contains(5));
            Assert.IsTrue(result1.Contains(10));

            var result2 = zf2.Evaluate(new Set<int>().Add(5));
            var result3 = zf2.Evaluate(new Set<int>().Add(10));
            var result4 = zf2.Evaluate(new Set<int>().Add(5).Add(10));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsTrue(result4);

            zf2.Compile();
            result2 = zf2.Evaluate(new Set<int>().Add(5));
            result3 = zf2.Evaluate(new Set<int>().Add(10));
            result4 = zf2.Evaluate(new Set<int>().Add(5).Add(10));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsTrue(result4);

            var result5 = zf3.Evaluate();
            Assert.AreEqual(0, result5.Count());

            zf3.Compile();
            result5 = zf3.Evaluate();
            Assert.AreEqual(0, result5.Count());
        }

        /// <summary>
        /// Test that set symbolic evaluation with equality and empty set.
        /// </summary>
        [TestMethod]
        public void TestSetEqualsEmpty()
        {
            var zf = new ZenConstraint<Set<int>>(d => d == Set.Empty<int>());
            var result = zf.Find();

            Assert.AreEqual(0, result.Value.Count());
        }

        /// <summary>
        /// Test symbolic evaluation with set.
        /// </summary>
        [TestMethod]
        public void TestSetAdd()
        {
            var zf = new ZenFunction<Set<int>, Set<int>>(d => d.Add(10));
            var result = zf.Find((d1, d2) => d2 == Set.Empty<int>());

            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Test set symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestSetContains()
        {
            var zf = new ZenConstraint<Set<int>>(d => d.Contains(10));
            var result = zf.Find();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(1, result.Value.Count());
            Assert.IsTrue(result.Value.Contains(10));
        }

        /// <summary>
        /// Test set symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestSetEquals()
        {
            CheckAgreement<Set<ushort>>(d => d.Add(1).Add(3) == Set.Empty<ushort>().Add(1), runBdds: false);
            CheckAgreement<Set<ushort>>(d => d.Add(1).Add(3) == Set.Empty<ushort>().Add(1).Add(3), runBdds: false);

            var zf1 = new ZenConstraint<Set<int>>(d => d.Add(1).Add(3) == Set.Empty<int>().Add(1));
            var zf2 = new ZenConstraint<Set<int>>(d => d.Add(1).Add(3) == Set.Empty<int>().Add(1).Add(3));

            Assert.IsFalse(zf1.Evaluate(new Set<int>()));
            Assert.IsTrue(zf2.Evaluate(new Set<int>()));

            zf1.Compile();
            zf2.Compile();
            Assert.IsFalse(zf1.Evaluate(new Set<int>()));
            Assert.IsTrue(zf2.Evaluate(new Set<int>()));
        }

        /// <summary>
        /// Test that If works with sets.
        /// </summary>
        [TestMethod]
        public void TestSetIte()
        {
            CheckValid<Set<long>, bool>((d, b) =>
                Implies(d == Set.Empty<long>(),
                        Not(If(b, d.Add(1), d.Add(2)).Contains(3))), runBdds: false);
        }

        /// <summary>
        /// Test that adding and then checking containment works.
        /// </summary>
        [TestMethod]
        public void TestAddThenContainsIsTrue()
        {
            CheckValid<Set<int>>(d => d.Add(1).Contains(1), runBdds: false);
        }

        /// <summary>
        /// Test that the empty set does not contain anything.
        /// </summary>
        [TestMethod]
        public void TestSetEmpty()
        {
            RandomBytes(x => CheckAgreement<Set<int>>(d => Not(Set.Empty<int>().Contains(x)), runBdds: false));
        }

        /// <summary>
        /// Test that the evaluation is working.
        /// </summary>
        [TestMethod]
        public void TestSetEvaluateInput()
        {
            CheckAgreement<Set<int>>(d => d.Contains(1), runBdds: false);

            var f = new ZenFunction<Set<int>, bool>(d => d.Contains(1));

            var d1 = new Set<int>().Add(1);
            Assert.AreEqual(true, f.Evaluate(d1));

            var d2 = new Set<int>().Add(2);
            Assert.AreEqual(false, f.Evaluate(d2));
        }

        /// <summary>
        /// Test that the set contains operation works.
        /// </summary>
        [TestMethod]
        public void TestSetContainsKey()
        {
            var d = new Set<int>().Add(1).Add(2).Add(1);

            Assert.IsTrue(d.Contains(1));
            Assert.IsTrue(d.Contains(2));
            Assert.IsFalse(d.Contains(3));
        }

        /// <summary>
        /// Test that the set works with strings.
        /// </summary>
        [TestMethod]
        public void TestSetStrings()
        {
            var f = new ZenFunction<Set<string>, bool>(d => true);
            var sat = f.Find((d, allowed) =>
            {
                var v1 = d.Contains("k1");
                var v2 = d.Contains("k2");
                var v3 = d.Contains("k3");
                return And(v1, v2, v3);
            });

            Assert.IsTrue(sat.HasValue);
            Assert.IsTrue(sat.Value.Contains("k1"));
            Assert.IsTrue(sat.Value.Contains("k2"));
            Assert.IsTrue(sat.Value.Contains("k3"));
        }

        /// <summary>
        /// Test that the set contains operation works.
        /// </summary>
        [TestMethod]
        public void TestSetContainsMissing()
        {
            var d = new Set<int>().Add(1).Add(2);
            Assert.IsFalse(d.Contains(3));
        }

        /// <summary>
        /// Test that the set tostring operation works.
        /// </summary>
        [TestMethod]
        public void TestSetToString()
        {
            var d = new Set<int>().Add(1).Add(2);
            Assert.AreEqual("{1, 2}", d.ToString());
        }

        /// <summary>
        /// Test that the set works with pairs.
        /// </summary>
        [TestMethod]
        public void TestSetPairs()
        {
            var f = new ZenFunction<Set<Pair<int, bool>>, bool>(d => d.Contains(Pair.Create<int, bool>(1, true)));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual("{(1, True)}", sat.Value.ToString());
        }

        /// <summary>
        /// Test that the set works with pairs.
        /// </summary>
        [TestMethod]
        public void TestSetNestedObjects()
        {
            var f = new ZenFunction<Set<Pair<int, Pair<int, int>>>, bool>(d => d.Contains(Pair.Create<int, Pair<int, int>>(2, Pair.Create<int, int>(3, 4))));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual(1, sat.Value.Count());
        }

        /// <summary>
        /// Test that the set works with options.
        /// </summary>
        [TestMethod]
        public void TestSetOptions()
        {
            var f = new ZenFunction<Set<Option<int>>, bool>(d => d.Contains(Option.Null<int>()));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual("{None}", sat.Value.ToString());
        }

        /// <summary>
        /// Test that the set works with options.
        /// </summary>
        [TestMethod]
        public void TestSetUnit()
        {
            var f = new ZenFunction<Set<Unit>, bool>(d => d.Contains(new Unit()));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual("{ZenLib.Unit}", sat.Value.ToString());
        }

        /// <summary>
        /// Test that all primitive types work with sets.
        /// </summary>
        [TestMethod]
        public void TestSetPrimitiveTypes()
        {
            Assert.IsTrue(new ZenConstraint<Set<bool>>(m => m.Contains(true)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<byte>>(m => m.Contains(1)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<short>>(m => m.Contains(2)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<ushort>>(m => m.Contains(3)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<int>>(m => m.Contains(4)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<uint>>(m => m.Contains(5)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<long>>(m => m.Contains(6)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<ulong>>(m => m.Contains(7)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<BigInteger>>(m => m.Contains(new BigInteger(8))).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<string>>(m => m.Contains("9")).Find().HasValue);
        }

        /// <summary>
        /// Test that non-primitive types do not work with sets.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSetNonPrimitiveTypesException1()
        {
            new ZenConstraint<Set<Set<uint>>>(m => m.Contains(Set.Empty<uint>())).Find();
        }

        /// <summary>
        /// Test that set works with fixed integers.
        /// </summary>
        [TestMethod]
        public void TestSetWithFixedInteger1()
        {
            var zf = new ZenConstraint<Set<UInt3>>(d => d.Contains(new UInt3(2)));
            var result = zf.Find();

            Assert.AreEqual(1, result.Value.Count());
            Assert.IsTrue(result.Value.Contains(new UInt3(2)));
        }

        /// <summary>
        /// Test that set works with fixed integers.
        /// </summary>
        [TestMethod]
        public void TestSetWithFixedInteger2()
        {
            var zf = new ZenConstraint<Set<Int10>>(d => d.Contains(new Int10(-2)));
            var result = zf.Find();

            Assert.AreEqual(1, result.Value.Count());
            Assert.IsTrue(result.Value.Contains(new Int10(-2)));
        }
    }
}
