using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.Wpf.Properties;
using TypeExtensions = Dynamo.Utilities.TypeExtensions;

namespace Dynamo.Wpf
{
    internal static class NodeViewCustomizationLoader
    {
        private static Dictionary<Assembly, INodeViewCustomizations> cache = 
            new Dictionary<Assembly, INodeViewCustomizations>();

        public static INodeViewCustomizations LoadCustomizations(Assembly assem, ILogger logger)
        {
            if (cache.ContainsKey(assem)) return cache[assem];

            var types = new Dictionary<Type, IEnumerable<Type>>();

            try
            {
                var customizerImps = GetCustomizationTypes(assem);

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
            }
            catch (Exception ex)
            {
                logger.Log(string.Format(Resources.NodeViewCustomizationFindErrorMessage, assem.FullName));
                logger.Log(ex.Message);
            }

            return new NodeViewCustomizations(types);
        }

        private static IEnumerable<Type> GetCustomizationTypes(Assembly assem)
        {
            var customizerType = typeof(INodeViewCustomization<>);
            var customizerImps = assem.GetTypes().Where(t => !t.IsAbstract && 
                TypeExtensions.ImplementsGeneric(customizerType, t));
            return customizerImps;
        }

        private static Type GetCustomizerTypeParameters(Type toCheck)
        {
            var customizerInterfaces = toCheck.GetInterfaces().Where(
                x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof (INodeViewCustomization<>)).ToList();

            var customizerTypeArgs = customizerInterfaces.Select(x => x.GetGenericArguments()[0]);
            var mostDerived = MostDerivedCommonBase(customizerTypeArgs);
            if (mostDerived == null) return null;

            var ints = customizerInterfaces.FirstOrDefault(x => x.GetGenericArguments()[0] == mostDerived);

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

            if (!types.Skip(1).Any()) return types.First();

            var counts = types.SelectMany(t => t.TypeHierarchy())
                              .GroupBy(t => t)
                              .ToDictionary(g => g.Key, g => g.Count());

            Type type = counts.First().Key;
            var min = counts.First().Value;
            foreach (var ele in counts.Skip(1))
            {
                if (ele.Value < min)
                {
                    type = ele.Key;
                    min = ele.Value;
                }
            }

            return type;

            //var total = counts[typeof(object)]; 
            //return types.First().TypeHierarchy().First(t => counts[t] == total);
        }

       

        internal static bool ContainsNodeViewCustomizationType(Assembly assem)
        {
            return GetCustomizationTypes(assem).Any();
        }
    }
}
