// <copyright file="ZenDepthConfigurationAttribute.cs" company="Microsoft">
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
    public class ZenDepthConfigurationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZenDepthConfigurationAttribute"/> class.
        /// </summary>
        /// <param name="depth">The depth for this field.</param>
        /// <param name="enumerationType">The exhaustiveness of the depth parameter..</param>
        /// <param name="recursive">Whether to apply these setting recursively.</param>
        public ZenDepthConfigurationAttribute(int depth = -1, EnumerationType enumerationType = EnumerationType.User, bool recursive = false)
        {
            this.Depth = depth;
            this.EnumerationType = enumerationType;
            this.Recursive = recursive;
        }

        /// <summary>
        /// Gets the size depth for the field or property.
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Gets a value indicating whether to generate exhaustive depth objects.
        /// </summary>
        public EnumerationType EnumerationType { get; }

        /// <summary>
        /// Gets a value indicating whether to apply these settings recursively.
        /// </summary>
        public bool Recursive { get; }
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