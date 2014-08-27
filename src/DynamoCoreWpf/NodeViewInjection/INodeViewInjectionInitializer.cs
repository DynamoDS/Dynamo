using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Models;
using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    public interface INodeViewInjectionInitializer
    {
        Dictionary<Type, IEnumerable<INodeViewInjection>> GetInjections();
    }

    /// <summary>
    /// Enumerates the injectors for the core node types
    /// </summary>
    internal class HardcodedInitializer : INodeViewInjectionInitializer
    {
        public Dictionary<Type, IEnumerable<INodeViewInjection>> GetInjections()
        {
            // SEPARATECORE: this should just scan this assembly

            return new Dictionary<Type, IEnumerable<INodeViewInjection>>
            {
                { typeof(Nodes.DSVarArgFunction), new [] { new DSVarArgFunction() } },
                { typeof(CodeBlockNodeModel), new [] { new CodeBlock() } },
            };
        }
    }

    /// <summary>
    /// Allows node injectors to be loaded from a foreign assembly
    /// </summary>
    public class ReflectionInitializer : INodeViewInjectionInitializer
    {
        /// <summary>
        /// Register an assembly to be scanned when GetInjectors is run
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterAssembly(Assembly assembly)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Type, IEnumerable<INodeViewInjection>> GetInjections()
        {
            throw new NotImplementedException();
        }
    }
}
