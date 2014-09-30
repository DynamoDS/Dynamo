using System;
using System.Collections.Generic;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    internal class NodeViewCustomizationLibrary
    {
        private Dictionary<Type, List<InternalNodeViewCustomization>> lookupDict =
            new Dictionary<Type, List<InternalNodeViewCustomization>>();

        internal void Apply(dynNodeView view)
        {
            var model = view.ViewModel.NodeModel;
            Type t = model.GetType();
            do
            {
                List<InternalNodeViewCustomization> custs; 
                if (lookupDict.TryGetValue(t, out custs))
                {
                    foreach (var customization in custs)
                    {
                        var disposable = customization.CustomizeView(model, view);
                        // TODO CORESPLIT
                        //view.Destroyed += disposable.Dispose;
                    }
                }
                t = t.BaseType;
            } while (t != typeof(NodeModel));
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
                    this.Add(nodeModelType, InternalNodeViewCustomization.Create(custType));
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