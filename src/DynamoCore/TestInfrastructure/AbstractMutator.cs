using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    abstract class AbstractMutator
    {
        protected Random rand;

        //Convienence state, the presence of this state cache means that
        //usage of this mutator should be short lived
        protected DynamoController controller;
        protected DynamoViewModel dynamoViewModel;
        protected DynamoModel dynamoModel;

        protected AbstractMutator(Random rand)
        {
            this.rand = rand;
            this.controller = dynSettings.Controller;
            this.dynamoViewModel = controller.DynamoViewModel;
            this.dynamoModel = controller.DynamoModel;
        }

        /// <summary>
        /// Returns the number of undoable operations that have been performed 
        /// </summary>
        /// <returns></returns>
        public abstract int Mutate();

    }


    class DeleteNodeMutator : AbstractMutator
    {
        public DeleteNodeMutator(Random rand) : base(rand)
        {
        }

        public override int Mutate()
        {
            List<NodeModel> nodes = dynamoModel.Nodes;


            NodeModel node = nodes[rand.Next(nodes.Count)];
            //writer.WriteLine("### - Deletion target: " + node.GUID);

            dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoViewModel.DeleteModelCommand delCommand =
                    new DynamoViewModel.DeleteModelCommand(node.GUID);
                
                
                dynamoViewModel.ExecuteCommand(delCommand);

            }));

            //We've performed a single delete
            return 1;

        }

        
    }
}
