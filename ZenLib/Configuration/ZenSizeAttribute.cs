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
        public ZenSizeAttribute(int depth = -1)
        {
            Depth = depth;
        }

        /// <summary>
        /// Gets the size depth for the field or property.
        /// </summary>
        public int Depth { get; }
    }
}