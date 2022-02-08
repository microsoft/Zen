// <copyright file="UsageTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Basic;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Test for throwing exceptions when library is not used correctly.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UsageTests
    {
        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddException1()
        {
            DictExtensions.Add(null, Constant(1), Constant(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddException2()
        {
            DictExtensions.Add(Dict.Create<int, int>(), null, 1);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddException3()
        {
            DictExtensions.Add(Dict.Create<int, int>(), 1, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddBackException1()
        {
            SeqExtensions.AddBack(null, Constant(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddBackException2()
        {
            SeqExtensions.AddBack(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddFrontException1()
        {
            SeqExtensions.AddFront(null, Constant(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddFrontException2()
        {
            SeqExtensions.AddFront(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAllException1()
        {
            SeqExtensions.All<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAllException2()
        {
            SeqExtensions.All(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAnyException1()
        {
            SeqExtensions.Any<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAnyException2()
        {
            SeqExtensions.Any(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAndException1()
        {
            Basic.And(null, true);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAndException2()
        {
            Basic.And(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAppendException1()
        {
            SeqExtensions.Append(null, Seq.Create<int>());
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAppendException2()
        {
            SeqExtensions.Append(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAtException1()
        {
            SeqExtensions.At<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAtException2()
        {
            SeqExtensions.At(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for adding big integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseBigIntegerException()
        {
            Basic.BitwiseAnd<BigInteger>(new BigInteger(0), new BigInteger(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseAndException1()
        {
            Basic.BitwiseAnd<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseAndException2()
        {
            Basic.BitwiseAnd<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseNotException()
        {
            Basic.BitwiseNot<byte>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseOrException1()
        {
            Basic.BitwiseOr<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseOrException2()
        {
            Basic.BitwiseOr<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseXorException1()
        {
            Basic.BitwiseXor<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseXorException2()
        {
            Basic.BitwiseXor<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestContainsException1()
        {
            SeqExtensions.Contains<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestContainsException2()
        {
            SeqExtensions.Contains(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestContainsKeyException1()
        {
            DictExtensions.ContainsKey<int, int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestContainsKeyException2()
        {
            DictExtensions.ContainsKey(Dict.Create<int, int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropException1()
        {
            SeqExtensions.Drop<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropException2()
        {
            SeqExtensions.Drop(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropWhileException1()
        {
            SeqExtensions.DropWhile<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropWhileException2()
        {
            SeqExtensions.DropWhile(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDuplicatesException1()
        {
            SeqExtensions.Duplicates<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDuplicatesException2()
        {
            SeqExtensions.Duplicates(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEqException1()
        {
            Basic.Eq<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEqException2()
        {
            Basic.Eq<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFindException1()
        {
            SeqExtensions.Find<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFindException2()
        {
            SeqExtensions.Find(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFoldException1()
        {
            SeqExtensions.Fold<int, int>(null, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFoldException2()
        {
            SeqExtensions.Fold<int, int>(Seq.Create<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFoldException3()
        {
            SeqExtensions.Fold<int, int>(Seq.Create<int>(), 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFunctionException()
        {
            new ZenFunction<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGeqException1()
        {
            Basic.Geq<byte>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGeqException2()
        {
            Basic.Geq<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGetException1()
        {
            DictExtensions.Get<int, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGetException2()
        {
            DictExtensions.Get(Dict.Create<int, int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGetFieldException1()
        {
            Basic.GetField<Object1, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGetFieldException2()
        {
            Basic.GetField<Object1, int>(Basic.Create<Object1>(("Field1", Constant(0))), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGtException1()
        {
            Basic.Gt<byte>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGtException2()
        {
            Basic.Gt<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestHasValueException()
        {
            OptionExtensions.HasValue<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIfException1()
        {
            Basic.If<int>(null, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIfException2()
        {
            Basic.If<int>(true, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIfException3()
        {
            Basic.If<int>(true, 3, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestImpliesException1()
        {
            Basic.Implies(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestImpliesException2()
        {
            Basic.Implies(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIndexOfException1()
        {
            SeqExtensions.IndexOf<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIndexOfException2()
        {
            SeqExtensions.IndexOf(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInsertException1()
        {
            SeqExtensions.Insert<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInsertException2()
        {
            SeqExtensions.Insert<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIntersperseException1()
        {
            SeqExtensions.Intersperse<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIntersperseException2()
        {
            SeqExtensions.Intersperse(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIsEmptyException1()
        {
            SeqExtensions.IsEmpty<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIsSortedException1()
        {
            SeqExtensions.IsSorted<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception1()
        {
            Zen<Pair<int, int>> x = null;
            PairExtensions.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception2()
        {
            Zen<Pair<int, int, int>> x = null;
            PairExtensions.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception3()
        {
            Zen<Pair<int, int, int, int>> x = null;
            PairExtensions.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception4()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            PairExtensions.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception1()
        {
            Zen<Pair<int, int>> x = null;
            PairExtensions.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception2()
        {
            Zen<Pair<int, int, int>> x = null;
            PairExtensions.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception3()
        {
            Zen<Pair<int, int, int, int>> x = null;
            PairExtensions.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception4()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            PairExtensions.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem3Exception1()
        {
            Zen<Pair<int, int, int>> x = null;
            PairExtensions.Item3(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem3Exception2()
        {
            Zen<Pair<int, int, int, int>> x = null;
            PairExtensions.Item3(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem3Exception3()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            PairExtensions.Item3(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem4Exception1()
        {
            Zen<Pair<int, int, int, int>> x = null;
            PairExtensions.Item4(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem4Exception2()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            PairExtensions.Item4(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem5Exception1()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            PairExtensions.Item5(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLengthException()
        {
            SeqExtensions.Length<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLeqException1()
        {
            Basic.Leq<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLeqException2()
        {
            Basic.Leq<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestListException()
        {
            Zen<int>[] x = null;
            Seq.Create<int>(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLtException1()
        {
            Basic.Lt<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLtException2()
        {
            Basic.Lt<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseOptionException1()
        {
            Basic.Case<int, int>(null, null, (Func<Zen<int>, Zen<int>>)null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseOptionException2()
        {
            Basic.Case<int, int>(Option.Null<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseOptionException3()
        {
            Basic.Case<int, int>(Option.Null<int>(), () => 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseListException1()
        {
            Zen<Seq<int>> x = null;
            SeqExtensions.Case<int, int>(x, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseListException2()
        {
            SeqExtensions.Case<int, int>(Seq.Create<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseListException3()
        {
            SeqExtensions.Case<int, int>(Seq.Create<int>(), 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMaxException1()
        {
            Basic.Max(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMaxException2()
        {
            Basic.Max(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinException1()
        {
            Basic.Min(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinException2()
        {
            Basic.Min(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinusException1()
        {
            Basic.Minus(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinusException2()
        {
            Basic.Minus(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMultiplyException1()
        {
            Basic.Multiply(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMultiplyException2()
        {
            Basic.Multiply(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestNotException()
        {
            Basic.Not(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestOrException1()
        {
            Basic.Or(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestOrException2()
        {
            Basic.Or(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestPlusException1()
        {
            Basic.Plus<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestPlusException2()
        {
            Basic.Plus<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestRemoveAllException1()
        {
            SeqExtensions.RemoveAll<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestRemoveAllException2()
        {
            SeqExtensions.RemoveAll(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestRemoveFirstException1()
        {
            SeqExtensions.RemoveFirst<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestRemoveFirstException2()
        {
            SeqExtensions.RemoveFirst(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestReverseException()
        {
            SeqExtensions.Reverse<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException1()
        {
            Zen<Option<int>> x = null;
            x.Select<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException2()
        {
            Zen<Option<int>> x = Option.Null<int>();
            x.Select<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException3()
        {
            Zen<Seq<int>> x = null;
            SeqExtensions.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException4()
        {
            Zen<Seq<int>> x = Seq.Create<int>();
            SeqExtensions.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSingletonException()
        {
            Zen<int> x = null;
            Seq.Create(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSomeException()
        {
            Option.Create<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSortException()
        {
            SeqExtensions.Sort<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSplitAtException1()
        {
            SeqExtensions.SplitAt<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSplitAtException2()
        {
            SeqExtensions.SplitAt(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeException1()
        {
            SeqExtensions.Take<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeException2()
        {
            SeqExtensions.Take(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeWhileException1()
        {
            SeqExtensions.TakeWhile<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeWhileException2()
        {
            SeqExtensions.TakeWhile(Seq.Create<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestToListException()
        {
            OptionExtensions.ToSequence<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTupleException1()
        {
            Pair.Create<int, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTupleException2()
        {
            Pair.Create<int, int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTupleValueException()
        {
            Basic.Value<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestValueOrDefaultException1()
        {
            OptionExtensions.ValueOrDefault<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestValueOrDefaultException2()
        {
            Option.Null<int>().ValueOrDefault(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException1()
        {
            Zen<Option<int>> x = null;
            x.Where(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException2()
        {
            Zen<Option<int>> x = Option.Null<int>();
            x.Where(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException3()
        {
            Zen<Seq<int>> x = null;
            SeqExtensions.Where(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException4()
        {
            Zen<Seq<int>> x = Seq.Create<int>();
            SeqExtensions.Where(x, null);
        }

        /// <summary>
        /// Can implicitly convert to function.
        /// </summary>
        [TestMethod]
        public void TestImplicitConversions()
        {
            Func<int> a = new ZenFunction<int>(() => 1);
            Func<int, int> b = new ZenFunction<int, int>((i) => 1);
            Func<int, int, int> c = new ZenFunction<int, int, int>((i1, i2) => 1);
            Func<int, int, int, int> d = new ZenFunction<int, int, int, int>((i1, i2, i3) => 1);
            Func<int, int, int, int, int> e = new ZenFunction<int, int, int, int, int>((i1, i2, i3, i4) => 1);

            a();
            b(1);
            c(1, 2);
            d(1, 2, 3);
            e(1, 2, 3, 4);
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs1()
        {
            var f = new ZenFunction<int, int>(i => 0);
            f.Find((i, o) => true, Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs2()
        {
            var f = new ZenFunction<int, int, int>((i1, i2) => 0);
            f.Find((i1, i2, o) => true, Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs3()
        {
            var f = new ZenFunction<int, int, int, int>((i1, i2, i3) => 0);
            f.Find((i1, i2, i3, o) => true, Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs4()
        {
            var f = new ZenFunction<int, int, int, int, int>((i1, i2, i3, i4) => 0);
            f.Find((i1, i2, i3, i4, o) => true, Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestExactLists()
        {
            var f = new ZenFunction<Seq<int>, ushort>(l => l.Length());
            f.Find((i, o) => true, checkSmallerLists: false);
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile0()
        {
            var f = new ZenFunction<int>(() => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate());
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile1()
        {
            var f = new ZenFunction<int, int>((i1) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0));
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile2()
        {
            var f = new ZenFunction<int, int, int>((i1, i2) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0, 0));
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile3()
        {
            var f = new ZenFunction<int, int, int, int>((i1, i2, i3) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0, 0, 0));
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile4()
        {
            var f = new ZenFunction<int, int, int, int, int>((i1, i2, i3, i4) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0, 0, 0, 0));
        }

        /// <summary>
        /// Exception thrown since implicit conversion won't work with object fields.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestImplicitObjectFieldConversionException()
        {
            Basic.Create<Object1>(("Field1", 0));
        }

        /// <summary>
        /// Exception thrown since we provide a value with the wrong Zen type.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCreationIncorrectType1()
        {
            Basic.Create<Object1>(("Field1", Constant(false)));
        }

        /// <summary>
        /// Exception thrown since we provide a value with the wrong Zen type.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCreationIncorrectType2()
        {
            Basic.Create<Object2>(("Field1", Constant(0)), ("Field2", Constant(false)));
        }

        /// <summary>
        /// Exception thrown since new field has the wrong type.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldZenType()
        {
            Basic.Create<Object1>(("Field1", Constant(0))).WithField("Field1", Constant(true));
        }
    }
}