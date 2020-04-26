// <copyright file="OptionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.ZenTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Research.Zen;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Microsoft.Research.Zen.Language;
    using static Microsoft.Research.ZenTests.TestHelper;

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
            Repeat(x => CheckAgreement<Option<int>>(o => And(o.HasValue(), o.Value() == Int(x))));
        }

        /// <summary>
        /// Test option with tuple underneath.
        /// </summary>
        [TestMethod]
        public void TestOptionSomeTuple()
        {
            Repeat(x =>
            {
                CheckAgreement<Option<Tuple<byte, byte>>>(o =>
                {
                    var item1 = o.Value().Item1() == Byte(x);
                    var item2 = o.Value().Item2() == Byte(x);
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
                Implies(o.HasValue(), o.Select(v => v + Int(1)).Value() == o.Value() + Int(1)));
        }

        /// <summary>
        /// Test option where.
        /// </summary>
        [TestMethod]
        public void TestOptionWhere()
        {
            CheckValid<Option<int>>(o =>
                Implies(And(o.HasValue(), o.Value() <= Int(4)), Not(o.Where(v => v > Int(4)).HasValue())));
        }

        /// <summary>
        /// Test option match.
        /// </summary>
        [TestMethod]
        public void TestOptionMatch()
        {
            CheckValid<Option<int>>(o =>
                Implies(And(o.HasValue(), o.Value() <= Int(4)),
                    o.Match(none: () => False(), some: v => v <= Int(4))));
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
            CheckValid<Option<int>>(o => o.Select(v => Int(2)).ValueOrDefault(Int(2)) == Int(2));
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
    }
}
