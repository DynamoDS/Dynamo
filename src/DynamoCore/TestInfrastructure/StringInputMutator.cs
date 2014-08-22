using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.TestInfrastructure
{
    class StringInputMutator : AbstractMutator
    {
        public StringInputMutator(DynamoViewModel viewModel, Random rand)
            : base(viewModel, rand)
        {

        }

        public override int Mutate()
        {
            List<NodeModel> nodes = DynamoModel.Nodes.Where(t => t.GetType() == typeof(StringInput)).ToList();
            if (nodes.Count == 0)
                return 0;

            NodeModel node = nodes[Rand.Next(nodes.Count)];

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*(),./[];=-:<\\>?";
            Random random = new Random();
            string value = new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.UpdateModelValueCommand updateValue =
                    new DynamoViewModel.UpdateModelValueCommand(node.GUID, "Value", value);

                DynamoViewModel.ExecuteCommand(updateValue);
            }));

            return 1;
        }
    }
}
