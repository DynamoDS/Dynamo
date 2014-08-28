using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;

using Dynamo.Models;
using Dynamo.Utilities;

using RestSharp.Extensions;

namespace Dynamo.Wpf
{
    //<summary>
    // Enumerates the injectors for the NodeCustomizations in this assembly
    //</summary>
    internal class CoreNodeCustomizations : INodeCustomizations
    {
        private static Dictionary<Type, IEnumerable<Type>> cache;

        public IDictionary<Type, IEnumerable<Type>> GetCustomizations()
        {
            if (cache != null) return cache;

            cache = new Dictionary<Type, IEnumerable<Type>>();

            var customizerType = typeof(INodeCustomization<>);

            var customizerImps = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && ImplementsGeneric(customizerType, t));

            foreach (var customizerImp in customizerImps)
            {
                var nodeModelType = GetCustomizerTypeParameter(customizerImp);

                if (cache.ContainsKey(nodeModelType))
                {
                    cache[nodeModelType] = cache[nodeModelType].Concat(new[] { customizerImp });
                }
                else
                {
                    cache.Add(nodeModelType, new[] { customizerImp });
                }
            }

            return cache;
        }

        private static Type GetCustomizerTypeParameter(Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var genInterf = toCheck.GetInterfaces().FirstOrDefault(
                    x =>
                        x.IsGenericType &&
                            x.GetGenericTypeDefinition() == typeof(INodeCustomization<>));

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
