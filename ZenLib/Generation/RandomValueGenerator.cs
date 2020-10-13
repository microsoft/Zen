// <copyright file="RandomValueGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    /// <summary>
    /// Class to help generate a random value for a type.
    /// </summary>
    internal sealed class RandomValueGenerator : ITypeVisitor<object>
    {
        /// <summary>
        /// None method for options.
        /// </summary>
        private static MethodInfo optionNoneMethod = typeof(Option).GetMethod("None");

        /// <summary>
        /// Some method for options.
        /// </summary>
        private static MethodInfo optionSomeMethod = typeof(Option).GetMethod("Some");

        /// <summary>
        /// Random number generator.
        /// </summary>
        private Random random;

        /// <summary>
        /// Upper bound on the size of structures.
        /// </summary>
        private int sizeBound;

        /// <summary>
        /// Collection of magic constants to draw from.
        /// </summary>
        internal Dictionary<Type, ISet<object>> magicConstants;

        /// <summary>
        /// Creates a new instance of a random value generator.
        /// </summary>
        /// <param name="magicConstants">Magic constants to use.</param>
        /// <param name="sizeBound">The uppoer bound on the size of structures.</param>
        public RandomValueGenerator(Dictionary<Type, ISet<object>> magicConstants = null, int sizeBound = 20)
        {
            this.random = new Random();
            this.sizeBound = sizeBound;
            this.magicConstants = magicConstants;
        }

        public object VisitBool()
        {
            return RandomInt() % 2 == 0;
        }

        public object VisitByte()
        {
            return this.GenerateRandom(typeof(byte), () => (byte)RandomInt());
        }

        public object VisitInt()
        {
            return this.GenerateRandom(typeof(int), () => RandomInt());
        }

        public object VisitList(Func<Type, object> recurse, Type listType, Type innerType)
        {
            return this.GenerateRandom(listType, () =>
            {
                var size = RandomInt() % this.sizeBound;

                var type = typeof(List<>).MakeGenericType(innerType);
                var addMethod = type.GetMethodCached("Add");

                var constructor = type.GetConstructor(new Type[] { });
                var list = constructor.Invoke(CommonUtilities.EmptyArray);

                for (int i = 0; i < size; i++)
                {
                    var item = recurse(innerType);
                    addMethod.Invoke(list, new object[] { item });
                }

                return list;
            });
        }

        public object VisitDictionary(Func<Type, object> recurse, Type dictType, Type keyType, Type valueType)
        {
            return this.GenerateRandom(dictType, () =>
            {
                var size = RandomInt() % this.sizeBound;

                var type = typeof(Dictionary<,>).MakeGenericType(new Type[] { keyType, valueType });
                var addMethod = type.GetMethodCached("set_Item");

                var constructor = type.GetConstructor(new Type[] { });
                var dictionary = constructor.Invoke(CommonUtilities.EmptyArray);

                for (int i = 0; i < size; i++)
                {
                    var key = recurse(keyType);
                    var value = recurse(valueType);
                    addMethod.Invoke(dictionary, new object[] { key, value });
                }

                return dictionary;
            });
        }

        public object VisitLong()
        {
            return this.GenerateRandom(typeof(long), () => ((long)RandomInt() << 32) | (long)RandomInt());
        }

        public object VisitBigInteger()
        {
            return this.GenerateRandom(typeof(BigInteger), () => new BigInteger(RandomInt()));
        }

        public object VisitObject(Func<Type, object> recurse, Type objectType, SortedDictionary<string, Type> fields)
        {
            return this.GenerateRandom(objectType, () =>
            {
                var asList = fields.ToArray();

                var fieldNames = new string[asList.Length];
                var values = new object[asList.Length];

                for (int i = 0; i < asList.Length; i++)
                {
                    fieldNames[i] = asList[i].Key;
                    values[i] = recurse(asList[i].Value);
                }

                return ReflectionUtilities.CreateInstance(objectType, fieldNames, values);
            });
        }

        public object VisitOption(Func<Type, object> recurse, Type optionType, Type innerType)
        {
            return this.GenerateRandom(optionType, () =>
            {
                if (RandomInt() % 2 == 0)
                {
                    var noneMethod = optionNoneMethod.MakeGenericMethod(innerType);
                    return noneMethod.Invoke(null, CommonUtilities.EmptyArray);
                }

                var value = recurse(innerType);
                var someMethod = optionSomeMethod.MakeGenericMethod(innerType);
                return someMethod.Invoke(null, new object[] { value });
            });
        }

        public object VisitShort()
        {
            return this.GenerateRandom(typeof(short), () => (short)RandomInt());
        }

        public object VisitTuple(Func<Type, object> recurse, Type tupleType, Type innerTypeLeft, Type innerTypeRight)
        {
            return this.GenerateRandom(tupleType, () =>
            {
                var constructor = tupleType.GetConstructor(new Type[] { innerTypeLeft, innerTypeRight });
                return constructor.Invoke(new object[] { recurse(innerTypeLeft), recurse(innerTypeRight) });
            });
        }

        public object VisitValueTuple(Func<Type, object> recurse, Type tupleType, Type innerTypeLeft, Type innerTypeRight)
        {
            return this.GenerateRandom(tupleType, () =>
            {
                var constructor = tupleType.GetConstructor(new Type[] { innerTypeLeft, innerTypeRight });
                return constructor.Invoke(new object[] { recurse(innerTypeLeft), recurse(innerTypeRight) });
            });
        }

        public object VisitUint()
        {
            return this.GenerateRandom(typeof(uint), () => (uint)RandomInt());
        }

        public object VisitUlong()
        {
            return this.GenerateRandom(typeof(ulong), () => (ulong)(long)this.VisitLong());
        }

        public object VisitFixedInteger(Type intType)
        {
            var c = intType.GetConstructor(new Type[] { typeof(long) });
            var size = CommonUtilities.IntegerSize(intType);
            int exp = size <= 1 ? 0 : size - 2;
            long value = RandomInt() % (int)Math.Pow(2, exp);
            return c.Invoke(new object[] { value });
        }

        public object VisitUshort()
        {
            return this.GenerateRandom(typeof(ushort), () => (ushort)RandomInt());
        }

        public object VisitString()
        {
            return this.GenerateRandom(typeof(string), () =>
            {
                var size = RandomInt() % this.sizeBound;

                var characters = new char[size];

                for (int i = 0; i < size; i++)
                {
                    var c = (ushort)this.random.Next(32, 126);
                    characters[i] = Convert.ToChar(c);
                }

                return new string(characters);
            });
        }

        /// <summary>
        /// Returns a random value of a given type.
        /// </summary>
        /// <param name="type">The type to return.</param>
        /// <param name="generateFunction">The generation function.</param>
        /// <returns>An object of the appropriate type.</returns>
        private object GenerateRandom(Type type, Func<object> generateFunction)
        {
            if (this.GenerateMagicConstant(type, out var result))
            {
                return result;
            }

            return generateFunction();
        }

        /// <summary>
        /// Generate a magic constant if one is defined and a coin flip works out.
        /// </summary>
        /// <param name="type">The type to generate.</param>
        /// <param name="result">The result.</param>
        /// <returns>Whether a value was generated.</returns>
        private bool GenerateMagicConstant(Type type, out object result)
        {
            if (this.magicConstants != null &&
                this.random.Next() % 2 == 0 &&
                this.magicConstants.TryGetValue(type, out var values) &&
                values.Count > 0)
            {
                result = values.First();
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Generate a random integer.
        /// </summary>
        /// <returns></returns>
        private int RandomInt()
        {
            return this.random.Next();
        }
    }
}
