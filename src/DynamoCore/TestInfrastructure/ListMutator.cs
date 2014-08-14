using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System.IO;

namespace Dynamo.TestInfrastructure
{
    class ListMutator : AbstractMutator
    {
        public ListMutator(DynamoViewModel viewModel, Random rand)
            : base(viewModel, rand)
        {

        }

        public override int Mutate()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);

            Type type = assembly.GetType("DSCoreNodesUI.CreateList");
            List<NodeModel> nodes = new List<NodeModel>();
            if (type != null)
                nodes = DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();
            if (nodes.Count == 0)
                return 0;

            Guid guidNumber1 = Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"); //Guid of the node "Number" to connect to the node "List" on InPorts(0)

            foreach (NodeModel n in nodes)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    //create commands
                    DynamoViewModel.MakeConnectionCommand connToStart1 =
                        new DynamoViewModel.MakeConnectionCommand(guidNumber1, 0, (PortType)1, (DynamoViewModel.MakeConnectionCommand.Mode)0);
                    DynamoViewModel.MakeConnectionCommand connToStart2 =
                        new DynamoViewModel.MakeConnectionCommand(n.GUID, 0, (PortType)0, (DynamoViewModel.MakeConnectionCommand.Mode)1);

                    //execute commands
                    DynamoViewModel.ExecuteCommand(connToStart1); //"Number" with "List" on InPort(0)
                    DynamoViewModel.ExecuteCommand(connToStart2); //"Number" with "List" on InPort(0)                        
                }));

            }

            return 1;
        }
    }
}