using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamo.TestInfrastructure
{
    class DirectoryPathMutator : AbstractMutator
    {
        public DirectoryPathMutator(Random rand)
            : base(rand)
        {

        }

        public override int Mutate()
        {
            string assemblyPath = Environment.CurrentDirectory + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(assemblyPath);

            List<NodeModel> nodes = new List<NodeModel>();

            Type type = assembly.GetType("DSCore.File.Directory");
            if (type != null)
                nodes = DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

            //If there aren't any Directory nodes, we can't mutate anything
            if (nodes.Count == 0)
                return 0;

            NodeModel node = nodes[Rand.Next(nodes.Count)];

            dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.UpdateModelValueCommand updateValue =
                    new DynamoViewModel.UpdateModelValueCommand(node.GUID, "Value", Environment.CurrentDirectory);

                DynamoViewModel.ExecuteCommand(updateValue);
            }));

            return 1;
        }
    }
}
