using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;

namespace Dynamo.Interfaces
{
    // TODO:http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-8116

    /// <summary>
    /// Interface contains definitions for core model of Dynamo.
    /// </summary>
    public interface IDynamoModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents the Workspaces in Dynamo.
        /// </summary>
        IEnumerable<WorkspaceModel> Workspaces { get; }

        /// <summary>
        /// Represents the current workspace.
        /// </summary>
        WorkspaceModel CurrentWorkspace { get; }

        /// <summary>
        /// This event is fired when workspace is cleared.
        /// </summary>
        event Action<WorkspaceModel> WorkspaceCleared;

        /// <summary>
        /// This event is fired when Dynamo is shutting down.
        /// </summary>
        event DynamoModelHandler ShutdownStarted;

        /// <summary>
        /// This event is fired when graph evaluation is completed.
        /// </summary>
        event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;

        /// <summary>
        /// This event is fired when a new workspace is added.
        /// </summary>
        event Action<WorkspaceModel> WorkspaceAdded;

        /// <summary>
        /// This event is fired when a workspace is removed.
        /// </summary>
        event Action<WorkspaceModel> WorkspaceRemoved;

        /// <summary>
        /// This event is fired when a workspace is opened.
        /// </summary>
        event Action<XmlDocument> WorkspaceOpening;
    }
}
