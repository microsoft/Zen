// <copyright file="CommonUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// A collection of common utility functions.
    /// </summary>
    public static class CommonUtilities
    {
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
        public static (T, IList<T>) SplitHead<T>(ImmutableList<T> list)
        {
            var hd = list[0];
            var tl = list.Count == 1 ? ImmutableList<T>.Empty : list.GetRange(1, list.Count - 1);
            return (hd, tl);
        }

        /// <summary>
        /// Convert to an immutable list if necessary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ImmutableList<T> ToImmutableList<T>(object obj)
        {
            if (obj is ImmutableList<T>)
            {
                return (ImmutableList<T>)obj;
            }

            return ImmutableList.CreateRange((IList<T>)obj);
        }

        /// <summary>
        /// Validate that an argument is not null.
        /// </summary>
        /// <param name="obj">The argument.</param>
        public static void Validate(object obj)
        {
            if (obj is null)
            {
                throw new ArgumentException($"Invalid null argument");
            }
        }

        /// <summary>
        /// Validate that a type is an integer type.
        /// </summary>
        /// <param name="type"></param>
        public static void ValidateIsIntegerType(Type type)
        {
            if (!ReflectionUtilities.IsIntegerType(type))
            {
                throw new ArgumentException($"Invalid non-integer type {type} used as integer.");
            }
        }
    }
}
