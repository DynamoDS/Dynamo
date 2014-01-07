using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using Dynamo.Core;

namespace Dynamo.ViewModels
{
    partial class WorkspaceViewModel
    {
        #region events
        public event EventHandler DragSelectionStarted;
        public event EventHandler DragSelectionEnded;
        #endregion

        #region State Machine Related Methods/Data Members

        private StateMachine stateMachine = null;
        private ConnectorViewModel activeConnector = null;
        private List<DraggedNode> draggedNodes = new List<DraggedNode>();

        internal StateMachine.State CurrentState
        {
            get { return stateMachine.CurrentState; }
        }

        internal ConnectorViewModel ActiveConnector
        {
            get { return activeConnector; }
        }

        internal bool HandleLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            return stateMachine.HandleLeftButtonDown(sender, e);
        }

        internal bool HandleMouseRelease(object sender, MouseButtonEventArgs e)
        {
            return stateMachine.HandleMouseRelease(sender, e);
        }

        internal bool HandleMouseMove(object sender, MouseEventArgs e)
        {
            return stateMachine.HandleMouseMove(sender, e);
        }

        internal bool HandleMouseMove(object sender, Point mouseCursor)
        {
            return stateMachine.HandleMouseMove(sender, mouseCursor);
        }

        internal bool HandlePortClicked(PortViewModel portViewModel)
        {
            return stateMachine.HandlePortClicked(portViewModel);
        }

        internal void RequestTogglePanMode()
        {
            stateMachine.RequestTogglePanMode();
        }

        internal void CancelActiveState()
        {
            stateMachine.CancelActiveState();
        }

        internal void BeginDragSelection(Point mouseCursor)
        {
            // This represents the first mouse-move event after the mouse-down
            // event. Note that a mouse-down event can either be followed by a
            // mouse-move event or simply a mouse-up event. That means having 
            // a click does not imply there will be a drag operation. That is 
            // the reason the first mouse-move event is used to signal the 
            // actual drag operation (as oppose to just click-and-release).
            // Here each node in the selection is being recorded for undo right
            // before they get updated by the drag operation.
            // 
            RecordSelectionForUndo();
            foreach (ISelectable selectable in DynamoSelection.Instance.Selection)
            {
                ILocatable locatable = selectable as ILocatable;
                if (null != locatable)
                    draggedNodes.Add(new DraggedNode(locatable, mouseCursor));
            }

            if (draggedNodes.Count <= 0) // There is nothing to drag.
            {
                string message = "Shouldn't get here if nothing is dragged";
                throw new InvalidOperationException(message);
            }
        }

        internal void UpdateDraggedSelection(Point mouseCursor)
        {
            if (draggedNodes.Count <= 0)
            {
                throw new InvalidOperationException(
                    "UpdateDraggedSelection cannot be called now");
            }

            foreach (DraggedNode draggedNode in draggedNodes)
                draggedNode.Update(mouseCursor);
        }

        internal void EndDragSelection(Point mouseCursor)
        {
            UpdateDraggedSelection(mouseCursor); // Final position update.
            draggedNodes.Clear(); // We are no longer dragging anything.
        }

