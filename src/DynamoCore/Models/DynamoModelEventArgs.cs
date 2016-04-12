using System;
using System.Collections.Generic;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Models
{
    internal class ZoomEventArgs : EventArgs
    {
        internal enum ZoomModes
        {
            ByPoint = 0x00000001,
            ByFactor = 0x00000002,
            ByFitView = 0x00000004
        }

        internal Point2D Point { get; set; }
        internal double Zoom { get; set; }
        internal ZoomModes Modes { get; private set; }

        internal Point2D Offset { get; set; }
        internal double FocusWidth { get; set; }
        internal double FocusHeight { get; set; }

        internal ZoomEventArgs(double zoom)
        {
            Zoom = zoom;
            this.Modes = ZoomModes.ByFactor;
        }

        internal ZoomEventArgs(Point2D point)
        {
            this.Point = point;
            this.Modes = ZoomModes.ByPoint;
        }

        internal ZoomEventArgs(double zoom, Point2D point)
        {
            this.Point = point;
            this.Zoom = zoom;
            this.Modes = ZoomModes.ByPoint | ZoomModes.ByFactor;
        }

        internal ZoomEventArgs(Point2D offset, double focusWidth, double focusHeight)
        {
            this.Offset = offset;
            this.FocusWidth = focusWidth;
            this.FocusHeight = focusHeight;
            this.Modes = ZoomModes.ByFitView;
        }

        internal ZoomEventArgs(Point2D offset, double focusWidth, double focusHeight, double zoom)
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

    /// <summary>
    /// This class represents the arguments when a graph evaluation is completed.
    /// </summary>
    public class EvaluationCompletedEventArgs : EventArgs
    {
        private readonly IOption<Exception> error;

        /// <summary>
        /// Returns true if there was any evaluation.
        /// </summary>
        public bool EvaluationTookPlace { get; private set; }

        /// <summary>
        /// Returns true if Evaluation is succeeded.
        /// </summary>
        public bool EvaluationSucceeded
        {
            get { return !error.HasValue(); }
        }

        /// <summary>
        /// Exception thrown during graph evaluation.
        /// </summary>
        public Exception Error
        {
            get
            {
                return error.Match(
                    msg => msg,
                    () =>
                    {
                        throw new InvalidOperationException(
                            "Evaluation success, no error message recorded.");
                    });
            }
        }

        /// <summary>
        /// Creates EvaluationCompletedEventArgs
        /// </summary>
        /// <param name="evaluationTookPlace">Bool value indicates if evaluation took place</param>
        /// <param name="errorMsg">Exception thrown during evaluation</param>
        public EvaluationCompletedEventArgs(bool evaluationTookPlace, Exception errorMsg = null)
        {
            EvaluationTookPlace = evaluationTookPlace;

            error = errorMsg != null ? Option.Some(errorMsg) : Option.None<Exception>();
        }
    }

    internal class DynamoModelUpdateArgs : EventArgs
    {
        public object Item { get; set; }

        public DynamoModelUpdateArgs(object item)
        {
            Item = item;
        }
    }

    internal class FunctionNamePromptEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool CanEditName { get; set; }
        public bool Success { get; set; }

        public FunctionNamePromptEventArgs()
        {
            Name = "";
            Category = "";
            Description = "";
            CanEditName = true;
        }
    }

    internal class PresetsNamePromptEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Success { get; set; }

        public PresetsNamePromptEventArgs()
        {
            Name = System.DateTime.Now.ToString();
            Description = "";
        }
    }

    internal class ViewOperationEventArgs : EventArgs
    {
        public enum Operation
        {
            FitView, ZoomIn, ZoomOut
        }

        public ViewOperationEventArgs(Operation operation)
        {
            this.ViewOperation = operation;
        }

        public Operation ViewOperation { get; private set; }
    }

    internal class PointEventArgs : EventArgs
    {
        public Point2D Point { get; set; }

        public PointEventArgs(Point2D p)
        {
            Point = p;
        }
    }

    internal class WorkspaceEventArgs
    {
        public WorkspaceModel Workspace { get; set; }

        public WorkspaceEventArgs(WorkspaceModel workspace)
        {
            this.Workspace = workspace;
        }
    }

    internal class ModelEventArgs : EventArgs
    {
        public ModelBase Model { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public bool PositionSpecified { get; private set; }
        public bool TransformCoordinates { get; private set; }

        public ModelEventArgs(ModelBase model)
            : this(model, false)
        {
        }

        public ModelEventArgs(ModelBase model, bool transformCoordinates)
        {
            Model = model;
            PositionSpecified = false;
            TransformCoordinates = transformCoordinates;
        }

        public ModelEventArgs(ModelBase model, double x, double y, bool transformCoordinates)
        {
            Model = model;
            X = x;
            Y = y;
            PositionSpecified = true;
            TransformCoordinates = transformCoordinates;
        }
    }

    internal class DeltaComputeStateEventArgs : EventArgs
    {
        public List<Guid> NodeGuidList;
        public bool GraphExecuted;
      
        public DeltaComputeStateEventArgs(List<Guid> nodeGuidList, bool graphExecuted)
        {
            this.NodeGuidList = nodeGuidList;
            this.GraphExecuted = graphExecuted;
        }
    }

    internal class SettingsMigrationEventArgs : EventArgs
    {
        public enum EventStatusType
        { 
            Invalid = 0,
            Begin,
            End 
        }

        public EventStatusType EventStatus;

        public SettingsMigrationEventArgs(EventStatusType eventStatus)
        {
            EventStatus = eventStatus;
        }

    }

}
