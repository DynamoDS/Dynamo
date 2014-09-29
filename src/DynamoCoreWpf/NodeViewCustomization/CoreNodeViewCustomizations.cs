using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamo.Wpf
{
    internal class CoreNodeViewCustomizations : INodeViewCustomizations
    {
        private static INodeViewCustomizations cache;

        public IDictionary<Type, IEnumerable<Type>> GetCustomizations()
        {
            if (cache != null) return cache.GetCustomizations();

            cache = NodeViewCustomizationLoader.LoadCustomizations(Assembly.GetExecutingAssembly());

            return cache.GetCustomizations();
        }
    }
}
