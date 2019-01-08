using System;
using System.IO;
using System.Threading;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    abstract class AbstractMutator
    {
        //Convienence state, the presence of this state cache means that
        //usage of this mutator should be short lived
        protected readonly DynamoViewModel DynamoViewModel;
        protected readonly DynamoModel DynamoModel;

        private const int MAX_TIME_WAIT = 20000;
        private readonly ManualResetEvent executionWaitHandle = new ManualResetEvent(false);

        protected AbstractMutator(DynamoViewModel dynamoViewModel)
        {
            this.DynamoViewModel = dynamoViewModel;
            this.DynamoModel = dynamoViewModel.Model;
            this.DynamoModel.EvaluationCompleted += (s, e) => executionWaitHandle.Set();
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
            executionWaitHandle.Reset();
            
            DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                var runCancel = new DynamoModel.RunCancelCommand(false, false);
                DynamoViewModel.ExecuteCommand(runCancel);
            }));

            WaitForExecution();
        }

        private void WaitForExecution()
        {
            if (!executionWaitHandle.WaitOne(MAX_TIME_WAIT))
            {
                throw new TimeoutException("Execution has been taking too long. " +
                        "Mutation Test timed out: " + MAX_TIME_WAIT + " ms");
            }
        }
    }
}
