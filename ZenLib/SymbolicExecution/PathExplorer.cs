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
    internal sealed class PathExplorer : IZenExprVisitor<PathConstraint, IEnumerable<PathConstraint>>
    {
        public IEnumerable<PathConstraint> VisitZenAdapterExpr<T1, T2>(ZenAdapterExpr<T1, T2> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenAndExpr(ZenAndExpr expression, PathConstraint parameter)
        {
            // e1 && e2 is treated as (if !e1 then false else if !e2 then false else true)
            return Fork2(ZenNotExpr.Create(expression.Expr1), ZenNotExpr.Create(expression.Expr2), ZenConstantBoolExpr.False, ZenConstantBoolExpr.False, ZenConstantBoolExpr.True, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        [ExcludeFromCodeCoverage]
        public IEnumerable<PathConstraint> VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, PathConstraint parameter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PathConstraint> VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenComparisonExpr<T>(ZenComparisonExpr<T> expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenConcatExpr(ZenConcatExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantByteExpr(ZenConstantByteExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantIntExpr(ZenConstantIntExpr expression, PathConstraint parameter)
        {
           yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantLongExpr(ZenConstantLongExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantShortExpr(ZenConstantShortExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantStringExpr(ZenConstantStringExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantUintExpr(ZenConstantUintExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, PathConstraint parameter)
        {
            if (expression.Fields.Count == 0)
            {
                yield break;
            }

            var zenObjs = expression.Fields.Select(f => (dynamic)f.Value);

            var enumerable = zenObjs.First().Accept(this, parameter);
            var rest = zenObjs.Skip(1);

            foreach (var zenObj in rest)
            {
                enumerable = Compose2(enumerable, zenObj, parameter);
            }

            foreach (var pathConstraint in enumerable)
            {
                yield return pathConstraint;
            }
        }

        [ExcludeFromCodeCoverage]
        public IEnumerable<PathConstraint> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenIfExpr<T>(ZenIfExpr<T> expression, PathConstraint parameter)
        {
            return Fork(expression.GuardExpr, expression.TrueExpr, expression.FalseExpr, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr1, expression.Expr2, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, PathConstraint parameter)
        {
            return Compose2(expression.Expr, expression.Element, parameter);
        }

        [ExcludeFromCodeCoverage]
        public IEnumerable<PathConstraint> VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, PathConstraint parameter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PathConstraint> VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, PathConstraint parameter)
        {
            yield return parameter;
        }

        public IEnumerable<PathConstraint> VisitZenNotExpr(ZenNotExpr expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenOrExpr(ZenOrExpr expression, PathConstraint parameter)
        {
            // e1 || e2 is treated as (if e1 then true else if e2 then true else false)
            return Fork2(expression.Expr1, expression.Expr2, ZenConstantBoolExpr.True, ZenConstantBoolExpr.True, ZenConstantBoolExpr.False, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenStringAtExpr(ZenStringAtExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.StringExpr, expression.IndexExpr, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenStringContainmentExpr(ZenStringContainmentExpr expression, PathConstraint parameter)
        {
            return Compose2(expression.StringExpr, expression.SubstringExpr, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenStringIndexOfExpr(ZenStringIndexOfExpr expression, PathConstraint parameter)
        {
            return Compose3(expression.StringExpr, expression.SubstringExpr, expression.OffsetExpr, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenStringLengthExpr(ZenStringLengthExpr expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenStringReplaceExpr(ZenStringReplaceExpr expression, PathConstraint parameter)
        {
            return Compose3(expression.StringExpr, expression.SubstringExpr, expression.ReplaceExpr, parameter);
        }

        public IEnumerable<PathConstraint> VisitZenStringSubstringExpr(ZenStringSubstringExpr expression, PathConstraint parameter)
        {
            return Compose3(expression.StringExpr, expression.OffsetExpr, expression.LengthExpr, parameter);
        }

        [ExcludeFromCodeCoverage]
        public IEnumerable<PathConstraint> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, PathConstraint parameter)
        {
            return expression.Expr.Accept(this, parameter);
        }

        private IEnumerable<PathConstraint> Compose3<T1, T2, T3>(
            Zen<T1> e1,
            Zen<T2> e2,
            Zen<T3> e3,
            PathConstraint parameter)
        {
            foreach (var p1 in e1.Accept(this, parameter))
            {
                var parameter2 = parameter.AddPathConstraint(p1);

                foreach (var p2 in e2.Accept(this, parameter2))
                {
                    var parameter3 = parameter2.AddPathConstraint(p2);

                    foreach (var p3 in e3.Accept(this, parameter3))
                    {
                        yield return p3;
                    }
                }
            }
        }

        private IEnumerable<PathConstraint> Compose2<T1, T2>(
            Zen<T1> e1,
            Zen<T2> e2,
            PathConstraint parameter)
        {
            return Compose2(e1.Accept(this, parameter), e2, parameter);
        }

        private IEnumerable<PathConstraint> Compose2<T>(
            IEnumerable<PathConstraint> enumerable,
            Zen<T> e,
            PathConstraint parameter)
        {
            foreach (var p1 in enumerable)
            {
                foreach (var p2 in e.Accept(this, parameter.AddPathConstraint(p1)))
                {
                    yield return p2;
                }
            }
        }

        private IEnumerable<PathConstraint> Fork<T>(
            Zen<bool> guardExpr,
            Zen<T> trueExpr,
            Zen<T> falseExpr,
            PathConstraint parameter)
        {
            var trueEnv = parameter.AddPathConstraint(guardExpr);
            var falseEnv = parameter.AddPathConstraint(ZenNotExpr.Create(guardExpr));

            var enumerable = guardExpr.Accept(this, parameter).ToList();

            foreach (var pathConstraint in Compose2(enumerable, trueExpr, trueEnv))
            {
                yield return pathConstraint;
            }

            foreach (var pathConstraint in Compose2(enumerable, falseExpr, falseEnv))
            {
                yield return pathConstraint;
            }
        }

        private IEnumerable<PathConstraint> Fork2<T>(
            Zen<bool> guardExpr1,
            Zen<bool> guardExpr2,
            Zen<T> caseExpr1,
            Zen<T> caseExpr2,
            Zen<T> caseExpr3,
            PathConstraint parameter)
        {
            var case1Env = parameter.AddPathConstraint(guardExpr1);
            var not1Env = parameter.AddPathConstraint(ZenNotExpr.Create(guardExpr1));
            var case2Env = not1Env.AddPathConstraint(guardExpr2);
            var case3Env = not1Env.AddPathConstraint(ZenNotExpr.Create(guardExpr2));

            var enumerable = guardExpr1.Accept(this, parameter);

            foreach (var pathConstraint in Compose2(enumerable, caseExpr1, case1Env))
            {
                yield return pathConstraint;
            }

            enumerable = Compose2(enumerable, guardExpr2, not1Env);

            foreach (var pathConstraint in Compose2(enumerable, caseExpr2, case2Env))
            {
                yield return pathConstraint;
            }

            foreach (var pathConstraint in Compose2(enumerable, caseExpr3, case3Env))
            {
                yield return pathConstraint;
            }
        }
    }
}