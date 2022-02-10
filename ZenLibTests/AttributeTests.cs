// <copyright file="AttributeTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Z3;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen object type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AttributeTests
    {
        /// <summary>
        /// Test that attributes work correctly for depth.
        /// </summary>
        [TestMethod]
        public void TestZ3()
        {
            var ctx = new Context();
            var solver = ctx.MkSolver();

            var c1 = ctx.MkConstructor("None", "none");
            var c2 = ctx.MkConstructor("Some", "some", new string[] { "some" }, new Sort[] { ctx.IntSort });
            var optionSort = ctx.MkDatatypeSort("Option", new Constructor[] { c1, c2 });
            var none = ctx.MkApp(c1.ConstructorDecl);

            var arraySort = ctx.MkArraySort(ctx.IntSort, optionSort);

            var a = ctx.MkArrayConst("a", ctx.IntSort, optionSort);
            var b = ctx.MkArrayConst("b", ctx.IntSort, optionSort);
            var some = ctx.MkApp(c2.ConstructorDecl, ctx.MkInt(9));

            // this is the magic
            solver.Assert(ctx.MkEq(ctx.MkTermArray(a), none));
            solver.Assert(ctx.MkEq(ctx.MkTermArray(b), none));

            Console.WriteLine(ctx.MkEq(ctx.MkSelect(a, ctx.MkInt(10)), some));

            solver.Assert(ctx.MkEq(ctx.MkSelect(b, ctx.MkInt(11)), some));
            solver.Assert(ctx.MkEq(ctx.MkStore(a, ctx.MkInt(10), some), b));

            Console.WriteLine(solver.Check());

            Console.WriteLine(solver.Model.Evaluate(b, true));

            var expr = solver.Model.Evaluate(b, true);

            Console.WriteLine(expr.IsStore);

            var store = expr.Args[0].Args[0].Args[0].Sort;

            Console.WriteLine(store);
        }

        /// <summary>
        /// Test that dictionary evaluation works.
        /// </summary>
        [TestMethod]
        public void TestDictEvaluation()
        {
            var zf1 = new ZenFunction<IDictionary<int, int>, IDictionary<int, int>>(d => DictSet(d, 10, 20));
            var result1 = zf1.Evaluate(new Dictionary<int, int> { { 5, 5 } });
            Assert.AreEqual(2, result1.Count);
            Assert.AreEqual(5, result1[5]);
            Assert.AreEqual(20, result1[10]);

            var zf2 = new ZenFunction<IDictionary<int, int>, bool>(d => DictGet(d, 10) == Option.Some(11));
            var result2 = zf2.Evaluate(new Dictionary<int, int> { { 5, 5 } });
            var result3 = zf2.Evaluate(new Dictionary<int, int> { { 10, 10 } });
            var result4 = zf2.Evaluate(new Dictionary<int, int> { { 5, 5 }, { 10, 11 } });
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);

            var zf3 = new ZenFunction<IDictionary<int, int>>(() => EmptyDict<int, int>());
            var result5 = zf3.Evaluate();
            Assert.AreEqual(0, result5.Count);
        }

        /// <summary>
        /// Test that dictionary symbolic evaluation with equality and empty dictionary.
        /// </summary>
        [TestMethod]
        public void TestDictEqualsEmpty()
        {
            var zf = new ZenConstraint<IDictionary<int, int>>(d => d == EmptyDict<int, int>());
            var result = zf.Find();

            Assert.AreEqual(0, result.Value.Count);
            Console.WriteLine(result.Value.Count);
            foreach (var kv in result.Value)
            {
                Console.WriteLine($"{kv.Key} -> {kv.Value}");
            }
        }

        /// <summary>
        /// Test dictionary symbolic evaluation with set.
        /// </summary>
        [TestMethod]
        public void TestDictSet()
        {
            var zf = new ZenFunction<IDictionary<int, int>, IDictionary<int, int>>(d => DictSet(d, 10, 20));
            var result = zf.Find((d1, d2) => d2 == EmptyDict<int, int>());

            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Test dictionary symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestDictGet()
        {
            var zf = new ZenConstraint<IDictionary<int, int>>(d => DictGet(d, 10) == Option.Some(11));
            var result = zf.Find();

            Console.WriteLine(result.Value.Count);
        }

        /// <summary>
        /// Test that attributes work correctly for depth.
        /// </summary>
        [TestMethod]
        public void TestGenerationDepth()
        {
            // work around unused field warning.
            ObjectAttribute o1;
            o1.Field1 = new List<int>();
            o1.Field2 = new List<int>();
            o1.Field3 = new List<int>();

            var o = Symbolic<ObjectAttribute>(depth: 3, exhaustiveDepth: true);

            var s1 = o.GetField<ObjectAttribute, IList<int>>("Field1").ToString();
            var s2 = o.GetField<ObjectAttribute, IList<int>>("Field2").ToString();
            var s3 = o.GetField<ObjectAttribute, IList<int>>("Field3").ToString();

            Assert.AreEqual(11, s1.Split("::").Length);

            for (int i = 1; i <= 5; i++)
            {
                Assert.IsTrue(s2.Contains($"== {i}"));
            }
            Assert.IsFalse(s2.Contains($"== 6"));

            Console.WriteLine(s3);

            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(s3.Contains($"== {i}"));
            }
            Assert.IsFalse(s3.Contains($"== 4"));
        }

        private struct ObjectAttribute
        {
            [ZenSize(depth: 10, EnumerationType.FixedSize)]
            public IList<int> Field1;

            [ZenSize(depth: 5, EnumerationType.Exhaustive)]
            public IList<int> Field2;

            [ZenSize(depth: -1, EnumerationType.User)]
            public IList<int> Field3;
        }
    }
}
