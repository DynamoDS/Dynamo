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

namespace Dynamo.Views
{
    partial class dynWorkspaceView
    {
        #region State Machine Related Methods/Data Members

        private StateMachine stateMachine = null;

        #endregion

        /// <summary>
        /// The StateMachine class manages various states in the WorkspaceView it 
        /// belongs. The class is made nested private class because there are 
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
            private dynWorkspaceView owningWorkspace = null;

            #endregion

            #region Public Class Operational Methods

            internal StateMachine(dynWorkspaceView owningWorkspace)
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

            internal void HandleLeftButtonDown(MouseButtonEventArgs e)
            {
                if (this.currentState == State.Connection)
                {
                    // Clicking on the canvas while connecting simply cancels 
                    // the operation and drop the temporary connector.
                    this.currentState = State.None;
                    object dataContext = owningWorkspace.DataContext;
                    WorkspaceViewModel wvm = dataContext as WorkspaceViewModel;
                    wvm.ActiveConnector = null;
                }
                else if (this.currentState != State.Connection)
                {
                    DynamoSelection.Instance.ClearSelection();

                    // Record the mouse down position.
                    mouseDownPos = e.GetPosition(owningWorkspace.WorkBench);

                    // Initial placement of the drag selection box.         
                    Rectangle selectionBox = owningWorkspace.selectionBox;
                    Canvas.SetLeft(selectionBox, mouseDownPos.X);
                    Canvas.SetTop(selectionBox, mouseDownPos.Y);
                    selectionBox.Width = 0;
                    selectionBox.Height = 0;

                    // Make the drag selection box visible.
                    selectionBox.Visibility = Visibility.Visible;
                    this.currentState = State.WindowSelection;
                }

                dynSettings.ReturnFocusToSearch();
            }

            internal void HandleMouseRelease(MouseButtonEventArgs e)
            {
                if (e.ChangedButton != MouseButton.Left)
                    return; // We only handle left mouse button for now.

                if (this.currentState == State.WindowSelection)
                {
                    Rectangle selectionBox = owningWorkspace.selectionBox;
                    selectionBox.Visibility = Visibility.Collapsed;
                    this.currentState = State.None;
                }
            }

            internal void HandleMouseMove(MouseEventArgs e)
            {
                if (this.currentState == State.Connection)
                {
                    // If we are currently connecting and there is an active 
                    // connector, redraw it to match the new mouse coordinates.
                    // 
                    object dataContext = owningWorkspace.DataContext;
                    var wvm = dataContext as WorkspaceViewModel;
                    if (null != wvm.ActiveConnector)
                        wvm.ActiveConnector.Redraw(e.GetPosition(owningWorkspace.WorkBench));
                }
                else if (this.currentState == State.WindowSelection)
                {
                    // When the mouse is held down, reposition the drag selection box.
                    object dataContext = owningWorkspace.DataContext;
                    var wvm = dataContext as WorkspaceViewModel;

                    Point mousePos = e.GetPosition(owningWorkspace.WorkBench);
                    Rectangle selectionBox = owningWorkspace.selectionBox;

                    if (mouseDownPos.X < mousePos.X)
                    {
                        Canvas.SetLeft(selectionBox, mouseDownPos.X);
                        selectionBox.Width = mousePos.X - mouseDownPos.X;
                    }
                    else
                    {
                        Canvas.SetLeft(selectionBox, mousePos.X);
                        selectionBox.Width = mouseDownPos.X - mousePos.X;
                    }

                    if (mouseDownPos.Y < mousePos.Y)
                    {
                        Canvas.SetTop(selectionBox, mouseDownPos.Y);
                        selectionBox.Height = mousePos.Y - mouseDownPos.Y;
                    }
                    else
                    {
                        Canvas.SetTop(selectionBox, mousePos.Y);
                        selectionBox.Height = mouseDownPos.Y - mousePos.Y;
                    }

                    // TODO(Ben): Can we not only select those nodes that we 
                    // have not previously selected? Of course that requires 
                    // us to take deselection into consideration.
                    // 
                    // Clear the selected elements before reselecting 
                    // all nodes that fall within the selection window.
                    DynamoSelection.Instance.ClearSelection();

                    var rect = new Rect(
                        Canvas.GetLeft(selectionBox),
                        Canvas.GetTop(selectionBox),
                        selectionBox.Width, selectionBox.Height);

                    if (mousePos.X > mouseDownPos.X)
                    {
                        selectionBox.StrokeDashArray = null;
                        wvm.ContainSelectCommand.Execute(rect);
                    }
                    else if (mousePos.X < mouseDownPos.X)
                    {
                        selectionBox.StrokeDashArray = new DoubleCollection { 4 };
                        wvm.CrossSelectCommand.Execute(rect);
                    }
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
