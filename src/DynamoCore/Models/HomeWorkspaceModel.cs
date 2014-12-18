using System.Collections.Generic;

namespace Dynamo.Models
{
    public class HomeWorkspaceModel : WorkspaceModel
    {
        internal bool IsEvaluationPending
        {
            get { return false; }
        }

        public HomeWorkspaceModel(DynamoModel dynamoModel)
            : this(dynamoModel, new List<NodeModel>(), new List<ConnectorModel>(), 0, 0){}

        public HomeWorkspaceModel(DynamoModel dynamoModel, IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
            : base(dynamoModel, "Home", e, c, x, y){}

        public override void Modified()
        {
            base.Modified();

            // When Dynamo is shut down, the workspace is cleared, which results
            // in Modified() being called. But, we don't want to run when we are
            // shutting down so we check that shutdown has not been requested.
            if (DynamoModel.DynamicRunEnabled && !DynamoModel.ShutdownRequested)
            {
                //Action run = () => DynamoModel.RunExpression();
                //Dispatcher.CurrentDispatcher.Invoke(run);
                DynamoModel.RunExpression();
            }
        }
    }
}
