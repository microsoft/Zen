// <copyright file="Z3TypeToSortConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Z3;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Convert a Z3 expression to a model checking symbolic value.
    /// </summary>
    internal class Z3ExprToSymbolicValueConverter :
        ITypeVisitor<SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>, Expr>
    {
        private SolverZ3 solver;

        public Z3ExprToSymbolicValueConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> ConvertExpr(Expr e, Type type)
        {
            return ReflectionUtilities.ApplyTypeVisitor(this, type, e);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitBigInteger(Expr parameter)
        {
            return new SymbolicInteger<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (IntExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitBool(Expr parameter)
        {
            return new SymbolicBool<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BoolExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitByte(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitDictionary(Type dictionaryType, Type keyType, Type valueType, Expr parameter)
        {
            return new SymbolicDict<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (ArrayExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitFixedInteger(Type intType, Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitInt(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }

        [ExcludeFromCodeCoverage]
        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitList(Type listType, Type innerType, Expr parameter)
        {
            throw new ZenException("Invalid use of list in map or set type");
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitLong(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitObject(Type objectType, SortedDictionary<string, Type> fields, Expr parameter)
        {
            var result = ImmutableSortedDictionary<string, SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>>.Empty;

            if (objectType != ReflectionUtilities.SetUnitType)
            {
                var dataTypeSort = (DatatypeSort)this.solver.TypeToSortConverter.GetSortForType(objectType);
                var fieldsAndTypes = fields.ToArray();
                for (int i = 0; i < fieldsAndTypes.Length; i++)
                {
                    var fieldName = fieldsAndTypes[i].Key;
                    var fieldAccessor = dataTypeSort.Accessors[0][i];
                    var fieldSymbolicValue = this.solver.ConvertExprToSymbolicValue(this.solver.Context.MkApp(fieldAccessor, parameter), fieldsAndTypes[i].Value);
                    result = result.Add(fieldName, fieldSymbolicValue);
                }
            }

            return new SymbolicObject<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(objectType, this.solver, result);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitSeq(Type sequenceType, Type innerType, Expr parameter)
        {
            return new SymbolicSeq<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (SeqExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitShort(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitString(Expr parameter)
        {
            return new SymbolicString<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (SeqExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitUint(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitUlong(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> VisitUshort(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this.solver, (BitVecExpr)parameter);
        }
    }
}
