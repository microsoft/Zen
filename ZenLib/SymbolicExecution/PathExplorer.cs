// <copyright file="PathExplorer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Class to walk through all possible paths in the expression and
    /// enumerate constraints along the paths.
    /// </summary>
    internal sealed class PathExplorer : IZenExprVisitor<PathConstraint, NestedEnumerable<PathConstraint>>
    {
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

        public NestedEnumerable<PathConstraint> VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantByteExpr(ZenConstantByteExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantIntExpr(ZenConstantIntExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantLongExpr(ZenConstantLongExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantShortExpr(ZenConstantShortExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantStringExpr(ZenConstantStringExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantUintExpr(ZenConstantUintExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, PathConstraint parameter)
        {
            return new NestedEnumerable<PathConstraint>(parameter);
        }

        public NestedEnumerable<PathConstraint> VisitZenConstantBigIntExpr(ZenConstantBigIntExpr expression, PathConstraint parameter)
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
                enumerable = Compose2(enumerable, zenObj, parameter);
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

        private NestedEnumerable<PathConstraint> Compose3<T1, T2, T3>(
            Zen<T1> e1,
            Zen<T2> e2,
            Zen<T3> e3,
            PathConstraint parameter)
        {
            var result = new NestedEnumerable<PathConstraint>();

            foreach (var p1 in e1.Accept(this, parameter))
            {
                var parameter2 = parameter.AddPathConstraint(p1);
                result.AddNested(Compose2(e2.Accept(this, parameter2), e3, parameter2));
            }

            return result;
        }

        private NestedEnumerable<PathConstraint> Compose2<T1, T2>(
            Zen<T1> e1,
            Zen<T2> e2,
            PathConstraint parameter)
        {
            return Compose2(e1.Accept(this, parameter), e2, parameter);
        }

        private NestedEnumerable<PathConstraint> Compose2<T>(
            NestedEnumerable<PathConstraint> enumerable,
            Zen<T> e,
            PathConstraint parameter)
        {
            var result = new NestedEnumerable<PathConstraint>();

            foreach (var p1 in enumerable)
            {
                result.AddNested(e.Accept(this, parameter.AddPathConstraint(p1)));
            }

            return result;
        }

        private NestedEnumerable<PathConstraint> Fork<T>(
            Zen<bool> guardExpr,
            Zen<T> trueExpr,
            Zen<T> falseExpr,
            PathConstraint parameter)
        {
            var result = new NestedEnumerable<PathConstraint>();

            var trueEnv = parameter.AddPathConstraint(guardExpr);
            var falseEnv = parameter.AddPathConstraint(ZenNotExpr.Create(guardExpr));

            var enumerable = guardExpr.Accept(this, parameter);

            result.AddNested(Compose2(enumerable, trueExpr, trueEnv));
            result.AddNested(Compose2(enumerable, falseExpr, falseEnv));
            return result;
        }
    }
}