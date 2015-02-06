using System;
using ProtoScript.Runners;

namespace Dynamo.Models
{
    public delegate void CleanupHandler(DynamoModel dynamoModel);
    public delegate void DynamoModelHandler(DynamoModel model);
    public delegate void NodeHandler(NodeModel node);
    public delegate void ConnectorHandler(ConnectorModel connector);
    public delegate void WorkspaceHandler(WorkspaceModel model);   
    public delegate void ActionHandler(Action action);
    public delegate void NodeEventHandler(object sender, EventArgs e);
}
