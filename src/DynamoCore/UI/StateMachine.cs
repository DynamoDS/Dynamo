using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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

        internal void HandleLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            stateMachine.HandleLeftButtonDown(sender, e);
        }

        internal void HandleMouseRelease(object sender, MouseButtonEventArgs e)
        {
            stateMachine.HandleMouseRelease(sender, e);
        }

        internal void HandleMouseMove(object sender, MouseEventArgs e)
        {
            stateMachine.HandleMouseMove(sender, e);
        }

        #endregion

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

            internal void HandleLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (this.currentState == State.Connection)
                {
                    // Clicking on the canvas while connecting simply cancels 
                    // the operation and drop the temporary connector.
                    this.currentState = State.None;
                    owningWorkspace.ActiveConnector = null;
                }
                else if (this.currentState != State.Connection)
                {
                    DynamoSelection.Instance.ClearSelection();

                    // Record the mouse down position.
                    IInputElement element = sender as IInputElement;
                    mouseDownPos = e.GetPosition(element);

                    // Update the selection box and make it visible 
                    // but with an initial dimension of zero.
                    SelectionBoxUpdateArgs args = null;
                    args = new SelectionBoxUpdateArgs(mouseDownPos.X, mouseDownPos.Y, 0, 0);
                    args.SetVisibility(Visibility.Visible);

                    this.owningWorkspace.RequestSelectionBoxUpdate(this, args);
                    this.currentState = State.WindowSelection;
                }

                dynSettings.ReturnFocusToSearch();
            }

            internal void HandleMouseRelease(object sender, MouseButtonEventArgs e)
            {
                if (e.ChangedButton != MouseButton.Left)
                    return; // We only handle left mouse button for now.

                if (this.currentState == State.WindowSelection)
                {
                    SelectionBoxUpdateArgs args = null;
                    args = new SelectionBoxUpdateArgs(Visibility.Collapsed);
                    this.owningWorkspace.RequestSelectionBoxUpdate(this, args);
                    this.currentState = State.None;
                }
            }

            internal void HandleMouseMove(object sender, MouseEventArgs e)
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
                else if (this.currentState == State.NodeReposition)
                {
                }
            }

            #endregion

            #region Private Class Helper Method

            #endregion
        }

    }
}
