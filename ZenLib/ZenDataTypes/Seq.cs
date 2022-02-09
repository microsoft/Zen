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
    public class Seq<T>
    {
        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        public IList<T> Values { get; set; } = new List<T>();

        /// <summary>
        /// Convert the sequence to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"[{string.Join(",", this.Values)}]";
        }
    }

    /// <summary>
    /// Static factory class for sequence Zen objects.
    /// </summary>
    public static class Seq
    {
        /// <summary>
        /// Convert a collection of items to a sequence.
        /// </summary>
        /// <param name="values">The items to add to the sequence.</param>
        public static Seq<T> FromArray<T>(params T[] values)
        {
            return new Seq<T> { Values = values.ToList() };
        }

        /// <summary>
        /// The Zen value for an empty sequence.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Empty<T>()
        {
            return Zen.Create<Seq<T>>(("Values", ZenListEmptyExpr<T>.Instance));
        }

        /// <summary>
        /// The Zen value for a sequence from a list.
        /// </summary>
        /// <param name="listExpr">The list expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Seq<T>> Create<T>(Zen<IList<T>> listExpr)
        {
            return Zen.Create<Seq<T>>(("Values", listExpr));
        }

        /// <summary>
        /// Create a singleton sequence.
        /// </summary>
        /// <param name="element">Zen element.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Create<T>(Zen<T> element)
        {
            CommonUtilities.ValidateNotNull(element);

            return Seq.Empty<T>().AddFront(element);
        }

        /// <summary>
        /// Create a sequence from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Create<T>(IEnumerable<Zen<T>> elements)
        {
            CommonUtilities.ValidateNotNull(elements);

            return Seq.Create(Zen.List(elements.ToArray()));
        }
    }

    /// <summary>
    /// Extension methods for Zen sequence objects.
    /// </summary>
    public static class SeqExtensions
    {
        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<IList<T>> Values<T>(this Zen<Seq<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            return expr.GetField<Seq<T>, IList<T>>("Values");
        }

        /// <summary>
        /// Add a value to the back of a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> AddBack<T>(this Zen<Seq<T>> seqExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return seqExpr.Case(
                empty: Seq.Create(valueExpr),
                cons: (hd, tl) => AddBack(tl, valueExpr).AddFront(hd));
        }

        /// <summary>
        /// Add a value to the front of a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> AddFront<T>(this Zen<Seq<T>> seqExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return Seq.Create(ZenListAddFrontExpr<T>.Create(seqExpr.Values(), valueExpr));
        }

        /// <summary>
        /// Match and deconstruct a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence expression.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        /// <returns>Zen value.</returns>
        public static Zen<TResult> Case<T, TResult>(
            this Zen<Seq<T>> seqExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<Seq<T>>, Zen<TResult>> cons)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(empty);
            CommonUtilities.ValidateNotNull(cons);

            return ZenListCaseExpr<T, TResult>.Create(seqExpr.Values(), empty, (hd, tl) => cons(hd, Seq.Create(tl)));
        }

        /// <summary>
        /// Find an element that satisfies a predicate.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The filtering function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Find<T>(this Zen<Seq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return seqExpr.Case(
                empty: Option.Null<T>(),
                cons: (hd, tl) => If(predicate(hd), Option.Create(hd), tl.Find(predicate)));
        }

        /// <summary>
        /// Get the length of the sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Length<T>(this Zen<Seq<T>> seqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);

            return seqExpr.Case(
                empty: Constant<ushort>(0),
                cons: (hd, tl) => tl.Length() + 1);
        }

        /// <summary>
        /// Map over a sequence to create a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="function">The map function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T2>> Select<T1, T2>(this Zen<Seq<T1>> seqExpr, Func<Zen<T1>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(function);

            return seqExpr.Case(
                empty: Seq.Empty<T2>(),
                cons: (hd, tl) => tl.Select(function).AddFront(function(hd)));
        }

        /// <summary>
        /// Filter a sequence to create a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The filtering function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Where<T>(this Zen<Seq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) =>
                {
                    var x = tl.Where(predicate);
                    return If(predicate(hd), x.AddFront(hd), x);
                });
        }

        /// <summary>
        /// Whether a sequence is empty.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsEmpty<T>(this Zen<Seq<T>> seqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);

            return seqExpr.Case(empty: True(), cons: (hd, tl) => False());
        }

        /// <summary>
        /// Check if a sequence contains an element.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="element">The element.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<Seq<T>> seqExpr, Zen<T> element)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(element);

            return seqExpr.Any((x) => Eq(x, element));
        }

        /// <summary>
        /// Append two sequences together.
        /// </summary>
        /// <param name="seqExpr1">First Zen sequence expression.</param>
        /// <param name="seqExpr2">Second Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Append<T>(this Zen<Seq<T>> seqExpr1, Zen<Seq<T>> seqExpr2)
        {
            CommonUtilities.ValidateNotNull(seqExpr1);
            CommonUtilities.ValidateNotNull(seqExpr2);

            return seqExpr1.Case(
                empty: seqExpr2,
                cons: (hd, tl) => tl.Append(seqExpr2).AddFront(hd));
        }

        /// <summary>
        /// Reverse a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Reverse<T>(this Zen<Seq<T>> seqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);

            return Reverse(seqExpr, Seq.Empty<T>());
        }

        private static Zen<Seq<T>> Reverse<T>(this Zen<Seq<T>> expr, Zen<Seq<T>> acc)
        {
            return expr.Case(
                empty: acc,
                cons: (hd, tl) => tl.Reverse(acc.AddFront(hd)));
        }

        /// <summary>
        /// Intersperse a sequence with an element.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="element">The element to intersperse.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Intersperse<T>(this Zen<Seq<T>> seqExpr, Zen<T> element)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(element);

            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) => If(IsEmpty(tl), Seq.Create(hd), tl.Intersperse(element).AddFront(hd)));
        }

        /// <summary>
        /// Count the occurences of an element in a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Duplicates<T>(this Zen<Seq<T>> seqExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return seqExpr.Case(
                empty: Constant<ushort>(0),
                cons: (hd, tl) =>
                    If(hd == valueExpr, tl.Duplicates(valueExpr), tl.Duplicates(valueExpr) + Constant<ushort>(1)));
        }

        /// <summary>
        /// Remove the first instance of a value from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> RemoveFirst<T>(this Zen<Seq<T>> seqExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) => If(hd == valueExpr, tl, tl.RemoveFirst(valueExpr).AddFront(hd)));
        }

        /// <summary>
        /// Remove the first instance of a value from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> RemoveAll<T>(this Zen<Seq<T>> seqExpr, Zen<T> valueExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) =>
                {
                    var tlRemoved = tl.RemoveAll(valueExpr);
                    return If(hd == valueExpr, tlRemoved, tlRemoved.AddFront(hd));
                });
        }

        /// <summary>
        /// Fold a function over a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="acc">The initial value.</param>
        /// <param name="function">The fold function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Fold<T1, T2>(this Zen<Seq<T1>> seqExpr, Zen<T2> acc, Func<Zen<T1>, Zen<T2>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(acc);
            CommonUtilities.ValidateNotNull(function);

            return seqExpr.Case(
                empty: acc,
                cons: (hd, tl) => tl.Fold(function(hd, acc), function));
        }

        /// <summary>
        /// Check if any element of the sequence satisfies a predicate.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate to check.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Any<T>(this Zen<Seq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return seqExpr.Fold(False(), (x, y) => Or(predicate(x), y));
        }

        /// <summary>
        /// Check if all elements of a sequence satisfy a predicate.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate to check.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> All<T>(this Zen<Seq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return seqExpr.Fold(True(), (x, y) => And(predicate(x), y));
        }

        /// <summary>
        /// Take n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Take<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> numElements)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(numElements);

            return Take(seqExpr, numElements, 0);
        }

        /// <summary>
        /// Take n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Seq<T>> Take<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> numElements, int i)
        {
            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) => If(Constant((ushort)i) == numElements, Seq.Empty<T>(), tl.Take(numElements, i + 1).AddFront(hd)));
        }

        /// <summary>
        /// Take elements from a sequence while a predicate is true.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> TakeWhile<T>(this Zen<Seq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) => If(predicate(hd), tl.TakeWhile(predicate).AddFront(hd), Seq.Empty<T>()));
        }

        /// <summary>
        /// Drop n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Drop<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> numElements)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(numElements);

            return Drop(seqExpr, numElements, 0);
        }

        /// <summary>
        /// Drop n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Seq<T>> Drop<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> numElements, int i)
        {
            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) => If(Constant((ushort)i) == numElements, seqExpr, tl.Drop(numElements, i + 1)));
        }

        /// <summary>
        /// Drop elements from a sequence while a predicate is true.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> DropWhile<T>(this Zen<Seq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(predicate);

            return seqExpr.Case(
                empty: Seq.Empty<T>(),
                cons: (hd, tl) => If(predicate(hd), Seq.Empty<T>(), tl.DropWhile(predicate).AddFront(hd)));
        }

        /// <summary>
        /// Split a sequence at an element.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">The index to split at.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Pair<Seq<T>, Seq<T>>> SplitAt<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> index)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(index);

            return SplitAt(seqExpr, index, 0);
        }

        /// <summary>
        /// Split a sequence at an element.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">The index to split at.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Pair<Seq<T>, Seq<T>>> SplitAt<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> index, int i)
        {
            return seqExpr.Case(
                empty: Pair.Create(Seq.Empty<T>(), Seq.Empty<T>()),
                cons: (hd, tl) =>
                {
                    var tup = tl.SplitAt(index, i + 1);
                    return If((ushort)i <= index,
                              Pair.Create(tup.Item1().AddFront(hd), tup.Item2()),
                              Pair.Create(tup.Item1(), tup.Item2().AddFront(hd)));
                });
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> At<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> index)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(index);

            return At(seqExpr, index, 0);
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="i">Current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<T>> At<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> index, int i)
        {
            return seqExpr.Case(
                empty: Option.Null<T>(),
                cons: (hd, tl) => If(Constant<ushort>((ushort)i) == index, Option.Create(hd), tl.At(index, i + 1)));
        }

        /// <summary>
        /// Sets the value of the sequence at a given index and returns a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Set<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> index, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(index);
            CommonUtilities.ValidateNotNull(value);

            return Set(seqExpr, index, value, 0);
        }

        /// <summary>
        /// Sets the value of the sequence at a given index and returns a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <param name="i">Current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Seq<T>> Set<T>(this Zen<Seq<T>> seqExpr, Zen<ushort> index, Zen<T> value, int i)
        {
            return seqExpr.Case(
                empty: seqExpr,
                cons: (hd, tl) => If(Constant((ushort)i) == index, tl.AddFront(value), tl.Set(index, value, i + 1).AddFront(hd)));
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<ushort>> IndexOf<T>(this Zen<Seq<T>> seqExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(value);

            return seqExpr.IndexOf(value, 0);
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <param name="i">Current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<ushort>> IndexOf<T>(this Zen<Seq<T>> seqExpr, Zen<T> value, int i)
        {
            return seqExpr.Case(
                empty: Option.Null<ushort>(),
                cons: (hd, tl) => If(value == hd, Option.Create(Constant((ushort)i)), tl.IndexOf(value, i + 1)));
        }

        /// <summary>
        /// Check if a sequence is sorted.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsSorted<T>(this Zen<Seq<T>> seqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);

            return seqExpr.Case(
                empty: True(),
                cons: (hd1, tl1) =>
                    tl1.Case(empty: True(),
                              cons: (hd2, tl2) => And(hd1 <= hd2, tl1.IsSorted())));
        }

        /// <summary>
        /// Sort a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Sort<T>(this Zen<Seq<T>> seqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);

            return seqExpr.Case(empty: Seq.Empty<T>(), cons: (hd, tl) => Insert(hd, tl.Sort()));
        }

        /// <summary>
        /// Insert a value into a sorted sequence.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Insert<T>(Zen<T> element, Zen<Seq<T>> seqExpr)
        {
            CommonUtilities.ValidateNotNull(element);
            CommonUtilities.ValidateNotNull(seqExpr);

            return seqExpr.Case(
                empty: Seq.Create(element),
                cons: (hd, tl) => If(element <= hd, seqExpr.AddFront(element), Insert(element, tl).AddFront(hd)));
        }
    }
}
