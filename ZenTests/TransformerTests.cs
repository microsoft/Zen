// <copyright file="TransformerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.ZenTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Research.Zen;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Microsoft.Research.Zen.Language;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TransformerTests
    {
        /// <summary>
        /// Test a transformer with an abitrary works.
        /// </summary>
        [TestMethod]
        public void TestTransformerArbitrary()
        {
            var b = Arbitrary<bool>();
            var f1 = Function<uint, bool>(i => Or(b, i <= 10));
            var f2 = Function<bool, uint>(b => 3);
            var t1 = f1.Transformer();
            var t2 = f2.Transformer();
            var set1 = t1.InputSet();
            var set2 = t2.OutputSet();
            t1.TransformForward(set2);
        }

        /// <summary>
        /// Test weaving sets through combinations of transformers.
        /// </summary>
        [TestMethod]
        public void TestTransformerCombinations()
        {
            var f1 = Function<uint, bool>(i => i < 10);
            var f2 = Function<bool, uint>(b => If<uint>(b, 11, 9));
            var t1 = f1.Transformer();
            var t2 = f2.Transformer();
            var set1 = t1.InputSet((i, b) => b);
            var set2 = t1.TransformForward(set1);
            var set3 = t2.TransformForward(set2);
            var set4 = t2.OutputSet((b, i) => b);

            Assert.AreEqual(11U, set3.Element().Value);
            Assert.AreEqual(11U, set4.Element().Value);
        }

        /// <summary>
        /// Test getting the input sets for a transformer.
        /// </summary>
        [TestMethod]
        public void TestTransformerInputSets()
        {
            var f = Function<uint, uint>(i => i + 1);
            var t = f.Transformer();
            var inSet1 = t.InputSet((x, y) => y == 10);
            var inSet2 = t.InputSet((x, y) => y == 11);
            var inSet3 = inSet1.Intersect(inSet2);
            var inSet4 = inSet1.Union(inSet2);

            Assert.AreEqual(9U, inSet1.Element().Value);
            Assert.AreEqual(10U, inSet2.Element().Value);
            Assert.IsTrue(inSet3.IsEmpty());
            Assert.IsFalse(inSet4.IsEmpty());
        }

        /// <summary>
        /// Test getting the input sets for a transformer.
        /// </summary>
        [TestMethod]
        public void TestTransformerTransformForward()
        {
            var f = Function<uint, bool>(i => i >= 10);
            var t = f.Transformer();
            var inputSet1 = t.InputSet((i, b) => b);
            var outputSet = t.TransformForward(inputSet1);
            var inputSet2 = t.TransformBackwards(outputSet);

            Console.WriteLine(inputSet1.Element());
            Console.WriteLine(outputSet.Element());
            Console.WriteLine(inputSet2.Element());
            Console.WriteLine(inputSet2.Intersect(inputSet1.Complement()).Element());

            Assert.AreEqual(inputSet1, inputSet2);
        }

        /// <summary>
        /// Test getting the input sets for a transformer.
        /// </summary>
        [TestMethod]
        public void TestTransformerVariablesAlign()
        {
            var f1 = Function<uint, uint>(i => i + 1);
            var f2 = Function<uint, uint>(i => i + 2);
            var t1 = f1.Transformer();
            var t2 = f2.Transformer();
            var set1 = t1.InputSet((x, y) => y == 10);
            var set2 = t2.InputSet((x, y) => y == 11);
            var set3 = set1.Intersect(set2);
            Assert.AreEqual(set1, set2);
            Assert.AreEqual(9U, set3.Element().Value);
        }
    }
}