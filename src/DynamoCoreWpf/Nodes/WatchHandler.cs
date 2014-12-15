using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.ViewModels;

namespace Dynamo.Nodes
{
    public interface WatchHandler
    {
        bool AcceptsValue(object o);
        void ProcessNode(object value, WatchViewModel node, bool showRawData);
    }
}
