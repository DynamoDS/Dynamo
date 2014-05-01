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
    // Event to be sent when PreviewControl goes into a stable 
    // state (e.g. when all on-going storyboards have been completed).
    public delegate void StateChangedEventHandler(object sender, EventArgs e);

    public partial class PreviewControl : UserControl
    {
        #region Class Data Members and Configurations

        public enum State
        {
            Hidden, Condensed, Expanded, Transitional
        }

        private const double MaxPreviewWidth = 500.0;
        private const double MaxPreviewHeight = 300.0;

        private State currentState = State.Hidden;
        private Queue<State> queuedRequest = new Queue<State>();
        private Canvas hostingCanvas = null;

        // Animation storyboards.
        private Storyboard phaseInStoryboard = null;
        private Storyboard phaseOutStoryboard = null;
        private Storyboard expandStoryboard = null;
        private Storyboard condenseStoryboard = null;
        private DoubleAnimation widthAnimator = null;
        private DoubleAnimation heightAnimator = null;

        public event StateChangedEventHandler StateChanged = null;

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
            if (this.IsLoaded)
                DequeueAndBeginTransition();
        }

        #endregion

        #region Public Class Properties

        internal bool IsHidden { get { return currentState == State.Hidden; } }
        internal bool IsCondensed { get { return currentState == State.Condensed; } }
        internal bool IsExpanded { get { return currentState == State.Expanded; } }
        internal bool IsTransitional { get { return currentState == State.Transitional; } }

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
            if (this.IsTransitional || (queuedRequest.Count <= 0))
                return; // Still in transition or nothing else to do.

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
                if (this.IsHidden != false)
                    BeginFadeInTransition();
                else if (this.IsExpanded != false)
                    BeginCondenseTransition();
            }
            else if (requestedState == State.Expanded)
            {
                BeginExpandTransition();
            }
        }

        private void SetCurrentStateAndNotify(State newState)
        {
            this.currentState = newState;
            if (this.StateChanged != null)
                this.StateChanged(this, EventArgs.Empty);
        }

        static bool switched = false;

        private Size ComputeLargeContentSize()
        {
            // TODO: Remove these. Temporary logic to ensure animation's 
            // value can be updated before the storyboard is started.
            // 
            Size size = new Size(180.0, 310.0);
            switched = !switched;

            if (switched)
            {
                size.Width = 310.0;
                size.Height = 180.0;
            }

            // Cap the values within reasonable size.
            if (size.Width > MaxPreviewWidth)
                size.Width = MaxPreviewWidth;
            if (size.Height > MaxPreviewHeight)
                size.Height = MaxPreviewHeight;

            return size;
        }

        #endregion

        #region Private Class Methods - Transition Helpers

        private void BeginFadeInTransition()
        {
            if (this.IsHidden == false)
                throw new InvalidOperationException();

            CenterHorizontallyOnHostCanvas();

            this.centralizedGrid.Opacity = 0.0;
            this.centralizedGrid.Visibility = System.Windows.Visibility.Visible;
            this.smallContentGrid.Visibility = System.Windows.Visibility.Visible;

            SetCurrentStateAndNotify(State.Transitional);
            phaseInStoryboard.Begin(this, true);
        }

        private void BeginFadeOutTransition()
        {
            if (this.IsCondensed == false)
                throw new InvalidOperationException();

            SetCurrentStateAndNotify(State.Transitional);
            phaseOutStoryboard.Begin(this, true);
        }

        private void BeginCondenseTransition()
        {
            if (this.IsExpanded == false)
                throw new InvalidOperationException();

            this.smallContentGrid.Visibility = System.Windows.Visibility.Visible;
            SetCurrentStateAndNotify(State.Transitional);
            this.condenseStoryboard.Begin(this, true);
        }

        private void BeginExpandTransition()
        {
            if (this.IsCondensed == false)
                throw new InvalidOperationException();

            this.largeContentGrid.Visibility = System.Windows.Visibility.Visible;
            SetCurrentStateAndNotify(State.Transitional);

            var largeContentSize = ComputeLargeContentSize();
            this.widthAnimator.To = largeContentSize.Width;
            this.heightAnimator.To = largeContentSize.Height;
            this.expandStoryboard.Begin(this, true);
        }

        #endregion

        #region Private Event Handlers

        private void OnPreviewControlLoaded(object sender, RoutedEventArgs e)
        {
            phaseInStoryboard = this.Resources["phaseInStoryboard"] as Storyboard;
            phaseOutStoryboard = this.Resources["phaseOutStoryboard"] as Storyboard;
            expandStoryboard = this.Resources["expandStoryboard"] as Storyboard;
            condenseStoryboard = this.Resources["condenseStoryboard"] as Storyboard;

            // There must be width and height animators under expansion storyboard.
            widthAnimator = expandStoryboard.Children[0] as DoubleAnimation;
            heightAnimator = expandStoryboard.Children[1] as DoubleAnimation;
            if (widthAnimator == null || (!widthAnimator.Name.Equals("widthAnimator")))
                throw new InvalidOperationException("Width animator expected");
            if (heightAnimator == null || (!heightAnimator.Name.Equals("heightAnimator")))
                throw new InvalidOperationException("Height animator expected");

            // If there was a request queued before this control is loaded, 
            // then process the request as we now have the right width.
            DequeueAndBeginTransition();
        }

        private void OnPreviewControlPhasedIn(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Condensed);
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlPhasedOut(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Hidden);
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlExpanded(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Expanded);
            smallContentGrid.Visibility = System.Windows.Visibility.Hidden;
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlCondensed(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Condensed);
            largeContentGrid.Visibility = System.Windows.Visibility.Hidden;
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        #endregion
    }
}
