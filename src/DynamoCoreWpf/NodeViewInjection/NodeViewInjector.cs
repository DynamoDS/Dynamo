using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    internal class NodeViewInjector
    {
        private readonly Dictionary<Type, IEnumerable<INodeViewInjection>> registeredInjectors;

        private NodeViewInjector(Dictionary<Type, IEnumerable<INodeViewInjection>> injectors)
        {
            registeredInjectors = injectors;
        }

        internal static NodeViewInjector Create(INodeViewInjectionInitializer initializer)
        {
            return new NodeViewInjector(initializer.GetInjections());
        }

        internal void Inject(dynNodeView nodeView)
        {
            var model = nodeView.ViewModel.NodeModel;
            var injectors =
                registeredInjectors.Where(pair => pair.Key.IsInstanceOfType(model))
                    .SelectMany(pair => pair.Value);

            foreach (var injector in injectors)
            {
                injector.SetupCustomUIElements(nodeView);

                // SEPARATECORE
                //nodeView.ViewModel.Deleted += injector.Dispose();
            }
        }

    }
}