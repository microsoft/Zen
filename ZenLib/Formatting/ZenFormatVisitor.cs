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
    internal class ZenFormatVisitor : IZenExprVisitor<Parameter, (LazyString, bool)>
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

        private LazyString CreateLetBinding<T>(Zen<T> expression)
        {
            var (s, _) = expression.Accept(this, new Parameter { Level = 0 });
            var variable = FreshLetVariable();
            this.letDefinitions.Add(new LazyString("let ") + new LazyString(variable) + new LazyString(" = ") + s);
            return new LazyString(variable);
        }

        private string FreshLetVariable()
        {
            return $"e!{this.nextId++}";
        }

        private bool IsBasicExpression<T>(Zen<T> expression)
        {
            var typeName = expression.GetType().Name;
            return expression is ZenArbitraryExpr<T> ||
                   expression is ZenConstantExpr<T> ||
                   typeName.StartsWith("ZenSeqEmptyExpr") ||
                   typeName.StartsWith("ZenDictEmptyExpr") ||
                   typeName.StartsWith("ZenListEmptyExpr");
        }

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

        public (LazyString, bool) Visit(ZenAndExpr expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var stack = new Stack<Zen<bool>>();
            var exprs = new List<Zen<bool>>();
            var queue = new Queue<ZenAndExpr>();
            queue.Enqueue(expression);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Expr1 is ZenAndExpr e1)
                    queue.Enqueue(e1);
                else
                    exprs.Add(current.Expr1);
                if (current.Expr2 is ZenAndExpr e2)
                    queue.Enqueue(e2);
                else
                    stack.Push(current.Expr2);
            }

            exprs.AddRange(stack);
            var arguments = exprs.Select(e => Format(e, indent)).ToArray();
            return FormatFunction(parameter, "And", arguments);
        }

        public (LazyString, bool) Visit(ZenOrExpr expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var exprs = new List<Zen<bool>>();
            var queue = new Queue<ZenOrExpr>();
            queue.Enqueue(expression);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Expr1 is ZenOrExpr e1)
                    queue.Enqueue(e1);
                else
                    exprs.Add(current.Expr1);
                if (current.Expr2 is ZenOrExpr e2)
                    queue.Enqueue(e2);
                else
                    exprs.Add(current.Expr2);
            }

            var arguments = exprs.Select(e => Format(e, indent)).ToArray();
            return FormatFunction(parameter, "Or", arguments);
        }

        public (LazyString, bool) Visit(ZenNotExpr expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            return FormatFunction(parameter, "Not", e1);
        }

        public (LazyString, bool) Visit<T>(ZenIfExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.GuardExpr, indent);
            var e2 = Format(expression.TrueExpr, indent);
            var e3 = Format(expression.FalseExpr, indent);
            return FormatFunction(parameter, "If", e1, e2, e3);
        }

        public (LazyString, bool) Visit<T>(ZenConstantExpr<T> expression, Parameter parameter)
        {
            return (new LazyString(expression.ToString()), true);
        }

        public (LazyString, bool) Visit<T>(ZenArithBinopExpr<T> expression, Parameter parameter)
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

        public (LazyString, bool) Visit<T>(ZenBitwiseNotExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            return FormatFunction(parameter, "BitwiseNot", e1);
        }

        public (LazyString, bool) Visit<T>(ZenBitwiseBinopExpr<T> expression, Parameter parameter)
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

        public (LazyString, bool) Visit<T>(ZenListEmptyExpr<T> expression, Parameter parameter)
        {
            return (new LazyString("[]"), true);
        }

        public (LazyString, bool) Visit<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, Parameter parameter)
        {
            return (new LazyString("{}"), true);
        }

        public (LazyString, bool) Visit<T>(ZenListAddFrontExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Element, indent);
            var e2 = Format(expression.Expr, indent);
            return FormatFunction(parameter, "Cons", e1, e2);
        }

        public (LazyString, bool) Visit<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.DictExpr, indent);
            var e2 = Format(expression.KeyExpr, indent);
            var e3 = Format(expression.ValueExpr, indent);
            return FormatFunction(parameter, "Set", e1, e2, e3);
        }

        public (LazyString, bool) Visit<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.DictExpr, indent);
            var e2 = Format(expression.KeyExpr, indent);
            return FormatFunction(parameter, "Delete", e1, e2);
        }

        public (LazyString, bool) Visit<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.DictExpr, indent);
            var e2 = Format(expression.KeyExpr, indent);
            return FormatFunction(parameter, "Get", e1, e2);
        }

        public (LazyString, bool) Visit<TKey>(ZenDictCombineExpr<TKey> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.DictExpr1, indent);
            var e2 = Format(expression.DictExpr2, indent);
            var op = expression.CombinationType.ToString();
            return FormatFunction(parameter, op, e1, e2);
        }

        public (LazyString, bool) Visit<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.ListExpr, indent);
            var e2 = Format(expression.EmptyCase, indent);
            return FormatFunction(parameter, "Case", e1, e2, (new LazyString("<lambda>"), true));
        }

        public (LazyString, bool) Visit<T>(ZenSeqEmptyExpr<T> expression, Parameter parameter)
        {
            return (new LazyString("[]"), true);
        }

        public (LazyString, bool) Visit<T>(ZenSeqUnitExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.ValueExpr, indent);
            return FormatFunction(parameter, "Unit", e1);
        }

        public (LazyString, bool) Visit<T>(ZenSeqConcatExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr1, indent);
            var e2 = Format(expression.SeqExpr2, indent);
            return FormatFunction(parameter, "Concat", e1, e2);
        }

        public (LazyString, bool) Visit<T>(ZenSeqLengthExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            return FormatFunction(parameter, "Length", e1);
        }

        public (LazyString, bool) Visit<T>(ZenSeqAtExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.IndexExpr, indent);
            return FormatFunction(parameter, "At", e1, e2);
        }

        public (LazyString, bool) Visit<T>(ZenSeqContainsExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.SubseqExpr, indent);
            var op = expression.ContainmentType.ToString();
            return FormatFunction(parameter, op, e1, e2);
        }

        public (LazyString, bool) Visit<T>(ZenSeqIndexOfExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.SubseqExpr, indent);
            return FormatFunction(parameter, "IndexOf", e1, e2);
        }

        public (LazyString, bool) Visit<T>(ZenSeqSliceExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.OffsetExpr, indent);
            var e3 = Format(expression.LengthExpr, indent);
            return FormatFunction(parameter, "Slice", e1, e2, e3);
        }

        public (LazyString, bool) Visit<T>(ZenSeqReplaceFirstExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            var e2 = Format(expression.SubseqExpr, indent);
            var e3 = Format(expression.ReplaceExpr, indent);
            return FormatFunction(parameter, "ReplaceFirst", e1, e2, e3);
        }

        public (LazyString, bool) Visit<T>(ZenSeqRegexExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SeqExpr, indent);
            return FormatFunction(parameter, "MatchesRegex", e1, (new LazyString(expression.Regex.ToString()), true));
        }

        public (LazyString, bool) Visit<T1, T2>(ZenGetFieldExpr<T1, T2> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            return FormatFunction(parameter, "GetField", e1, (new LazyString(expression.FieldName), true));
        }

        public (LazyString, bool) Visit<T1, T2>(ZenWithFieldExpr<T1, T2> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr, indent);
            var e3 = Format(expression.FieldValue, indent);
            return FormatFunction(parameter, "WithField", e1, (new LazyString(expression.FieldName), true), e3);
        }

        [ExcludeFromCodeCoverage] // weird issue with call to FormatFunction
        public (LazyString, bool) Visit<TObject>(ZenCreateObjectExpr<TObject> expression, Parameter parameter)
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

        public (LazyString, bool) Visit<T>(ZenEqualityExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr1, indent);
            var e2 = Format(expression.Expr2, indent);
            return FormatFunction(parameter, "Equals", e1, e2);
        }

        public (LazyString, bool) Visit<T>(ZenArithComparisonExpr<T> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.Expr1, indent);
            var e2 = Format(expression.Expr2, indent);
            var op = expression.ComparisonType.ToString();
            return FormatFunction(parameter, op, e1, e2);
        }

        public (LazyString, bool) Visit<T>(ZenArgumentExpr<T> expression, Parameter parameter)
        {
            return (new LazyString($"Argument({expression.ArgumentId})"), true);
        }

        public (LazyString, bool) Visit<T>(ZenArbitraryExpr<T> expression, Parameter parameter)
        {
            var str = expression.ToString();
            this.variableTypes[str] = typeof(T);
            return (new LazyString(str), true);
        }

        public (LazyString, bool) Visit<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, Parameter parameter)
        {
            var indent = parameter.Indent();
            var e1 = Format(expression.SourceExpr, indent);
            return FormatFunction(parameter, "Cast", e1, (new LazyString(typeof(TValue).ToString()), true));
        }
    }

    internal class Parameter
    {
        public int Level { get; set; }

        public Parameter Indent() { return new Parameter { Level = this.Level + 1 }; }

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
