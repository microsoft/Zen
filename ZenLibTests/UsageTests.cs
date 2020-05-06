// <copyright file="UsageTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Language;
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
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddException1()
        {
            Language.Add(null, Int(1), Int(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddException2()
        {
            Language.Add(EmptyDict<int, int>(), null, 1);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddException3()
        {
            Language.Add(EmptyDict<int, int>(), 1, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddBackException1()
        {
            Language.AddBack(null, Int(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddBackException2()
        {
            Language.AddBack(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddFrontException1()
        {
            Language.AddFront(null, Int(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddFrontException2()
        {
            Language.AddFront(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAllException1()
        {
            Language.All<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAllException2()
        {
            Language.All(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAnyException1()
        {
            Language.Any<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAnyException2()
        {
            Language.Any(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAndException1()
        {
            Language.And(null, true);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAndException2()
        {
            Language.And(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAppendException1()
        {
            Language.Append(null, EmptyList<int>());
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAppendException2()
        {
            Language.Append(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAtException1()
        {
            Language.At<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAtException2()
        {
            Language.At(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseAndException1()
        {
            Language.BitwiseAnd<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseAndException2()
        {
            Language.BitwiseAnd<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseNotException()
        {
            Language.BitwiseNot<byte>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseOrException1()
        {
            Language.BitwiseOr<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseOrException2()
        {
            Language.BitwiseOr<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseXorException1()
        {
            Language.BitwiseXor<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBitwiseXorException2()
        {
            Language.BitwiseXor<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestContainsException1()
        {
            Language.Contains<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestContainsException2()
        {
            Language.Contains(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestContainsKeyException1()
        {
            Language.ContainsKey<int, int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestContainsKeyException2()
        {
            Language.ContainsKey(EmptyDict<int, int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDropException1()
        {
            Language.Drop<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDropException2()
        {
            Language.Drop(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDropWhileException1()
        {
            Language.DropWhile<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDropWhileException2()
        {
            Language.DropWhile(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDuplicatesException1()
        {
            Language.Duplicates<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDuplicatesException2()
        {
            Language.Duplicates(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEqException1()
        {
            Language.Eq<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEqException2()
        {
            Language.Eq<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFindException1()
        {
            Language.Find<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFindException2()
        {
            Language.Find(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFoldException1()
        {
            Language.Fold<int, int>(null, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFoldException2()
        {
            Language.Fold<int, int>(EmptyList<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFoldException3()
        {
            Language.Fold<int, int>(EmptyList<int>(), 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFunctionException()
        {
            Language.Function<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGeqException1()
        {
            Language.Geq<byte>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGeqException2()
        {
            Language.Geq<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetException1()
        {
            Language.Get<int, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetException2()
        {
            Language.Get(EmptyDict<int, int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetFieldException1()
        {
            Language.GetField<Object1, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetFieldException2()
        {
            Language.GetField<Object1, int>(Language.Create<Object1>(("Field1", Int(0))), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGtException1()
        {
            Language.Gt<byte>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGtException2()
        {
            Language.Gt<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestHasValueException()
        {
            Language.HasValue<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIfException1()
        {
            Language.If<int>(null, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIfException2()
        {
            Language.If<int>(true, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIfException3()
        {
            Language.If<int>(true, 3, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestImpliesException1()
        {
            Language.Implies(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestImpliesException2()
        {
            Language.Implies(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIndexOfException1()
        {
            Language.IndexOf<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIndexOfException2()
        {
            Language.IndexOf(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInsertException1()
        {
            Language.Insert<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInsertException2()
        {
            Language.Insert<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIntersperseException1()
        {
            Language.Intersperse<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIntersperseException2()
        {
            Language.Intersperse(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIsEmptyException1()
        {
            Language.IsEmpty<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIsSortedException1()
        {
            Language.IsSorted<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestItem1Exception1()
        {
            Zen<Tuple<int, int>> x = null;
            Language.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestItem1Exception2()
        {
            Zen<ValueTuple<int, int>> x = null;
            Language.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestItem2Exception1()
        {
            Zen<Tuple<int, int>> x = null;
            Language.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestItem2Exception2()
        {
            Zen<ValueTuple<int, int>> x = null;
            Language.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLengthException()
        {
            Language.Length<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLeqException1()
        {
            Language.Leq<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLeqException2()
        {
            Language.Leq<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestListException()
        {
            Language.List<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestListToDictionaryException()
        {
            Language.ListToDictionary<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLtException1()
        {
            Language.Lt<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLtException2()
        {
            Language.Lt<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseOptionException1()
        {
            Language.Case<int, int>(null, null, (Func<Zen<int>, Zen<int>>)null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseOptionException2()
        {
            Language.Case<int, int>(Language.Null<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseOptionException3()
        {
            Language.Case<int, int>(Language.Null<int>(), () => 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseListException1()
        {
            Zen<IList<int>> x = null;
            Language.Case<int, int>(x, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseListException2()
        {
            Language.Case<int, int>(EmptyList<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseListException3()
        {
            Language.Case<int, int>(EmptyList<int>(), 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMaxException1()
        {
            Language.Max(null, Byte(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMaxException2()
        {
            Language.Max(Byte(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMinException1()
        {
            Language.Min(null, Byte(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMinException2()
        {
            Language.Min(Byte(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMinusException1()
        {
            Language.Minus(null, Byte(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMinusException2()
        {
            Language.Minus(Byte(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMultiplyException1()
        {
            Language.Multiply(null, Byte(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMultiplyException2()
        {
            Language.Multiply(Byte(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNotException()
        {
            Language.Not(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestOrException1()
        {
            Language.Or(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestOrException2()
        {
            Language.Or(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPlusException1()
        {
            Language.Plus<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPlusException2()
        {
            Language.Plus<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveAllException1()
        {
            Language.RemoveAll<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveAllException2()
        {
            Language.RemoveAll(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveFirstException1()
        {
            Language.RemoveFirst<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveFirstException2()
        {
            Language.RemoveFirst(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestReverseException()
        {
            Language.Reverse<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSelectException1()
        {
            Zen<Option<int>> x = null;
            Language.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSelectException2()
        {
            Zen<Option<int>> x = Null<int>();
            Language.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSelectException3()
        {
            Zen<IList<int>> x = null;
            Language.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSelectException4()
        {
            Zen<IList<int>> x = EmptyList<int>();
            Language.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSingletonException()
        {
            Language.Singleton<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSomeException()
        {
            Language.Some<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSortException()
        {
            Language.Sort<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSplitAtException1()
        {
            Language.SplitAt<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSplitAtException2()
        {
            Language.SplitAt(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTakeException1()
        {
            Language.Take<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTakeException2()
        {
            Language.Take(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTakeWhileException1()
        {
            Language.TakeWhile<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTakeWhileException2()
        {
            Language.TakeWhile(EmptyList<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestToListException()
        {
            Language.ToList<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTupleException1()
        {
            Language.Tuple<int, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTupleException2()
        {
            Language.Tuple<int, int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTupleToOptionException1()
        {
            Language.TupleToOption<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTupleToOptionException2()
        {
            Language.TupleToOption<int>(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTupleValueException()
        {
            Language.Value<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValueOrDefaultException1()
        {
            Language.ValueOrDefault<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValueOrDefaultException2()
        {
            Language.ValueOrDefault(Null<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValueTupleException1()
        {
            Language.ValueTuple<int, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValueTupleException2()
        {
            Language.ValueTuple<int, int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhereException1()
        {
            Zen<Option<int>> x = null;
            Language.Where(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhereException2()
        {
            Zen<Option<int>> x = Null<int>();
            Language.Where(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhereException3()
        {
            Zen<IList<int>> x = null;
            Language.Where(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhereException4()
        {
            Zen<IList<int>> x = EmptyList<int>();
            Language.Where(x, null);
        }

        /// <summary>
        /// Can implicitly convert to function.
        /// </summary>
        [TestMethod]
        public void TestImplicitConversions()
        {
            Func<int> a = Function<int>(() => 1);
            Func<int, int> b = Function<int, int>((i) => 1);
            Func<int, int, int> c = Function<int, int, int>((i1, i2) => 1);
            Func<int, int, int, int> d = Function<int, int, int, int>((i1, i2, i3) => 1);
            Func<int, int, int, int, int> e = Function<int, int, int, int, int>((i1, i2, i3, i4) => 1);

            a();
            b(1);
            c(1, 2);
            d(1, 2, 3);
            e(1, 2, 3, 4);
        }

        /// <summary>
        /// Exception thrown for non-integer parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidIntegerConversion()
        {
            Zen<IList<int>> x = 0;
        }

        /// <summary>
        /// Exception thrown for non-integer/bool parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidEquality()
        {
            _ = EmptyList<int>() == EmptyList<int>();
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs1()
        {
            var f = Function<int, int>(i => 0);
            f.Find((i, o) => true, Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs2()
        {
            var f = Function<int, int, int>((i1, i2) => 0);
            f.Find((i1, i2, o) => true, Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs3()
        {
            var f = Function<int, int, int, int>((i1, i2, i3) => 0);
            f.Find((i1, i2, i3, o) => true, Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs4()
        {
            var f = Function<int, int, int, int, int>((i1, i2, i3, i4) => 0);
            f.Find((i1, i2, i3, i4, o) => true, Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestExactLists()
        {
            var f = Function<IList<int>, ushort>(l => l.Length());
            f.Find((i, o) => true, checkSmallerLists: false);
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile0()
        {
            var f = Function<int>(() => 1);
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
            var f = Function<int, int>((i1) => 1);
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
            var f = Function<int, int, int>((i1, i2) => 1);
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
            var f = Function<int, int, int, int>((i1, i2, i3) => 1);
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
            var f = Function<int, int, int, int, int>((i1, i2, i3, i4) => 1);
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
            Language.Create<Object1>(("Field1", 0));
        }
    }
}