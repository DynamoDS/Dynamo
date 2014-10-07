using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("FilePathMutator")]
    class FilePathMutator : AbstractMutator
    {
        public FilePathMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);
            Type type = assembly.GetType("DSCore.File.Filename");

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

            writer.WriteLine("### - Beginning test of FilePath");
            if (node.OutPorts.Count > 0)
            {
                try
                {
                    String valmap = valueMap[node.GUID].ToString();
                    Object data = node.GetValue(0).Data;
                    String nodeVal = data != null ? data.ToString() : "null";

                    if (valmap != nodeVal)
                    {
                        writer.WriteLine("!!!!!!!!!!! - test of FilePath is failed");
                        writer.WriteLine(node.GUID);

                        writer.WriteLine("Was: " + nodeVal);
                        writer.WriteLine("Should have been: " + valmap);
                        writer.Flush();
                        return pass;
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine("!!!!!!!!!!! - test of FilePath is failed");
                    writer.Flush();
                    return pass;
                }
            }
            writer.WriteLine("### - test of FilePath complete");
            writer.Flush();

            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.UpdateModelValueCommand updateValue =
                    new DynamoModel.UpdateModelValueCommand(node.GUID, "Value", assemblyPath);

                DynamoViewModel.ExecuteCommand(updateValue);
            }));

            return 1;
        }
    }
}
