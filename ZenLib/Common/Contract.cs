// <copyright file="Contract.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// A basic contract/assertion class.
    /// </summary>
    internal static class Contract
    {
        /// <summary>
        /// Validate that an argument is true.
        /// </summary>
        /// <param name="obj">The argument.</param>
        /// <param name="msg">An optional message parameter.</param>
        public static void Assert(bool obj, string msg = "Assertion failed")
        {
            if (!obj)
            {
                throw new ZenException(msg);
            }
        }

        /// <summary>
        /// Validate that an argument is not null.
        /// </summary>
        /// <param name="obj">The argument.</param>
        public static void AssertNotNull(object obj)
        {
            if (obj is null)
            {
                throw new ZenException($"Invalid null argument");
            }
        }

        /// <summary>
        /// Validates whether the field or property exists.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fieldType">The field type.</param>
        /// <param name="fieldOrPropertyName">The field or property name.</param>
        public static void AssertFieldOrProperty(Type objectType, Type fieldType, string fieldOrPropertyName)
        {
            var p = objectType.GetPropertyCached(fieldOrPropertyName);

            if (p != null && p.PropertyType != fieldType)
            {
                throw new ZenException($"Field or property {fieldOrPropertyName} type mismatch with {fieldType} for object with type {objectType}.");
            }

            var f = objectType.GetFieldCached(fieldOrPropertyName);

            if (f != null && f.FieldType != fieldType)
            {
                throw new ZenException($"Field or property {fieldOrPropertyName} type mismatch with {fieldType} for object with type {objectType}.");
            }

            if (p == null && f == null)
            {
                throw new ZenException($"Invalid field or property {fieldOrPropertyName} for object with type {objectType}");
            }
        }

        /// <summary>
        ///     Reports a null error in a conversion from a constant to a Zen value.
        /// </summary>
        /// <param name="obj">The object that may be null.</param>
        /// <param name="where">Description of where the error occurs.</param>
        /// <param name="type">The containing type.</param>
        public static void AssertNullConversion(object obj, string where, Type type)
        {
            if (obj is null)
            {
                throw new ZenException($"Null constant in {where} of type {type} can not be converted to a Zen value.");
            }
        }
    }
}
