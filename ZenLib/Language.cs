// <copyright file="ZenFunctions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.Generation;

    /// <summary>
    /// Collection of helper functions for building Zen programs.
    /// </summary>
    public static class Language
    {
        /// <summary>
        /// Get field method for reflection.
        /// </summary>
        private static MethodInfo getFieldMethod = typeof(Language).GetMethod("GetField");

        /// <summary>
        /// Equality method for reflection.
        /// </summary>
        private static MethodInfo eqMethod = typeof(Language).GetMethod("Eq");

        /// <summary>
        /// Boolean equality method for reflection.
        /// </summary>
        private static MethodInfo eqBoolMethod = typeof(Language).GetMethod("Eq").MakeGenericMethod(typeof(bool));

        /// <summary>
        /// List equality method for reflection.
        /// </summary>
        private static MethodInfo eqListsMethod = typeof(Language).GetMethod("EqLists", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Generates a new random value of a given type.
        /// </summary>
        /// <param name="magicConstants">Magic constants to use.</param>
        /// <param name="sizeBound">The bound on the size of objects.</param>
        /// <returns>A random value of a given type.</returns>
        public static T Generate<T>(Dictionary<Type, ISet<object>> magicConstants = null, int sizeBound = 20)
        {
            var generator = new RandomValueGenerator(magicConstants, sizeBound);
            return (T)ReflectionUtilities.ApplyTypeVisitor(generator, typeof(T));
        }

        /// <summary>
        /// Lift a C# value to a Zen value.
        /// </summary>
        /// <param name="x">The value.</param>
        public static Zen<T> Lift<T>(T x)
        {
            CommonUtilities.ValidateNotNull(x);

            if (typeof(T) == ReflectionUtilities.StringType)
            {
                CommonUtilities.ValidateStringLiteral((string)(object)x);
            }

            return (Zen<T>)ReflectionUtilities.CreateZenConstant(x);
        }

        /// <summary>
        /// The Zen value for false.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<bool> False()
        {
            return ZenConstantExpr<bool>.Create(false);
        }

        /// <summary>
        /// The Zen value for true.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<bool> True()
        {
            return ZenConstantExpr<bool>.Create(true);
        }

        /// <summary>
        /// The Zen value for a constant.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Constant<T>(T value)
        {
            return Lift(value);
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
        /// Create a list of arbitrary elements.
        /// </summary>
        /// <param name="count">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> ArbitraryList<T>(ushort count)
        {
            var elements = new Zen<T>[count];
            for (int i = 0; i < count; i++)
            {
                elements[i] = Arbitrary<T>();
            }

            return List(elements);
        }

        /// <summary>
        /// The Zen value for null.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Null<T>()
        {
            var v = (Zen<T>)ReflectionUtilities.ApplyTypeVisitor(new DefaultTypeGenerator(), typeof(T));
            return Create<Option<T>>(("HasValue", False()), ("Value", v));
        }

        /// <summary>
        /// The Zen expression for Some value.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Some<T>(Zen<T> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return Create<Option<T>>(("HasValue", True()), ("Value", expr));
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

            return If(expr.HasValue(), some(expr.Value()), none());
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> HasValue<T>(this Zen<Option<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Option<T>, bool>("HasValue");
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<IList<T>> ToList<T>(this Zen<Option<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            var l = EmptyList<T>();
            return If(expr.HasValue(), l.AddFront(expr.Value()), l);
        }

        /// <summary>
        /// The Zen expression for an option or a default if no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Value<T>(this Zen<Option<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Option<T>, T>("Value");
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

            return If(expr.HasValue(), expr.Value(), deflt);
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

            return If(expr.HasValue(), Some(function(expr.Value())), Null<T2>());
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

            return If(And(expr.HasValue(), function(expr.Value())), expr, Null<T>());
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
        public static Zen<Dict<TKey, TValue>> EmptyDict<TKey, TValue>()
        {
            return Create<Dict<TKey, TValue>>(("Values", EmptyList<Pair<TKey, TValue>>()));
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

            if (exprs.Length == 0)
            {
                return True();
            }

            var result = True();
            for (int i = exprs.Length - 1; i >= 0; i--)
            {
                result = And(exprs[i], result);
            }

            return result;
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(params Zen<bool>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            if (exprs.Length == 0)
            {
                return False();
            }

            var result = False();
            for (int i = exprs.Length - 1; i >= 0; i--)
            {
                result = Or(exprs[i], result);
            }

            return result;
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

            return Or(Not(guardExpr), thenExpr);
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
        /// Compute a switch statement.
        /// </summary>
        /// <param name="deflt">The default value.</param>
        /// <param name="cases">A collection of ordered cases.</param>
        /// <returns>The resulting Zen value.</returns>
        public static Zen<T> Cases<T>(Zen<T> deflt, params (Zen<bool>, Zen<T>)[] cases)
        {
            CommonUtilities.ValidateNotNull(deflt);
            CommonUtilities.ValidateNotNull(cases);

            for (int i = cases.Length - 1; i >= 0; i--)
            {
                var (guard, value) = cases[i];
                deflt = If(guard, value, deflt);
            }

            return deflt;
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

            if (ReflectionUtilities.IsIListType(type))
            {
                var innerType = type.GetGenericArgumentsCached()[0];
                var method = eqListsMethod.MakeGenericMethod(innerType);
                return (Zen<bool>)method.Invoke(null, new object[] { expr1, expr2 });
            }

            // some class or struct
            var acc = True();
            foreach (var field in ReflectionUtilities.GetAllFields(type))
            {
                var method = getFieldMethod.MakeGenericMethod(typeof(T), field.FieldType);
                object field1 = method.Invoke(null, new object[] { expr1, field.Name });
                object field2 = method.Invoke(null, new object[] { expr2, field.Name });
                var emethod = eqMethod.MakeGenericMethod(field.FieldType);
                acc = And(acc, (Zen<bool>)emethod.Invoke(null, new object[] { field1, field2 }));
            }

            foreach (var property in ReflectionUtilities.GetAllProperties(type))
            {
                var method = getFieldMethod.MakeGenericMethod(typeof(T), property.PropertyType);
                object prop1 = method.Invoke(null, new object[] { expr1, property.Name });
                object prop2 = method.Invoke(null, new object[] { expr2, property.Name });
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
        public static Zen<string> Substring(this Zen<string> str, Zen<BigInteger> offset, Zen<BigInteger> length)
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
        public static Zen<string> At(this Zen<string> str, Zen<BigInteger> index)
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
        public static Zen<BigInteger> Length(this Zen<string> str)
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
        public static Zen<BigInteger> IndexOf(this Zen<string> str, Zen<string> sub, Zen<BigInteger> offset)
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
        public static Zen<BigInteger> IndexOf(this Zen<string> str, Zen<string> sub)
        {
            return IndexOf(str, sub, new BigInteger(0));
        }

        /// <summary>
        /// Creates an FiniteString.
        /// </summary>
        /// <returns>The finite string.</returns>
        public static Zen<FiniteString> FiniteString(Zen<IList<ushort>> values)
        {
            return Create<FiniteString>(("Characters", values));
        }

        /// <summary>
        /// Creates a constant string value.
        /// </summary>
        /// <returns>The string value.</returns>
        public static Zen<FiniteString> FiniteString(string s)
        {
            var l = EmptyList<ushort>();
            foreach (var c in s.Reverse())
            {
                l = l.AddFront(c);
            }

            return FiniteString(l);
        }

        /// <summary>
        /// Creates a finite string from a character.
        /// </summary>
        /// <returns>The finite string.</returns>
        public static Zen<FiniteString> Char(Zen<ushort> b)
        {
            return FiniteString(Singleton(b));
        }

        /// <summary>
        /// Get whether a string is empty.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> IsEmpty(this Zen<FiniteString> s)
        {
            return s.GetCharacters().IsEmpty();
        }

        /// <summary>
        /// Gets the list of bytes for a finite string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The bytes.</returns>
        public static Zen<IList<ushort>> GetCharacters(this Zen<FiniteString> s)
        {
            return s.GetField<FiniteString, IList<ushort>>("Characters");
        }

        /// <summary>
        /// Concatenation of two strings.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns>The concatenated string.</returns>
        public static Zen<FiniteString> Concat(this Zen<FiniteString> s1, Zen<FiniteString> s2)
        {
            return FiniteString(s1.GetCharacters().Append(s2.GetCharacters()));
        }

        /// <summary>
        /// Gets the length of the string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The length.</returns>
        public static Zen<ushort> Length(this Zen<FiniteString> s)
        {
            return s.GetCharacters().Length();
        }

        /// <summary>
        /// Whether a string is a prefix of another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="pre">The prefix.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> StartsWith(this Zen<FiniteString> s, Zen<FiniteString> pre)
        {
            return StartsWith(s.GetCharacters(), pre.GetCharacters());
        }

        private static Zen<bool> StartsWith(Zen<IList<ushort>> s, Zen<IList<ushort>> pre)
        {
            return pre.Case(
                empty: true,
                cons: (hd1, tl1) => s.Case(
                    empty: false,
                    cons: (hd2, tl2) => AndIf(hd1 == hd2, StartsWith(tl2, tl1))));
        }

        /// <summary>
        /// Whether a string is a suffix of another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="suf">The suffix.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> EndsWith(this Zen<FiniteString> s, Zen<FiniteString> suf)
        {
            return StartsWith(s.GetCharacters().Reverse(), suf.GetCharacters().Reverse());
        }

        /// <summary>
        /// Substring of length 1 at the offset.
        /// Empty if the offset is invalid.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="i">The index.</param>
        /// <returns>The substring.</returns>
        public static Zen<FiniteString> At(this Zen<FiniteString> s, Zen<ushort> i)
        {
            return At(s.GetCharacters(), i, 0);
        }

        private static Zen<FiniteString> At(Zen<IList<ushort>> s, Zen<ushort> i, int current)
        {
            return s.Case(
                empty: FiniteString(""),
                cons: (hd, tl) =>
                    If(i == (ushort)current, Char(hd), At(tl, i, current + 1)));
        }

        /// <summary>
        /// Whether a string contains a substring.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> Contains(this Zen<FiniteString> s, Zen<FiniteString> sub)
        {
            return Contains(s.GetCharacters(), sub.GetCharacters());
        }

        private static Zen<bool> Contains(Zen<IList<ushort>> s, Zen<IList<ushort>> sub)
        {
            return s.Case(
                empty: sub.IsEmpty(),
                cons: (hd, tl) => OrIf(StartsWith(s, sub), Contains(tl, sub)));
        }

        /// <summary>
        /// Gets the first index of a substring in a string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <returns>An index.</returns>
        public static Zen<Option<ushort>> IndexOf(this Zen<FiniteString> s, Zen<FiniteString> sub)
        {
            return IndexOf(s.GetCharacters(), sub.GetCharacters(), 0);
        }

        private static Zen<Option<ushort>> IndexOf(Zen<IList<ushort>> s, Zen<IList<ushort>> sub, int current)
        {
            return s.Case(
                empty: If(sub.IsEmpty(), Some<ushort>((ushort)current), Null<ushort>()),
                cons: (hd, tl) => If(StartsWith(s, sub), Some<ushort>((ushort)current), IndexOf(tl, sub, current + 1)));
        }

        /// <summary>
        /// Gets the first index of a substring in a
        /// string starting at an offset.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>An index.</returns>
        public static Zen<Option<ushort>> IndexOf(this Zen<FiniteString> s, Zen<FiniteString> sub, Zen<ushort> offset)
        {
            var trimmed = s.GetCharacters().Drop(offset);
            var idx = IndexOf(trimmed, sub.GetCharacters(), 0);
            return If(idx.HasValue(), Some(idx.Value() + offset), idx);
        }

        /// <summary>
        /// Gets the substring at an offset and for a given length..
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="len">The length.</param>
        /// <returns>An index.</returns>
        public static Zen<FiniteString> SubString(this Zen<FiniteString> s, Zen<ushort> offset, Zen<ushort> len)
        {
            return FiniteString(s.GetCharacters().Drop(offset).Take(len));
        }

        /// <summary>
        /// Replaces all occurrences of a given character with another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="src">The source value.</param>
        /// <param name="dst">The destination value.</param>
        /// <returns>A new string.</returns>
        public static Zen<FiniteString> ReplaceAll(this Zen<FiniteString> s, Zen<ushort> src, Zen<ushort> dst)
        {
            return FiniteString(s.GetCharacters().Select(c => If(c == src, dst, c)));
        }

        /// <summary>
        /// Removes all occurrences of a given character.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns>A new string.</returns>
        public static Zen<FiniteString> RemoveAll(this Zen<FiniteString> s, Zen<ushort> value)
        {
            return Transform(s, l => l.Where(c => c != value));
        }

        /// <summary>
        /// Transform a string by modifying its characters.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="f">The transformation function.</param>
        /// <returns>A new string.</returns>
        public static Zen<FiniteString> Transform(this Zen<FiniteString> s, Func<Zen<IList<ushort>>, Zen<IList<ushort>>> f)
        {
            return FiniteString(f(s.GetCharacters()));
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
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Max(Zen<BigInteger> expr1, Zen<BigInteger> expr2)
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
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value..</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Min(Zen<BigInteger> expr1, Zen<BigInteger> expr2)
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
                empty: Constant<ushort>(0),
                cons: (hd, tl) =>
                    If(hd == valueExpr, tl.Duplicates(valueExpr), tl.Duplicates(valueExpr) + Constant<ushort>(1)));
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
                empty: Constant<ushort>(0),
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
        /// <param name="acc">The initial value.</param>
        /// <param name="function">The fold function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Fold<T1, T2>(this Zen<IList<T1>> expr, Zen<T2> acc, Func<Zen<T1>, Zen<T2>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(acc);
            CommonUtilities.ValidateNotNull(function);

            return expr.Case(
                empty: acc,
                cons: (hd, tl) => tl.Fold(function(hd, acc), function));
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
                cons: (hd, tl) => If(Constant<ushort>((ushort)i) == numElements, EmptyList<T>(), tl.Take(numElements, i + 1).AddFront(hd)));
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
                cons: (hd, tl) => If(Constant<ushort>((ushort)i) == numElements, expr, tl.Drop(numElements, i + 1)));
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
        public static Zen<Pair<IList<T>, IList<T>>> SplitAt<T>(this Zen<IList<T>> expr, Zen<ushort> index)
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
        private static Zen<Pair<IList<T>, IList<T>>> SplitAt<T>(this Zen<IList<T>> expr, Zen<ushort> index, int i)
        {
            return expr.Case(
                empty: Pair(EmptyList<T>(), EmptyList<T>()),
                cons: (hd, tl) =>
                {
                    var tup = tl.SplitAt(index, i + 1);
                    return If((ushort)i <= index,
                              Pair(tup.Item1().AddFront(hd), tup.Item2()),
                              Pair(tup.Item1(), tup.Item2().AddFront(hd)));
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
                cons: (hd, tl) => If(Constant<ushort>((ushort)i) == index, Some(hd), tl.At(index, i + 1)));
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
                cons: (hd, tl) => If(value == hd, Some(Constant<ushort>((ushort)i)), tl.IndexOf(value, i + 1)));
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
        public static Zen<Dict<TKey, TValue>> Add<TKey, TValue>(this Zen<Dict<TKey, TValue>> mapExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            var l = mapExpr.GetField<Dict<TKey, TValue>, IList<Pair<TKey, TValue>>>("Values");
            return Create<Dict<TKey, TValue>>(("Values", l.AddFront(Pair(keyExpr, valueExpr))));
        }

        /// <summary>
        /// Get a value from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<TValue>> Get<TKey, TValue>(this Zen<Dict<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);

            var l = mapExpr.GetField<Dict<TKey, TValue>, IList<Pair<TKey, TValue>>>("Values");
            return l.ListGet(keyExpr);
        }

        /// <summary>
        /// Check if a Zen map contains a key.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> ContainsKey<TKey, TValue>(this Zen<Dict<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);

            return mapExpr.Get(keyExpr).HasValue();
        }

        /// <summary>
        /// Get the first binding in a list for a given key.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="key">The key.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<TValue>> ListGet<TKey, TValue>(this Zen<IList<Pair<TKey, TValue>>> expr, Zen<TKey> key)
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
        public static Zen<Pair<T1, T2>> Pair<T1, T2>(Zen<T1> expr1, Zen<T2> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenCreateObjectExpr<Pair<T1, T2>>.Create(
                ("Item1", expr1),
                ("Item2", expr2));
        }

        /// <summary>
        /// Create a new Zen value for a tuple.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <param name="expr3">Third Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Pair<T1, T2, T3>> Pair<T1, T2, T3>(Zen<T1> expr1, Zen<T2> expr2, Zen<T3> expr3)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            return ZenCreateObjectExpr<Pair<T1, T2, T3>>.Create(
                ("Item1", expr1),
                ("Item2", expr2),
                ("Item3", expr3));
        }

        /// <summary>
        /// Create a new Zen value for a tuple.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <param name="expr3">Third Zen expression.</param>
        /// <param name="expr4">Fourth Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Pair<T1, T2, T3, T4>> Pair<T1, T2, T3, T4>(Zen<T1> expr1, Zen<T2> expr2, Zen<T3> expr3, Zen<T4> expr4)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            return ZenCreateObjectExpr<Pair<T1, T2, T3, T4>>.Create(
                ("Item1", expr1),
                ("Item2", expr2),
                ("Item3", expr3),
                ("Item4", expr4));
        }

        /// <summary>
        /// Create a new Zen value for a tuple.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <param name="expr3">Third Zen expression.</param>
        /// <param name="expr4">Fourth Zen expression.</param>
        /// <param name="expr5">Fifth Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Pair<T1, T2, T3, T4, T5>> Pair<T1, T2, T3, T4, T5>(Zen<T1> expr1, Zen<T2> expr2, Zen<T3> expr3, Zen<T4> expr4, Zen<T5> expr5)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            return ZenCreateObjectExpr<Pair<T1, T2, T3, T4, T5>>.Create(
                ("Item1", expr1),
                ("Item2", expr2),
                ("Item3", expr3),
                ("Item4", expr4),
                ("Item5", expr5));
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2>(this Zen<Pair<T1, T2>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2>, T1>("Item1");
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2, T3>(this Zen<Pair<T1, T2, T3>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3>, T1>("Item1");
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T1>("Item1");
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T1>("Item1");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2>(this Zen<Pair<T1, T2>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2>, T2>("Item2");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2, T3>(this Zen<Pair<T1, T2, T3>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3>, T2>("Item2");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T2>("Item2");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T2>("Item2");
        }

        /// <summary>
        /// Get the third element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T3> Item3<T1, T2, T3>(this Zen<Pair<T1, T2, T3>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3>, T3>("Item3");
        }

        /// <summary>
        /// Get the third element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T3> Item3<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T3>("Item3");
        }

        /// <summary>
        /// Get the third element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T3> Item3<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T3>("Item3");
        }

        /// <summary>
        /// Get the fourth element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T4> Item4<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T4>("Item4");
        }

        /// <summary>
        /// Get the fourth element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T4> Item4<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T4>("Item4");
        }

        /// <summary>
        /// Get the fifth element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T5> Item5<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T5>("Item5");
        }

        /// <summary>
        /// Logical or, implemented as an if to allow better test generation.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <returns>The logical or of the two expressions.</returns>
        public static Zen<bool> OrIf(Zen<bool> expr1, Zen<bool> expr2)
        {
            return If(expr1, True(), expr2);
        }

        /// <summary>
        /// Logical and, implemented as an if to allow better test generation.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <returns>The logical and of the two expressions.</returns>
        public static Zen<bool> AndIf(Zen<bool> expr1, Zen<bool> expr2)
        {
            return If(Not(expr1), False(), expr2);
        }
    }
}
