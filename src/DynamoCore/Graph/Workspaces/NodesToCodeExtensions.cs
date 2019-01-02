using Dynamo.Engine;
using Dynamo.Engine.NodeToCode;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Selection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Dynamo.Graph.Workspaces
{
    internal static class NodesToCodeExtensions
    {
        internal static void ConvertNodesToCodeInternal(this WorkspaceModel workspace, EngineController engineController, 
            INamingProvider namingProvider)
        {
            var selectedNodes = DynamoSelection.Instance
                                               .Selection
                                               .OfType<NodeModel>()
                                               .Where(n => n.IsConvertible);
            if (!selectedNodes.Any())
                return;

            var cliques = NodeToCodeCompiler.GetCliques(selectedNodes).Where(c => !(c.Count == 1 && c.First() is CodeBlockNodeModel));
            var codeBlockNodes = new List<CodeBlockNodeModel>();

            //UndoRedo Action Group----------------------------------------------
            NodeToCodeUndoHelper undoHelper = new NodeToCodeUndoHelper();

            foreach (var nodeList in cliques)
            {
                //Create two dictionarys to store the details of the external connections that have to
                //be recreated after the conversion
                var externalInputConnections = new Dictionary<ConnectorModel, string>();
                var externalOutputConnections = new Dictionary<ConnectorModel, string>();

                //Also collect the average X and Y co-ordinates of the different nodes
                int nodeCount = nodeList.Count;

                var nodeToCodeResult = engineController.ConvertNodesToCode(workspace.Nodes, nodeList, namingProvider);

                #region Step I. Delete all nodes and their connections

                double totalX = 0, totalY = 0;

                foreach (var node in nodeList)
                {
                    #region Step I.A. Delete the connections for the node

                    foreach (var connector in node.AllConnectors.ToList())
                    {
                        if (!IsInternalNodeToCodeConnection(nodeList, connector))
                        {
                            //If the connector is an external connector, the save its details
                            //for recreation later
                            var startNode = connector.Start.Owner;
                            int index = startNode.OutPorts.IndexOf(connector.Start);
                            //We use the varibleName as the connection between the port of the old Node
                            //to the port of the new node.
                            var variableName = startNode.GetAstIdentifierForOutputIndex(index).Value;

                            //Store the data in the corresponding dictionary
                            if (startNode == node)
                            {
                                if (nodeToCodeResult.OutputMap.ContainsKey(variableName))
                                    variableName = nodeToCodeResult.OutputMap[variableName];
                                externalOutputConnections.Add(connector, variableName);
                            }
                            else
                            {
                                if (nodeToCodeResult.InputMap.ContainsKey(variableName))
                                    variableName = nodeToCodeResult.InputMap[variableName];
                                externalInputConnections.Add(connector, variableName);
                            }
                        }

                        //Delete the connector
                        undoHelper.RecordDeletion(connector);
                        connector.Delete();
                    }
                    #endregion

                    #region Step I.B. Delete the node
                    totalX += node.X;
                    totalY += node.Y;
                    undoHelper.RecordDeletion(node);
                    workspace.RemoveAndDisposeNode(node);
                    #endregion
                }
                #endregion

                #region Step II. Create the new code block node
                var outputVariables = externalOutputConnections.Values;
                var newResult = NodeToCodeCompiler.ConstantPropagationForTemp(nodeToCodeResult, outputVariables);

                // Rewrite the AST using the shortest unique name in case of namespace conflicts
                NodeToCodeCompiler.ReplaceWithShortestQualifiedName(
                    engineController.LibraryServices.LibraryManagementCore.ClassTable, newResult.AstNodes, workspace.ElementResolver);
                var codegen = new ProtoCore.CodeGenDS(newResult.AstNodes);
                var code = codegen.GenerateCode();

                var codeBlockNode = new CodeBlockNodeModel(
                    code,
                    System.Guid.NewGuid(),
                    totalX / nodeCount,
                    totalY / nodeCount, engineController.LibraryServices, workspace.ElementResolver);
                undoHelper.RecordCreation(codeBlockNode);

                workspace.AddAndRegisterNode(codeBlockNode, false);
                codeBlockNodes.Add(codeBlockNode);
                #endregion

                #region Step III. Recreate the necessary connections
                var newInputConnectors = ReConnectInputConnections(externalInputConnections, codeBlockNode, workspace);
                foreach (var connector in newInputConnectors)
                {
                    undoHelper.RecordCreation(connector);
                }

                var newOutputConnectors = ReConnectOutputConnections(externalOutputConnections, codeBlockNode);
                foreach (var connector in newOutputConnectors)
                {
                    undoHelper.RecordCreation(connector);
                }
                #endregion
            }

            undoHelper.ApplyActions(workspace.UndoRecorder);

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.AddRange(codeBlockNodes);

            Debug.WriteLine(string.Format("Workspace has {0} nodes and {1} connectors after N2C operation.", workspace.Nodes.Count(), workspace.Connectors.Count()));
            workspace.RequestRun();
        }

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
        /// <param name="workspace"></param>
        private static List<ConnectorModel> ReConnectInputConnections(
            Dictionary<ConnectorModel, string> externalInputConnections, CodeBlockNodeModel cbn, WorkspaceModel workspace)
        {
            List<ConnectorModel> newConnectors = new List<ConnectorModel>();

            foreach (var kvp in externalInputConnections)
            {
                var connector = kvp.Key;
                var variableName = kvp.Value;

                var endPortIndex = CodeBlockNodeModel.GetInportIndex(cbn, variableName);
                if (endPortIndex < 0)
                    continue;

                if (workspace.Connectors.Any(c => c.End == cbn.InPorts[endPortIndex]))
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
