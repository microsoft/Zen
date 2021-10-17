﻿// <copyright file="InterleavingHeuristic.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class to conservatively estimate which variables
    /// must be interleaved to avoid exponential blowup in the encoding.
    /// </summary>
    internal sealed class InterleavingHeuristic : IZenExprVisitor<Dictionary<long, object>, InterleavingResult>
    {
        /// <summary>
        /// The disjoint sets of variables that must be interleaved.
        /// Should replace this with a union find at some point.
        /// </summary>
        public Dictionary<object, ImmutableHashSet<object>> DisjointSets { get; } = new Dictionary<object, ImmutableHashSet<object>>();

        /// <summary>
        /// An empty set of variables.
        /// </summary>
        private ImmutableHashSet<object> emptyVariableSet = ImmutableHashSet<object>.Empty;

        /// <summary>
        /// Expression cache to avoid redundant work.
        /// </summary>
        private Dictionary<object, InterleavingResult> cache = new Dictionary<object, InterleavingResult>();

        public Dictionary<object, ImmutableHashSet<object>> Compute<T>(Zen<T> expr, Dictionary<long, object> arguments)
        {
            var _ = expr.Accept(this, arguments);
            return this.DisjointSets;
        }

        public InterleavingResult VisitZenAndExpr(ZenAndExpr expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = expression.Expr1.Accept(this, parameter);
            var y = expression.Expr2.Accept(this, parameter);
            var result = x.Union(y);

            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenOrExpr(ZenOrExpr expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = expression.Expr1.Accept(this, parameter);
            var y = expression.Expr2.Accept(this, parameter);
            var result = x.Union(y);

            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenNotExpr(ZenNotExpr expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Expr.Accept(this, parameter);
            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenIfExpr<T>(ZenIfExpr<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            expression.GuardExpr.Accept(this, parameter);
            var t = expression.TrueExpr.Accept(this, parameter);
            var f = expression.FalseExpr.Accept(this, parameter);
            var result = t.Union(f);
            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, Dictionary<long, object> parameter)
        {
            return new InterleavingSet(emptyVariableSet);
        }

        public InterleavingResult VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

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
                    var result1 = x.Union(y);
                    this.cache[expression] = result1;
                    return result1;
                default:
                    var result2 = x.Union(y);
                    this.cache[expression] = result2;
                    return result2;
            }
        }

        public InterleavingResult VisitZenConcatExpr(ZenConcatExpr expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = expression.Expr1.Accept(this, parameter);
            var y = expression.Expr2.Accept(this, parameter);
            this.Combine(x, y);
            var result = x.Union(y);

            this.cache[expression] = result;
            return result;
        }

        [ExcludeFromCodeCoverage]
        public InterleavingResult VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public InterleavingResult VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public InterleavingResult VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public InterleavingResult VisitZenStringAtExpr(ZenStringAtExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public InterleavingResult VisitZenStringLengthExpr(ZenStringLengthExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        [ExcludeFromCodeCoverage]
        public InterleavingResult VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, Dictionary<long, object> parameter)
        {
            throw new ZenException($"Invalid string type used with Decision Diagram backend.");
        }

        public InterleavingResult VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Expr.Accept(this, parameter);

            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, Dictionary<long, object> parameter)
        {
            return new InterleavingSet(emptyVariableSet);
        }

        public InterleavingResult VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = expression.Element.Accept(this, parameter);
            var y = expression.Expr.Accept(this, parameter);
            var result = x.Union(y);

            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = expression.ListExpr.Accept(this, parameter);
            var e = expression.EmptyCase.Accept(this, parameter);
            var result = x.Union(e); // no easy way to evaluate cons case.

            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var result = expression.Expr.Accept(this, parameter);
            if (result is InterleavingClass rc)
            {
                result = rc.Fields[expression.FieldName];
            }

            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = expression.Expr.Accept(this, parameter);
            var y = expression.FieldValue.Accept(this, parameter);

            if (x is InterleavingClass xc)
            {
                x = new InterleavingClass(xc.Fields.SetItem(expression.FieldName, y));
                this.cache[expression] = x;
                return x;
            }

            var result = x.Union(y);
            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var fieldMapping = ImmutableDictionary<string, InterleavingResult>.Empty;
            foreach (var fieldValuePair in expression.Fields)
            {
                InterleavingResult valueResult;
                try
                {
                    var valueType = fieldValuePair.Value.GetType();
                    var acceptMethod = valueType
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(Dictionary<long, object>), typeof(InterleavingResult));
                    valueResult = (InterleavingResult)acceptMethod.Invoke(fieldValuePair.Value, new object[] { this, parameter });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }

                fieldMapping = fieldMapping.Add(fieldValuePair.Key, valueResult);
            }

            var result = new InterleavingClass(fieldMapping);
            this.cache[expression] = result;
            return result;
        }

        public InterleavingResult VisitZenComparisonExpr<T>(ZenComparisonExpr<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var x = expression.Expr1.Accept(this, parameter);
            var y = expression.Expr2.Accept(this, parameter);
            this.Combine(x, y);
            var result = x.Union(y);
            this.cache[expression] = result;
            return result;
        }

        [ExcludeFromCodeCoverage]
        public InterleavingResult VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            try
            {
                var expr = parameter[expression.ArgumentId];
                var acceptMethod = expr.GetType()
                    .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(typeof(Dictionary<long, object>), typeof(InterleavingResult));
                var result = (InterleavingResult)acceptMethod.Invoke(expr, new object[] { this, parameter });
                this.cache[expression] = result;
                return result;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public InterleavingResult VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, Dictionary<long, object> parameter)
        {
            if (this.cache.TryGetValue(expression, out var value))
            {
                return value;
            }

            var variableSet = emptyVariableSet.Add(expression);
            if (!this.DisjointSets.ContainsKey(expression))
            {
                this.DisjointSets[expression] = variableSet;
            }

            var result = new InterleavingSet(variableSet);
            this.cache[expression] = result;
            return result;
        }

        /// <summary>
        /// Determines if a set of variables is comprised only of boolean values, which do not need interleaving.
        /// </summary>
        /// <param name="variableSet">The set of variables.</param>
        /// <returns>True or false.</returns>
        private bool IsBoolVariableSet(ImmutableHashSet<object> variableSet)
        {
            return variableSet.All(x => typeof(Zen<bool>).IsAssignableFrom(x.GetType()));
        }

        /// <summary>
        /// Combines two results to indicate that they depend on each other.
        /// Should replace this with a proper union-find data structure later.
        /// </summary>
        /// <param name="result1">The first result.</param>
        /// <param name="result2">The second result.</param>
        private void Combine(InterleavingResult result1, InterleavingResult result2)
        {
            var variableSet1 = result1.GetAllVariables();
            var variableSet2 = result2.GetAllVariables();

            if (IsBoolVariableSet(variableSet1) || IsBoolVariableSet(variableSet2))
            {
                return;
            }

            var all = variableSet1.Union(variableSet2);
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
    }
}
