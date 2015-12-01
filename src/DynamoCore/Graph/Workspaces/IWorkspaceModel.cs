using System;
using System.Collections.Generic;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;

namespace Dynamo.Graph.Workspaces
{
    public interface IWorkspaceModel
    {
        string Name { get; }
        string Description { get; }
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

        /// <summary>
        /// Implement to record node modification for undo/redo
        /// </summary>
        /// <param name="models"></param>
        void RecordModelsForModification(IEnumerable<ModelBase> models);
    }
}
