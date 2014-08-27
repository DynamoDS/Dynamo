using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    class CopyNodeMutator : AbstractMutator
    {
        public CopyNodeMutator(DynamoViewModel viewModel, Random rand)
            : base(viewModel, rand)
        {

        }

        public override int Mutate()
        {
            List<NodeModel> nodes = DynamoModel.Nodes.ToList();

            if (nodes.Count == 0)
                return 0;

            NodeModel node = nodes[Rand.Next(nodes.Count)];

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.SelectModelCommand selectNodeCommand =
                    new DynamoViewModel.SelectModelCommand(node.GUID, ModifierKeys.None);

                DynamoViewModel.ExecuteCommand(selectNodeCommand);

                DynamoModel.Copy(null);
                DynamoModel.Paste(null);
            }));

            return 1;
        }
    }
}
