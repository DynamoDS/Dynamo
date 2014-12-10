using System;
using System.Collections.Generic;

using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    internal class NodeViewCustomizationLibrary
    {
        private readonly Dictionary<Type, List<InternalNodeViewCustomization>> lookupDict =
            new Dictionary<Type, List<InternalNodeViewCustomization>>();

        private readonly ILogger logger;

        internal NodeViewCustomizationLibrary(ILogger logger)
        {
            this.logger = logger;
        }

        internal void Apply(NodeView view)
        {
            var model = view.ViewModel.NodeModel;

            // We only apply the most specific INodeViewCustomization!

            // By "specific", we mean the INodeViewCustomization<T> where
            // T is closest to the runtime type of the NodeModel

            Type nodeModelType = model.GetType();
            Type customizableType = null;

            do
            {
                if (lookupDict.ContainsKey(nodeModelType))
                {
                    customizableType = nodeModelType;
                    break;
                }

                nodeModelType = nodeModelType.BaseType;

            } while (nodeModelType != typeof(NodeModel));

            // If there is a NodeViewCustomization<T>, apply the most specific one
            if (customizableType != null)
            {
                // There may be multiple customizations to apply
                List<InternalNodeViewCustomization> custs;
                if (lookupDict.TryGetValue(customizableType, out custs))
                {
                    foreach (var customization in custs)
                    {
                        TryApplyCustomization(model, view, customization);
                    }
                }
            }
        }

        private void TryApplyCustomization(NodeModel model, NodeView view, InternalNodeViewCustomization customization)
        {
            try
            {
                var disposable = customization.CustomizeView(model, view);
                view.Unloaded += (s, a) => disposable.Dispose();
            }
            catch (Exception e)
            {
                var message = "Failed to apply NodeViewCustomization for " + model.GetType().Name;
                logger.LogError(message);
                logger.Log(e);
            }
        }

        internal void Add(INodeViewCustomizations cs)
        {
            var custs = cs.GetCustomizations();

            foreach (var pair in custs)
            {
                var nodeModelType = pair.Key;
                var custTypes = pair.Value;

                foreach (var custType in custTypes)
                {
                    this.Add(nodeModelType, InternalNodeViewCustomization.Create(nodeModelType, custType));
                }
            }
        }

        internal void Add(Type modelType, InternalNodeViewCustomization internalNodeViewCustomization)
        {
            List<InternalNodeViewCustomization> entries;
            if (!lookupDict.TryGetValue(modelType, out entries))
            {
                entries = new List<InternalNodeViewCustomization>();
                lookupDict[modelType] = entries;
            }
            entries.Add(internalNodeViewCustomization);
        }

    }
}