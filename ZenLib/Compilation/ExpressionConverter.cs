// <copyright file="Compiler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq.Expressions;
    using System.Reflection;
    using ZenLib.Interpretation;

    /// <summary>
    /// Translates a Zen expression to a compilable expression.
    /// One challenge is that there is no notion of "variable" in Zen
    /// itself, instead variables are defined at the level of C# code.
    /// This means there can be many common subexpressions, and we do not
    /// want to have to execute them multiple times. To avoid redundant work,
    /// we convert to an SSA-like format where we store all intermediate compuations
    /// in variables and reference these variables when needed.
    ///
    /// Unfortunately, we cannot compile recursive match expressions since there
    /// is no built in notion of a function in Zen: it uses C# lambdas. We are not
    /// aware of when recursion is taking place since a match simply expands to a new
    /// match with an opaque callback, which may be the same or may not, for example,
    /// if the lambda is a closure that captures some C# state. Instead, we simply unroll
    /// the match statements to native code and then fall back to interpreting the expression
    /// when we exceed some threshold.
    /// </summary>
    internal class ExpressionConverter : IZenExprVisitor<ExpressionConverterEnvironment, Expression>
    {
        /// <summary>
        /// Lock for thread-safe compilation.
        /// </summary>
        private static object nextVariableLock = new object();

        /// <summary>
        /// Variable id used for easier debugging of compiled code.
        /// </summary>
        private static int nextVariableId = 0;

        /// <summary>
        /// Create an instance of the <see cref="ExpressionConverter"/> class.
        /// </summary>
        /// <param name="subexpressionCache">In scope zen expression to variable cache.</param>
        /// <param name="currentMatchUnrollingDepth">The current unrolling depth.</param>
        /// <param name="maxMatchUnrollingDepth">The maximum allowed unrolling depth.</param>
        public ExpressionConverter(
            ImmutableDictionary<object, Expression> subexpressionCache,
            int currentMatchUnrollingDepth,
            int maxMatchUnrollingDepth)
        {
            this.SubexpressionCache = subexpressionCache;
            this.currentMatchUnrollingDepth = currentMatchUnrollingDepth;
            this.maxMatchUnrollingDepth = maxMatchUnrollingDepth;
        }

        /// <summary>
        /// List of all variables introduced.
        /// </summary>
        public List<ParameterExpression> Variables = new List<ParameterExpression>();

        /// <summary>
        /// Compiles to an SSA-like form where each expression
        /// introduces fresh variables into the block.
        /// </summary>
        public List<Expression> BlockExpressions = new List<Expression>();

        /// <summary>
        /// To avoid redundant computation, we reuse existing computations, which
        /// are stored in previously defined variables.
        /// </summary>
        public ImmutableDictionary<object, Expression> SubexpressionCache;

        /// <summary>
        /// The current match unrolling depth.
        /// </summary>
        private int currentMatchUnrollingDepth;

        /// <summary>
        /// The current match unrolling depth.
        /// </summary>
        private int maxMatchUnrollingDepth;

        /// <summary>
        /// Lookup an existing variable for the expression if defined.
        /// Otherwise, compile the expression, assign it a variable, and
        /// return this variable. Add the assignment to the blockExpressions.
        /// </summary>
        /// <param name="obj">The Zen expression object.</param>
        /// <param name="callback">The callback to compute the result.</param>
        /// <returns>The result of the computation.</returns>
        private Expression LookupOrCompute<T>(Zen<T> obj, Func<Expression> callback)
        {
            if (this.SubexpressionCache.TryGetValue(obj, out var value))
            {
                return value;
            }

            var result = callback();
            var variable = FreshVariable(typeof(T));
            var assign = Expression.Assign(variable, result);
            this.Variables.Add(variable);
            this.BlockExpressions.Add(assign);
            this.SubexpressionCache = this.SubexpressionCache.Add(obj, variable);
            return variable;
        }

        /// <summary>
        /// Convert an 'Adapter' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenAdapterExpr<T1, T2>(ZenAdapterExpr<T1, T2> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var expr = expression.Expr.Accept(this, parameter);
                foreach (var converter in expression.Converters)
                {
                    var method = converter.GetType().GetMethod("Invoke");
                    expr = Expression.Convert(Expression.Call(
                        Expression.Constant(converter),
                        method,
                        Expression.Convert(expr, typeof(object))), typeof(T1));
                }

                return expr;
            });
        }

        /// <summary>
        /// Convert an 'And' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenAndExpr(ZenAndExpr expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var left = CodeGenerator.CompileToBlock(
                    expression.Expr1,
                    parameter,
                    this.SubexpressionCache,
                    this.currentMatchUnrollingDepth,
                    this.maxMatchUnrollingDepth);

                var right = CodeGenerator.CompileToBlock(
                    expression.Expr2,
                    parameter,
                    this.SubexpressionCache,
                    this.currentMatchUnrollingDepth,
                    this.maxMatchUnrollingDepth);

                return Expression.AndAlso(left, right);
            });
        }

        /// <summary>
        /// Convert an 'Arbitrary' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.Constant(default(T));
            });
        }

        /// <summary>
        /// Convert an 'Argument' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.Id];
        }

        /// <summary>
        /// Convert a 'BitwiseAnd' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenBitwiseAndExpr<T>(ZenBitwiseAndExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return BitwiseAnd<T>(
                    expression.Expr1.Accept(this, parameter),
                    expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'BitwiseNot' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return BitwiseNot<T>(expression.Expr.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'BitwiseOr' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenBitwiseOrExpr<T>(ZenBitwiseOrExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return BitwiseOr<T>(
                    expression.Expr1.Accept(this, parameter),
                    expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'BitwiseXor' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenBitwiseXorExpr<T>(ZenBitwiseXorExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return BitwiseXor<T>(
                    expression.Expr1.Accept(this, parameter),
                    expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'BoolConstant' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantBoolExpr(ZenConstantBoolExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant byte expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantByteExpr(ZenConstantByteExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant int expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantIntExpr(ZenConstantIntExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant long expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantLongExpr(ZenConstantLongExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant short expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantShortExpr(ZenConstantShortExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant uint expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantUintExpr(ZenConstantUintExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant ulong expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantUlongExpr(ZenConstantUlongExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant ushort expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantUshortExpr(ZenConstantUshortExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Convert a constant string expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConstantStringExpr(ZenConstantStringExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Create an object given the fields.
        /// </summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="objects"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        private Expression CreateObject<TObject>(Expression[] objects, string[] fields)
        {
            Expression[] exprs = new Expression[fields.Length + 2];

            // first use new default constructor.
            var variable = FreshVariable(typeof(TObject));
            exprs[0] = Expression.Assign(variable, Expression.New(typeof(TObject)));

            // assign each field
            for (int i = 0; i < fields.Length; i++)
            {
                var field = Expression.PropertyOrField(variable, fields[i]);
                exprs[i + 1] = Expression.Assign(field, objects[i]);
            }

            // return a block with the variable.
            exprs[fields.Length + 1] = variable;
            return Expression.Block(new ParameterExpression[] { variable }, exprs);
        }

        /// <summary>
        /// Convert a 'CreateObject' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var fieldNames = new List<string>();
                var parameters = new List<Expression>();
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    var value = fieldValuePair.Value;
                    var valueType = value.GetType();
                    var acceptMethod = valueType
                        .GetMethod("Accept", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(ExpressionConverterEnvironment), typeof(Expression));
                    var valueResult = (Expression)acceptMethod.Invoke(value, new object[] { this, parameter });

                    fieldNames.Add(field);
                    parameters.Add(valueResult);
                }

                return CreateObject<TObject>(parameters.ToArray(), fieldNames.ToArray());
            });
        }

        /// <summary>
        /// Convert an 'Eq' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenEqExpr<T>(ZenEqExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.Equal(
                expression.Expr1.Accept(this, parameter),
                expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert an 'GetField' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var obj = expression.Expr.Accept(this, parameter);
                return Expression.PropertyOrField(obj, expression.FieldName);
            });
        }

        /// <summary>
        /// Convert an 'If' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenIfExpr<T>(ZenIfExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var guardExpr = expression.GuardExpr.Accept(this, parameter);

                var trueExpr = CodeGenerator.CompileToBlock(
                    expression.TrueExpr,
                    parameter,
                    this.SubexpressionCache,
                    this.currentMatchUnrollingDepth,
                    this.maxMatchUnrollingDepth);

                var falseExpr = CodeGenerator.CompileToBlock(
                    expression.FalseExpr,
                    parameter,
                    this.SubexpressionCache,
                    this.currentMatchUnrollingDepth,
                    this.maxMatchUnrollingDepth);

                return Expression.Condition(guardExpr, trueExpr, falseExpr);
            });
        }

        /// <summary>
        /// Convert a 'Leq' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenLeqExpr<T>(ZenLeqExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.LessThanOrEqual(
                expression.Expr1.Accept(this, parameter),
                expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'Geq' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenGeqExpr<T>(ZenGeqExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.GreaterThanOrEqual(
                expression.Expr1.Accept(this, parameter),
                expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'ListAddFront' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var method = typeof(ImmutableList<T>).GetMethod("Insert");
                var list = expression.Expr.Accept(this, parameter);

                var toImmutableListMethod = typeof(CommonUtilities)
                    .GetMethod("ToImmutableList")
                    .MakeGenericMethod(typeof(T));

                var immutableListExpr = Expression.Call(null, toImmutableListMethod, list);

                var element = expression.Element.Accept(this, parameter);
                return Expression.Call(immutableListExpr, method, Expression.Constant(0), element);
            });
        }

        /// <summary>
        /// Convert a 'ListEmpty' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var fieldInfo = typeof(ImmutableList<T>).GetField("Empty");
                return Expression.Field(null, fieldInfo);
            });
        }

        /// <summary>
        /// Convert a 'Match' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenListCaseExpr<TList, TResult>(ZenListCaseExpr<TList, TResult> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var immutableListType = typeof(ImmutableList<TList>);

                // compile the list expression
                var listExpr = expression.ListExpr.Accept(this, parameter);

                // Console.WriteLine($"listExpr: {listExpr}");

                // cast to an immutable list, since it will return IList
                var toImmutableListMethod = typeof(CommonUtilities)
                    .GetMethod("ToImmutableList")
                    .MakeGenericMethod(typeof(TList));

                var immutableListExpr = Expression.Call(toImmutableListMethod, listExpr);

                var listVariable = FreshVariable(immutableListType);
                this.Variables.Add(listVariable);
                this.BlockExpressions.Add(Expression.Assign(listVariable, immutableListExpr));

                // check if list is empty, if so return the empty case
                var isEmptyExpr = Expression.PropertyOrField(listVariable, "IsEmpty");

                // call SplitHead to get the tuple result.
                var splitMethod = typeof(CommonUtilities).GetMethod("SplitHead").MakeGenericMethod(typeof(TList));
                var splitExpr = Expression.Call(splitMethod, listVariable);
                var splitVariable = FreshVariable(typeof(ValueTuple<TList, IList<TList>>));

                // extract the head and tail
                var hdExpr = Expression.PropertyOrField(splitVariable, "Item1");
                var tlExpr = Expression.PropertyOrField(splitVariable, "Item2");

                // compile the empty expression
                var emptyExpr = expression.EmptyCase.Accept(this, parameter);

                // run the cons lambda
                var runMethod = typeof(Interpreter)
                    .GetMethod("CompileRunHelper")
                    .MakeGenericMethod(typeof(TList), typeof(IList<TList>), typeof(TResult));

                // create the bound arguments by constructing the immutable list
                var dictType = typeof(ImmutableDictionary<string, object>);
                var dictField = dictType.GetField("Empty");
                Expression argsExpr = Expression.Field(null, dictField);
                var dictAddMethod = dictType.GetMethod("Add");

                foreach (var kv in parameter.ArgumentAssignment)
                {
                    argsExpr = Expression.Call(
                        argsExpr,
                        dictAddMethod,
                        Expression.Constant(kv.Key),
                        Expression.Convert(kv.Value, typeof(object)));
                }

                // either unroll the match one level, or hand off the the interpreter.
                Expression consExpr;
                if (this.currentMatchUnrollingDepth == this.maxMatchUnrollingDepth)
                {
                    var function = Expression.Constant(expression.ConsCase);
                    consExpr = Expression.Call(runMethod, function, hdExpr, tlExpr, argsExpr);
                }
                else
                {
                    var newAssignment = parameter.ArgumentAssignment;

                    var argHd = new ZenArgumentExpr<TList>();
                    newAssignment = newAssignment.Add(argHd.Id, hdExpr);
                    var argTl = new ZenArgumentExpr<IList<TList>>();
                    newAssignment = newAssignment.Add(argTl.Id, tlExpr);

                    var zenConsExpr = expression.ConsCase(argHd, argTl);

                    consExpr = CodeGenerator.CompileToBlock(
                        zenConsExpr,
                        new ExpressionConverterEnvironment(newAssignment),
                        this.SubexpressionCache,
                        this.currentMatchUnrollingDepth + 1,
                        this.maxMatchUnrollingDepth);
                }

                var nonEmptyBlock = Expression.Block(
                    new List<ParameterExpression> { splitVariable },
                    Expression.Assign(splitVariable, splitExpr),
                    consExpr);

                return Expression.Condition(isEmptyExpr, emptyExpr, nonEmptyBlock);
            });
        }

        /// <summary>
        /// Convert a 'Minus' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenMinusExpr<T>(ZenMinusExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Subtract<T>(
                    expression.Expr1.Accept(this, parameter),
                    expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'Multiply' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenMultiplyExpr<T>(ZenMultiplyExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.Multiply(
                expression.Expr1.Accept(this, parameter),
                expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'Not' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenNotExpr(ZenNotExpr expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.Not(expression.Expr.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert an 'Or' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenOrExpr(ZenOrExpr expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var left = CodeGenerator.CompileToBlock(
                    expression.Expr1,
                    parameter,
                    this.SubexpressionCache,
                    this.currentMatchUnrollingDepth,
                    this.maxMatchUnrollingDepth);

                var right = CodeGenerator.CompileToBlock(
                    expression.Expr2,
                    parameter,
                    this.SubexpressionCache,
                    this.currentMatchUnrollingDepth,
                    this.maxMatchUnrollingDepth);

                return Expression.OrElse(left, right);
            });
        }

        /// <summary>
        /// Convert an 'Add' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenSumExpr<T>(ZenSumExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Add<T>(
                    expression.Expr1.Accept(this, parameter),
                    expression.Expr2.Accept(this, parameter));
            });
        }

        /// <summary>
        /// Convert a 'Concat' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenConcatExpr<T>(ZenConcatExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var l = Expression.Convert(expression.Expr1.Accept(this, parameter), typeof(string));
                var r = Expression.Convert(expression.Expr2.Accept(this, parameter), typeof(string));
                return Expression.Add(l, r, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
            });
        }

        /// <summary>
        /// Convert a 'WithField' expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The compilable expression.</returns>
        public Expression VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var obj = expression.Expr.Accept(this, parameter);
                var value = expression.FieldValue.Accept(this, parameter);
                return WithField<T1>(obj, expression.FieldName, value);
            });
        }

        /// <summary>
        /// Create a copy of an object with a field updated.
        /// </summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <returns></returns>
        private Expression WithField<TObject>(Expression obj, string modifyField, Expression value)
        {
            var fields = ReflectionUtilities.GetAllFields(typeof(TObject));
            var properties = ReflectionUtilities.GetAllProperties(typeof(TObject));

            Expression[] exprs = new Expression[fields.Length + properties.Length + 2];

            // first use new default constructor.
            var variable = FreshVariable(typeof(TObject));
            exprs[0] = Expression.Assign(variable, Expression.New(typeof(TObject)));

            int i = 1;

            // assign each field
            foreach (var field in fields)
            {
                exprs[i++] = WithUpdate(field, modifyField, variable, obj, value);
            }

            // assign each property
            foreach (var property in properties)
            {
                exprs[i++] = WithUpdate(property, modifyField, variable, obj, value);
            }

            // return a block with the variable.
            exprs[fields.Length + properties.Length + 1] = variable;
            return Expression.Block(new ParameterExpression[] { variable }, exprs);
        }

        private Expression WithUpdate(dynamic field, string modifyField, ParameterExpression variable, Expression obj, Expression value)
        {
            if (field.Name == modifyField)
            {
                var propertyOrFieldToSet = Expression.PropertyOrField(variable, field.Name);
                return Expression.Assign(propertyOrFieldToSet, value);
            }
            else
            {
                var propertyOrFieldToSet = Expression.PropertyOrField(variable, field.Name);
                var propertyOrFieldToGet = Expression.PropertyOrField(obj, field.Name);
                return Expression.Assign(propertyOrFieldToSet, propertyOrFieldToGet);
            }
        }

        private ParameterExpression FreshVariable(Type type)
        {
            lock (nextVariableLock)
            {
                return Expression.Variable(type, "v" + nextVariableId++);
            }
        }

        private Expression WrapMathUnary<T>(Expression input, Func<Expression, Expression> f)
        {
            var type = typeof(T);
            return Expression.Convert(f(Expression.Convert(input, type)), type);
        }

        private Expression WrapMathBinary<T1, T2>(Expression left, Expression right, Func<Expression, Expression, Expression> f)
        {
            var type = typeof(T1);
            var l = Expression.Convert(left, type);
            var r = Expression.Convert(right, type);
            return Expression.Convert(f(l, r), typeof(T2));
        }

        private Expression BitwiseNot<T>(Expression e)
        {
            return WrapMathUnary<T>(e, Expression.OnesComplement);
        }

        private Expression BitwiseOr<T>(Expression left, Expression right)
        {
            return WrapMathBinary<T, T>(left, right, Expression.Or);
        }

        private Expression BitwiseAnd<T>(Expression left, Expression right)
        {
            return WrapMathBinary<T, T>(left, right, Expression.And);
        }

        private Expression BitwiseXor<T>(Expression left, Expression right)
        {
            return WrapMathBinary<T, T>(left, right, Expression.ExclusiveOr);
        }

        private Expression Add<T>(Expression left, Expression right)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.ByteType ||
                type == ReflectionUtilities.ShortType ||
                type == ReflectionUtilities.UshortType)
            {
                return WrapMathBinary<int, T>(left, right, Expression.Add);
            }

            return WrapMathBinary<T, T>(left, right, Expression.Add);
        }

        private Expression Subtract<T>(Expression left, Expression right)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.ByteType ||
                type == ReflectionUtilities.ShortType ||
                type == ReflectionUtilities.UshortType)
            {
                return WrapMathBinary<int, T>(left, right, Expression.Subtract);
            }

            return WrapMathBinary<T, T>(left, right, Expression.Subtract);
        }
    }
}
