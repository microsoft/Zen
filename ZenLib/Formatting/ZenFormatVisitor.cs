// <copyright file="ZenFormatVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class to help pretty-print an expression.
    /// </summary>
    internal class ZenFormatVisitor : ZenExprVisitor<Parameter, (LazyString, bool)>
    {
        /// <summary>
        /// Cutoff width for inlining.
        /// </summary>
        private static readonly int inlineCutoff = 80;

        /// <summary>
        /// The variables and their types.
        /// </summary>
        private IDictionary<string, Type> variableTypes = new SortedDictionary<string, Type>();

        /// <summary>
        /// The let definitions.
        /// </summary>
        private IList<LazyString> letDefinitions = new List<LazyString>();

        /// <summary>
        /// The subexpression that are reused.
        /// </summary>
        private ISet<object> reusedSubExpressions;

        /// <summary>
        /// Mapping from reused subexpressions to their let variable binding.
        /// </summary>
        private IDictionary<object, LazyString> letReuseNames = new Dictionary<object, LazyString>();

        /// <summary>
        /// The next variable id.
        /// </summary>
        private long nextId;

        /// <summary>
        /// Format an expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <returns>The expression as a formatted string.</returns>
        public string Format<T>(Zen<T> expression)
        {
            var reuseVisitor = new ExpressionReuseVisitor();
            this.reusedSubExpressions = reuseVisitor.GetReusedSubExpressions(expression);

            var sb = new StringBuilder();
            var (s, _) = Format(expression, new Parameter { Level = 0 });

            foreach (var kv in this.variableTypes)
            {
                sb.Append("let ");
                sb.Append(kv.Key);
                sb.Append(": ");
                sb.AppendLine(kv.Value.ToString());
            }

            foreach (var letDefinition in this.letDefinitions)
            {
                letDefinition.Write(sb);
                sb.AppendLine();
            }

            s.Write(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Format an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        private (LazyString, bool) Format<T>(Zen<T> expression, Parameter parameter)
        {
            if (this.reusedSubExpressions.Contains(expression))
            {
                if (this.letReuseNames.TryGetValue(expression, out var binding))
                {
                    return (binding, true);
                }

                if (!IsBasicExpression(expression))
                {
                    var v = CreateLetBinding(expression);
                    this.letReuseNames.Add(expression, v);
                    return (v, true);
                }
            }

            if (parameter.Level >= 8 && !IsBasicExpression(expression))
            {
                return (CreateLetBinding(expression), true);
            }

            return expression.Accept(this, parameter);
        }

        /// <summary>
        /// Create a let binding for an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A formatted string.</returns>
        private LazyString CreateLetBinding<T>(Zen<T> expression)
        {
            var (s, _) = expression.Accept(this, new Parameter { Level = 0 });
            var variable = FreshLetVariable();
            this.letDefinitions.Add(new LazyString("let ") + new LazyString(variable) + new LazyString(" = ") + s);
            return new LazyString(variable);
        }

        /// <summary>
        /// Gets a fresh let variable name.
        /// </summary>
        /// <returns></returns>
        private string FreshLetVariable()
        {
            return $"e!{this.nextId++}";
        }

        /// <summary>
        /// Determines if an expression is a basic expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>True if the expression can be formatted on one line.</returns>
        private bool IsBasicExpression<T>(Zen<T> expression)
        {
            var typeName = expression.GetType().Name;
            return expression is ZenArbitraryExpr<T> ||
                   expression is ZenConstantExpr<T> ||
                   typeName.StartsWith("ZenSeqEmptyExpr") ||
                   typeName.StartsWith("ZenMapEmptyExpr") ||
                   typeName.StartsWith("ZenFSeqEmptyExpr");
        }

        /// <summary>
        /// A formatting function that for a function.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="name">The name.</param>
        /// <param name="results">The formatted arguments.</param>
        /// <returns>A formatted string.</returns>
        private (LazyString, bool) FormatFunction(Parameter parameter, string name, params (LazyString, bool)[] results)
        {
            var indent = parameter.Indent().ToString();
            var canInline = true;
            var totalSize = indent.Length + name.Length;
            for (int i = 0; i < results.Length; i++)
            {
                totalSize += results[i].Item1.Length;
                if (!results[i].Item2)
                {
                    canInline = false;
                }
            }

            var inline = canInline && totalSize < inlineCutoff;

            var str = new LazyString(name);
            str = str + new LazyString("(");
            for (int i = 0; i < results.Length; i++)
            {
                if (!inline)
                {
                    str = str + new LazyString("\n");
                    str = str + new LazyString(indent);
                }

                str = str + results[i].Item1;
                if (i + 1 != results.Length)
                {
                    str = str + new LazyString(", ");
                }
            }
            str = str + new LazyString(")");

            return (str, inline);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitLogicalBinop(ZenLogicalBinopExpr expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var stack = new Stack<Zen<bool>>();
            var exprs = new List<Zen<bool>>();
            var queue = new Queue<ZenLogicalBinopExpr>();
            queue.Enqueue(expression);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Expr1 is ZenLogicalBinopExpr e1 && e1.Operation == expression.Operation)
                    queue.Enqueue(e1);
                else
                    exprs.Add(current.Expr1);
                if (current.Expr2 is ZenLogicalBinopExpr e2 && e2.Operation == expression.Operation)
                    queue.Enqueue(e2);
                else
                    stack.Push(current.Expr2);
            }

            exprs.AddRange(stack);
            var arguments = exprs.Select(e => Format(e, indent)).ToArray();
            return FormatFunction(parameter, expression.Operation.ToString(), arguments);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitNot(ZenNotExpr expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            return FormatFunction(parameter, "Not", e1);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitIf<T>(ZenIfExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.GuardExpr, indent);
            var e2 = Format(expression.TrueExpr, indent);
            var e3 = Format(expression.FalseExpr, indent);
            return FormatFunction(parameter, "If", e1, e2, e3);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitConstant<T>(ZenConstantExpr<T> expression, Parameter parameter)
        {
            return (new LazyString(expression.ToString()), true);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitArithBinop<T>(ZenArithBinopExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var exprs = new List<Zen<T>>();
            var stack = new Stack<Zen<T>>();
            var queue = new Queue<ZenArithBinopExpr<T>>();
            queue.Enqueue(expression);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Expr1 is ZenArithBinopExpr<T> e1 && e1.Operation == expression.Operation && expression.Operation != ArithmeticOp.Subtraction)
                    queue.Enqueue(e1);
                else
                    exprs.Add(current.Expr1);
                if (current.Expr2 is ZenArithBinopExpr<T> e2 && e2.Operation == expression.Operation && expression.Operation != ArithmeticOp.Subtraction)
                    queue.Enqueue(e2);
                else
                    stack.Push(current.Expr2);
            }

            exprs.AddRange(stack);
            var arguments = exprs.Select(e => Format(e, indent)).ToArray();
            return FormatFunction(parameter, expression.Operation.ToString(), arguments);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            return FormatFunction(parameter, "BitwiseNot", e1);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var exprs = new List<Zen<T>>();
            var stack = new Stack<Zen<T>>();
            var queue = new Queue<ZenBitwiseBinopExpr<T>>();
            queue.Enqueue(expression);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Expr1 is ZenBitwiseBinopExpr<T> e1 && e1.Operation == expression.Operation)
                    queue.Enqueue(e1);
                else
                    exprs.Add(current.Expr1);
                if (current.Expr2 is ZenBitwiseBinopExpr<T> e2 && e2.Operation == expression.Operation)
                    queue.Enqueue(e2);
                else
                    stack.Push(current.Expr2);
            }

            exprs.AddRange(stack);
            var arguments = exprs.Select(e => Format(e, indent)).ToArray();
            return FormatFunction(parameter, expression.Operation.ToString(), arguments);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitListEmpty<T>(ZenFSeqEmptyExpr<T> expression, Parameter parameter)
        {
            return (new LazyString("[]"), true);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitListAdd<T>(ZenFSeqAddFrontExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.ElementExpr, indent);
            var e2 = Format(expression.ListExpr, indent);
            return FormatFunction(parameter, "Cons", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.MapExpr, indent);
            var e2 = Format(expression.KeyExpr, indent);
            var e3 = Format(expression.ValueExpr, indent);
            return FormatFunction(parameter, "Set", e1, e2, e3);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.MapExpr, indent);
            var e2 = Format(expression.KeyExpr, indent);
            return FormatFunction(parameter, "Delete", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.MapExpr, indent);
            var e2 = Format(expression.KeyExpr, indent);
            return FormatFunction(parameter, "Get", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.MapExpr1, indent);
            var e2 = Format(expression.MapExpr2, indent);
            var op = expression.CombinationType.ToString();
            return FormatFunction(parameter, op, e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.MapExpr, indent);
            var e2 = Format(expression.ValueExpr, indent);
            return FormatFunction(parameter, "Set", e1, (new LazyString(expression.Key.ToString()), true), e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.MapExpr, indent);
            return FormatFunction(parameter, "Get", e1, (new LazyString(expression.Key.ToString()), true));
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitListCase<TList, TResult>(ZenFSeqCaseExpr<TList, TResult> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.ListExpr, indent);
            var e2 = Format(expression.EmptyExpr, indent);
            return FormatFunction(parameter, "Case", e1, e2, (new LazyString("<lambda>"), true));
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.ValueExpr, indent);
            return FormatFunction(parameter, "Unit", e1);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr1, indent);
            var e2 = Format(expression.SeqExpr2, indent);
            return FormatFunction(parameter, "Concat", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            return FormatFunction(parameter, "Length", e1);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqAt<T>(ZenSeqAtExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.IndexExpr, indent);
            return FormatFunction(parameter, "At", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqNth<T>(ZenSeqNthExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.IndexExpr, indent);
            return FormatFunction(parameter, "Nth", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.SubseqExpr, indent);
            var op = expression.ContainmentType.ToString();
            return FormatFunction(parameter, op, e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.SubseqExpr, indent);
            return FormatFunction(parameter, "IndexOf", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.OffsetExpr, indent);
            var e3 = Format(expression.LengthExpr, indent);
            return FormatFunction(parameter, "Slice", e1, e2, e3);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.SubseqExpr, indent);
            var e3 = Format(expression.ReplaceExpr, indent);
            return FormatFunction(parameter, "ReplaceFirst", e1, e2, e3);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            return FormatFunction(parameter, "MatchesRegex", e1, (new LazyString(expression.Regex.ToString()), true));
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            return FormatFunction(parameter, "GetField", e1, (new LazyString("\"" + expression.FieldName + "\""), true));
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            var e3 = Format(expression.FieldExpr, indent);
            return FormatFunction(parameter, "WithField", e1, (new LazyString("\"" + expression.FieldName + "\""), true), e3);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        [ExcludeFromCodeCoverage] // weird issue with call to FormatFunction
        public override (LazyString, bool) VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var fields = expression.Fields.ToArray();
            var results = new (LazyString, bool)[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var fieldName = fields[i].Key;
                var fieldValue = (dynamic)fields[i].Value;
                var (str, inline) = ((LazyString, bool))Format(fieldValue, indent);
                results[i] = (new LazyString(fieldName + "=") + str, inline);
            }

            return FormatFunction(parameter, "new " + typeof(TObject), results);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitEquality<T>(ZenEqualityExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr1, indent);
            var e2 = Format(expression.Expr2, indent);
            return FormatFunction(parameter, "Equals", e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr1, indent);
            var e2 = Format(expression.Expr2, indent);
            var op = expression.ComparisonType.ToString();
            return FormatFunction(parameter, op, e1, e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitParameter<T>(ZenParameterExpr<T> expression, Parameter parameter)
        {
            return (new LazyString($"Parameter({expression.ParameterId})"), true);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitArbitrary<T>(ZenArbitraryExpr<T> expression, Parameter parameter)
        {
            var str = expression.ToString();
            this.variableTypes[str] = typeof(T);
            return (new LazyString(str), true);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SourceExpr, indent);
            return FormatFunction(parameter, "Cast", e1, (new LazyString(typeof(TValue).ToString()), true));
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A formatted string.</returns>
        public override (LazyString, bool) VisitApply<TSrc, TDst>(ZenApplyExpr<TSrc, TDst> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Lambda.Parameter, indent);
            var e2 = Format(expression.Lambda.Body, indent);
            var e3 = Format(expression.ArgumentExpr, indent);
            return FormatFunction(parameter, "Apply", e1, e2, e3);
        }
    }

    /// <summary>
    /// Parameter for the visitor.
    /// </summary>
    internal class Parameter
    {
        /// <summary>
        /// The indentation level.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Create a new parameter with an increased indentation.
        /// </summary>
        /// <returns>The new parameter.</returns>
        public Parameter Indent() { return new Parameter { Level = this.Level + 1 }; }

        /// <summary>
        /// Get the parameter as a string.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            var indent = string.Empty;
            for (int i = 0; i < this.Level; i++)
            {
                indent += "  ";
            }

            return indent;
        }
    }
}
