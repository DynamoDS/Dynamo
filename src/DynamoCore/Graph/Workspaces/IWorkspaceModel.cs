using System;
using System.Collections.Generic;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;

namespace Dynamo.Graph.Workspaces
{
    public interface IWorkspaceModel
    {
        /// <summary>
        /// Name of the workspace
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets description of the workspace
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets FileName of the workspace
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets list of connectors in the workspace
        /// </summary>
        IEnumerable<ConnectorModel> Connectors { get; }

        /// <summary>
        /// Gets list of nodes owned by this workspace.
        /// </summary>
        IEnumerable<NodeModel> Nodes { get; }

        /// <summary>
        /// Gets list of selected nodes.
        /// </summary>
        IEnumerable<NodeModel> CurrentSelection { get; }

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
