using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System.Reflection;
using System.IO;
namespace Dynamo.TestInfrastructure
{
    class IntegerSliderMutator : AbstractMutator
    {
        public IntegerSliderMutator(DynamoViewModel viewModel, Random rand)
            : base(viewModel, rand)
        {

        }

        public override int Mutate()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);

            Type type = assembly.GetType("Dynamo.Nodes.IntegerSlider");
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

            int min = 0;
            int max = 0;
            int returnCode = 0;

            if (Int32.TryParse(propertyMin.ToString(), out min) && Int32.TryParse(propertyMax.ToString(), out max))
            {
                string value = (Rand.Next(min, max)).ToString();

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