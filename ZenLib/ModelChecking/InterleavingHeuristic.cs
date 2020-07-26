// <copyright file="InterleavingHeuristic.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class to conservatively estimate which variables
    /// must be interleaved to avoid exponential blowup in the encoding.
    /// </summary>
    internal sealed class InterleavingHeuristic : IZenExprVisitor<Unit, ImmutableHashSet<object>>
    {
        private static Type ZenBoolType = typeof(Zen<bool>);

        public Dictionary<object, ImmutableHashSet<object>> DisjointSets { get; } =
            new Dictionary<object, ImmutableHashSet<object>>();

        private ImmutableHashSet<object> emptySet = ImmutableHashSet<object>.Empty;

        private Dictionary<object, ImmutableHashSet<object>> cache =
            new Dictionary<object, ImmutableHashSet<object>>();

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

        public Dictionary<object, ImmutableHashSet<object>> Compute<T>(Zen<T> expr)
        {
            var _ = expr.Accept(this, new Unit());
            return this.DisjointSets;
        }

        private bool IsSafeSet(ImmutableHashSet<object> set)
        {
            return set.All(x => ZenBoolType.IsAssignableFrom(x.GetType()));
        }

        private void Combine(ImmutableHashSet<object> set1, ImmutableHashSet<object> set2)
        {
            // don't need to combine when one side is only a boolean
            if (IsSafeSet(set1) || IsSafeSet(set2))
            {
                return;
            }

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

        public ImmutableHashSet<object> VisitZenConstantStringExpr(ZenConstantStringExpr expression, Unit parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);

                switch (expression.Operation)
                {
                    case Op.Addition:
                    case Op.Multiplication:
                    case Op.Subtraction:
                    case Op.BitwiseAnd:
                    case Op.BitwiseXor:
                        this.Combine(x, y);
                        return x.Union(y);
                    case Op.BitwiseOr:
                        return x.Union(y);
                    default:
                        throw new ZenException($"Invalid operation: {expression.Operation}");
                }
            });
        }

        public ImmutableHashSet<object> VisitZenConcatExpr(ZenConcatExpr expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                this.Combine(x, y);
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

        public ImmutableHashSet<object> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Unit parameter)
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

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, Unit parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var set = ImmutableHashSet<object>.Empty;
                foreach (var value in expression.Fields.Values)
                {
                    var valueType = value.GetType();
                    var acceptMethod = valueType
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(Unit), typeof(ImmutableHashSet<object>));
                    var valueResult = (ImmutableHashSet<object>)acceptMethod.Invoke(value, new object[] { this, parameter });

                    set = set.Union(valueResult);
                }

                return set;
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
