using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.IO;

namespace Dynamo.TestInfrastructure
{
    abstract class AbstractMutator
    {
        //Convienence state, the presence of this state cache means that
        //usage of this mutator should be short lived
        protected DynamoViewModel DynamoViewModel;
        protected DynamoModel DynamoModel;

        protected AbstractMutator(DynamoViewModel dynamoViewModel)
        {
            this.DynamoViewModel = dynamoViewModel;
            this.DynamoModel = dynamoViewModel.Model;
        }

        /// <summary>
        /// Returns the number of undoable operations that have been performed 
        /// </summary>
        /// <returns></returns>
        public abstract int Mutate(NodeModel node);
                                                                                                   

        public abstract bool RunTest(NodeModel node, StreamWriter writer);

        public virtual Type GetNodeType()
        {
            return typeof(NodeModel);
        }

        public virtual int NumberOfLaunches
        {
            get { return 1000; }
        }
    }
}
