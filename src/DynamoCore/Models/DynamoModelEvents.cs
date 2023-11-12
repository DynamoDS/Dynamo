using System;
using System.ComponentModel;
using Dynamo.Annotations;
using Dynamo.Core;
using Dynamo.Events;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using System.Collections.Generic;
using Dynamo.Graph;
using Dynamo.Extensions;

namespace Dynamo.Models
{
    partial class DynamoModel
    {
        #region events

        /// <summary>
        /// Occurs when a property of DynamoModel is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when an action needs to be invoked on a Dispather
        /// </summary>
        public static event ActionHandler RequestDispatcherInvoke;

        /// <summary>
        /// Tries to invoke a given action on Dispather
        /// </summary>
        /// <param name="action">Action to invoke</param>
        public static void OnRequestDispatcherInvoke(Action action)
        {
            // if a dispatcher is attached, invoke it
            if (RequestDispatcherInvoke != null)
                RequestDispatcherInvoke(action);
            else
                // otherwise invoke the action
                action();
        }

        /// <summary>
        /// Occurs when an action needs to be invoked asynchronously on a Dispather
        /// </summary>
        public static event ActionHandler RequestDispatcherBeginInvoke;

        /// <summary>
        /// Tries to invoke a given action asynchronously on Dispather
        /// </summary>
        /// <param name="action">Action to invoke</param>
        public static void OnRequestDispatcherBeginInvoke(Action action)
        {
            // if a dispatcher is attached, invoke it
            if (RequestDispatcherBeginInvoke != null)
                RequestDispatcherBeginInvoke(action);
            else
                // otherwise invoke the action directly
                action();
        }

        /// <summary>
        /// Event to throw for Splash Screen to update Dynamo launching tasks
        /// </summary>
        internal static event SplashScreenLoadingHandler RequestUpdateLoadBarStatus;
        internal static void OnRequestUpdateLoadBarStatus(SplashScreenLoadEventArgs args)
        {
            RequestUpdateLoadBarStatus?.Invoke(args);
        }

        /// <summary>
        /// Event to throw for Splash Screen to display the content in the proper language
        /// </summary>
        internal static event SplashScreenLanguageDetected LanguageDetected;
        internal static void OnDetectLanguage()
        {
            LanguageDetected?.Invoke();
        }

        /// <summary>
        /// Occurs when changes in data may affect UI and UI needs to be refreshed
        /// </summary>
        public event EventHandler RequestLayoutUpdate;

        /// <summary>
        /// Called when Requests to update UI is made.
        /// </summary>
        /// <param name="sender">Object which caused the event</param>
        /// <param name="e">The event data</param>
        public virtual void OnRequestLayoutUpdate(object sender, EventArgs e)
        {
            if (RequestLayoutUpdate != null)
                RequestLayoutUpdate(this, e);
        }

        /// <summary>
        /// Occurs before current workspace is cleared
        /// </summary>
        [Obsolete("Do not use! Use WorkspaceClearingStarted event instead")]
        public event Action WorkspaceClearing;

        /// <summary>
        /// Triggers WorkspaceClearing event
        /// </summary>

        [Obsolete("Do not use! Use OnWorkspaceClearingStarted virtual instead")]
        public virtual void OnWorkspaceClearing()
        {
            if (WorkspaceClearing != null)
                WorkspaceClearing();

            WorkspaceEvents.OnWorkspaceClearing();
        }

        /// <summary>
        /// Occurs before current workspace is cleared
        /// </summary>
        public event Action<WorkspaceModel> WorkspaceClearingStarted;

        /// <summary>
        /// Triggers WorkspaceClearing event
        /// </summary>
        /// <param name="workspace">Workspace about to be cleared</param>
        public virtual void OnWorkspaceClearingStarted(WorkspaceModel workspace)
        {
            WorkspaceClearingStarted?.Invoke(workspace);
            WorkspaceEvents.OnWorkspaceClearing();
        }

        /// <summary>
        /// Occurs after current workspace is cleared
        /// </summary>
        public event Action<WorkspaceModel> WorkspaceCleared;

        /// <summary>
        /// Triggers WorkspaceCleared event
        /// </summary>
        /// <param name="workspace">Cleared workspace</param>
        public virtual void OnWorkspaceCleared(WorkspaceModel workspace)
        {
            if (WorkspaceCleared != null)
                WorkspaceCleared(workspace);

            WorkspaceEvents.OnWorkspaceCleared();
        }

