using System;
using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    /// <summary>
    /// Mutator that deletes a random node
    /// </summary>
    class DeleteNodeMutator : AbstractMutator
    {
        public DeleteNodeMutator(Random rand) : base(rand)
        {
        }

        public override int Mutate()
        {
            List<NodeModel> nodes = DynamoModel.Nodes;
            NodeModel node = nodes[Rand.Next(nodes.Count)];

            dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoViewModel.DeleteModelCommand delCommand =
                        new DynamoViewModel.DeleteModelCommand(node.GUID);
                
                    DynamoViewModel.ExecuteCommand(delCommand);

                }));

            //We've performed a single delete
            return 1;

        }

        
    }
}