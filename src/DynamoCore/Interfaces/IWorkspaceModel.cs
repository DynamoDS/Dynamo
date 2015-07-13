using Dynamo.Models;
using System;
using System.Collections.Generic;

namespace Dynamo.Interfaces
{
    public interface IWorkspaceModel
    {
        string Name { get; set; }

        IEnumerable<ConnectorModel> Connectors { get; }
        IEnumerable<NodeModel> Nodes { get; }

        double CenterX { get; set; }
        double CenterY { get; set; }

        event Action<NodeModel> NodeAdded;
        event Action<NodeModel> NodeRemoved;
        event Action NodesCleared;
        event Action<ConnectorModel> ConnectorAdded;
        event Action<ConnectorModel> ConnectorDeleted;
    }
}
