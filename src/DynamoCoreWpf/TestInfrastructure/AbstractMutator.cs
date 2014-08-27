using System;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    abstract class AbstractMutator
    {
        protected Random Rand;

        //Convienence state, the presence of this state cache means that
        //usage of this mutator should be short lived
        protected DynamoViewModel DynamoViewModel;
        protected DynamoModel DynamoModel;

        protected AbstractMutator(DynamoViewModel dynamoViewModel, Random rand)
        {
            this.Rand = rand;
            this.DynamoViewModel = dynamoViewModel;
            this.DynamoModel = dynamoViewModel.Model;
        }

        /// <summary>
        /// Returns the number of undoable operations that have been performed 
        /// </summary>
        /// <returns></returns>
        public abstract int Mutate();

    }
}
