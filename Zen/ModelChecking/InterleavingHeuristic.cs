// <copyright file="InterleavingHeuristic.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class to conservatively estimate which variables
    /// must be interleaved to avoid exponential blowup in the encoding.
    /// </summary>
    internal sealed class InterleavingHeuristic : IZenExprVisitor<Unit, ImmutableHashSet<object>>
    {
        /// <summary>
        /// Disjoint sets data structure.
        /// </summary>
        public Dictionary<object, ImmutableHashSet<object>> DisjointSets { get; } = new Dictionary<object, ImmutableHashSet<object>>();

        private ImmutableHashSet<object> emptySet = ImmutableHashSet<object>.Empty;

        private Dictionary<object, ImmutableHashSet<object>> cache = new Dictionary<object, ImmutableHashSet<object>>();

        /// <summary>
        /// Lookup an existing cached value or compute it and cache the result.
        /// </summary>
        /// <param name="obj">The expression object.</param>
        /// <param name="callback">The callback to compute the result.</param>
        /// <returns>The result of the computation.</returns>
        private ImmutableHashSet<object> LookupOrCompute(object obj, Func<ImmutableHashSet<object>> callback)
        {
            if (this.cache.TryGetValue(obj, out var value))
            {
                return value;
            }

            var result = callback();
            this.cache[obj] = result;
            return result;
        }

        /// <summary>
        /// Compute the interleaving heurstics.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Groups of variables that must be interleaved.</returns>
        public Dictionary<object, ImmutableHashSet<object>> Compute<T>(Zen<T> expr)
        {
            var _ = expr.Accept(this, new Unit());
            return this.DisjointSets;
        }

        /// <summary>
        /// Combine two sets by merging them together.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        private void Combine(ImmutableHashSet<object> set1, ImmutableHashSet<object> set2)
        {
            var all = emptySet;
            var allAffected = new HashSet<object>();

            foreach (var s in set1)
            {
                all = all.Union(this.DisjointSets[s]);
                allAffected.Add(s);
            }

            foreach (var s in set2)
            {
                all = all.Union(this.DisjointSets[s]);
                allAffected.Add(s);
            }

            foreach (var obj in allAffected)
            {
                this.DisjointSets[obj] = all;
            }
        }

        public ImmutableHashSet<object> VisitZenAdapterExpr<T1, T2>(ZenAdapterExpr<T1, T2> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenAndExpr(ZenAndExpr expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenOrExpr(ZenOrExpr expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenMaxExpr<T>(ZenMaxExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenMinExpr<T>(ZenMinExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenNotExpr(ZenNotExpr expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenIfExpr<T>(ZenIfExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var g = expression.GuardExpr.Accept(this, parameter);
                var t = expression.TrueExpr.Accept(this, parameter);
                var f = expression.FalseExpr.Accept(this, parameter);
                return g.Union(t).Union(f);
            });
        }

        public ImmutableHashSet<object> VisitZenEqExpr<T>(ZenEqExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                this.Combine(x, y);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenConstantByteExpr(ZenConstantByteExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenConstantShortExpr(ZenConstantShortExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenConstantUintExpr(ZenConstantUintExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenConstantIntExpr(ZenConstantIntExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenConstantLongExpr(ZenConstantLongExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenSumExpr<T>(ZenSumExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                this.Combine(x, y);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenMultiplyExpr<T>(ZenMultiplyExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                this.Combine(x, y);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenMinusExpr<T>(ZenMinusExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                this.Combine(x, y);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenBitwiseAndExpr<T>(ZenBitwiseAndExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenBitwiseOrExpr<T>(ZenBitwiseOrExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenBitwiseXorExpr<T>(ZenBitwiseXorExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Element.Accept(this, parameter);
                var y = expression.Expr.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenListMatchExpr<TList, TResult>(ZenListMatchExpr<TList, TResult> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.ListExpr.Accept(this, parameter);
                var e = expression.EmptyCase.Accept(this, parameter);
                return x.Union(e); // no easy way to evaluate cons case.
            });
        }

        public ImmutableHashSet<object> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr.Accept(this, parameter);
                var y = expression.FieldValue.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1>(ZenCreateObjectExpr<TObject, T1> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.FieldValue1.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1, T2>(ZenCreateObjectExpr<TObject, T1, T2> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var a = expression.FieldValue1.Accept(this, parameter);
                var b = expression.FieldValue2.Accept(this, parameter);
                return a.Union(b);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1, T2, T3>(ZenCreateObjectExpr<TObject, T1, T2, T3> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var a = expression.FieldValue1.Accept(this, parameter);
                var b = expression.FieldValue2.Accept(this, parameter);
                var c = expression.FieldValue3.Accept(this, parameter);
                return a.Union(b).Union(c);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1, T2, T3, T4>(ZenCreateObjectExpr<TObject, T1, T2, T3, T4> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var a = expression.FieldValue1.Accept(this, parameter);
                var b = expression.FieldValue2.Accept(this, parameter);
                var c = expression.FieldValue3.Accept(this, parameter);
                var d = expression.FieldValue4.Accept(this, parameter);
                return a.Union(b).Union(c).Union(d);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5>(ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var a = expression.FieldValue1.Accept(this, parameter);
                var b = expression.FieldValue2.Accept(this, parameter);
                var c = expression.FieldValue3.Accept(this, parameter);
                var d = expression.FieldValue4.Accept(this, parameter);
                var e = expression.FieldValue5.Accept(this, parameter);
                return a.Union(b).Union(c).Union(d).Union(e);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6>(ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var a = expression.FieldValue1.Accept(this, parameter);
                var b = expression.FieldValue2.Accept(this, parameter);
                var c = expression.FieldValue3.Accept(this, parameter);
                var d = expression.FieldValue4.Accept(this, parameter);
                var e = expression.FieldValue5.Accept(this, parameter);
                var f = expression.FieldValue6.Accept(this, parameter);
                return a.Union(b).Union(c).Union(d).Union(e).Union(f);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7>(ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var a = expression.FieldValue1.Accept(this, parameter);
                var b = expression.FieldValue2.Accept(this, parameter);
                var c = expression.FieldValue3.Accept(this, parameter);
                var d = expression.FieldValue4.Accept(this, parameter);
                var e = expression.FieldValue5.Accept(this, parameter);
                var f = expression.FieldValue6.Accept(this, parameter);
                var g = expression.FieldValue7.Accept(this, parameter);
                return a.Union(b).Union(c).Union(d).Union(e).Union(f).Union(g);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7, T8>(ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7, T8> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var a = expression.FieldValue1.Accept(this, parameter);
                var b = expression.FieldValue2.Accept(this, parameter);
                var c = expression.FieldValue3.Accept(this, parameter);
                var d = expression.FieldValue4.Accept(this, parameter);
                var e = expression.FieldValue5.Accept(this, parameter);
                var f = expression.FieldValue6.Accept(this, parameter);
                var g = expression.FieldValue7.Accept(this, parameter);
                var h = expression.FieldValue8.Accept(this, parameter);
                return a.Union(b).Union(c).Union(d).Union(e).Union(f).Union(g).Union(h);
            });
        }

        public ImmutableHashSet<object> VisitZenLeqExpr<T>(ZenLeqExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                this.Combine(x, y);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenGeqExpr<T>(ZenGeqExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                this.Combine(x, y);
                return x.Union(y);
            });
        }

        [ExcludeFromCodeCoverage]
        public ImmutableHashSet<object> VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, Unit parameter)
        {
            throw new UnreachableException();
        }

        public ImmutableHashSet<object> VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var result = emptySet.Add(expression);
                if (!this.DisjointSets.ContainsKey(expression))
                {
                    this.DisjointSets[expression] = result;
                }

                return result;
            });
        }
    }
}
