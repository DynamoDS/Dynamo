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

        #region CORESEP : These are temporarily here

        public delegate void PointEventHandler(object sender, EventArgs e);

        public delegate void FunctionNamePromptRequestHandler(object sender, FunctionNamePromptEventArgs e);

        public class ZoomEventArgs : EventArgs
        {
            internal enum ZoomModes
            {
                ByPoint = 0x00000001,
                ByFactor = 0x00000002,
                ByFitView = 0x00000004
            }

            internal Point Point { get; set; }
            internal double Zoom { get; set; }
            internal ZoomModes Modes { get; private set; }

            internal Point Offset { get; set; }
            internal double FocusWidth { get; set; }
            internal double FocusHeight { get; set; }

            internal ZoomEventArgs(double zoom)
            {
                Zoom = zoom;
                this.Modes = ZoomModes.ByFactor;
            }

            internal ZoomEventArgs(Point point)
            {
                this.Point = point;
                this.Modes = ZoomModes.ByPoint;
            }

            internal ZoomEventArgs(double zoom, Point point)
            {
                this.Point = point;
                this.Zoom = zoom;
                this.Modes = ZoomModes.ByPoint | ZoomModes.ByFactor;
            }

            internal ZoomEventArgs(Point offset, double focusWidth, double focusHeight)
            {
                this.Offset = offset;
                this.FocusWidth = focusWidth;
                this.FocusHeight = focusHeight;
                this.Modes = ZoomModes.ByFitView;
            }

            internal ZoomEventArgs(Point offset, double focusWidth, double focusHeight, double zoom)
            {
                this.Offset = offset;
                this.FocusWidth = focusWidth;
                this.FocusHeight = focusHeight;
                this.Zoom = zoom;
                this.Modes = ZoomModes.ByFitView | ZoomModes.ByFactor;
            }

            internal bool hasPoint()
            {
                return this.Modes.HasFlag(ZoomModes.ByPoint);
            }

            internal bool hasZoom()
            {
                return this.Modes.HasFlag(ZoomModes.ByFactor);
            }
        }

        internal class TaskDialogEventArgs : EventArgs
        {
            List<Tuple<int, string, bool>> buttons = null;

            #region Public Operational Methods

            internal TaskDialogEventArgs(Uri imageUri, string dialogTitle,
                string summary, string description)
            {
                this.ImageUri = imageUri;
                this.DialogTitle = dialogTitle;
                this.Summary = summary;
                this.Description = description;
            }

            internal void AddLeftAlignedButton(int id, string content)
            {
                if (buttons == null)
                    buttons = new List<Tuple<int, string, bool>>();

                buttons.Add(new Tuple<int, string, bool>(id, content, true));
            }

            internal void AddRightAlignedButton(int id, string content)
            {
                if (buttons == null)
                    buttons = new List<Tuple<int, string, bool>>();

                buttons.Add(new Tuple<int, string, bool>(id, content, false));
            }

            #endregion

            #region Public Class Properties

            // Settable properties.
            internal int ClickedButtonId { get; set; }
            internal Exception Exception { get; set; }

            // Read-only properties.
            internal Uri ImageUri { get; private set; }
            internal string DialogTitle { get; private set; }
            internal string Summary { get; private set; }
            internal string Description { get; private set; }

            internal IEnumerable<Tuple<int, string, bool>> Buttons
            {
                get { return buttons; }
            }

            #endregion
        }

        public delegate void NodeEventHandler(object sender, EventArgs e);
        public delegate void ZoomEventHandler(object sender, EventArgs e);

        #endregion

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
        public virtual void OnCleanup(EventArgs e)
        {
            if (CleaningUp != null)
                CleaningUp(this, e);
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

        /// <summary>
        /// An event triggered when a single graph evaluation completes.
        /// </summary>
        public event EventHandler EvaluationCompleted;
        public virtual void OnEvaluationCompleted(object sender, EventArgs e)
        {
            if (EvaluationCompleted != null)
                EvaluationCompleted(sender, e);
        }

        #endregion
    }
}
