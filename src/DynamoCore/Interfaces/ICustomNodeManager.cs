using Dynamo.Models;
using System;
using System.Collections.Generic;

namespace Dynamo.Interfaces
{
    public interface ICustomNodeManager
    {
        IEnumerable<CustomNodeInfo> AddUninitializedCustomNodesInPath(string path, bool isTestMode, bool isPackageMember = false);
        Guid GuidFromPath(string path);
        bool TryGetFunctionWorkspace(Guid id, bool isTestMode, out CustomNodeWorkspaceModel ws);
    }
}
