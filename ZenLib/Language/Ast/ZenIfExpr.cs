// <copyright file="ZenIfExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an If expression.
    /// </summary>
    internal sealed class ZenIfExpr<T> : Zen<T>
    {
        /// <summary>
        /// Hash cons table for ZenIfExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), Zen<T>> hashConsTable = new HashConsTable<(long, long, long), Zen<T>>();

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
        /// Simplify and create a new ZenIfExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<T> Simplify((Zen<bool> g, Zen<T> t, Zen<T> f) args)
        {
            // if true then e1 else e2 = e1
            // if false then e1 else e2 = e2
            if (args.g is ZenConstantExpr<bool> ce)
            {
                return ce.Value ? args.t : args.f;
            }

            // if not e then x else y = if e then y else x
            if (args.g is ZenNotExpr ne)
            {
                return new ZenIfExpr<T>(ne.Expr, args.f, args.t);
            }

            if (!ZenSettings.PreserveBranches)
            {
                // if g then e else e = e
                if (ReferenceEquals(args.t, args.f))
                {
                    return args.t;
                }

                if (typeof(T) == ReflectionUtilities.BoolType)
                {
                    // if e1 then true else e2 = Or(e1, e2)
                    if (args.t is ZenConstantExpr<bool> te && te.Value)
                    {
                        return ZenLogicalBinopExpr.Create((dynamic)args.g, (dynamic)args.f, ZenLogicalBinopExpr.LogicalOp.Or);
                    }

                    // if e1 then e2 else false = And(e1, e2)
                    if (args.f is ZenConstantExpr<bool> fe && !fe.Value)
                    {
                        return ZenLogicalBinopExpr.Create((dynamic)args.g, (dynamic)args.t, ZenLogicalBinopExpr.LogicalOp.And);
                    }
                }
            }

            return new ZenIfExpr<T>(args.g, args.t, args.f);
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
            Contract.AssertNotNull(guardExpr);
            Contract.AssertNotNull(trueExpr);
            Contract.AssertNotNull(falseExpr);

            var key = (guardExpr.Id, trueExpr.Id, falseExpr.Id);
            hashConsTable.GetOrAdd(key, (guardExpr, trueExpr, falseExpr), Simplify, out var value);
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitIf(this, parameter);
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
}
