// <copyright file="OptionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Tests.Network;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen option type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class OptionTests
    {
        /// <summary>
        /// Test that option equality works.
        /// </summary>
        [TestMethod]
        public void TestOptionEquality()
        {
            Assert.AreNotEqual(Option.None<int>(), null);
            Assert.AreNotEqual(Option.None<int>(), 4);
            Assert.AreEqual(Option.None<int>(), Option.None<int>());
            Assert.AreEqual(new Option<int>(false, 1), new Option<int>(false, 2));
            Assert.AreNotEqual(Option.None<int>(), Option.Some(0));
            Assert.AreNotEqual(Option.Some(0), Option.None<int>());
            Assert.AreEqual(Option.Some(0), Option.Some(0));
            Assert.AreNotEqual(Option.Some(0), Option.Some(1));
            Assert.AreNotEqual(Option.None<int>().GetHashCode(), Option.Some(1).GetHashCode());
        }

        /// <summary>
        /// Test that finding an option works.
        /// </summary>
        [TestMethod]
        public void TestOptionFind()
        {
            var zf = new ZenFunction<Option<int>, bool>(o => o.IsSome());
            var example = zf.Find((i, o) => o);
            Assert.IsTrue(example.HasValue);
            Assert.AreEqual(Option.Some(0), example.Value);
        }

        /// <summary>
        /// Test none has no value.
        /// </summary>
        [TestMethod]
        public void TestOptionNone()
        {
            CheckAgreement<Option<int>>(o => o.IsSome());
        }

        /// <summary>
        /// Test some returns the correct value.
        /// </summary>
        [TestMethod]
        public void TestOptionSomeInt()
        {
            RandomBytes(x => CheckAgreement<Option<int>>(o => And(o.IsSome(), o.Value() == Constant<int>(x))));
        }

        /// <summary>
        /// Test option with tuple underneath.
        /// </summary>
        [TestMethod]
        public void TestOptionSomeTuple()
        {
            RandomBytes(x =>
            {
                CheckAgreement<Option<Pair<byte, byte>>>(o =>
                {
                    var item1 = o.Value().Item1() == x;
                    var item2 = o.Value().Item2() == x;
                    return And(o.IsSome(), Or(item1, item2));
                });
            });
        }

        /// <summary>
        /// Test option match.
        /// </summary>
        [TestMethod]
        public void TestOptionMatch()
        {
            CheckValid<Option<int>>(o =>
                Implies(And(o.IsSome(), o.Value() <= Constant<int>(4)),
                    o.Case(none: () => False(), some: v => v <= Constant<int>(4))));
        }

        /// <summary>
        /// Test option value or.
        /// </summary>
        [TestMethod]
        public void TestOptionNullValueType()
        {
            CheckAgreement<Option<Pair<int, int>>>(o => o.Value().Item1() == o.Value().Item2());
        }

        /// <summary>
        /// Test option value or.
        /// </summary>
        [TestMethod]
        public void TestOptionValueOrDefaultValid()
        {
            CheckValid<Option<int>>(o => o.Select(v => Constant(2)).ValueOrDefault(Constant(2)) == Constant(2));
        }

        /// <summary>
        /// Test option value or default.
        /// </summary>
        [TestMethod]
        public void TestOptionValueOrDefault()
        {
            var zf = new ZenFunction<Option<int>, int, int>((o, i) => o.ValueOrDefault(i));

            Assert.AreEqual(1, zf.Evaluate(Option.Some(1), 3));
            Assert.AreEqual(3, zf.Evaluate(Option.None<int>(), 3));

            zf.Compile();
            Assert.AreEqual(1, zf.Evaluate(Option.Some(1), 3));
            Assert.AreEqual(3, zf.Evaluate(Option.None<int>(), 3));

            Assert.AreEqual(1, Option.Some(1).ValueOrDefault(3));
            Assert.AreEqual(3, Option.None<int>().ValueOrDefault(3));
        }

        /// <summary>
        /// Test option IsSome.
        /// </summary>
        [TestMethod]
        public void TestOptionIsSome()
        {
            var zf = new ZenFunction<Option<int>, bool>(o => o.IsSome());

            Assert.AreEqual(true, zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(false, zf.Evaluate(Option.None<int>()));

            zf.Compile();
            Assert.AreEqual(true, zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(false, zf.Evaluate(Option.None<int>()));

            Assert.AreEqual(true, Option.Some(1).IsSome());
            Assert.AreEqual(false, Option.None<int>().IsSome());
        }

        /// <summary>
        /// Test option IsNone.
        /// </summary>
        [TestMethod]
        public void TestOptionIsNone()
        {
            var zf = new ZenFunction<Option<int>, bool>(o => o.IsNone());

            Assert.AreEqual(false, zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(true, zf.Evaluate(Option.None<int>()));

            zf.Compile();
            Assert.AreEqual(false, zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(true, zf.Evaluate(Option.None<int>()));

            Assert.AreEqual(false, Option.Some(1).IsNone());
            Assert.AreEqual(true, Option.None<int>().IsNone());
        }

        /// <summary>
        /// Test option where.
        /// </summary>
        [TestMethod]
        public void TestOptionWhereValid()
        {
            CheckValid<Option<int>>(o =>
                Implies(And(o.IsSome(), o.Value() <= Constant<int>(4)), Not(o.Where(v => v > Constant<int>(4)).IsSome())));
        }

        /// <summary>
        /// Test option Where.
        /// </summary>
        [TestMethod]
        public void TestOptionWhere()
        {
            var zf = new ZenFunction<Option<int>, Option<int>>(o => o.Where(i => i > 10));

            Assert.AreEqual(Option.None<int>(), zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(Option.None<int>()));
            Assert.AreEqual(Option.Some<int>(11), zf.Evaluate(Option.Some(11)));

            zf.Compile();
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(Option.None<int>()));
            Assert.AreEqual(Option.Some<int>(11), zf.Evaluate(Option.Some(11)));

            Assert.AreEqual(Option.None<int>(), Option.Some(1).Where(i => i > 10));
            Assert.AreEqual(Option.None<int>(), Option.None<int>().Where(i => i > 10));
            Assert.AreEqual(Option.Some<int>(11), Option.Some(11).Where(i => i > 10));
        }

        /// <summary>
        /// Test option to sequence.
        /// </summary>
        [TestMethod]
        public void TestOptionToSequenceValid1()
        {
            CheckAgreement<Option<int>>(o => o.ToSequence().IsEmpty());
        }

        /// <summary>
        /// Test option to sequence.
        /// </summary>
        [TestMethod]
        public void TestOptionToSequenceValid2()
        {
            CheckValid<Option<int>>(o => Implies(o.IsSome(), o.ToSequence().Length() == 1));
        }

        /// <summary>
        /// Test option ToSequence.
        /// </summary>
        [TestMethod]
        public void TestOptionToSequence()
        {
            var zf = new ZenFunction<Option<int>, FSeq<int>>(o => o.ToSequence());

            Assert.AreEqual(1, zf.Evaluate(Option.Some(1)).Values.Count);
            Assert.AreEqual(0, zf.Evaluate(Option.None<int>()).Values.Count);

            zf.Compile();
            Assert.AreEqual(1, zf.Evaluate(Option.Some(1)).Values.Count);
            Assert.AreEqual(0, zf.Evaluate(Option.None<int>()).Values.Count);

            Assert.AreEqual(1, Option.Some(1).ToSequence().Values.Count);
            Assert.AreEqual(0, Option.None<int>().ToSequence().Values.Count);
        }

        /// <summary>
        /// Test option select.
        /// </summary>
        [TestMethod]
        public void TestOptionSelectValid()
        {
            CheckValid<Option<int>>(o =>
                Implies(o.IsSome(), o.Select(v => v + Constant<int>(1)).Value() == o.Value() + Constant<int>(1)));
        }

        /// <summary>
        /// Test option Where.
        /// </summary>
        [TestMethod]
        public void TestOptionSelect()
        {
            var zf = new ZenFunction<Option<int>, Option<int>>(o => o.Select(i => i + 1));

            Assert.AreEqual(Option.None<int>(), zf.Evaluate(Option.None<int>()));
            Assert.AreEqual(Option.Some(2), zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(Option.Some(11), zf.Evaluate(Option.Some(10)));

            zf.Compile();
            Assert.AreEqual(Option.None<int>(), zf.Evaluate(Option.None<int>()));
            Assert.AreEqual(Option.Some(2), zf.Evaluate(Option.Some(1)));
            Assert.AreEqual(Option.Some(11), zf.Evaluate(Option.Some(10)));

            Assert.AreEqual(Option.None<int>(), Option.None<int>().Select(i => i + 1));
            Assert.AreEqual(Option.Some(2), Option.Some(1).Select(i => i + 1));
            Assert.AreEqual(Option.Some(11), Option.Some(10).Select(i => i + 1));
        }

        /// <summary>
        /// Test non-null default.
        /// </summary>
        [TestMethod]
        public void TestOptionDefault()
        {
            var f = new ZenFunction<IpHeader>(() =>
            {
                var x = Option.Null<IpHeader>();
                return x.Value();
            });

            Assert.AreEqual(0U, f.Evaluate().DstIp.Value);
        }
    }
}
