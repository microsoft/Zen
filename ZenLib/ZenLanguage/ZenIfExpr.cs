// <copyright file="ZenIfExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an If expression.
    /// </summary>
    internal sealed class ZenIfExpr<T> : Zen<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<bool>, Zen<T>, Zen<T>), Zen<T>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenIfExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), Zen<T>> hashConsTable = new HashConsTable<(long, long, long), Zen<T>>();

        /// <summary>
        /// Unroll the ZenIfExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<T> Unroll()
        {
            return Create(this.GuardExpr.Unroll(), this.TrueExpr.Unroll(), this.FalseExpr.Unroll());
        }

        /// <summary>
        /// Simplify and create a new ZenIfExpr.
        /// </summary>
        /// <param name="g">The guard expr.</param>
        /// <param name="t">The true expr.</param>
        /// <param name="f">The false expr.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<T> Simplify(Zen<bool> g, Zen<T> t, Zen<T> f)
        {
            // if true then e1 else e2 = e1
            // if false then e1 else e2 = e2
            if (g is ZenConstantExpr<bool> ce)
            {
                return ce.Value ? t : f;
            }

            if (!Settings.PreserveBranches)
            {
                // if g then e else e = e
                if (ReferenceEquals(t, f))
                {
                    return t;
                }

                if (typeof(T) == ReflectionUtilities.BoolType)
                {
                    // if e1 then true else e2 = Or(e1, e2)
                    if (t is ZenConstantExpr<bool> te && te.Value)
                    {
                        return ZenLogicalBinopExpr.Create((dynamic)g, (dynamic)f, ZenLogicalBinopExpr.LogicalOp.Or);
                    }

                    // if e1 then e2 else false = And(e1, e2)
                    if (f is ZenConstantExpr<bool> fe && !fe.Value)
                    {
                        return ZenLogicalBinopExpr.Create((dynamic)g, (dynamic)t, ZenLogicalBinopExpr.LogicalOp.And);
                    }
                }
            }

            return new ZenIfExpr<T>(g, t, f);
        }

        /// <summary>
        /// Create a new ZenIfExpr.
        /// </summary>
        /// <param name="guardExpr">The guard expr.</param>
        /// <param name="trueExpr">The true expr.</param>
        /// <param name="falseExpr">The false expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<T> Create(Zen<bool> guardExpr, Zen<T> trueExpr, Zen<T> falseExpr)
        {
            CommonUtilities.ValidateNotNull(guardExpr);
            CommonUtilities.ValidateNotNull(trueExpr);
            CommonUtilities.ValidateNotNull(falseExpr);

            var key = (guardExpr.Id, trueExpr.Id, falseExpr.Id);
            hashConsTable.GetOrAdd(key, (guardExpr, trueExpr, falseExpr), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenIfExpr{T}"/> class.
        /// </summary>
        /// <param name="guardExpr">The guard expression.</param>
        /// <param name="trueExpr">The true expression.</param>
        /// <param name="falseExpr">The false expression.</param>
        private ZenIfExpr(Zen<bool> guardExpr, Zen<T> trueExpr, Zen<T> falseExpr)
        {
            this.GuardExpr = guardExpr;
            this.TrueExpr = trueExpr;
            this.FalseExpr = falseExpr;
        }

        /// <summary>
        /// Gets the guard expression.
        /// </summary>
        internal Zen<bool> GuardExpr { get; }

        /// <summary>
        /// Gets the true expression.
        /// </summary>
        internal Zen<T> TrueExpr { get; }

        /// <summary>
        /// Gets the false expression.
        /// </summary>
        internal Zen<T> FalseExpr { get; }

        /// <summary>
        /// Convert the condition to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"If({this.GuardExpr}, {this.TrueExpr}, {this.FalseExpr})";
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The visitor parameter.</param>
        /// <typeparam name="TParam">The visitor parameter type.</typeparam>
        /// <typeparam name="TReturn">The visitor return type.</typeparam>
        /// <returns>A return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.Visit(this, parameter);
        }
    }
}
