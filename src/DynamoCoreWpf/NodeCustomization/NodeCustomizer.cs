using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Wpf
{
    internal class NodeViewCustomizer
    {
        private readonly Dictionary<Type, List<InternalNodeCustomization>> lookupDict =
            new Dictionary<Type, List<InternalNodeCustomization>>();

        internal void Apply(NodeModel model, dynNodeView view)
        {
            Type t = model.GetType();
            do
            {
                List<InternalNodeCustomization> custs;
                if (lookupDict.TryGetValue(t, out custs))
                {
                    foreach (var customization in custs)
                    {
                        var disposable = customization.CustomizeView(model, view);
                        //view.Destroyed += disposable.Dispose;
                    }
                }
                t = t.BaseType;
            } while (t != typeof(NodeModel));
        }

        internal void Add(INodeCustomizations cs)
        {
            var custs = cs.GetCustomizations();

            foreach (var pair in custs)
            {
                var nodeModelType = pair.Key;
                var custTypes = pair.Value;

                foreach (var custType in custTypes)
                {
                    this.Add(nodeModelType, InternalNodeCustomization.Create(custType));
                }
            }
        }

        internal void Add(Type modelType, InternalNodeCustomization internalNodeCustomization)
        {
            List<InternalNodeCustomization> entries;
            if (!lookupDict.TryGetValue(modelType, out entries))
            {
                entries = new List<InternalNodeCustomization>();
                lookupDict[modelType] = entries;
            }
            entries.Add(internalNodeCustomization);
        }

    }
}