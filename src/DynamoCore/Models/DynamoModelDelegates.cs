using System;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;

namespace Dynamo.Models
{
    /// <summary>
    /// This delegate is used to manage Dynamo's shutting down.
    /// </summary>
    public delegate void DynamoModelHandler(DynamoModel model);

    /// <summary>
    /// Delegate used in events, when it's required to send node.
    /// </summary>
    /// <param name="node">Node model</param>
    public delegate void NodeHandler(NodeModel node);

    /// <summary>
    /// Represents the method that will handle workspace related events.
    /// </summary>
    /// <param name="workspace">The <see cref="WorkspaceModel"/> which was saved.</param>
    public delegate void WorkspaceHandler(WorkspaceModel workspace);

    /// <summary>
    /// This delegate is used to manage Dynamo's request dispatcher invoke.
    /// </summary>
    public delegate void ActionHandler(Action action);

    /// <summary>
    /// This delegate is used in workspace events.
    /// </summary>
    /// <param name="sender">Workspace</param>
    /// <param name="e"><see cref="EventArgs"/></param>
    public delegate void NodeEventHandler(object sender, EventArgs e);

    internal delegate void SettingsMigrationHandler(SettingsMigrationEventArgs args);
}
