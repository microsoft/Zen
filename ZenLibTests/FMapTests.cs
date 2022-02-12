// <copyright file="FMapTests.cs" company="Microsoft">
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
    public class FMapTests
    {
        /// <summary>
        /// Test map symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestMapDelete()
        {
            var zf = new ZenFunction<FMap<int, int>, FMap<int, int>>(d => d.Delete(10).Set(10, 1));

            var d = zf.Evaluate(new FMap<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);

            d = zf.Evaluate(new FMap<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);

            zf.Compile();
            d = zf.Evaluate(new FMap<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);

            d = zf.Evaluate(new FMap<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);
        }

        /// <summary>
        /// Test map symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestMapDeleteImplementation()
        {
            Assert.AreEqual(0, new FMap<int, int>().Delete(10).Count());
            Assert.AreEqual(1, new FMap<int, int>().Set(1, 10).Delete(10).Count());
            Assert.AreEqual(0, new FMap<int, int>().Set(1, 10).Delete(1).Count());
            Assert.AreEqual(1, new FMap<int, int>().Set(1, 10).Set(10, 10).Delete(1).Count());
        }

        /// <summary>
        /// Test that some basic map equations hold.
        /// </summary>
        [TestMethod]
        public void TestMapEquations()
        {
            CheckValid<FMap<byte, byte>, byte, byte>((d, k, v) => d.Set(k, v).Delete(k) == d.Delete(k), runBdds: false);
            // CheckValid<FMap<byte, byte>, byte, byte>((d, k, v) => d.Delete(k).Set(k, v) == d.Set(k, v), runBdds: false);
            // CheckValid<FMap<byte, byte>, byte, byte>((d, k, v) => Implies(d.Get(k) == Option.Create(v), d.Set(k, v) == d), runBdds: false);
            CheckValid<FMap<byte, byte>, byte, byte>((d, k, v) => Implies(d.Get(k).IsNone(), d.Delete(k) == d), runBdds: false);
        }

        /// <summary>
        /// Test that map evaluation works.
        /// </summary>
        [TestMethod]
        public void TestMapEvaluation1()
        {
            var zf1 = new ZenFunction<FMap<int, int>, FMap<int, int>>(d => d.Set(10, 20));
            var zf2 = new ZenFunction<FMap<int, int>, bool>(d => d.Get(10) == Option.Some(11));
            var zf3 = new ZenFunction<FMap<int, int>>(() => FMap.Empty<int, int>());

            var result1 = zf1.Evaluate(new FMap<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5).Value);
            Assert.AreEqual(20, result1.Get(10).Value);

            zf1.Compile();
            result1 = zf1.Evaluate(new FMap<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5).Value);
            Assert.AreEqual(20, result1.Get(10).Value);

            var result2 = zf2.Evaluate(new FMap<int, int>().Set(5, 5));
            var result3 = zf2.Evaluate(new FMap<int, int>().Set(10, 10));
            var result4 = zf2.Evaluate(new FMap<int, int>().Set(5, 5).Set(10, 11));
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);

            zf2.Compile();
            result2 = zf2.Evaluate(new FMap<int, int>().Set(5, 5));
            result3 = zf2.Evaluate(new FMap<int, int>().Set(10, 10));
            result4 = zf2.Evaluate(new FMap<int, int>().Set(5, 5).Set(10, 11));
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
            var f = new ZenFunction<FMap<int, int>, FMap<int, int>>(d => d.Set(1, 1).Set(2, 2));

            var result = f.Evaluate(new FMap<int, int>());
            Assert.AreEqual(1, result.Get(1).Value);
            Assert.AreEqual(2, result.Get(2).Value);

            f.Compile();
            result = f.Evaluate(new FMap<int, int>());
            Assert.AreEqual(1, result.Get(1).Value);
            Assert.AreEqual(2, result.Get(2).Value);
        }

        /// <summary>
        /// Test that map symbolic evaluation with equality and empty map.
        /// </summary>
        [TestMethod]
        public void TestMapEqualsEmpty()
        {
            var zf = new ZenConstraint<FMap<int, int>>(d => d == FMap.Empty<int, int>());
            var result = zf.Find();

            Assert.AreEqual(0, result.Value.Count());
        }

        /// <summary>
        /// Test map symbolic evaluation with set.
        /// </summary>
        [TestMethod]
        public void TestMapSet()
        {
            var zf = new ZenFunction<FMap<int, int>, FMap<int, int>>(d => d.Set(10, 20));
            var result = zf.Find((d1, d2) => d2 == FMap.Empty<int, int>());

            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Test map symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestMapGet1()
        {
            var zf = new ZenConstraint<FMap<int, int>>(d => d.Get(10) == Option.Some(11));
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
            CheckAgreement<FMap<ushort, long>>(d => d.Set(1, 2).Set(3, 4) == FMap.Empty<ushort, long>().Set(1, 2), runBdds: false);
            CheckAgreement<FMap<ushort, long>>(d => d.Set(1, 2).Set(3, 4) == FMap.Empty<ushort, long>().Set(1, 2).Set(3, 4), runBdds: false);

            var zf1 = new ZenConstraint<FMap<int, int>>(d => d.Set(1, 2).Set(3, 4) == FMap.Empty<int, int>().Set(1, 2));
            var zf2 = new ZenConstraint<FMap<int, int>>(d => d.Set(1, 2).Set(3, 4) == FMap.Empty<int, int>().Set(1, 2).Set(3, 4));

            Assert.IsFalse(zf1.Evaluate(new FMap<int, int>()));
            Assert.IsTrue(zf2.Evaluate(new FMap<int, int>()));

            zf1.Compile();
            zf2.Compile();
            Assert.IsFalse(zf1.Evaluate(new FMap<int, int>()));
            Assert.IsTrue(zf2.Evaluate(new FMap<int, int>()));
        }

        /// <summary>
        /// Test that If works with maps.
        /// </summary>
        [TestMethod]
        public void TestMapIte()
        {
            CheckValid<FMap<long, long>, bool>((d, b) =>
                Implies(d == FMap.Empty<long, long>(),
                        If(b, d.Set(1, 1), d.Set(2, 2)).Get(3).IsNone()), runBdds: false);
        }

        /// <summary>
        /// Test that adding and then getting an element returns that element.
        /// </summary>
        [TestMethod]
        public void TestAddThenGetIsEqual()
        {
            CheckValid<FMap<int, int>>(d => d.Set(1, 1).Get(1).Value() == 1, runBdds: false);
        }

        /// <summary>
        /// Test that the empty map does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestMapEmpty()
        {
            RandomBytes(x => CheckAgreement<FMap<int, int>>(d => Not(Map.Empty<int, int>().Get(x).IsSome()), runBdds: false));
        }

        /// <summary>
        /// Test that map get and set return the right values.
        /// </summary>
        [TestMethod]
        public void TestMapGetAndSet()
        {
            CheckAgreement<FMap<ushort, long>>(d => d.Set(1, 2).Set(3, 4).Get(5).IsSome(), runBdds: false);
            RandomBytes(x => CheckAgreement<FMap<ushort, long>>(d => d.Set(1, 2).Set(3, 4).Get(x).IsSome(), runBdds: false));
        }

        /// <summary>
        /// Test that the empty map does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestMapEvaluateOutput()
        {
            var f = new ZenFunction<int, int, FMap<int, int>>((x, y) => FMap.Empty<int, int>().Set(x, y));
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
            CheckAgreement<FMap<int, int>>(d => d.ContainsKey(1), runBdds: false);

            var f = new ZenFunction<FMap<int, int>, bool>(d => d.ContainsKey(1));

            var d1 = new FMap<int, int>().Set(1, 2);
            Assert.AreEqual(true, f.Evaluate(d1));

            var d2 = new FMap<int, int>().Set(2, 1);
            Assert.AreEqual(false, f.Evaluate(d2));
        }

        /// <summary>
        /// Test that the map get operation works.
        /// </summary>
        [TestMethod]
        public void TestMapContainsKey()
        {
            var d = new FMap<int, int>().Set(1, 2).Set(2, 3).Set(1, 4);

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
            var f = new ZenFunction<FMap<string, string>, bool>(d => true);
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
            var d = new FMap<int, int>().Set(1, 2).Set(2, 3);
            Assert.IsFalse(d.Get(3).HasValue);
        }

        /// <summary>
        /// Test that the map tostring operation works.
        /// </summary>
        [TestMethod]
        public void TestMapToString()
        {
            var d = new FMap<int, int>().Set(1, 2).Set(2, 3);
            Assert.AreEqual("{2 => 3, 1 => 2}", d.ToString());
        }

        /// <summary>
        /// Test that the map works with pairs.
        /// </summary>
        [TestMethod]
        public void TestMapPairs()
        {
            var f = new ZenFunction<FMap<int, Pair<int, int>>, bool>(d => d.Get(1) == Option.Create(Pair.Create<int, int>(2, 3)));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual(2, sat.Value.Get(1).Value.Item1);
            Assert.AreEqual(3, sat.Value.Get(1).Value.Item2);
        }

        /// <summary>
        /// Test that the map works with options.
        /// </summary>
        [TestMethod]
        public void TestMapOptions()
        {
            var f = new ZenFunction<FMap<int, Option<int>>, bool>(d => d.Get(1) == Option.Create(Option.Create<int>(2)));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual(2, sat.Value.Get(1).Value.Value);
        }

        /// <summary>
        /// Test that the map works with options.
        /// </summary>
        [TestMethod]
        public void TestMapUnit()
        {
            var f = new ZenFunction<FMap<int, Unit>, bool>(d => d.Get(1) == Option.Create<Unit>(new Unit()));
            var sat = f.Find((d, allowed) => allowed);
            Assert.IsFalse(sat.Value.Get(0).HasValue);
            Assert.IsTrue(sat.Value.Get(1).HasValue);
        }
    }
}