        internal void BeginConnection(Guid nodeId, int portIndex, PortType portType)
        {
            int index = portIndex;
            bool isInPort = portType == PortType.INPUT;

            NodeModel node = _model.GetModelInternal(nodeId) as NodeModel;
            PortModel portModel = isInPort ? node.InPorts[index] : node.OutPorts[index];

            // Test if port already has a connection, if so grab it and begin connecting 
            // to somewhere else (we don't allow the grabbing of the start connector).
            if (portModel.Connectors.Count > 0 && portModel.Connectors[0].Start != portModel)
            {
                // Define the new active connector
                var c = new ConnectorViewModel(portModel.Connectors[0].Start);
                this.SetActiveConnector(c);

                // Disconnect the connector model from its start and end ports
                // and remove it from the connectors collection. This will also
                // remove the view model.
                ConnectorModel connector = portModel.Connectors[0];
                if (_model.Connectors.Contains(connector))
                {
                    List<ModelBase> models = new List<ModelBase>();
                    models.Add(connector);
                    _model.RecordAndDeleteModels(models);
                    connector.NotifyConnectedPortsOfDeletion();
                }
            }
            else
            {
                try
                {
                    // Create a connector view model to begin drawing
                    var connector = new ConnectorViewModel(portModel);
                    this.SetActiveConnector(connector);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        internal void EndConnection(Guid nodeId, int portIndex, PortType portType)
        {
            int index = portIndex;
            bool isInPort = portType == PortType.INPUT;

            NodeModel node = _model.GetModelInternal(nodeId) as NodeModel;
            PortModel portModel = isInPort ? node.InPorts[index] : node.OutPorts[index];
            ConnectorModel connectorToRemove = null;

            // Remove connector if one already exists
            if (portModel.Connectors.Count > 0 && portModel.PortType == PortType.INPUT)
            {
                connectorToRemove = portModel.Connectors[0];
                _model.Connectors.Remove(connectorToRemove);
                portModel.Disconnect(connectorToRemove);
                var startPort = connectorToRemove.Start;
                startPort.Disconnect(connectorToRemove);
            }

            // Create the new connector model
            var start = this.activeConnector.ActiveStartPort;
            var end = portModel;

            // We could either connect from an input port to an output port, or 
            // another way around (in which case we swap first and second ports).
            PortModel firstPort = start, second = end;
            if (portModel.PortType != PortType.INPUT)
            {
                firstPort = end;
                second = start;
            }

            ConnectorModel newConnectorModel = ConnectorModel.Make(firstPort.Owner,
                second.Owner, firstPort.Index, second.Index, PortType.INPUT);

            if (newConnectorModel != null) // Add to the current workspace
                _model.Connectors.Add(newConnectorModel);

            // Record the creation of connector in the undo recorder.
            var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>();
            if (connectorToRemove != null)
                models.Add(connectorToRemove, UndoRedoRecorder.UserAction.Deletion);
            models.Add(newConnectorModel, UndoRedoRecorder.UserAction.Creation);
            _model.RecordModelsForUndo(models);
            this.SetActiveConnector(null);
        }

        internal bool CheckActiveConnectorCompatibility(PortViewModel portVM)
        {
            // Check if required ports exist
            if ( this.activeConnector == null || portVM == null )
                return false;

            PortModel srcPortM = this.activeConnector.ActiveStartPort;
            PortModel desPortM = portVM.PortModel;

            // No self connection
            // No start to start or end or end connection
            if (srcPortM.Owner != desPortM.Owner && srcPortM.PortType != desPortM.PortType)
            {
                // Change cursor to show compatible port connection
                CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcAdding);
                return true;
            }
            else
            {
                // Change cursor to show not compatible
                CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
                return false;
            }
        }

        internal void CancelConnection()
        {
            this.SetActiveConnector(null);
        }

        internal void UpdateActiveConnector(Point mouseCursor)
        {
            if (null != this.activeConnector)
                this.activeConnector.Redraw(mouseCursor);

        }

        private void SetActiveConnector(ConnectorViewModel connector)
        {
            if (null != connector)
            {
                System.Diagnostics.Debug.Assert(null == activeConnector);
                this.WorkspaceElements.Add(connector);
                this.activeConnector = connector;
            }
            else
            {
                System.Diagnostics.Debug.Assert(null != activeConnector);
                this.WorkspaceElements.Remove(activeConnector);
                this.activeConnector = null;
            }

            this.RaisePropertyChanged("ActiveConnector");
        }

        private void RecordSelectionForUndo()
        {
            // This is where we attempt to store all the models in undo recorder 
            // before they are modified (i.e. being dragged around the canvas).
            // Note that we only do this once when the first mouse-move occurs 
            // after a mouse-down, because mouse-down can potentially be used 
            // just to select a node (as opposed to moving the selected nodes), in 
            // which case we don't want any of the nodes to be recorded for undo.
            // 
            List<ModelBase> models = DynamoSelection.Instance.Selection.
                Where((x) => (x is ModelBase)).Cast<ModelBase>().ToList<ModelBase>();

            this._model.RecordModelsForModification(models);
            DynamoController controller = Dynamo.Utilities.dynSettings.Controller;
            controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private void OnDragSelectionStarted(object sender, EventArgs e)
        {
            Debug.WriteLine("Drag started : Visualization paused.");
            if (DragSelectionStarted != null)
                DragSelectionStarted(sender, e);
        }

        private void OnDragSelectionEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("Drag ended : Visualization unpaused.");
            if (DragSelectionEnded != null)
                DragSelectionEnded(sender, e);
        }

        #endregion

        /// <summary>
        /// Each instance of this class represents a node that is being dragged.
        /// It keeps the offset of a node from the mouse cursor when a click 
        /// event occurs, and it updates the node position based on the internal
        /// offset values, and the updated mouse cursor position.
        /// </summary>
        public class DraggedNode
        {
            double deltaX = 0, deltaY = 0;
            ILocatable locatable = null;

            /// <summary>
            /// Construct a DraggedNode for a given ILocatable object.
            /// </summary>
            /// <param name="locatable">The ILocatable (usually a node) that is 
            /// associated with this DraggedNode object. During an update, the 
            /// position of ILocatable will be updated based on the specified 
            /// mouse position and the internal delta values.</param>
            /// <param name="mouseCursor">The mouse cursor at the point this 
            /// DraggedNode object is constructed. This is used to determine the 
            /// offset of the ILocatable from the mouse cursor.</param>
            /// <param name="region">The region within which the ILocatable can 
            /// be moved. However, the movement of ILocatable will be limited by 
            /// region and that it cannot be moved beyond the region.</param>
            /// 
            public DraggedNode(ILocatable locatable, Point mouseCursor)
            {
                this.locatable = locatable;
                deltaX = mouseCursor.X - locatable.X;
                deltaY = mouseCursor.Y - locatable.Y;
            }

            public void Update(Point mouseCursor)
            {
                // Make sure the nodes do not go beyond the region.
                double x = mouseCursor.X - deltaX;
                double y = mouseCursor.Y - deltaY;
                locatable.X = x;
                locatable.Y = y;
                locatable.ReportPosition();
            }
        }

        /// <summary>
        /// The StateMachine class manages states in the WorkspaceViewModel it 
        /// belongs. The class is made nested private class because there are 
        /// things that we don't expose beyond WorkspaceViewModel object, but 
        /// should still be readily accessible by the StateMachine class.
        /// </summary>
        internal class StateMachine
        {
            #region Private Class Data Members

            /// <summary>
            /// PanMode: Left mouse button will be use for panning instead
            ///     - Mouse cursor changed, disable all node interaction
            /// </summary>

            internal enum State
            {
                None,
                WindowSelection,
                DragSetup,
                NodeReposition,
                Connection,
                PanMode
            }

            private bool ignoreMouseClick = false;
            private State currentState = State.None;
            internal State CurrentState
            {
                get { return this.currentState; }
            }

            private Point mouseDownPos = new Point();
            private WorkspaceViewModel owningWorkspace = null;

            #endregion

            #region Public Class Operational Methods

            internal StateMachine(WorkspaceViewModel owningWorkspace)
            {
                this.owningWorkspace = owningWorkspace;
            }

            /// <summary>
            /// The owning WorkspaceView calls this to cancel the current state
            /// </summary>
            internal void CancelActiveState()
            {
                SetCurrentState(State.None);
            }

            /// <summary>
            /// The owning WorkspaceViewModel calls this method in an attempt to
            /// place the StateMachine into view panning mode. Note that as a 
            /// result of calling this method, the StateMachine may be kicked
            /// out of its existing state.
            /// </summary>
            internal void RequestTogglePanMode()
            {
                // In pan mode, left mouse click shall not be handled
                if (currentState == State.PanMode)
                    SetCurrentState(State.None);

                else // no matter in which state, goes to PanMode directly
                    SetCurrentState(State.PanMode);
            }

            private void SetCurrentState(State newState)
            {
                if (newState == this.currentState)
                    return; // No state changes

                // Exiting from current state
                if (State.None != this.currentState)
                {
                    switch (this.currentState)
                    {
                        case State.WindowSelection:
                            CancelWindowSelection();
                            break;
                        case State.Connection:
                            CancelConnection();
                            break;
                    }
                }

                // Entering into state
                switch (newState)
                {
                    case State.WindowSelection:
                        // Change cursor for window selection
                        owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(CursorSet.RectangularSelection);
                        owningWorkspace.IsCursorForced = true;
                        break;
                    case State.Connection:
                        // Change cursor for connection
                        owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcAdding);
                        owningWorkspace.IsCursorForced = true;
                        break;
                    case State.PanMode:
                        // change cursor for pan mode
                        owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(CursorSet.HandPan);
                        owningWorkspace.IsCursorForced = true;
                        break;
                    case State.None:
                        // Change cursor to follow default mouse set at the parent
                        owningWorkspace.CurrentCursor = null;
                        owningWorkspace.IsCursorForced = false;
                        break;
                }

                this.currentState = newState; // update state
            }

            #endregion

            #region User Input Event Handlers

            internal bool HandleLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (false != ignoreMouseClick)
                {
                    ignoreMouseClick = false;
                    return false;
                }

                bool eventHandled = false;
                if (this.currentState == State.Connection)
                {
                    // Clicking on the canvas while connecting simply cancels 
                    // the operation and drop the temporary connector.
                    SetCurrentState(State.None);

                    eventHandled = true; // Mouse event handled.
                }
                else if (this.currentState == State.None)
                {
                    // Record the mouse down position.
                    IInputElement element = sender as IInputElement;
                    mouseDownPos = e.GetPosition(element);

                    // We'll see if there is any node being clicked on. If so, 
                    // then the state machine should initiate a drag operation.
                    if (null != GetSelectableFromPoint(mouseDownPos))
                        InitiateDragSequence();
                    else
                        InitiateWindowSelectionSequence();

                    eventHandled = true; // Mouse event handled.
                }
                else if (this.currentState == State.PanMode)
                {
                    owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(CursorSet.HandPanActive);
                }

                dynSettings.ReturnFocusToSearch();

                return eventHandled;
            }

            #region Create CodeBlockNode
            private void CreateCodeBlockNode(Point cursor)
            {
                // create node
                var guid = Guid.NewGuid();
                dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                    new DynCmd.CreateNodeCommand(guid, "Code Block",
                        cursor.X, cursor.Y, false, true));

                // select node
                var placedNode = dynSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
                if (placedNode != null)
                {
                    DynamoSelection.Instance.ClearSelection();
                    DynamoSelection.Instance.Selection.Add(placedNode);
                }

                //correct node position
                if (placedNode != null)
                {
                    placedNode.X = (int)mouseDownPos.X - 92;
                    placedNode.Y = (int)mouseDownPos.Y - 31;
                }
            }
            #endregion

