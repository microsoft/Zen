// <copyright file="LogicTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenTests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static TestHelper;
    using static Zen.Language;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LogicTests
    {
        /// <summary>
        /// Test that Or is commutative.
        /// </summary>
        [TestMethod]
        public void TestOrCommutative()
        {
            CheckValid<bool, bool>((x, y) => Or(x, y) == Or(y, x));
        }

        /// <summary>
        /// Test that Or is commutative with multiple arguments.
        /// </summary>
        [TestMethod]
        public void TestOrCommutativeMultiple()
        {
            CheckValid<bool, bool, bool>((x, y, z) => Or(x, y, z) == Or(y, z, x));
        }

        /// <summary>
        /// Test that Or is associative.
        /// </summary>
        [TestMethod]
        public void TestOrAssociative()
        {
            CheckValid<bool, bool, bool>((x, y, z) => Or(Or(x, y), z) == Or(x, Or(y, z)));
        }

        /// <summary>
        /// Test that And is commutative.
        /// </summary>
        [TestMethod]
        public void TestAndCommutative()
        {
            CheckValid<bool, bool>((x, y) => And(x, y) == And(y, x));
        }

        /// <summary>
        /// Test that And is commutative with multiple arguments.
        /// </summary>
        [TestMethod]
        public void TestAndCommutativeMultiple()
        {
            CheckValid<bool, bool, bool>((x, y, z) => And(x, y, z) == And(y, z, x));
        }

        /// <summary>
        /// Test that And is associative.
        /// </summary>
        [TestMethod]
        public void TestAndAssociative()
        {
            CheckValid<bool, bool, bool>((x, y, z) => And(And(x, y), z) == And(x, And(y, z)));
        }

        /// <summary>
        /// Test that Implies is the same as its disjunction form.
        /// </summary>
        [TestMethod]
        public void TestImpliesDisjunction()
        {
            CheckValid<bool, bool>((x, y) => Implies(x, y) == Or(Not(x), y));
        }

        /// <summary>
        /// Test demorgan's property.
        /// </summary>
        [TestMethod]
        public void TestDemorgan1()
        {
            CheckValid<bool, bool>((x, y) => Not(Or(x, y)) == And(Not(x), Not(y)));
        }

        /// <summary>
        /// Test demorgan's property.
        /// </summary>
        [TestMethod]
        public void TestDemorgan2()
        {
            CheckValid<bool, bool>((x, y) => Not(And(x, y)) == Or(Not(x), Not(y)));
        }

        /// <summary>
        /// Test distributivity of Or with And.
        /// </summary>
        [TestMethod]
        public void TestDistributivity1()
        {
            CheckValid<bool, bool, bool>((x, y, z) => Or(x, And(y, z)) == And(Or(x, y), Or(x, z)));
        }

        /// <summary>
        /// Test distributivity of And with Or.
        /// </summary>
        [TestMethod]
        public void TestDistributivity2()
        {
            CheckValid<bool, bool, bool>((x, y, z) => And(x, Or(y, z)) == Or(And(x, y), And(x, z)));
        }

        /// <summary>
        /// Test nested commutativity.
        /// </summary>
        [TestMethod]
        public void TestLogicaFormula()
        {
            CheckValid<bool, bool, bool, bool>((a, b, c, d) =>
                Or(And(a, b), And(c, d)) == Or(And(d, c), And(b, a)));
        }

        /// <summary>
        /// Test nested commutativity.
        /// </summary>
        [TestMethod]
        public void TestLogicaFormula2()
        {
            CheckValid<bool>(x => Or(x, If(x, x, true)));
        }

        /// <summary>
        /// Test nested commutativity.
        /// </summary>
        [TestMethod]
        public void TestLogicaFormula3()
        {
            CheckAgreement(() => True());
        }
    }
}