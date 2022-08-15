using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Utilities
{
    internal class CrashOnDemand
    {
        internal static void CurrentWorkspace_NodeAdded(NodeModel obj)
        {
            if (DebugModes.IsEnabled("CrashOnNewNodeModel"))
                throw new Exception($"On demand crash when creating NodeModel {obj.Name}");
        }
    }
}
