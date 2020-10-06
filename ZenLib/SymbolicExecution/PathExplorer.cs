// <copyright file="PathExplorer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Class to walk through all possible paths in the expression and
    /// enumerate constraints along the paths.
    /// </summary>
    internal sealed class PathExplorer : IZenExprVisitor<PathConstraint, NestedEnumerable<PathConstraint>>
    {
        /// <summary>
        /// Function to check feasibility of a (partial) path constraint.
        /// </summary>
        private Func<Zen<bool>, bool> feasibilityCheck;

        private Dictionary<Zen<bool>, bool> feasibilityCheckCache;

        public PathExplorer(Func<Zen<bool>, bool> feasibilityCheck)
        {
            this.feasibilityCheckCache = new Dictionary<Zen<bool>, bool>();
            this.feasibilityCheck = feasibilityCheck;
        }

        private bool IsFeasible(Zen<bool> pc)
        {
            if (this.feasibilityCheckCache.TryGetValue(pc, out var result))
            {
                return result;
            }

            var ret = this.feasibilityCheck(pc);
            this.feasibilityCheckCache[pc] = ret;
            return ret;
        }

        public NestedEnumerable<PathConstraint> VisitZenAdapterExpr<T1, T2>(ZenAdapterExpr<T1, T2> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenAndExpr(ZenAndExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        [ExcludeFromCodeCoverage]
        public NestedEnumerable<PathConstraint> VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, PathConstraint parameter)
        {
            throw new NotImplementedException();
        }

        public NestedEnumerable<PathConstraint> VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenComparisonExpr<T>(ZenComparisonExpr<T> expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConcatExpr(ZenConcatExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, PathConstraint parameter)
        {
            var result = new NestedEnumerable<PathConstraint>();

            if (expression.Fields.Count == 0)
            {
                return result;
            }

            var zenObjs = expression.Fields.Select(f => (dynamic)f.Value);

            var enumerable = zenObjs.First().Accept(this, parameter);
            var rest = zenObjs.Skip(1);

            foreach (var zenObj in rest)
            {
                enumerable = Compose2(enumerable, zenObj);
            }

            result.AddNested(enumerable);
            return result;
        }

        [ExcludeFromCodeCoverage]
        public NestedEnumerable<PathConstraint> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenIfExpr<T>(ZenIfExpr<T> expression, PathConstraint parameter)
        {
            return Fork(expression.GuardExpr, expression.TrueExpr, expression.FalseExpr, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr, expression.Element, parameter);
        }

        [ExcludeFromCodeCoverage]
        public NestedEnumerable<PathConstraint> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, PathConstraint parameter)
        {
            throw new NotImplementedException();
        }

        public NestedEnumerable<PathConstraint> VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenNotExpr(ZenNotExpr expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenOrExpr(ZenOrExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenStringAtExpr(ZenStringAtExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.StringExpr, expression.IndexExpr, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.StringExpr, expression.SubstringExpr, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, PathConstraint parameter)
        {
            return Compose3(expression.StringExpr, expression.SubstringExpr, expression.OffsetExpr, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenStringLengthExpr(ZenStringLengthExpr expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, PathConstraint parameter)
        {
            return Compose3(expression.StringExpr, expression.SubstringExpr, expression.ReplaceExpr, parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, PathConstraint parameter)
        {
            return Compose3(expression.StringExpr, expression.OffsetExpr, expression.LengthExpr, parameter);
        }

        [ExcludeFromCodeCoverage]
        public NestedEnumerable<PathConstraint> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        private NestedEnumerable<PathConstraint> Compose3<T1, T2, T3>(Zen<T1> e1, Zen<T2> e2, Zen<T3> e3, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(Compose3Internal(e1.Accept(this, parameter), e2, e3));
        }

        private IEnumerable<PathConstraint> Compose3Internal<T1, T2>(NestedEnumerable<PathConstraint> enumerable, Zen<T1> e1, Zen<T2> e2)
        {
            foreach (var p1 in enumerable)
            {
                if (!IsFeasible(p1.Expr))
                {
                    continue;
                }

                foreach (var p2 in e1.Accept(this, p1))
                {
                    foreach (var p3 in e2.Accept(this, p2))
                    {
                        yield return p3;
                    }
                }
            }
        }

        private NestedEnumerable<PathConstraint> Compose2<T1, T2>(Zen<T1> e1, Zen<T2> e2, PathConstraint parameter)
        {
            return Compose2(e1.Accept(this, parameter), e2);
        }

        private NestedEnumerable<PathConstraint> Compose2<T>(NestedEnumerable<PathConstraint> enumerable, Zen<T> e)
        {
            return new NestedEnumerable<PathConstraint>(Compose2Internal(enumerable, e));
        }

        private IEnumerable<PathConstraint> Compose2Internal<T>(NestedEnumerable<PathConstraint> enumerable, Zen<T> e)
        {
            foreach (var p1 in enumerable)
            {
                if (!IsFeasible(p1.Expr))
                {
                    continue;
                }

                foreach (var p2 in e.Accept(this, p1))
                {
                    yield return p2;
                }
            }
        }

        private NestedEnumerable<PathConstraint> Fork<T>(
            Zen<bool> guardExpr,
            Zen<T> trueExpr,
            Zen<T> falseExpr,
            PathConstraint parameter)
        {
            var result = new NestedEnumerable<PathConstraint>();

            var guardEnumerable = guardExpr.Accept(this, parameter);

            foreach (var g in guardEnumerable)
            {
                Console.WriteLine($"fork guard: {g.Expr}");
            }

            var notGuardExpr = ZenNotExpr.Create(guardExpr);

            var tEnumerable = new NestedEnumerable<PathConstraint>(
                guardEnumerable.Select(pc => pc.AddPathConstraint(guardExpr)));

            var fEnumerable = new NestedEnumerable<PathConstraint>(
                guardEnumerable.Select(pc => pc.AddPathConstraint(notGuardExpr)));

            result.AddNested(Compose2(fEnumerable, falseExpr));
            result.AddNested(Compose2(tEnumerable, trueExpr));

            return result;
        }
    }
}