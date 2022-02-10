// <copyright file="Bag.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a simple unordered multi-set.
    /// </summary>
    public class FiniteBag<T>
    {
        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        [ZenSize(enumerationType: EnumerationType.FixedSize)]
        public Seq<Option<T>> Values { get; set; } = new Seq<Option<T>>();

        /// <summary>
        /// Convert this Zen Bag to a C# array.
        /// </summary>
        /// <returns>A C# array.</returns>
        public T[] ToArray()
        {
            return this.Values.Values.Where(x => x.HasValue).Select(x => x.Value).ToArray();
        }

        /// <summary>
        /// Convert the array to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var elements = this.Values.Values.Where(x => x.HasValue).Select(x => x.Value).ToArray();
            return "{" + string.Join(",", elements) + "}";
        }
    }

    /// <summary>
    /// Static factory class for Bag Zen objects.
    /// </summary>
    public static class FiniteBag
    {
        /// <summary>
        /// Convert a collection of items to a Bag.
        /// </summary>
        /// <param name="values">The items to add to the bag.</param>
        public static FiniteBag<T> FromArray<T>(params T[] values)
        {
            return new FiniteBag<T> { Values = new Seq<Option<T>> { Values = values.Select(Option.Some).ToList() } };
        }

        /// <summary>
        /// The Zen value for a bag from a sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<FiniteBag<T>> Create<T>(Zen<Seq<Option<T>>> seqExpr)
        {
            return Zen.Create<FiniteBag<T>>(("Values", seqExpr));
        }

        /// <summary>
        /// Create an array from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FiniteBag<T>> Create<T>(params Zen<T>[] elements)
        {
            CommonUtilities.ValidateNotNull(elements);

            var asOptions = elements.Select(Option.Create);
            return FiniteBag.Create(Seq.Create(asOptions));
        }
    }

    /// <summary>
    /// Extension methods for Zen bag objects.
    /// </summary>
    public static class FiniteBagExtensions
    {
        /// <summary>
        /// Gets the underlying sequence for a bag.
        /// </summary>
        /// <param name="bagExpr">The bag expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Seq<Option<T>>> Values<T>(this Zen<FiniteBag<T>> bagExpr)
        {
            CommonUtilities.ValidateNotNull(bagExpr);

            return bagExpr.GetField<FiniteBag<T>, Seq<Option<T>>>("Values");
        }

        /// <summary>
        /// Checks if the bag contains an element.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to check for containment.</param>
        /// <returns>Zen value indicating the containment.</returns>
        public static Zen<bool> Contains<T>(this Zen<FiniteBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return bagExpr.Values().Any(o => And(o.HasValue(), o.Value() == value));
        }

        /// <summary>
        /// Add a value to a bag.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to add to the bag.</param>
        /// <returns>The new bag from adding the value.</returns>
        public static Zen<FiniteBag<T>> Add<T>(this Zen<FiniteBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return FiniteBag.Create(bagExpr.Values().AddFront(Option.Create(value)));
        }

        /// <summary>
        /// Add a value to a bag if space given the maximum size.
        /// If no space, then replace the last value.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to add to the bag.</param>
        /// <returns>The new bag from adding the value.</returns>
        public static Zen<FiniteBag<T>> AddIfSpace<T>(this Zen<FiniteBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return FiniteBag.Create(AddIfSpace(bagExpr.Values(), value));
        }

        /// <summary>
        /// Add a value to a sequence if there is space.
        /// Otherwise, replace the last value.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="value">The value to add to the sequence.</param>
        /// <returns>The new bag from adding the value.</returns>
        public static Zen<Seq<Option<T>>> AddIfSpace<T>(this Zen<Seq<Option<T>>> seqExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(value);

            return seqExpr.Case(seqExpr, (hd, tl) =>
                If(hd.HasValue(), AddIfSpace(tl, value).AddFront(hd), tl.AddFront(Option.Create(value))));
        }

        /// <summary>
        /// Remove all occurences of a value from a bag.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to add to the bag.</param>
        /// <returns>The new bag from adding the value..</returns>
        public static Zen<FiniteBag<T>> Remove<T>(this Zen<FiniteBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return FiniteBag.Create(bagExpr.Values().Select(o => If(And(o.HasValue(), o.Value() == value), Option.Null<T>(), o)));
        }

        /// <summary>
        /// Compute the size of the bag.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <returns>The new bag from adding the value..</returns>
        public static Zen<ushort> Size<T>(this Zen<FiniteBag<T>> bagExpr)
        {
            CommonUtilities.ValidateNotNull(bagExpr);

            return Size(bagExpr.Values());
        }

        /// <summary>
        /// Compute the size of the bag sequence.
        /// </summary>
        /// <param name="seqExpr">Zen array expression.</param>
        /// <returns>The new bag from adding the value..</returns>
        private static Zen<ushort> Size<T>(Zen<Seq<Option<T>>> seqExpr)
        {
            return seqExpr.Case(0, (hd, tl) => If<ushort>(hd.HasValue(), 1, 0) + Size(tl));
        }
    }
}