        /// <summary>
        /// Called when a workspace is added.
        /// </summary>
        public event Action<WorkspaceModel> WorkspaceAdded;
        protected virtual void OnWorkspaceAdded(WorkspaceModel obj)
        {
            var handler = WorkspaceAdded;
            if (handler != null) handler(obj);

            WorkspaceEvents.OnWorkspaceAdded(obj.Guid, obj.Name, obj.GetType());
        }

        /// <summary>
        /// Occurs before a workspace is removed
        /// </summary>
        public event Action<WorkspaceModel> WorkspaceRemoveStarted;
        protected virtual void OnWorkspaceRemoveStarted(WorkspaceModel obj)
        {
            var handler = WorkspaceRemoveStarted;
            if (handler != null) handler(obj);

            WorkspaceEvents.OnWorkspaceRemoveStarted(obj.Guid, obj.Name, obj.GetType());
        }

        /// <summary>
        /// Occurs after a workspace is removed
        /// </summary>
        public event Action<WorkspaceModel> WorkspaceRemoved;
        protected virtual void OnWorkspaceRemoved(WorkspaceModel obj)
        {
            var handler = WorkspaceRemoved;
            if (handler != null) handler(obj);

            WorkspaceEvents.OnWorkspaceRemoved(obj.Guid, obj.Name, obj.GetType());
        }

        /// <summary>
        /// Occurs before items of workspace are removed
        /// </summary>
        public event Action DeletionStarted;

        /// <summary>
        /// Called when Deletion started.
        /// </summary>
        public virtual void OnDeletionStarted(List<ModelBase> modelsToDelete, CancelEventArgs cancelEventArgs)
        {
            foreach (var model in modelsToDelete)
            {
                model.OnDeletionStarted(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return;
                }
            }

            if (DeletionStarted != null)
            {
                DeletionStarted();
            }
        }

        /// <summary>
        /// Occurs after items of workspace are removed
        /// </summary>
        public event EventHandler DeletionComplete;

        /// <summary>
        /// Triggers DeletionComplete event
        /// </summary>
        /// <param name="sender">The object which caused the event</param>
        /// <param name="e">The event data</param>
        public virtual void OnDeletionComplete(object sender, EventArgs e)
        {
            if (DeletionComplete != null)
                DeletionComplete(this, e);
        }

        /// <summary>
        /// An event triggered when the workspace is being cleaned.
        /// </summary>
        public event Action CleaningUp;

        /// <summary>
        /// Triggers CleaningUp event
        /// </summary>
        public virtual void OnCleanup()
        {
            if (CleaningUp != null)
                CleaningUp();
        }

        /// <summary>
        /// Called when current state of a node is canelled.
        /// </summary>
        public event NodeHandler RequestCancelActiveStateForNode;
        private void OnRequestCancelActiveStateForNode(NodeModel node)
        {
            if (RequestCancelActiveStateForNode != null)
                RequestCancelActiveStateForNode(node);
        }

        /// <summary>
        /// Event called when a workspace is hidden
        /// </summary>
        public event WorkspaceHandler WorkspaceHidden;
        private void OnWorkspaceHidden(WorkspaceModel workspace)
        {
            if (WorkspaceHidden != null)
                WorkspaceHidden(workspace);
        }

        // TODO: it is not used anywhere. is it needed?
        public event EventHandler RequestsRedraw;
        public virtual void OnRequestsRedraw(object sender, EventArgs e)
        {
            if (RequestsRedraw != null)
                RequestsRedraw(sender, e);
        }

        /// <summary>
        /// An event which requests that a node be selected
        /// </summary>
        public event NodeEventHandler RequestNodeSelect;
        internal virtual void OnRequestSelect(object sender, ModelEventArgs e)
        {
            if (RequestNodeSelect != null)
                RequestNodeSelect(sender, e);
        }

        /// <summary>
        /// Represents the method that will handle the <see cref="RunCompleted"/> event of the <see cref="DynamoModel"/> class.
        /// </summary>
        /// <param name="sender">The object which caused the event</param>
        /// <param name="success">Indicates if run completed successfully</param>
        public delegate void RunCompletedHandler(object sender, bool success);

        /// <summary>
        /// Occurs when running is completed
        /// </summary>
        public event RunCompletedHandler RunCompleted;

