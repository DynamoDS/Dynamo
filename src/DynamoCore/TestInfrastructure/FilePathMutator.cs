using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dynamo.TestInfrastructure
{
    class FilePathMutator : AbstractMutator
    {
        public FilePathMutator(DynamoViewModel viewModel, Random rand)
            : base(viewModel, rand)
        {

        }

        public override int Mutate()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);

            List<NodeModel> nodes = new List<NodeModel>();

            Type type = assembly.GetType("DSCore.File.Filename");
            if (type != null)
                nodes = DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

            //If there aren't any File nodes, we can't mutate anything
            if (nodes.Count == 0)
                return 0;

            NodeModel node = nodes[Rand.Next(nodes.Count)];

            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.UpdateModelValueCommand updateValue =
                    new DynamoViewModel.UpdateModelValueCommand(node.GUID, "Value", assemblyPath);

                DynamoViewModel.ExecuteCommand(updateValue);
            }));

            return 1;
        }
    }
}
