// <copyright file="ISymbolicValueVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    /// <summary>
    /// Visitor class for symbolic values.
    /// </summary>
    internal interface ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal, TReturn, TParam>
    {
        /// <summary>
        /// Visit the symbolic boolean type.
        /// </summary>
        TReturn Visit(SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic bitvec type.
        /// </summary>
        TReturn Visit(SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic char type.
        /// </summary>
        TReturn Visit(SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic integer type.
        /// </summary>
        TReturn Visit(SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic integer type.
        /// </summary>
        TReturn Visit(SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic string type.
        /// </summary>
        TReturn Visit(SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic class type.
        /// </summary>
        TReturn Visit(SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic map type.
        /// </summary>
        TReturn Visit(SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic const map type.
        /// </summary>
        TReturn Visit(SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic list type.
        /// </summary>
        TReturn Visit(SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic list type.
        /// </summary>
        TReturn Visit(SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic list type.
        /// </summary>
        TReturn Visit(SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> v, TParam parameter);
    }
}
