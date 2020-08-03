// <copyright file="ZenIfExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an If expression.
    /// </summary>
    internal sealed class ZenIfExpr<T> : Zen<T>
    {
        private static Dictionary<(object, object, object), Zen<T>> hashConsTable =
            new Dictionary<(object, object, object), Zen<T>>();

        private static Zen<T> Simplify(Zen<bool> g, Zen<T> t, Zen<T> f)
        {
            // if true then e1 else e2 = e1
            // if false then e1 else e2 = e2
            if (g is ZenConstantBoolExpr ce)
            {
                return ce.Value ? t : f;
            }

            // if g then e else e = e
            if (ReferenceEquals(t, f))
            {
                return t;
            }

            if (typeof(T) == ReflectionUtilities.BoolType)
            {
                // if e1 then true else e2 = Or(e1, e2)
                // if e1 then false else e2 = And(Not(e1), e2)
                if (t is ZenConstantBoolExpr te)
                {
                    return te.Value ?
                        ZenOrExpr.Create((dynamic)g, (dynamic)f) :
                        ZenAndExpr.Create(ZenNotExpr.Create((dynamic)g), (dynamic)f);
                }

                // if e1 then e2 else true = Or(Not(e1), e2)
                // if e1 then e2 else false = And(e1, e2)
                if (f is ZenConstantBoolExpr fe)
                {
                    return fe.Value ?
                        ZenOrExpr.Create(ZenNotExpr.Create((dynamic)g), (dynamic)t) :
                        ZenAndExpr.Create((dynamic)g, (dynamic)t);
                }
            }

            return new ZenIfExpr<T>(g, t, f);
        }

        public static Zen<T> Create(Zen<bool> guardExpr, Zen<T> trueExpr, Zen<T> falseExpr)
        {
            CommonUtilities.ValidateNotNull(guardExpr);
            CommonUtilities.ValidateNotNull(trueExpr);
            CommonUtilities.ValidateNotNull(falseExpr);

            var key = (guardExpr, trueExpr, falseExpr);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(guardExpr, trueExpr, falseExpr);
            hashConsTable[key] = ret;
            return ret;
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
            return $"if({this.GuardExpr}, {this.TrueExpr}, {this.FalseExpr})";
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
            return visitor.VisitZenIfExpr(this, parameter);
        }
    }
}
