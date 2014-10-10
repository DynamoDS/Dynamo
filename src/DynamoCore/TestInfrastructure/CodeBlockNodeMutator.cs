using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("CodeBlockNodeMutator")]
    class CodeBlockNodeMutator : AbstractMutator
    {
        public CodeBlockNodeMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            return typeof(CodeBlockNodeModel);
        }

        public override bool RunTest(NodeModel node, StreamWriter writer)
        {
            bool pass = false;        

            writer.WriteLine("### - Beginning readout");

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

            writer.WriteLine("### - Readout complete");
            writer.Flush();

            writer.WriteLine("### - Beginning delete");
            writer.WriteLine("### - Deletion target: " + node.GUID);

            int numberOfUndosNeeded = Mutate(node);
            Thread.Sleep(100);

            writer.WriteLine("### - delete complete");
            writer.Flush();

            writer.WriteLine("### - Beginning re-exec");

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.RunCancelCommand runCancel =
                    new DynamoModel.RunCancelCommand(false, false);

                DynamoViewModel.ExecuteCommand(runCancel);
            }));
            Thread.Sleep(100);

            writer.WriteLine("### - re-exec complete");
            writer.Flush();

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
            }
            Thread.Sleep(100);

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

            if (node.OutPorts.Count > 0)
            {
                try
                {
                    String valmap = valueMap[node.GUID].ToString();
                    Object data = node.GetValue(0).Data;
                    String nodeVal = data != null ? data.ToString() : "null";

                    if (valmap != nodeVal)
                    {
                        writer.WriteLine("!!!!!!!!!!! Read-back failed");
                        writer.WriteLine(node.GUID);

                        writer.WriteLine("Was: " + nodeVal);
                        writer.WriteLine("Should have been: " + valmap);
                        writer.Flush();
                        return pass;
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine("!!!!!!!!!!! Read-back failed");
                    writer.Flush();
                    return pass;
                }
            }

            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            Random Rand = new Random(1);

            this.DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    string code = ((CodeBlockNodeModel)node).Code;

                    if (code.Length == 0)
                        code = "";

                    string replacement;

                    if (Rand.NextDouble() <= 0.5)
                    {
                        //Strategy 1: Replacement with simplest minimal replacement

                        replacement = "1;";
                    }
                    else
                    {
                        //Strategy 2: Noise injection

                        replacement = code;

                        while (Rand.NextDouble() > 0.5)
                        {
                            int locat = Rand.Next(code.Length);
                            const string junk = "<>:L/;'\\/[=+-";

                            replacement = code.Substring(0, locat) + junk[Rand.Next(junk.Length)] +
                                          code.Substring(locat);
                        }
                    }

                    var cmd = new DynamoModel.UpdateModelValueCommand(node.GUID, "Code", replacement);

                    this.DynamoViewModel.ExecuteCommand(cmd);
                }));

            //We've performed a single edit from the perspective of undo
            return 1;
        }
    }
}
