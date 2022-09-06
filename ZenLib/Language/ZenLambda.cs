// <copyright file="ZenLambda.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A Zen lambda function.
    /// </summary>
    public sealed class ZenLambda<TSrc, TDst>
    {
        /// <summary>
        /// The C# function.
        /// </summary>
        internal Func<Zen<TSrc>, Zen<TDst>> Function { get; private set; }

        /// <summary>
        /// The argument expression.
        /// </summary>
        internal ZenParameterExpr<TSrc> Parameter { get; private set; }

        /// <summary>
        /// The body expression.
        /// </summary>
        internal Zen<TDst> Body { get; private set; }

        /// <summary>
        /// Initializes the lambda.
        /// </summary>
        public void Initialize(Func<Zen<TSrc>, Zen<TDst>> function)
        {
            this.Function = function;
            this.Parameter = new ZenParameterExpr<TSrc>();
            this.Body = function(this.Parameter);
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"lambda({this.Parameter} => {this.Body})";
        }
    }
}
