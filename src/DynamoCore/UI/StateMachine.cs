using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Views;

namespace Dynamo.UI
{
    class StateMachine
    {
        #region Private Class Data Members

        internal enum State
        {
            None,
            SelectionBox,
            NodeReposition,
            Connection
        }

        State currentState = State.None;
        dynWorkspaceView owningWorkspace = null;

        #endregion

        #region Public Class Operational Methods

        internal StateMachine(dynWorkspaceView owningWorkspace)
        {
            this.owningWorkspace = owningWorkspace;
        }

        /// <summary>
        /// The owning WorkspaceView calls this to cancel the current state, if
        /// the current state matches the one specified in the "state" parameter.
        /// </summary>
        /// <param name="state">The state to cancel, if the state machine is in 
        /// an internal state that matches this parameter, otherwise the internal 
        /// state will not be changed.</param>
        /// <returns>Returns true if the internal state has been successfully 
        /// changed, or false otherwise.</returns>
        internal bool CancelState(State state)
        {
            // We only cancel the state if the specified state matches.
            if (currentState == State.None || (currentState != state))
                return false;

            UpdateInternalState(State.None);
            return true;
        }

        #endregion

        #region Private Class Helper Method

        private void UpdateInternalState(State newState)
        {
            if (currentState == newState)
                throw new InvalidOperationException("Unexpected state change!");

            // Update the internal state before notifying the owning workspace.
            State oldState = currentState;
            currentState = newState;
            this.owningWorkspace.HandleStateTransition(oldState, currentState);
        }

        #endregion
    }
}
