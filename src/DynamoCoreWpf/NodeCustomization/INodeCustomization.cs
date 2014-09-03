using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.Wpf
{
    public interface INodeCustomization<in T> : IDisposable where T : NodeModel
    {
        void CustomizeView(T model, dynNodeView view);
    }
}
