using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Revit;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Dynamo.Applications
{
    class DynamoRunner_Revit : Dynamo.Core.DynamoRunner
    {
        private DynamoController_Revit controllerRevit;

        public DynamoRunner_Revit(DynamoController_Revit controllerRevit)
        {
            this.controllerRevit = controllerRevit;
        }


        protected override void OnRunCancelled(bool error)
        {
            base.OnRunCancelled(error);

            if (controllerRevit.transaction != null && controllerRevit.transaction.Status == TransactionStatus.Started)
                controllerRevit.transaction.CancelTransaction();
        }


        protected override void Evaluate()
        {
            //DocumentManager.Instance.CurrentDBDocument = DocumentManager.Instance.CurrentUIDocument.Document;

            if (controllerRevit.DynamoViewModel.RunInDebug)
            {
                controllerRevit.TransMode = TransactionMode.Debug; //Debug transaction control
                dynSettings.DynamoLogger.Log("Running expression in debug.");
            }
            else
            {
                // As we use a generic function node to represent all functions,
                // we don't know if a node has something to do with Revit or 
                // not, neither we know that the re-execution of a dirty node
                // will trigger the update for a Revit related node. Now just
                // run the execution in the idle thread until we find out a 
                // way to control the execution.
                controllerRevit.TransMode = TransactionMode.Automatic; //Automatic transaction control
                Debug.WriteLine("Adding a run to the idle stack.");
            }

            //Run in idle thread no matter what
            RevitServices.Threading.IdlePromise.ExecuteOnIdleSync(base.Evaluate);
        }
        

    }
}
