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
    [MutationTest("NumberRangeMutator")]
    class NumberRangeMutator : AbstractMutator
    {
        public NumberRangeMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);
            Type type = assembly.GetType("DSCoreNodesUI.NumberRange");

            return type;
        }

        public override bool RunTest(NodeModel node, StreamWriter writer)
        {
            bool pass = false;

            var valueMap = new Dictionary<Guid, String>();
            if (node.OutPorts.Count > 0)
            {
                var firstNodeConnectors = node.AllConnectors.ToList(); //Get node connectors
                foreach (ConnectorModel connector in firstNodeConnectors)
                {
                    Guid guid = connector.Start.Owner.GUID;
                    if (!valueMap.ContainsKey(guid))
                    {
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

            writer.WriteLine("### - Beginning test of NumberRange");
            if (node.OutPorts.Count > 0)
            {
                try
                {
                    var firstNodeConnectors = node.AllConnectors.ToList();
                    foreach (ConnectorModel connector in firstNodeConnectors)
                    {
                        String valmap = valueMap[connector.Start.Owner.GUID].ToString();
                        Object data = connector.Start.Owner.GetValue(0).Data;
                        String nodeVal = data != null ? data.ToString() : "null";

                        if (valmap != nodeVal)
                        {
                            writer.WriteLine("!!!!!!!!!!! - test of NumberRange is failed");
                            writer.WriteLine(node.GUID);

                            writer.WriteLine("Was: " + nodeVal);
                            writer.WriteLine("Should have been: " + valmap);
                            writer.Flush();
                            return pass;
                        }
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine("!!!!!!!!!!! - test of NumberRange is failed");
                    writer.Flush();
                    return pass;
                }
            }
            writer.WriteLine("### - test of NumberRange complete");
            writer.Flush();

            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                Guid guidNumber = Guid.NewGuid();

                double coordinatesX = 120;
                double coordinatesY = 180;

                DynamoModel.CreateNodeCommand createNodeCmd1 =
                    new DynamoModel.CreateNodeCommand(guidNumber, "Number", coordinatesX,
                        coordinatesY, false, true);

                DynamoViewModel.ExecuteCommand(createNodeCmd1);

                DynamoModel.MakeConnectionCommand connToStart1 =
                    new DynamoModel.MakeConnectionCommand(guidNumber, 0, PortType.OUTPUT,
                        DynamoModel.MakeConnectionCommand.Mode.Begin);
                DynamoModel.MakeConnectionCommand connToStart2 =
                    new DynamoModel.MakeConnectionCommand(node.GUID, 0, PortType.INPUT,
                        DynamoModel.MakeConnectionCommand.Mode.End);

                DynamoModel.MakeConnectionCommand connToAmount1 =
                    new DynamoModel.MakeConnectionCommand(guidNumber, 0, PortType.OUTPUT,
                        DynamoModel.MakeConnectionCommand.Mode.Begin);
                DynamoModel.MakeConnectionCommand connToAmount2 =
                    new DynamoModel.MakeConnectionCommand(node.GUID, 1, PortType.INPUT,
                        DynamoModel.MakeConnectionCommand.Mode.End);

                DynamoModel.MakeConnectionCommand connToStep1 =
                    new DynamoModel.MakeConnectionCommand(guidNumber, 0, PortType.OUTPUT,
                        DynamoModel.MakeConnectionCommand.Mode.Begin);
                DynamoModel.MakeConnectionCommand connToStep2 =
                    new DynamoModel.MakeConnectionCommand(node.GUID, 2, PortType.INPUT,
                        DynamoModel.MakeConnectionCommand.Mode.End);

                DynamoViewModel.ExecuteCommand(connToStart1); //"Number" with "Number Range" on Start
                DynamoViewModel.ExecuteCommand(connToStart2); //"Number" with "Number Range" on Start
                DynamoViewModel.ExecuteCommand(connToAmount1); //"Number" with "Number Range" on End
                DynamoViewModel.ExecuteCommand(connToAmount2); //"Number" with "Number Range" on End
                DynamoViewModel.ExecuteCommand(connToStep1); //"Number" with "Number Range" on Step
                DynamoViewModel.ExecuteCommand(connToStep2); //"Number" with "Number Range" on Step
            }));

            return 4;
        }
    }
}
