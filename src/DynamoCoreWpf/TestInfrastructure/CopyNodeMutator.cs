using System;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ModifierKeys = System.Windows.Input.ModifierKeys;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("CopyNodeMutator")]
    class CopyNodeMutator : AbstractMutator
    {
        public CopyNodeMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override bool RunTest(NodeModel node, EngineController engine, StreamWriter writer)
        {
            const bool pass = false;

            var nodes = DynamoViewModel.Model.CurrentWorkspace.Nodes;
            if (nodes.Count() == 0)
                return pass;

            int nodesCountBeforeCopying = nodes.Count();

            int numberOfUndosNeeded = this.Mutate(node);
            Thread.Sleep(10);

            writer.WriteLine("### - Beginning undo");
            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.UndoRedoCommand undoCommand =
                        new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
                    DynamoViewModel.ExecuteCommand(undoCommand);

                }));
                Thread.Sleep(0);
            }
            writer.WriteLine("### - undo complete");
            writer.Flush();

            ExecuteAndWait();

            writer.WriteLine("### - Beginning test of CopyNode");
            if (node.OutPorts.Count > 0)
            {
                try
                {
                    int nodesCountAfterCopying = DynamoViewModel.Model.CurrentWorkspace.Nodes.Count();

                    if (nodesCountBeforeCopying != nodesCountAfterCopying)
                    {
                        writer.WriteLine("!!!!!!!!!!! - test of CopyNode is failed");
                        writer.WriteLine(node.GUID);

                        writer.WriteLine("Was: " + nodesCountAfterCopying);
                        writer.WriteLine("Should have been: " + nodesCountBeforeCopying);
                        writer.Flush();
                        return pass;
                    }
                    else
                    {
                        writer.WriteLine("### - test of CopyNode is passed");
                        writer.Flush();
                    }

                }
                catch (Exception)
                {
                    writer.WriteLine("!!!!!!!!!!! - test of CopyNode is failed");
                    writer.Flush();
                    return pass;
                }
            }
            writer.WriteLine("### - test of CopyNode complete");
            writer.Flush();

            return true;
        }

        public override int Mutate(NodeModel node)
        {
            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.SelectModelCommand selectNodeCommand =
                    new DynamoModel.SelectModelCommand(node.GUID, ModifierKeys.None.AsDynamoType());

                DynamoViewModel.ExecuteCommand(selectNodeCommand);

                DynamoModel.Copy();
                DynamoModel.Paste();
            }));

            return 1;
        }        
    }
}
