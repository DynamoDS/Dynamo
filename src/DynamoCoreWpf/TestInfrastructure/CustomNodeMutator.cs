using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Engine;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("CustomNodeMutator")]
    class CustomNodeMutator : AbstractMutator
    {
        public CustomNodeMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            return typeof(Function);
        }

        public override bool RunTest(NodeModel node, EngineController engine, StreamWriter writer)
        {
            bool pass = false;

            int workspaceIndex = DynamoViewModel.CurrentWorkspaceIndex;

            var firstNodeConnectors = node.AllConnectors.ToList();

            var valueMap = new Dictionary<Guid, String>();
            foreach (ConnectorModel connector in firstNodeConnectors)
            {
                if (connector.End.Owner.GUID != node.GUID)
                {
                    Guid guid = connector.Start.Owner.GUID;
                    Object data = connector.Start.Owner.GetValue(0, engine).Data;
                    String val = data != null ? data.ToString() : "null";
                    valueMap.Add(guid, val);
                    writer.WriteLine(guid + " :: " + val);
                    writer.Flush();
                }
            }

            string customNodeFilePath = string.Empty;

            var function = node as Function;
            CustomNodeInfo info = null;
            if (function != null)
            {
                var id = function.Definition.FunctionId;
                if (DynamoViewModel.Model.CustomNodeManager.TryGetNodeInfo(id, out info))
                {
                    customNodeFilePath = info.Path;
                }
            }

            var workspaces = DynamoViewModel.Model.Workspaces;

            if (File.Exists(customNodeFilePath))
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.OpenFileCommand openFile =
                        new DynamoModel.OpenFileCommand(customNodeFilePath);

                    DynamoViewModel.ExecuteCommand(openFile);
                }));
                Thread.Sleep(100);

                var nodesInCustomNodeBeforeMutation =
                    workspaces.FirstOrDefault((t) => (t.Name == info.Name)).Nodes.ToList();

                var customNodeStructureBeforeMutation = 
                    GetDictionaryOfConnectedNodes(nodesInCustomNodeBeforeMutation);

                int numberOfUndosNeeded = Mutate(node);
                Thread.Sleep(100);
                
                writer.WriteLine("### - Beginning undo");
                for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                {
                    DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoModel.UndoRedoCommand undoCommand =
                            new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);

                        DynamoViewModel.ExecuteCommand(undoCommand);
                    }));
                    Thread.Sleep(100);
                }
                writer.WriteLine("### - undo complete");
                writer.Flush();

                ExecuteAndWait();

                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.SwitchTabCommand switchCmd =
                        new DynamoModel.SwitchTabCommand(workspaceIndex);

                    DynamoViewModel.ExecuteCommand(switchCmd);
                }));
                Thread.Sleep(100);

                var nodesInCustomNodeAfterMutation =
                    workspaces.FirstOrDefault((t) => (t.Name == info.Name)).Nodes.ToList();

                var customNodeStructureAfterMutation = 
                    GetDictionaryOfConnectedNodes(nodesInCustomNodeAfterMutation);

                writer.WriteLine("### - Beginning test of CustomNode structure");
                if (customNodeStructureBeforeMutation.Count == customNodeStructureAfterMutation.Count)
                {
                    foreach (var item in customNodeStructureAfterMutation)
                    {
                        if (item.Value != customNodeStructureBeforeMutation[item.Key])
                        {
                            writer.WriteLine("!!!!!!!!!!! - test of CustomNode structure is failed");
                            writer.Flush();
                            return pass;
                        }
                    }
                }
                else
                {
                    writer.WriteLine("!!!!!!!!!!! - test of CustomNode structure is failed");
                    writer.Flush();
                    return pass;
                }

                writer.WriteLine("### - Beginning test of CustomNode");
                if (node.OutPorts.Count > 0)
                {
                    try
                    {
                        NodeModel nodeAfterUndo =
                            workspaces.SelectMany(ws => ws.Nodes)
                                .FirstOrDefault(t => t.GUID.Equals(node.GUID));

                        if (nodeAfterUndo == null)
                        {
                            writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                            writer.Flush();
                            return pass;
                        }

                        var firstNodeConnectorsAfterUndo = nodeAfterUndo.AllConnectors.ToList();

                        foreach (ConnectorModel connector in firstNodeConnectorsAfterUndo)
                        {
                            if (connector.End.Owner.GUID != node.GUID)
                            {
                                Object data = connector.Start.Owner.GetValue(0, engine).Data;
                                String nodeVal = data != null ? data.ToString() : "null";

                                if (valueMap[connector.Start.Owner.GUID] != nodeVal)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                                    writer.WriteLine(node.GUID);

                                    writer.WriteLine("Was: " + nodeVal);
                                    writer.WriteLine("Should have been: " + 
                                        valueMap[connector.End.Owner.GUID]);
                                    writer.Flush();
                                    return pass;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                        writer.Flush();
                        return pass;
                    }
                }
                var workspacesOfCustomNodes = DynamoViewModel.Workspaces.Where((t) =>
                    {
                        return (t.Model is CustomNodeWorkspaceModel);
                    }).ToList();

                if (workspacesOfCustomNodes != null)
                {
                    foreach (var item in workspacesOfCustomNodes)
                    {
                        DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoModel.SwitchTabCommand swithCommand =
                                new DynamoModel.SwitchTabCommand(workspaceIndex);

                            DynamoViewModel.ExecuteCommand(swithCommand);

                            DynamoViewModel.Workspaces.Remove(item);
                        }));
                    }
                }

                writer.WriteLine("### - test of CustomNode complete");
                writer.Flush();
            }
            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            int workspaceIndex = DynamoViewModel.CurrentWorkspaceIndex;
            Random rand = new Random(1);

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.SwitchTabCommand switchCmd =
                    new DynamoModel.SwitchTabCommand(workspaceIndex);

                DynamoViewModel.ExecuteCommand(switchCmd);
                Thread.Sleep(100);
            }));

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.SwitchTabCommand switchCmd =
                    new DynamoModel.SwitchTabCommand(workspaceIndex);

                DynamoViewModel.ExecuteCommand(switchCmd);
                Thread.Sleep(100);
            }));
            
            var workspaces = DynamoModel.Workspaces;

            CustomNodeInfo info;
            var id = ((Function)node).Definition.FunctionId;
            DynamoViewModel.Model.CustomNodeManager.TryGetNodeInfo(id, out info);

            var outputsInCustomNode =
                workspaces.FirstOrDefault((t) => (t.Name == info.Name))
                    .Nodes.Where(t => t.GetType() == typeof(Output))
                    .ToList();

            Guid numberGuid = Guid.NewGuid();
            double coordinatesX = rand.NextDouble() * node.X;
            double coordinatesY = rand.NextDouble() * node.Y;

            //DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            //{
            //    DynamoModel.AddNodeCommand addCommand =
            //        new DynamoModel.AddNodeCommand(numberGuid, "Number", 
            //            coordinatesX, coordinatesY, false, false);
            //    DynamoViewModel.ExecuteCommand(addCommand);
            //}));

            foreach (NodeModel output in outputsInCustomNode)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.MakeConnectionCommand connToAnother1 =
                        new DynamoModel.MakeConnectionCommand(numberGuid, 0, PortType.Output, 
                            DynamoModel.MakeConnectionCommand.Mode.Begin);
                    DynamoModel.MakeConnectionCommand connToAnother2 =
                        new DynamoModel.MakeConnectionCommand(output.GUID, 0, PortType.Input, 
                            DynamoModel.MakeConnectionCommand.Mode.End);

                    DynamoViewModel.ExecuteCommand(connToAnother1);
                    DynamoViewModel.ExecuteCommand(connToAnother2);
                }));
            }

            int numberOfUndosNeeded = outputsInCustomNode.Count * 2 + 1;

            return numberOfUndosNeeded;
        }

        private Dictionary<Guid, String> GetDictionaryOfConnectedNodes(List<NodeModel> list)
        {
            var dictionary = new Dictionary<Guid, string>();
            foreach (NodeModel node in list)
            {
                var nodeConnectors = node.AllConnectors.ToList();
                string connectorGuids = string.Empty;

                foreach (ConnectorModel connector in nodeConnectors)
                {
                    if (connector.Start.Owner.GUID != node.GUID)
                        connectorGuids += connector.Start.Owner.GUID.ToString() + "_";

                    else if (connector.End.Owner.GUID != node.GUID)
                        connectorGuids += connector.End.Owner.GUID.ToString() + "_";
                }
                dictionary.Add(node.GUID, connectorGuids);
            }
            return dictionary;
        }
    }
}
