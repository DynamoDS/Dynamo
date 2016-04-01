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
    /// 
    /// </summary>
    public interface IDynamoModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        IEnumerable<WorkspaceModel> Workspaces { get; }

        /// <summary>
        /// 
        /// </summary>
        WorkspaceModel CurrentWorkspace { get; }

        /// <summary>
        /// 
        /// </summary>
        event Action<WorkspaceModel> WorkspaceCleared;

        /// <summary>
        /// 
        /// </summary>
        event DynamoModelHandler ShutdownStarted;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;

        /// <summary>
        /// 
        /// </summary>
        event Action<WorkspaceModel> WorkspaceAdded;

        /// <summary>
        /// 
        /// </summary>
        event Action<WorkspaceModel> WorkspaceRemoved;

        /// <summary>
        /// 
        /// </summary>
        event Action<XmlDocument> WorkspaceOpening;
    }
}
