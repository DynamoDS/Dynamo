using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    class DoubleSliderMutator : AbstractMutator
    {
        public DoubleSliderMutator(DynamoViewModel viewModel, Random rand)
            : base(viewModel, rand)
        {

        }

        public override int Mutate()
        {
            string assemblyPass = Environment.CurrentDirectory + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(assemblyPass);
            Type type = assembly.GetType("Dynamo.Nodes.DoubleSlider");
            List<NodeModel> nodes = new List<NodeModel>();
            if (type != null)
            {
                nodes = DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();
            }

            //If there aren't any Sliders, we can't mutate anything
            if (nodes.Count == 0)
                return 0;

            NodeModel node = nodes[Rand.Next(nodes.Count)];

            PropertyInfo propInfo = type.GetProperty("Min");
            dynamic propertyMin = propInfo.GetValue(node, null);
            propInfo = type.GetProperty("Max");
            dynamic propertyMax = propInfo.GetValue(node, null);

            double min = 0;
            double max = 0;
            int returnCode = 0;

            if (double.TryParse(propertyMin.ToString(), out min) && double.TryParse(propertyMax.ToString(), out max))
            {
                string value = (min + (max - min) * Rand.NextDouble()).ToString();

                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoViewModel.UpdateModelValueCommand updateValue =
                        new DynamoViewModel.UpdateModelValueCommand(node.GUID, "Value", value);
                    DynamoViewModel.ExecuteCommand(updateValue);
                }));

                returnCode = 1;
            }

            return returnCode;
        }
    }
}
