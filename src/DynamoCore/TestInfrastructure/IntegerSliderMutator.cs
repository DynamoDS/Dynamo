using System;
using System.Collections.Generic;

using Dynamo.Models;
using Dynamo.ViewModels;
using System.Reflection;
using System.IO;
using System.Threading;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("IntegerSliderMutator")]
    class IntegerSliderMutator : AbstractMutator
    {
        public IntegerSliderMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {            
        }

        public override Type GetNodeType()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);
            Type type = assembly.GetType("Dynamo.Nodes.IntegerSlider");

            return type;
        }

        public override bool RunTest(NodeModel node, StreamWriter writer)
        {
            bool pass = false;

            var valueMap = new Dictionary<Guid, String>();
            if (node.OutPorts.Count > 0)
            {
                Guid guid = node.GUID;
                Object data = node.GetValue(0).Data;
                String val = data != null ? data.ToString() : "null";
                valueMap.Add(guid, val);
                writer.WriteLine(guid + " :: " + val);
                writer.Flush();
            }

            int numberOfUndosNeeded = Mutate(node);
            Thread.Sleep(100);

            writer.WriteLine("### - Beginning undo");
            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.UndoRedoCommand undoCommand =
                        new DynamoModel.UndoRedoCommand(
                            DynamoModel.UndoRedoCommand.Operation.Undo);

                    DynamoViewModel.ExecuteCommand(undoCommand);
                }));
                Thread.Sleep(100);
            }
            writer.WriteLine("### - undo complete");
            writer.Flush();

            writer.WriteLine("### - Beginning re-exec");

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.RunCancelCommand runCancel =
                    new DynamoModel.RunCancelCommand(false, false);

                DynamoViewModel.ExecuteCommand(runCancel);
            }));
            Thread.Sleep(10);
            while (DynamoViewModel.Model.Runner.Running)
            {
                Thread.Sleep(10);
            }

            writer.WriteLine("### - re-exec complete");
            writer.Flush();

            writer.WriteLine("### - Beginning readback");

            writer.WriteLine("### - Beginning test of IntegerSlider");

            if (node.OutPorts.Count > 0)
            {
                try
                {
                    String valmap = valueMap[node.GUID].ToString();
                    Object data = node.GetValue(0).Data;
                    String nodeVal = data != null ? data.ToString() : "null";

                    if (valmap != nodeVal)
                    {
                        writer.WriteLine("!!!!!!!!!!! - test of IntegerSlider is failed");
                        writer.WriteLine(node.GUID);

                        writer.WriteLine("Was: " + nodeVal);
                        writer.WriteLine("Should have been: " + valmap);
                        writer.Flush();
                        return pass;
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine("!!!!!!!!!!! - test of IntegerSlider is failed");
                    writer.Flush();
                    return pass;
                }

            }
            writer.WriteLine("### - test of IntegerSlider complete");
            writer.Flush();

            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);

            Type type = assembly.GetType("Dynamo.Nodes.IntegerSlider");

            PropertyInfo propInfo = type.GetProperty("Min");
            dynamic propertyMin = propInfo.GetValue(node, null);
            propInfo = type.GetProperty("Max");
            dynamic propertyMax = propInfo.GetValue(node, null);

            int min = 0;
            int max = 0;
            int returnCode = 0;
            Random rand = new Random();

            if (Int32.TryParse(propertyMin.ToString(), out min) &&
                Int32.TryParse(propertyMax.ToString(), out max))
            {
                string value = (rand.Next(min, max)).ToString();

                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.UpdateModelValueCommand updateValue =
                        new DynamoModel.UpdateModelValueCommand(node.GUID, "Value", value);
                    DynamoViewModel.ExecuteCommand(updateValue);
                }));

                returnCode = 1;
            }

            return returnCode;
        }        
    }
}