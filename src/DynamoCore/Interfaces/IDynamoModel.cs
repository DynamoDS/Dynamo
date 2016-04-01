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
        /// Opened workspaces in Dynamo.
        /// </summary>
        IEnumerable<WorkspaceModel> Workspaces { get; }

        /// <summary>
        /// Workspace, that is currently opened.
        /// </summary>
        WorkspaceModel CurrentWorkspace { get; }

        /// <summary>
        /// Event, which is fired when workspace is cleared.
        /// </summary>
        event Action<WorkspaceModel> WorkspaceCleared;

        /// <summary>
        /// Event, which is fired when Dynamo starting shuting down.
        /// </summary>
        event DynamoModelHandler ShutdownStarted;

        /// <summary>
        /// Event, which is fired when graph evaluation is comleted.
        /// </summary>
        event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;

        /// <summary>
        /// Event, which is fired when new workspace is added.
        /// </summary>
        event Action<WorkspaceModel> WorkspaceAdded;

        /// <summary>
        /// Event, which is fired when workspace is removed.
        /// </summary>
        event Action<WorkspaceModel> WorkspaceRemoved;

        /// <summary>
        /// Event that is fired during the opening of the workspace.
        /// 
        /// Use the XmlDocument object provided to conduct additional
        /// workspace opening operations.
        /// </summary>
        event Action<XmlDocument> WorkspaceOpening;
    }
}
