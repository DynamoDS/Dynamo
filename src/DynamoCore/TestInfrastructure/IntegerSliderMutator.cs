using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
namespace Dynamo.TestInfrastructure
{
    class IntegerSliderMutator : AbstractMutator
    {
        public IntegerSliderMutator(Random rand)
            : base(rand)
        {

        }

        public override int Mutate()
        {
            List<NodeModel> nodes = DynamoModel.Nodes.Where(x => x.Name == "Integer Slider").ToList();

            //If there aren't any CBNs, we can't mutate anything
            if (nodes.Count == 0)
                return 0;

            NodeModel node = nodes[Rand.Next(nodes.Count)];

            //string value = "1";
            string value = Rand.Next(100).ToString();

            dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.UpdateModelValueCommand updateValue =
                    new DynamoViewModel.UpdateModelValueCommand(node.GUID, "Value", value);
                DynamoViewModel.ExecuteCommand(updateValue);
            }));

            return 1;
        }
    }
}

