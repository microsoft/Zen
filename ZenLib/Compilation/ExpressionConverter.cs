// <copyright file="Compiler.cs" company="Microsoft">
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
    internal class ExpressionConverter : IZenExprVisitor<ExpressionConverterEnvironment, Expression>
    {
        /// <summary>
        /// Variable id used for easier debugging of compiled code.
        /// </summary>
        private int nextVariableId = 0;

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

        public Expression VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.Constant(ReflectionUtilities.GetDefaultValue<T>());
            });
        }

        public Expression VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return parameter.ArgumentAssignment[expression.ArgumentId];
        }

        public Expression VisitZenIntegerBinopExpr<T>(ZenIntegerBinopExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                switch (expression.Operation)
                {
                    case Op.BitwiseAnd:
                        return BitwiseAnd<T>(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    case Op.BitwiseOr:
                        return BitwiseOr<T>(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    case Op.BitwiseXor:
                        return BitwiseXor<T>(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    case Op.Addition:
                        return Add<T>(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    case Op.Subtraction:
                        return Subtract<T>(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    case Op.Multiplication:
                        return Expression.Multiply(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    default:
                        throw new ZenUnreachableException();
                }
            });
        }

        public Expression VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return BitwiseNot<T>(expression.Expr.Accept(this, parameter));
            });
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

            if (type == ReflectionUtilities.BigIntType)
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

            if (type == ReflectionUtilities.BigIntType)
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

        public Expression VisitZenConstantExpr<T>(ZenConstantExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return Expression.Constant(expression.Value);
        }

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

        public Expression VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var obj = expression.Expr.Accept(this, parameter);
                return Expression.PropertyOrField(obj, expression.FieldName);
            });
        }

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

        public Expression VisitZenEqualityExpr<T>(ZenEqualityExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
                {
                    return Expression.Call(
                        expression.Expr1.Accept(this, parameter),
                        typeof(T).GetMethod("Equals", new Type[] { typeof(object) }),
                        expression.Expr2.Accept(this, parameter));
                }

                if (ReflectionUtilities.IsIDictType(typeof(T)))
                {
                    var typeArgs = typeof(T).GetGenericArgumentsCached();
                    var keyType = typeArgs[0];
                    var valueType = typeArgs[1];

                    var method = typeof(CommonUtilities)
                        .GetMethodCached("DictionaryEquals")
                        .MakeGenericMethod(keyType, valueType);

                    var dict1 = expression.Expr1.Accept(this, parameter);
                    var dict2 = expression.Expr2.Accept(this, parameter);

                    var toImmutableDictMethod = typeof(CommonUtilities)
                        .GetMethodCached("ToImmutableDictionary")
                        .MakeGenericMethod(keyType, valueType);

                    var immutableDictExpr1 = Expression.Call(null, toImmutableDictMethod, dict1);
                    var immutableDictExpr2 = Expression.Call(null, toImmutableDictMethod, dict2);

                    return Expression.Call(null, method, immutableDictExpr1, immutableDictExpr2);
                }

                return Expression.Equal(
                    expression.Expr1.Accept(this, parameter),
                    expression.Expr2.Accept(this, parameter));
            });
        }

        public Expression VisitZenComparisonExpr<T>(ZenIntegerComparisonExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                switch (expression.ComparisonType)
                {
                    case ComparisonType.Geq:
                        if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
                        {
                            return Expression.Call(
                                expression.Expr1.Accept(this, parameter),
                                typeof(T).GetMethodCached("GreaterThanOrEqual"),
                                expression.Expr2.Accept(this, parameter));
                        }

                        return Expression.GreaterThanOrEqual(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    case ComparisonType.Leq:
                        if (ReflectionUtilities.IsFixedIntegerType(typeof(T)))
                        {
                            return Expression.Call(
                                expression.Expr1.Accept(this, parameter),
                                typeof(T).GetMethodCached("LessThanOrEqual"),
                                expression.Expr2.Accept(this, parameter));
                        }

                        return Expression.LessThanOrEqual(
                            expression.Expr1.Accept(this, parameter),
                            expression.Expr2.Accept(this, parameter));

                    default:
                        throw new ZenUnreachableException();
                }
            });
        }

        public Expression VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var method = typeof(ImmutableList<T>).GetMethodCached("Insert");
                var list = expression.Expr.Accept(this, parameter);

                var toImmutableListMethod = typeof(CommonUtilities)
                    .GetMethodCached("ToImmutableList")
                    .MakeGenericMethod(typeof(T));

                var immutableListExpr = Expression.Call(null, toImmutableListMethod, list);

                var element = expression.Element.Accept(this, parameter);
                return Expression.Call(immutableListExpr, method, Expression.Constant(0), element);
            });
        }

        public Expression VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var fieldInfo = typeof(ImmutableList<T>).GetFieldCached("Empty");
                return Expression.Field(null, fieldInfo);
            });
        }

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
                    .GetMethodCached("ToImmutableList")
                    .MakeGenericMethod(typeof(TList));

                var immutableListExpr = Expression.Call(toImmutableListMethod, listExpr);

                var listVariable = FreshVariable(immutableListType);
                this.Variables.Add(listVariable);
                this.BlockExpressions.Add(Expression.Assign(listVariable, immutableListExpr));

                // check if list is empty, if so return the empty case
                var isEmptyExpr = Expression.PropertyOrField(listVariable, "IsEmpty");

                // call SplitHead to get the tuple result.
                var splitMethod = typeof(CommonUtilities).GetMethodCached("SplitHead").MakeGenericMethod(typeof(TList));
                var splitExpr = Expression.Call(splitMethod, listVariable);
                var splitVariable = FreshVariable(typeof(ValueTuple<TList, IList<TList>>));

                // extract the head and tail
                var hdExpr = Expression.PropertyOrField(splitVariable, "Item1");
                var tlExpr = Expression.PropertyOrField(splitVariable, "Item2");

                // compile the empty expression
                var emptyExpr = expression.EmptyCase.Accept(this, parameter);

                // run the cons lambda
                var runMethod = typeof(Interpreter)
                    .GetMethodCached("CompileRunHelper")
                    .MakeGenericMethod(typeof(TList), typeof(IList<TList>), typeof(TResult));

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
                    newAssignment = newAssignment.Add(argHd.ArgumentId, hdExpr);
                    var argTl = new ZenArgumentExpr<IList<TList>>();
                    newAssignment = newAssignment.Add(argTl.ArgumentId, tlExpr);

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

        public Expression VisitZenNotExpr(ZenNotExpr expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                return Expression.Not(expression.Expr.Accept(this, parameter));
            });
        }

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

        public Expression VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var obj = expression.Expr.Accept(this, parameter);
                var value = expression.FieldValue.Accept(this, parameter);
                return WithField<T1>(obj, expression.FieldName, value);
            });
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

        public Expression VisitZenDictEmptyExpr<TKey, TValue>(ZenDictEmptyExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var fieldInfo = typeof(ImmutableDictionary<TKey, TValue>).GetFieldCached("Empty");
                return Expression.Field(null, fieldInfo);
            });
        }

        public Expression VisitZenDictSetExpr<TKey, TValue>(ZenDictSetExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var method = typeof(ImmutableDictionary<TKey, TValue>).GetMethodCached("SetItem");
                var dict = expression.DictExpr.Accept(this, parameter);

                var toImmutableDictMethod = typeof(CommonUtilities)
                    .GetMethodCached("ToImmutableDictionary")
                    .MakeGenericMethod(typeof(TKey), typeof(TValue));

                var immutableDictExpr = Expression.Call(null, toImmutableDictMethod, dict);

                var key = expression.KeyExpr.Accept(this, parameter);
                var value = expression.ValueExpr.Accept(this, parameter);
                return Expression.Call(immutableDictExpr, method, key, value);
            });
        }

        public Expression VisitZenDictDeleteExpr<TKey, TValue>(ZenDictDeleteExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var method = typeof(ImmutableDictionary<TKey, TValue>).GetMethodCached("Remove");
                var dict = expression.DictExpr.Accept(this, parameter);

                var toImmutableDictMethod = typeof(CommonUtilities)
                    .GetMethodCached("ToImmutableDictionary")
                    .MakeGenericMethod(typeof(TKey), typeof(TValue));

                var immutableDictExpr = Expression.Call(null, toImmutableDictMethod, dict);

                var key = expression.KeyExpr.Accept(this, parameter);
                return Expression.Call(immutableDictExpr, method, key);
            });
        }

        public Expression VisitZenDictGetExpr<TKey, TValue>(ZenDictGetExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var method = typeof(CommonUtilities)
                    .GetMethodCached("DictionaryGet")
                    .MakeGenericMethod(typeof(TKey), typeof(TValue));

                var dict = expression.DictExpr.Accept(this, parameter);

                var toImmutableDictMethod = typeof(CommonUtilities)
                    .GetMethodCached("ToImmutableDictionary")
                    .MakeGenericMethod(typeof(TKey), typeof(TValue));

                var immutableDictExpr = Expression.Call(null, toImmutableDictMethod, dict);

                var key = expression.KeyExpr.Accept(this, parameter);
                return Expression.Call(null, method, immutableDictExpr, key);
            });
        }

        public Expression VisitZenDictCombineExpr<TKey>(ZenDictCombineExpr<TKey> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                MethodInfo method;
                switch (expression.CombinationType)
                {
                    case ZenDictCombineExpr<TKey>.CombineType.Union:
                        method = typeof(CommonUtilities).GetMethodCached("DictionaryUnion").MakeGenericMethod(typeof(TKey));
                        break;
                    case ZenDictCombineExpr<TKey>.CombineType.Intersect:
                        method = typeof(CommonUtilities).GetMethodCached("DictionaryIntersect").MakeGenericMethod(typeof(TKey));
                        break;
                    default:
                        throw new ZenUnreachableException();
                }

                var dict1 = expression.DictExpr1.Accept(this, parameter);
                var dict2 = expression.DictExpr2.Accept(this, parameter);

                var toImmutableDictMethod = typeof(CommonUtilities)
                    .GetMethodCached("ToImmutableDictionary")
                    .MakeGenericMethod(typeof(TKey), typeof(SetUnit));

                var immutableDictExpr1 = Expression.Call(null, toImmutableDictMethod, dict1);
                var immutableDictExpr2 = Expression.Call(null, toImmutableDictMethod, dict2);

                return Expression.Call(null, method, immutableDictExpr1, immutableDictExpr2);
            });
        }

        public Expression VisitZenSeqEmptyExpr<T>(ZenSeqEmptyExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var c = typeof(Seq<T>).GetConstructor(new Type[] { });
                return Expression.New(c);
            });
        }

        public Expression VisitZenSeqUnitExpr<T>(ZenSeqUnitExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var c = typeof(Seq<T>).GetConstructor(new Type[] { typeof(T) });
                var e = expression.ValueExpr.Accept(this, parameter);
                return Expression.New(c, new Expression[] { e });
            });
        }

        public Expression VisitZenSeqConcatExpr<T>(ZenSeqConcatExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var l = expression.SeqExpr1.Accept(this, parameter);
                var r = expression.SeqExpr2.Accept(this, parameter);
                var m = typeof(Seq<T>).GetMethod("Concat");
                return Expression.Call(l, m, new Expression[] { r });
            });
        }

        public Expression VisitZenSeqLengthExpr<T>(ZenSeqLengthExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var s = expression.SeqExpr.Accept(this, parameter);
                var m = typeof(Seq<T>).GetMethod("Length");
                var e = Expression.Call(s, m);
                var c = typeof(BigInteger).GetConstructor(new Type[] { typeof(int) });
                return Expression.New(c, e);
            });
        }

        public Expression VisitZenSeqAtExpr<T>(ZenSeqAtExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var e1 = expression.SeqExpr.Accept(this, parameter);
                var e2 = expression.IndexExpr.Accept(this, parameter);
                var m = typeof(Seq<T>).GetMethod("AtBigInteger", BindingFlags.Instance | BindingFlags.NonPublic);
                return Expression.Call(e1, m, new Expression[] { e2 });
            });
        }

        public Expression VisitZenSeqContainsExpr<T>(ZenSeqContainsExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var e1 = expression.SeqExpr.Accept(this, parameter);
                var e2 = expression.SubseqExpr.Accept(this, parameter);

                switch (expression.ContainmentType)
                {
                    case SeqContainmentType.HasPrefix:
                        return Expression.Call(e1, typeof(Seq<T>).GetMethodCached("HasPrefix"), new Expression[] { e2 });
                    case SeqContainmentType.HasSuffix:
                        return Expression.Call(e1, typeof(Seq<T>).GetMethodCached("HasSuffix"), new Expression[] { e2 });
                    case SeqContainmentType.Contains:
                        return Expression.Call(e1, typeof(Seq<T>).GetMethodCached("Contains"), new Expression[] { e2 });
                    default:
                        throw new ZenUnreachableException();
                }
            });
        }

        public Expression VisitZenSeqIndexOfExpr<T>(ZenSeqIndexOfExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var e1 = expression.SeqExpr.Accept(this, parameter);
                var e2 = expression.SubseqExpr.Accept(this, parameter);
                var e3 = expression.OffsetExpr.Accept(this, parameter);
                var m = typeof(Seq<T>).GetMethod("IndexOfBigInteger", BindingFlags.Instance | BindingFlags.NonPublic);
                return Expression.Call(e1, m, new Expression[] { e2, e3 });
            });
        }

        public Expression VisitZenSeqSliceExpr<T>(ZenSeqSliceExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var e1 = expression.SeqExpr.Accept(this, parameter);
                var e2 = expression.OffsetExpr.Accept(this, parameter);
                var e3 = expression.LengthExpr.Accept(this, parameter);
                var m = typeof(Seq<T>).GetMethod("SliceBigInteger", BindingFlags.Instance | BindingFlags.NonPublic);
                return Expression.Call(e1, m, new Expression[] { e2, e3 });
            });
        }

        private ParameterExpression FreshVariable(Type type)
        {
            return Expression.Variable(type, "v" + nextVariableId++);
        }

        public Expression VisitZenSeqReplaceFirstExpr<T>(ZenSeqReplaceFirstExpr<T> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var e1 = expression.SeqExpr.Accept(this, parameter);
                var e2 = expression.SubseqExpr.Accept(this, parameter);
                var e3 = expression.ReplaceExpr.Accept(this, parameter);
                var m = typeof(Seq<T>).GetMethod("ReplaceFirst");
                return Expression.Call(e1, m, new Expression[] { e2, e3 });
            });
        }

        public Expression VisitZenCastExpr<TKey, TValue>(ZenCastExpr<TKey, TValue> expression, ExpressionConverterEnvironment parameter)
        {
            return LookupOrCompute(expression, () =>
            {
                var e = expression.SourceExpr.Accept(this, parameter);

                if (typeof(TKey) == ReflectionUtilities.StringType)
                {
                    var m = typeof(Seq).GetMethod("FromString");
                    return Expression.Call(null, m, new Expression[] { e });
                }

                if (typeof(TKey) == ReflectionUtilities.ByteSequenceType)
                {
                    var m = typeof(Seq).GetMethod("AsString");
                    return Expression.Call(null, m, new Expression[] { e });
                }

                throw new ZenUnreachableException();
            });
        }
    }
}
