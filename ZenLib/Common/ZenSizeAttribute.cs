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
        /// <param name="exhaustiveDepth">Whether to exit on the first failure.</param>
        /// <param name="recursive">Whether to apply these setting recursively.</param>
        public ZenSizeAttribute(int? depth = null, bool? exhaustiveDepth = null, bool recursive = false)
        {
            this.Depth = depth;
            this.ExhaustiveDepth = exhaustiveDepth;
            this.Recursive = recursive;
        }

        /// <summary>
        /// Gets the size depth for the field or property.
        /// </summary>
        public int? Depth { get; }

        /// <summary>
        /// Gets a value indicating whether to generate exhaustive depth objects.
        /// </summary>
        public bool? ExhaustiveDepth { get; }

        /// <summary>
        /// Gets a value indicating whether to apply these settings recursively.
        /// </summary>
        public bool Recursive { get; }
    }
}