// <copyright file="GeneratorHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class to help generate objects from types.
    /// </summary>
    internal static class GeneratorHelper
    {
        /// <summary>
        /// List method from Zen.Language.
        /// </summary>
        private static MethodInfo listMethod = typeof(Basic).GetMethod("List", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Name of the function used to create an object via reflection.
        /// </summary>
        private static MethodInfo createMethod = typeof(Basic).GetMethod("Create");

        public static object ApplyToObject(Func<Type, object> recurse, Type objectType, SortedDictionary<string, Type> fields)
        {
            var asList = fields.ToArray();

            var method = createMethod.MakeGenericMethod(objectType);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                args[i] = (asList[i].Key, recurse(asList[i].Value));
            }

            return method.Invoke(null, new object[] { args });
        }

        public static object ApplyToList(Func<Type, object> recurse, Type innerType, int size)
        {
            var method = listMethod.MakeGenericMethod(innerType);

            var args = new object[size];
            for (int i = 0; i < size; i++)
            {
                var arg = recurse(innerType);
                args[i] = arg;
            }

            var zenType = typeof(Zen<>).MakeGenericType(innerType);
            var finalArgs = Array.CreateInstance(zenType, args.Length);
            Array.Copy(args, finalArgs, args.Length);
            return method.Invoke(null, new object[] { finalArgs });
        }
    }
}
