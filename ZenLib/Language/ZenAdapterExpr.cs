// <copyright file="ZenAdapterExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Expression that allows for converting between types.
    /// </summary>
    internal sealed class ZenAdapterExpr<TTo, TFrom> : Zen<TTo>
    {
        private static Dictionary<object, Zen<TTo>> hashConsTable = new Dictionary<object, Zen<TTo>>();

        private static Zen<TTo> Simplify(Zen<TFrom> e, ImmutableList<Func<object, object>> converters)
        {
            // adapt(t1, t2, adapt(t2, t1, e)) == e
            if (e is ZenAdapterExpr<TFrom, TTo> inner1)
            {
                return inner1.Expr;
            }

            // adapt(t1, t2, adapt(t2, t3, e)) == adapt(t1, t3, e)
            /* var type = e.GetType();
            if (type.GetGenericTypeDefinition() == typeof(ZenAdapterExpr<,>))
            {
                if (type.GetGenericArguments()[0] == typeof(T2))
                {
                    var type1 = typeof(T1);
                    var type3 = type.GetGenericArguments()[1];
                    var astType = typeof(ZenAdapterExpr<,>).MakeGenericType(type1, type3);
                    var paramType1 = typeof(Zen<>).MakeGenericType(type3);
                    var paramType2 = typeof(ImmutableList<>).MakeGenericType(typeof(Func<object, object>));
                    var method = astType.GetMethod("CreateMulti");
                    var param1 = type.GetProperty("Expr").GetValue(e);
                    var converters = (ImmutableList<Func<object, object>>)type.GetProperty("Converters").GetValue(e);
                    var param2 = converters.AddRange(expression.Converters);
                    return (Zen<T1>)method.Invoke(null, new object[] { param1, param2 });
                }
            } */
            if (Settings.SimplifyRecursive)
            {
                if (e is ZenIfExpr<TFrom> inner2)
                {
                    var trueBranch = ZenAdapterExpr<TTo, TFrom>.CreateMulti(inner2.TrueExpr, converters);
                    var falseBranch = ZenAdapterExpr<TTo, TFrom>.CreateMulti(inner2.FalseExpr, converters);
                    return ZenIfExpr<TTo>.Create(inner2.GuardExpr, trueBranch, falseBranch);
                }
            }

            return new ZenAdapterExpr<TTo, TFrom>(e, converters);
        }

        public static Zen<TTo> Create(Zen<TFrom> expr, Func<object, object> converter)
        {
            var converters = ImmutableList<Func<object, object>>.Empty.Add(converter);
            return CreateMulti(expr, converters);
        }

        public static Zen<TTo> CreateMulti(Zen<TFrom> expr, ImmutableList<Func<object, object>> converters)
        {
            CommonUtilities.Validate(expr);
            var key = (expr, converters);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr, converters);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAdapterExpr{TTo, TFrom}"/> class.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="converters">Converter between types.</param>
        private ZenAdapterExpr(Zen<TFrom> expr, ImmutableList<Func<object, object>> converters)
        {
            this.Expr = expr;
            this.Converters = converters;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Zen<TFrom> Expr { get; }

        /// <summary>
        /// Gets the converter.
        /// </summary>
        public ImmutableList<Func<object, object>> Converters { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"adapter({this.Expr})";
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
            return visitor.VisitZenAdapterExpr(this, parameter);
        }
    }
}
