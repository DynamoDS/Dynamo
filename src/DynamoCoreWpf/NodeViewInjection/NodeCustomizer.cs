using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Wpf
{
    internal class NodeCustomizer
    {
        private readonly IDictionary<Type, IEnumerable<Type>> registeredInjectors;

        private NodeCustomizer(IDictionary<Type, IEnumerable<Type>> injectors)
        {
            registeredInjectors = injectors;
        }

        internal static NodeCustomizer Create(INodeCustomizations initializer)
        {
            return new NodeCustomizer(initializer.GetCustomizations());
        }

        internal void Customize(dynNodeView nodeView)
        {
            var model = nodeView.ViewModel.NodeModel;

            var customizationTypes =
                registeredInjectors
                    .Where(pair => pair.Key.IsInstanceOfType(model))
                    .SelectMany(pair => pair.Value);

            foreach (var customizationType in customizationTypes)
            {
                dynamic obj = customizationType.GetInstance();
                obj.SetupCustomUIElements((dynamic) model, nodeView);
            }
        }

    }
}