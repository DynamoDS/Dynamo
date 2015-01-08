using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Dynamo.DSEngine;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("StringInputMutator")]
    class StringInputMutator : AbstractMutator
    {
        public StringInputMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            return null;//typeof(StringInput);
        }

        public override bool RunTest(NodeModel node, EngineController engine, StreamWriter writer)
        {
            bool pass = false;

            var valueMap = new Dictionary<Guid, String>();
            if (node.OutPorts.Count > 0)
            {
                Guid guid = node.GUID;
                Object data = node.GetValue(0, engine).Data;
                String val = data != null ? data.ToString() : "null";
                valueMap.Add(guid, val);
                writer.WriteLine(guid + " :: " + val);
                writer.Flush();
            }

            int numberOfUndosNeeded = Mutate(node);
            Thread.Sleep(100);

            writer.WriteLine(/*NXLT*/"### - Beginning undo");
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
            writer.WriteLine(/*NXLT*/"### - undo complete");
            writer.Flush();

            writer.WriteLine(/*NXLT*/"### - undo complete");
            writer.Flush();
            writer.WriteLine(/*NXLT*/"### - Beginning re-exec");

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.RunCancelCommand runCancel =
                    new DynamoModel.RunCancelCommand(false, false);

                DynamoViewModel.ExecuteCommand(runCancel);
            }));
            Thread.Sleep(10);

            while (!DynamoViewModel.HomeSpace.RunEnabled)
            {
                Thread.Sleep(10);
            }
            writer.WriteLine(/*NXLT*/"### - re-exec complete");
            writer.Flush();
            writer.WriteLine(/*NXLT*/"### - Beginning readback");

            writer.WriteLine(/*NXLT*/"### - Beginning test of String");
            if (node.OutPorts.Count > 0)
            {
                try
                {
                    String valmap = valueMap[node.GUID].ToString();
                    Object data = node.GetValue(0, engine).Data;
                    String nodeVal = data != null ? data.ToString() : "null";

                    if (valmap != nodeVal)
                    {
                        writer.WriteLine(/*NXLT*/"!!!!!!!!!!! - test of String is failed");
                        writer.WriteLine(node.GUID);

                        writer.WriteLine(/*NXLT*/"Was: " + nodeVal);
                        writer.WriteLine(/*NXLT*/"Should have been: " + valmap);
                        writer.Flush();
                        return pass;
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine(/*NXLT*/"!!!!!!!!!!! - test of String is failed");
                    writer.Flush();
                    return pass;
                }
            }
            writer.WriteLine(/*NXLT*/"### - test of Number complete");
            writer.Flush();

            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            string chars = /*NXLT*/"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*(),./[];=-:<\\>?";
            Random random = new Random();
            string value = new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.UpdateModelValueCommand updateValue =
                    new DynamoModel.UpdateModelValueCommand(node.GUID, "Value", value);

                DynamoViewModel.ExecuteCommand(updateValue);
            }));

            return 1;
        }        
    }
}
