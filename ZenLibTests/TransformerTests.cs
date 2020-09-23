// <copyright file="TransformerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.ModelChecking;
    using ZenLib.Tests.Network;
    using static ZenLib.Language;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TransformerTests
    {
        /// <summary>
        /// Test a transformer fails for unbounded types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerExceptionForUnboundedTypes1()
        {
            Function<BigInteger, bool>(i => i > new BigInteger(0)).Transformer();
        }

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

        /// <summary>
        /// Test checking if a set is full.
        /// </summary>
        [TestMethod]
        public void TestTransformerSetIsFull()
        {
            var t = Function<bool, bool>(b => true).Transformer();
            var set = t.InputSet();

            Assert.IsTrue(set.IsFull());
        }

        /// <summary>
        /// Test checking if a set is full.
        /// </summary>
        [TestMethod]
        public void TestTransformerSetIsEmpty()
        {
            var t = Function<bool, bool>(b => true).Transformer();
            var set = t.InputSet().Complement();

            Assert.IsTrue(set.IsEmpty());
        }

        /// <summary>
        /// Test for different types.
        /// </summary>
        [TestMethod]
        public void TestTransformerArgTypes()
        {
            Assert.IsTrue(Function<bool, bool>(b => true).Transformer().InputSet().IsFull());
            Assert.IsTrue(Function<byte, bool>(b => true).Transformer().InputSet().IsFull());
            Assert.IsTrue(Function<short, bool>(b => true).Transformer().InputSet().IsFull());
            Assert.IsTrue(Function<ushort, bool>(b => true).Transformer().InputSet().IsFull());
            Assert.IsTrue(Function<int, bool>(b => true).Transformer().InputSet().IsFull());
            Assert.IsTrue(Function<uint, bool>(b => true).Transformer().InputSet().IsFull());
            Assert.IsTrue(Function<long, bool>(b => true).Transformer().InputSet().IsFull());
            Assert.IsTrue(Function<ulong, bool>(b => true).Transformer().InputSet().IsFull());
        }

        /// <summary>
        /// Test state set equality.
        /// </summary>
        [TestMethod]
        public void TestTransformerSetEquality()
        {
            var t1 = Function<bool, bool>(b => true).Transformer();
            var t2 = Function<bool, bool>(b => true).Transformer();

            var set1 = t1.InputSet((x, y) => Not(x));
            var set2 = t2.InputSet((x, y) => Not(x));

            Assert.IsTrue(set1.Equals(set2));
            Assert.IsFalse(set1.Equals(2));
        }

        /// <summary>
        /// Test getting an element for an empty set.
        /// </summary>
        [TestMethod]
        public void TestTransformerNoElement()
        {
            var f = Function<uint, uint>(i => i + 1);
            var t = f.Transformer();
            var emptySet = t.InputSet((x, y) => x + 2 == y);

            Assert.IsFalse(emptySet.Element().HasValue);
        }

        /// <summary>
        /// Test a transformer over an object.
        /// </summary>
        [TestMethod]
        public void TestTransformerObject()
        {
            var f = Function<IpHeader, bool>(p => And(p.GetDstIp().GetValue() <= 4, p.GetSrcIp().GetValue() <= 5));
            var t = f.Transformer();

            /* var set = t.InputSet((p, b) => Not(b));
            Assert.IsFalse(set.Element().Value.DstIp.Value <= 4 && set.Element().Value.SrcIp.Value <= 5);

            var outputSet = t.TransformForward(set);
            Assert.AreEqual(false, outputSet.Element().Value);

            var inputSet = t.TransformBackwards(outputSet);
            Assert.IsFalse(set.Element().Value.DstIp.Value <= 4 && set.Element().Value.SrcIp.Value <= 5); */
        }

        /// <summary>
        /// Test packet transformations.
        /// </summary>
        [TestMethod]
        public void TestPacketSet()
        {
            var rnd = new System.Random();
            for (int j = 0; j < 2; j++)
            {
                var i = (uint)rnd.Next();
                var f = Function<IpHeader, bool>(p =>
                {
                    return p.GetDstIp().GetValue() == i;
                });

                StateSetTransformer<IpHeader, bool> transformer = f.Transformer();
                var set = transformer.InputSet((pkt, matches) => matches);
                Assert.AreEqual(i, set.Element().Value.DstIp.Value);
            }
        }

        /// <summary>
        /// Test packet transformations.
        /// </summary>
        [TestMethod]
        public void TestMultipleTransformers()
        {
            var t1 = Function<IpHeader, bool>(p => true).Transformer();
            var set1 = t1.InputSet((p, v) => v);
            var t2 = Function<IpHeader, bool>(p => p.GetDstIp().GetValue() == 1).Transformer();
            var set2 = t2.InputSet((p, v) => v);
            var t3 = Function<uint, bool>(u => u == 2).Transformer();
            var set3 = t3.InputSet((u, v) => v);
            Assert.IsTrue(set1.IsFull());
            Assert.AreEqual(1U, set2.Element().Value.DstIp.Value);
            Assert.AreEqual(2U, set3.Element().Value);
        }

        /// <summary>
        /// Test that multiple arguments works with tuples.
        /// </summary>
        [TestMethod]
        public void TestMultipleArguments()
        {
            var t = Function<(IpHeader, IpHeader), bool>(x => x.Item1() == x.Item2()).Transformer();
            var set = t.InputSet((x, b) => b);
            var e = set.Element();
            Assert.AreEqual(e.Value.Item1.DstIp, e.Value.Item2.DstIp);
            Assert.AreEqual(e.Value.Item1.SrcIp, e.Value.Item2.SrcIp);
            Assert.AreEqual(e.Value.Item1.SrcPort, e.Value.Item2.DstPort);
            Assert.AreEqual(e.Value.Item1.DstPort, e.Value.Item2.DstPort);
        }

        /// <summary>
        /// Test that multiple arguments works with tuples.
        /// </summary>
        [TestMethod]
        public void TestTransformerFieldAccesses()
        {
            var t1 = Function<IpHeader, Ip>(x => x.GetDstIp()).Transformer();
            var t2 = Function<IpHeader, Ip>(x => x.GetSrcIp()).Transformer();
            var t3 = Function<IpHeader, ushort>(x => x.GetDstPort()).Transformer();
            var t4 = Function<IpHeader, ushort>(x => x.GetSrcPort()).Transformer();
            var t5 = Function<IpHeader, byte>(x => x.GetProtocol()).Transformer();

            var allHeaders = Function<IpHeader, bool>(x => true).Transformer().InputSet();
            var s1 = t1.InputSet((x, ip) => true);
            var s2 = t2.InputSet((x, ip) => true);
            var s3 = t3.InputSet((x, ip) => true);
            var s4 = t4.InputSet((x, ip) => true);
            var s5 = t5.InputSet((x, ip) => true);
            var all = s1.Intersect(s2).Intersect(s3).Intersect(s4).Intersect(s5);
            Assert.AreEqual(allHeaders, all);
        }
    }
}