            internal bool HandleMouseRelease(object sender, MouseButtonEventArgs e)
            {
                if (e.ChangedButton != MouseButton.Left)
                    return false; // We only handle left mouse button for now.

                if (this.currentState == State.WindowSelection)
                {
                    SetCurrentState(State.None);
                    return true; // Mouse event handled.
                }
                else if (this.currentState == State.NodeReposition)
                {
                    Point mouseCursor = e.GetPosition(sender as IInputElement);
                    var operation = DynCmd.DragSelectionCommand.Operation.EndDrag;
                    var command = new DynCmd.DragSelectionCommand(mouseCursor, operation);
                    var dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                    dynamoViewModel.ExecuteCommand(command);

                    SetCurrentState(State.None); // Dragging operation ended.
                }
                else if (this.currentState == State.DragSetup)
                    SetCurrentState(State.None);
                else if (this.currentState == State.PanMode)
                {
                    // Change cursor back to Pan
                    owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(CursorSet.HandPan);
                }

                return false; // Mouse event not handled.
            }

            internal bool HandleMouseMove(object sender, Point mouseCursor)
            {
                if (this.currentState == State.Connection)
                {
                    // If we are currently connecting and there is an active 
                    // connector, redraw it to match the new mouse coordinates.

                    owningWorkspace.UpdateActiveConnector(mouseCursor);

                }
                else if (this.currentState == State.WindowSelection)
                {
                    // When the mouse is held down, reposition the drag selection box.
                    double x = Math.Min(mouseDownPos.X, mouseCursor.X);
                    double y = Math.Min(mouseDownPos.Y, mouseCursor.Y);
                    double width = Math.Abs(mouseDownPos.X - mouseCursor.X);
                    double height = Math.Abs(mouseCursor.Y - mouseDownPos.Y);

                    // We perform cross selection (i.e. select a node whenever 
                    // it touches the selection box as opposed to only select 
                    // it when it is entirely within the selection box) when 
                    // mouse moves in the opposite direction (i.e. the current 
                    // mouse position is smaller than the point mouse-down 
                    // happened).
                    // 
                    bool isCrossSelection = mouseCursor.X < mouseDownPos.X;

                    SelectionBoxUpdateArgs args = null;
                    args = new SelectionBoxUpdateArgs(x, y, width, height);
                    args.SetSelectionMode(isCrossSelection);
                    this.owningWorkspace.RequestSelectionBoxUpdate(this, args);

                    var rect = new Rect(x, y, width, height);

                    var command = new DynCmd.SelectInRegionCommand(rect, isCrossSelection);
                    DynamoViewModel dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                    dynamoViewModel.ExecuteCommand(command);

                }
                else if (this.currentState == State.DragSetup)
                {
                    // There are something in the selection, but none is ILocatable.
                    if (!DynamoSelection.Instance.Selection.Any((x) => (x is ILocatable)))
                    {
                        SetCurrentState(State.None);
                        return false;
                    }

                    // Record and begin the drag operation for selected nodes.
                    var operation = DynCmd.DragSelectionCommand.Operation.BeginDrag;
                    var command = new DynCmd.DragSelectionCommand(mouseCursor, operation);
                    DynamoViewModel dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                    dynamoViewModel.ExecuteCommand(command);

                    SetCurrentState(State.NodeReposition);
                    return true;
                }
                else if (this.currentState == State.NodeReposition)
                {
                    // Update the dragged nodes (note: this isn't recorded).
                    owningWorkspace.UpdateDraggedSelection(mouseCursor);
                }

                return false; // Mouse event not handled.
            }

