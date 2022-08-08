// <copyright file="ZenArgumentExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// A function argument placeholder expression..
    /// </summary>
    /// <typeparam name="T">Type of an underlying C# value.</typeparam>
    internal sealed class ZenArgumentExpr<T> : Zen<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZenArgumentExpr{T}"/> class.
        /// </summary>
        public ZenArgumentExpr()
        {
            this.ArgumentId = Interlocked.Increment(ref ZenArgumentId.nextId);
        }

        /// <summary>
        /// Unroll this expression.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<T> Unroll()
        {
            return this;
        }

        /// <summary>
        /// Gets the unique id for the object.
        /// </summary>
        public long ArgumentId { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Arg({this.ArgumentId})";
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The visitor parameter.</param>
        /// <typeparam name="TParam">The visitor parameter type.</typeparam>
        /// <typeparam name="TReturn">The visitor return type.</typeparam>
        /// <returns>A return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitArgument(this, parameter);
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        internal override void Accept(ZenExprActionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Placeholder for the next unique id for an argument expression.
    /// This is kept outside the class to avoid having a separate id
    /// for each instantiation of the generic type T.
    /// </summary>
    internal static class ZenArgumentId
    {
        internal static long nextId;
    }
}
