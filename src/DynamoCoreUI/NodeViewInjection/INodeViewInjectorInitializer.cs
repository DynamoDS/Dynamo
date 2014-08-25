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
    public interface INodeViewInjectorInitializer
    {
        Dictionary<Type, IEnumerable<INodeViewInjector>> GetInjectors();
    }

    /// <summary>
    /// Enumerates the injectors for the core node types
    /// </summary>
    internal class HardcodedInitializer : INodeViewInjectorInitializer
    {
        public Dictionary<Type, IEnumerable<INodeViewInjector>> GetInjectors()
        {
            // TODO: this should just scan this assembly

            return new Dictionary<Type, IEnumerable<INodeViewInjector>>
            {
                { typeof(DSVarArgFunction), new [] { new DSVarArgFunctionViewInjector() } },
                { typeof(VariableInputNode), new [] { new VariableInputViewInjector() } },
                { typeof(CodeBlockNodeModel), new [] { new CodeBlockViewInjector() } },
            };
        }
    }

    /// <summary>
    /// Allows node injectors to be loaded from a foreign assembly
    /// </summary>
    public class ReflectionInitializer : INodeViewInjectorInitializer
    {
        /// <summary>
        /// Register an assembly to be scanned when GetInjectors is run
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterAssembly(Assembly assembly)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Type, IEnumerable<INodeViewInjector>> GetInjectors()
        {
            throw new NotImplementedException();
        }
    }
}
