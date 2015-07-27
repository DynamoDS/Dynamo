using Dynamo.Models;
using System;
using System.Collections.Generic;

namespace Dynamo.Interfaces
{
    public interface IWorkspaceModel
    {
        string Name { get; }
        string FileName { get; }

        IEnumerable<ConnectorModel> Connectors { get; }
        IEnumerable<NodeModel> Nodes { get; }

        double CenterX { get; }
        double CenterY { get; }

        event Action<NodeModel> NodeAdded;
        event Action<NodeModel> NodeRemoved;
        event Action NodesCleared;
        event Action<ConnectorModel> ConnectorAdded;
        event Action<ConnectorModel> ConnectorDeleted;
    }
}
