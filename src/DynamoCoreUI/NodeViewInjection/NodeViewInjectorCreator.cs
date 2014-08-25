using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    public class NodeViewInjectorCreator
    {
        private readonly Dictionary<Type, IEnumerable<INodeViewInjector>> registeredInjectors;

        private NodeViewInjectorCreator(Dictionary<Type, IEnumerable<INodeViewInjector>> injectors)
        {
            registeredInjectors = injectors;
        }

        public static NodeViewInjectorCreator Create(INodeViewInjectorInitializer initializer)
        {
            return new NodeViewInjectorCreator(initializer.GetInjectors());
        }

        public void CreateInjector(NodeModel model, dynNodeView nodeView)
        {
            var injectors =
                registeredInjectors.Where(pair => pair.Key.IsInstanceOfType(model))
                    .SelectMany(pair => pair.Value);

            foreach (var injector in injectors)
            {
                injector.Inject(model, nodeView);
            }
        }
    }
}