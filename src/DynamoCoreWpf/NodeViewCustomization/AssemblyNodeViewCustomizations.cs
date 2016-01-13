using System;
using System.Collections.Generic;
using System.Reflection;
using Dynamo.Logging;

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

        public IDictionary<Type, IEnumerable<Type>> GetCustomizations(ILogger logger)
        {
            return NodeViewCustomizationLoader
                .LoadCustomizations(this.assembly, logger)
                .GetCustomizations(logger);
        }
    }
}