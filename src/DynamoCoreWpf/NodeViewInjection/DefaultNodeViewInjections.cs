using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dynamo.Wpf
{
    /// <summary>
    /// Enumerates the injectors for the node view injectors in DynamoCoreWpf
    /// </summary>
    internal class CoreNodeViewInjectionInitializer : INodeViewInjectionInitializer
    {
        public Dictionary<Type, IEnumerable<INodeViewInjection>> GetInjections()
        {
            var type = typeof(INodeViewInjectionInitializer);
            var types = Assembly.GetExecutingAssembly().GetExportedTypes()
                .Where(p => p.IsClass && p.IsAssignableFrom(type));

            foreach (var initializer in types)
            {
                
            }

            return null;

            //return new Dictionary<Type, IEnumerable<INodeViewInjection>>
            //{
            //    { typeof(Nodes.DSVarArgFunction), new [] { new DSVarArgFunction() } },
            //    { typeof(CodeBlockNodeModel), new [] { new CodeBlock() } },
            //};
        }
    }

}
