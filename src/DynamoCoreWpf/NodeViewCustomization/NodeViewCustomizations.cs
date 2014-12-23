using System;
using System.Collections.Generic;

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
}