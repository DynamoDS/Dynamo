using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dynamo.Wpf
{
    public class AssemblyNodeViewCustomizations : INodeViewCustomizations
    {
        private readonly Assembly assembly;

        public AssemblyNodeViewCustomizations(Assembly assem)
        {
            if (assem == null) throw new ArgumentNullException("assem");
            this.assembly = assem;
        }

        public IDictionary<Type, IEnumerable<Type>> GetCustomizations()
        {
            return NodeViewCustomizationLoader
                .LoadCustomizations(this.assembly)
                .GetCustomizations();
        }
    }
}