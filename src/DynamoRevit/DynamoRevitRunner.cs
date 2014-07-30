#region
using System.Diagnostics;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using RevitServices.Threading;
using RevitServices.Transactions;

#endregion

namespace Dynamo.Applications
{
    internal class DynamoRevitRunner : DynamoRunner
    {
        private readonly DynamoRevitModel dynamoRevitModel;

        public TransactionMode TransMode
        {
            get
            {
                if (TransactionManager.Instance.Strategy is AutomaticTransactionStrategy)
                    return TransactionMode.Automatic;

                return TransactionMode.Manual;
            }
            set
            {
                switch (value)
                {
                    case TransactionMode.Automatic:
                        TransactionManager.Instance.Strategy = new AutomaticTransactionStrategy();
                        break;
                    default:
                        TransactionManager.Instance.Strategy = new DebugTransactionStrategy();
                        break;
                }

                dynamoRevitModel.RunInDebug = value == TransactionMode.Manual;
            }
        }

        public DynamoRevitRunner(DynamoRevitModel dynamoRevitModel)
            : base(dynamoRevitModel)
        {
            this.dynamoRevitModel = dynamoRevitModel;
        }
         
        protected override void OnRunCancelled(bool error)
        {
            base.OnRunCancelled(error);

            if (dynamoRevitModel.transaction != null
                && dynamoRevitModel.transaction.Status == TransactionStatus.Started)
                dynamoRevitModel.transaction.CancelTransaction();
        }

        protected override void Evaluate()
        {
            if (dynamoRevitModel.RunInDebug)
            {
                TransMode = TransactionMode.Manual; //Debug transaction control
            }
            else
            {
                // As we use a generic function node to represent all functions,
                // we don't know if a node has something to do with Revit or 
                // not, neither we know that the re-execution of a dirty node
                // will trigger the update for a Revit related node. Now just
                // run the execution in the idle thread until we find out a 
                // way to control the execution.
                TransMode = TransactionMode.Automatic;
                    //Automatic transaction control
                Debug.WriteLine("Adding a run to the idle stack.");
            }

            //Run in idle thread no matter what
            IdlePromise.ExecuteOnIdleSync(base.Evaluate);
        }
    }
}
