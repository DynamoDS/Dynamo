using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    public class DynamoRevitViewModel : DynamoViewModel
    {
        public DynamoRevitViewModel(DynamoController controller, string commandFilePath) : base(controller, commandFilePath) { }

        public override bool CanRunDynamically
        {
            get
            {
                return canRunDynamically;
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

                if (debug)
                    DynamicRunEnabled = false;

                RaisePropertyChanged("RunInDebug");
            }
        }

        //public override Function CreateFunction(CustomNodeDefinition customNodeDefinition)
        //{
        //    if (customNodeDefinition.WorkspaceModel.Nodes.Any(x => x is RevitTransactionNode)
        //        || customNodeDefinition.Dependencies.Any(d => d.WorkspaceModel.Nodes.Any(x => x is RevitTransactionNode)))
        //    {
        //        return new FunctionWithRevit(customNodeDefinition);
        //    }
        //    return base.CreateFunction(customNodeDefinition);
        //}

        //bool ExecutionRequiresManualTransaction()
        //{
        //    //if there are no topmost nodes, just return false
        //    //this will avoid a binding error during bench initialization
        //    return Model.HomeSpace.GetTopMostNodes().Any()
        //           && Model.HomeSpace.GetTopMostNodes().Any(
        //           ((DynamoController_Revit)Controller).CheckManualTransaction.TraverseUntilAny);
        //}
    }
}
