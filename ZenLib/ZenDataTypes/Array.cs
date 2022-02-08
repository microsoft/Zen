// <copyright file="Dict.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a simple sequence.
    /// </summary>
    public class Array<T>
    {
        /// <summary>
        /// Convert a C# array to a Zen Array.
        /// </summary>
        /// <param name="array">The C# array.</param>
        public static implicit operator Array<T>(T[] array)
        {
            return new Array<T> { Values = new Seq<T> { Values = array.ToList() } };
        }

        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        [ZenDepthConfiguration(enumerationType: EnumerationType.FixedSize)]
        public Seq<T> Values { get; set; } = new Seq<T>();

        /// <summary>
        /// Convert this Zen Array to a C# array.
        /// </summary>
        /// <returns>A C# array.</returns>
        public T[] ToArray()
        {
            return this.Values.Values.ToArray();
        }

        /// <summary>
        /// Convert the array to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return this.Values.ToString();
        }
    }

    /// <summary>
    /// Static factory class for array Zen objects.
    /// </summary>
    public static class Array
    {
        /// <summary>
        /// The Zen value for a sequence from a sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Array<T>> Create<T>(Zen<Seq<T>> seqExpr)
        {
            return Zen.Create<Array<T>>(("Values", seqExpr));
        }

        /// <summary>
        /// Create an array from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Array<T>> Create<T>(IEnumerable<Zen<T>> elements)
        {
            CommonUtilities.ValidateNotNull(elements);

            return Array.Create(Seq.Create(elements));
        }
    }

    /// <summary>
    /// Extension methods for Zen array objects.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Gets the underlying sequence for an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Seq<T>> Values<T>(this Zen<Array<T>> arrayExpr)
        {
            CommonUtilities.ValidateNotNull(arrayExpr);

            return arrayExpr.GetField<Array<T>, Seq<T>>("Values");
        }

        /// <summary>
        /// Get the value of an array at an index.
        /// </summary>
        /// <param name="arrayExpr">Zen array expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Get<T>(this Zen<Array<T>> arrayExpr, Zen<ushort> index)
        {
            CommonUtilities.ValidateNotNull(arrayExpr);
            CommonUtilities.ValidateNotNull(index);

            return arrayExpr.Values().At(index);
        }

        /// <summary>
        /// Sets the value of the array at a given index and returns a new array.
        /// </summary>
        /// <param name="arrayExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Array<T>> Set<T>(this Zen<Array<T>> arrayExpr, Zen<ushort> index, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(arrayExpr);
            CommonUtilities.ValidateNotNull(index);

            return Array.Create(arrayExpr.Values().Set(index, value));
        }
    }
}
