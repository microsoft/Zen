// <copyright file="ISymbolicValueVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    /// <summary>
    /// Visitor class for symbolic values.
    /// </summary>
    internal interface ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TReturn, TParam>
    {
        /// <summary>
        /// Visit the symbolic boolean type.
        /// </summary>
        TReturn VisitSymbolicBool(SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic bitvec type.
        /// </summary>
        TReturn VisitSymbolicBitvec(SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic integer type.
        /// </summary>
        TReturn VisitSymbolicInteger(SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic string type.
        /// </summary>
        TReturn VisitSymbolicString(SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic class type.
        /// </summary>
        TReturn VisitSymbolicObject(SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic dict type.
        /// </summary>
        TReturn VisitSymbolicDict(SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic list type.
        /// </summary>
        TReturn VisitSymbolicList(SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);

        /// <summary>
        /// Visit the symbolic list type.
        /// </summary>
        TReturn VisitSymbolicSeq(SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> v, TParam parameter);
    }
}
