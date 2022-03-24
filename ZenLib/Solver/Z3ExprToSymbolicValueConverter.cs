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
        ITypeVisitor<SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>, Expr>
    {
        private SolverZ3 solver;

        public Z3ExprToSymbolicValueConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> ConvertExpr(Expr e, Type type)
        {
            return ReflectionUtilities.ApplyTypeVisitor(this, type, e);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitBigInteger(Expr parameter)
        {
            return new SymbolicInteger<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (IntExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitReal(Expr parameter)
        {
            return new SymbolicReal<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (RealExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitBool(Expr parameter)
        {
            return new SymbolicBool<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BoolExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitByte(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitChar(Expr parameter)
        {
            return new SymbolicChar<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitMap(Type dictionaryType, Type keyType, Type valueType, Expr parameter)
        {
            return new SymbolicDict<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (ArrayExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitFixedInteger(Type intType, Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitInt(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }

        [ExcludeFromCodeCoverage]
        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitList(Type listType, Type innerType, Expr parameter)
        {
            throw new ZenException("Invalid use of list in map or set type");
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitLong(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitObject(Type objectType, SortedDictionary<string, Type> fields, Expr parameter)
        {
            var result = ImmutableSortedDictionary<string, SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>>.Empty;

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

            return new SymbolicObject<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(objectType, this.solver, result);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitSeq(Type sequenceType, Type innerType, Expr parameter)
        {
            return new SymbolicSeq<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (SeqExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitShort(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitString(Expr parameter)
        {
            return new SymbolicString<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (SeqExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitUint(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitUlong(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> VisitUshort(Expr parameter)
        {
            return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>(this.solver, (BitVecExpr)parameter);
        }
    }
}
