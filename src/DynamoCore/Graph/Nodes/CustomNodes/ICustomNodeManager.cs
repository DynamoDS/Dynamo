using System;
using System.Collections.Generic;
using Dynamo.Graph.Workspaces;

namespace Dynamo.Graph.Nodes.CustomNodes
{
    public interface ICustomNodeManager
    {
        IEnumerable<CustomNodeInfo> AddUninitializedCustomNodesInPath(string path, bool isTestMode, bool isPackageMember = false);
        Guid GuidFromPath(string path);
        bool TryGetFunctionWorkspace(Guid id, bool isTestMode, out ICustomNodeWorkspaceModel ws);
    }
}
