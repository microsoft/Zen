﻿// <copyright file="CommonUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using ZenLib;

    /// <summary>
    /// A collection of common utility functions.
    /// </summary>
    internal static class CommonUtilities
    {
        /// <summary>
        /// An empty array.
        /// </summary>
        public static object[] EmptyArray = new object[] { };

        /// <summary>
        /// constructor arguments for creating fixed integers.
        /// </summary>
        private static object[] constructorArgs = new object[] { 0L };

        /// <summary>
        /// constructor types for creating fixed integers.
        /// </summary>
        private static Type[] constructorTypes = new Type[] { typeof(long) };

        /// <summary>
        /// Merge two immutable dictionaries together by key.
        /// </summary>
        /// <typeparam name="T1">Type of the key.</typeparam>
        /// <typeparam name="T2">Type of the value.</typeparam>
        /// <typeparam name="T3">Type of the new value.</typeparam>
        /// <param name="dict1">The first dictionary.</param>
        /// <param name="dict2">The second dictionary.</param>
        /// <param name="merger">The merging function.</param>
        /// <returns></returns>
        public static ImmutableDictionary<T1, T3> Merge<T1, T2, T3>(
            ImmutableDictionary<T1, T2> dict1,
            ImmutableDictionary<T1, T2> dict2,
            Func<T1, Option<T2>, Option<T2>, Option<T3>> merger)
        {
            var result = ImmutableDictionary<T1, T3>.Empty;

            var keys = new HashSet<T1>(dict1.Keys);
            keys.UnionWith(dict2.Keys);

            foreach (var key in keys)
            {
                var b1 = dict1.TryGetValue(key, out var value1);
                var b2 = dict2.TryGetValue(key, out var value2);
                var o1 = b1 ? Option.Some(value1) : Option.None<T2>();
                var o2 = b2 ? Option.Some(value2) : Option.None<T2>();
                if (b1 || b2)
                {
                    var mergedValue = merger(key, o1, o2);
                    if (mergedValue.HasValue)
                    {
                        result = result.Add(key, mergedValue.Value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Map a function over a dictionary.
        /// </summary>
        /// <typeparam name="T1">The key type.</typeparam>
        /// <typeparam name="T2">The value type.</typeparam>
        /// <typeparam name="T3">The new value type.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="mapper">The map function.</param>
        /// <returns>The resulting dictionary.</returns>
        public static ImmutableDictionary<T1, T3> Map<T1, T2, T3>(ImmutableDictionary<T1, T2> dict, Func<T2, T3> mapper)
        {
            var result = ImmutableDictionary<T1, T3>.Empty;
            foreach (var kv in dict)
            {
                result = result.Add(kv.Key, mapper(kv.Value));
            }

            return result;
        }

        /// <summary>
        /// Split a list to peel off the first element.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The head and the rest of the list.</returns>
        public static (Option<T>, FSeq<T>) SplitHead<T>(FSeq<T> list)
        {
            var (hd, tl) = SplitHeadHelper(list.Values);
            return (hd, new FSeq<T>(tl));
        }

        /// <summary>
        /// Split a list to peel off the first element.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The head and the rest of the list.</returns>
        public static (T, ImmutableList<T>) SplitHeadHelper<T>(ImmutableList<T> list)
        {
            var hd = list[0];
            var tl = list.Count == 1 ? ImmutableList<T>.Empty : list.GetRange(1, list.Count - 1);
            return (hd, tl);
        }

        /// <summary>
        /// Unions two maps.
        /// </summary>
        /// <param name="map1">A map.</param>
        /// <param name="map2">A map.</param>
        /// <returns>The union of the two maps.</returns>
        public static Map<T, SetUnit> MapUnion<T>(Map<T, SetUnit> map1, Map<T, SetUnit> map2)
        {
            Contract.Assert(!map1.Negated);
            Contract.Assert(!map2.Negated);
            return new Map<T, SetUnit>(ImmutableDictionary<T, SetUnit>.Empty.AddRange(map1.Values.Union(map2.Values)));
        }

        /// <summary>
        /// Intersects two maps.
        /// </summary>
        /// <param name="map1">A map.</param>
        /// <param name="map2">A map.</param>
        /// <returns>The intersection of the two maps.</returns>
        public static Map<T, SetUnit> MapIntersect<T>(Map<T, SetUnit> map1, Map<T, SetUnit> map2)
        {
            Contract.Assert(!map1.Negated);

            if (map2.Negated)
            {
                return new Map<T, SetUnit>(ImmutableDictionary<T, SetUnit>.Empty.AddRange(map1.Values.Except(map2.Values)));
            }

            return new Map<T, SetUnit>(ImmutableDictionary<T, SetUnit>.Empty.AddRange(map1.Values.Intersect(map2.Values)));
        }

        /// <summary>
        /// Differences two maps.
        /// </summary>
        /// <param name="map1">A map.</param>
        /// <param name="map2">A map.</param>
        /// <returns>The difference of the two maps.</returns>
        public static Map<T, SetUnit> MapDifference<T>(Map<T, SetUnit> map1, Map<T, SetUnit> map2)
        {
            Contract.Assert(!map1.Negated);
            Contract.Assert(!map2.Negated);
            return new Map<T, SetUnit>(ImmutableDictionary<T, SetUnit>.Empty.AddRange(map1.Values.Except(map2.Values)));
        }

        /// <summary>
        /// Unions two cmaps.
        /// </summary>
        /// <param name="cmap1">A cmap.</param>
        /// <param name="cmap2">A cmap.</param>
        /// <returns>The union of the two cmaps.</returns>
        public static CMap<T, bool> CMapUnion<T>(CMap<T, bool> cmap1, CMap<T, bool> cmap2)
        {
            return new CMap<T, bool>(ImmutableDictionary<T, bool>.Empty.AddRange(cmap1.Values.Union(cmap2.Values)));
        }

        /// <summary>
        /// Intersects two cmaps.
        /// </summary>
        /// <param name="cmap1">A cmap.</param>
        /// <param name="cmap2">A cmap.</param>
        /// <returns>The intersection of the two cmaps.</returns>
        public static CMap<T, bool> CMapIntersect<T>(CMap<T, bool> cmap1, CMap<T, bool> cmap2)
        {
            return new CMap<T, bool>(ImmutableDictionary<T, bool>.Empty.AddRange(cmap1.Values.Intersect(cmap2.Values)));
        }

        /// <summary>
        /// Difference two cmaps.
        /// </summary>
        /// <param name="cmap1">A cmap.</param>
        /// <param name="cmap2">A cmap.</param>
        /// <returns>The difference of the two cmaps.</returns>
        public static CMap<T, bool> CMapDifference<T>(CMap<T, bool> cmap1, CMap<T, bool> cmap2)
        {
            return new CMap<T, bool>(ImmutableDictionary<T, bool>.Empty.AddRange(cmap1.Values.Except(cmap2.Values)));
        }

        /// <summary>
        /// Check if a cast is valid.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        public static bool IsSafeCast(Type sourceType, Type targetType)
        {
            return (sourceType == ReflectionUtilities.StringType && targetType == ReflectionUtilities.UnicodeSequenceType) ||
                   (targetType == ReflectionUtilities.StringType && sourceType == ReflectionUtilities.UnicodeSequenceType) ||
                   (ReflectionUtilities.IsFiniteIntegerType(sourceType) && ReflectionUtilities.IsFiniteIntegerType(targetType));
        }

        /// <summary>
        /// Escape a Z3 string.
        /// </summary>
        /// <returns>The escaped Z3 string.</returns>
        public static string ConvertCShaprStringToZ3(Seq<Char> s)
        {
            return string.Join(string.Empty, s.Values.Select(c => EscapeChar(c)));
        }

        /// <summary>
        /// Unescape a Z3 string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>An unescaped string.</returns>
        public static string ConvertZ3StringToCSharp(string s)
        {
            // strip outer quotation marks
            s = s.Substring(1, s.Length - 2);

            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                var isLast = i + 1 == s.Length;

                // strip double quotation marks
                if (!isLast && s[i] == '\"' && s[i + 1] == '\"')
                {
                    sb.Append('\"');
                    i++;
                    continue;
                }

                // unescape characters if needed
                if (!isLast && s[i] == '\\' && s[i + 1] == 'u')
                {
                    i += 2;
                    Contract.Assert(s[i++] == '{');
                    var start = i;
                    while (s[i] != '}')
                        i++;
                    Contract.Assert(i - start <= 5);
                    var hex = new char[5];
                    for (int j = 0; j < 5; j++)
                        hex[j] = '0';
                    for (int j = start; j < i; j++)
                        hex[5 - (i - j)] = s[j];
                    var str = new string(hex);
                    var intVal = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                    var c = (char)intVal;
                    sb.Append(c.ToString());
                    continue;
                }

                // otherwise copy the character literal
                sb.Append(s[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Run a function with a large stack in another thread.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="f">The function to run.</param>
        /// <returns>The result of the function.</returns>
        internal static T RunWithLargeStack<T>(Func<T> f)
        {
            // don't spawn a new thread if we are in a thread with a larger stack.
            if (!ZenSettings.UseLargeStack || ZenSettings.InSeparateThread)
            {
                return f();
            }

            T result = default;
            Exception exn = null;

            // start a new thread to fun the function.
            Thread t = new Thread(() =>
            {
                try
                {
                    ZenSettings.InSeparateThread = true;
                    result = f();
                }
                catch (Exception e)
                {
                    exn = e;
                }
            }, ZenSettings.LargeStackSize);

            t.Start();
            t.Join();

            // propagate an internal exception
            if (exn != null)
            {
                if (exn is ZenException || exn is ZenSolverTimeoutException)
                {
                    throw exn;
                }

                throw new ZenException("Execption thrown in RunWithLargeStack", exn);
            }

            return result;
        }

        /// <summary>
        /// Creates an arbitrary value if the input is null.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="depth">The input depth.</param>
        /// <returns>An arbitrary Zen value.</returns>
        public static Zen<T> GetArbitraryIfNull<T>(Zen<T> input, int depth)
        {
            return (input is null) ? Zen.Arbitrary<T>(depth: depth) : input;
        }

        /// <summary>
        /// Copies the elements of the values stored right to left to a new array
        /// of the target size. If the target size is larger than the current array,
        /// it fills the left-most values with the provided default.
        /// </summary>
        /// <typeparam name="T">The element types.</typeparam>
        /// <param name="values">The array of values.</param>
        /// <param name="defaultValue">The default value to fill.</param>
        /// <param name="targetSize">The target size of the new array.</param>
        /// <returns></returns>
        public static T[] CopyBigEndian<T>(T[] values, T defaultValue, int targetSize)
        {
            var newValues = new T[targetSize];

            for (var i = 1; i <= targetSize; i++)
            {
                if (i <= values.Length)
                {
                    newValues[targetSize - i] = values[values.Length - i];
                }
                else
                {
                    newValues[targetSize - i] = defaultValue;
                }
            }

            return newValues;
        }

        /// <summary>
        /// Escape this character using the \uXXXX notation.
        /// </summary>
        /// <returns>The escaped character.</returns>
        public static string EscapeChar(char c)
        {
            return @"\u{" + ((long)c).ToString("X4") + "}";
        }
    }
}
