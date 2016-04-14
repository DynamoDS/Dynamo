using System;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using ProtoCore.Lang;
using ProtoScript.Runners;

namespace Dynamo.Models
{
    public delegate void DynamoModelHandler(DynamoModel model);

    /// <summary>
    /// Delegate used in events, when it's requered to send node.
    /// </summary>
    /// <param name="node">Node model</param>
    public delegate void NodeHandler(NodeModel node);
    public delegate void WorkspaceHandler(WorkspaceModel model);   
    public delegate void ActionHandler(Action action);

    /// <summary>
    /// This delegate is used in workspace events.
    /// </summary>
    /// <param name="sender">Workspace</param>
    /// <param name="e"><see cref="EventArgs"/></param>
    public delegate void NodeEventHandler(object sender, EventArgs e);

    internal delegate void SettingsMigrationHandler(SettingsMigrationEventArgs args);
}
