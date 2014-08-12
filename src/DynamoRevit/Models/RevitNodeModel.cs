using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Models;

namespace Dynamo.Applications.Models
{
    public abstract class RevitNodeModel : NodeModel
    {
        protected RevitNodeModel(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            this.RevitDynamoModel = Workspace.DynamoModel as RevitDynamoModel;
        }

        protected RevitDynamoModel RevitDynamoModel { get; private set; }

    }
}