        /// <summary>
        /// Triggers RunCompleted event
        /// </summary>
        /// <param name="sender">The object which caused the event</param>
        /// <param name="success">Indicates if run completed successfully</param>
        public virtual void OnRunCompleted(object sender, bool success)
        {
            if (RunCompleted != null)
                RunCompleted(sender, success);
        }

        // TODO(Ben): Obsolete CrashPrompt and make use of GenericTaskDialog.
        public delegate void CrashPromptHandler(object sender, CrashPromptArgs e);
        public event CrashPromptHandler RequestsCrashPrompt;

        /// <summary>
        /// Shows the crash error reporting window.
        /// This method will always try to show the Autodesk CER UI first (if the CER tool is found on disk). 
        /// If the CER tool is not found, the Dynamo in-house crash prompt will be shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Can be called with CrashErrorReportArgs or CrashPromptArgs</param>
        [Obsolete("Will be removed in Dynamo3.0. Please use 'OnRequestsCrashPrompt(CrashErrorReportArgs args)' instead.")]
        public void OnRequestsCrashPrompt(object sender, CrashPromptArgs args)
        {
            if (RequestsCrashPrompt != null)
                RequestsCrashPrompt(this, args);
        }

        /// <summary>
        /// Shows the crash error reporting window.
        /// This method will always try to show the Autodesk CER UI first (if the CER tool is found on disk). 
        /// If the CER tool is not found, the Dynamo in-house crash prompt will be shown.
        /// </summary>
        /// <param name="args">CER options</param>
        public void OnRequestsCrashPrompt(CrashErrorReportArgs args)
        {
            if (RequestsCrashPrompt != null)
                RequestsCrashPrompt(this, args);
        }

        internal delegate void TaskDialogHandler(object sender, TaskDialogEventArgs e);
        internal event TaskDialogHandler RequestTaskDialog;
        internal virtual void OnRequestTaskDialog(object sender, TaskDialogEventArgs args)
        {
            if (RequestTaskDialog != null)
                RequestTaskDialog(sender, args);
        }

        internal delegate void VoidHandler();
        internal event VoidHandler RequestDownloadDynamo;
        internal void OnRequestDownloadDynamo()
        {
            if (RequestDownloadDynamo != null)
                RequestDownloadDynamo();
        }

        internal event VoidHandler RequestBugReport;
        internal void OnRequestBugReport()
        {
            if (RequestBugReport != null)
                RequestBugReport();
        }

        /// <summary>
        /// An event triggered when a single graph evaluation completes.
        /// </summary>
        public event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;

        /// <summary>
        /// Triggers EvaluationCompleted event
        /// </summary>
        /// <param name="sender">The object which caused the event</param>
        /// <param name="e">The event data</param>
        public virtual void OnEvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            if (!e.EvaluationSucceeded)
            {
                Action showFailureMessage = () => DisplayEngineFailureMessage(e.Error);
                OnRequestDispatcherBeginInvoke(showFailureMessage);
            }

            if (EvaluationCompleted != null)
                EvaluationCompleted(sender, e);
        }

        /// <summary>
        /// An event triggered when all tasks in scheduler are completed.
        /// </summary>
        public event Action<HomeWorkspaceModel> RefreshCompleted;

        /// <summary>
        /// Triggers RefreshCompleted event
        /// </summary>
        /// <param name="sender">The object which caused the event</param>
        /// <param name="e">The event data</param>
        public virtual void OnRefreshCompleted(object sender, EventArgs e)
        {
            var homeWorkspaceModel = sender as HomeWorkspaceModel;
            if (RefreshCompleted != null && homeWorkspaceModel != null)
                RefreshCompleted(homeWorkspaceModel);
        }

        /// <summary>
        /// This Event is raised after the compute section of the workspace is deserialized
        /// </summary>
        internal event Action ComputeModelDeserialized;

        /// <summary>
        /// Triggers ComputeModelSerialized event
        /// </summary>
        internal virtual void OnComputeModelDeserialized()
        {
            if (ComputeModelDeserialized != null)
                ComputeModelDeserialized();
        }

        internal delegate void FunctionNamePromptRequestHandler(object sender, FunctionNamePromptEventArgs e);
        internal event FunctionNamePromptRequestHandler RequestsFunctionNamePrompt;
        internal void OnRequestsFunctionNamePrompt(Object sender, FunctionNamePromptEventArgs e)
        {
            if (RequestsFunctionNamePrompt != null)
                RequestsFunctionNamePrompt(this, e);
        }

