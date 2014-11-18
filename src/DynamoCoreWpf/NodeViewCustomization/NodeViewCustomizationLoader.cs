using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            var customizerImps0 = assem.GetTypes();
            var customizerImps  = customizerImps0.Where(t => !t.IsAbstract && ImplementsGeneric(customizerType, t));

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
            var ints = toCheck.GetInterfaces().FirstOrDefault(
                x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof (INodeViewCustomization<>) &&
                    !x.GetGenericArguments()[0].IsAbstract);

            return ints != null ? ints.GetGenericArguments()[0] : null;
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
