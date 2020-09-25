// <copyright file="OptionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Tests.Network;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for the Zen option type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class OptionTests
    {
        /// <summary>
        /// Test none has no value.
        /// </summary>
        [TestMethod]
        public void TestOptionNone()
        {
            CheckAgreement<Option<int>>(o => o.HasValue());
        }

        /// <summary>
        /// Test some returns the correct value.
        /// </summary>
        [TestMethod]
        public void TestOptionSomeInt()
        {
            RandomBytes(x => CheckAgreement<Option<int>>(o => And(o.HasValue(), o.Value() == Constant<int>(x))));
        }

        /// <summary>
        /// Test option with tuple underneath.
        /// </summary>
        [TestMethod]
        public void TestOptionSomeTuple()
        {
            RandomBytes(x =>
            {
                CheckAgreement<Option<Tuple<byte, byte>>>(o =>
                {
                    var item1 = o.Value().Item1() == x;
                    var item2 = o.Value().Item2() == x;
                    return And(o.HasValue(), Or(item1, item2));
                });
            });
        }

        /// <summary>
        /// Test option identities.
        /// </summary>
        [TestMethod]
        public void TestOptionIdentities()
        {
            CheckValid<Option<int>>(o => TupleToOption(o.HasValue(), o.Value()).HasValue() == o.HasValue());
            // CheckValid<Option<int>>(o => TupleToOption(o.HasValue(), o.Value()).Value() == o.Value());
        }

        /// <summary>
        /// Test option select.
        /// </summary>
        [TestMethod]
        public void TestOptionSelect()
        {
            CheckValid<Option<int>>(o =>
                Implies(o.HasValue(), o.Select(v => v + Constant<int>(1)).Value() == o.Value() + Constant<int>(1)));
        }

        /// <summary>
        /// Test option where.
        /// </summary>
        [TestMethod]
        public void TestOptionWhere()
        {
            CheckValid<Option<int>>(o =>
                Implies(And(o.HasValue(), o.Value() <= Constant<int>(4)), Not(o.Where(v => v > Constant<int>(4)).HasValue())));
        }

        /// <summary>
        /// Test option match.
        /// </summary>
        [TestMethod]
        public void TestOptionMatch()
        {
            CheckValid<Option<int>>(o =>
                Implies(And(o.HasValue(), o.Value() <= Constant<int>(4)),
                    o.Case(none: () => False(), some: v => v <= Constant<int>(4))));
        }

        /// <summary>
        /// Test option to list.
        /// </summary>
        [TestMethod]
        public void TestOptionToList1()
        {
            CheckAgreement<Option<int>>(o => o.ToList().IsEmpty());
        }

        /// <summary>
        /// Test option to list.
        /// </summary>
        [TestMethod]
        public void TestOptionToList2()
        {
            CheckValid<Option<int>>(o => Implies(o.HasValue(), o.ToList().Length() == 1));
        }

        /// <summary>
        /// Test option value or.
        /// </summary>
        [TestMethod]
        public void TestOptionValueOrDefault()
        {
            CheckValid<Option<int>>(o => o.Select(v => Constant(2)).ValueOrDefault(Constant(2)) == Constant(2));
        }

        /// <summary>
        /// Test option value or.
        /// </summary>
        [TestMethod]
        public void TestOptionNullValueType()
        {
            CheckAgreement<Option<(int, int)>>(o => o.Value().Item1() == o.Value().Item2());
        }

        /// <summary>
        /// Test option value or.
        /// </summary>
        [TestMethod]
        public void TestOptionValueOr()
        {
            var o1 = Option.Some(1);
            var o2 = Option.None<int>();
            Assert.AreEqual(1, o1.ValueOr(3));
            Assert.AreEqual(3, o2.ValueOr(3));
        }

        /// <summary>
        /// Test non-null default.
        /// </summary>
        [TestMethod]
        public void TestOptionDefault()
        {
            var f = Function(() =>
            {
                var x = Null<IpHeader>();
                return x.Value();
            });

            Assert.AreEqual(0U, f.Evaluate().DstIp.Value);
        }
    }
}
