// <copyright file="Z3SymbolicValueToExprConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Z3;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Convert a symbolic value to a Z3 expression.
    /// </summary>
    internal class Z3SymbolicValueToExprConverter :
        ISymbolicValueVisitor<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, Type>
    {
        private SolverZ3 solver;

        public Expr ConvertSymbolicValue(SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> value, Type type)
        {
            return value.Accept(this, type);
        }

        public Z3SymbolicValueToExprConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        public Expr VisitSymbolicBitvec(SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr VisitSymbolicBool(SymbolicBool<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr VisitSymbolicDict(SymbolicDict<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr VisitSymbolicInteger(SymbolicInteger<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            return v.Value;
        }

        [ExcludeFromCodeCoverage]
        public Expr VisitSymbolicList(SymbolicList<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            throw new ZenException("Invalid use of list in map or set type.");
        }

        public Expr VisitSymbolicObject(SymbolicObject<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            var fieldTypes = ReflectionUtilities.GetAllFieldAndPropertyTypes(v.ObjectType);
            var fields = v.Fields.ToArray();
            var fieldNames = new string[fields.Length];
            var fieldExprs = new Expr[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                fieldNames[i] = fields[i].Key;
                fieldExprs[i] = fields[i].Value.Accept(this, fieldTypes[fieldNames[i]]);
            }

            var dataTypeSort = (DatatypeSort)this.solver.TypeToSortConverter.GetSortForType(parameter);
            var objectConstructor = dataTypeSort.Constructors[0];
            var ret = this.solver.Context.MkApp(objectConstructor, fieldExprs);
            return ret;
        }

        public Expr VisitSymbolicString(SymbolicString<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr VisitSymbolicSeq(SymbolicSeq<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> v, Type parameter)
        {
            return v.Value;
        }
    }
}
