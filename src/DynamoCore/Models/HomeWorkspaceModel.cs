using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    public class HomeWorkspaceModel : WorkspaceModel
    {
        private DispatcherTimer runExpressionTimer;

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

        private void OnRunExpression(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            var controller = dynSettings.Controller;
            if (!controller.Running)
            {
                controller.RunExpression(false);
            }
        }

        public override void Modified()
        {
            base.Modified();

            DynamoLogger.Instance.Log("===============Modified==================");

            var controller = dynSettings.Controller;
            if (dynSettings.Controller.DynamoViewModel.DynamicRunEnabled)
            {
#if USE_DSENGINE
                if (null == runExpressionTimer)
                {
                    runExpressionTimer = new DispatcherTimer();
                    runExpressionTimer.Interval += new TimeSpan(0, 0, 0, 0, 100);
                    runExpressionTimer.Tick += new EventHandler(OnRunExpression);
                }

                runExpressionTimer.Stop();
                runExpressionTimer.Start(); // reset timer
#else
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
#endif
            }
        }

        public override void OnDisplayed()
        {
            //DynamoView bench = dynSettings.Bench; // ewwwy
        }
    }
}
