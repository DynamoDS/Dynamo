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

        // Animation controllers.
        private DoubleAnimation controlOpacityAnimation = null;
        private DoubleAnimation controlOffsetAnimation = null;

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

        #region Private Class Methods

        private void SetupAnimators()
        {
            if (controlOpacityAnimation == null)
            {
                var ms = Configurations.FadeInOutDurationInMs;
                controlOpacityAnimation = new DoubleAnimation();
                controlOpacityAnimation.AutoReverse = false;
                controlOpacityAnimation.Duration = TimeSpan.FromMilliseconds(ms);
                controlOpacityAnimation.Completed += OnPreviewOpacityAnimationCompleted;
            }

            if (controlOffsetAnimation == null)
            {
                var ms = Configurations.FadeInOutDurationInMs;
                controlOffsetAnimation = new DoubleAnimation();
                controlOffsetAnimation.AutoReverse = false;
                controlOffsetAnimation.Duration = TimeSpan.FromMilliseconds(ms);
            }
        }

        private void CenterHorizontallyOnHostCanvas()
        {
            var horzOffset = ((HostingCanvas.ActualWidth - this.ActualWidth) * 0.5);
            SetValue(Canvas.LeftProperty, horzOffset);
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

            this.SetupAnimators();

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

        private void BeginFadeInTransition()
        {
            if (this.currentState != State.Hidden)
                throw new InvalidOperationException();

            CenterHorizontallyOnHostCanvas();

            this.Opacity = 0.0;
            this.Visibility = System.Windows.Visibility.Visible;
            this.currentTransition = Transition.FadingIn;

            controlOpacityAnimation.From = 0.0;
            controlOpacityAnimation.To = 1.0;
            this.BeginAnimation(UIElement.OpacityProperty, controlOpacityAnimation);

            controlOffsetAnimation.From = Configurations.PreviewHiddenOffset;
            controlOffsetAnimation.To = 0.0;
            this.BeginAnimation(Canvas.TopProperty, controlOffsetAnimation);
        }

        private void BeginFadeOutTransition()
        {
            if (this.currentState != State.Condensed)
                throw new InvalidOperationException();

            this.currentTransition = Transition.FadingOut;

            controlOpacityAnimation.From = 1.0;
            controlOpacityAnimation.To = 0.0;
            this.BeginAnimation(UIElement.OpacityProperty, controlOpacityAnimation);

            controlOffsetAnimation.From = 0.0;
            controlOffsetAnimation.To = Configurations.PreviewHiddenOffset;
            this.BeginAnimation(Canvas.TopProperty, controlOffsetAnimation);
        }

        private void BeginCondenseTransition()
        {
            if (this.currentState != State.Expanded)
                throw new InvalidOperationException();
        }

        private void BeginExpandTransition()
        {
            if (this.currentState != State.Condensed)
                throw new InvalidOperationException();
        }

        #endregion

        #region Private Event Handlers

        private void OnPreviewControlLoaded(object sender, RoutedEventArgs e)
        {
            // If there was a request queued before this control is loaded, 
            // then process the request as we now have the right width.
            if (this.currentTransition == Transition.None)
                DequeueAndBeginTransition();
        }

        private void OnPreviewOpacityAnimationCompleted(object sender, EventArgs e)
        {
            if (this.currentTransition == Transition.FadingIn)
            {
                this.currentState = State.Condensed;
            }
            else if (this.currentTransition == Transition.FadingOut)
            {
                this.Visibility = System.Windows.Visibility.Hidden;
                this.currentState = State.Hidden;
            }
            else
            {
                // Opacity of the preview control is only enabled for fading in/out.
                var message = "Opacity animation is only expected for fading in/out";
                throw new InvalidOperationException(message);
            }

            this.currentTransition = Transition.None;
            DequeueAndBeginTransition(); // See if there's any more requests.
        }

        #endregion
    }
}
