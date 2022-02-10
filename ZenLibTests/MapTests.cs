// <copyright file="MapTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen dictionary type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MapTests
    {
        /// <summary>
        /// Test that map evaluation works.
        /// </summary>
        [TestMethod]
        public void TestMapEvaluation1()
        {
            var zf1 = new ZenFunction<Map<int, int>, Map<int, int>>(d => d.Set(10, 20));
            var zf2 = new ZenFunction<Map<int, int>, bool>(d => d.Get(10) == Option.Some(11));
            var zf3 = new ZenFunction<Map<int, int>>(() => Map.Empty<int, int>());

            var result1 = zf1.Evaluate(new Map<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5).Value);
            Assert.AreEqual(20, result1.Get(10).Value);

            zf1.Compile();
            result1 = zf1.Evaluate(new Map<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5).Value);
            Assert.AreEqual(20, result1.Get(10).Value);

            var result2 = zf2.Evaluate(new Map<int, int>().Set(5, 5));
            var result3 = zf2.Evaluate(new Map<int, int>().Set(10, 10));
            var result4 = zf2.Evaluate(new Map<int, int>().Set(5, 5).Set(10, 11));
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);

            zf2.Compile();
            result2 = zf2.Evaluate(new Map<int, int>().Set(5, 5));
            result3 = zf2.Evaluate(new Map<int, int>().Set(10, 10));
            result4 = zf2.Evaluate(new Map<int, int>().Set(5, 5).Set(10, 11));
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);

            var result5 = zf3.Evaluate();
            Assert.AreEqual(0, result5.Count());

            zf3.Compile();
            result5 = zf3.Evaluate();
            Assert.AreEqual(0, result5.Count());
        }

        /// <summary>
        /// Check that adding to a dictionary evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestMapEvaluation2()
        {
            var f = new ZenFunction<Map<int, int>, Map<int, int>>(d => d.Set(1, 1).Set(2, 2));

            var result = f.Evaluate(new Map<int, int>());
            Assert.AreEqual(1, result.Get(1).Value);
            Assert.AreEqual(2, result.Get(2).Value);

            f.Compile();
            result = f.Evaluate(new Map<int, int>());
            Assert.AreEqual(1, result.Get(1).Value);
            Assert.AreEqual(2, result.Get(2).Value);
        }

        /// <summary>
        /// Test that map symbolic evaluation with equality and empty map.
        /// </summary>
        [TestMethod]
        public void TestMapEqualsEmpty()
        {
            var zf = new ZenConstraint<Map<int, int>>(d => d == Map.Empty<int, int>());
            var result = zf.Find();

            Assert.AreEqual(0, result.Value.Count());
        }

        /// <summary>
        /// Test map symbolic evaluation with set.
        /// </summary>
        [TestMethod]
        public void TestMapSet()
        {
            var zf = new ZenFunction<Map<int, int>, Map<int, int>>(d => d.Set(10, 20));
            var result = zf.Find((d1, d2) => d2 == Map.Empty<int, int>());

            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Test map symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestMapGet1()
        {
            var zf = new ZenConstraint<Map<int, int>>(d => d.Get(10) == Option.Some(11));
            var result = zf.Find();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(11, result.Value.Get(10).Value);
        }

        /// <summary>
        /// Test map symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestMapEquals()
        {
            CheckAgreement<Map<ushort, long>>(d => d.Set(1, 2).Set(3, 4) == Map.Empty<ushort, long>().Set(1, 2), runBdds: false);
            CheckAgreement<Map<ushort, long>>(d => d.Set(1, 2).Set(3, 4) == Map.Empty<ushort, long>().Set(1, 2).Set(3, 4), runBdds: false);

            var zf1 = new ZenConstraint<Map<int, int>>(d => d.Set(1, 2).Set(3, 4) == Map.Empty<int, int>().Set(1, 2));
            var zf2 = new ZenConstraint<Map<int, int>>(d => d.Set(1, 2).Set(3, 4) == Map.Empty<int, int>().Set(1, 2).Set(3, 4));

            Assert.IsFalse(zf1.Evaluate(new Map<int, int>()));
            Assert.IsTrue(zf2.Evaluate(new Map<int, int>()));

            zf1.Compile();
            zf2.Compile();
            Assert.IsFalse(zf1.Evaluate(new Map<int, int>()));
            Assert.IsTrue(zf2.Evaluate(new Map<int, int>()));
        }

        /// <summary>
        /// Test that If works with maps.
        /// </summary>
        [TestMethod]
        public void TestMapIte()
        {
            CheckValid<Map<long, long>, bool>((d, b) =>
                Implies(d == Map.Empty<long, long>(),
                        If(b, d.Set(1, 1), d.Set(2, 2)).Get(3).IsNone()), runBdds: false);
        }

        /// <summary>
        /// Test that adding and then getting an element returns that element.
        /// </summary>
        [TestMethod]
        public void TestAddThenGetIsEqual()
        {
            CheckValid<Map<int, int>>(d => d.Set(1, 1).Get(1).Value() == 1, runBdds: false);
        }

        /// <summary>
        /// Test that the empty map does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestMapEmpty()
        {
            RandomBytes(x => CheckAgreement<Map<int, int>>(d => Not(Map.Empty<int, int>().Get(x).IsSome()), runBdds: false));
        }

        /// <summary>
        /// Test that map get and set return the right values.
        /// </summary>
        [TestMethod]
        public void TestMapGetAndSet()
        {
            CheckAgreement<Map<ushort, long>>(d => d.Set(1, 2).Set(3, 4).Get(5).IsSome(), runBdds: false);
            RandomBytes(x => CheckAgreement<Map<ushort, long>>(d => d.Set(1, 2).Set(3, 4).Get(x).IsSome(), runBdds: false));
        }

        /// <summary>
        /// Test that the empty map does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestMapEvaluateOutput()
        {
            var f = new ZenFunction<int, int, Map<int, int>>((x, y) => Map.Empty<int, int>().Set(x, y));
            var d = f.Evaluate(1, 2);
            Assert.AreEqual(2, d.Get(1).Value);

            f.Compile();
            d = f.Evaluate(1, 2);
            Assert.AreEqual(2, d.Get(1).Value);
        }

        /// <summary>
        /// Test that the empty map does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestMapEvaluateInput()
        {
            CheckAgreement<Map<int, int>>(d => d.ContainsKey(1), runBdds: false);

            var f = new ZenFunction<Map<int, int>, bool>(d => d.ContainsKey(1));

            var d1 = new Map<int, int>().Set(1, 2);
            Assert.AreEqual(true, f.Evaluate(d1));

            var d2 = new Map<int, int>().Set(2, 1);
            Assert.AreEqual(false, f.Evaluate(d2));
        }

        /// <summary>
        /// Test that the map get operation works.
        /// </summary>
        [TestMethod]
        public void TestMapContainsKey()
        {
            var d = new Map<int, int>().Set(1, 2).Set(2, 3).Set(1, 4);

            Assert.IsTrue(d.ContainsKey(1));
            Assert.IsTrue(d.ContainsKey(2));
            Assert.IsFalse(d.ContainsKey(3));
        }

        /// <summary>
        /// Test that the map works with strings.
        /// </summary>
        [TestMethod]
        public void TestMapStrings()
        {
            var f = new ZenFunction<Map<string, string>, bool>(d => true);
            var sat = f.Find((d, allowed) =>
            {
                var v1 = d.Get("k1");
                var v2 = d.Get("k2");
                var v3 = d.Get("k3");
                return And(
                    v1 == Option.Some("v1"),
                    v2 == Option.Some("v2"),
                    v3 == Option.Some("v3"));
            });

            Assert.IsTrue(sat.HasValue);
            Assert.AreEqual("v1", sat.Value.Get("k1").Value);
            Assert.AreEqual("v2", sat.Value.Get("k2").Value);
            Assert.AreEqual("v3", sat.Value.Get("k3").Value);
        }

        /// <summary>
        /// Test that the map get operation works.
        /// </summary>
        [TestMethod]
        public void TestMapGetMissing()
        {
            var d = new Map<int, int>().Set(1, 2).Set(2, 3);
            Assert.IsFalse(d.Get(3).HasValue);
        }

        /// <summary>
        /// Test that the map tostring operation works.
        /// </summary>
        [TestMethod]
        public void TestMapToString()
        {
            var d = new Map<int, int>().Set(1, 2).Set(2, 3);
            Assert.AreEqual("{1 => 2, 2 => 3}", d.ToString());
        }
    }
}
