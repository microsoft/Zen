// <copyright file="FSeq.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a simple finite sequence.
    /// </summary>
    public class FSeq<T> : IEquatable<FSeq<T>>
    {
        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        public ImmutableList<T> Values { get; set; } = ImmutableList<T>.Empty;

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        public FSeq()
        {
            this.Values = ImmutableList<T>.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public FSeq(params T[] values)
        {
            this.Values = ImmutableList<T>.Empty.AddRange(values);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public FSeq(IEnumerable<T> values)
        {
            this.Values = ImmutableList<T>.Empty.AddRange(values);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        /// <param name="list">The existing list.</param>
        internal FSeq(ImmutableList<T> list)
        {
            this.Values = list;
        }

        /// <summary>
        /// Checks if the sequence is empty.
        /// </summary>
        /// <returns>True if the sequence contains no elements..</returns>
        public bool IsEmpty()
        {
            return this.Values.Count == 0;
        }

        /// <summary>
        /// Gets the count of elements in the sequence.
        /// </summary>
        /// <returns>An integer count.</returns>
        public int Count()
        {
            return this.Values.Count;
        }

        /// <summary>
        /// Add an element to the front of the list.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public FSeq<T> AddFront(T value)
        {
            var l = (ImmutableList<T>)this.Values;
            return new FSeq<T>(l.Insert(0, value));
        }

        /// <summary>
        /// Convert the sequence to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"[{string.Join(",", this.Values)}]";
        }

        /// <summary>
        /// Equality for sequences.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is FSeq<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for sequences.
        /// </summary>
        /// <param name="other">The other sequences.</param>
        /// <returns>True or false.</returns>
        public bool Equals(FSeq<T> other)
        {
            if (this.Values.Count != other.Values.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Values.Count; i++)
            {
                if (!this.Values[i].Equals(other.Values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Hashcode for sequences.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            var hash = 1291433875;
            foreach (var element in this.Values)
            {
                hash += element.GetHashCode();
            }

            return hash;
        }

        /// <summary>
        /// Equality for sequences.
        /// </summary>
        /// <param name="left">The left sequence.</param>
        /// <param name="right">The right sequence.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(FSeq<T> left, FSeq<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for sequences.
        /// </summary>
        /// <param name="left">The left sequence.</param>
        /// <param name="right">The right sequence.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(FSeq<T> left, FSeq<T> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for sequence Zen objects.
    /// </summary>
    public static class FSeq
    {
        /// <summary>
        /// Convert a collection of items to a sequence.
        /// </summary>
        /// <param name="values">The items to add to the sequence.</param>
        public static FSeq<T> FromRange<T>(IEnumerable<T> values)
        {
            return new FSeq<T> { Values = ImmutableList.CreateRange(values) };
        }

        /// <summary>
        /// The Zen value for an empty sequence.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Empty<T>()
        {
            return ZenListEmptyExpr<T>.Instance;
        }

        /// <summary>
        /// Create a singleton sequence.
        /// </summary>
        /// <param name="element">Zen element.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Create<T>(Zen<T> element)
        {
            Contract.AssertNotNull(element);

            return FSeq.Empty<T>().AddFront(element);
        }

        /// <summary>
        /// Create a sequence from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Create<T>(IEnumerable<Zen<T>> elements)
        {
            Contract.AssertNotNull(elements);

            return Zen.List(elements.ToArray());
        }

        /// <summary>
        /// Add a value to the back of a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> AddBack<T>(this Zen<FSeq<T>> seqExpr, Zen<T> valueExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(valueExpr);

            return seqExpr.Append(FSeq.Empty<T>().AddFront(valueExpr));
        }

        /// <summary>
        /// Add a value to the front of a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> AddFront<T>(this Zen<FSeq<T>> seqExpr, Zen<T> valueExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(valueExpr);

            return ZenListAddFrontExpr<T>.Create(seqExpr, valueExpr);
        }

        /// <summary>
        /// Match and deconstruct a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence expression.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        /// <returns>Zen value.</returns>
        public static Zen<TResult> Case<T, TResult>(
            this Zen<FSeq<T>> seqExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<FSeq<T>>, Zen<TResult>> cons)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(empty);
            Contract.AssertNotNull(cons);

            return ZenListCaseExpr<T, TResult>.Create(seqExpr, empty, (hd, tl) => cons(hd, tl));
        }

        /// <summary>
        /// Find an element that satisfies a predicate.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The filtering function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Find<T>(this Zen<FSeq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(predicate);

            return seqExpr.Fold(Option.Null<T>(), (x, acc) => If(predicate(x), Option.Create(x), acc));
        }

        /// <summary>
        /// Get the head of the sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>The head, or the default value if the sequence is empty.</returns>
        public static Zen<T> Head<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            return seqExpr.Case(empty: Zen.Default<T>(), cons: (hd, tl) => hd);
        }

        /// <summary>
        /// Get the tail of the sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>The tail, empty if the sequence is empty.</returns>
        public static Zen<FSeq<T>> Tail<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            return seqExpr.Case(empty: FSeq.Empty<T>(), cons: (hd, tl) => tl);
        }

        /// <summary>
        /// Get the length of the sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Length<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            return seqExpr.Fold(Constant<ushort>(0), (x, acc) => acc + 1);
        }

        /// <summary>
        /// Map over a sequence to create a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="function">The map function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T2>> Select<T1, T2>(this Zen<FSeq<T1>> seqExpr, Func<Zen<T1>, Zen<T2>> function)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(function);

            return seqExpr.Fold(FSeq.Empty<T2>(), (x, acc) => acc.AddFront(function(x)));
        }

        /// <summary>
        /// Filter a sequence to create a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The filtering function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Where<T>(this Zen<FSeq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(predicate);

            return seqExpr.Fold(FSeq.Empty<T>(), (x, acc) => If(predicate(x), acc.AddFront(x), acc));
        }

        /// <summary>
        /// Whether a sequence is empty.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsEmpty<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            return seqExpr.Case(empty: True(), (hd, tl) => False());
        }

        /// <summary>
        /// Check if a sequence contains an element.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="element">The element.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<FSeq<T>> seqExpr, Zen<T> element)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(element);

            return seqExpr.Any((x) => Eq(x, element));
        }

        /// <summary>
        /// Append two sequences together.
        /// </summary>
        /// <param name="seqExpr1">First Zen sequence expression.</param>
        /// <param name="seqExpr2">Second Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Append<T>(this Zen<FSeq<T>> seqExpr1, Zen<FSeq<T>> seqExpr2)
        {
            Contract.AssertNotNull(seqExpr1);
            Contract.AssertNotNull(seqExpr2);

            return seqExpr1.Fold(seqExpr2, (x, acc) => acc.AddFront(x));
        }

        /// <summary>
        /// Reverse a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Reverse<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            return Reverse(seqExpr, FSeq.Empty<T>());
        }

        /// <summary>
        /// Reverse a sequence.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="acc">An accumulator.</param>
        /// <returns>The reversed sequence.</returns>
        private static Zen<FSeq<T>> Reverse<T>(this Zen<FSeq<T>> expr, Zen<FSeq<T>> acc)
        {
            return expr.Case(
                empty: acc,
                cons: (hd, tl) => tl.Reverse(acc.AddFront(hd)));
        }

        /// <summary>
        /// Count the occurences of an element in a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Duplicates<T>(this Zen<FSeq<T>> seqExpr, Zen<T> valueExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(valueExpr);

            return seqExpr.Fold(Constant<ushort>(0), (x, acc) => If(x == valueExpr, acc + 1, acc));
        }

        /// <summary>
        /// Remove the first instance of a value from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> RemoveAll<T>(this Zen<FSeq<T>> seqExpr, Zen<T> valueExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(valueExpr);

            return seqExpr.Where(x => x != valueExpr);
        }

        /// <summary>
        /// Fold a function over a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="acc">The initial value.</param>
        /// <param name="function">The fold function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Fold<T1, T2>(this Zen<FSeq<T1>> seqExpr, Zen<T2> acc, Func<Zen<T1>, Zen<T2>, Zen<T2>> function)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(acc);
            Contract.AssertNotNull(function);

            return seqExpr.Case(
                empty: acc,
                cons: (hd, tl) => function(hd, tl.Fold(acc, function)));
        }

        /// <summary>
        /// Fold a function over a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="acc">The initial value.</param>
        /// <param name="function">The fold function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> FoldLeft<T1, T2>(this Zen<FSeq<T1>> seqExpr, Zen<T2> acc, Func<Zen<T2>, Zen<T1>, Zen<T2>> function)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(acc);
            Contract.AssertNotNull(function);

            return seqExpr.Case(
                empty: acc,
                cons: (hd, tl) => tl.FoldLeft(function(acc, hd), function));
        }

        /// <summary>
        /// Check if any element of the sequence satisfies a predicate.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate to check.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Any<T>(this Zen<FSeq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(predicate);

            return seqExpr.Fold(False(), (x, y) => Or(predicate(x), y));
        }

        /// <summary>
        /// Check if all elements of a sequence satisfy a predicate.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate to check.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> All<T>(this Zen<FSeq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(predicate);

            return seqExpr.Fold(True(), (x, y) => And(predicate(x), y));
        }

        /// <summary>
        /// Take n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Take<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> numElements)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(numElements);

            var init = Pair.Create(Constant<ushort>(0), FSeq.Empty<T>());
            return seqExpr.FoldLeft(init, (acc, x) =>
                Pair.Create(acc.Item1() + 1, If(acc.Item1() >= numElements, acc.Item2(), acc.Item2().AddFront(x)))).Item2().Reverse();
        }

        /// <summary>
        /// Take elements from a sequence while a predicate is true.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> TakeWhile<T>(this Zen<FSeq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(predicate);

            return seqExpr.Case(
                empty: FSeq.Empty<T>(),
                cons: (hd, tl) => If(predicate(hd), tl.TakeWhile(predicate).AddFront(hd), FSeq.Empty<T>()));
        }

        /// <summary>
        /// Drop n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Drop<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> numElements)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(numElements);

            return Drop(seqExpr, numElements, 0);
        }

        /// <summary>
        /// Drop n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<FSeq<T>> Drop<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> numElements, int i)
        {
            return seqExpr.Case(
                empty: FSeq.Empty<T>(),
                cons: (hd, tl) => If(Constant((ushort)i) == numElements, seqExpr, tl.Drop(numElements, i + 1)));
        }

        /// <summary>
        /// Drop elements from a sequence while a predicate is true.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> DropWhile<T>(this Zen<FSeq<T>> seqExpr, Func<Zen<T>, Zen<bool>> predicate)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(predicate);

            return seqExpr.Case(
                empty: FSeq.Empty<T>(),
                cons: (hd, tl) => If(predicate(hd), FSeq.Empty<T>(), tl.DropWhile(predicate).AddFront(hd)));
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> At<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> index)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(index);

            var init = Pair.Create(Constant<ushort>(0), Option.Null<T>());
            return seqExpr.FoldLeft(init, (acc, x) =>
                Pair.Create(acc.Item1() + 1, If(acc.Item1() == index, Option.Create(x), acc.Item2()))).Item2();
        }

        /// <summary>
        /// Sets the value of the sequence at a given index and returns a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Set<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> index, Zen<T> value)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(index);
            Contract.AssertNotNull(value);

            var init = Pair.Create(Constant<ushort>(0), FSeq.Empty<T>());
            return seqExpr.FoldLeft(init, (acc, x) =>
                Pair.Create(acc.Item1() + 1, acc.Item2().AddFront(If(acc.Item1() == index, value, x)))).Item2().Reverse();
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> IndexOf<T>(this Zen<FSeq<T>> seqExpr, Zen<T> value)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(value);

            var init = Pair.Create<short, short>(-1, 0);
            return seqExpr.FoldLeft(init, (acc, x) =>
                Pair.Create(If(And(acc.Item1() == -1, value == x), acc.Item2(), acc.Item1()), acc.Item2() + 1)).Item1();
        }
    }
}
