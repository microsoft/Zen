﻿// <copyright file="CMapKeyVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to walk over a constant value and pull out the CMap key values.
    /// </summary>
    internal sealed class CMapValueVisitor : TypeVisitor<Unit, object>
    {
        /// <summary>
        /// The key visitor.
        /// </summary>
        private CMapKeyVisitor keyVisitor;

        /// <summary>
        /// Creates a new instance of the <see cref="CMapValueVisitor"/> class.
        /// </summary>
        /// <param name="keyVisitor">The key visitor.</param>
        public CMapValueVisitor(CMapKeyVisitor keyVisitor)
        {
            this.keyVisitor = keyVisitor;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitBigInteger(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitBool(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitByte(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitChar(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitConstMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            var consts = this.keyVisitor.GetOrCreate(mapType);
            dynamic value = parameter;
            foreach (var kv in value.Values)
            {
                consts.Add(kv.Key);
                this.Visit(keyType, kv.Key);
                this.Visit(valueType, kv.Value);
            }

            return Unit.Instance;
        }

        public override Unit VisitFixedInteger(Type intType, object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitInt(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitList(Type listType, Type innerType, object parameter)
        {
            // we don't support these in lists anyway
            /* dynamic value = parameter;
            foreach (var v in value.Values)
            {
                ReflectionUtilities.ApplyTypeVisitor(this, innerType, v);
            } */

            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitLong(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            dynamic value = parameter;
            foreach (var kv in value.Values)
            {
                this.Visit(keyType, kv.Key);
                this.Visit(valueType, kv.Value);
            }

            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The object fields.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitObject(Type objectType, SortedDictionary<string, Type> fields, object parameter)
        {
            foreach (var field in ReflectionUtilities.GetAllFields(objectType))
            {
                this.Visit(field.FieldType, field.GetValue(parameter));
            }

            foreach (var property in ReflectionUtilities.GetAllProperties(objectType))
            {
                this.Visit(property.PropertyType, property.GetValue(parameter));
            }

            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitReal(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitSeq(Type sequenceType, Type innerType, object parameter)
        {
            dynamic value = parameter;
            foreach (var v in value.Values)
            {
                this.Visit(innerType, v);
            }

            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitShort(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitString(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitUint(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitUlong(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public override Unit VisitUshort(object parameter)
        {
            return Unit.Instance;
        }
    }
}
