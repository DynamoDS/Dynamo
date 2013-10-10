using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    public class HomeWorkspaceModel : WorkspaceModel
    {
        public HomeWorkspaceModel()
            : this(new List<NodeModel>(), new List<ConnectorModel>(), 0, 0)
        {
        }

        public HomeWorkspaceModel(double x, double y)
            : this(new List<NodeModel>(), new List<ConnectorModel>(), x, y)
        {
        }

        public HomeWorkspaceModel(IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
            : base("Home", e, c, x, y)
        {
        }

        public override void Modified()
        {
            base.Modified();

            var controller = dynSettings.Controller;
            if (dynSettings.Controller.DynamoViewModel.DynamicRunEnabled)
            {
                //DynamoLogger.Instance.Log("Running Dynamically");
                if (!controller.Running)
                {
                    //DynamoLogger.Instance.Log("Nothing currently running, now running.");
                    controller.RunExpression(false);
                }
                else
                {
                    //DynamoLogger.Instance.Log("Run in progress, cancelling then running.");
                    controller.QueueRun();
                }
            }
        }

        public override void OnDisplayed()
        {
            //DynamoView bench = dynSettings.Bench; // ewwwy
        }
    }
}
