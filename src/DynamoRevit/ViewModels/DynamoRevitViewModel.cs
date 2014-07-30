using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    public class DynamoRevitViewModel : ViewModels.DynamoViewModel
    {
        public DynamoRevitViewModel(DynamoModel dynamoModel, IWatchHandler watchHandler, IVisualizationManager vizManager, string commandFilePath) : 
            base(dynamoModel, watchHandler, vizManager, commandFilePath)
    {
        
    }

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

    }
}