        internal event Action<PresetsNamePromptEventArgs> RequestPresetsNamePrompt;
        internal void OnRequestPresetNamePrompt(PresetsNamePromptEventArgs e)
        {
            if (RequestPresetsNamePrompt != null)
                RequestPresetsNamePrompt(e);
        }

        /// <summary>
        /// Occurs when a workspace is saved to a file.
        /// </summary>
        public event WorkspaceHandler WorkspaceSaved;
        internal void OnWorkspaceSaved(WorkspaceModel workspace)
        {
            if (WorkspaceSaved != null)
            {
                WorkspaceSaved(workspace);
            }
        }

        /// <summary>
        /// Occurs when a workspace is about to be saved to a file.
        /// </summary>
        public event WorkspaceSaveHandler WorkspaceSaving;
        internal void OnWorkspaceSaving(WorkspaceModel workspace, SaveContext saveContext)
        {
            if (WorkspaceSaving != null)
            {
                WorkspaceSaving(workspace, saveContext);
                if (workspace is HomeWorkspaceModel hws)
                    HandleStorageExtensionsOnWorkspaceSaving(hws, saveContext);
            }
        }

        /// <summary>
        /// Occurs when a workspace is scheduled to be saved to a backup file.
        /// </summary>
        public event Action<string, bool> RequestWorkspaceBackUpSave;
        internal void OnRequestWorkspaceBackUpSave(string path, bool isBackUp)
        {
            if (RequestWorkspaceBackUpSave != null)
            {
                RequestWorkspaceBackUpSave(path, isBackUp);
            }
        }

        /// <summary>
        /// Event that is fired during the opening of the workspace.
        ///
        /// Use the XmlDocument object provided to conduct additional
        /// workspace opening operations.
        /// </summary>
        public event Action<object> WorkspaceOpening;
        internal void OnWorkspaceOpening(object obj)
        {
            var handler = WorkspaceOpening;
            if (handler != null) handler(obj);
        }

        /// <summary>
        /// Occurs when a workspaces is opened
        /// </summary>
        public event WorkspaceHandler WorkspaceOpened;
        internal void OnWorkspaceOpened(WorkspaceModel workspace)
        {
            if (WorkspaceOpened != null)
            {
                WorkspaceOpened.Invoke(workspace);
                if (workspace is HomeWorkspaceModel hws)
                    HandleStorageExtensionsOnWorkspaceOpened(hws);
            }
        }

        /// <summary>
        /// This event is raised right before the shutdown of DynamoModel started.
        /// When this event is raised, the shutdown is guaranteed to take place
        /// (i.e. user has had a chance to save the work and decided to proceed
        /// with shutting down Dynamo). Handlers of this event can still safely
        /// access the DynamoModel, the WorkspaceModel (along with its contents),
        /// and the DynamoScheduler.
        /// </summary>
        public event DynamoModelHandler ShutdownStarted;

        private void OnShutdownStarted()
        {
            if (ShutdownStarted != null)
                ShutdownStarted(this);
        }

        /// <summary>
        /// This event is raised after DynamoModel has been shut down. At this
        /// point the DynamoModel is no longer valid and access to it should be
        /// avoided.
        /// </summary>
        public event DynamoModelHandler ShutdownCompleted;

        private void OnShutdownCompleted()
        {
            if (ShutdownCompleted != null)
                ShutdownCompleted(this);
        }

        /// <summary>
        /// This event is raised when Dynamo is ready for user interaction.
        /// </summary>
        public event Action<ReadyParams> DynamoReady;
        private bool dynamoReady;
        /// <summary>
        /// Event that is raised when Dynamo model requests a particular python engine
        /// to reset. String parameter is engine name.
        /// </summary>
        internal static event Action<string> RequestPythonReset;
        internal void OnRequestPythonReset(string pythonEngine)
        {
            //only reset if current workspace is a homeworkspace
            //can't guarantee which workspace to mark dirty otherwise
            if (CurrentWorkspace is HomeWorkspaceModel hmwsm)
            {
                RequestPythonReset?.Invoke(pythonEngine);
                ResetEngine(true);
            }

        }

        /// <summary>
        /// This event is used to raise a toast notification from the DynamoViewModel 
        /// </summary>
        internal event Action<string> RequestNotification;
        internal void OnRequestNotification(string notification)
        {
            if (RequestNotification != null)
            {
                RequestNotification(notification);
            }
        }

        #endregion
    }

}
