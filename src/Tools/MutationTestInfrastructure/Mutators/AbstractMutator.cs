using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.IO;
using Dynamo.DSEngine;
using System.Threading;

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
                                                                                                   

        public abstract bool RunTest(NodeModel node, EngineController engine, StreamWriter writer);

        public virtual Type GetNodeType()
        {
            return typeof(NodeModel);
        }

        /// <summary>
        /// Number of times to repeat a specific mutation
        /// </summary>
        public virtual int NumberOfLaunches
        {
            get { return 10; }
        }




        // TODO(lukechurch): Move this to a support class
        protected void ExecuteAndWait()
        {
            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                DynamoModel.RunCancelCommand runCancel =
                    new DynamoModel.RunCancelCommand(false, false);
                DynamoViewModel.ExecuteCommand(runCancel);
            }));

            SpinWaitForExecution();
        }

        // TODO(lukechurch): Move this to use event waiting rather than spin waiting
        protected void SpinWaitForExecution()
        {
            while (!DynamoViewModel.HomeSpace.RunSettings.RunEnabled)
            {
                Thread.Sleep(10);
            }
        }
    }
}
