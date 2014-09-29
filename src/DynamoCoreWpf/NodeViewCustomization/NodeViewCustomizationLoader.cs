using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamo.Wpf
{
    public class NodeViewCustomizations : INodeViewCustomizations
    {
        private readonly IDictionary<Type, IEnumerable<Type>> customizations;

        public NodeViewCustomizations(IDictionary<Type, IEnumerable<Type>> customizationMap)
        {
            customizations = customizationMap ?? new Dictionary<Type, IEnumerable<Type>>();
        }

        public IDictionary<Type, IEnumerable<Type>> GetCustomizations()
        {
            return customizations;
        }
    }

    public static class NodeViewCustomizationLoader
    {
        private static Dictionary<Assembly, INodeViewCustomizations> cache;

        public static INodeViewCustomizations LoadCustomizations(Assembly assem)
        {
            var types = new Dictionary<Type, IEnumerable<Type>>();

            var customizerType = typeof(INodeViewCustomization<>);

            var customizerImps = assem.GetTypes()
                .Where(t => !t.IsAbstract && ImplementsGeneric(customizerType, t));

            foreach (var customizerImp in customizerImps)
            {
                var nodeModelType = GetCustomizerTypeParameter(customizerImp);

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

        private static Type GetCustomizerTypeParameter(Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var genInterf = toCheck.GetInterfaces().FirstOrDefault(
                    x =>
                        x.IsGenericType &&
                            x.GetGenericTypeDefinition() == typeof(INodeViewCustomization<>));

                if (genInterf != null)
                {
                    return genInterf.GetGenericArguments()[0];
                }
                toCheck = toCheck.BaseType;
            }

            return null;
        }

        private static bool ImplementsGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
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
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
