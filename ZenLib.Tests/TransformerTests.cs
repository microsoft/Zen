﻿// <copyright file="TransformerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.ModelChecking;
    using ZenLib.Tests.Network;
    using static ZenLib.Zen;

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
            new ZenFunction<BigInteger, bool>(i => i > new BigInteger(0)).Transformer();
        }

        /// <summary>
        /// Test a transformer fails for unbounded types.
        /// </summary>
        [TestMethod]
        public void TestTransformerExceptionForUnboundedTypes2()
        {
            ZenSettings.UseLargeStack = true;

            try
            {
                new ZenFunction<BigInteger, bool>(i => i > new BigInteger(0)).Transformer();
                Assert.Fail();
            }
            catch
            {
                ZenSettings.UseLargeStack = false;
            }
        }

        class Obj
        {
            public Seq<int> Value { get; set; }
        }

        /// <summary>
        /// Test a transformer fails for unbounded types.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerExceptionForUnboundedTypes3()
        {
            var x = Zen.If(Symbolic<bool>(), Zen.Create<Obj>(("Value", Seq.Empty<int>())), Symbolic<Obj>());
            new ZenFunction<Obj, Obj>(o => x).Transformer();
        }

        /// <summary>
        /// Test a transformer with an abitrary works.
        /// </summary>
        [TestMethod]
        public void TestTransformerArbitrary()
        {
            var manager = new StateSetTransformerManager(0);
            var b = Arbitrary<bool>();
            var f1 = new ZenFunction<uint, bool>(i => Or(b, i <= 10));
            var f2 = new ZenFunction<bool, uint>(b => 3);
            var t1 = f1.Transformer(manager);
            var t2 = f2.Transformer(manager);
            var set1 = t1.InputSet((i, o) => o);
            var set2 = t2.OutputSet((i, o) => o == 3);
            t1.TransformForward(set2);
        }

        /// <summary>
        /// Test a transformer with an abitrary works.
        /// </summary>
        [TestMethod]
        public void TestTransformerArbitraryLocal()
        {
            var manager = new StateSetTransformerManager(0);

            var inputs = Zen.StateSet<Pair<uint, uint, uint>>(p =>
                Zen.Or(
                    Zen.And(p.Item1() == 6, p.Item2() == 1, p.Item3() == 44),
                    Zen.And(p.Item1() == 6, p.Item2() == 2, p.Item3() == 55)), manager);
            Assert.AreEqual(new Pair<uint, uint, uint>(6, 1, 44), inputs.Element());

            var transformer = Zen.Transformer<Pair<uint, uint, uint>, Pair<uint, uint, uint>>(p => Pair.Create(p.Item1(), p.Item2(), Zen.Arbitrary<uint>()), manager);
            var results = transformer.TransformForward(inputs);
            Assert.AreEqual(new Pair<uint, uint, uint>(6, 1, 0), results.Element());

            var postInputs = Zen.StateSet<Pair<uint, uint, uint>>(p => p.Item3() >= 11, manager);
            results = results.Intersect(postInputs);
            Assert.AreEqual(new Pair<uint, uint, uint>(6, 1, 11), results.Element());

            var backwards = transformer.TransformBackwards(results);
            Assert.AreEqual(new Pair<uint, uint, uint>(6, 1, 0), backwards.Element());

            var init = backwards.Intersect(inputs);
            Assert.AreEqual(new Pair<uint, uint, uint>(6, 1, 44), inputs.Element());
        }

        /// <summary>
        /// Test weaving sets through combinations of transformers.
        /// </summary>
        [TestMethod]
        public void TestTransformerCombinations()
        {
            var manager = new StateSetTransformerManager(0);
            var f1 = new ZenFunction<uint, bool>(i => i < 10);
            var f2 = new ZenFunction<bool, uint>(b => If<uint>(b, 11, 9));
            var t1 = f1.Transformer(manager);
            var t2 = f2.Transformer(manager);
            var set1 = t1.InputSet((i, b) => b);
            var set2 = t1.TransformForward(set1);
            var set3 = t2.TransformForward(set2);
            var set4 = t2.OutputSet((b, i) => b);

            Assert.AreEqual(11U, set3.Element());
            Assert.AreEqual(11U, set4.Element());
        }

        /// <summary>
        /// Test getting the input sets for a transformer.
        /// </summary>
        [TestMethod]
        public void TestTransformerInputSets()
        {
            var manager = new StateSetTransformerManager(0);
            var f = new ZenFunction<uint, uint>(i => i + 1);
            var t = f.Transformer(manager);
            var inSet1 = t.InputSet((x, y) => y == 10);
            var inSet2 = t.InputSet((x, y) => y == 11);
            var inSet3 = inSet1.Intersect(inSet2);
            var inSet4 = inSet1.Union(inSet2);

            Assert.AreEqual(9U, inSet1.Element());
            Assert.AreEqual(10U, inSet2.Element());
            Assert.IsTrue(inSet3.IsEmpty());
            Assert.IsFalse(inSet4.IsEmpty());
        }

        /// <summary>
        /// Test getting the input sets for a transformer.
        /// </summary>
        [TestMethod]
        public void TestTransformerTransformForward()
        {
            var manager = new StateSetTransformerManager(0);
            var f = new ZenFunction<uint, bool>(i => i >= 10);
            var t = f.Transformer(manager);
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
            var manager = new StateSetTransformerManager(0);
            var f1 = new ZenFunction<uint, uint>(i => i + 1);
            var f2 = new ZenFunction<uint, uint>(i => i + 2);
            var t1 = f1.Transformer(manager);
            var t2 = f2.Transformer(manager);
            var set1 = t1.InputSet((x, y) => y == 10);
            var set2 = t2.InputSet((x, y) => y == 11);
            var set3 = set1.Intersect(set2);

            Assert.AreEqual(set1, set2);
            Assert.AreEqual(9U, set3.Element());
        }

        /// <summary>
        /// Test checking if a set is full.
        /// </summary>
        [TestMethod]
        public void TestTransformerSetIsFull()
        {
            var manager = new StateSetTransformerManager(0);
            var t = new ZenFunction<bool, bool>(b => true).Transformer(manager);
            var set = t.InputSet((i, o) => o);

            Assert.IsTrue(set.IsFull());
        }

        /// <summary>
        /// Test checking if a set is full.
        /// </summary>
        [TestMethod]
        public void TestTransformerSetIsEmpty()
        {
            var manager = new StateSetTransformerManager(0);
            var t = new ZenFunction<bool, bool>(b => true).Transformer(manager);
            var set = t.InputSet((i, o) => o).Complement();

            Assert.IsTrue(set.IsEmpty());
        }

        /// <summary>
        /// Test for different types.
        /// </summary>
        [TestMethod]
        public void TestTransformerArgTypes()
        {
            var manager = new StateSetTransformerManager(0);
            Assert.IsTrue(Zen.Transformer<bool, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
            Assert.IsTrue(Zen.Transformer<byte, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
            Assert.IsTrue(Zen.Transformer<short, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
            Assert.IsTrue(Zen.Transformer<ushort, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
            Assert.IsTrue(Zen.Transformer<int, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
            Assert.IsTrue(Zen.Transformer<uint, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
            Assert.IsTrue(Zen.Transformer<long, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
            Assert.IsTrue(Zen.Transformer<ulong, bool>(b => true, manager).InputSet((i, o) => o).IsFull());
        }

        /// <summary>
        /// Test state sets with fixed width integers.
        /// </summary>
        [TestMethod]
        public void TestTransformerFixedWidthInteger()
        {
            var manager = new StateSetTransformerManager(0);
            var t = new ZenFunction<Int<_5>, bool>(i => i <= new Int<_5>(0)).Transformer(manager);
            var set = t.InputSet((x, y) => y);
            Assert.IsTrue(set.Element() <= new Int<_5>(0));
        }

        /// <summary>
        /// Test state set equality.
        /// </summary>
        [TestMethod]
        public void TestTransformerSetEquality()
        {
            var manager = new StateSetTransformerManager(0);
            var t1 = new ZenFunction<bool, bool>(b => true).Transformer(manager);
            var t2 = new ZenFunction<bool, bool>(b => true).Transformer(manager);

            var set1 = t1.InputSet((x, y) => Not(x));
            var set2 = t2.InputSet((x, y) => Not(x));

            Assert.IsTrue(set1.Equals(set2));
            Assert.IsFalse(set1.Equals(2));
        }

        /// <summary>
        /// Test getting an element for an empty set.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerNoElement()
        {
            var manager = new StateSetTransformerManager(0);
            var f = new ZenFunction<uint, uint>(i => i + 1);
            var t = f.Transformer(manager);
            var emptySet = t.InputSet((x, y) => x + 2 == y);
            emptySet.Element();
        }

        /// <summary>
        /// Test a transformer over an object.
        /// </summary>
        [TestMethod]
        public void TestTransformerObject()
        {
            var manager = new StateSetTransformerManager(0);
            var f = new ZenFunction<IpHeader, bool>(p => And(p.GetDstIp().GetValue() <= 4, p.GetSrcIp().GetValue() <= 5));
            var t = f.Transformer(manager);

            var set = t.InputSet((p, b) => Not(b));
            Assert.IsFalse(set.Element().DstIp.Value <= 4 && set.Element().SrcIp.Value <= 5);

            var outputSet = t.TransformForward(set);
            Assert.AreEqual(false, outputSet.Element());

            var inputSet = t.TransformBackwards(outputSet);
            Assert.IsFalse(set.Element().DstIp.Value <= 4 && set.Element().SrcIp.Value <= 5);
        }

        /// <summary>
        /// Test a transformer over an object with a variable width integer field.
        /// </summary>
        [TestMethod]
        public void TestTransformerObjectWithInt()
        {
            var manager = new StateSetTransformerManager(0);
            var t = new ZenFunction<TestHelper.ObjectWithInt, bool>(o => o.GetField<TestHelper.ObjectWithInt, UInt<_10>>("Field1") == new UInt<_10>(1))
                .Transformer(manager);
            var set = t.InputSet((p, o) => o);
            Assert.AreEqual(1L, set.Element().Field1.ToLong());
        }

        /// <summary>
        /// Test packet transformations.
        /// </summary>
        [TestMethod]
        public void TestPacketSet()
        {
            var manager = new StateSetTransformerManager(0);
            var rnd = new Random();
            for (int j = 0; j < 2; j++)
            {
                var i = (uint)rnd.Next();
                var f = new ZenFunction<IpHeader, bool>(p =>
                {
                    return p.GetDstIp().GetValue() == i;
                });

                StateSetTransformer<IpHeader, bool> transformer = f.Transformer(manager);
                var set = transformer.InputSet((pkt, matches) => matches);
                Assert.AreEqual(i, set.Element().DstIp.Value);
            }
        }

        /// <summary>
        /// Test packet transformations.
        /// </summary>
        [TestMethod]
        public void TestMultipleTransformers()
        {
            var manager = new StateSetTransformerManager(0);
            var t1 = new ZenFunction<IpHeader, bool>(p => true).Transformer(manager);
            var set1 = t1.InputSet((p, v) => v);
            var t2 = new ZenFunction<IpHeader, bool>(p => p.GetDstIp().GetValue() == 1).Transformer(manager);
            var set2 = t2.InputSet((p, v) => v);
            var t3 = new ZenFunction<uint, bool>(u => u == 2).Transformer(manager);
            var set3 = t3.InputSet((u, v) => v);
            Assert.IsTrue(set1.IsFull());
            Assert.AreEqual(1U, set2.Element().DstIp.Value);
            Assert.AreEqual(2U, set3.Element());
        }

        /// <summary>
        /// Test that multiple arguments works with tuples.
        /// </summary>
        [TestMethod]
        public void TestMultipleArguments()
        {
            var manager = new StateSetTransformerManager(0);
            var t = new ZenFunction<Pair<IpHeader, IpHeader>, bool>(x => x.Item1() == x.Item2()).Transformer(manager);
            var set = t.InputSet((x, b) => b);
            var e = set.Element();
            Assert.AreEqual(e.Item1.DstIp, e.Item2.DstIp);
            Assert.AreEqual(e.Item1.SrcIp, e.Item2.SrcIp);
            Assert.AreEqual(e.Item1.SrcPort, e.Item2.DstPort);
            Assert.AreEqual(e.Item1.DstPort, e.Item2.DstPort);
        }

        /// <summary>
        /// Test that multiple arguments works with tuples.
        /// </summary>
        [TestMethod]
        public void TestTransformerFieldAccesses()
        {
            var manager = new StateSetTransformerManager(0);
            var t1 = new ZenFunction<IpHeader, Ip>(x => x.GetDstIp()).Transformer(manager);
            var t2 = new ZenFunction<IpHeader, Ip>(x => x.GetSrcIp()).Transformer(manager);
            var t3 = new ZenFunction<IpHeader, ushort>(x => x.GetDstPort()).Transformer(manager);
            var t4 = new ZenFunction<IpHeader, ushort>(x => x.GetSrcPort()).Transformer(manager);
            var t5 = new ZenFunction<IpHeader, byte>(x => x.GetProtocol()).Transformer(manager);

            var allHeaders = new ZenFunction<IpHeader, bool>(x => true).Transformer(manager).InputSet((i, o) => o);
            var s1 = t1.InputSet((x, ip) => true);
            var s2 = t2.InputSet((x, ip) => true);
            var s3 = t3.InputSet((x, ip) => true);
            var s4 = t4.InputSet((x, ip) => true);
            var s5 = t5.InputSet((x, ip) => true);
            var all = s1.Intersect(s2).Intersect(s3).Intersect(s4).Intersect(s5);
            Assert.AreEqual(allHeaders, all);
        }

        /// <summary>
        /// Test that transformers work with multiple inputs.
        /// </summary>
        [TestMethod]
        public void TestTransformersMultipleArguments()
        {
            var manager = new StateSetTransformerManager(0);
            var t1 = Zen.Transformer<uint, uint, uint>((x, y) => x, manager);
            var t2 = Zen.Transformer<uint, uint, uint, uint>((x, y, z) => y, manager);
            var t3 = Zen.Transformer<uint, uint, uint, uint, uint>((w, x, y, z) => y, manager);

            Assert.AreEqual(3U, t1.OutputSet((p, o) => p.Item1() == 3U).Element());
            Assert.AreEqual(3U, t2.OutputSet((p, o) => p.Item2() == 3U).Element());
            Assert.AreEqual(3U, t3.OutputSet((p, o) => p.Item3() == 3U).Element());
        }

        /// <summary>
        /// Test that using different manager objects works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerInvalidArguments1()
        {
            var t1 = Zen.Transformer<uint, bool>(x => true, new StateSetTransformerManager());
            var t2 = Zen.Transformer<uint, bool>(x => true, new StateSetTransformerManager());

            var set1 = t1.InputSet((i, o) => o);
            t2.TransformForward(set1);
        }

        /// <summary>
        /// Test that using different manager objects works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerInvalidArguments2()
        {
            var t1 = new ZenFunction<uint, bool>(x => true).Transformer(new StateSetTransformerManager());
            var t2 = new ZenFunction<uint, bool>(x => true).Transformer(new StateSetTransformerManager());

            var set1 = t1.InputSet((i, o) => o);
            var set2 = t2.InputSet((i, o) => o);
            set1.Union(set2);
        }

        /// <summary>
        /// Test that using different manager objects works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerInvalidArguments3()
        {
            var t1 = new ZenFunction<uint, bool>(x => true).Transformer(new StateSetTransformerManager());
            var t2 = new ZenFunction<uint, bool>(x => true).Transformer(new StateSetTransformerManager());

            var set1 = t1.InputSet((i, o) => o);
            var set2 = t2.InputSet((i, o) => o);
            set1.Intersect(set2);
        }

        /// <summary>
        /// Test that using different manager objects works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTransformerInvalidType()
        {
            var t1 = new ZenFunction<string, bool>(x => true).Transformer(new StateSetTransformerManager());
        }

        /// <summary>
        /// Test that transformers are cached.
        /// </summary>
        [TestMethod]
        public void TestTransformerCaching1()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = new ZenFunction<byte, bool>(x => true).Transformer();
            var f2 = new ZenFunction<byte, bool>(x => true).Transformer();
            var f3 = new ZenFunction<byte, bool>(x => true).Transformer(m1);
            var f4 = new ZenFunction<byte, bool>(x => true).Transformer(m1);
            var f5 = new ZenFunction<byte, bool>(x => true).Transformer(m2);
            var f6 = new ZenFunction<byte, bool>(x => true).Transformer(m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }

        /// <summary>
        /// Test that transformers are cached.
        /// </summary>
        [TestMethod]
        public void TestTransformerCaching2()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = new ZenFunction<byte, byte, bool>((x, y) => true).Transformer();
            var f2 = new ZenFunction<byte, byte, bool>((x, y) => true).Transformer();
            var f3 = new ZenFunction<byte, byte, bool>((x, y) => true).Transformer(m1);
            var f4 = new ZenFunction<byte, byte, bool>((x, y) => true).Transformer(m1);
            var f5 = new ZenFunction<byte, byte, bool>((x, y) => true).Transformer(m2);
            var f6 = new ZenFunction<byte, byte, bool>((x, y) => true).Transformer(m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }

        /// <summary>
        /// Test that transformers are cached.
        /// </summary>
        [TestMethod]
        public void TestTransformerCaching3()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = new ZenFunction<byte, byte, byte, bool>((x, y, z) => true).Transformer();
            var f2 = new ZenFunction<byte, byte, byte, bool>((x, y, z) => true).Transformer();
            var f3 = new ZenFunction<byte, byte, byte, bool>((x, y, z) => true).Transformer(m1);
            var f4 = new ZenFunction<byte, byte, byte, bool>((x, y, z) => true).Transformer(m1);
            var f5 = new ZenFunction<byte, byte, byte, bool>((x, y, z) => true).Transformer(m2);
            var f6 = new ZenFunction<byte, byte, byte, bool>((x, y, z) => true).Transformer(m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }

        /// <summary>
        /// Test that transformers are cached.
        /// </summary>
        [TestMethod]
        public void TestTransformerCaching4()
        {
            var m1 = new StateSetTransformerManager();
            var m2 = new StateSetTransformerManager();

            var f1 = new ZenFunction<byte, byte, byte, byte, bool>((w, x, y, z) => true).Transformer();
            var f2 = new ZenFunction<byte, byte, byte, byte, bool>((w, x, y, z) => true).Transformer();
            var f3 = new ZenFunction<byte, byte, byte, byte, bool>((w, x, y, z) => true).Transformer(m1);
            var f4 = new ZenFunction<byte, byte, byte, byte, bool>((w, x, y, z) => true).Transformer(m1);
            var f5 = new ZenFunction<byte, byte, byte, byte, bool>((w, x, y, z) => true).Transformer(m2);
            var f6 = new ZenFunction<byte, byte, byte, byte, bool>((w, x, y, z) => true).Transformer(m2);

            Assert.IsTrue(ReferenceEquals(f1, f2));
            Assert.IsTrue(ReferenceEquals(f3, f4));
            Assert.IsTrue(ReferenceEquals(f5, f6));
            Assert.IsFalse(ReferenceEquals(f1, f3));
            Assert.IsFalse(ReferenceEquals(f3, f5));
        }

        /// <summary>
        /// Test that transformers work with complex expressions.
        /// </summary>
        [TestMethod]
        public void TestTransformerComplex()
        {
            var manager = new StateSetTransformerManager(0);

            var f = new Func<Zen<TestHelper.Object2>, Zen<Option<TestHelper.Object2>>>(p =>
            {
                return If(
                    p.GetField<TestHelper.Object2, int>("Field1") == 1,
                    Option.Create(p.WithField<TestHelper.Object2, int>("Field1", 2)),
                    If(
                        p.GetField<TestHelper.Object2, int>("Field1") == 3,
                        Option.Create(p.WithField<TestHelper.Object2, int>("Field1", 4)),
                        Option.Create(p)));
            });

            var set1 = new ZenFunction<TestHelper.Object2, bool>(o => o.GetField<TestHelper.Object2, int>("Field1") == 1).StateSet(manager);
            var set2 = new ZenFunction<TestHelper.Object2, bool>(o => o.GetField<TestHelper.Object2, int>("Field1") == 4).StateSet(manager);
            var t = new ZenFunction<TestHelper.Object2, TestHelper.Object2>(p => f(p).Value()).Transformer(manager);

            var x = t.TransformForward(set1).Element();
            var y = t.TransformBackwards(set2).Element();

            Assert.AreEqual(2, x.Field1);
            Assert.AreEqual(3, y.Field1);
        }
    }
}