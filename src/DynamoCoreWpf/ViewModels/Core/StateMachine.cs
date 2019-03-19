using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Wpf.Utilities;
using Newtonsoft.Json;
using DynCmd = Dynamo.Models.DynamoModel;
using ModifierKeys = System.Windows.Input.ModifierKeys;
using Point = System.Windows.Point;

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
        private List<DraggedNode> draggedNodes = new List<DraggedNode>();

        // When a new connector is created or a single connector is selected,
        // activeConnectors has array size of 1.
        // In the case of shift + click to reconnect multiple connectors, 
        // all selected connectors are saved in activeConnectors.
        private ConnectorViewModel[] activeConnectors = null;
        private static bool multipleConnections = false;

        // firstStartPort records the first port selected when connections
        // are made using ctrl + click, so that the subsequent new
        // connections are all connected to firstStartPort.
        private PortModel firstStartPort;

        [JsonIgnore]
        // These properties need to be public for data-binding to work.
        public bool IsInIdleState { get { return stateMachine.IsInIdleState; } }

        [JsonIgnore]
        public bool IsSelecting { get { return stateMachine.IsSelecting; } }

        [JsonIgnore]
        public bool IsDragging { get { return stateMachine.IsDragging; } }

        [JsonIgnore]
        public bool IsConnecting { get { return stateMachine.IsConnecting; } }

        [JsonIgnore]
        public bool IsPanning { get { return stateMachine.IsPanning; } }

        [JsonIgnore]
        public bool IsOrbiting { get { return stateMachine.IsOrbiting; } }

        [JsonIgnore]
        internal ConnectorViewModel FirstActiveConnector
        {
            get {
                if (null != activeConnectors && activeConnectors.Count() > 0)
                {
                    return activeConnectors[0];
                }
                return null;
            }
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

        internal bool HandleMouseMove(object sender, System.Windows.Point mouseCursor)
        {
            return stateMachine.HandleMouseMove(sender, mouseCursor);
        }

        internal bool HandleFocusChanged(object sender, bool focused)
        {
            return stateMachine.HandleFocusChanged(sender, focused);
        }

        internal bool HandlePortClicked(PortViewModel portViewModel)
        {
            return stateMachine.HandlePortClicked(portViewModel);
        }

        internal void RequestTogglePanMode()
        {
            stateMachine.RequestTogglePanMode();
        }

        internal void RequestToggleOrbitMode()
        {
            stateMachine.RequestToggleOrbitMode();
        }

        internal void CancelActiveState()
        {
            stateMachine.CancelActiveState();
        }

        internal void BeginDragSelection(Point2D mouseCursor)
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

        internal void UpdateDraggedSelection(Point2D mouseCursor)
        {
            if (draggedNodes.Count <= 0)
            {
                throw new InvalidOperationException(
                    "UpdateDraggedSelection cannot be called now");
            }

            foreach (DraggedNode draggedNode in draggedNodes)
                draggedNode.Update(mouseCursor);
        }

        internal void EndDragSelection(Point2D mouseCursor)
        {
            UpdateDraggedSelection(mouseCursor); // Final position update.
            draggedNodes.Clear(); // We are no longer dragging anything.
        }

        internal void PasteSelection(Point2D targetPoint)
        {
            stateMachine.PasteSelection(targetPoint);
        }

        internal void BeginConnection(Guid nodeId, int portIndex, PortType portType)
        {
            bool isInPort = portType == PortType.Input;

            NodeModel node = Model.GetModelInternal(nodeId) as NodeModel;
            if (node == null)
                return;
            PortModel portModel = isInPort ? node.InPorts[portIndex] : node.OutPorts[portIndex];

            // Test if port already has a connection, if so grab it and begin connecting 
            // to somewhere else (we don't allow the grabbing of the start connector).
            if (portModel.Connectors.Count > 0 && portModel.Connectors[0].Start != portModel)
            {
                // Define the new active connector
                var c = new ConnectorViewModel[] { new ConnectorViewModel(this, portModel.Connectors[0].Start) };
                this.SetActiveConnectors(c);
                firstStartPort = portModel.Connectors[0].Start;
            }
            else
            {
                try
                {
                    // Create an array containing a connector view model to begin drawing
                    var connectors = new ConnectorViewModel[] { new ConnectorViewModel(this, portModel) };
                    this.SetActiveConnectors(connectors);
                    firstStartPort = isInPort ? null : node.OutPorts[portIndex];
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        internal void BeginShiftReconnections(Guid nodeId, int portIndex, PortType portType)
        {
            if (portType == PortType.Input) return;

            NodeModel node = Model.GetModelInternal(nodeId) as NodeModel;
            if (node == null) return;

            PortModel portModel = node.OutPorts[portIndex];
            if (portModel.Connectors.Count <= 0) return;

            // Try to obtain connectors for selected nodes
            var selected = portModel.Connectors.Where(x => x.End.Owner.IsSelected).Select(y => y.End);

            // If there are no selected nodes, obtain all the associated connectors
            if (selected.Count() <= 0)
            {
                selected = portModel.Connectors.Select(y => y.End);
            }

            var connectorsAr = new ConnectorViewModel[selected.Count()];
            for (int i = 0; i < selected.Count(); i++)
            {
                var c = new ConnectorViewModel(this, selected.ElementAt(i));
                connectorsAr[i] = c;
            }

            this.SetActiveConnectors(connectorsAr);
            return;
        }

        internal void EndConnection(Guid nodeId, int portIndex, PortType portType)
        {
            this.SetActiveConnectors(null);
        }

        internal void BeginCreateConnections(Guid nodeId, int portIndex, PortType portType)
        {
            // Only handle ctrl connections if selected port is an input port
            if (firstStartPort == null || portType == PortType.Output) return; 
            this.SetActiveConnectors(null); // End the current connection

            // Then, start a new connection
            NodeModel node = Model.GetModelInternal(nodeId) as NodeModel;
            if (node == null)
                return;
            try
            {
                // Create an array containing a connector view model to begin drawing
                var connectors = new ConnectorViewModel[] { new ConnectorViewModel(this, firstStartPort) };
                this.SetActiveConnectors(connectors);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);

            }
        }

        internal bool CheckActiveConnectorCompatibility(PortViewModel portVM,bool isSnapping = true)
        {
            // Check if required ports exist
            if (FirstActiveConnector == null || portVM == null)
                return false;
            //By default the ports will be in snapping mode. But if the connection is not completed,
            //then on mouse leave, the cursor should be pointed as arcselect instead of arcadd.             
            if (!isSnapping)
            {
                CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
                return false;
            }

            foreach (ConnectorViewModel activeConnector in activeConnectors)
            {
                var srcPortM = activeConnector.ActiveStartPort;
                var desPortM = portVM.PortModel;
                // No self connection
                // No start to start or end or end connection
                if (srcPortM.Owner == desPortM.Owner || srcPortM.PortType == desPortM.PortType)
                {
                    // Change cursor to show not compatible
                    CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
                    return false;
                }
            }
            // If all connections are compatible, change cursor to show compatible port connection
            CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcAdding);
            return true;
        }

        internal void CancelConnection()
        {
            multipleConnections = false;
            this.SetActiveConnectors(null);
            firstStartPort = null;
        }

        internal void UpdateActiveConnector(System.Windows.Point mouseCursor)
        {
            if (null != this.activeConnectors)
            {
                for (int i = 0; i < this.activeConnectors.Count(); i++)
                {
                    this.activeConnectors[i].Redraw(mouseCursor.AsDynamoType());
                }
            }
        }

        private void SetActiveConnectors (ConnectorViewModel[] connectors) // A replacement for SetActiveConnector(), for handling both single and multiple connectors
        {
            if (null != connectors && connectors.Count() > 0)
            {
                this.activeConnectors = new ConnectorViewModel[connectors.Count()];
                for (int i = 0; i < connectors.Count(); i++)
                {
                    this.WorkspaceElements.Add(connectors[i]);
                    this.activeConnectors[i] = connectors[i];
                }
            }
            else
            {
                if (null != activeConnectors)
                {
                    foreach (ConnectorViewModel a in activeConnectors)
                    {
                        this.WorkspaceElements.Remove(a);
                    }
                }
                this.activeConnectors = null;
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

            WorkspaceModel.RecordModelsForModification(models, Model.UndoRecorder);

            DynamoViewModel.RaiseCanExecuteUndoRedo();
        }

        private void OnDragSelectionStarted(object sender, EventArgs e)
        {
            //Debug.WriteLine("Drag started : Visualization paused.");
            if (DragSelectionStarted != null)
                DragSelectionStarted(sender, e);
        }

        private void OnDragSelectionEnded(object sender, EventArgs e)
        {
            //Debug.WriteLine("Drag ended : Visualization unpaused.");
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
            public DraggedNode(ILocatable locatable, Point2D mouseCursor)
            {
                this.locatable = locatable;
                deltaX = mouseCursor.X - locatable.X;
                deltaY = mouseCursor.Y - locatable.Y;
            }

            public void Update(Point2D mouseCursor)
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

            private enum State
            {
                None,
                WindowSelection,
                DragSetup,
                NodeReposition,
                Connection,
                PanMode,
                OrbitMode
            }

            private bool ignoreMouseClick = false;
            private State currentState = State.None;
            private Point mouseDownPos = new Point();
            private WorkspaceViewModel owningWorkspace = null;

            #endregion

            #region Public Class Properties

            internal bool IsInIdleState
            {
                get { return this.currentState == State.None; }
            }

            internal bool IsSelecting
            {
                get { return this.currentState == State.WindowSelection; }
            }

            internal bool IsDragging
            {
                get
                {
                    return this.currentState == State.DragSetup ||
                        this.currentState == State.NodeReposition;
                }
            }

            internal bool IsConnecting
            {
                get { return this.currentState == State.Connection; }
            }

            internal bool IsPanning
            {
                get { return this.currentState == State.PanMode; }
            }

            internal bool IsOrbiting
            {
                get { return this.currentState == State.OrbitMode; }
            }

            private State CurrentState
            {
                get { return this.currentState; }
            }

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

            /// <summary>
            /// The owning WorkspaceViewModel calls this method in an attempt to
            /// place the StateMachine into view orbiting mode. Note that as a 
            /// result of calling this method, the StateMachine may be kicked
            /// out of its existing state.
            /// </summary>
            internal void RequestToggleOrbitMode()
            {
                if (currentState == State.OrbitMode)
                    SetCurrentState(State.None);
                else
                    SetCurrentState(State.OrbitMode);
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

                // Entering into a new state
                CursorSet cursorToUse = CursorSet.Pointer;

                switch (newState)
                {
                    case State.WindowSelection:
                        cursorToUse = CursorSet.RectangularSelection;
                        owningWorkspace.IsCursorForced = true;
                        break;

                    case State.Connection:
                        cursorToUse = CursorSet.ArcAdding;
                        owningWorkspace.IsCursorForced = true;
                        break;

                    case State.PanMode:
                        cursorToUse = CursorSet.HandPan;
                        owningWorkspace.IsCursorForced = true;
                        break;

                    case State.OrbitMode:
                        cursorToUse = CursorSet.HandPan;
                        owningWorkspace.IsCursorForced = true;
                        break;

                    case State.None:
                        cursorToUse = CursorSet.Pointer;
                        owningWorkspace.IsCursorForced = false;
                        break;
                }

                owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(cursorToUse);
                this.currentState = newState; // update state
            }

            #endregion

            #region User Input Event Handlers

            internal bool HandleLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (ignoreMouseClick)
                {
                    ignoreMouseClick = false;
                    return false;
                }

                var eventHandled = false;
                var returnFocus = true;
                if (currentState == State.Connection)
                {
                    // Clicking on the canvas while connecting simply cancels 
                    // the operation and drop the temporary connector.
                    SetCurrentState(State.None);

                    eventHandled = true; // Mouse event handled.
                }
                else if (currentState == State.None)
                {
                    // Record the mouse down position.
                    var element = sender as IInputElement;
                    mouseDownPos = e.GetPosition(element);

                    // We'll see if there is any node being clicked on. If so, 
                    // then the state machine should initiate a drag operation.
                    if (null != GetSelectableFromPoint(mouseDownPos))
                    {
                        InitiateDragSequence();
                        returnFocus = ShouldDraggingReturnFocus();
                    }
                    else
                    {
                        if (e.ClickCount < 2) 
                        {
                            // if the Shift key is held down while the left mouse button is pressed, then do not initiate the window selection sequence
                            DynamoSelection.Instance.ClearSelectionDisabled = Keyboard.Modifiers == ModifierKeys.Shift;
                            InitiateWindowSelectionSequence();

                        }
                        else // Double-clicking on canvas.
                        {
                            // Double-clicking on the background grid results in 
                            // a code block node being created, in which case we
                            // should keep the input focus on the code block to 
                            // avoid it being dismissed (with empty content).
                            //
                            // If Shift/Ctrl is pressed, CBN shouldn't be created.
                            // Shift Modifier indicates, that user tries to call InCanvasSearch
                            // by using Shift + DoubleClick.
                            // Ctrl Modifier indicates, that user tries to copy node.
                            if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control)
                            {
                                CreateCodeBlockNode(mouseDownPos);
                            }

                            returnFocus = false;
                        }
                    }

                    eventHandled = true; // Mouse event handled.
                }
                else if (currentState == State.PanMode || currentState == State.OrbitMode)
                {
                    var c = CursorLibrary.GetCursor(CursorSet.HandPanActive);
                    owningWorkspace.CurrentCursor = c;
                }

                if (returnFocus)
                {
                    owningWorkspace.DynamoViewModel.OnRequestReturnFocusToView();
                }

                return eventHandled;
            }

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
                    var command = new DynCmd.DragSelectionCommand(mouseCursor.AsDynamoType(), operation);

                    owningWorkspace.DynamoViewModel.ExecuteCommand(command);

                    SetCurrentState(State.None); // Dragging operation ended.
                }
                else if (this.currentState == State.DragSetup)
                    SetCurrentState(State.None);
                else if (this.currentState == State.PanMode)
                {
                    // Change cursor back to Pan
                    var c = CursorLibrary.GetCursor(CursorSet.HandPan);
                    owningWorkspace.CurrentCursor = c;
                }
                else if (this.currentState == State.OrbitMode)
                {
                    var c = CursorLibrary.GetCursor(CursorSet.HandPan);
                    owningWorkspace.CurrentCursor = c;
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

                    var rect = new Dynamo.Utilities.Rect2D(x, y, width, height);

                    var command = new DynCmd.SelectInRegionCommand(rect, isCrossSelection);

                    owningWorkspace.DynamoViewModel.ExecuteCommand(command);

                }
                else if (this.currentState == State.DragSetup)
                {
                    // There are something in the selection, but none is ILocatable.
                    if (!DynamoSelection.Instance.Selection.Any((x) => (x is ILocatable)))
                    {
                        SetCurrentState(State.None);
                        return false;
                    }

                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        // Record and begin the drag operation for selected nodes.
                        var operation = DynCmd.DragSelectionCommand.Operation.BeginDrag;
                        var command = new DynCmd.DragSelectionCommand(
                            mouseCursor.AsDynamoType(),
                            operation);
                        owningWorkspace.DynamoViewModel.ExecuteCommand(command);

                        SetCurrentState(State.NodeReposition);
                        return true;
                    }

                }
                else if (this.currentState == State.NodeReposition)
                {
                    // Update the dragged nodes (note: this isn't recorded).
                    owningWorkspace.UpdateDraggedSelection(mouseCursor.AsDynamoType());
                }

                return false; // Mouse event not handled.
            }

            internal bool HandleMouseMove(object sender, MouseEventArgs e)
            {
                IInputElement element = sender as IInputElement;
                Point mouseCursor = e.GetPosition(element);
                return HandleMouseMove(sender, mouseCursor);
            }

            internal bool HandleFocusChanged(object sender, bool focused)
            {
                CancelActiveState();
                return true; // Handled.
            }

            internal bool HandlePortClicked(PortViewModel portViewModel)
            {
                // We only entertain port clicking when the current state is idle, 
                // or when it is already having a connector being established.
                if (currentState != State.None && (currentState != State.Connection))
                    return false;

                var portModel = portViewModel.PortModel;

                var workspaceViewModel = owningWorkspace.DynamoViewModel.CurrentSpaceViewModel;

                if (this.currentState != State.Connection) // Not in a connection attempt...
                {
                    Guid nodeId = portModel.Owner.GUID;
                    int portIndex = portModel.Index;

                    var mode = DynamoModel.MakeConnectionCommand.Mode.Begin;
                    if (Keyboard.Modifiers == ModifierKeys.Shift) //if shift key is down, handle multiple wire reconnections
                    {
                        multipleConnections = true;
                        mode = DynamoModel.MakeConnectionCommand.Mode.BeginShiftReconnections;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        // If the control key is held down, check if there is a need to duplicate connections
                        mode = DynamoModel.MakeConnectionCommand.Mode.BeginDuplicateConnection;
                    }

                    var command = new DynamoModel.MakeConnectionCommand(nodeId, portIndex, portModel.PortType, mode);
                    owningWorkspace.DynamoViewModel.ExecuteCommand(command);

                    if (null != owningWorkspace.FirstActiveConnector)
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
                        Guid nodeId = portModel.Owner.GUID;
                        int portIndex = portModel.Index;

                        var mode = DynamoModel.MakeConnectionCommand.Mode.End;
                        this.currentState = State.None;
                        owningWorkspace.CurrentCursor = null;

                        if (multipleConnections)
                        {
                            mode = DynamoModel.MakeConnectionCommand.Mode.EndShiftReconnections;
                        }
                        else if (Keyboard.Modifiers == ModifierKeys.Control) // If the control key is held down
                        {
                            mode = DynamoModel.MakeConnectionCommand.Mode.BeginCreateConnections;
                            this.currentState = State.Connection; // Start a new connection
                            owningWorkspace.CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect); // Reassign the cursor
                        }
                        var command = new DynamoModel.MakeConnectionCommand(nodeId,
                            portIndex, portModel.PortType, mode);
                        owningWorkspace.DynamoViewModel.ExecuteCommand(command);
                        multipleConnections = false;
                        owningWorkspace.IsCursorForced = false;
                    }
                }

                return true;
            }

            internal void PasteSelection(Point2D targetPoint)
            {
                var model = owningWorkspace.DynamoViewModel.Model;
                var oldClipboardData = model.ClipBoard.ToList();

                model.Copy();
                if (model.ClipBoard.Any())
                {
                    model.Paste(targetPoint, false);
                    owningWorkspace.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
                    owningWorkspace.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
                }

                model.ClipBoard.Clear();
                model.ClipBoard.AddRange(oldClipboardData);
                SetCurrentState(State.None);
            }

            #endregion

            #region Cancel State Methods

            private void CancelConnection()
            {
                var command = new DynCmd.MakeConnectionCommand(Guid.Empty, -1,
                        PortType.Input, DynCmd.MakeConnectionCommand.Mode.Cancel);

                owningWorkspace.DynamoViewModel.ExecuteCommand(command);
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
                foreach (var selectable in DynamoSelection.Instance.Selection)
                {
                    var locatable = selectable as ILocatable;
                    if (locatable == null || (!locatable.Rect.Contains(point.AsDynamoType())))
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

            private bool ShouldDraggingReturnFocus()
            {
                if (currentState != State.DragSetup)
                    throw new InvalidOperationException();

                // If there is only one node being dragged, and that node is a 
                // code block node, then do not set the focus back to search. 
                // This is to prevent code block editor losing its focus, causing 
                // the newly created code block to be deleted (when user is just 
                // trying to reposition the newly created node before editing its 
                // contents. User reported issue can be found here:
                // 
                //      https://github.com/DynamoDS/Dynamo/issues/1447
                // 
                if (DynamoSelection.Instance.Selection.Count == 1)
                {
                    var selectedNode = DynamoSelection.Instance.Selection[0];
                    if (selectedNode is CodeBlockNodeModel)
                        return false;
                }

                return true; // Return focus to search box.
            }

            private void InitiateWindowSelectionSequence()
            {
                // Visualization pause
                owningWorkspace.OnDragSelectionStarted(this, EventArgs.Empty);

                // The state machine must be in idle state.
                if (this.currentState != State.None)
                    throw new InvalidOperationException();

                // Clear existing selection set.
                var selectNothing = new DynCmd.SelectModelCommand(Guid.Empty, ModifierKeys.None.AsDynamoType());

                owningWorkspace.DynamoViewModel.ExecuteCommand(selectNothing);

                // Update the selection box and make it visible 
                // but with an initial dimension of zero.
                SelectionBoxUpdateArgs args = null;
                args = new SelectionBoxUpdateArgs(mouseDownPos.X, mouseDownPos.Y, 0, 0);
                args.SetVisibility(Visibility.Visible);

                this.owningWorkspace.RequestSelectionBoxUpdate(this, args);

                SetCurrentState(State.WindowSelection);
            }

            private void CreateCodeBlockNode(Point cursor)
            {
                // create node
                var node = new CodeBlockNodeModel(owningWorkspace.DynamoViewModel.Model.LibraryServices);

                owningWorkspace.DynamoViewModel.ExecuteCommand(new DynCmd.CreateNodeCommand(node, cursor.X, cursor.Y, false, true));

                //correct node position
                node.X = (int)mouseDownPos.X - 92;
                node.Y = (int)mouseDownPos.Y - 31;
            }

            #endregion
        }

    }
}
