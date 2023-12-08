// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Generation;
    using static ZenLib.Zen;

    /// <summary>
    /// A simple option type to parameterize over nullable
    /// values for both structs and classes.
    /// </summary>
    public struct Option<T> : IEquatable<Option<T>>
    {
        internal Option(bool b, T value) : this()
        {
            this.HasValue = b;
            this.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether this option has a value.
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Return the underlying value or another value if none.
        /// </summary>
        /// <param name="other">The default value.</param>
        /// <returns>A value of the underlying type.</returns>
        public T ValueOrDefault(T other)
        {
            if (this.HasValue)
            {
                return this.Value;
            }

            return other;
        }

        /// <summary>
        /// Return the option if it has a value, or the result of a function if not.
        /// Lazy equivalent (without unwrapping) of <see cref="ValueOrDefault"/>.
        /// </summary>
        /// <param name="other">The default-generating function.</param>
        /// <returns>An option of the underlying type.</returns>
        public Option<T> SomeOrDefault(Func<Option<T>> other)
        {
            if (this.HasValue)
            {
                return this;
            }

            return other();
        }

        /// <summary>
        /// Map a function over an option.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>A new option with the function mapped over the value.</returns>
        public Option<TResult> Select<TResult>(Func<T, TResult> function)
        {
            Contract.AssertNotNull(function);
            return this.HasValue ? Option.Some(function(this.Value)) : Option.None<TResult>();
        }

        /// <summary>
        /// Map a function that returns an option over an option and "flatten" the result.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>A new option with the function mapped over the value.</returns>
        public Option<TResult> SelectMany<TResult>(Func<T, Option<TResult>> function)
        {
            Contract.AssertNotNull(function);
            return this.HasValue ? function(this.Value) : Option.None<TResult>();
        }

        /// <summary>
        /// Filter an option with a predicate.
        /// </summary>
        /// <param name="function">The predicate function.</param>
        /// <returns>A new option that is filtered to None if the predicate matches.</returns>
        public Option<T> Where(Func<T, bool> function)
        {
            Contract.AssertNotNull(function);

            if (this.HasValue && !function(this.Value))
            {
                return Option.None<T>();
            }

            return this;
        }

        /// <summary>
        /// Determines if this option is none.
        /// </summary>
        /// <returns>True if the option has no value.</returns>
        public bool IsNone()
        {
            return !this.HasValue;
        }

        /// <summary>
        /// Determines if this option is some.
        /// </summary>
        /// <returns>True if the option has a value.</returns>
        public bool IsSome()
        {
            return this.HasValue;
        }

        /// <summary>
        /// Convert the option to a sequence with zero or one element.
        /// </summary>
        /// <returns>A sequence.</returns>
        public FSeq<T> ToSequence()
        {
            return this.HasValue ? new FSeq<T>().AddFront(this.Value)  : new FSeq<T>();
        }

        /// <summary>
        /// Return the "intersection" of the option with another:
        /// an option with no value if this option has no value,
        /// otherwise the other option.
        /// </summary>
        /// <param name="other">The other option.</param>
        /// <returns>An option.</returns>
        public Option<T> Intersect(Option<T> other)
        {
            return !this.HasValue ? this : other;
        }

        /// <summary>
        /// Return the "union" of the option with another:
        /// this option if it has a value,
        /// otherwise the other option.
        /// </summary>
        /// <param name="other">The other option.</param>
        /// <returns>An option.</returns>
        public Option<T> Union(Option<T> other)
        {
            return this.HasValue ? this : other;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (this.HasValue)
            {
                return $"Some({this.Value})";
            }

            return "None";
        }

        /// <summary>
        /// Equality between option types.
        /// </summary>
        /// <param name="obj">The other option.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is Option<T> o && this.Equals(o);
        }

        /// <summary>
        /// Equality between option types.
        /// </summary>
        /// <param name="other">The other option.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Option<T> other)
        {
            if (!this.HasValue && !other.HasValue)
            {
                return true;
            }

            if (this.HasValue && other.HasValue && this.Value.Equals(other.Value))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the hashcode for the optional value.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            int hashCode = 299404170;
            hashCode = hashCode * -1521134295 + HasValue.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            return hashCode;
        }
    }

    /// <summary>
    /// Builder class for options.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// Return some value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>An option that has some value.</returns>
        public static Option<T> Some<T>(T value)
        {
            return new Option<T>(true, value);
        }

        /// <summary>
        /// Return some value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>An option that has some value.</returns>
        public static Option<T> None<T>()
        {
            return new Option<T>(false, default);
        }

        /// <summary>
        /// The Zen value for null.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Null<T>()
        {
            return Zen.Create<Option<T>>(("HasValue", False()), ("Value", Zen.Default<T>()));
        }

        /// <summary>
        /// The Zen expression for Some value.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Create<T>(Zen<T> expr)
        {
            Contract.AssertNotNull(expr);

            return Zen.Create<Option<T>>(("HasValue", True()), ("Value", expr));
        }

        /// <summary>
        /// The Zen expression for mapping over an option.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T2>> Select<T1, T2>(this Zen<Option<T1>> expr, Func<Zen<T1>, Zen<T2>> function)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(function);

            return If(expr.IsSome(), Option.Create(function(expr.Value())), Option.Null<T2>());
        }

        /// <summary>
        /// The Zen expression for mapping (and projecting) over an option.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <typeparam name="T1">The expression type.</typeparam>
        /// <typeparam name="T2">The function return type.</typeparam>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T2>> SelectMany<T1, T2>(this Zen<Option<T1>> expr,
            Func<Zen<T1>, Zen<Option<T2>>> function)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(function);

            return If(expr.IsSome(), function(expr.Value()), Option.Null<T2>());
        }

        /// <summary>
        /// The Zen expression for filtering over an option.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Where<T>(this Zen<Option<T>> expr, Func<Zen<T>, Zen<bool>> function)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(function);

            return If(Zen.And(expr.IsSome(), function(expr.Value())), expr, Option.Null<T>());
        }

        /// <summary>
        /// The Zen expression for an option or a default if no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="deflt">The default value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> ValueOrDefault<T>(this Zen<Option<T>> expr, Zen<T> deflt)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(deflt);

            return If(expr.IsSome(), expr.Value(), deflt);
        }

        /// <summary>
        /// The Zen expression for an option if it has a value, or the result of a function otherwise.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> SomeOrDefault<T>(this Zen<Option<T>> expr, Func<Zen<Option<T>>> function)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(function);

            return If(expr.IsSome(), expr, function());
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsSome<T>(this Zen<Option<T>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Option<T>, bool>("HasValue");
        }

        /// <summary>
        /// The Zen expression for whether an option has no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsNone<T>(this Zen<Option<T>> expr)
        {
            Contract.AssertNotNull(expr);

            return Not(expr.IsSome());
        }

        /// <summary>
        /// The Zen expression for representing the option as a finite sequence.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> ToFSeq<T>(this Zen<Option<T>> expr)
        {
            Contract.AssertNotNull(expr);

            var l = FSeq.Empty<T>();
            return If(expr.IsSome(), l.AddFront(expr.Value()), l);
        }

        /// <summary>
        /// The Zen expression for an "intersection" of two options.
        /// None if the first option is None, otherwise the second value.
        /// </summary>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Zen<Option<T>> Intersect<T>(this Zen<Option<T>> expr1, Zen<Option<T>> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1.IsNone(), expr1, expr2);
        }

        /// <summary>
        /// The Zen expression for a "union" of two options.
        /// The first option if it is Some, otherwise the second value.
        /// </summary>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Zen<Option<T>> Union<T>(this Zen<Option<T>> expr1, Zen<Option<T>> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1.IsSome(), expr1, expr2);
        }
    }
}
