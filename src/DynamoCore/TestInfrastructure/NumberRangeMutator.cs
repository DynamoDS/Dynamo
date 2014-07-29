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

namespace Dynamo.TestInfrastructure
{
    class NumberRangeMutator : AbstractMutator
    {
        public NumberRangeMutator(Random rand)
            : base(rand)
        {

        }

        public override int Mutate()
        {
            string assemblyPass = Environment.CurrentDirectory + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(assemblyPass);
            Type type = assembly.GetType("DSCoreNodesUI.NumberRange");
            List<NodeModel> nodes = new List<NodeModel>();
            if (type != null)
                nodes = DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();
            if (nodes.Count == 0)
                return 0;

            Guid guidNumber1 = Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"); //Guid of node "Number" that connect to node "Number Range" on Start
            Guid guidNumber2 = Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"); //Guid of node "Number" that connect to node "Number Range" on End
            Guid guidNumber3 = Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"); //Guid of node "Number" that connect to node "Number Range" on Step

            foreach (NodeModel n in nodes)
            {
                dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
                {
                    //make connection
                    DynamoViewModel.MakeConnectionCommand connToStart1 =
                        new DynamoViewModel.MakeConnectionCommand(guidNumber1, 0, (PortType)1, (DynamoViewModel.MakeConnectionCommand.Mode)0);
                    DynamoViewModel.MakeConnectionCommand connToStart2 =
                        new DynamoViewModel.MakeConnectionCommand(n.GUID, 0, (PortType)0, (DynamoViewModel.MakeConnectionCommand.Mode)1);

                    DynamoViewModel.MakeConnectionCommand connToAmount1 =
                        new DynamoViewModel.MakeConnectionCommand(guidNumber2, 0, (PortType)1, (DynamoViewModel.MakeConnectionCommand.Mode)0);
                    DynamoViewModel.MakeConnectionCommand connToAmount2 =
                        new DynamoViewModel.MakeConnectionCommand(n.GUID, 1, (PortType)0, (DynamoViewModel.MakeConnectionCommand.Mode)1);

                    DynamoViewModel.MakeConnectionCommand connToStep1 =
                        new DynamoViewModel.MakeConnectionCommand(guidNumber3, 0, (PortType)1, (DynamoViewModel.MakeConnectionCommand.Mode)0);
                    DynamoViewModel.MakeConnectionCommand connToStep2 =
                        new DynamoViewModel.MakeConnectionCommand(n.GUID, 2, (PortType)0, (DynamoViewModel.MakeConnectionCommand.Mode)1);

                    //create connections
                    DynamoViewModel.ExecuteCommand(connToStart1); //"Number" with "Number Range" on Start
                    DynamoViewModel.ExecuteCommand(connToStart2); //"Number" with "Number Range" on Start
                    DynamoViewModel.ExecuteCommand(connToAmount1); //"Number" with "Number Range" on End
                    DynamoViewModel.ExecuteCommand(connToAmount2); //"Number" with "Number Range" on End
                    DynamoViewModel.ExecuteCommand(connToStep1); //"Number" with "Number Range" on Step
                    DynamoViewModel.ExecuteCommand(connToStep2); //"Number" with "Number Range" on Step
                }));
            }

            return 1;
        }
    }
}