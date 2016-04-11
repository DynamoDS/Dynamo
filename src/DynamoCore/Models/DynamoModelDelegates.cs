using System;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using ProtoCore.Lang;
using ProtoScript.Runners;

namespace Dynamo.Models
{
    /// <summary>
    /// This delegate is used to manage Dynamo's shutting down.
    /// </summary>
    public delegate void DynamoModelHandler(DynamoModel model);
    public delegate void NodeHandler(NodeModel node);
    public delegate void WorkspaceHandler(WorkspaceModel model);
    /// <summary>
    /// This delegate is used to manage Dynamo's request dispatcher invoke.
    /// </summary>
    public delegate void ActionHandler(Action action);

    /// <summary>
    /// This delegate is used in workspace events.
    /// </summary>
    /// <param name="sender">Object sender</param>
    /// <param name="e">EventArgs</param>
    public delegate void NodeEventHandler(object sender, EventArgs e);

    internal delegate void SettingsMigrationHandler(SettingsMigrationEventArgs args);
}
