using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for PreviewControl.xaml
    /// </summary>
    public partial class PreviewControl : UserControl
    {
        #region Private Class Data Members

        public enum State
        {
            Hidden, Condensed, Expanded
        }

        private enum Transition
        {
            None, FadingIn, Expanding, Condensing, FadingOut
        }

        private State currentState = State.Hidden;
        private Transition currentTransition = Transition.None;
        private Queue<State> queuedRequest = new Queue<State>();

        #endregion

        #region Public Class Operational Methods

        public PreviewControl()
        {
            InitializeComponent();
        }

        public void TransitionToState(State nextState)
        {
            queuedRequest.Enqueue(nextState);
        }

        #endregion

        #region Public Class Properties

        public State CurrentState { get { return currentState; } }

        #endregion
    }
}
