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
    internal class Z3SymbolicValueToExprConverter : ISymbolicValueVisitor<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr, Expr, Type>
    {
        private SolverZ3 solver;

        public Expr ConvertSymbolicValue(SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> value, Type type)
        {
            return value.Accept(this, type);
        }

        public Z3SymbolicValueToExprConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        public Expr Visit(SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr Visit(SymbolicChar<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr Visit(SymbolicBool<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr Visit(SymbolicMap<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }

        [ExcludeFromCodeCoverage]
        public Expr Visit(SymbolicConstMap<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            throw new ZenException("Invalid use of const map in map or set type.");
        }

        public Expr Visit(SymbolicInteger<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr Visit(SymbolicReal<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }

        [ExcludeFromCodeCoverage]
        public Expr Visit(SymbolicList<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            throw new ZenException("Invalid use of list in map or set type.");
        }

        public Expr Visit(SymbolicObject<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
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
            var ret = SolverZ3.Context.MkApp(objectConstructor, fieldExprs);
            return ret;
        }

        public Expr Visit(SymbolicString<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }

        public Expr Visit(SymbolicSeq<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> v, Type parameter)
        {
            return v.Value;
        }
    }
}
