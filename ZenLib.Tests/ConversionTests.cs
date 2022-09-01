// <copyright file="ConstantTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests conversion from values to Zen values.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConversionTests
    {
        /// <summary>
        /// Test that we can convert C# values to Zen values.
        /// </summary>
        [TestMethod]
        public void TestConversions()
        {
            CheckAgreement<bool>(x => x == true);
            CheckAgreement<byte>(x => x == 3);
            CheckAgreement<short>(x => x == 4);
            CheckAgreement<ushort>(x => x == 5);
            CheckAgreement<int>(x => x == 6);
            CheckAgreement<uint>(x => x == 7);
            CheckAgreement<long>(x => x == 8);
            CheckAgreement<ulong>(x => x == 9);
            CheckAgreement<Option<int>>(x => x == Option.Some(3));
            CheckAgreement<Pair<int, int>>(x => x == new Pair<int, int>(1, 2));
            CheckAgreement<(int, int)>(x => x == (1, 2));
            CheckAgreement<FSeq<int>>(x => x == FSeq.FromRange(new List<int>() { 1, 2, 3 }));
            CheckAgreement<FSeq<FSeq<int>>>(x => x == new FSeq<FSeq<int>>(new FSeq<int>(1)));
            CheckAgreement<FString>(x => x == new FString("hello"));
            CheckAgreement<Object2>(x => x == new Object2 { Field1 = 1, Field2 = 2 });
        }

        /// <summary>
        /// Test that we can evaluate values correctly.
        /// </summary>
        [TestMethod]
        public void TestEvaluation()
        {
            CheckEqual(true);
            CheckEqual((byte)1);
            CheckEqual((short)2);
            CheckEqual((ushort)3);
            CheckEqual(4);
            CheckEqual(5U);
            CheckEqual(6L);
            CheckEqual(7UL);
            CheckEqual(Option.None<int>());
            CheckEqual(Option.Some(8));
            CheckEqual(new Pair<int, int>(9, 10));
            CheckEqual(new FString("hello"));
            CheckEqualLists(FSeq.FromRange(new List<int>() { 1, 2, 3 }));
        }

        /// <summary>
        /// Test that converting works properly when a field has a concrete instantiation of an interface.
        /// </summary>
        [TestMethod]
        public void TestConvertConcreteListField()
        {
            var o = new NestedClass { Field1 = FSeq.FromRange(new List<int>()) };
            var _ = Constant(o);
        }

        /// <summary>
        /// Test that converting a value with a null field does not work.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConvertNullClass()
        {
            var o = new NestedClass { };
            var _ = Constant(o);
        }

        /// <summary>
        /// Test that converting a value with a null tuple inner value.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConvertNullTupleValue()
        {
            var o = new Pair<Object1, Object1>(null, null);
            var _ = Constant(o);
        }

        /// <summary>
        /// Test that converting a value with a null option inner value.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConvertNullOptionValue()
        {
            Option<Object1> o = Option.Some<Object1>(null);
            var _ = Constant(o);
        }

        /// <summary>
        /// Test that converting an option constant works for None.
        /// </summary>
        [TestMethod]
        public void TestConvertOptionConstantNone()
        {
            Zen<Option<Unit>> x = Option.None<Unit>();
            Assert.AreEqual(x, Option.Null<Unit>());
        }

        /// <summary>
        /// Test that converting an option constant works for Some.
        /// </summary>
        [TestMethod]
        public void TestConvertOptionConstantSome()
        {
            Zen<Option<int>> x = Option.Some(3);
            Assert.AreEqual(x, Option.Create<int>(3));
        }

        /// <summary>
        /// Test that converting a value with a null dictionary key does not work.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConvertNullListValue()
        {
            FSeq<Object1> o = FSeq.FromRange(new List<Object1> { { null } });
            var _ = Constant(o);
        }

        /// <summary>
        /// Check we convert a value correctly.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckEqual<T>(T value)
        {
            var f = new ZenFunction<T>(() => value);
            Assert.AreEqual(value, f.Evaluate());
        }

        /// <summary>
        /// Check we convert a list correctly.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckEqualLists<T>(FSeq<T> value)
        {
            var f = new ZenFunction<FSeq<T>>(() => Lift(value));
            var result = f.Evaluate();

            Assert.AreEqual(value.Count(), result.Count());

            for (int i = 0; i < value.Count(); i++)
            {
                Assert.AreEqual(value.ToList()[i], result.ToList()[i]);
            }
        }

        /// <summary>
        /// An object with another nested inside.
        /// </summary>
        internal class NestedClass
        {
            public FSeq<int> Field1 { get; set; }
        }
    }
}