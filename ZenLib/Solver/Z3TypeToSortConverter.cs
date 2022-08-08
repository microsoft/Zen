// <copyright file="Z3TypeToSortConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Z3;

    /// <summary>
    /// Convert a C# type into a Z3 sort.
    /// </summary>
    internal class Z3TypeToSortConverter : TypeVisitor<Sort, Unit>
    {
        /// <summary>
        /// The Z3 solver object.
        /// </summary>
        private SolverZ3 solver;

        /// <summary>
        /// A mapping from a C# type to its Z3 sort.
        /// </summary>
        private Dictionary<Type, Sort> typeToSort;

        /// <summary>
        /// The object application names.
        /// </summary>
        public ISet<string> ObjectAppNames;

        /// <summary>
        /// Creates a new instance of the <see cref="Z3TypeToSortConverter"/> class.
        /// </summary>
        /// <param name="solver">The Z3 solver.</param>
        public Z3TypeToSortConverter(SolverZ3 solver)
        {
            this.solver = solver;
            this.ObjectAppNames = new HashSet<string>();
            this.typeToSort = new Dictionary<Type, Sort>();
        }

        /// <summary>
        /// Get a sort for a given type.
        /// </summary>
        /// <param name="type">The C# type.</param>
        /// <returns>The Z3 sort.</returns>
        public Sort GetSortForType(Type type)
        {
            if (this.typeToSort.TryGetValue(type, out var sort))
            {
                return sort;
            }

            Sort result;
            if (type == ReflectionUtilities.SetUnitType)
            {
                result = this.solver.BoolSort;
            }
            else
            {
                result = this.Visit(type, Unit.Instance);
            }

            this.typeToSort[type] = result;
            return result;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitBigInteger(Unit parameter)
        {
            return this.solver.BigIntSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitReal(Unit parameter)
        {
            return this.solver.RealSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitBool(Unit parameter)
        {
            return this.solver.BoolSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitByte(Unit parameter)
        {
            return this.solver.ByteSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitChar(Unit parameter)
        {
            return this.solver.CharSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            var keySort = this.GetSortForType(keyType);
            var valueSort = this.GetSortForType(valueType);

            if (valueType != ReflectionUtilities.SetUnitType)
            {
                valueSort = this.solver.GetOrCreateOptionSort(valueSort);
            }

            return SolverZ3.Context.MkArraySort(keySort, valueSort);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        [ExcludeFromCodeCoverage]
        public override Sort VisitConstMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            throw new ZenException("Can not use a const map in another map.");
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitFixedInteger(Type intType, Unit parameter)
        {
            int size = ((dynamic)Activator.CreateInstance(intType, 0L)).Size;
            return SolverZ3.Context.MkBitVecSort((uint)size);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitInt(Unit parameter)
        {
            return this.solver.IntSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        [ExcludeFromCodeCoverage]
        public override Sort VisitList(Type listType, Type innerType, Unit parameter)
        {
            throw new ZenException("Can not use finite sequence type in another map.");
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitLong(Unit parameter)
        {
            return this.solver.LongSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectFields">The fields and their types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitObject(Type objectType, SortedDictionary<string, Type> objectFields, Unit parameter)
        {
            var fields = objectFields.ToArray();
            var fieldNames = new string[fields.Length];
            var fieldSorts = new Sort[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                fieldNames[i] = fields[i].Key;
                fieldSorts[i] = this.GetSortForType(fields[i].Value);
            }

            this.ObjectAppNames.Add(objectType.ToString());
            var objectConstructor = SolverZ3.Context.MkConstructor(objectType.ToString(), "value", fieldNames, fieldSorts);
            return SolverZ3.Context.MkDatatypeSort(objectType.ToString(), new Constructor[] { objectConstructor });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitShort(Unit parameter)
        {
            return this.solver.ShortSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitString(Unit parameter)
        {
            return this.solver.StringSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitUint(Unit parameter)
        {
            return this.solver.IntSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitUlong(Unit parameter)
        {
            return this.solver.LongSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitUshort(Unit parameter)
        {
            return this.solver.ShortSort;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A sort for the type.</returns>
        public override Sort VisitSeq(Type sequenceType, Type innerType, Unit parameter)
        {
            var valueSort = this.GetSortForType(innerType);
            return SolverZ3.Context.MkSeqSort(valueSort);
        }
    }
}
