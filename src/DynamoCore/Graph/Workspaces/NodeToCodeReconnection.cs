using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Graph.Workspaces
{
    public partial class WorkspaceModel
    {
        /// <summary>
        /// Checks whether the given connection is inside the node to code set or outside it.
        /// This determines if it should be redrawn(if it is external) or if it should be
        /// deleted (if it is internal)
        /// </summary>
        private static bool IsInternalNodeToCodeConnection(IEnumerable<NodeModel> nodes, ConnectorModel connector)
        {
            return nodes.Contains(connector.Start.Owner) && nodes.Contains(connector.End.Owner);
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalOutputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="cbn">The new Node To Code created Code Block Node</param>
        private static List<ConnectorModel> ReConnectOutputConnections(Dictionary<ConnectorModel, string> externalOutputConnections, CodeBlockNodeModel cbn)
        {
            List<ConnectorModel> newConnectors = new List<ConnectorModel>();
            foreach (var kvp in externalOutputConnections)
            {
                var connector = kvp.Key;
                var variableName = kvp.Value;

                //Get the start and end idex for the ports for the connection
                var portModel = cbn.OutPorts.FirstOrDefault(
                    port => cbn.GetRawAstIdentifierForOutputIndex(port.Index).Value.Equals(variableName));

                if (portModel == null)
                    continue;

                //Make the new connection and then record and add it
                var newConnector = ConnectorModel.Make(
                    cbn,
                    connector.End.Owner,
                    portModel.Index,
                    connector.End.Index);

                newConnectors.Add(newConnector);
            }
            return newConnectors;
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalInputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="cbn">The new Node To Code created Code Block Node</param>
        private List<ConnectorModel> ReConnectInputConnections(
            Dictionary<ConnectorModel, string> externalInputConnections, CodeBlockNodeModel cbn)
        {
            List<ConnectorModel> newConnectors = new List<ConnectorModel>();

            foreach (var kvp in externalInputConnections)
            {
                var connector = kvp.Key;
                var variableName = kvp.Value;

                var endPortIndex = CodeBlockNodeModel.GetInportIndex(cbn, variableName);
                if (endPortIndex < 0)
                    continue;

                if (Connectors.Any(c => c.End == cbn.InPorts[endPortIndex]))
                    continue;

                var newConnector = ConnectorModel.Make(
                    connector.Start.Owner,
                    cbn,
                    connector.Start.Index,
                    endPortIndex);

                newConnectors.Add(newConnector);
            }

            return newConnectors;
        }
    }
}
