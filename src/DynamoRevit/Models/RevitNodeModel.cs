using Dynamo.Models;

namespace Dynamo.Applications.Models
{
    public abstract class RevitNodeModel : NodeModel
    {
        protected RevitNodeModel(WorkspaceModel workspaceModel) : base()
        {
            this.RevitDynamoModel = Workspace.DynamoModel as RevitDynamoModel;
        }

        protected RevitDynamoModel RevitDynamoModel { get; private set; }

    }
}
