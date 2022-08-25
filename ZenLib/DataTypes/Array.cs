// <copyright file="Array.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a simple finite set.
    /// </summary>
    public class Array<T, TSize> : IEquatable<Array<T, TSize>>
    {
        /// <summary>
        /// The size of the array.
        /// </summary>
        internal static int Size;

        /// <summary>
        /// Gets the constant map that backs this array.
        /// </summary>
        public CMap<int, T> Values { get; set; }

        /// <summary>
        /// Computes the size from the type.
        /// </summary>
        static Array()
        {
            var sizeType = typeof(TSize).ToString();
            try
            {
                Size = int.Parse(sizeType.Split('_')[1]);
            }
            catch
            {
                throw new ZenException($"Invalid array size type: {sizeType}");
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Array{T, TSize}"/> class.
        /// </summary>
        public Array()
        {
            this.Values = new CMap<int, T>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Array{T, TSize}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public Array(params T[] values)
        {
            if (values.Length != Size)
            {
                throw new ZenException($"Array size mismatch, expected array of length: {Size}");
            }

            this.Values = new CMap<int, T>();
            for (int i = 0; i < Size; i++)
            {
                this.Values = this.Values.Set(i, values[i]);
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Array{T, TSize}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public Array(IEnumerable<T> values) : this(values.ToArray())
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Array{T, TSize}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        private Array(CMap<int, T> values)
        {
            this.Values = values;
        }

        /// <summary>
        /// Get the elements of the array.
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            var result = new T[Size];
            for (int i = 0; i < Size; i++)
            {
                result[i] = this.Values.Get(i);
            }

            return result;
        }

        /// <summary>
        /// Get the element at the index.
        /// </summary>
        /// <param name="index">The index to get.</param>
        /// <returns>The element at the index.</returns>
        public T Get(int index)
        {
            Contract.Assert(IsValidIndex(index));
            return this.Values.Get(index);
        }

        /// <summary>
        /// Set the element at a given index.
        /// </summary>
        /// <param name="index">The index to set.</param>
        /// <param name="element">The element to set the array index to.</param>
        /// <returns>A new array with the index updated.</returns>
        public Array<T, TSize> Set(int index, T element)
        {
            Contract.Assert(IsValidIndex(index));
            return new Array<T, TSize>(this.Values.Set(index, element));
        }

        /// <summary>
        /// Get the length of the array.
        /// </summary>
        /// <returns>The array length.</returns>
        public int Length()
        {
            return Size;
        }

        /// <summary>
        /// Validate that an index is in range.
        /// </summary>
        /// <param name="index">The index.</param>
        internal static bool IsValidIndex(int index)
        {
            return index >= 0 && index < Size;
        }

        /// <summary>
        /// Convert the array to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "[" + string.Join(",", this.ToArray()) + "]";
        }

        /// <summary>
        /// Equality for arrays.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Array<T, TSize> o && Equals(o);
        }

        /// <summary>
        /// Equality for arrays.
        /// </summary>
        /// <param name="other">The other array.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Array<T, TSize> other)
        {
            for (int i = 0; i < Size; i++)
            {
                if (!this.Get(i).Equals(other.Get(i)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Hashcode for arrays.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            return this.Values.GetHashCode();
        }

        /// <summary>
        /// Equality for arrays.
        /// </summary>
        /// <param name="left">The left array.</param>
        /// <param name="right">The right array.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(Array<T, TSize> left, Array<T, TSize> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Equality for arrays.
        /// </summary>
        /// <param name="left">The left array.</param>
        /// <param name="right">The right array.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(Array<T, TSize> left, Array<T, TSize> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for Array Zen objects.
    /// </summary>
    public static class Array
    {
        /// <summary>
        /// The Zen value for an array from a cmap.
        /// </summary>
        /// <param name="mapExpr">The cmap expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Array<T, TSize>> Create<T, TSize>(Zen<CMap<int, T>> mapExpr)
        {
            return Zen.Create<Array<T, TSize>>(("Values", mapExpr));
        }

        /// <summary>
        /// Create an array from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Array<T, TSize>> Create<T, TSize>(params Zen<T>[] elements)
        {
            Contract.AssertNotNull(elements);
            Contract.Assert(Array<T, TSize>.Size == elements.Length, "Invalid array length");

            var cmap = Constant(new CMap<int, T>());
            for (int i = 0; i < elements.Length; i++)
            {
                cmap = cmap.Set(i, elements[i]);
            }

            return Array.Create<T, TSize>(cmap);
        }

        /// <summary>
        /// Gets an array of Zen elements from a Zen array.
        /// </summary>
        /// <param name="arrayExpr">The Zen array..</param>
        /// <returns>Zen value.</returns>
        public static Zen<T>[] ToArray<T, TSize>(this Zen<Array<T, TSize>> arrayExpr)
        {
            Contract.AssertNotNull(arrayExpr);

            var result = new Zen<T>[Array<T, TSize>.Size];
            for (int i = 0; i < Array<T, TSize>.Size; i++)
            {
                result[i] = arrayExpr.Get(i);
            }

            return result;
        }

        /// <summary>
        /// Gets the underlying cmap for an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<int, T>> Values<T, TSize>(this Zen<Array<T, TSize>> arrayExpr)
        {
            Contract.AssertNotNull(arrayExpr);

            return arrayExpr.GetField<Array<T, TSize>, CMap<int, T>>("Values");
        }

        /// <summary>
        /// Get an array value at an index.
        /// </summary>
        /// <param name="arrayExpr">Zen array expression.</param>
        /// <param name="index">The index.</param>
        /// <returns>The array element.</returns>
        public static Zen<T> Get<T, TSize>(this Zen<Array<T, TSize>> arrayExpr, int index)
        {
            Contract.AssertNotNull(arrayExpr);
            Contract.Assert(Array<T, TSize>.IsValidIndex(index));

            return arrayExpr.Values().Get(index);
        }

        /// <summary>
        /// Set a value at an index for an array.
        /// </summary>
        /// <param name="arrayExpr">Zen array expression.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value to set at the index.</param>
        /// <returns>The new array after setting the value.</returns>
        public static Zen<Array<T, TSize>> Set<T, TSize>(this Zen<Array<T, TSize>> arrayExpr, int index, Zen<T> value)
        {
            Contract.AssertNotNull(arrayExpr);
            Contract.AssertNotNull(value);
            Contract.Assert(Array<T, TSize>.IsValidIndex(index));

            return Array.Create<T, TSize>(arrayExpr.Values().Set(index, value));
        }

        /// <summary>
        /// Map a function over an array to produce a new array.
        /// </summary>
        /// <param name="arrayExpr">Zen array expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>The new array from mapping over all the elements.</returns>
        public static Zen<Array<TResult, TSize>> Select<T, TResult, TSize>(this Zen<Array<T, TSize>> arrayExpr, Func<Zen<T>, Zen<TResult>> function)
        {
            Contract.AssertNotNull(arrayExpr);
            Contract.AssertNotNull(function);

            return Array.Create<TResult, TSize>(arrayExpr.ToArray().Select(function).ToArray());
        }

        /// <summary>
        /// Determines if any element of the array satisfies some predicate.
        /// </summary>
        /// <param name="arrayExpr">Zen array expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if an element satisfies the predicate.</returns>
        public static Zen<bool> Any<T, TSize>(this Zen<Array<T, TSize>> arrayExpr, Func<Zen<T>, Zen<bool>> function)
        {
            Contract.AssertNotNull(arrayExpr);
            Contract.AssertNotNull(function);

            var result = Zen.False();
            foreach (var value in arrayExpr.ToArray())
            {
                result = Zen.Or(result, function(value));
            }

            return result;
        }

        /// <summary>
        /// Determines if all elements of the array satisfy some predicate.
        /// </summary>
        /// <param name="arrayExpr">Zen array expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if all elements satisfy the predicate.</returns>
        public static Zen<bool> All<T, TSize>(this Zen<Array<T, TSize>> arrayExpr, Func<Zen<T>, Zen<bool>> function)
        {
            Contract.AssertNotNull(arrayExpr);
            Contract.AssertNotNull(function);

            var result = Zen.True();
            foreach (var value in arrayExpr.ToArray())
            {
                result = Zen.And(result, function(value));
            }

            return result;
        }

        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        /// <param name="arrayExpr">Zen array expression.</param>
        /// <returns>The length.</returns>
        public static int Length<T, TSize>(this Zen<Array<T, TSize>> arrayExpr)
        {
            return Array<T, TSize>.Size;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    [ExcludeFromCodeCoverage] public struct _0 { }
    [ExcludeFromCodeCoverage] public struct _1 { }
    [ExcludeFromCodeCoverage] public struct _2 { }
    [ExcludeFromCodeCoverage] public struct _3 { }
    [ExcludeFromCodeCoverage] public struct _4 { }
    [ExcludeFromCodeCoverage] public struct _5 { }
    [ExcludeFromCodeCoverage] public struct _6 { }
    [ExcludeFromCodeCoverage] public struct _7 { }
    [ExcludeFromCodeCoverage] public struct _8 { }
    [ExcludeFromCodeCoverage] public struct _9 { }
    [ExcludeFromCodeCoverage] public struct _10 { }
    [ExcludeFromCodeCoverage] public struct _11 { }
    [ExcludeFromCodeCoverage] public struct _12 { }
    [ExcludeFromCodeCoverage] public struct _13 { }
    [ExcludeFromCodeCoverage] public struct _14 { }
    [ExcludeFromCodeCoverage] public struct _15 { }
    [ExcludeFromCodeCoverage] public struct _16 { }
    [ExcludeFromCodeCoverage] public struct _17 { }
    [ExcludeFromCodeCoverage] public struct _18 { }
    [ExcludeFromCodeCoverage] public struct _19 { }
    [ExcludeFromCodeCoverage] public struct _20 { }
    [ExcludeFromCodeCoverage] public struct _21 { }
    [ExcludeFromCodeCoverage] public struct _22 { }
    [ExcludeFromCodeCoverage] public struct _23 { }
    [ExcludeFromCodeCoverage] public struct _24 { }
    [ExcludeFromCodeCoverage] public struct _25 { }
    [ExcludeFromCodeCoverage] public struct _26 { }
    [ExcludeFromCodeCoverage] public struct _27 { }
    [ExcludeFromCodeCoverage] public struct _28 { }
    [ExcludeFromCodeCoverage] public struct _29 { }
    [ExcludeFromCodeCoverage] public struct _30 { }
    [ExcludeFromCodeCoverage] public struct _31 { }
    [ExcludeFromCodeCoverage] public struct _32 { }
    [ExcludeFromCodeCoverage] public struct _33 { }
    [ExcludeFromCodeCoverage] public struct _34 { }
    [ExcludeFromCodeCoverage] public struct _35 { }
    [ExcludeFromCodeCoverage] public struct _36 { }
    [ExcludeFromCodeCoverage] public struct _37 { }
    [ExcludeFromCodeCoverage] public struct _38 { }
    [ExcludeFromCodeCoverage] public struct _39 { }
    [ExcludeFromCodeCoverage] public struct _40 { }
    [ExcludeFromCodeCoverage] public struct _41 { }
    [ExcludeFromCodeCoverage] public struct _42 { }
    [ExcludeFromCodeCoverage] public struct _43 { }
    [ExcludeFromCodeCoverage] public struct _44 { }
    [ExcludeFromCodeCoverage] public struct _45 { }
    [ExcludeFromCodeCoverage] public struct _46 { }
    [ExcludeFromCodeCoverage] public struct _47 { }
    [ExcludeFromCodeCoverage] public struct _48 { }
    [ExcludeFromCodeCoverage] public struct _49 { }
    [ExcludeFromCodeCoverage] public struct _50 { }
    [ExcludeFromCodeCoverage] public struct _51 { }
    [ExcludeFromCodeCoverage] public struct _52 { }
    [ExcludeFromCodeCoverage] public struct _53 { }
    [ExcludeFromCodeCoverage] public struct _54 { }
    [ExcludeFromCodeCoverage] public struct _55 { }
    [ExcludeFromCodeCoverage] public struct _56 { }
    [ExcludeFromCodeCoverage] public struct _57 { }
    [ExcludeFromCodeCoverage] public struct _58 { }
    [ExcludeFromCodeCoverage] public struct _59 { }
    [ExcludeFromCodeCoverage] public struct _60 { }
    [ExcludeFromCodeCoverage] public struct _61 { }
    [ExcludeFromCodeCoverage] public struct _62 { }
    [ExcludeFromCodeCoverage] public struct _63 { }
    [ExcludeFromCodeCoverage] public struct _64 { }
    [ExcludeFromCodeCoverage] public struct _65 { }
    [ExcludeFromCodeCoverage] public struct _66 { }
    [ExcludeFromCodeCoverage] public struct _67 { }
    [ExcludeFromCodeCoverage] public struct _68 { }
    [ExcludeFromCodeCoverage] public struct _69 { }
    [ExcludeFromCodeCoverage] public struct _70 { }
    [ExcludeFromCodeCoverage] public struct _71 { }
    [ExcludeFromCodeCoverage] public struct _72 { }
    [ExcludeFromCodeCoverage] public struct _73 { }
    [ExcludeFromCodeCoverage] public struct _74 { }
    [ExcludeFromCodeCoverage] public struct _75 { }
    [ExcludeFromCodeCoverage] public struct _76 { }
    [ExcludeFromCodeCoverage] public struct _77 { }
    [ExcludeFromCodeCoverage] public struct _78 { }
    [ExcludeFromCodeCoverage] public struct _79 { }
    [ExcludeFromCodeCoverage] public struct _80 { }
    [ExcludeFromCodeCoverage] public struct _81 { }
    [ExcludeFromCodeCoverage] public struct _82 { }
    [ExcludeFromCodeCoverage] public struct _83 { }
    [ExcludeFromCodeCoverage] public struct _84 { }
    [ExcludeFromCodeCoverage] public struct _85 { }
    [ExcludeFromCodeCoverage] public struct _86 { }
    [ExcludeFromCodeCoverage] public struct _87 { }
    [ExcludeFromCodeCoverage] public struct _88 { }
    [ExcludeFromCodeCoverage] public struct _89 { }
    [ExcludeFromCodeCoverage] public struct _90 { }
    [ExcludeFromCodeCoverage] public struct _91 { }
    [ExcludeFromCodeCoverage] public struct _92 { }
    [ExcludeFromCodeCoverage] public struct _93 { }
    [ExcludeFromCodeCoverage] public struct _94 { }
    [ExcludeFromCodeCoverage] public struct _95 { }
    [ExcludeFromCodeCoverage] public struct _96 { }
    [ExcludeFromCodeCoverage] public struct _97 { }
    [ExcludeFromCodeCoverage] public struct _98 { }
    [ExcludeFromCodeCoverage] public struct _99 { }
    [ExcludeFromCodeCoverage] public struct _100 { }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}