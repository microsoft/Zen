﻿// <copyright file="FMapTests.cs" company="Microsoft">
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
        /// Test that adding and then getting an element returns that element.
        /// </summary>
        [TestMethod]
        public void TestAddThenGetIsEqual()
        {
            CheckValid<FMap<int, int>>(d => d.Set(1, 1).Get(1).Value() == 1);
        }

        /// <summary>
        /// Check that adding to a dictionary evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestDictionaryEvaluation()
        {
            var f = new ZenFunction<FMap<int, int>, FMap<int, int>>(d => d.Set(1, 1).Set(2, 2));
            var result = f.Evaluate(new FMap<int, int>());
            Assert.AreEqual(result.Get(1), 1);
            Assert.AreEqual(result.Get(2), 2);
        }

        /// <summary>
        /// Test that the empty dictionary does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestDictionaryEmpty()
        {
            RandomBytes(x => CheckAgreement<FMap<int, int>>(d => Not(FMap.Empty<int, int>().Get(x).IsSome())));
        }

        /// <summary>
        /// Test that the empty dictionary does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestDictionaryEvaluateOutput()
        {
            var f = new ZenFunction<int, int, FMap<int, int>>((x, y) => FMap.Empty<int, int>().Set(x, y));
            var d = f.Evaluate(1, 2);
            // Assert.AreEqual(1, d.Count);
            Assert.AreEqual(2, d.Get(1));

            f.Compile();
            d = f.Evaluate(1, 2);
            Assert.AreEqual(2, d.Get(1));
        }

        /// <summary>
        /// Test that the empty dictionary does not return anything for get.
        /// </summary>
        [TestMethod]
        public void TestDictionaryEvaluateInput()
        {
            CheckAgreement<FMap<int, int>>(d => d.ContainsKey(1));

            var f = new ZenFunction<FMap<int, int>, bool>(d => d.ContainsKey(1));

            var d1 = new FMap<int, int>();
            d1.Set(1, 2);
            Assert.AreEqual(true, f.Evaluate(d1));

            var d2 = new FMap<int, int>();
            d2.Set(2, 1);
            Assert.AreEqual(false, f.Evaluate(d2));
        }

        /// <summary>
        /// Test that the dictionary works with strings.
        /// </summary>
        [TestMethod]
        public void TestDictionaryStrings()
        {
            var f = new ZenFunction<FMap<string, string>, bool>(d => true);
            var sat = f.Find((d, allowed) =>
            {
                return And(
                    d.Get("k1").Value() == "v1",
                    d.Get("k2").Value() == "v2",
                    d.Get("k3").Value() == "v3");
            });

            Assert.IsTrue(sat.HasValue);
            Assert.AreEqual("v1", sat.Value.Get("k1"));
            Assert.AreEqual("v2", sat.Value.Get("k2"));
            Assert.AreEqual("v3", sat.Value.Get("k3"));
        }

        /// <summary>
        /// Test that the dictionary get operation works.
        /// </summary>
        [TestMethod]
        public void TestDictionaryGet()
        {
            var d = new FMap<int, int>();
            d.Set(1, 2);
            d.Set(2, 3);
            d.Set(1, 4);

            Assert.IsTrue(d.ContainsKey(1));
            Assert.IsTrue(d.ContainsKey(2));
            Assert.IsFalse(d.ContainsKey(3));
        }

        /// <summary>
        /// Test that the dictionary get operation works.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.IndexOutOfRangeException))]
        public void TestDictionaryGetException()
        {
            var d = new FMap<int, int>();
            d.Set(1, 2);
            d.Set(2, 3);
            d.Get(3);
        }

        /// <summary>
        /// Test that the dictionary tostring operation works.
        /// </summary>
        [TestMethod]
        public void TestDictionaryToString()
        {
            var d = new FMap<int, int>();
            d.Set(1, 2);
            d.Set(2, 3);

            Assert.AreEqual("{2 => 3, 1 => 2}", d.ToString());
        }
    }
}
