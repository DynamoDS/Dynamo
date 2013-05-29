using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Revit;

namespace Dynamo.Controls
{
    class DynamoRevitViewModel : DynamoViewModel
    {
        PredicateTraverser checkManualTransaction;
        PredicateTraverser checkRequiresTransaction;

        public PredicateTraverser CheckManualTransaction
        {
            get { return checkManualTransaction; }
            set { checkManualTransaction = value; }
        }

        public PredicateTraverser CheckRequiresTransaction
        {
            get { return checkRequiresTransaction; }
            set { checkRequiresTransaction = value; }
        }

        public enum TransactionMode
        {
            Automatic,
            Manual,
            Debug
        }

        private TransactionMode transMode;
        public TransactionMode TransMode
        {
            get { return transMode; }
            set
            {
                transMode = value;
                if (transMode == TransactionMode.Debug)
                {
                    RunInDebug = true;
                }

                RaisePropertyChanged("TransMode");
            }
        }

        public DynamoRevitViewModel(DynamoController controller):base(controller)
        {
            Predicate<dynNodeModel> requiresTransactionPredicate = delegate(dynNodeModel node)
            {
                return node is dynRevitTransactionNode;
            };
            checkRequiresTransaction = new PredicateTraverser(requiresTransactionPredicate);

            Predicate<dynNodeModel> manualTransactionPredicate = delegate(dynNodeModel node)
            {
                return node is dynTransaction;
            };

            checkManualTransaction = new PredicateTraverser(manualTransactionPredicate);
        }

        public override bool CanRunDynamically
        {
            get
            {
                //we don't want to be able to run
                //dynamically if we're in debug mode
                bool manTran = ExecutionRequiresManualTransaction();
                return !manTran && !debug;
            }
            set
            {
                canRunDynamically = value;
                RaisePropertyChanged("CanRunDynamically");
            }
        }

        public override bool DynamicRunEnabled
        {
            get
            {
                return dynamicRun;
            }
            set
            {
                dynamicRun = value;
                RaisePropertyChanged("DynamicRunEnabled");
            }
        }

        public override bool RunInDebug
        {
            get { return debug; }
            set
            {
                debug = value;

                //toggle off dynamic run
                CanRunDynamically = !debug;

                if (debug == true)
                    DynamicRunEnabled = false;

                RaisePropertyChanged("RunInDebug");
            }
        }

        public override dynFunction CreateFunction(IEnumerable<string> inputs, IEnumerable<string> outputs, FunctionDefinition functionDefinition)
        {
            if (functionDefinition.Workspace.Nodes.Any(x => x is dynRevitTransactionNode)
                || functionDefinition.Dependencies.Any(d => d.Workspace.Nodes.Any(x => x is dynRevitTransactionNode)))
            {
                return new dynFunctionWithRevit(inputs, outputs, functionDefinition);
            }
            return base.CreateFunction(inputs, outputs, functionDefinition);
        }

        bool ExecutionRequiresManualTransaction()
        {
            //if there are no topmost nodes, just return false
            //this will avoid a binding error during bench initialization
            if (Model.HomeSpace.GetTopMostNodes().Count() > 0)
            {
                return Model.HomeSpace.GetTopMostNodes().Any(
                    checkManualTransaction.TraverseUntilAny
                );
            }
            else
            {
                return false;
            }
        }

    }
}
