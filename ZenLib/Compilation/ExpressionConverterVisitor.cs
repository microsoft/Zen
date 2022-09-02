// <copyright file="ExpressionConverterVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq.Expressions;
    using System.Numerics;
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
    internal class ExpressionConverterVisitor : ZenExprVisitor<ExpressionConverterEnvironment, Expression>
    {
        /// <summary>
        /// The convert method.
        /// </summary>
        private static MethodInfo convertMethod = typeof(ExpressionConverterVisitor).GetMethodCached("Convert");

        /// <summary>
        /// Variable id used for easier debugging of compiled code.
        /// </summary>
        private int nextVariableId = 0;

        /// <summary>
        /// Create an instance of the <see cref="ExpressionConverterVisitor"/> class.
        /// </summary>
        /// <param name="subexpressionCache">In scope zen expression to variable cache.</param>
        /// <param name="currentMatchUnrollingDepth">The current unrolling depth.</param>
        /// <param name="maxMatchUnrollingDepth">The maximum allowed unrolling depth.</param>
        public ExpressionConverterVisitor(
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
        /// <param name="parameter">The environment parameter.</param>
        /// <returns>The result of the computation.</returns>
        public Expression Convert<T>(Zen<T> obj, ExpressionConverterEnvironment parameter)
        {
            var key = (obj, parameter);
            if (this.SubexpressionCache.TryGetValue(key, out var value))
            {
                return value;
            }

            var result = obj.Accept(this, parameter);
            var variable = FreshVariable(typeof(T));
            var assign = Expression.Assign(variable, result);
            this.Variables.Add(variable);
            this.BlockExpressions.Add(assign);
            this.SubexpressionCache = this.SubexpressionCache.Add(key, variable);
            return variable;
        }

        /// <summary>
        /// Compile a lambda to a C# function.
        /// </summary>
        /// <typeparam name="TSrc">The parameter type.</typeparam>
        /// <typeparam name="TDst">The return type.</typeparam>
        /// <param name="lambda">The Zen lambda.</param>
        /// <returns>A C# function.</returns>
        public Func<TSrc, TDst> CompileLambda<TSrc, TDst>(ZenLambda<TSrc, TDst> lambda)
        {
            var param = Expression.Parameter(typeof(TSrc));
            var paramExprs = ImmutableDictionary<long, Expression>.Empty.Add(lambda.Parameter.ParameterId, param);
            return CodeGenerator.Compile<TSrc, TDst>(lambda.Body, paramExprs, param, this.maxMatchUnrollingDepth);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitApply<TSrc, TDst>(ZenApplyExpr<TSrc, TDst> expression, ExpressionConverterEnvironment parameter)
        {
            var lambdaFunction = CompileLambda(expression.Lambda);
            Expression<Func<TSrc, TDst>> lambdaExpression = (x) => lambdaFunction(x);

            var argument = CodeGenerator.CompileToBlock(
                expression.ArgumentExpr,
                parameter,
                this.SubexpressionCache,
                this.currentMatchUnrollingDepth,
                this.maxMatchUnrollingDepth);

            return Expression.Invoke(lambdaExpression, argument);

            /* var newAssignment = parameter.ArgumentAssignment.SetItem(expression.Lambda.Parameter.ParameterId, left);

            Console.WriteLine(expression.Lambda.GetHashCode());

            return CodeGenerator.CompileToBlock(
                expression.Lambda.Body,
                new ExpressionConverterEnvironment(newAssignment),
                this.SubexpressionCache,
                this.currentMatchUnrollingDepth,
                this.maxMatchUnrollingDepth); */
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitLogicalBinop(ZenLogicalBinopExpr expression, ExpressionConverterEnvironment parameter)
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

            switch (expression.Operation)
            {
                case ZenLogicalBinopExpr.LogicalOp.And:
                    return Expression.AndAlso(left, right);
                default:
                    Contract.Assert(expression.Operation == ZenLogicalBinopExpr.LogicalOp.Or);
                    return Expression.OrElse(left, right);
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitArbitrary<T>(ZenArbitraryExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(ReflectionUtilities.GetDefaultValue<T>());
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitParameter<T>(ZenParameterExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.ParameterId];
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitArithBinop<T>(ZenArithBinopExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.Expr1, parameter);
            var e2 = Convert(expression.Expr2, parameter);

            switch (expression.Operation)
            {
                case ArithmeticOp.Addition:
                    return Add<T>(e1, e2);
                case ArithmeticOp.Subtraction:
                    return Subtract<T>(e1, e2);
                default:
                    Contract.Assert(expression.Operation == ArithmeticOp.Multiplication);
                    return Expression.Multiply(e1, e2);
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitBitwiseBinop<T>(ZenBitwiseBinopExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.Expr1, parameter);
            var e2 = Convert(expression.Expr2, parameter);

            switch (expression.Operation)
            {
                case BitwiseOp.BitwiseAnd:
                    return BitwiseAnd<T>(e1, e2);
                case BitwiseOp.BitwiseOr:
                    return BitwiseOr<T>(e1, e2);
                default:
                    Contract.Assert(expression.Operation == BitwiseOp.BitwiseXor);
                    return BitwiseXor<T>(e1, e2);
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitBitwiseNot<T>(ZenBitwiseNotExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return BitwiseNot<T>(Convert(expression.Expr, parameter));
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitConstant<T>(ZenConstantExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitCreateObject<TObject>(ZenCreateObjectExpr<TObject> expression, ExpressionConverterEnvironment parameter)
        {
            var fieldNames = new List<string>();
            var parameters = new List<Expression>();
            foreach (var fieldValuePair in expression.Fields)
            {
                var field = fieldValuePair.Key;
                var value = fieldValuePair.Value;
                var valueType = value.GetType();
                var method = convertMethod.MakeGenericMethod(valueType.BaseType.GetGenericArgumentsCached()[0]);
                var valueResult = (Expression)method.Invoke(this, new object[] { value, parameter });
                fieldNames.Add(field);
                parameters.Add(valueResult);
            }

            return CreateObject<TObject>(parameters.ToArray(), fieldNames.ToArray());
        }

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
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitGetField<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionConverterEnvironment parameter)
        {
            var obj = Convert(expression.Expr, parameter);
            return Expression.PropertyOrField(obj, expression.FieldName);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitIf<T>(ZenIfExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var guardExpr = Convert(expression.GuardExpr, parameter);

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
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitEquality<T>(ZenEqualityExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.Expr1, parameter);
            var e2 = Convert(expression.Expr2, parameter);
            return Expression.Call(e1, typeof(T).GetMethod("Equals", new Type[] { typeof(T) }), e2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitArithComparison<T>(ZenArithComparisonExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.Expr1, parameter);
            var e2 = Convert(expression.Expr2, parameter);

            switch (expression.ComparisonType)
            {
                case ComparisonType.Geq:
                    if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
                    {
                        return Expression.Call(e1, typeof(T).GetMethodCached("GreaterThanOrEqual"), e2);
                    }

                    return Expression.GreaterThanOrEqual(e1, e2);

                case ComparisonType.Gt:
                    if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
                    {
                        return Expression.Call(e1, typeof(T).GetMethodCached("GreaterThan"), e2);
                    }

                    return Expression.GreaterThan(e1, e2);

                case ComparisonType.Lt:
                    if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
                    {
                        return Expression.Call(e1, typeof(T).GetMethodCached("LessThan"), e2);
                    }

                    return Expression.LessThan(e1, e2);

                default:
                    Contract.Assert(expression.ComparisonType == ComparisonType.Leq);
                    if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
                    {
                        return Expression.Call(e1, typeof(T).GetMethodCached("LessThanOrEqual"), e2);
                    }

                    return Expression.LessThanOrEqual(e1, e2);
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitListAdd<T>(ZenFSeqAddFrontExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var list = Convert(expression.Expr, parameter);
            var element = Convert(expression.ElementExpr, parameter);
            var fseqExpr = Expression.Convert(list, typeof(FSeq<T>));
            var method = typeof(FSeq<T>).GetMethodCached("AddFrontOption");
            return Expression.Call(fseqExpr, method, element);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitListEmpty<T>(ZenFSeqEmptyExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var c = typeof(FSeq<T>).GetConstructor(new Type[] { });
            return Expression.New(c);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitListCase<TList, TResult>(ZenFSeqCaseExpr<TList, TResult> expression, ExpressionConverterEnvironment parameter)
        {
            var fseqType = typeof(FSeq<TList>);

            // compile the list expression
            var listExpr = Convert(expression.ListExpr, parameter);
            var fseqExpr = Expression.Convert(listExpr, typeof(FSeq<TList>));

            var fseqVariable = FreshVariable(fseqType);
            this.Variables.Add(fseqVariable);
            this.BlockExpressions.Add(Expression.Assign(fseqVariable, fseqExpr));

            // check if list is empty, if so return the empty case
            var isEmptyMethod = typeof(FSeq<TList>).GetMethodCached("IsEmpty");
            var isEmptyExpr = Expression.Call(fseqVariable, isEmptyMethod);

            // call SplitHead to get the tuple result.
            var splitMethod = typeof(CommonUtilities).GetMethodCached("SplitHead").MakeGenericMethod(typeof(TList));
            var splitExpr = Expression.Call(splitMethod, fseqVariable);
            var splitVariable = FreshVariable(typeof(ValueTuple<Option<TList>, FSeq<TList>>));

            // extract the head and tail
            var hdExpr = Expression.PropertyOrField(splitVariable, "Item1");
            var tlExpr = Expression.PropertyOrField(splitVariable, "Item2");

            // compile the empty expression
            var emptyExpr = Convert(expression.EmptyExpr, parameter);

            // run the cons lambda
            var runMethod = typeof(Interpreter)
                .GetMethodCached("CompileRunHelper")
                .MakeGenericMethod(typeof(Pair<Option<TList>, FSeq<TList>>), typeof(TResult));

            // create the bound arguments by constructing the immutable list
            var dictType = typeof(ImmutableDictionary<long, object>);
            var dictField = dictType.GetFieldCached("Empty");
            Expression argsExpr = Expression.Field(null, dictField);
            var dictAddMethod = dictType.GetMethodCached("Add");

            foreach (var kv in parameter.ArgumentAssignment)
            {
                argsExpr = Expression.Call(
                    argsExpr,
                    dictAddMethod,
                    Expression.Constant(kv.Key),
                    Expression.Convert(kv.Value, typeof(object)));
            }

            // pair constructor
            var constructor = typeof(Pair<Option<TList>, FSeq<TList>>).GetConstructor(new Type[] { typeof(Option<TList>), typeof(FSeq<TList>) });
            var pairExpr = Expression.New(constructor, hdExpr, tlExpr);

            // either unroll the match one level, or hand off the the interpreter.
            Expression consExpr;
            if (this.currentMatchUnrollingDepth == this.maxMatchUnrollingDepth)
            {
                var function = Expression.PropertyOrField(Expression.Constant(expression.ConsCase), "Function");
                consExpr = Expression.Call(runMethod, function, pairExpr, argsExpr);
            }
            else
            {
                var newAssignment = parameter.ArgumentAssignment;
                newAssignment = newAssignment.SetItem(
                    expression.ConsCase.Parameter.ParameterId,
                    pairExpr);

                consExpr = CodeGenerator.CompileToBlock(
                    expression.ConsCase.Body,
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
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitNot(ZenNotExpr expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Not(Convert(expression.Expr, parameter));
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitWithField<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ExpressionConverterEnvironment parameter)
        {
            var obj = Convert(expression.Expr, parameter);
            var value = Convert(expression.FieldExpr, parameter);
            return WithField<T1>(obj, expression.FieldName, value);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitMapSet<TKey, TValue>(ZenMapSetExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            var dict = Convert(expression.MapExpr, parameter);
            var key = Convert(expression.KeyExpr, parameter);
            var value = Convert(expression.ValueExpr, parameter);
            var mapExpr = Expression.Convert(dict, typeof(Map<TKey, TValue>));
            var method = typeof(Map<TKey, TValue>).GetMethodCached("Set");
            return Expression.Call(mapExpr, method, key, value);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitMapDelete<TKey, TValue>(ZenMapDeleteExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            var dict = Convert(expression.MapExpr, parameter);
            var key = Convert(expression.KeyExpr, parameter);
            var mapExpr = Expression.Convert(dict, typeof(Map<TKey, TValue>));
            var method = typeof(Map<TKey, TValue>).GetMethodCached("Delete");
            return Expression.Call(mapExpr, method, key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitMapGet<TKey, TValue>(ZenMapGetExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            var dict = Convert(expression.MapExpr, parameter);
            var key = Convert(expression.KeyExpr, parameter);
            var mapExpr = Expression.Convert(dict, typeof(Map<TKey, TValue>));
            var method = typeof(Map<TKey, TValue>).GetMethodCached("Get");
            return Expression.Call(mapExpr, method, key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitMapCombine<TKey>(ZenMapCombineExpr<TKey> expression, ExpressionConverterEnvironment parameter)
        {
            MethodInfo method;
            switch (expression.CombinationType)
            {
                case ZenMapCombineExpr<TKey>.CombineType.Union:
                    method = typeof(CommonUtilities).GetMethodCached("DictionaryUnion").MakeGenericMethod(typeof(TKey));
                    break;
                case ZenMapCombineExpr<TKey>.CombineType.Intersect:
                    method = typeof(CommonUtilities).GetMethodCached("DictionaryIntersect").MakeGenericMethod(typeof(TKey));
                    break;
                default:
                    Contract.Assert(expression.CombinationType == ZenMapCombineExpr<TKey>.CombineType.Difference);
                    method = typeof(CommonUtilities).GetMethodCached("DictionaryDifference").MakeGenericMethod(typeof(TKey));
                    break;
            }

            var dict1 = Convert(expression.MapExpr1, parameter);
            var dict2 = Convert(expression.MapExpr2, parameter);
            var mapExpr1 = Expression.Convert(dict1, typeof(Map<TKey, SetUnit>));
            var mapExpr2 = Expression.Convert(dict2, typeof(Map<TKey, SetUnit>));
            return Expression.Call(null, method, mapExpr1, mapExpr2);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitConstMapSet<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            var dict = Convert(expression.MapExpr, parameter);
            var key = Expression.Constant(expression.Key);
            var value = Convert(expression.ValueExpr, parameter);
            var mapExpr = Expression.Convert(dict, typeof(CMap<TKey, TValue>));
            var method = typeof(CMap<TKey, TValue>).GetMethodCached("Set");
            return Expression.Call(mapExpr, method, key, value);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitConstMapGet<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            var dict = Convert(expression.MapExpr, parameter);
            var key = Expression.Constant(expression.Key);
            var mapExpr = Expression.Convert(dict, typeof(CMap<TKey, TValue>));
            var method = typeof(CMap<TKey, TValue>).GetMethodCached("Get");
            return Expression.Call(mapExpr, method, key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqUnit<T>(ZenSeqUnitExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var c = typeof(Seq<T>).GetConstructor(new Type[] { typeof(T) });
            var e = Convert(expression.ValueExpr, parameter);
            return Expression.New(c, new Expression[] { e });
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqConcat<T>(ZenSeqConcatExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var l = Convert(expression.SeqExpr1, parameter);
            var r = Convert(expression.SeqExpr2, parameter);
            var m = typeof(Seq<T>).GetMethod("Concat");
            return Expression.Call(l, m, new Expression[] { r });
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqLength<T>(ZenSeqLengthExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var s = Convert(expression.SeqExpr, parameter);
            var m = typeof(Seq<T>).GetMethod("Length");
            var e = Expression.Call(s, m);
            var c = typeof(BigInteger).GetConstructor(new Type[] { typeof(int) });
            return Expression.New(c, e);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqAt<T>(ZenSeqAtExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.SeqExpr, parameter);
            var e2 = Convert(expression.IndexExpr, parameter);
            var m = typeof(Seq<T>).GetMethod("AtBigInteger", BindingFlags.Instance | BindingFlags.NonPublic);
            return Expression.Call(e1, m, new Expression[] { e2 });
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqContains<T>(ZenSeqContainsExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.SeqExpr, parameter);
            var e2 = Convert(expression.SubseqExpr, parameter);

            switch (expression.ContainmentType)
            {
                case SeqContainmentType.HasPrefix:
                    return Expression.Call(e1, typeof(Seq<T>).GetMethodCached("HasPrefix"), new Expression[] { e2 });
                case SeqContainmentType.HasSuffix:
                    return Expression.Call(e1, typeof(Seq<T>).GetMethodCached("HasSuffix"), new Expression[] { e2 });
                default:
                    Contract.Assert(expression.ContainmentType == SeqContainmentType.Contains);
                    return Expression.Call(e1, typeof(Seq<T>).GetMethodCached("Contains"), new Expression[] { e2 });
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqIndexOf<T>(ZenSeqIndexOfExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.SeqExpr, parameter);
            var e2 = Convert(expression.SubseqExpr, parameter);
            var e3 = Convert(expression.OffsetExpr, parameter);
            var m = typeof(Seq<T>).GetMethod("IndexOfBigInteger", BindingFlags.Instance | BindingFlags.NonPublic);
            return Expression.Call(e1, m, new Expression[] { e2, e3 });
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqSlice<T>(ZenSeqSliceExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.SeqExpr, parameter);
            var e2 = Convert(expression.OffsetExpr, parameter);
            var e3 = Convert(expression.LengthExpr, parameter);
            var m = typeof(Seq<T>).GetMethod("SliceBigInteger", BindingFlags.Instance | BindingFlags.NonPublic);
            return Expression.Call(e1, m, new Expression[] { e2, e3 });
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqReplaceFirst<T>(ZenSeqReplaceFirstExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e1 = Convert(expression.SeqExpr, parameter);
            var e2 = Convert(expression.SubseqExpr, parameter);
            var e3 = Convert(expression.ReplaceExpr, parameter);
            var m = typeof(Seq<T>).GetMethod("ReplaceFirst");
            return Expression.Call(e1, m, new Expression[] { e2, e3 });
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitCast<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            var e = Convert(expression.SourceExpr, parameter);

            if (typeof(TKey) == ReflectionUtilities.StringType)
            {
                var m = typeof(Seq).GetMethodCached("FromString");
                return Expression.Call(null, m, new Expression[] { e });
            }
            else if (typeof(TKey) == ReflectionUtilities.UnicodeSequenceType)
            {
                var m = typeof(Seq).GetMethodCached("AsString");
                return Expression.Call(null, m, new Expression[] { e });
            }
            else
            {
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TKey)));
                Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(TValue)));
                var m = typeof(IntN).GetMethodCached("CastFiniteInteger").MakeGenericMethod(typeof(TKey), typeof(TValue));
                return Expression.Call(null, m, new Expression[] { e });
            }
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The Zen expression.</param>
        /// <param name="parameter">The environment.</param>
        /// <returns>An expression tree.</returns>
        public override Expression VisitSeqRegex<T>(ZenSeqRegexExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            var e = Convert(expression.SeqExpr, parameter);
            var m = typeof(Seq<T>).GetMethod("MatchesRegex");
            return Expression.Call(e, m, new Expression[] { Expression.Constant(expression.Regex) });
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
            if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
            {
                var method = typeof(T).GetMethodCached("BitwiseOr");
                return Expression.Call(left, method, right);
            }

            return WrapMathBinary<T, T>(left, right, Expression.Or);
        }

        private Expression BitwiseAnd<T>(Expression left, Expression right)
        {
            if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
            {
                var method = typeof(T).GetMethodCached("BitwiseAnd");
                return Expression.Call(left, method, right);
            }

            return WrapMathBinary<T, T>(left, right, Expression.And);
        }

        private Expression BitwiseXor<T>(Expression left, Expression right)
        {
            if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
            {
                var method = typeof(T).GetMethodCached("BitwiseXor");
                return Expression.Call(left, method, right);
            }

            return WrapMathBinary<T, T>(left, right, Expression.ExclusiveOr);
        }

        private Expression Add<T>(Expression left, Expression right)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.BigIntType || type == ReflectionUtilities.RealType)
            {
                return Expression.Add(left, right);
            }

            if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var method = typeof(T).GetMethodCached("Add");
                return Expression.Call(left, method, right);
            }

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

            if (type == ReflectionUtilities.BigIntType || type == ReflectionUtilities.RealType)
            {
                return Expression.Subtract(left, right);
            }

            if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                var method = typeof(T).GetMethodCached("Subtract");
                return Expression.Call(left, method, right);
            }

            if (type == ReflectionUtilities.ByteType ||
                type == ReflectionUtilities.ShortType ||
                type == ReflectionUtilities.UshortType)
            {
                return WrapMathBinary<int, T>(left, right, Expression.Subtract);
            }

            return WrapMathBinary<T, T>(left, right, Expression.Subtract);
        }

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
            return Expression.Variable(type, "v" + nextVariableId++);
        }
    }
}
