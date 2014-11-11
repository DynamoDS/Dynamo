using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Dynamo.Models
{
    public class HomeWorkspaceModel : WorkspaceModel
    {
        private readonly DispatcherTimer runExpressionTimer;

        internal bool IsEvaluationPending
        {
            get
            {
                if (runExpressionTimer == null)
                    return false;

                return runExpressionTimer.IsEnabled;
            }
        }

        public HomeWorkspaceModel(DynamoModel dynamoModel)
            : this(dynamoModel, new List<NodeModel>(), new List<ConnectorModel>(), 0, 0)
        {
        }

        public HomeWorkspaceModel(DynamoModel dynamoModel, IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
            : base(dynamoModel, "Home", e, c, x, y)
        {
            runExpressionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            runExpressionTimer.Tick += OnRunExpression;
        }

        private void OnRunExpression(object sender, EventArgs e)
        {
// ReSharper disable once PossibleNullReferenceException
            (sender as DispatcherTimer).Stop();

            DynamoModel.RunExpression();
        }

        public override void Modified()
        {
            base.Modified();

            // When Dynamo is shut down, the workspace is cleared, which results
            // in Modified() being called. But, we don't want to run when we are
            // shutting down so we check that shutdown has not been requested.
            if (DynamoModel.DynamicRunEnabled && !DynamoModel.ShutdownRequested)
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
                runExpressionTimer.Stop();
                runExpressionTimer.Start(); // reset timer
            }
        }

        protected override void ResetWorkspaceCore()
        {
            runExpressionTimer.Stop();
            base.ResetWorkspaceCore();
        }
    }
}
