// <copyright file="Z3TypeToSortConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using Microsoft.Z3;

    /// <summary>
    /// Convert a C# type into a Z3 sort.
    /// </summary>
    internal class Z3TypeToSortConverter : ITypeVisitor<Sort, Unit>
    {
        private SolverZ3 solver;

        private Dictionary<Type, Sort> typeToSort;

        public ISet<string> ObjectAppNames;

        public Z3TypeToSortConverter(SolverZ3 solver)
        {
            this.solver = solver;
            this.ObjectAppNames = new HashSet<string>();
            this.typeToSort = new Dictionary<Type, Sort>();
            this.typeToSort[typeof(bool)] = this.solver.BoolSort;
            this.typeToSort[typeof(byte)] = this.solver.ByteSort;
            this.typeToSort[typeof(short)] = this.solver.ShortSort;
            this.typeToSort[typeof(ushort)] = this.solver.ShortSort;
            this.typeToSort[typeof(int)] = this.solver.IntSort;
            this.typeToSort[typeof(uint)] = this.solver.IntSort;
            this.typeToSort[typeof(long)] = this.solver.LongSort;
            this.typeToSort[typeof(ulong)] = this.solver.LongSort;
            this.typeToSort[typeof(BigInteger)] = this.solver.BigIntSort;
            this.typeToSort[typeof(string)] = this.solver.StringSort;
        }

        public Sort VisitBigInteger(Unit parameter)
        {
            return this.solver.BigIntSort;
        }

        public Sort VisitBool(Unit parameter)
        {
            return this.solver.BoolSort;
        }

        public Sort VisitByte(Unit parameter)
        {
            return this.solver.ByteSort;
        }

        public Sort VisitDictionary(Func<Type, Unit, Sort> recurse, Type dictionaryType, Type keyType, Type valueType, Unit parameter)
        {
            if (this.typeToSort.TryGetValue(dictionaryType, out var sort))
            {
                return sort;
            }

            var keySort = this.solver.GetSortForType(keyType);
            var valueSort = this.solver.GetSortForType(valueType);

            if (valueType != ReflectionUtilities.SetUnitType)
            {
                valueSort = this.solver.GetOrCreateOptionSort(valueSort);
            }

            var dictionarySort = this.solver.Context.MkArraySort(keySort, valueSort);
            this.typeToSort[dictionaryType] = dictionarySort;
            return dictionarySort;
        }

        public Sort VisitFixedInteger(Type intType, Unit parameter)
        {
            if (this.typeToSort.TryGetValue(intType, out var sort))
            {
                return sort;
            }

            int size = ((dynamic)Activator.CreateInstance(intType, 0L)).Size;
            var bitvecSort = this.solver.Context.MkBitVecSort((uint)size);
            this.typeToSort[intType] = bitvecSort;
            return bitvecSort;
        }

        public Sort VisitInt(Unit parameter)
        {
            return this.solver.IntSort;
        }

        [ExcludeFromCodeCoverage]
        public Sort VisitList(Func<Type, Unit, Sort> recurse, Type listType, Type innerType, Unit parameter)
        {
            throw new ZenException("Can not use finite sequence type in another map.");
        }

        public Sort VisitLong(Unit parameter)
        {
            return this.solver.LongSort;
        }

        public Sort VisitObject(Func<Type, Unit, Sort> recurse, Type objectType, SortedDictionary<string, Type> objectFields, Unit parameter)
        {
            if (this.typeToSort.TryGetValue(objectType, out var sort))
            {
                return sort;
            }

            var fields = objectFields.ToArray();
            var fieldNames = new string[fields.Length];
            var fieldSorts = new Sort[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                fieldNames[i] = fields[i].Key;
                fieldSorts[i] = this.solver.GetSortForType(fields[i].Value);
            }

            var objectConstructor = this.solver.Context.MkConstructor(objectType.ToString(), "value", fieldNames, fieldSorts);
            var objectSort = this.solver.Context.MkDatatypeSort(objectType.ToString(), new Constructor[] { objectConstructor });
            this.ObjectAppNames.Add(objectType.ToString());
            this.typeToSort[objectType] = objectSort;
            return objectSort;
        }

        public Sort VisitShort(Unit parameter)
        {
            return this.solver.ShortSort;
        }

        public Sort VisitString(Unit parameter)
        {
            return this.solver.StringSort;
        }

        public Sort VisitUint(Unit parameter)
        {
            return this.solver.IntSort;
        }

        public Sort VisitUlong(Unit parameter)
        {
            return this.solver.LongSort;
        }

        public Sort VisitUshort(Unit parameter)
        {
            return this.solver.ShortSort;
        }
    }
}
