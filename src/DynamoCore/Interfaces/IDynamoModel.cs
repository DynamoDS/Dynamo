using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Dynamo.Models;

namespace Dynamo.Interfaces
{
    public interface IDynamoModel : INotifyPropertyChanged
    {
        IEnumerable<WorkspaceModel> Workspaces { get; }
        WorkspaceModel CurrentWorkspace { get; }

        event Action<WorkspaceModel> WorkspaceCleared;
        event DynamoModelHandler ShutdownStarted;
        event Action CleaningUp;
        event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;
        event Action<WorkspaceModel> WorkspaceAdded;
        event Action<WorkspaceModel> WorkspaceRemoved;
        event Action<XmlDocument> WorkspaceOpening;
    }
}
