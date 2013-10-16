using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.ViewModels
{
    partial class WorkspaceViewModel
    {
        #region State Machine Related Methods/Data Members

        private StateMachine stateMachine = null;
        private ConnectorViewModel activeConnector = null;
        private List<DraggedNode> draggedNodes = new List<DraggedNode>();

        internal bool IsConnecting { get { return null != this.activeConnector; } }

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

        internal bool HandlePortClicked(PortViewModel portViewModel)
        {
            return stateMachine.HandlePortClicked(portViewModel);
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

            // Remove connector if one already exists
            if (portModel.Connectors.Count > 0 && portModel.PortType == PortType.INPUT)
            {
                var connToRemove = portModel.Connectors[0];
                _model.Connectors.Remove(connToRemove);
                portModel.Disconnect(connToRemove);
                var startPort = connToRemove.Start;
                startPort.Disconnect(connToRemove);
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
            _model.RecordCreatedModel(newConnectorModel);
            this.SetActiveConnector(null);
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
            }
        }

        /// <summary>
        /// The StateMachine class manages states in the WorkspaceViewModel it 
        /// belongs. The class is made nested private class because there are 
        /// things that we don't expose beyond WorkspaceViewModel object, but 
        /// should still be readily accessible by the StateMachine class.
        /// </summary>
        class StateMachine
        {
            #region Private Class Data Members

            internal enum State
            {
                None,
                WindowSelection,
                DragSetup,
                NodeReposition,
                Connection
            }

            private bool ignoreMouseClick = false;
            private State currentState = State.None;
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
                currentState = State.None;
                ignoreMouseClick = true;
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
                    var command = new DynCmd.MakeConnectionCommand(Guid.Empty, -1,
                        PortType.INPUT, DynCmd.MakeConnectionCommand.Mode.Cancel);

                    var dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                    dynamoViewModel.ExecuteCommand(command);

                    this.currentState = State.None;
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

                dynSettings.ReturnFocusToSearch();
                return eventHandled;
            }

            internal bool HandleMouseRelease(object sender, MouseButtonEventArgs e)
            {
                if (e.ChangedButton != MouseButton.Left)
                    return false; // We only handle left mouse button for now.

                if (this.currentState == State.WindowSelection)
                {
                    SelectionBoxUpdateArgs args = null;
                    args = new SelectionBoxUpdateArgs(Visibility.Collapsed);
                    this.owningWorkspace.RequestSelectionBoxUpdate(this, args);
                    this.currentState = State.None;
                    return true; // Mouse event handled.
                }
                else if (this.currentState == State.NodeReposition)
                {
                    Point mouseCursor = e.GetPosition(sender as IInputElement);
                    var operation = DynCmd.DragSelectionCommand.Operation.EndDrag;
                    var command = new DynCmd.DragSelectionCommand(mouseCursor, operation);
                    var dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                    dynamoViewModel.ExecuteCommand(command);

                    this.currentState = State.None; // Dragging operation ended.
                }
                else if (this.currentState == State.DragSetup)
                    this.currentState = State.None;

                return false; // Mouse event not handled.
            }

            internal bool HandleMouseMove(object sender, MouseEventArgs e)
            {
                IInputElement element = sender as IInputElement;
                Point mouseCursor = e.GetPosition(element);

                if (this.currentState == State.Connection)
                {
                    // If we are currently connecting and there is an active 
                    // connector, redraw it to match the new mouse coordinates.
                    // 
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
                        this.currentState = State.None;
                        return false;
                    }

                    // Record and begin the drag operation for selected nodes.
                    var operation = DynCmd.DragSelectionCommand.Operation.BeginDrag;
                    var command = new DynCmd.DragSelectionCommand(mouseCursor, operation);
                    DynamoViewModel dynamoViewModel = dynSettings.Controller.DynamoViewModel;
                    dynamoViewModel.ExecuteCommand(command);

                    this.currentState = State.NodeReposition;
                    return true;
                }
                else if (this.currentState == State.NodeReposition)
                {
                    // Update the dragged nodes (note: this isn't recorded).
                    owningWorkspace.UpdateDraggedSelection(mouseCursor);
                }

                return false; // Mouse event not handled.
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
                    int portIndex = portModel.Owner.GetPortIndex(portModel, out portType);

                    dynamoViewModel.ExecuteCommand(new DynCmd.MakeConnectionCommand(
                        nodeId, portIndex, portType, DynCmd.MakeConnectionCommand.Mode.Begin));

                    if (owningWorkspace.IsConnecting)
                        this.currentState = State.Connection;
                }
                else  // Attempt to complete the connection
                {
                    PortType portType = PortType.INPUT;
                    Guid nodeId = portModel.Owner.GUID;
                    int portIndex = portModel.Owner.GetPortIndex(portModel, out portType);

                    dynamoViewModel.ExecuteCommand(new DynCmd.MakeConnectionCommand(
                        nodeId, portIndex, portType, DynCmd.MakeConnectionCommand.Mode.End));

                    this.currentState = State.None;
                }

                return true;
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

                this.currentState = State.DragSetup;
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
                this.currentState = State.WindowSelection;
            }

            #endregion
        }

    }
}
