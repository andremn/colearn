using System;
using System.Collections.Generic;

namespace FinalProject.Shared.Expressions
{
    public static class TypeMapper
    {
        private static readonly IDictionary<Type, Type> Maps;

        static TypeMapper()
        {
            Maps = new Dictionary<Type, Type>();
        }

        public static void AddMap<TFrom, TTo>()
        {
            AddMap(typeof(TFrom), typeof(TTo));
        }

        public static void AddMap(Type from, Type to)
        {
            Maps[from] = to;
        }

        public static Type Map<T>()
        {
            return Map(typeof(T));
        }

        public static Type Map(Type type)
        {
            Type resultType;

            return Maps.TryGetValue(type, out resultType) 
                ? resultType 
                : null;
        }
    }
}
