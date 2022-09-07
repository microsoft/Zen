// <copyright file="ISolver.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Numerics;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Solver interface for a model checker backend.
    /// </summary>
    /// <typeparam name="TModel">The model type..</typeparam>
    /// <typeparam name="TVariable">The variable type.</typeparam>
    /// <typeparam name="TBool">The boolean expression type.</typeparam>
    /// <typeparam name="TBitvec">The finite integer type.</typeparam>
    /// <typeparam name="TInteger">The integer type.</typeparam>
    /// <typeparam name="TSeq">The sequence type.</typeparam>
    /// <typeparam name="TArray">The array type.</typeparam>
    /// <typeparam name="TChar">The character type.</typeparam>
    /// <typeparam name="TReal">The real type.</typeparam>
    internal interface ISolver<TModel, TVariable, TBool, TBitvec, TInteger, TSeq, TArray, TChar, TReal>
    {
        /// <summary>
        /// The false expression.
        /// </summary>
        /// <returns>The expression.</returns>
        TBool False();

        /// <summary>
        /// The true expression.
        /// </summary>
        /// <returns>The expression.</returns>
        TBool True();

        /// <summary>
        /// Create a new boolean expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TBool) CreateBoolVar(object e);

        /// <summary>
        /// Create a new byte expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TBitvec) CreateByteVar(object e);

        /// <summary>
        /// Create a byte constant.
        /// </summary>
        /// <returns></returns>
        TBitvec CreateByteConst(byte b);

        /// <summary>
        /// Create a new char expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TChar) CreateCharVar(object e);

        /// <summary>
        /// Create a char constant.
        /// </summary>
        /// <returns></returns>
        TChar CreateCharConst(char c);

        /// <summary>
        /// Create a char constant.
        /// </summary>
        /// <returns></returns>
        TSeq CreateStringConst(string s);

        /// <summary>
        /// Create a new short expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TBitvec) CreateShortVar(object e);

        /// <summary>
        /// Create a short constant.
        /// </summary>
        /// <returns></returns>
        TBitvec CreateShortConst(short s);

        /// <summary>
        /// Create a new int expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TBitvec) CreateIntVar(object e);

        /// <summary>
        /// Create an integer constant.
        /// </summary>
        /// <returns></returns>
        TBitvec CreateIntConst(int i);

        /// <summary>
        /// Create a new long expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TBitvec) CreateLongVar(object e);

        /// <summary>
        /// Create a long constant.
        /// </summary>
        /// <returns></returns>
        TBitvec CreateLongConst(long l);

        /// <summary>
        /// Create a bitvector variable.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <param name="size">The size of the bitvector.</param>
        /// <returns>The expression.</returns>
        (TVariable, TBitvec) CreateBitvecVar(object e, uint size);

        /// <summary>
        /// Create a bitvector constant.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <returns>A bitvector expression.</returns>
        TBitvec CreateBitvecConst(bool[] bits);

        /// <summary>
        /// Create a new big integer expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TInteger) CreateBigIntegerVar(object e);

        /// <summary>
        /// Create a big integer constant.
        /// </summary>
        /// <returns></returns>
        TInteger CreateBigIntegerConst(BigInteger b);

        /// <summary>
        /// Create a new real expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TReal) CreateRealVar(object e);

        /// <summary>
        /// Create a real constant.
        /// </summary>
        /// <returns></returns>
        TReal CreateRealConst(Real r);

        /// <summary>
        /// Create a new sequence expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TSeq) CreateSeqVar(object e);

        /// <summary>
        /// Create a new dictionary expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        (TVariable, TArray) CreateDictVar(object e);

        /// <summary>
        /// The 'And' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool And(TBool x, TBool y);

        /// <summary>
        /// The 'Iff' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Iff(TBool x, TBool y);

        /// <summary>
        /// The 'Or' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Or(TBool x, TBool y);

        /// <summary>
        /// The 'Not' of an expression.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <returns></returns>
        TBool Not(TBool x);

        /// <summary>
        /// The 'BitwiseAnd' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBitvec BitwiseAnd(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'BitwiseOr' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBitvec BitwiseOr(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'BitwiseXor' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBitvec BitwiseXor(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'BitwiseNot' of an expression.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <returns></returns>
        TBitvec BitwiseNot(TBitvec x);

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBitvec Add(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TInteger Add(TInteger x, TInteger y);

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TReal Add(TReal x, TReal y);

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBitvec Subtract(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TInteger Subtract(TInteger x, TInteger y);

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TReal Subtract(TReal x, TReal y);

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBitvec Multiply(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TInteger Multiply(TInteger x, TInteger y);

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TReal Multiply(TReal x, TReal y);

        /// <summary>
        /// The 'Resize' of a bitvec expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="sourceSize">The source bitwidth.</param>
        /// <param name="targetSize">The target bitwidth.</param>
        /// <returns></returns>
        TBitvec Resize(TBitvec x, int sourceSize, int targetSize);

        /// <summary>
        /// The 'EmptySeq' for a given type.
        /// </summary>
        /// <param name="type">The type of the sequence.</param>
        /// <returns></returns>
        TSeq SeqEmpty(Type type);

        /// <summary>
        /// The 'SeqUnit' for a given type.
        /// </summary>
        /// <param name="valueExpr">The value expression.</param>
        /// <param name="type">The type of the sequence.</param>
        /// <returns></returns>
        TSeq SeqUnit(SymbolicValue<TModel, TVariable, TBool, TBitvec, TInteger, TSeq, TArray, TChar, TReal> valueExpr, Type type);

        /// <summary>
        /// The 'Concat' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TSeq SeqConcat(TSeq x, TSeq y);

        /// <summary>
        /// The 'PrefixOf' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        TBool SeqPrefixOf(TSeq x, TSeq y);

        /// <summary>
        /// The 'SuffixOf' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        TBool SeqSuffixOf(TSeq x, TSeq y);

        /// <summary>
        /// The 'Contains' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        TBool SeqContains(TSeq x, TSeq y);

        /// <summary>
        /// The seq 'Replace' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <param name="z">The replacement expression.</param>
        /// <returns></returns>
        TSeq SeqReplaceFirst(TSeq x, TSeq y, TSeq z);

        /// <summary>
        /// The seq 'Slice' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The offset expression.</param>
        /// <param name="z">The length expression.</param>
        /// <returns></returns>
        TSeq SeqSlice(TSeq x, TInteger y, TInteger z);

        /// <summary>
        /// The seq 'At' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The index expression.</param>
        /// <returns></returns>
        TSeq SeqAt(TSeq x, TInteger y);

        /// <summary>
        /// The seq 'At' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The index expression.</param>
        /// <returns></returns>
        object SeqNth(TSeq x, TInteger y);

        /// <summary>
        /// The seq 'Length' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <returns></returns>
        TInteger SeqLength(TSeq x);

        /// <summary>
        /// The seq 'IndexOf' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <param name="z">The offset expression.</param>
        /// <returns></returns>
        TInteger SeqIndexOf(TSeq x, TSeq y, TInteger z);

        /// <summary>
        /// The seq 'Regex' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The regex expression.</param>
        /// <returns></returns>
        TBool SeqRegex<T>(TSeq x, Regex<T> y);

        /// <summary>
        /// The empty dictionary.
        /// </summary>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        TArray DictEmpty(Type keyType, Type valueType);

        /// <summary>
        /// The result of setting a key to a value for an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="valueExpr">The value expression.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        TArray DictSet(TArray arrayExpr, SymbolicValue<TModel, TVariable, TBool, TBitvec, TInteger, TSeq, TArray, TChar, TReal> keyExpr, SymbolicValue<TModel, TVariable, TBool, TBitvec, TInteger, TSeq, TArray, TChar, TReal> valueExpr, Type keyType, Type valueType);

        /// <summary>
        /// The result of deleting a key from an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        TArray DictDelete(TArray arrayExpr, SymbolicValue<TModel, TVariable, TBool, TBitvec, TInteger, TSeq, TArray, TChar, TReal> keyExpr, Type keyType, Type valueType);

        /// <summary>
        /// The result of getting a value for a key from an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        (TBool, object) DictGet(TArray arrayExpr, SymbolicValue<TModel, TVariable, TBool, TBitvec, TInteger, TSeq, TArray, TChar, TReal> keyExpr, Type keyType, Type valueType);

        /// <summary>
        /// The result of unioning two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        TArray DictUnion(TArray arrayExpr1, TArray arrayExpr2);

        /// <summary>
        /// The result of intersecting two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        TArray DictIntersect(TArray arrayExpr1, TArray arrayExpr2);

        /// <summary>
        /// The result of differencing two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        TArray DictDifference(TArray arrayExpr1, TArray arrayExpr2);

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Eq(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Eq(TInteger x, TInteger y);

        /// <summary>
        /// The 'Equal' of two reals.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Eq(TReal x, TReal y);

        /// <summary>
        /// The 'Equal' of two seqs.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Eq(TSeq x, TSeq y);

        /// <summary>
        /// The 'Equal' of two arrays.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Eq(TArray x, TArray y);

        /// <summary>
        /// The 'Equal' of two characters.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool Eq(TChar x, TChar y);

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThanOrEqual(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThanOrEqual(TInteger x, TInteger y);

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThanOrEqual(TReal x, TReal y);

        /// <summary>
        /// The 'LessThanOrEqualSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThanOrEqualSigned(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThan(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThan(TInteger x, TInteger y);

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThan(TReal x, TReal y);

        /// <summary>
        /// The 'LessThanSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool LessThanSigned(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThanOrEqual(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThanOrEqual(TInteger x, TInteger y);

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThanOrEqual(TReal x, TReal y);

        /// <summary>
        /// The 'GreaterThanOrEqualSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThanOrEqualSigned(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThan(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThan(TInteger x, TInteger y);

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThan(TReal x, TReal y);

        /// <summary>
        /// The 'GreaterThanSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        TBool GreaterThanSigned(TBitvec x, TBitvec y);

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        TBool Ite(TBool g, TBool t, TBool f);

        /// <summary>
        /// The 'Ite' of a guard and two integers.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        TBitvec Ite(TBool g, TBitvec t, TBitvec f);

        /// <summary>
        /// The 'Ite' of a guard and two integers.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        TInteger Ite(TBool g, TInteger t, TInteger f);

        /// <summary>
        /// The 'Ite' of a guard and two integers.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        TSeq Ite(TBool g, TSeq t, TSeq f);

        /// <summary>
        /// The 'Ite' of a guard and two arrays.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        TArray Ite(TBool g, TArray t, TArray f);

        /// <summary>
        /// The 'Ite' of a guard and two characters.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        TChar Ite(TBool g, TChar t, TChar f);

        /// <summary>
        /// The 'Ite' of a guard and two reals.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        TReal Ite(TBool g, TReal t, TReal f);

        /// <summary>
        /// Check whether a boolean expression is satisfiable.
        /// </summary>
        /// <param name="x">The expression.</param>
        /// <returns>A model, if satisfiable.</returns>
        TModel Solve(TBool x);

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        TModel Maximize(TBitvec objective, TBool subjectTo);

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        TModel Maximize(TInteger objective, TBool subjectTo);

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        TModel Maximize(TReal objective, TBool subjectTo);

        /// <summary>
        /// Minimize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The minimize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        TModel Minimize(TBitvec objective, TBool subjectTo);

        /// <summary>
        /// Minimize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The minimize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        TModel Minimize(TInteger objective, TBool subjectTo);

        /// <summary>
        /// Minimize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The minimize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        TModel Minimize(TReal objective, TBool subjectTo);

        /// <summary>
        /// Get the value for a variable in a model.
        /// </summary>
        /// <param name="m">The model.</param>
        /// <param name="v">The variable.</param>
        /// <param name="type">The C# type to coerce the result to.</param>
        /// <returns>The value.</returns>
        object Get(TModel m, TVariable v, Type type);

        /// <summary>
        /// Convert a value back into a symbolic value.
        /// </summary>
        /// <param name="e">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        SymbolicValue<TModel, TVariable, TBool, TBitvec, TInteger, TSeq, TArray, TChar, TReal> ConvertExprToSymbolicValue(object e, Type type);
    }
}
