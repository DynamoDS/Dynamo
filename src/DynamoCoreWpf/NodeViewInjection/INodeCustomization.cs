using System;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    public interface INodeCustomization<in T> : IDisposable
        where T : NodeModel
    {
        void SetupCustomUIElements(T nodeModel, dynNodeView nodeView);
    }
}
