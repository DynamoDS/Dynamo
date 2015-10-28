using Dynamo.Models;
using System;
using System.Collections.Generic;
using Dynamo.Nodes;

namespace Dynamo.Interfaces
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

        /// <summary>
        /// Returns all zero touch nodes in the workspace that match the given names
        /// </summary>
        /// <param name="nodeNames">list of CreationName's of ZT nodes</param>
        /// <returns></returns>
        IEnumerable<DSFunctionBase> GetZTNodesForMatchingNames(IEnumerable<string> nodeNames);
    }
}
