using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    public class DynamoRevitViewModel : DynamoViewModel
    {
        public DynamoRevitViewModel(DynamoModel dynamoModel, string commandFilePath) : base(dynamoModel, commandFilePath) { }

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
