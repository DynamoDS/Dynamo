using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Dynamo.Models;
using Dynamo.ViewModels;
using System.IO;
using System.Threading;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("ListMutator")]
    class ListMutator : AbstractMutator
    {
        public ListMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);
            Type type = assembly.GetType("DSCoreNodesUI.CreateList");

            return type;
        }

        public override bool RunTest(NodeModel node, StreamWriter writer)
        {
            bool pass = false;

            var firstNodeConnectors = node.AllConnectors.ToList();

            var valueMap = new Dictionary<Guid, String>();
            if (node.OutPorts.Count > 0)
            {
                foreach (ConnectorModel connector in firstNodeConnectors)
                {
                    if (connector.Start.Owner.GUID != node.GUID && 
                        !valueMap.ContainsKey(connector.Start.Owner.GUID))
                    {
                        Guid guid = connector.Start.Owner.GUID;
                        Object data = connector.Start.Owner.GetValue(0).Data;
                        String val = data != null ? data.ToString() : "null";
                        valueMap.Add(guid, val);
                        writer.WriteLine(guid + " :: " + val);
                        writer.Flush();
                    }
                }
            }

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

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.RunCancelCommand runCancel =
                    new DynamoModel.RunCancelCommand(false, false);

                DynamoViewModel.ExecuteCommand(runCancel);
            }));
            while (DynamoViewModel.Model.Runner.Running)
            {
                Thread.Sleep(10);
            }

            writer.WriteLine("### - Beginning test of List");
            if (node.OutPorts.Count > 0)
            {
                try
                {
                    foreach (ConnectorModel connector in firstNodeConnectors)
                    {
                        if (connector.Start.Owner.GUID != node.GUID)
                        {
                            String valmap = valueMap[connector.Start.Owner.GUID].ToString();
                            Object data = connector.Start.Owner.GetValue(0).Data;
                            String nodeVal = data != null ? data.ToString() : "null";

                            if (valmap != nodeVal)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of List is failed");
                                writer.WriteLine(node.GUID);

                                writer.WriteLine("Was: " + nodeVal);
                                writer.WriteLine("Should have been: " + valmap);
                                writer.Flush();
                                return pass;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine("!!!!!!!!!!! - test of List is failed");
                    writer.Flush();
                    return pass;
                }
            }
            writer.WriteLine("### - test of List complete");
            writer.Flush();

            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            Random rand = new Random(1);

            int countOfInPorts = 0;

            var nodeConnectors = node.AllConnectors.ToList();

            foreach (ConnectorModel connector in nodeConnectors)
            {
                if (connector.Start.Owner.GUID != node.GUID)
                {
                    Guid guidNumber = Guid.NewGuid();
                    double coordinatesX = node.X * rand.NextDouble();
                    double coordinatesYNumber1 = node.Y * rand.NextDouble();

                    countOfInPorts++;

                    DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        //make node
                        DynamoModel.CreateNodeCommand createNodeNumber1 =
                            new DynamoModel.CreateNodeCommand(guidNumber, "Number", 
                                coordinatesX, coordinatesYNumber1, false, true);

                        //create node
                        DynamoViewModel.ExecuteCommand(createNodeNumber1);

                        int outPortIndex = connector.Start.Index;
                        int inPortIndex = connector.End.Index;

                        //make connection
                        DynamoModel.MakeConnectionCommand connToStart1 =
                            new DynamoModel.MakeConnectionCommand(guidNumber, outPortIndex, 
                                PortType.OUTPUT, DynamoModel.MakeConnectionCommand.Mode.Begin);
                        DynamoModel.MakeConnectionCommand connToStart2 =
                            new DynamoModel.MakeConnectionCommand(node.GUID, inPortIndex, 
                                PortType.INPUT, DynamoModel.MakeConnectionCommand.Mode.End);

                        //create connections
                        DynamoViewModel.ExecuteCommand(connToStart1);
                        DynamoViewModel.ExecuteCommand(connToStart2);
                    }));
                }
            }
            return countOfInPorts * 2;
        }        
    }
}
