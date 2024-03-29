﻿// <copyright file="ZenApplyExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing applying a lambda to a parameter.
    /// </summary>
    internal sealed class ZenApplyExpr<TSrc, TDst> : Zen<TDst>
    {
        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal ZenLambda<TSrc, TDst> Lambda { get; }

        /// <summary>
        /// Gets the paramter expression.
        /// </summary>
        internal Zen<TSrc> ArgumentExpr { get; }

        /// <summary>
        /// Simplify and create a new ZenApplyExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new expr.</returns>
        private static Zen<TDst> Simplify((ZenLambda<TSrc, TDst> lambda, Zen<TSrc> parameterExpr) args)
        {
            return new ZenApplyExpr<TSrc, TDst>(args.lambda, args.parameterExpr);
        }

        /// <summary>
        /// Create a new ZenApplyExpr.
        /// </summary>
        /// <param name="lambda">The lambda expression.</param>
        /// <param name="parameterExpr">The parameter expression.</param>
        /// <returns>The new expr.</returns>
        public static Zen<TDst> Create(ZenLambda<TSrc, TDst> lambda, Zen<TSrc> parameterExpr)
        {
            Contract.AssertNotNull(lambda);
            Contract.AssertNotNull(parameterExpr);

            var key = (lambda, parameterExpr.Id);
            var flyweight = ZenAstCache<ZenApplyExpr<TSrc, TDst>, Zen<TDst>>.Flyweight;
            flyweight.GetOrAdd(key, (lambda, parameterExpr), Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenApplyExpr{TSrc, TDst}"/> class.
        /// </summary>
        /// <param name="lambda">The lambda expression.</param>
        /// <param name="parameterExpr">The parameter expression.</param>
        private ZenApplyExpr(ZenLambda<TSrc, TDst> lambda, Zen<TSrc> parameterExpr)
        {
            this.Lambda = lambda;
            this.ArgumentExpr = parameterExpr;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Apply({this.Lambda.GetHashCode()}:{this.Lambda}, {this.ArgumentExpr})";
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
            return visitor.VisitApply(this, parameter);
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