            internal bool HandleMouseMove(object sender, MouseEventArgs e)
            {
                IInputElement element = sender as IInputElement;
                Point mouseCursor = e.GetPosition(element);
                return HandleMouseMove(sender, mouseCursor);
            }

            internal bool HandlePortClicked(PortViewModel portViewModel)
            {
                // We only entertain port clicking when the current state is idle, 
                // or when it is already having a connector being established.
                if (currentState != State.None && (currentState != State.Connection))
                    return false;

                PortModel portModel = portViewModel.PortModel;
                DynamoViewModel dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                WorkspaceViewModel workspaceViewModel = dynamoViewModel.CurrentSpaceViewModel;

                if (this.currentState != State.Connection) // Not in a connection attempt...
                {
                    PortType portType = PortType.INPUT;
                    Guid nodeId = portModel.Owner.GUID;
                    int portIndex = portModel.Owner.GetPortIndexAndType(portModel, out portType);

                    dynamoViewModel.ExecuteCommand(new DynCmd.MakeConnectionCommand(
                        nodeId, portIndex, portType, DynCmd.MakeConnectionCommand.Mode.Begin));

                    if (null != owningWorkspace.activeConnector)
                    {
                        this.currentState = State.Connection;
                        owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
                        owningWorkspace.IsCursorForced = false;
                    }
                }
                else  // Attempt to complete the connection
                {
                    // Check if connection is valid
                    if (owningWorkspace.CheckActiveConnectorCompatibility(portViewModel))
                    {
                        PortType portType = PortType.INPUT;
                        Guid nodeId = portModel.Owner.GUID;
                        int portIndex = portModel.Owner.GetPortIndexAndType(portModel, out portType);

                        dynamoViewModel.ExecuteCommand(new DynCmd.MakeConnectionCommand(
                            nodeId, portIndex, portType, DynCmd.MakeConnectionCommand.Mode.End));

                        owningWorkspace.CurrentCursor = null;
                        owningWorkspace.IsCursorForced = false;
                        this.currentState = State.None;
                    }
                }

                return true;
            }
            #endregion

