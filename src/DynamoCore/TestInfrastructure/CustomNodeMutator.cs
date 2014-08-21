using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace Dynamo.TestInfrastructure
{
    class CustomNodeMutator : AbstractMutator
    {
        private int workspaceIndex = 0;

        public CustomNodeMutator(DynamoViewModel viewModel, int workspaceIndex, Random rand)
            : base(viewModel, rand)
        {
            this.workspaceIndex = workspaceIndex;
        }

        public override int Mutate()
        {
            int customNodeWorkspaceIndex = DynamoViewModel.CurrentWorkspaceIndex;

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.SwitchTabCommand switchCmd =
                    new DynamoViewModel.SwitchTabCommand(workspaceIndex);

                DynamoViewModel.ExecuteCommand(switchCmd);
                Thread.Sleep(100);
            }));

            List<NodeModel> customNodes = DynamoModel.Nodes.Where(t => t.GetType() == typeof(Function)).ToList();

            if (customNodes.Count == 0)
                return 0;

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.SwitchTabCommand switchCmd =
                    new DynamoViewModel.SwitchTabCommand(customNodeWorkspaceIndex);

                DynamoViewModel.ExecuteCommand(switchCmd);
                Thread.Sleep(100);
            }));

            NodeModel customNode = customNodes[Rand.Next(customNodes.Count)];

            var workspaces = DynamoModel.Workspaces;
            List<NodeModel> outputsInCustomNode = workspaces.FirstOrDefault(t => t.Name == ((Function)customNode).Definition.WorkspaceModel.Name).Nodes.Where(t => t.GetType() == typeof(Output)).ToList();

            Guid numberGuid = Guid.NewGuid();
            double coordinatesX = Rand.NextDouble() * customNode.X;
            double coordinatesY = Rand.NextDouble() * customNode.Y;

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.CreateNodeCommand createCommand =
                    new DynamoViewModel.CreateNodeCommand(numberGuid, "Number", coordinatesX, coordinatesY, false, false);
                DynamoViewModel.ExecuteCommand(createCommand);
            }));

            foreach (NodeModel output in outputsInCustomNode)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoViewModel.MakeConnectionCommand connToAnother1 =
                        new DynamoViewModel.MakeConnectionCommand(numberGuid, 0, PortType.OUTPUT, DynamoViewModel.MakeConnectionCommand.Mode.Begin);
                    DynamoViewModel.MakeConnectionCommand connToAnother2 =
                        new DynamoViewModel.MakeConnectionCommand(output.GUID, 0, PortType.INPUT, DynamoViewModel.MakeConnectionCommand.Mode.End);

                    DynamoViewModel.ExecuteCommand(connToAnother1);
                    DynamoViewModel.ExecuteCommand(connToAnother2);
                }));
            }

            int numberOfUndosNeeded = outputsInCustomNode.Count * 2 + 1;

            return numberOfUndosNeeded;
        }
    }
}