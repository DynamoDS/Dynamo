using System;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using ProtoCore.Lang;
using ProtoScript.Runners;

namespace Dynamo.Models
{
    public delegate void DynamoModelHandler(DynamoModel model);
    public delegate void NodeHandler(NodeModel node);
    public delegate void WorkspaceHandler(WorkspaceModel model);   
    public delegate void ActionHandler(Action action);
    public delegate void NodeEventHandler(object sender, EventArgs e);

    internal delegate void SettingsMigrationHandler(SettingsMigrationEventArgs args);
}
