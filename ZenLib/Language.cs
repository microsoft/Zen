// <copyright file="ZenFunctions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using ZenLib.Generation;

    /// <summary>
    /// Collection of helper functions for building Zen programs.
    /// </summary>
    public static class Language
    {
        private static MethodInfo getFieldMethod = typeof(Language).GetMethod("GetField");

        private static MethodInfo eqMethod = typeof(Language).GetMethod("Eq");

        private static MethodInfo eqBoolMethod = typeof(Language).GetMethod("Eq").MakeGenericMethod(typeof(bool));

        private static MethodInfo eqListsMethod = typeof(Language).GetMethod("EqLists", BindingFlags.Static | BindingFlags.NonPublic);

        private static MethodInfo hasValueMethod = typeof(Language).GetMethod("HasValue");

        private static MethodInfo valueMethod = typeof(Language).GetMethod("Value");

        private static MethodInfo tupItem1Method = typeof(Language).GetMethod("TupleItem1", BindingFlags.Static | BindingFlags.NonPublic);

        private static MethodInfo tupItem2Method = typeof(Language).GetMethod("TupleItem2", BindingFlags.Static | BindingFlags.NonPublic);

        private static MethodInfo valueTupItem1Method = typeof(Language).GetMethod("ValueTupleItem1", BindingFlags.Static | BindingFlags.NonPublic);

        private static MethodInfo valueTupItem2Method = typeof(Language).GetMethod("ValueTupleItem2", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The Zen value for false.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<bool> False()
        {
            return ZenConstantBoolExpr.False;
        }

        /// <summary>
        /// The Zen value for true.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<bool> True()
        {
            return ZenConstantBoolExpr.True;
        }

        /// <summary>
        /// The Zen value for a bool.
        /// </summary>
        /// <param name="b">A bool value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Bool(bool b)
        {
            return b ? True() : False();
        }

        /// <summary>
        /// The Zen value for a byte.
        /// </summary>
        /// <param name="b">A byte value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<byte> Byte(byte b)
        {
            return ZenConstantByteExpr.Create(b);
        }

        /// <summary>
        /// The Zen value for a ushort.
        /// </summary>
        /// <param name="s">A ushort value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> UShort(ushort s)
        {
            return ZenConstantUshortExpr.Create(s);
        }

        /// <summary>
        /// The Zen value for a short.
        /// </summary>
        /// <param name="s">A short value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> Short(short s)
        {
            return ZenConstantShortExpr.Create(s);
        }

        /// <summary>
        /// The Zen value for a uint.
        /// </summary>
        /// <param name="i">A uint value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<uint> UInt(uint i)
        {
            return ZenConstantUintExpr.Create(i);
        }

        /// <summary>
        /// The Zen value for an int.
        /// </summary>
        /// <param name="i">An int value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<int> Int(int i)
        {
            return ZenConstantIntExpr.Create(i);
        }

        /// <summary>
        /// The Zen value for a ulong.
        /// </summary>
        /// <param name="l">A ulong value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ulong> ULong(ulong l)
        {
            return ZenConstantUlongExpr.Create(l);
        }

        /// <summary>
        /// The Zen value for a long.
        /// </summary>
        /// <param name="l">A long value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<long> Long(long l)
        {
            return ZenConstantLongExpr.Create(l);
        }

        /// <summary>
        /// The Zen value for a string.
        /// </summary>
        /// <param name="s">A string value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> String(string s)
        {
            CommonUtilities.ValidateNotNull(s);
            CommonUtilities.ValidateStringLiteral(s);
            return ZenConstantStringExpr.Create(s);
        }

        /// <summary>
        /// A Zen object representing some arbitrary value.
        /// </summary>
        /// <param name="listSize">Depth bound on the size of the object.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Arbitrary<T>(int listSize = 5, bool checkSmallerLists = true)
        {
            var generator = new SymbolicInputGenerator(listSize, checkSmallerLists);
            return Arbitrary<T>(generator);
        }

        internal static Zen<T> Arbitrary<T>(SymbolicInputGenerator generator)
        {
            return (Zen<T>)ReflectionUtilities.ApplyTypeVisitor(generator, typeof(T));
        }

        /// <summary>
        /// The Zen value for null.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Null<T>()
        {
            var b = False();
            var v = (Zen<T>)ReflectionUtilities.ApplyTypeVisitor(new DefaultTypeGenerator(), typeof(T));
            return ZenAdapterExpr<Option<T>, CustomTuple<bool, T>>.Create(
                ZenCreateObjectExpr<CustomTuple<bool, T>>.Create(("Item1", b), ("Item2", v)),
                CustomTupleToOption<T>);
        }

        /// <summary>
        /// The Zen expression for Some value.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Some<T>(Zen<T> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return ZenAdapterExpr<Option<T>, CustomTuple<bool, T>>.Create(
                ZenCreateObjectExpr<CustomTuple<bool, T>>.Create(("Item1", True()), ("Item2", expr)),
                CustomTupleToOption<T>);
        }

        /// <summary>
        /// The Zen expression for an option create from a tuple.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Zen<Option<T>> TupleToOption<T>(Zen<bool> flag, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(flag);
            CommonUtilities.ValidateNotNull(value);

            return ZenAdapterExpr<Option<T>, CustomTuple<bool, T>>.Create(
                ZenCreateObjectExpr<CustomTuple<bool, T>>.Create(("Item1", flag), ("Item2", value)),
                CustomTupleToOption<T>);
        }

        /// <summary>
        /// The Zen expression for an option match.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="none">The none case.</param>
        /// <param name="some">The some case.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Case<T1, T2>(this Zen<Option<T1>> expr, Func<Zen<T2>> none, Func<Zen<T1>, Zen<T2>> some)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(none);
            CommonUtilities.ValidateNotNull(some);

            var tupleExpr = ZenAdapterExpr<CustomTuple<bool, T1>, Option<T1>>.Create(expr, OptionToCustomTuple<T1>);
            return If(
                tupleExpr.GetField<CustomTuple<bool, T1>, bool>("Item1"),
                some(tupleExpr.GetField<CustomTuple<bool, T1>, T1>("Item2")),
                none());
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> HasValue<T>(this Zen<Option<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var tupleExpr = ZenAdapterExpr<CustomTuple<bool, T>, Option<T>>.Create(expr, OptionToCustomTuple<T>);
            return tupleExpr.GetField<CustomTuple<bool, T>, bool>("Item1");
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> ToList<T>(this Zen<Option<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var tupleExpr = ZenAdapterExpr<CustomTuple<bool, T>, Option<T>>.Create(expr, OptionToCustomTuple<T>);
            var emptyList = EmptyList<T>();
            return If(
                    tupleExpr.GetField<CustomTuple<bool, T>, bool>("Item1"),
                    emptyList.AddFront(tupleExpr.GetField<CustomTuple<bool, T>, T>("Item2")),
                    emptyList);
        }

        /// <summary>
        /// The Zen expression for an option or a default if no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Value<T>(this Zen<Option<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var tupleExpr = ZenAdapterExpr<CustomTuple<bool, T>, Option<T>>.Create(expr, OptionToCustomTuple<T>);
            return tupleExpr.GetField<CustomTuple<bool, T>, T>("Item2");
        }

        /// <summary>
        /// The Zen expression for an option or a default if no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="deflt">The default value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> ValueOrDefault<T>(this Zen<Option<T>> expr, Zen<T> deflt)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(deflt);

            var tupleExpr = ZenAdapterExpr<CustomTuple<bool, T>, Option<T>>.Create(expr, OptionToCustomTuple<T>);
            return If(
                    tupleExpr.GetField<CustomTuple<bool, T>, bool>("Item1"),
                    tupleExpr.GetField<CustomTuple<bool, T>, T>("Item2"),
                    deflt);
        }

        /// <summary>
        /// The Zen expression for mapping over an option.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T2>> Select<T1, T2>(this Zen<Option<T1>> expr, Func<Zen<T1>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(function);

            var tupleExpr = ZenAdapterExpr<CustomTuple<bool, T1>, Option<T1>>.Create(expr, OptionToCustomTuple<T1>);
            return If(
                    tupleExpr.GetField<CustomTuple<bool, T1>, bool>("Item1"),
                    Some(function(tupleExpr.GetField<CustomTuple<bool, T1>, T1>("Item2"))),
                    Null<T2>());
        }

        /// <summary>
        /// The Zen expression for filtering over an option.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Where<T>(this Zen<Option<T>> expr, Func<Zen<T>, Zen<bool>> function)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(function);

            var tupleExpr = ZenAdapterExpr<CustomTuple<bool, T>, Option<T>>.Create(expr, OptionToCustomTuple<T>);
            return If(
                    And(tupleExpr.GetField<CustomTuple<bool, T>, bool>("Item1"),
                        function(tupleExpr.GetField<CustomTuple<bool, T>, T>("Item2"))),
                    expr,
                    Null<T>());
        }

        /// <summary>
        /// The Zen value for an empty List.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> EmptyList<T>()
        {
            return ZenListEmptyExpr<T>.Instance;
        }

        /// <summary>
        /// The Zen value for an empty Dictionary.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<IDictionary<TKey, TValue>> EmptyDict<TKey, TValue>()
        {
            var emptyList = EmptyList<Tuple<TKey, TValue>>();
            return ZenAdapterExpr<IDictionary<TKey, TValue>, IList<Tuple<TKey, TValue>>>.Create(emptyList, ListToDict<TKey, TValue>);
        }

        /// <summary>
        /// Compute the and of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> And(Zen<bool> expr1, Zen<bool> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenAndExpr.Create(expr1, expr2);
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(Zen<bool> expr1, Zen<bool> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenOrExpr.Create(expr1, expr2);
        }

        /// <summary>
        /// Compute the and of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> And(params Zen<bool>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            return exprs.Aggregate(And);
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(params Zen<bool>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            return exprs.Aggregate(Or);
        }

        /// <summary>
        /// Compute the not of a Zen value.
        /// </summary>
        /// <param name="expr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Not(Zen<bool> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return ZenNotExpr.Create(expr);
        }

        /// <summary>
        /// Compute the implication of a Zen value.
        /// </summary>
        /// <param name="guardExpr">Zen guard expression.</param>
        /// <param name="thenExpr">Zen then expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Implies(Zen<bool> guardExpr, Zen<bool> thenExpr)
        {
            CommonUtilities.ValidateNotNull(guardExpr);
            CommonUtilities.ValidateNotNull(thenExpr);

            return ZenIfExpr<bool>.Create(guardExpr, thenExpr, True());
        }

        /// <summary>
        /// Compute the if-then-else of a Zen value.
        /// </summary>
        /// <param name="guardExpr">Zen guard expression.</param>
        /// <param name="trueExpr">Zen true expression.</param>
        /// <param name="falseExpr">Zen false expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> If<T>(Zen<bool> guardExpr, Zen<T> trueExpr, Zen<T> falseExpr)
        {
            CommonUtilities.ValidateNotNull(guardExpr);
            CommonUtilities.ValidateNotNull(trueExpr);
            CommonUtilities.ValidateNotNull(falseExpr);

            return ZenIfExpr<T>.Create(guardExpr, trueExpr, falseExpr);
        }

        /// <summary>
        /// Compute the equality of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Eq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            return EqHelper<T>(expr1, expr2);
        }

        private static Zen<bool> EqLists<T>(Zen<IList<T>> expr1, Zen<IList<T>> expr2)
        {
            return expr1.Case(
                empty: expr2.IsEmpty(),
                cons: (hd1, tl1) =>
                    expr2.Case(empty: false, cons: (hd2, tl2) => And(hd1 == hd2, EqLists(tl1, tl2))));
        }

        private static Zen<bool> EqHelper<T>(object expr1, object expr2)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.BoolType || type == ReflectionUtilities.StringType || ReflectionUtilities.IsIntegerType(type))
            {
                return ZenComparisonExpr<T>.Create((dynamic)expr1, (dynamic)expr2, ComparisonType.Eq);
            }

            if (ReflectionUtilities.IsOptionType(type))
            {
                var innerType = type.GetGenericArguments()[0];

                var method = hasValueMethod.MakeGenericMethod(innerType);
                var hasValue1 = method.Invoke(null, new object[] { expr1 });
                var hasValue2 = method.Invoke(null, new object[] { expr2 });
                var eqBool = (Zen<bool>)eqBoolMethod.Invoke(null, new object[] { hasValue1, hasValue2 });

                method = valueMethod.MakeGenericMethod(innerType);
                var equals = eqMethod.MakeGenericMethod(innerType);
                var value1 = method.Invoke(null, new object[] { expr1 });
                var value2 = method.Invoke(null, new object[] { expr2 });

                var eqValue = (Zen<bool>)equals.Invoke(null, new object[] { value1, value2 });
                return And(eqBool, eqValue);
            }

            if (ReflectionUtilities.IsSomeTupleType(type))
            {
                var isTuple = ReflectionUtilities.IsTupleType(type);
                var item1Method = isTuple ? tupItem1Method : valueTupItem1Method;
                var item2Method = isTuple ? tupItem2Method : valueTupItem2Method;

                var genericArgs = type.GetGenericArguments();
                var innerType1 = genericArgs[0];
                var innerType2 = genericArgs[1];
                var equals1 = eqMethod.MakeGenericMethod(innerType1);
                var equals2 = eqMethod.MakeGenericMethod(innerType2);
                var method1 = item1Method.MakeGenericMethod(innerType1, innerType2);
                var method2 = item2Method.MakeGenericMethod(innerType1, innerType2);

                var e11 = method1.Invoke(null, new object[] { expr1 });
                var e12 = method1.Invoke(null, new object[] { expr2 });

                var e21 = method2.Invoke(null, new object[] { expr1 });
                var e22 = method2.Invoke(null, new object[] { expr2 });

                var eqItem1 = (Zen<bool>)equals1.Invoke(null, new object[] { e11, e12 });
                var eqItem2 = (Zen<bool>)equals2.Invoke(null, new object[] { e21, e22 });
                return And(eqItem1, eqItem2);
            }

            if (ReflectionUtilities.IsIListType(type))
            {
                var innerType = type.GetGenericArguments()[0];
                var method = eqListsMethod.MakeGenericMethod(innerType);
                return (Zen<bool>)method.Invoke(null, new object[] { expr1, expr2 });
            }

            if (ReflectionUtilities.IsIDictionaryType(type))
            {
                throw new ZenException($"Zen does not support equality of {type} types.");
            }

            // some class or struct
            var acc = True();
            foreach (var field in ReflectionUtilities.GetAllFields(type))
            {
                var method = getFieldMethod.MakeGenericMethod(typeof(T), field.FieldType);
                dynamic field1 = method.Invoke(null, new object[] { expr1, field.Name });
                dynamic field2 = method.Invoke(null, new object[] { expr2, field.Name });
                var emethod = eqMethod.MakeGenericMethod(field.FieldType);
                acc = And(acc, (Zen<bool>)emethod.Invoke(null, new object[] { field1, field2 }));
            }

            foreach (var property in ReflectionUtilities.GetAllProperties(type))
            {
                var method = getFieldMethod.MakeGenericMethod(typeof(T), property.PropertyType);
                dynamic prop1 = method.Invoke(null, new object[] { expr1, property.Name });
                dynamic prop2 = method.Invoke(null, new object[] { expr2, property.Name });
                var emethod = eqMethod.MakeGenericMethod(property.PropertyType);
                acc = And(acc, (Zen<bool>)emethod.Invoke(null, new object[] { prop1, prop2 }));
            }

            return acc;
        }

        /// <summary>
        /// Compute the less than or equal of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Leq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Leq);
        }

        /// <summary>
        /// Compute the less than of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Lt<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return And(Leq(expr1, expr2), expr1 != expr2);
        }

        /// <summary>
        /// Compute the greater than of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Gt<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return Not(Leq(expr1, expr2));
        }

        /// <summary>
        /// Compute the greater than or equal of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Geq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Geq);
        }

        /// <summary>
        /// Compute the sum of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Plus<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.Addition);
        }

        /// <summary>
        /// Compute the concatenation of two Zen strings.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> Concat(Zen<string> expr1, Zen<string> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenConcatExpr.Create(expr1, expr2);
        }

        /// <summary>
        /// Check if one string starts with another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> StartsWith(this Zen<string> str, Zen<string> substr)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);

            return ZenStringContainmentExpr.Create(str, substr, ContainmentType.PrefixOf);
        }

        /// <summary>
        /// Check if one string ends with another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> EndsWith(this Zen<string> str, Zen<string> substr)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);

            return ZenStringContainmentExpr.Create(str, substr, ContainmentType.SuffixOf);
        }

        /// <summary>
        /// Check if one string contains another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains(this Zen<string> str, Zen<string> substr)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);

            return ZenStringContainmentExpr.Create(str, substr, ContainmentType.Contains);
        }

        /// <summary>
        /// Replace a substring with another string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <param name="replace">The replacement string.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> ReplaceFirst(this Zen<string> str, Zen<string> substr, Zen<string> replace)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);
            CommonUtilities.ValidateNotNull(replace);

            return ZenStringReplaceExpr.Create(str, substr, replace);
        }

        /// <summary>
        /// Get a substring from a string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="offset">The offset Zen expression.</param>
        /// <param name="length">The length Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> Substring(this Zen<string> str, Zen<ushort> offset, Zen<ushort> length)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(offset);
            CommonUtilities.ValidateNotNull(length);

            return ZenStringSubstringExpr.Create(str, offset, length);
        }

        /// <summary>
        /// Get the character for a string at a given index.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="index">The index Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> At(this Zen<string> str, Zen<ushort> index)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(index);

            return ZenStringAtExpr.Create(str, index);
        }

        /// <summary>
        /// Get the length of a string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Length(this Zen<string> str)
        {
            CommonUtilities.ValidateNotNull(str);

            return ZenStringLengthExpr.Create(str);
        }

        /// <summary>
        /// Get the index of a substring from an offset.
        /// If the substring is empty and the offset is in bounds then returns the offset.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="sub">The substring Zen expression.</param>
        /// <param name="offset">The offset Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> IndexOf(this Zen<string> str, Zen<string> sub, Zen<ushort> offset)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(sub);
            CommonUtilities.ValidateNotNull(offset);

            return ZenStringIndexOfExpr.Create(str, sub, offset);
        }

        /// <summary>
        /// Get the index of a substring.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="sub">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> IndexOf(this Zen<string> str, Zen<string> sub)
        {
            return IndexOf(str, sub, 0);
        }

        /// <summary>
        /// Compute the difference of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Minus<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.Subtraction);
        }

        /// <summary>
        /// Compute the multiplication of a Zen value with a constant.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Multiply<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.Multiplication);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<byte> Max(Zen<byte> expr1, Zen<byte> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> Max(Zen<short> expr1, Zen<short> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Max(Zen<ushort> expr1, Zen<ushort> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<int> Max(Zen<int> expr1, Zen<int> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<uint> Max(Zen<uint> expr1, Zen<uint> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<long> Max(Zen<long> expr1, Zen<long> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ulong> Max(Zen<ulong> expr1, Zen<ulong> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<byte> Min(Zen<byte> expr1, Zen<byte> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> Min(Zen<short> expr1, Zen<short> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Min(Zen<ushort> expr1, Zen<ushort> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<int> Min(Zen<int> expr1, Zen<int> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<uint> Min(Zen<uint> expr1, Zen<uint> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<long> Min(Zen<long> expr1, Zen<long> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ulong> Min(Zen<ulong> expr1, Zen<ulong> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the bitwise and of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseAnd<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.BitwiseAnd);
        }

        /// <summary>
        /// Compute the bitwise and of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseAnd<T>(params Zen<T>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            return exprs.Aggregate(BitwiseAnd);
        }

        /// <summary>
        /// Compute the bitwise or of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseOr<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.BitwiseOr);
        }

        /// <summary>
        /// Compute the bitwise or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseOr<T>(params Zen<T>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            return exprs.Aggregate(BitwiseOr);
        }

        /// <summary>
        /// Compute the bitwise not of a Zen value.
        /// </summary>
        /// <param name="expr">First Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseNot<T>(Zen<T> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return ZenBitwiseNotExpr<T>.Create(expr);
        }

        /// <summary>
        /// Compute the bitwise xor of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseXor<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.BitwiseXor);
        }

        /// <summary>
        /// Compute the bitwise xor of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseXor<T>(params Zen<T>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            return exprs.Aggregate(BitwiseXor);
        }

        /// <summary>
        /// Add a value to the back of a Zen list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> AddBack<T>(this Zen<IList<T>> listExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return listExpr.Case(
                empty: Singleton(valueExpr),
                cons: (hd, tl) => AddBack(tl, valueExpr).AddFront(hd));
        }

        /// <summary>
        /// Add a value to the front of a Zen list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> AddFront<T>(this Zen<IList<T>> listExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return ZenListAddFrontExpr<T>.Create(listExpr, valueExpr);
        }

        /// <summary>
        /// Count the occurences of an element in a list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Duplicates<T>(this Zen<IList<T>> listExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return listExpr.Case(
                empty: UShort(0),
                cons: (hd, tl) =>
                    If(hd == valueExpr, tl.Duplicates(valueExpr), tl.Duplicates(valueExpr) + UShort(1)));
        }

        /// <summary>
        /// Remove the first instance of a value from a list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> RemoveFirst<T>(this Zen<IList<T>> listExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return listExpr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) => If(hd == valueExpr, tl, tl.RemoveFirst(valueExpr).AddFront(hd)));
        }

        /// <summary>
        /// Remove the first instance of a value from a list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> RemoveAll<T>(this Zen<IList<T>> listExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return listExpr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) =>
                {
                    var tlRemoved = tl.RemoveAll(valueExpr);
                    return If(hd == valueExpr, tlRemoved, tlRemoved.AddFront(hd));
                });
        }

        /// <summary>
        /// Match and deconstruct a Zen list.
        /// </summary>
        /// <param name="listExpr">The list expression.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        /// <returns>Zen value.</returns>
        public static Zen<TResult> Case<T, TResult>(
            this Zen<IList<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> cons)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(empty);
            CommonUtilities.ValidateNotNull(cons);

            return ZenListCaseExpr<T, TResult>.Create(listExpr, empty, cons);
        }

        /// <summary>
        /// Map over a list to create a new list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="function">The map function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T2>> Select<T1, T2>(this Zen<IList<T1>> listExpr, Func<Zen<T1>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(function);

            return listExpr.Case(
                empty: EmptyList<T2>(),
                cons: (hd, tl) => tl.Select(function).AddFront(function(hd)));
        }

        /// <summary>
        /// Filter a list to create a new list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="predicate">The filtering function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Where<T>(this Zen<IList<T>> listExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return listExpr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) =>
                {
                    var x = tl.Where(predicate);
                    return If(predicate(hd), x.AddFront(hd), x);
                });
        }

        /// <summary>
        /// Find an element that satisfies a predicate.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="predicate">The filtering function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Find<T>(this Zen<IList<T>> listExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return listExpr.Case(
                empty: Null<T>(),
                cons: (hd, tl) => If(predicate(hd), Some(hd), tl.Find(predicate)));
        }

        /// <summary>
        /// Get the length of the list.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Length<T>(this Zen<IList<T>> listExpr)
        {
            CommonUtilities.ValidateNotNull(listExpr);

            return listExpr.Case(
                empty: UShort(0),
                cons: (hd, tl) => tl.Length() + 1);
        }

        /// <summary>
        /// Check if a list contains an element.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="element">The element.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<IList<T>> listExpr, Zen<T> element)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(element);

            return listExpr.Any((x) => Eq(x, element));
        }

        /// <summary>
        /// Append two lists together.
        /// </summary>
        /// <param name="expr1">First Zen list expression.</param>
        /// <param name="expr2">Second Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Append<T>(this Zen<IList<T>> expr1, Zen<IList<T>> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return expr1.Case(
                empty: expr2,
                cons: (hd, tl) => tl.Append(expr2).AddFront(hd));
        }

        /// <summary>
        /// Whether a list is empty.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsEmpty<T>(this Zen<IList<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.Case(empty: True(), cons: (hd, tl) => False());
        }

        /// <summary>
        /// Create a singleton list.
        /// </summary>
        /// <param name="element">Zen element.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Singleton<T>(this Zen<T> element)
        {
            CommonUtilities.ValidateNotNull(element);

            return EmptyList<T>().AddFront(element);
        }

        /// <summary>
        /// Create a list from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> List<T>(params Zen<T>[] elements)
        {
            CommonUtilities.ValidateNotNull(elements);

            Zen<T>[] copy = new Zen<T>[elements.Length];
            Array.Copy(elements, copy, elements.Length);
            Array.Reverse(copy);
            var list = EmptyList<T>();
            foreach (var element in copy)
            {
                list = list.AddFront(element);
            }

            return list;
        }

        /// <summary>
        /// Reverse a list.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Reverse<T>(this Zen<IList<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return Reverse(expr, EmptyList<T>());
        }

        private static Zen<IList<T>> Reverse<T>(this Zen<IList<T>> expr, Zen<IList<T>> acc)
        {
            return expr.Case(
                empty: acc,
                cons: (hd, tl) => tl.Reverse(acc.AddFront(hd)));
        }

        /// <summary>
        /// Intersperse a list with an element.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="element">The element to intersperse.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Intersperse<T>(this Zen<IList<T>> expr, Zen<T> element)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(element);

            return expr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) => If(IsEmpty(tl), Singleton(hd), tl.Intersperse(element).AddFront(hd)));
        }

        /// <summary>
        /// Fold a function over a Zen list.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="init">The initial value.</param>
        /// <param name="function">The fold function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Fold<T1, T2>(this Zen<IList<T1>> expr, Zen<T2> init, Func<Zen<T1>, Zen<T2>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(init);
            CommonUtilities.ValidateNotNull(function);

            return expr.Case(
                empty: init,
                cons: (hd, tl) => function(hd, tl.Fold(init, function)));
        }

        /// <summary>
        /// Check if any element satisfies a predicate.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="predicate">The predicate to check.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Any<T>(this Zen<IList<T>> expr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(predicate);

            return expr.Fold(False(), (x, y) => Or(predicate(x), y));
        }

        /// <summary>
        /// Check if all elements satisfy a predicate.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="predicate">The predicate to check.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> All<T>(this Zen<IList<T>> expr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(predicate);

            return expr.Fold(True(), (x, y) => And(predicate(x), y));
        }

        /// <summary>
        /// Take n elements from a list.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Take<T>(this Zen<IList<T>> expr, Zen<ushort> numElements)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(numElements);

            return Take(expr, numElements, 0);
        }

        /// <summary>
        /// Take n elements from a list.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<IList<T>> Take<T>(this Zen<IList<T>> expr, Zen<ushort> numElements, int i)
        {
            return expr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) => If(UShort((ushort)i) == numElements, EmptyList<T>(), tl.Take(numElements, i + 1).AddFront(hd)));
        }

        /// <summary>
        /// Take elements from a list while a predicate is true.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> TakeWhile<T>(this Zen<IList<T>> expr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(predicate);

            return expr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) => If(predicate(hd), tl.TakeWhile(predicate).AddFront(hd), EmptyList<T>()));
        }

        /// <summary>
        /// Drop n elements from a list.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Drop<T>(this Zen<IList<T>> expr, Zen<ushort> numElements)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(numElements);

            return Drop(expr, numElements, 0);
        }

        /// <summary>
        /// Drop n elements from a list.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<IList<T>> Drop<T>(this Zen<IList<T>> expr, Zen<ushort> numElements, int i)
        {
            return expr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) => If(UShort((ushort)i) == numElements, expr, tl.Drop(numElements, i + 1)));
        }

        /// <summary>
        /// Drop elements from a list while a predicate is true.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> DropWhile<T>(this Zen<IList<T>> expr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(predicate);

            return expr.Case(
                empty: EmptyList<T>(),
                cons: (hd, tl) => If(predicate(hd), EmptyList<T>(), tl.DropWhile(predicate).AddFront(hd)));
        }

        /// <summary>
        /// Split a list at an element.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="index">The index to split at.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Tuple<IList<T>, IList<T>>> SplitAt<T>(this Zen<IList<T>> expr, Zen<ushort> index)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(index);

            return SplitAt(expr, index, 0);
        }

        /// <summary>
        /// Split a list at an element.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="index">The index to split at.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Tuple<IList<T>, IList<T>>> SplitAt<T>(this Zen<IList<T>> expr, Zen<ushort> index, int i)
        {
            return expr.Case(
                empty: Tuple(EmptyList<T>(), EmptyList<T>()),
                cons: (hd, tl) =>
                {
                    // splitat([x,y,z], 1)
                    // ([x,y], [z])
                    //
                    var tup = tl.SplitAt(index, i + 1);
                    return If(i <= index,
                              Tuple(tup.Item1().AddFront(hd), tup.Item2()),
                              Tuple(tup.Item1(), tup.Item2().AddFront(hd)));
                });
        }

        /// <summary>
        /// Get the value of a list at an index.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> At<T>(this Zen<IList<T>> listExpr, Zen<ushort> index)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(index);

            return At(listExpr, index, 0);
        }

        /// <summary>
        /// Get the value of a list at an index.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="i">Current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<T>> At<T>(this Zen<IList<T>> listExpr, Zen<ushort> index, int i)
        {
            return listExpr.Case(
                empty: Null<T>(),
                cons: (hd, tl) => If(UShort((ushort)i) == index, Some(hd), tl.At(index, i + 1)));
        }

        /// <summary>
        /// Get the value of a list at an index.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<ushort>> IndexOf<T>(this Zen<IList<T>> listExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(value);

            return listExpr.IndexOf(value, 0);
        }

        /// <summary>
        /// Get the value of a list at an index.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <param name="i">Current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<ushort>> IndexOf<T>(this Zen<IList<T>> listExpr, Zen<T> value, int i)
        {
            return listExpr.Case(
                empty: Null<ushort>(),
                cons: (hd, tl) => If(value == hd, Some(UShort((ushort)i)), tl.IndexOf(value, i + 1)));
        }

        /// <summary>
        /// Check if a list is sorted.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsSorted<T>(this Zen<IList<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.Case(
                empty: True(),
                cons: (hd1, tl1) =>
                    tl1.Case(empty: True(),
                              cons: (hd2, tl2) => And(hd1 <= hd2, tl1.IsSorted())));
        }

        /// <summary>
        /// Sort a list.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Sort<T>(this Zen<IList<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.Case(empty: EmptyList<T>(), cons: (hd, tl) => Insert(hd, tl.Sort()));
        }

        /// <summary>
        /// Insert a value into a sorted list.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="expr">Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> Insert<T>(Zen<T> element, Zen<IList<T>> expr)
        {
            CommonUtilities.ValidateNotNull(element);
            CommonUtilities.ValidateNotNull(expr);

            return expr.Case(
                empty: Singleton(element),
                cons: (hd, tl) => If(element <= hd, expr.AddFront(element), Insert(element, tl).AddFront(hd)));
        }

        /// <summary>
        /// Add a value to a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IDictionary<TKey, TValue>> Add<TKey, TValue>(this Zen<IDictionary<TKey, TValue>> mapExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            var list = ZenAdapterExpr<IList<Tuple<TKey, TValue>>, IDictionary<TKey, TValue>>.Create(mapExpr, DictToList<TKey, TValue>);
            var result = list.AddFront(Tuple(keyExpr, valueExpr));
            return ZenAdapterExpr<IDictionary<TKey, TValue>, IList<Tuple<TKey, TValue>>>.Create(result, ListToDict<TKey, TValue>);
        }

        /// <summary>
        /// Get a value from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<TValue>> Get<TKey, TValue>(this Zen<IDictionary<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);

            var list = ZenAdapterExpr<IList<Tuple<TKey, TValue>>, IDictionary<TKey, TValue>>.Create(mapExpr, DictToList<TKey, TValue>);
            return list.ListGet(keyExpr);
        }

        /// <summary>
        /// Check if a Zen map contains a key.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> ContainsKey<TKey, TValue>(this Zen<IDictionary<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);

            return mapExpr.Get(keyExpr).HasValue();
        }

        /// <summary>
        /// Convert a Zen list to a map.
        /// </summary>
        /// <param name="listExpr">Zen list expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IDictionary<TKey, TValue>> ListToDictionary<TKey, TValue>(this Zen<IList<Tuple<TKey, TValue>>> listExpr)
        {
            CommonUtilities.ValidateNotNull(listExpr);

            return ZenAdapterExpr<IDictionary<TKey, TValue>, IList<Tuple<TKey, TValue>>>.Create(listExpr, ListToDict<TKey, TValue>);
        }

        /// <summary>
        /// Get the first binding in a list for a given key.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="key">The key.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<TValue>> ListGet<TKey, TValue>(this Zen<IList<Tuple<TKey, TValue>>> expr, Zen<TKey> key)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(key);

            return expr.Case(
                empty: Null<TValue>(),
                cons: (hd, tl) => If(hd.Item1() == key, Some(hd.Item2()), tl.ListGet(key)));
        }

        /// <summary>
        /// Get a field from a Zen value.
        /// </summary>
        /// <param name="expr">Zen object expression.</param>
        /// <param name="fieldName">Object field name.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> GetField<T1, T2>(this Zen<T1> expr, string fieldName)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(fieldName);

            return ZenGetFieldExpr<T1, T2>.Create(expr, fieldName);
        }

        /// <summary>
        /// Create a new Zen value a field set to a new value.
        /// </summary>
        /// <param name="expr">Zen object expression.</param>
        /// <param name="fieldName">Object field name.</param>
        /// <param name="fieldValue">New Zen field value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> WithField<T1, T2>(this Zen<T1> expr, string fieldName, Zen<T2> fieldValue)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(fieldName);
            CommonUtilities.ValidateNotNull(fieldValue);

            return ZenWithFieldExpr<T1, T2>.Create(expr, fieldName, fieldValue);
        }

        /// <summary>
        /// Create a new Zen object from a constructor.
        /// </summary>
        /// <param name="fields">The fields as Zen values.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Create<T>(params (string, object)[] fields)
        {
            return ZenCreateObjectExpr<T>.Create(fields);
        }

        /// <summary>
        /// Create a new Zen value for a tuple.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Tuple<T1, T2>> Tuple<T1, T2>(Zen<T1> expr1, Zen<T2> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var objectExpr = ZenCreateObjectExpr<CustomTuple<T1, T2>>.Create(("Item1", expr1), ("Item2", expr2));
            return ZenAdapterExpr<Tuple<T1, T2>, CustomTuple<T1, T2>>.Create(objectExpr, CustomTupleToTuple<T1, T2>);
        }

        /// <summary>
        /// Create a new Zen value for a tuple.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ValueTuple<T1, T2>> ValueTuple<T1, T2>(Zen<T1> expr1, Zen<T2> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var objectExpr = ZenCreateObjectExpr<CustomTuple<T1, T2>>.Create(("Item1", expr1), ("Item2", expr2));
            return ZenAdapterExpr<ValueTuple<T1, T2>, CustomTuple<T1, T2>>.Create(objectExpr, CustomTupleToValueTuple<T1, T2>);
        }

        private static Zen<T1> TupleItem1<T1, T2>(this Zen<Tuple<T1, T2>> expr)
        {
            return Item1(expr);
        }

        private static Zen<T2> TupleItem2<T1, T2>(this Zen<Tuple<T1, T2>> expr)
        {
            return Item2(expr);
        }

        private static Zen<T1> ValueTupleItem1<T1, T2>(this Zen<ValueTuple<T1, T2>> expr)
        {
            return Item1(expr);
        }

        private static Zen<T2> ValueTupleItem2<T1, T2>(this Zen<ValueTuple<T1, T2>> expr)
        {
            return Item2(expr);
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2>(this Zen<Tuple<T1, T2>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var tup = ZenAdapterExpr<CustomTuple<T1, T2>, Tuple<T1, T2>>.Create(expr, TupleToCustomTuple<T1, T2>);
            return tup.GetField<CustomTuple<T1, T2>, T1>("Item1");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2>(this Zen<Tuple<T1, T2>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var tup = ZenAdapterExpr<CustomTuple<T1, T2>, Tuple<T1, T2>>.Create(expr, TupleToCustomTuple<T1, T2>);
            return tup.GetField<CustomTuple<T1, T2>, T2>("Item2");
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2>(this Zen<ValueTuple<T1, T2>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var tup = ZenAdapterExpr<CustomTuple<T1, T2>, ValueTuple<T1, T2>>.Create(expr, ValueTupleToCustomTuple<T1, T2>);
            return tup.GetField<CustomTuple<T1, T2>, T1>("Item1");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2>(this Zen<ValueTuple<T1, T2>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var tup = ZenAdapterExpr<CustomTuple<T1, T2>, ValueTuple<T1, T2>>.Create(expr, ValueTupleToCustomTuple<T1, T2>);
            return tup.GetField<CustomTuple<T1, T2>, T2>("Item2");
        }

        /// <summary>
        /// Create a ZenFunction.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="function">The function.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T> Function<T>(Func<Zen<T>> function)
        {
            CommonUtilities.ValidateNotNull(function);

            return new ZenFunction<T>(function);
        }

        /// <summary>
        /// Create a ZenFunction.
        /// </summary>
        /// <typeparam name="T1">The first argument type.</typeparam>
        /// <typeparam name="T2">The return type.</typeparam>
        /// <param name="function">The function.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2> Function<T1, T2>(Func<Zen<T1>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(function);

            return new ZenFunction<T1, T2>(function);
        }

        /// <summary>
        /// Create a ZenFunction.
        /// </summary>
        /// <typeparam name="T1">The first argument type.</typeparam>
        /// <typeparam name="T2">The second argument type.</typeparam>
        /// <typeparam name="T3">The return type.</typeparam>
        /// <param name="function">The function.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2, T3> Function<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> function)
        {
            CommonUtilities.ValidateNotNull(function);

            return new ZenFunction<T1, T2, T3>(function);
        }

        /// <summary>
        /// Create a ZenFunction.
        /// </summary>
        /// <typeparam name="T1">The first argument type.</typeparam>
        /// <typeparam name="T2">The second argument type.</typeparam>
        /// <typeparam name="T3">The third argument type.</typeparam>
        /// <typeparam name="T4">The return type.</typeparam>
        /// <param name="function">The function.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2, T3, T4> Function<T1, T2, T3,  T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function)
        {
            CommonUtilities.ValidateNotNull(function);

            return new ZenFunction<T1, T2, T3, T4>(function);
        }

        /// <summary>
        /// Create a ZenFunction.
        /// </summary>
        /// <typeparam name="T1">The first argument type.</typeparam>
        /// <typeparam name="T2">The second argument type.</typeparam>
        /// <typeparam name="T3">The third argument type.</typeparam>
        /// <typeparam name="T4">The fourth argument type.</typeparam>
        /// <typeparam name="T5">The return type.</typeparam>
        /// <param name="function">The function.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2, T3, T4, T5> Function<T1, T2, T3, T4, T5>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function)
        {
            CommonUtilities.ValidateNotNull(function);

            return new ZenFunction<T1, T2, T3, T4, T5>(function);
        }

        private static object DictToList<T1, T2>(object expr)
        {
            var d = (IDictionary<T1, T2>)expr;
            var list = new List<Tuple<T1, T2>>();
            foreach (var kv in d)
            {
                list.Add(new Tuple<T1, T2>(kv.Key, kv.Value));
            }

            return list;
        }

        private static object ListToDict<T1, T2>(object list)
        {
            var l = (IList<Tuple<T1, T2>>)list;
            var dict = ImmutableDictionary<T1, T2>.Empty;
            foreach (var elt in l.Reverse())
            {
                dict = dict.SetItem(elt.Item1, elt.Item2);
            }

            return dict;
        }

        private static object OptionToCustomTuple<T>(object opt)
        {
            var x = (Option<T>)opt;
            return new CustomTuple<bool, T> { Item1 = x.HasValue, Item2 = x.Value };
        }

        private static object CustomTupleToOption<T>(object tuple)
        {
            var x = (CustomTuple<bool, T>)tuple;
            return new Option<T>(x.Item1, x.Item2);
        }

        private static object TupleToCustomTuple<T1, T2>(object tup)
        {
            var x = (Tuple<T1, T2>)tup;
            return new CustomTuple<T1, T2> { Item1 = x.Item1, Item2 = x.Item2 };
        }

        private static object CustomTupleToTuple<T1, T2>(object tuple)
        {
            var x = (CustomTuple<T1, T2>)tuple;
            return new Tuple<T1, T2>(x.Item1, x.Item2);
        }

        private static object ValueTupleToCustomTuple<T1, T2>(object tup)
        {
            var x = (ValueTuple<T1, T2>)tup;
            return new CustomTuple<T1, T2> { Item1 = x.Item1, Item2 = x.Item2 };
        }

        private static object CustomTupleToValueTuple<T1, T2>(object tuple)
        {
            var x = (CustomTuple<T1, T2>)tuple;
            return new ValueTuple<T1, T2>(x.Item1, x.Item2);
        }

        internal class CustomTuple<T1, T2>
        {
            public T1 Item1 { get; set; }

            public T2 Item2 { get; set; }

            [ExcludeFromCodeCoverage]
            public override string ToString()
            {
                return $"({this.Item1}, {this.Item2})";
            }
        }
    }
}