            #region Cancel State Methods
            private void CancelConnection()
            {
                var command = new DynCmd.MakeConnectionCommand(Guid.Empty, -1,
                        PortType.INPUT, DynCmd.MakeConnectionCommand.Mode.Cancel);

                var dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                dynamoViewModel.ExecuteCommand(command);
            }

            private void CancelWindowSelection()
            {
                // visualization unpause
                owningWorkspace.OnDragSelectionEnded(this, EventArgs.Empty);

                SelectionBoxUpdateArgs args = null;
                args = new SelectionBoxUpdateArgs(Visibility.Collapsed);
                this.owningWorkspace.RequestSelectionBoxUpdate(this, args);
            }
            #endregion

            #region Private Class Helper Method

            private ISelectable GetSelectableFromPoint(Point point)
            {
                foreach (ISelectable selectable in DynamoSelection.Instance.Selection)
                {
                    var locatable = selectable as ILocatable;
                    if (locatable == null || (!locatable.Rect.Contains(point)))
                        continue;

                    return selectable;
                }

                return null;
            }

            private void InitiateDragSequence()
            {
                // The state machine must be in idle state.
                if (this.currentState != State.None)
                    throw new InvalidOperationException();

                SetCurrentState(State.DragSetup);
            }

            private void InitiateWindowSelectionSequence()
            {
                // The state machine must be in idle state.
                if (this.currentState != State.None)
                    throw new InvalidOperationException();

                // Clear existing selection set.
                var selectNothing = new DynCmd.SelectModelCommand(Guid.Empty, ModifierKeys.None);
                DynamoViewModel dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                dynamoViewModel.ExecuteCommand(selectNothing);

                // Update the selection box and make it visible 
                // but with an initial dimension of zero.
                SelectionBoxUpdateArgs args = null;
                args = new SelectionBoxUpdateArgs(mouseDownPos.X, mouseDownPos.Y, 0, 0);
                args.SetVisibility(Visibility.Visible);

                this.owningWorkspace.RequestSelectionBoxUpdate(this, args);

                SetCurrentState(State.WindowSelection);

                // visualization pause
                owningWorkspace.OnDragSelectionStarted(this, EventArgs.Empty);
            }
            #endregion
        }

    }
}
