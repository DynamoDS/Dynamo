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

            if (this.DynamoModel.DynamicRunEnabled)
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
    }
}
