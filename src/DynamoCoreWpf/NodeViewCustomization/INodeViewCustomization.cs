using System;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    public interface INodeViewCustomization<in T> : IDisposable where T : NodeModel
    {
        void CustomizeView(T model, NodeView nodeView);
    }
}
