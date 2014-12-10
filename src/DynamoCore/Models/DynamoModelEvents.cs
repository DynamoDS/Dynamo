using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Dynamo.Core;

namespace Dynamo.Models
{
    partial class DynamoModel
    {
        #region events

        public event ActionHandler RequestDispatcherInvoke;
        public virtual void OnRequestDispatcherInvoke(Action action)
        {
            // if a dispatcher is attached, invoke it
            if (RequestDispatcherInvoke != null)
            {
                RequestDispatcherInvoke(action);
            }
            else
            {
                // otherwise invoke the action
                action();
            }
        }

        public event ActionHandler RequestDispatcherBeginInvoke;
        public virtual void OnRequestDispatcherBeginInvoke(Action action)
        {
            // if a dispatcher is attached, invoke it
            if (RequestDispatcherBeginInvoke != null)
            {
                RequestDispatcherBeginInvoke(action);
            }
            else
            {
                // otherwise invoke the action directly
                action();
            }  
        }

        public event EventHandler RequestLayoutUpdate;
        public virtual void OnRequestLayoutUpdate(object sender, EventArgs e)
        {
            if (RequestLayoutUpdate != null)
                RequestLayoutUpdate(this, e);
        }

        public event EventHandler WorkspaceClearing;
        public virtual void OnWorkspaceClearing(object sender, EventArgs e)
        {
            if (WorkspaceClearing != null)
            {
                WorkspaceClearing(this, e);
            }
        }

        public event WorkspaceHandler CurrentWorkspaceChanged;
        public virtual void OnCurrentWorkspaceChanged(WorkspaceModel workspace)
        {
            if (CurrentWorkspaceChanged != null)
            {
                CurrentWorkspaceChanged(workspace);
            }
        }

        public event EventHandler WorkspaceCleared;
        public virtual void OnWorkspaceCleared(object sender, EventArgs e)
        {
            if (WorkspaceCleared != null)
            {
                WorkspaceCleared(this, e);
            }
        }

        public event EventHandler DeletionStarted;
        public virtual void OnDeletionStarted(object sender, EventArgs e)
        {
            if (DeletionStarted != null)
            {
                DeletionStarted(this, e);
            }
        }

        public event EventHandler DeletionComplete;
        public virtual void OnDeletionComplete(object sender, EventArgs e)
        {
            if (DeletionComplete != null)
            {
                DeletionComplete(this, e);
            }
        }

        /// <summary>
        /// An event triggered when the workspace is being cleaned.
        /// </summary>
        public event CleanupHandler CleaningUp;
        public virtual void OnCleanup()
        {
            if (CleaningUp != null)
                CleaningUp(this);
        }

        /// <summary>
        /// Event triggered when a node is added to a workspace
        /// </summary>
        public event NodeHandler NodeAdded;
        internal void OnNodeAdded(NodeModel node)
        {
            AddNodeToMap(node);

            if (NodeAdded != null && node != null)
            {
                NodeAdded(node);
            }
        }

        public event NodeHandler RequestCancelActiveStateForNode;
        private void OnRequestCancelActiveStateForNode(NodeModel node)
        {
            if (RequestCancelActiveStateForNode != null)
            {
                RequestCancelActiveStateForNode(node);
            }
        }

        /// <summary>
        /// Event triggered when a node is deleted
        /// </summary>
        public event NodeHandler NodeDeleted;
        internal void OnNodeDeleted(NodeModel node)
        {
            RemoveNodeFromMap(node);

            this.OnRequestCancelActiveStateForNode(node);

            if (NodeDeleted != null)
                NodeDeleted(node);
        }

        /// <summary>
        /// Event triggered when a connector is added.
        /// </summary>
        public event ConnectorHandler ConnectorAdded;
        internal void OnConnectorAdded(ConnectorModel connector)
        {
            if (ConnectorAdded != null)
            {
                ConnectorAdded(connector);
            }
        }

        /// <summary>
        /// Event triggered when a connector is deleted.
        /// </summary>
        public event ConnectorHandler ConnectorDeleted;
        internal void OnConnectorDeleted(ConnectorModel connector)
        {
            if (ConnectorDeleted != null)
            {
                ConnectorDeleted(connector);
            }
        }

        /// <summary>
        /// Event called when a workspace is hidden
        /// </summary>
        public event WorkspaceHandler WorkspaceHidden;
        private void OnWorkspaceHidden(WorkspaceModel workspace)
        {
            if (WorkspaceHidden != null)
            {
                WorkspaceHidden(workspace);
            }
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
            if (e.EvaluationTookPlace)
            {
                // When evaluation is completed, we mark all
                // nodes as ForceReexecuteOfNode = false to prevent
                // cyclical graph updates. It is therefore the responsibility 
                // of the node implementor to mark this flag = true, if they
                // want to require update.
                foreach (var n in HomeSpace.Nodes)
                {
                    n.ForceReExecuteOfNode = false;
                }
            }

            if (EvaluationCompleted != null)
                EvaluationCompleted(sender, e);
        }

        #endregion
    }

}
