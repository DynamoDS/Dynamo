﻿using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("Number Input Mutator")]
    class NumberInputMutator : AbstractMutator
    {
        public NumberInputMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            return null; //typeof(DoubleInput);
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

            ExecuteAndWait();

            writer.WriteLine("### - re-exec complete");
            writer.Flush();

            writer.WriteLine("### - Beginning readback");

            writer.WriteLine("### - Beginning test of Number");
            if (node.OutPorts.Count > 0)
            {
                try
                {
                    String valmap = valueMap[node.GUID].ToString();
                    Object data = node.GetValue(0, engine).Data;
                    String nodeVal = data != null ? data.ToString() : "null";

                    if (valmap != nodeVal)
                    {
                        writer.WriteLine("!!!!!!!!!!! - test of Number is failed");
                        writer.WriteLine(node.GUID);

                        writer.WriteLine("Was: " + nodeVal);
                        writer.WriteLine("Should have been: " + valmap);
                        writer.Flush();
                        return pass;
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine("!!!!!!!!!!! - test of Number is failed");
                    writer.Flush();
                    return pass;
                }
            }
            writer.WriteLine("### - test of Number complete");
            writer.Flush();

            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            Random rand = new Random(1);
            string value = rand.Next(100).ToString();

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.UpdateModelValueCommand updateValue =
                    new DynamoModel.UpdateModelValueCommand(System.Guid.Empty, node.GUID, "Value", value);

                DynamoViewModel.ExecuteCommand(updateValue);
            }));

            return 1;
        }        
    }
}
