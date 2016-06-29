using System;
using System.Collections.Generic;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Exposes workspace model.
    /// </summary>
    public interface IWorkspaceModel
    {
        /// <summary>
        /// Returns name of the workspace
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns description of the workspace
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Returns FileName of the workspace
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Returns list of connectors in the workspace
        /// </summary>
        IEnumerable<ConnectorModel> Connectors { get; }

        /// <summary>
        /// Returns list of nodes owned by this workspace.
        /// </summary>
        IEnumerable<NodeModel> Nodes { get; }

        /// <summary>
        /// Returns list of selected nodes.
        /// </summary>
        IEnumerable<NodeModel> CurrentSelection { get; }

        /// <summary>
        /// Returns X coordinate of center point of visible workspace part
        /// </summary>
        double CenterX { get; }

        /// <summary>
        /// Returns Y coordinate of center point of visible workspace part
        /// </summary>
        double CenterY { get; }

        /// <summary>
        /// Triggers when a node is added to the workspace.
        /// </summary>
        event Action<NodeModel> NodeAdded;

        /// <summary>
        /// Triggers when a node is removed from the workspace.
        /// </summary>
        event Action<NodeModel> NodeRemoved;

        /// <summary>
        /// Triggers when nodes are cleared from the workspace.
        /// </summary>
        event Action NodesCleared;

        /// <summary>
        /// Triggers when a connector is added to the workspace.
        /// </summary>
        event Action<ConnectorModel> ConnectorAdded;

        /// <summary>
        /// Triggers when a connector is removed from the workspace.
        /// </summary>
        event Action<ConnectorModel> ConnectorDeleted;

        /// <summary>
        /// Implement to record node modification for undo/redo
        /// </summary>
        /// <param name="models">Collection of <see cref="ModelBase"/> objects to record.</param>
        void RecordModelsForModification(IEnumerable<ModelBase> models);
    }
}
