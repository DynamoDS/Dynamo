using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Dynamo.Models
{
    public class HomeWorkspaceModel : WorkspaceModel
    {
        private DispatcherTimer runExpressionTimer;

        public HomeWorkspaceModel(DynamoModel dynamoModel)
            : this(dynamoModel, new List<NodeModel>(), new List<ConnectorModel>(), 0, 0)
        {
        }

        public HomeWorkspaceModel(DynamoModel dynamoModel, IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
            : base(dynamoModel, "Home", e, c, x, y)
        {
        }

        private void OnRunExpression(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            this.DynamoModel.RunExpression();
        }

        public override void Modified()
        {
            base.Modified();

            // When Dynamo is shut down, the workspace is cleared, which results
            // in Modified() being called. But, we don't want to run when we are
            // shutting down so we check that shutdown has not been requested.
            if (this.DynamoModel.DynamicRunEnabled && !DynamoModel.ShutdownRequested)
            {

                // This dispatch timer is to avoid updating graph too frequently.
                // It happens when we are modifying a bunch of connections in 
                // a short time frame. E.g., when we delete some nodes with a 
                // bunch of connections, each deletion of connection will call 
                // RequestSync(). Or, when we are modifying the content in a code 
                // block. 
                // 
                // Each time when RequestSync() is called, runExpressionTimer will
                // be reset and until no RequestSync events flood in, the updating
                // of graph will get executed. 
                //
                // We use DispatcherTimer so that the update of graph happens on
                // the main UI thread.
                if (null == runExpressionTimer)
                {
                    runExpressionTimer = new DispatcherTimer();
                    runExpressionTimer.Interval += new TimeSpan(0, 0, 0, 0, 100);
                    runExpressionTimer.Tick += new EventHandler(OnRunExpression);
                }

                runExpressionTimer.Stop();
                runExpressionTimer.Start(); // reset timer

            }
        }

        protected override void ResetWorkspaceCore()
        {
            // It is possible for a timer to be started (due to the workspace 
            // being modified) immediately before the DynamoModel gets destroyed.
            // This is especially true for cases where multiple DynamoModel are
            // re-created in a single app domain (e.g. across unit test cases, 
            // or hosted scenario). Here OnRunExpression is unregistered from the
            // DispatcherTimer so that it will never be called anymore after the 
            // owning WorkspaceModel is destroyed.
            // 
            if (runExpressionTimer != null)
            {
                if (runExpressionTimer.IsEnabled)
                    runExpressionTimer.Stop();

                runExpressionTimer.Tick -= OnRunExpression;
                runExpressionTimer = null;
            }

            base.ResetWorkspaceCore();
        }
    }
}
