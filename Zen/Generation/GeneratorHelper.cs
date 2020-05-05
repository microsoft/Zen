// <copyright file="GeneratorHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.Generation
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
        /// Method cache to avoid expensive reflection.
        /// </summary>
        private static Dictionary<int, MethodInfo> createObjectCache = new Dictionary<int, MethodInfo>();

        /// <summary>
        /// Tuple method from Zen.Language.
        /// </summary>
        private static MethodInfo tupleMethod = typeof(Language).GetMethod("Tuple");

        /// <summary>
        /// Tuple method from Zen.Language.
        /// </summary>
        private static MethodInfo valueTupleMethod = typeof(Language).GetMethod("ValueTuple");

        /// <summary>
        /// List method from Zen.Language.
        /// </summary>
        private static MethodInfo listMethod = typeof(Language).GetMethod("List");

        /// <summary>
        /// Name of the function used to create an object via reflection.
        /// </summary>
        private static MethodInfo createMethod = typeof(Language).GetMethod("Create");

        public static object ApplyToTuple(Func<Type, object> recurse, Type innerTypeLeft, Type innerTypeRight)
        {
            var left = recurse(innerTypeLeft);
            var right = recurse(innerTypeRight);
            var method = tupleMethod.MakeGenericMethod(innerTypeLeft, innerTypeRight);
            return method.Invoke(null, new object[] { left, right });
        }

        public static object ApplyToValueTuple(Func<Type, object> recurse, Type innerTypeLeft, Type innerTypeRight)
        {
            var left = recurse(innerTypeLeft);
            var right = recurse(innerTypeRight);
            var method = valueTupleMethod.MakeGenericMethod(innerTypeLeft, innerTypeRight);
            return method.Invoke(null, new object[] { left, right });
        }

        public static object ApplyToObject(Func<Type, object> recurse, Type objectType, Dictionary<string, Type> fields)
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
