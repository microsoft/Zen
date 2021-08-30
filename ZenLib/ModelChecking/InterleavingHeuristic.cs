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
    internal sealed class InterleavingHeuristic : IZenExprVisitor<Dictionary<long, object>, ImmutableHashSet<object>>
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

        public Dictionary<object, ImmutableHashSet<object>> Compute<T>(Zen<T> expr, Dictionary<long, object> arguments)
        {
            var _ = expr.Accept(this, arguments);
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

            var all = set1.Union(set2);
            int previousCount;

            do
            {
                previousCount = all.Count;
                var newAll = all;
                foreach (var element in all)
                {
                    newAll = newAll.Union(this.DisjointSets[element]);
                }

                all = newAll;
            } while (all.Count > previousCount);

            foreach (var element in all)
            {
                this.DisjointSets[element] = all;
            }
        }

        public ImmutableHashSet<object> VisitZenAndExpr(ZenAndExpr expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenOrExpr(ZenOrExpr expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr1.Accept(this, parameter);
                var y = expression.Expr2.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenNotExpr(ZenNotExpr expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenIfExpr<T>(ZenIfExpr<T> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var g = expression.GuardExpr.Accept(this, parameter);
                var t = expression.TrueExpr.Accept(this, parameter);
                var f = expression.FalseExpr.Accept(this, parameter);
                return g.Union(t).Union(f);
            });
        }

        public ImmutableHashSet<object> VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, Dictionary<long, object> parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, Dictionary<long, object> parameter)
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
                    default:
                        return x.Union(y);
                }
            });
        }

        public ImmutableHashSet<object> VisitZenConcatExpr(ZenConcatExpr expression, Dictionary<long, object> parameter)
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
        public ImmutableHashSet<object> VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public ImmutableHashSet<object> VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public ImmutableHashSet<object> VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public ImmutableHashSet<object> VisitZenStringAtExpr(ZenStringAtExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public ImmutableHashSet<object> VisitZenStringLengthExpr(ZenStringLengthExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public ImmutableHashSet<object> VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        public ImmutableHashSet<object> VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, Dictionary<long, object> parameter)
        {
            return emptySet;
        }

        public ImmutableHashSet<object> VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Element.Accept(this, parameter);
                var y = expression.Expr.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.ListExpr.Accept(this, parameter);
                var e = expression.EmptyCase.Accept(this, parameter);
                return x.Union(e); // no easy way to evaluate cons case.
            });
        }

        public ImmutableHashSet<object> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return expression.Expr.Accept(this, parameter);
            });
        }

        public ImmutableHashSet<object> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var x = expression.Expr.Accept(this, parameter);
                var y = expression.FieldValue.Accept(this, parameter);
                return x.Union(y);
            });
        }

        public ImmutableHashSet<object> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var set = ImmutableHashSet<object>.Empty;
                foreach (var value in expression.Fields.Values)
                {
                    var valueResult = CommonUtilities.RunAndPreserveExceptions(() =>
                    {
                        var valueType = value.GetType();
                        var acceptMethod = valueType
                            .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                            .MakeGenericMethod(typeof(Dictionary<long, object>), typeof(ImmutableHashSet<object>));
                        return (ImmutableHashSet<object>)acceptMethod.Invoke(value, new object[] { this, parameter });
                    });

                    set = set.Union(valueResult);
                }

                return set;
            });
        }

        public ImmutableHashSet<object> VisitZenComparisonExpr<T>(ZenComparisonExpr<T> expression, Dictionary<long, object> parameter)
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
        public ImmutableHashSet<object> VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, Dictionary<long, object> parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return CommonUtilities.RunAndPreserveExceptions(() =>
                {
                    var expr = parameter[expression.ArgumentId];
                    var acceptMethod = expr.GetType()
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(Dictionary<long, object>), typeof(ImmutableHashSet<object>));
                    return (ImmutableHashSet<object>)acceptMethod.Invoke(expr, new object[] { this, parameter });
                });
            });
        }

        public ImmutableHashSet<object> VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, Dictionary<long, object> parameter)
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
