﻿// <copyright file="Tuple.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2}"/> class.
        /// </summary>
        public Pair()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2}"/> class.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        public Pair(T1 item1, T2 item2)
        {
            Contract.AssertNotNull(item1);
            Contract.AssertNotNull(item2);

            this.Item1 = item1;
            this.Item2 = item2;
        }

        /// <summary>
        /// Convert a value tuple to a pair.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator Pair<T1, T2>((T1, T2) value)
        {
            return new Pair<T1, T2>(value.Item1, value.Item2);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2> p && Equals(p);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public bool Equals(Pair<T1, T2> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2})";
        }
    }

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2, T3> : IEquatable<Pair<T1, T2, T3>>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        ///     The third item.
        /// </summary>
        public T3 Item3 { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2, T3}"/> class.
        /// </summary>
        public Pair()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="item3">The third item.</param>
        public Pair(T1 item1, T2 item2, T3 item3)
        {
            Contract.AssertNotNull(item1);
            Contract.AssertNotNull(item2);
            Contract.AssertNotNull(item3);

            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }

        /// <summary>
        /// Convert a value tuple to a pair.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator Pair<T1, T2, T3>((T1, T2, T3) value)
        {
            return new Pair<T1, T2, T3>(value.Item1, value.Item2, value.Item3);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2, T3> p && Equals(p);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public bool Equals(Pair<T1, T2, T3> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2) &&
                   this.Item3.Equals(other.Item3);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2}, {this.Item3})";
        }
    }

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2, T3, T4> : IEquatable<Pair<T1, T2, T3, T4>>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        ///     The third item.
        /// </summary>
        public T3 Item3 { get; set; }

        /// <summary>
        ///     The fourth item.
        /// </summary>
        public T4 Item4 { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2, T3, T4}"/> class.
        /// </summary>
        public Pair()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2, T3, T4}"/> class.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="item3">The third item.</param>
        /// <param name="item4">The fourth item.</param>
        public Pair(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Contract.AssertNotNull(item1);
            Contract.AssertNotNull(item2);
            Contract.AssertNotNull(item3);
            Contract.AssertNotNull(item4);

            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
        }

        /// <summary>
        /// Convert a value tuple to a pair.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator Pair<T1, T2, T3, T4>((T1, T2, T3, T4) value)
        {
            return new Pair<T1, T2, T3, T4>(value.Item1, value.Item2, value.Item3, value.Item4);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2, T3, T4> p && Equals(p);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public bool Equals(Pair<T1, T2, T3, T4> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2) &&
                   this.Item3.Equals(other.Item3) &&
                   this.Item4.Equals(other.Item4);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            hashCode = hashCode * -1521134295 + EqualityComparer<T4>.Default.GetHashCode(Item4);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4})";
        }
    }

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2, T3, T4, T5>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        ///     The third item.
        /// </summary>
        public T3 Item3 { get; set; }

        /// <summary>
        ///     The fourth item.
        /// </summary>
        public T4 Item4 { get; set; }

        /// <summary>
        ///     The fifth item.
        /// </summary>
        public T5 Item5 { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        public Pair()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Pair{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="item3">The third item.</param>
        /// <param name="item4">The fourth item.</param>
        /// <param name="item5">The fifth item.</param>
        public Pair(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Contract.AssertNotNull(item1);
            Contract.AssertNotNull(item2);
            Contract.AssertNotNull(item3);
            Contract.AssertNotNull(item4);
            Contract.AssertNotNull(item5);

            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
        }

        /// <summary>
        /// Convert a value tuple to a pair.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator Pair<T1, T2, T3, T4, T5>((T1, T2, T3, T4, T5) value)
        {
            return new Pair<T1, T2, T3, T4, T5>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2, T3, T4, T5> p && Equals(p);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public bool Equals(Pair<T1, T2, T3, T4, T5> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2) &&
                   this.Item3.Equals(other.Item3) &&
                   this.Item4.Equals(other.Item4) &&
                   this.Item5.Equals(other.Item5);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            hashCode = hashCode * -1521134295 + EqualityComparer<T4>.Default.GetHashCode(Item4);
            hashCode = hashCode * -1521134295 + EqualityComparer<T5>.Default.GetHashCode(Item5);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5})";
        }
    }

    /// <summary>
    /// Pair factory class for creating Zen pair objects.
    /// </summary>
    public static class Pair
    {
        /// <summary>
        /// Create a new Zen value for a tuple.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Pair<T1, T2>> Create<T1, T2>(Zen<T1> expr1, Zen<T2> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

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
        public static Zen<Pair<T1, T2, T3>> Create<T1, T2, T3>(Zen<T1> expr1, Zen<T2> expr2, Zen<T3> expr3)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.AssertNotNull(expr3);

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
        public static Zen<Pair<T1, T2, T3, T4>> Create<T1, T2, T3, T4>(Zen<T1> expr1, Zen<T2> expr2, Zen<T3> expr3, Zen<T4> expr4)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.AssertNotNull(expr3);

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
        public static Zen<Pair<T1, T2, T3, T4, T5>> Create<T1, T2, T3, T4, T5>(Zen<T1> expr1, Zen<T2> expr2, Zen<T3> expr3, Zen<T4> expr4, Zen<T5> expr5)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.AssertNotNull(expr3);

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
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2>, T1>("Item1");
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2, T3>(this Zen<Pair<T1, T2, T3>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3>, T1>("Item1");
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T1>("Item1");
        }

        /// <summary>
        /// Get the first element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> Item1<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T1>("Item1");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2>(this Zen<Pair<T1, T2>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2>, T2>("Item2");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2, T3>(this Zen<Pair<T1, T2, T3>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3>, T2>("Item2");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T2>("Item2");
        }

        /// <summary>
        /// Get the second element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Item2<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T2>("Item2");
        }

        /// <summary>
        /// Get the third element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T3> Item3<T1, T2, T3>(this Zen<Pair<T1, T2, T3>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3>, T3>("Item3");
        }

        /// <summary>
        /// Get the third element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T3> Item3<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T3>("Item3");
        }

        /// <summary>
        /// Get the third element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T3> Item3<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T3>("Item3");
        }

        /// <summary>
        /// Get the fourth element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T4> Item4<T1, T2, T3, T4>(this Zen<Pair<T1, T2, T3, T4>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4>, T4>("Item4");
        }

        /// <summary>
        /// Get the fourth element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T4> Item4<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T4>("Item4");
        }

        /// <summary>
        /// Get the fifth element of a Zen tuple.
        /// </summary>
        /// <param name="expr">Zen tuple expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T5> Item5<T1, T2, T3, T4, T5>(this Zen<Pair<T1, T2, T3, T4, T5>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Pair<T1, T2, T3, T4, T5>, T5>("Item5");
        }
    }
}
