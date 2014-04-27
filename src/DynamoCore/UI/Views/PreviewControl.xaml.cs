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
using System.Windows.Media.Animation;
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
        private Canvas hostingCanvas = null;

        // Animation storyboards.
        private Storyboard phaseInStoryboard = null;
        private Storyboard phaseOutStoryboard = null;
        private Storyboard expandStoryboard = null;
        private Storyboard condenseStoryboard = null;

        #endregion

        #region Public Class Operational Methods

        public PreviewControl()
        {
            InitializeComponent();
            this.Loaded += OnPreviewControlLoaded;
        }

        public void TransitionToState(State nextState)
        {
            queuedRequest.Enqueue(nextState);

            // If this call is made before the PreviewControl is loaded, then 
            // it won't have the necessary width information for alignment 
            // computation (the PreviewControl is to be centralized horizontally
            // with respect to the HostingCanvas). So if the control is not yet 
            // loaded, then do not attempt to process the transition request.
            // 
            if (this.IsLoaded && (this.currentTransition == Transition.None))
                DequeueAndBeginTransition();
        }

        #endregion

        #region Public Class Properties

        public State CurrentState { get { return currentState; } }

        private Canvas HostingCanvas
        {
            get
            {
                if (this.hostingCanvas == null)
                {
                    this.hostingCanvas = this.Parent as Canvas;
                    if (this.hostingCanvas == null)
                    {
                        var message = "PreviewControl must be a child of Canvas";
                        throw new InvalidOperationException(message);
                    }
                }

                return this.hostingCanvas;
            }
        }

        #endregion

        #region Private Class Methods - Generic Helpers

        private void CenterHorizontallyOnHostCanvas()
        {
            var widthDifference = HostingCanvas.ActualWidth - this.ActualWidth;
            SetValue(Canvas.LeftProperty, widthDifference * 0.5);
        }

        private void DequeueAndBeginTransition()
        {
            if (this.currentTransition != Transition.None)
                throw new InvalidOperationException("Still in transition");

            if (queuedRequest.Count <= 0)
                return; // Nothing else to do now, moving on.

            State requestedState = queuedRequest.Dequeue();
            while (requestedState == this.currentState)
            {
                if (queuedRequest.Count <= 0)
                    return; // There's no more request for now.
            }

            if (requestedState == State.Hidden)
            {
                BeginFadeOutTransition();
            }
            else if (requestedState == State.Condensed)
            {
                if (this.currentState == State.Hidden)
                    BeginFadeInTransition();
                else if (this.currentState == State.Expanded)
                    BeginCondenseTransition();
            }
            else if (requestedState == State.Expanded)
            {
                BeginExpandTransition();
            }
        }

        #endregion

        #region Private Class Methods - Transition Helpers

        private void BeginFadeInTransition()
        {
            if (this.currentState != State.Hidden)
                throw new InvalidOperationException();

            CenterHorizontallyOnHostCanvas();

            this.Opacity = 0.0;
            this.Visibility = System.Windows.Visibility.Visible;
            this.currentTransition = Transition.FadingIn;
            this.smallContentGrid.Visibility = System.Windows.Visibility.Visible;
            phaseInStoryboard.Begin();
        }

        private void BeginFadeOutTransition()
        {
            if (this.currentState != State.Condensed)
                throw new InvalidOperationException();

            this.currentTransition = Transition.FadingOut;
            phaseOutStoryboard.Begin();
        }

        private void BeginCondenseTransition()
        {
            if (this.currentState != State.Expanded)
                throw new InvalidOperationException();

            this.currentTransition = Transition.Condensing;
            this.smallContentGrid.Visibility = System.Windows.Visibility.Visible;
            this.condenseStoryboard.Begin();
        }

        private void BeginExpandTransition()
        {
            if (this.currentState != State.Condensed)
                throw new InvalidOperationException();

            this.currentTransition = Transition.Expanding;
            this.largeContentGrid.Visibility = System.Windows.Visibility.Visible;
            this.expandStoryboard.Begin();
        }

        #endregion

        #region Private Event Handlers

        private void OnPreviewControlLoaded(object sender, RoutedEventArgs e)
        {
            phaseInStoryboard = this.Resources["phaseInStoryboard"] as Storyboard;
            phaseOutStoryboard = this.Resources["phaseOutStoryboard"] as Storyboard;
            expandStoryboard = this.Resources["expandStoryboard"] as Storyboard;
            condenseStoryboard = this.Resources["condenseStoryboard"] as Storyboard;

            // If there was a request queued before this control is loaded, 
            // then process the request as we now have the right width.
            if (this.currentTransition == Transition.None)
                DequeueAndBeginTransition();
        }

        private void OnPreviewControlPhasedIn(object sender, EventArgs e)
        {
            this.currentState = State.Condensed;
            this.currentTransition = Transition.None;
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlPhasedOut(object sender, EventArgs e)
        {
            this.currentState = State.Hidden;
            this.currentTransition = Transition.None;
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlExpanded(object sender, EventArgs e)
        {
            this.currentState = State.Expanded;
            this.currentTransition = Transition.None;
            smallContentGrid.Visibility = System.Windows.Visibility.Hidden;

            this.currentTransition = Transition.None;
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlCondensed(object sender, EventArgs e)
        {
            this.currentState = State.Condensed;
            this.currentTransition = Transition.None;
            largeContentGrid.Visibility = System.Windows.Visibility.Hidden;

            this.currentTransition = Transition.None;
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlSizeChanged(object sender, EventArgs e)
        {
            CenterHorizontallyOnHostCanvas();
        }

        #endregion
    }
}
