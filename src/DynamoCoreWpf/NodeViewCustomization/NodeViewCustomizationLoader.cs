using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    internal static class NodeViewCustomizationLoader
    {
        private static Dictionary<Assembly, INodeViewCustomizations> cache = 
            new Dictionary<Assembly, INodeViewCustomizations>();

        public static INodeViewCustomizations LoadCustomizations(Assembly assem)
        {
            if (cache.ContainsKey(assem)) return cache[assem];

            var types = new Dictionary<Type, IEnumerable<Type>>();

            var customizerType = typeof(INodeViewCustomization<>);
            var customizerImps  =  assem.GetTypes().Where(t => !t.IsAbstract && ImplementsGeneric(customizerType, t));

            foreach (var customizerImp in customizerImps)
            {
                var nodeModelType = GetCustomizerTypeParameters(customizerImp);

                if (nodeModelType == null) continue;

                if (types.ContainsKey(nodeModelType))
                {
                    types[nodeModelType] = types[nodeModelType].Concat(new[] { customizerImp });
                }
                else
                {
                    types.Add(nodeModelType, new[] { customizerImp });
                }
            }

            return new NodeViewCustomizations(types);
        }

        private static Type GetCustomizerTypeParameters(Type toCheck)
        {
            var types = toCheck.GetInterfaces().Where(
                    x =>
                        x.IsGenericType &&
                        x.GetGenericTypeDefinition() == typeof(INodeViewCustomization<>))
                    .Select(x => x.GetGenericArguments()[0]);

            var mostDerived = MostDerivedCommonBase(types);

            if (mostDerived == null) return null;

            var ints = toCheck.GetInterfaces().FirstOrDefault(
                x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(INodeViewCustomization<>) &&
                    x.GetGenericArguments()[0] == mostDerived);

            return ints != null ? ints.GetGenericArguments()[0] : null;
        }

        public static IEnumerable<Type> TypeHierarchy(this Type type)
        {
            do
            {
                yield return type;
                type = type.BaseType;
            } while (type != null);
        }

        public static Type MostDerivedCommonBase(IEnumerable<Type> types)
        {
            if (!types.Any()) return null;

            var counts = types.SelectMany(t => t.TypeHierarchy())
                              .GroupBy(t => t)
                              .ToDictionary(g => g.Key, g => g.Count());

            var total = counts[typeof(object)]; // optimization instead of types.Count()
            return types.First().TypeHierarchy().First(t => counts[t] == total);
        }

        private static bool ImplementsGeneric(Type generic, Type toCheck)
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

            var isGenInterf = cur.GetInterfaces().Any(
                x =>
                    x.IsGenericType &&
                        x.GetGenericTypeDefinition() == generic);

            if (generic == cur || isGenInterf)
            {
                return true;
            }

            return false;
        }
    }
}
