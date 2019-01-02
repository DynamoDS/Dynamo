using System;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    /// <summary>
    /// Mutator that deletes a random node
    /// </summary>

    [MutationTest("DeleteNodeMutator")]
    class DeleteNodeMutator : AbstractMutator
    {
        public DeleteNodeMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override bool RunTest(NodeModel node, EngineController engine, StreamWriter writer)
        {
            bool pass = false;

            var nodes = DynamoViewModel.Model.CurrentWorkspace.Nodes;
            if (nodes.Count() == 0)
                return true;

            int nodesCountBeforeDelete = nodes.Count();

            writer.WriteLine("### - Beginning readout");
            
            writer.WriteLine("### - Beginning delete");
            writer.WriteLine("### - Deletion target: " + node.GUID);

            int numberOfUndosNeeded = Mutate(node);

            Thread.Sleep(0);

            writer.WriteLine("### - delete complete");
            writer.Flush();

            writer.WriteLine("### - Beginning re-exec");

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.RunCancelCommand runCancel =
                    new DynamoModel.RunCancelCommand(false, false);

                DynamoViewModel.ExecuteCommand(runCancel);
            }));
            Thread.Sleep(0);

            writer.WriteLine("### - re-exec complete");
            writer.Flush();

            writer.WriteLine("### - Beginning undo");

            int nodesCountAfterDelete = DynamoViewModel.Model.CurrentWorkspace.Nodes.Count();

            if (nodesCountBeforeDelete > nodesCountAfterDelete)
            {
                for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                {
                    DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoModel.UndoRedoCommand undoCommand =
                            new DynamoModel.UndoRedoCommand(
                                DynamoModel.UndoRedoCommand.Operation.Undo);

                        DynamoViewModel.ExecuteCommand(undoCommand);
                    }));
                    Thread.Sleep(0);
                }
                writer.WriteLine("### - undo complete");
                writer.Flush();

                writer.WriteLine("### - Beginning re-exec");

                ExecuteAndWait();
                writer.WriteLine("### - re-exec complete");
                writer.Flush();

                int nodesCountAfterUbdo = DynamoViewModel.Model.CurrentWorkspace.Nodes.Count();
                if (nodesCountBeforeDelete == nodesCountAfterUbdo)
                    writer.WriteLine("### - Node was restored");
                else
                {
                    writer.WriteLine("### - Node wasn't restored");
                    return pass;
                }
                writer.Flush();
            }
            else
            {
                writer.WriteLine("### - Error removing a node");
                writer.Flush();
                return pass;
            }
            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            this.DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.DeleteModelCommand delCommand =
                        new DynamoModel.DeleteModelCommand(node.GUID);
                
                    DynamoViewModel.ExecuteCommand(delCommand);
                }));

            //We've performed a single delete
            return 1;
        }
    }
}