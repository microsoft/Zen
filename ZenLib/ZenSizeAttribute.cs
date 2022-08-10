// <copyright file="ZenSizeAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// Attribute annotation for fields that provides sizing information to Zen when
    /// generating symbolic values of a given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ZenSizeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSizeAttribute"/> class.
        /// </summary>
        /// <param name="depth">The depth for this field.</param>
        /// <param name="enumerationType">The exhaustiveness of the depth parameter..</param>
        public ZenSizeAttribute(int depth = -1, EnumerationType enumerationType = EnumerationType.User)
        {
            Depth = depth;
            EnumerationType = enumerationType;
        }

        /// <summary>
        /// Gets the size depth for the field or property.
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Gets a value indicating whether to generate exhaustive depth objects.
        /// </summary>
        public EnumerationType EnumerationType { get; }
    }

    /// <summary>
    /// The enumeration type of generation.
    /// </summary>
    public enum EnumerationType
    {
        /// <summary>
        /// Will generate exhaustive depths.
        /// </summary>
        Exhaustive,

        /// <summary>
        /// Will not generate exhaustive depths.
        /// </summary>
        FixedSize,

        /// <summary>
        /// Will defer to the user.
        /// </summary>
        User,
    }
}