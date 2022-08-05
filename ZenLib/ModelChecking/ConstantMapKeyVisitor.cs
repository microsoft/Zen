// <copyright file="ConstantMapKeyVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to trace the set of constants that can be used as keys for ConstMap
    /// for each Arbitrary expression.
    /// </summary>
    internal sealed class ConstantMapKeyVisitor : ZenExprActionVisitor
    {
        /// <summary>
        /// Mapping from each ConstMap type to the set of constants used.
        /// </summary>
        public Dictionary<Type, ISet<object>> Constants;

        /// <summary>
        /// The value visitor.
        /// </summary>
        private ConstantMapValueVisitor valueVisitor;

        /// <summary>
        /// Creates a new instance of the <see cref="ConstantMapKeyVisitor"/> class.
        /// </summary>
        /// <param name="arguments"></param>
        public ConstantMapKeyVisitor(Dictionary<long, object> arguments = null) : base(arguments)
        {
            this.Constants = new Dictionary<Type, ISet<object>>();
            this.valueVisitor = new ConstantMapValueVisitor(this);
        }

        /// <summary>
        /// Computes the variable ordering requirements for the expression.
        /// </summary>
        /// <param name="expr">The Zen expression.</param>
        /// <returns></returns>
        public Dictionary<Type, ISet<object>> Compute<T>(Zen<T> expr)
        {
            this.VisitCached(expr);
            return this.Constants;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public override void Visit<TKey, TValue>(ZenConstMapSetExpr<TKey, TValue> expression)
        {
            this.VisitCached(expression.MapExpr);
            this.VisitCached(expression.ValueExpr);
            AddConstant<TKey, TValue>(expression.Key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public override void Visit<TKey, TValue>(ZenConstMapGetExpr<TKey, TValue> expression)
        {
            this.VisitCached(expression.MapExpr);
            AddConstant<TKey, TValue>(expression.Key);
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The set of ConstMap variables.</returns>
        public override void Visit<T>(ZenConstantExpr<T> expression)
        {
            ReflectionUtilities.ApplyTypeVisitor(this.valueVisitor, typeof(T), expression.Value);
        }

        /// <summary>
        /// Add a constant to the constants.
        /// </summary>
        /// <param name="constant">The constant.</param>
        private void AddConstant<TKey, TValue>(object constant)
        {
            var type = typeof(ConstMap<TKey, TValue>);
            if (!this.Constants.TryGetValue(type, out var result))
            {
                result = new HashSet<object>();
                this.Constants[type] = result;
            }

            result.Add(constant);
        }
    }

    /// <summary>
    /// Class to walk over a constant value and pull out the ConstMap key values.
    /// </summary>
    internal sealed class ConstantMapValueVisitor : ITypeVisitor<Unit, object>
    {
        /// <summary>
        /// The key visitor.
        /// </summary>
        private ConstantMapKeyVisitor keyVisitor;

        /// <summary>
        /// Creates a new instance of the <see cref="ConstantMapValueVisitor"/> class.
        /// </summary>
        /// <param name="keyVisitor">The key visitor.</param>
        public ConstantMapValueVisitor(ConstantMapKeyVisitor keyVisitor)
        {
            this.keyVisitor = keyVisitor;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitBigInteger(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitBool(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitByte(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitChar(object parameter)
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
        public Unit VisitConstMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            var consts = this.GetOrCreate(mapType);
            dynamic value = parameter;
            foreach (var kv in value.Values)
            {
                consts.Add(kv.Key);
                ReflectionUtilities.ApplyTypeVisitor(this, keyType, kv.Key);
                ReflectionUtilities.ApplyTypeVisitor(this, valueType, kv.Value);
            }

            return Unit.Instance;
        }

        public Unit VisitFixedInteger(Type intType, object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitInt(object parameter)
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
        public Unit VisitList(Type listType, Type innerType, object parameter)
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
        public Unit VisitLong(object parameter)
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
        public Unit VisitMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            dynamic value = parameter;
            foreach (var kv in value.Values)
            {
                ReflectionUtilities.ApplyTypeVisitor(this, keyType, kv.Key);
                ReflectionUtilities.ApplyTypeVisitor(this, valueType, kv.Value);
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
        public Unit VisitObject(Type objectType, SortedDictionary<string, Type> fields, object parameter)
        {
            foreach (var field in ReflectionUtilities.GetAllFields(objectType))
            {
                ReflectionUtilities.ApplyTypeVisitor(this, field.FieldType, field.GetValue(parameter));
            }

            foreach (var property in ReflectionUtilities.GetAllProperties(objectType))
            {
                ReflectionUtilities.ApplyTypeVisitor(this, property.PropertyType, property.GetValue(parameter));
            }

            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitReal(object parameter)
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
        public Unit VisitSeq(Type sequenceType, Type innerType, object parameter)
        {
            dynamic value = parameter;
            foreach (var v in value.Values)
            {
                ReflectionUtilities.ApplyTypeVisitor(this, innerType, v);
            }

            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitShort(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitString(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitUint(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitUlong(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>No value.</returns>
        public Unit VisitUshort(object parameter)
        {
            return Unit.Instance;
        }

        /// <summary>
        /// Get or add the constants for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The set of constants.</returns>
        private ISet<object> GetOrCreate(Type type)
        {
            if (!this.keyVisitor.Constants.TryGetValue(type, out var consts))
            {
                consts = new HashSet<object>();
                this.keyVisitor.Constants.Add(type, consts);
            }

            return consts;
        }
    }
}
