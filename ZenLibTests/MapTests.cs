// <copyright file="MapTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
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
        /// Test map symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestMapDelete()
        {
            var zf = new ZenFunction<Map<int, int>, Map<int, int>>(d => d.Delete(10).Set(10, 1));

            var d = zf.Evaluate(new Map<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);

            d = zf.Evaluate(new Map<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);

            zf.Compile();
            d = zf.Evaluate(new Map<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);

            d = zf.Evaluate(new Map<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10).Value);
        }

        /// <summary>
        /// Test map symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestMapDeleteImplementation()
        {
            Assert.AreEqual(0, new Map<int, int>().Delete(10).Count());
            Assert.AreEqual(1, new Map<int, int>().Set(1, 10).Delete(10).Count());
            Assert.AreEqual(0, new Map<int, int>().Set(1, 10).Delete(1).Count());
            Assert.AreEqual(1, new Map<int, int>().Set(1, 10).Set(10, 10).Delete(1).Count());
        }

        /// <summary>
        /// Test that some basic map equations hold.
        /// </summary>
        [TestMethod]
        public void TestMapEquations()
        {
            CheckValid<Map<byte, byte>, byte, byte>((d, k, v) => d.Set(k, v).Delete(k) == d.Delete(k), runBdds: false);
            CheckValid<Map<byte, byte>, byte, byte>((d, k, v) => d.Delete(k).Set(k, v) == d.Set(k, v), runBdds: false);
            CheckValid<Map<byte, byte>, byte, byte>((d, k, v) => Implies(d.Get(k) == Option.Create(v), d.Set(k, v) == d), runBdds: false);
            CheckValid<Map<byte, byte>, byte, byte>((d, k, v) => Implies(d.Get(k).IsNone(), d.Delete(k) == d), runBdds: false);
        }

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
        public void TestMapGet()
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
                return And(
                    v1 == Option.Some("v1"),
                    v2 == Option.Some("v2"));
            });

            Assert.IsTrue(sat.HasValue);
            Assert.AreEqual("v1", sat.Value.Get("k1").Value);
            Assert.AreEqual("v2", sat.Value.Get("k2").Value);
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

        /// <summary>
        /// Test that the map works with pairs.
        /// </summary>
        [TestMethod]
        public void TestMapPairs()
        {
            var f = new ZenFunction<Map<int, Pair<int, bool>>, bool>(d => d.Get(1) == Option.Create(Pair.Create<int, bool>(2, true)));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual(2, sat.Value.Get(1).Value.Item1);
            Assert.AreEqual(true, sat.Value.Get(1).Value.Item2);
        }

        /// <summary>
        /// Test that the map works with chars.
        /// </summary>
        [TestMethod]
        public void TestMapChars()
        {
            var res = new ZenConstraint<Map<char, int>>(d => d.Get('a') == Option.Create<int>(1)).Find();
            Assert.AreEqual(1, res.Value.Get('a').Value);
        }

        /// <summary>
        /// Test that the map works with nested objects.
        /// </summary>
        [TestMethod]
        public void TestMapNestedObjects()
        {
            var f = new ZenFunction<Map<int, Pair<int, Pair<int, int>>>, bool>(d => d.Get(1) == Option.Create(Pair.Create<int, Pair<int, int>>(2, Pair.Create<int, int>(3, 4))));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual(2, sat.Value.Get(1).Value.Item1);
            Assert.AreEqual(3, sat.Value.Get(1).Value.Item2.Item1);
            Assert.AreEqual(4, sat.Value.Get(1).Value.Item2.Item2);
        }

        /// <summary>
        /// Test that the map works with objects with updates.
        /// </summary>
        [TestMethod]
        public void TestMapUpdatedObjects()
        {
            var f = new ZenFunction<Map<Pair<int, int>, int>, Map<Pair<int, int>, int>>(d =>
            {
                return d.Set(Pair.Create<int, int>(1, 2).WithField<Pair<int, int>, int>("Item1", 3), 1);
            });

            var key1 = new Pair<int, int> { Item1 = 1, Item2 = 2 };
            var key2 = new Pair<int, int> { Item1 = 3, Item2 = 2 };
            var sat = f.Find((d1, d2) => And(d1.Get(key1) == Option.None<int>(), d2.Get(key2) == Option.Some(1)));

            Assert.IsTrue(sat.HasValue);
            Assert.IsFalse(sat.Value.Get(key1).HasValue);
            Assert.IsTrue(f.Evaluate(sat.Value).Get(key2).HasValue);
        }

        /// <summary>
        /// Test that the map works with options.
        /// </summary>
        [TestMethod]
        public void TestMapOptions()
        {
            var f = new ZenFunction<Map<int, Option<int>>, bool>(d => d.Get(1) == Option.Create(Option.Create<int>(2)));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual(2, sat.Value.Get(1).Value.Value);
        }

        /// <summary>
        /// Test that the map works with options.
        /// </summary>
        [TestMethod]
        public void TestMapUnit()
        {
            var f = new ZenFunction<Map<int, Unit>, bool>(d => d.Get(1) == Option.Create<Unit>(new Unit()));
            var sat = f.Find((d, allowed) => allowed);
            Assert.IsFalse(sat.Value.Get(0).HasValue);
            Assert.IsTrue(sat.Value.Get(1).HasValue);
        }

        /// <summary>
        /// Test that all primitive types work with maps.
        /// </summary>
        [TestMethod]
        public void TestMapPrimitiveTypes()
        {
            Assert.IsTrue(new ZenConstraint<Map<bool, bool>>(m => m.Get(true).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<byte, byte>>(m => m.Get(1).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<short, short>>(m => m.Get(2).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<ushort, ushort>>(m => m.Get(3).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<int, int>>(m => m.Get(4).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<uint, uint>>(m => m.Get(5).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<long, long>>(m => m.Get(6).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<ulong, ulong>>(m => m.Get(7).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<BigInteger, BigInteger>>(m => m.Get(new BigInteger(8)).IsSome()).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Map<string, string>>(m => m.Get("9").IsSome()).Find().HasValue);
        }

        /// <summary>
        /// Test that sequence types do not work with maps.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMapNonPrimitiveTypesException2()
        {
            new ZenConstraint<Map<uint, FSeq<uint>>>(m => m.Get(10).IsSome()).Find();
        }

        /// <summary>
        /// Test map equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestMapEqualsHashcode()
        {
            var s1 = new Map<int, int>().Set(1, 2).Set(3, 4);
            var s2 = new Map<int, int>().Set(3, 4).Set(1, 2);
            var s3 = new Map<int, int>().Set(1, 2);
            var s4 = new Map<int, int>().Set(1, 2).Set(3, 5);
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.Equals((object)s2));
            Assert.IsFalse(s1.Equals(10));
            Assert.IsFalse(s1 == s3);
            Assert.IsFalse(s1 == s4);
            Assert.IsTrue(s1 != s3);
            Assert.IsTrue(s1.GetHashCode() != s3.GetHashCode());
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
        }

        /// <summary>
        /// Test that maps work as values in other maps.
        /// </summary>
        [TestMethod]
        public void TestMapWithMapValues1()
        {
            var f = new ZenFunction<Map<int, Map<int, int>>, bool>(d => And(d.Get(1).IsSome(), d.Get(1).Value().Get(2).IsSome()));
            var sat = f.Find((d, allowed) => allowed);

            Assert.IsTrue(sat.Value.ContainsKey(1));
            Assert.IsTrue(sat.Value.Get(1).Value.ContainsKey(2));
        }

        /// <summary>
        /// Test that map types do work with map values.
        /// </summary>
        [TestMethod]
        public void TestMapWithMapValues2()
        {
            var sat = new ZenConstraint<Map<uint, Map<uint, bool>>>(m => m.Get(10).IsSome()).Find();
            Assert.IsTrue(sat.Value.ContainsKey(10));
        }

        /// <summary>
        /// Test that map types do work with fixed integer values.
        /// </summary>
        [TestMethod]
        public void TestMapWithFixedIntegers()
        {
            var sat = new ZenConstraint<Map<uint, UInt3>>(m => m.Get(10).IsSome()).Find();
            Assert.IsTrue(sat.Value.ContainsKey(10));
        }

        /// <summary>
        /// Test that map types do work with sequence values.
        /// </summary>
        [TestMethod]
        public void TestMapWithSequences()
        {
            var sat = new ZenConstraint<Map<uint, Seq<long>>>(m => m.Get(10).IsSome()).Find();
            Assert.IsTrue(sat.Value.ContainsKey(10));
        }

        /// <summary>
        /// Test that map types do work with map keys.
        /// </summary>
        [TestMethod]
        public void TestMapWithMapKeys1()
        {
            var sat = new ZenConstraint<Map<Map<int, int>, int>>(m => m.Get(Map.Empty<int, int>()).IsSome()).Find();
            Assert.IsTrue(sat.Value.ContainsKey(new Map<int, int>()));
        }
    }
}
