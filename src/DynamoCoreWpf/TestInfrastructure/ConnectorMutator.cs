using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Engine;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("ConnectorMutator")]
    class ConnectorMutator : AbstractMutator
    {
        public ConnectorMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override bool RunTest(NodeModel node, EngineController engine, StreamWriter writer)
        {
            bool pass = false;

            List<ConnectorModel> firstNodeConnectors = node.AllConnectors.ToList();

            Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();

            foreach (ConnectorModel connector in firstNodeConnectors)
            {
                if (connector.Start.Owner.GUID != node.GUID)
                {
                    Guid guid = connector.Start.Owner.GUID;
                    if (!valueMap.ContainsKey(guid))
                    {
                        String val = connector.Start.Owner.Name;
                        valueMap.Add(guid, val);
                        writer.WriteLine(guid + " :: " + val);
                        writer.Flush();
                    }
                }
                else if (connector.End.Owner.GUID != node.GUID)
                {
                    Guid guid = connector.End.Owner.GUID;
                    if (!valueMap.ContainsKey(guid))
                    {
                        String val = connector.End.Owner.Name;
                        valueMap.Add(guid, val);
                        writer.WriteLine(guid + " :: " + val);
                        writer.Flush();
                    }
                }
            }

            int numberOfUndosNeeded = Mutate(node);

            Thread.Sleep(0);

            IEnumerable<NodeModel> nodesAfterMutate = DynamoViewModel.Model.CurrentWorkspace.Nodes;

            if (nodesAfterMutate.Contains(node))
            {
                writer.WriteLine("### - Connector wasn't deleted");
                writer.Flush();
                return pass;
            }
            else
                writer.WriteLine("### - Connector was deleted");

            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.UndoRedoCommand undoCommand =
                        new DynamoModel.UndoRedoCommand(
                            DynamoModel.UndoRedoCommand.Operation.Undo);

                    DynamoViewModel.ExecuteCommand(undoCommand);
                }));
            }
            Thread.Sleep(0);

            IEnumerable<NodeModel> nodesAfterUndo = DynamoViewModel.Model.CurrentWorkspace.Nodes;

            NodeModel nodeAfterUndo = nodesAfterUndo.FirstOrDefault(t => t.GUID.Equals(node.GUID));

            List<ConnectorModel> firstNodeConnectorsAfterUndo = nodeAfterUndo.AllConnectors.ToList();

            foreach (ConnectorModel connector in firstNodeConnectorsAfterUndo)
            {
                if (connector.Start.Owner.GUID != node.GUID)
                {
                    Guid guid = connector.Start.Owner.GUID;

                    if (!valueMap.ContainsKey(guid))
                    {
                        writer.WriteLine("### - ### - Connector wasn't recreated");
                        writer.Flush();
                        return pass;
                    }
                    else
                    {
                        writer.WriteLine("### - Connector was recreated");
                        writer.Flush();
                    }
                }
                else if (connector.End.Owner.GUID != node.GUID)
                {
                    Guid guid = connector.End.Owner.GUID;

                    if (!valueMap.ContainsKey(guid))
                    {
                        writer.WriteLine("### - ### - Connector wasn't recreated");
                        writer.Flush();
                        return pass;
                    }
                    else
                    {
                        writer.WriteLine("### - Connector was recreated");
                        writer.Flush();
                    }
                }
            }
            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.DeleteModelCommand delCommand =
                    new DynamoModel.DeleteModelCommand(node.GUID);

                DynamoViewModel.ExecuteCommand(delCommand);

            }));

            return 1;
        }
    }
}
