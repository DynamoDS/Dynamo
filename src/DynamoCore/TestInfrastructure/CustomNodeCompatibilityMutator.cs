using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.TestInfrastructure
{
    class CustomNodeCompatibilityMutator : AbstractMutator
    {
        public CustomNodeCompatibilityMutator(DynamoViewModel viewModel, Random rand)
            : base(viewModel, rand)
        {

        }

        public override int Mutate()
        {
            List<NodeModel> customNodes = DynamoModel.Nodes.Where(t => t.GetType() == typeof(Function)).ToList();
            NodeModel lastNode = DynamoModel.Nodes.ToList().Last();

            //If there aren't any Custom nodes, we can't mutate anything
            if (customNodes.Count == 0)
                return 0;

            NodeModel node = customNodes[Rand.Next(customNodes.Count)];

            if (lastNode.OutPorts.Count > 0)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoViewModel.MakeConnectionCommand connectCmd1 =
                        new DynamoViewModel.MakeConnectionCommand(lastNode.GUID, 0, PortType.OUTPUT, DynamoViewModel.MakeConnectionCommand.Mode.Begin);
                    DynamoViewModel.MakeConnectionCommand connectCmd2 =
                        new DynamoViewModel.MakeConnectionCommand(node.GUID, 0, PortType.INPUT, DynamoViewModel.MakeConnectionCommand.Mode.End);

                    DynamoViewModel.ExecuteCommand(connectCmd1);
                    DynamoViewModel.ExecuteCommand(connectCmd2);
                }));
            }
            else
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoViewModel.MakeConnectionCommand connectCmd1 =
                        new DynamoViewModel.MakeConnectionCommand(node.GUID, 0, PortType.OUTPUT, DynamoViewModel.MakeConnectionCommand.Mode.Begin);
                    DynamoViewModel.MakeConnectionCommand connectCmd2 =
                        new DynamoViewModel.MakeConnectionCommand(lastNode.GUID, 0, PortType.INPUT, DynamoViewModel.MakeConnectionCommand.Mode.End);

                    DynamoViewModel.ExecuteCommand(connectCmd1);
                    DynamoViewModel.ExecuteCommand(connectCmd2);
                }));
            }

            return 2;
        }
    }
}