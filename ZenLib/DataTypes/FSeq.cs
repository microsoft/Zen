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
        public ImmutableList<Option<T>> Values { get; set; } = ImmutableList<Option<T>>.Empty;

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        public FSeq()
        {
            this.Values = ImmutableList<Option<T>>.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public FSeq(params T[] values)
        {
            this.Values = ImmutableList<Option<T>>.Empty.AddRange(values.Select(Option.Some));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public FSeq(IEnumerable<T> values)
        {
            this.Values = ImmutableList<Option<T>>.Empty.AddRange(values.Select(Option.Some));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSeq{T}"/> class.
        /// </summary>
        /// <param name="list">The existing list.</param>
        internal FSeq(ImmutableList<Option<T>> list)
        {
            this.Values = list;
        }

        /// <summary>
        /// Convert the sequence to a list.
        /// </summary>
        /// <returns>The sequence as a list.</returns>
        public IList<T> ToList()
        {
            return this.Values.Where(x => x.HasValue).Select(x => x.Value).ToList();
        }

        /// <summary>
        /// Checks if the sequence is empty.
        /// </summary>
        /// <returns>True if the sequence contains no elements..</returns>
        public bool IsEmpty()
        {
            return this.ToList().Count == 0;
        }

        /// <summary>
        /// Gets the count of elements in the sequence.
        /// </summary>
        /// <returns>An integer count.</returns>
        public int Count()
        {
            return this.ToList().Count;
        }

        /// <summary>
        /// Add an element to the front of the list.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public FSeq<T> AddFront(T value)
        {
            return new FSeq<T>(this.Values.Insert(0, Option.Some(value)));
        }

        /// <summary>
        /// Add an element to the front of the list.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public FSeq<T> AddFrontOption(Option<T> value)
        {
            if (value.HasValue)
            {
                return new FSeq<T>(this.Values.Insert(0, value));
            }

            return this;
        }

        /// <summary>
        /// Convert the sequence to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"[{string.Join(",", this.ToList())}]";
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
            var v1 = this.ToList();
            var v2 = other.ToList();

            if (v1.Count != v2.Count)
            {
                return false;
            }

            for (int i = 0; i < v1.Count; i++)
            {
                if (!v1[i].Equals(v2[i]))
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
            foreach (var element in this.ToList())
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
            return new FSeq<T> { Values = ImmutableList.CreateRange(values.Select(Option.Some)) };
        }

        /// <summary>
        /// The Zen value for an empty sequence.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Empty<T>()
        {
            return ZenFSeqEmptyExpr<T>.Instance;
        }

        /// <summary>
        /// Create a singleton sequence.
        /// </summary>
        /// <param name="element">Zen element.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Create<T>(Zen<T> element)
        {
            Contract.AssertNotNull(element);

            return FSeq.Empty<T>().AddFrontOption(Option.Create(element));
        }

        /// <summary>
        /// Create a sequence from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> Create<T>(IEnumerable<Zen<T>> elements)
        {
            Contract.AssertNotNull(elements);

            return Zen.List(elements.Select(Option.Create).ToArray());
        }

        /// <summary>
        /// Add a value to the back of a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> AddBackOption<T>(this Zen<FSeq<T>> seqExpr, Zen<Option<T>> valueExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(valueExpr);

            return seqExpr.Append(FSeq.Empty<T>().AddFrontOption(valueExpr));
        }

        /// <summary>
        /// Add a value to the front of a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> AddFrontOption<T>(this Zen<FSeq<T>> seqExpr, Zen<Option<T>> valueExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(valueExpr);

            return ZenFSeqAddFrontExpr<T>.Create(seqExpr, valueExpr);
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

            return seqExpr.AddFrontOption(Option.Create(valueExpr));
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

            return seqExpr.AddBackOption(Option.Create(valueExpr));
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
            ZenLambda<Pair<Option<T>, FSeq<T>>, TResult> cons)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(empty);
            Contract.AssertNotNull(cons);

            return ZenFSeqCaseExpr<T, TResult>.Create(seqExpr, empty, cons);
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

            var findLambda = Zen.Lambda<FSeq<T>, Option<T>>();
            findLambda.Initialize(x => x.Case(
                empty: Option.Null<T>(),
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, Option<T>>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return If(And(hd.IsSome(), predicate(hd.Value())), hd, findLambda.Apply(tl));
                })));

            return findLambda.Apply(seqExpr);
        }

        /// <summary>
        /// Get the head of the sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>The head, or the default value if the sequence is empty.</returns>
        internal static Zen<Option<T>> Head<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            if (FSeqLambdas<T>.HeadLambda == null)
            {
                FSeqLambdas<T>.HeadLambda = Zen.Lambda<Pair<Option<T>, FSeq<T>>, Option<T>>(arg => arg.Item1());
            }

            return seqExpr.Case(empty: Option.Null<T>(), cons: FSeqLambdas<T>.HeadLambda);
        }

        /// <summary>
        /// Get the tail of the sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>The tail, empty if the sequence is empty.</returns>
        internal static Zen<FSeq<T>> Tail<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            if (FSeqLambdas<T>.TailLambda == null)
            {
                FSeqLambdas<T>.TailLambda = Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg => arg.Item2());
            }

            return seqExpr.Case(empty: FSeq.Empty<T>(), cons: FSeqLambdas<T>.TailLambda);
        }

        /// <summary>
        /// Get the length of the sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Length<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            if (FSeqLambdas<T>.LengthLambda == null)
            {
                FSeqLambdas<T>.LengthLambda = Zen.Lambda<FSeq<T>, ushort>();
                FSeqLambdas<T>.LengthLambda.Initialize(x => x.Case(
                    empty: 0,
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, ushort>(arg =>
                    {
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        return FSeqLambdas<T>.LengthLambda.Apply(tl) + If<ushort>(hd.IsSome(), 1, 0);
                    })));
            }

            return FSeqLambdas<T>.LengthLambda.Apply(seqExpr);
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

            var lambda = Zen.Lambda<FSeq<T1>, FSeq<T2>>();
            lambda.Initialize(x => x.Case(
                empty: FSeq.Empty<T2>(),
                cons: Zen.Lambda<Pair<Option<T1>, FSeq<T1>>, FSeq<T2>>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return lambda.Apply(tl).AddFrontOption(hd.Select(function));
                })));

            return lambda.Apply(seqExpr);
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

            var lambda = Zen.Lambda<FSeq<T>, FSeq<T>>();
            lambda.Initialize(x => x.Case(
                empty: FSeq.Empty<T>(),
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return lambda.Apply(tl).AddFrontOption(hd.Where(predicate));
                })));

            return lambda.Apply(seqExpr);
        }

        /// <summary>
        /// Whether a sequence is empty.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsEmpty<T>(this Zen<FSeq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            if (FSeqLambdas<T>.IsEmptyLambda == null)
            {
                FSeqLambdas<T>.IsEmptyLambda = Zen.Lambda<FSeq<T>, bool>();
                FSeqLambdas<T>.IsEmptyLambda.Initialize(x => x.Case(
                    empty: Zen.True(),
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, bool>(arg =>
                    {
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        return Zen.And(hd.IsNone(), FSeqLambdas<T>.IsEmptyLambda.Apply(tl));
                    })));
            }

            return FSeqLambdas<T>.IsEmptyLambda.Apply(seqExpr);
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

            if (FSeqLambdas<T>.AppendLambda == null)
            {
                FSeqLambdas<T>.AppendLambda = Zen.Lambda<Pair<FSeq<T>, FSeq<T>>, FSeq<T>>();
                FSeqLambdas<T>.AppendLambda.Initialize(x => x.Item1().Case(
                    empty: x.Item2(),
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                    {
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        return FSeqLambdas<T>.AppendLambda.Apply(Pair.Create(tl, x.Item2())).AddFrontOption(hd);
                    })));
            }

            return FSeqLambdas<T>.AppendLambda.Apply(Pair.Create(seqExpr1, seqExpr2));
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
        /// <param name="seqExpr">The seq expression.</param>
        /// <param name="acc">An accumulator.</param>
        /// <returns>The reversed sequence.</returns>
        private static Zen<FSeq<T>> Reverse<T>(this Zen<FSeq<T>> seqExpr, Zen<FSeq<T>> acc)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(acc);

            if (FSeqLambdas<T>.ReverseLambda == null)
            {
                FSeqLambdas<T>.ReverseLambda = Zen.Lambda<Pair<FSeq<T>, FSeq<T>>, FSeq<T>>();
                FSeqLambdas<T>.ReverseLambda.Initialize(x => x.Item1().Case(
                    empty: x.Item2(),
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                    {
                        var accumulator = x.Item2();
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        return FSeqLambdas<T>.ReverseLambda.Apply(Pair.Create(tl, accumulator.AddFrontOption(hd)));
                    })));
            }

            return FSeqLambdas<T>.ReverseLambda.Apply(Pair.Create(seqExpr, acc));
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

            var lambda = Zen.Lambda<FSeq<T>, ushort>();
            lambda.Initialize(x => x.Case(
                empty: Constant<ushort>(0),
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, ushort>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return lambda.Apply(tl) + If<ushort>(And(hd.IsSome(), hd.Value() == valueExpr), 1, 0);
                })));

            return lambda.Apply(seqExpr);
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

            var lambda = Zen.Lambda<Pair<FSeq<T1>, T2>, T2>();
            lambda.Initialize(x => x.Item1().Case(
                empty: x.Item2(),
                cons: Zen.Lambda<Pair<Option<T1>, FSeq<T1>>, T2>(arg =>
                {
                    var accumulator = x.Item2();
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    var rest = lambda.Apply(Pair.Create(tl, accumulator));
                    return If(hd.IsNone(), rest, function(hd.Value(), rest));
                })));

            return lambda.Apply(Pair.Create(seqExpr, acc));
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

            var lambda = Zen.Lambda<Pair<FSeq<T1>, T2>, T2>();
            lambda.Initialize(x => x.Item1().Case(
                empty: x.Item2(),
                cons: Zen.Lambda<Pair<Option<T1>, FSeq<T1>>, T2>(arg =>
                {
                    var accumulator = x.Item2();
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    var elt = If(hd.IsNone(), accumulator, function(accumulator, hd.Value()));
                    return lambda.Apply(Pair.Create(tl, elt));
                })));

            return lambda.Apply(Pair.Create(seqExpr, acc));
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

            var lambda = Zen.Lambda<FSeq<T>, bool>();
            lambda.Initialize(x => x.Case(
                empty: Zen.False(),
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, bool>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return Or(And(hd.IsSome(), predicate(hd.Value())), lambda.Apply(tl));
                })));

            return lambda.Apply(seqExpr);
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

            var lambda = Zen.Lambda<FSeq<T>, bool>();
            lambda.Initialize(x => x.Case(
                empty: Zen.True(),
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, bool>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return And(Or(hd.IsNone(), predicate(hd.Value())), lambda.Apply(tl));
                })));

            return lambda.Apply(seqExpr);
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

            return Take(seqExpr, numElements, 0);
        }

        /// <summary>
        /// Take n elements from a sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="numElements">The number of elements to take.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<FSeq<T>> Take<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> numElements, Zen<ushort> i)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(numElements);
            Contract.AssertNotNull(i);

            if (FSeqLambdas<T>.TakeLambda == null)
            {
                FSeqLambdas<T>.TakeLambda = Zen.Lambda<Pair<FSeq<T>, ushort, ushort>, FSeq<T>>();
                FSeqLambdas<T>.TakeLambda.Initialize(x => x.Item1().Case(
                    empty: FSeq.Empty<T>(),
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                    {
                        var numElts = x.Item2();
                        var idx = x.Item3();
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        return If(
                            idx == numElts,
                            FSeq.Empty<T>(),
                            FSeqLambdas<T>.TakeLambda.Apply(Pair.Create(tl, numElts, If(hd.IsNone(), idx, idx + 1))).AddFrontOption(hd));
                    })));
            }

            return FSeqLambdas<T>.TakeLambda.Apply(Pair.Create(seqExpr, numElements, i));
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

            var lambda = Zen.Lambda<FSeq<T>, FSeq<T>>();
            lambda.Initialize(x => x.Case(
                empty: FSeq.Empty<T>(),
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return If(Or(hd.IsNone(), predicate(hd.Value())), lambda.Apply(tl).AddFrontOption(hd), FSeq.Empty<T>());
                })));

            return lambda.Apply(seqExpr);
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
        private static Zen<FSeq<T>> Drop<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> numElements, Zen<ushort> i)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(numElements);
            Contract.AssertNotNull(i);

            if (FSeqLambdas<T>.DropLambda == null)
            {
                FSeqLambdas<T>.DropLambda = Zen.Lambda<Pair<FSeq<T>, ushort, ushort>, FSeq<T>>();
                FSeqLambdas<T>.DropLambda.Initialize(x => x.Item1().Case(
                    empty: FSeq.Empty<T>(),
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                    {
                        var numElts = x.Item2();
                        var idx = x.Item3();
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        return If(
                            idx == numElts,
                            x.Item1(),
                            FSeqLambdas<T>.DropLambda.Apply(Pair.Create(tl, numElts, If(hd.IsNone(), idx, idx + 1))));
                    })));
            }

            return FSeqLambdas<T>.DropLambda.Apply(Pair.Create(seqExpr, numElements, i));
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

            var lambda = Zen.Lambda<FSeq<T>, FSeq<T>>();
            lambda.Initialize(x => x.Case(
                empty: FSeq.Empty<T>(),
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                {
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return If(Or(hd.IsNone(), predicate(hd.Value())), lambda.Apply(tl), x);
                })));

            return lambda.Apply(seqExpr);
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

            return At(seqExpr, index, 0);
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="i">Current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<T>> At<T>(this Zen<FSeq<T>> seqExpr, Zen<ushort> index, Zen<ushort> i)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(index);
            Contract.AssertNotNull(i);

            if (FSeqLambdas<T>.AtLambda == null)
            {
                FSeqLambdas<T>.AtLambda = Zen.Lambda<Pair<FSeq<T>, ushort, ushort>, Option<T>>();
                FSeqLambdas<T>.AtLambda.Initialize(x => x.Item1().Case(
                    empty: Option.Null<T>(),
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, Option<T>>(arg =>
                    {
                        var indexP = x.Item2();
                        var iP = x.Item3();
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        return If(
                            And(iP == indexP, hd.IsSome()),
                            hd,
                            FSeqLambdas<T>.AtLambda.Apply(Pair.Create(tl, indexP, If(hd.IsNone(), iP, iP + 1))));
                    })));
            }

            return FSeqLambdas<T>.AtLambda.Apply(Pair.Create(seqExpr, index, i));
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

            return Set(seqExpr, value, index, 0);
        }

        /// <summary>
        /// Sets the value of the sequence at a given index and returns a new sequence.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="index">Zen index expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <param name="i">Current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<FSeq<T>> Set<T>(this Zen<FSeq<T>> seqExpr, Zen<T> value, Zen<ushort> index, Zen<ushort> i)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(index);
            Contract.AssertNotNull(value);
            Contract.AssertNotNull(i);

            if (FSeqLambdas<T>.SetLambda == null)
            {
                FSeqLambdas<T>.SetLambda = Zen.Lambda<Pair<FSeq<T>, ushort, ushort>, FSeq<T>>();
                FSeqLambdas<T>.SetLambda.Initialize(x => x.Item1().Case(
                    empty: x.Item1(),
                    cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, FSeq<T>>(arg =>
                    {
                        var indexP = x.Item2();
                        var iP = x.Item3();
                        var hd = arg.Item1();
                        var tl = arg.Item2();
                        var guard = And(iP == indexP, hd.IsSome());
                        var trueCase = tl.AddFrontOption(Option.Create(value));
                        var falseCase = FSeqLambdas<T>.SetLambda.Apply(Pair.Create(tl, indexP, If(hd.IsNone(), iP, iP + 1))).AddFrontOption(hd);
                        return If(guard, trueCase, falseCase);
                    })));
            }

            return FSeqLambdas<T>.SetLambda.Apply(Pair.Create(seqExpr, index, i));
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

            return IndexOf(seqExpr, value, 0);
        }

        /// <summary>
        /// Get the value of a sequence at an index.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="value">Zen value expression.</param>
        /// <param name="i">The current index.</param>
        /// <returns>Zen value.</returns>
        private static Zen<short> IndexOf<T>(this Zen<FSeq<T>> seqExpr, Zen<T> value, Zen<short> i)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(value);
            Contract.AssertNotNull(i);

            var lambda = Zen.Lambda<Pair<FSeq<T>, short>, short>();
            lambda.Initialize(x => x.Item1().Case(
                empty: -1,
                cons: Zen.Lambda<Pair<Option<T>, FSeq<T>>, short>(arg =>
                {
                    var idx = x.Item2();
                    var hd = arg.Item1();
                    var tl = arg.Item2();
                    return If(hd == Option.Create(value), idx, lambda.Apply(Pair.Create(tl, If(hd.IsNone(), idx, idx + 1))));
                })));

            return lambda.Apply(Pair.Create(seqExpr, i));
        }
    }

    /// <summary>
    /// The lambda functions for reuse.
    /// </summary>
    internal static class FSeqLambdas<T>
    {
        /// <summary>
        /// The lambda for the append operation.
        /// </summary>
        internal static ZenLambda<Pair<FSeq<T>, FSeq<T>>, FSeq<T>> AppendLambda;

        /// <summary>
        /// The lambda for the reverse operation.
        /// </summary>
        internal static ZenLambda<Pair<FSeq<T>, FSeq<T>>, FSeq<T>> ReverseLambda;

        /// <summary>
        /// The lambda for the head operation.
        /// </summary>
        internal static ZenLambda<Pair<Option<T>, FSeq<T>>, Option<T>> HeadLambda;

        /// <summary>
        /// The lambda for the tail operation.
        /// </summary>
        internal static ZenLambda<Pair<Option<T>, FSeq<T>>, FSeq<T>> TailLambda;

        /// <summary>
        /// The lambda for the length operation.
        /// </summary>
        internal static ZenLambda<FSeq<T>, ushort> LengthLambda;

        /// <summary>
        /// The lambda for the isempty operation.
        /// </summary>
        internal static ZenLambda<FSeq<T>, bool> IsEmptyLambda;

        /// <summary>
        /// The lambda for the take operation.
        /// </summary>
        internal static ZenLambda<Pair<FSeq<T>, ushort, ushort>, FSeq<T>> TakeLambda;

        /// <summary>
        /// The lambda for the drop operation.
        /// </summary>
        internal static ZenLambda<Pair<FSeq<T>, ushort, ushort>, FSeq<T>> DropLambda;

        /// <summary>
        /// The lambda for the at operation.
        /// </summary>
        internal static ZenLambda<Pair<FSeq<T>, ushort, ushort>, Option<T>> AtLambda;

        /// <summary>
        /// The lambda for the set operation.
        /// </summary>
        internal static ZenLambda<Pair<FSeq<T>, ushort, ushort>, FSeq<T>> SetLambda;
    }
}
