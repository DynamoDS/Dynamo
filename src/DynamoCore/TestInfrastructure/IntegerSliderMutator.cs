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
        public IntegerSliderMutator(Random rand)
            : base(rand)
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

