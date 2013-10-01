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

namespace Dynamo.ViewModels
{
    partial class WorkspaceViewModel
    {
        #region State Machine Related Methods/Data Members

        private StateMachine stateMachine = null;

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

        #endregion

        class DraggedNode
        {
            double deltaX = 0, deltaY = 0;
            ILocatable locatable = null;

            internal DraggedNode(ILocatable locatable, Point mouseCursor)
            {
                this.locatable = locatable;
                deltaX = mouseCursor.X - locatable.X;
                deltaY = mouseCursor.Y - locatable.Y;
            }

            internal void Update(Point mouseCursor)
            {
                locatable.X = mouseCursor.X - deltaX;
                locatable.Y = mouseCursor.Y - deltaY;
            }
        }

        /// <summary>
        /// The StateMachine class manages various states in the WorkspaceView 
        /// it belongs. The class is made nested private class because there are 
        /// things that we don't expose beyond WorkspaceView object, but should 
        /// still be readily accessible by the StateMachine class.
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
            /// The owning WorkspaceView calls this to cancel the current state,
            /// if the current state matches the one specified in the "state" 
            /// parameter.
            /// </summary>
            /// <param name="state">The state to cancel, if the state machine 
            /// is in an internal state that matches this parameter, otherwise 
            /// the internal state will not be changed.</param>
            /// <returns>Returns true if the internal state has been 
            /// successfully changed, or false otherwise.</returns>
            /// 
            internal bool CancelState(State state)
            {
                // We only cancel the state if the specified state matches.
                if (currentState == State.None || (currentState != state))
                    return false;

                currentState = State.None;
                return true;
            }

            #endregion

            #region User Input Event Handlers

            internal bool HandleLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (this.currentState == State.Connection)
                {
                    // Clicking on the canvas while connecting simply cancels 
                    // the operation and drop the temporary connector.
                    this.currentState = State.None;
                    owningWorkspace.ActiveConnector = null;
                    return true; // Mouse event handled.
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

                    return true; // Mouse event handled.
                }

                dynSettings.ReturnFocusToSearch();
                return false;
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
                    this.currentState = State.None;
                else if (this.currentState == State.DragSetup)
                    this.currentState = State.None;

                return false; // Mouse event not handled.
            }

            internal bool HandleMouseMove(object sender, MouseEventArgs e)
            {
                if (this.currentState == State.Connection)
                {
                    // If we are currently connecting and there is an active 
                    // connector, redraw it to match the new mouse coordinates.
                    // 
                    if (null != owningWorkspace.ActiveConnector)
                    {
                        IInputElement element = sender as IInputElement;
                        Point mouse = e.GetPosition(element);
                        owningWorkspace.ActiveConnector.Redraw(mouse);
                    }
                }
                else if (this.currentState == State.WindowSelection)
                {
                    // When the mouse is held down, reposition the drag selection box.
                    IInputElement element = sender as IInputElement;
                    Point mousePos = e.GetPosition(element);

                    // TODO(Ben): Can we not only select those nodes that we 
                    // have not previously selected? Of course that requires 
                    // us to take deselection into consideration.
                    // 
                    // Clear the selected elements before reselecting 
                    // all nodes that fall within the selection window.
                    DynamoSelection.Instance.ClearSelection();

                    double x = Math.Min(mouseDownPos.X, mousePos.X);
                    double y = Math.Min(mouseDownPos.Y, mousePos.Y);
                    double width = Math.Abs(mouseDownPos.X - mousePos.X);
                    double height = Math.Abs(mousePos.Y - mouseDownPos.Y);

                    // We perform cross selection (i.e. select a node whenever 
                    // it touches the selection box as opposed to only select 
                    // it when it is entirely within the selection box) when 
                    // mouse moves in the opposite direction (i.e. the current 
                    // mouse position is smaller than the point mouse-down 
                    // happened).
                    // 
                    bool isCrossSelection = mousePos.X < mouseDownPos.X;

                    SelectionBoxUpdateArgs args = null;
                    args = new SelectionBoxUpdateArgs(x, y, width, height);
                    args.SetSelectionMode(isCrossSelection);
                    this.owningWorkspace.RequestSelectionBoxUpdate(this, args);

                    var rect = new Rect(x, y, width, height);

                    if (isCrossSelection)
                        owningWorkspace.CrossSelectCommand.Execute(rect);
                    else
                        owningWorkspace.ContainSelectCommand.Execute(rect);
                }
                else if (this.currentState == State.DragSetup)
                {
                    this.currentState = State.NodeReposition;
                }
                else if (this.currentState == State.NodeReposition)
                {
                }

                return false; // Mouse event not handled.
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

                DynamoSelection.Instance.ClearSelection();

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
