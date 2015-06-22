using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Dynamo.Annotations;
using Dynamo.Core;

using DynamoServices;

namespace Dynamo.Models
{
    partial class DynamoModel
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static event ActionHandler RequestDispatcherInvoke;
        public static void OnRequestDispatcherInvoke(Action action)
        {
            // if a dispatcher is attached, invoke it
            if (RequestDispatcherInvoke != null)
                RequestDispatcherInvoke(action);
            else
                // otherwise invoke the action
                action();
        }

        public static event ActionHandler RequestDispatcherBeginInvoke;
        public static void OnRequestDispatcherBeginInvoke(Action action)
        {
            // if a dispatcher is attached, invoke it
            if (RequestDispatcherBeginInvoke != null)
                RequestDispatcherBeginInvoke(action);
            else
                // otherwise invoke the action directly
                action();
        }

        public static event SettingsMigrationHandler RequestMigrationStatusDialog;
        internal static void OnRequestMigrationStatusDialog(SettingsMigrationEventArgs args)
        {
            if (RequestMigrationStatusDialog != null)
                RequestMigrationStatusDialog(args);
        }

        public event EventHandler RequestLayoutUpdate;
        public virtual void OnRequestLayoutUpdate(object sender, EventArgs e)
        {
            if (RequestLayoutUpdate != null)
                RequestLayoutUpdate(this, e);
        }

        public event Action WorkspaceClearing;
        public virtual void OnWorkspaceClearing()
        {
            if (WorkspaceClearing != null)
                WorkspaceClearing();
        }

        public event EventHandler WorkspaceCleared;
        public virtual void OnWorkspaceCleared(object sender, EventArgs e)
        {
            if (WorkspaceCleared != null)
                WorkspaceCleared(this, e);
        }

        public event Action<WorkspaceModel> WorkspaceAdded;
        protected virtual void OnWorkspaceAdded(WorkspaceModel obj)
        {
            var handler = WorkspaceAdded;
            if (handler != null) handler(obj);

            WorkspaceEvents.OnWorkspaceAdded(obj.Guid, obj.Name);
        }

        public event Action<WorkspaceModel> WorkspaceRemoved;
        protected virtual void OnWorkspaceRemoved(WorkspaceModel obj)
        {
            var handler = WorkspaceRemoved;
            if (handler != null) handler(obj);

            WorkspaceEvents.OnWorkspaceRemoved(obj.Guid, obj.Name);
        }

        public event Action DeletionStarted;
        public virtual void OnDeletionStarted()
        {
            if (DeletionStarted != null)
                DeletionStarted();
        }

        public event EventHandler DeletionComplete;
        public virtual void OnDeletionComplete(object sender, EventArgs e)
        {
            if (DeletionComplete != null)
                DeletionComplete(this, e);
        }

        /// <summary>
        /// An event triggered when the workspace is being cleaned.
        /// </summary>
        public event Action CleaningUp;
        public virtual void OnCleanup()
        {
            if (CleaningUp != null)
                CleaningUp();
        }

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
        public virtual void OnRequestSelect(object sender, ModelEventArgs e)
        {
            if (RequestNodeSelect != null)
                RequestNodeSelect(sender, e);
        }

        public delegate void RunCompletedHandler(object sender, bool success);
        public event RunCompletedHandler RunCompleted;
        public virtual void OnRunCompleted(object sender, bool success)
        {
            if (RunCompleted != null)
                RunCompleted(sender, success);
        }

        // TODO(Ben): Obsolete CrashPrompt and make use of GenericTaskDialog.
        public delegate void CrashPromptHandler(object sender, CrashPromptArgs e);
        public event CrashPromptHandler RequestsCrashPrompt;
        public void OnRequestsCrashPrompt(object sender, CrashPromptArgs args)
        {
            if (RequestsCrashPrompt != null)
                RequestsCrashPrompt(this, args);
        }

        internal delegate void TaskDialogHandler(object sender, TaskDialogEventArgs e);
        internal event TaskDialogHandler RequestTaskDialog;
        internal void OnRequestTaskDialog(object sender, TaskDialogEventArgs args)
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
        public virtual void OnRefreshCompleted(object sender, EventArgs e)
        {
            var homeWorkspaceModel = sender as HomeWorkspaceModel;
            if (RefreshCompleted != null && homeWorkspaceModel != null)
                RefreshCompleted(homeWorkspaceModel);
        }

        #endregion
    }

}
