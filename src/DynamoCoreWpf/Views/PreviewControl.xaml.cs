using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.ViewModels;
using ProtoCore.Mirror;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

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
            Hidden, Condensed, Expanded, InTransition
        }

        public enum SizeAnimator
        {
            PhaseIn, Expansion, Condensation, Resizing
        }

        private struct Element
        {
            public const string PhaseInWidthAnimator = "phaseInWidthAnimator";
            public const string PhaseInHeightAnimator = "phaseInHeightAnimator";
            public const string ExpandWidthAnimator = "expandWidthAnimator";
            public const string ExpandHeightAnimator = "expandHeightAnimator";
            public const string CondenseWidthAnimator = "condenseWidthAnimator";
            public const string CondenseHeightAnimator = "condenseHeightAnimator";
            public const string GridWidthAnimator = "gridWidthAnimator";
            public const string GridHeightAnimator = "gridHeightAnimator";
        }

        private readonly NodeViewModel nodeViewModel;

        private State currentState = State.Hidden;
        private Queue<State> queuedRequest = new Queue<State>();
        private Canvas hostingCanvas = null;

        // Data source and display.
        private MirrorData mirrorData = null;
        private String cachedSmallContent = null;
        private WatchViewModel cachedLargeContent = null;

        // Animation storyboards.
        private Storyboard phaseInStoryboard = null;
        private Storyboard phaseOutStoryboard = null;
        private Storyboard expandStoryboard = null;
        private Storyboard condenseStoryboard = null;
        private Storyboard resizingStoryboard = null;
        private Dictionary<string, DoubleAnimation> sizeAnimators = null;

        public event StateChangedEventHandler StateChanged = null;

        #endregion

        #region Public Class Operational Methods

        public PreviewControl(NodeViewModel nodeViewModel)
        {
            this.nodeViewModel = nodeViewModel;
            InitializeComponent();
            Loaded += OnPreviewControlLoaded;
        }

        /// <summary>
        /// Call this method to request the preview control for a transition 
        /// into a new state. The requested state must be a logical state after
        /// the current state. The following sequence illustrates the logical 
        /// state progression of a preview control:
        /// 
        ///     Hidden -> Condensed -> Expanded -> Condensed -> Hidden
        /// 
        /// For example, an exception will be thrown if State.Expanded is
        /// requested when the preview control is currently in State.Hidden,
        /// skipping State.Condensed state.
        /// 
        /// Because this method deals with UI resources like Storyboard, this 
        /// call must be made on the main UI thread.
        /// </summary>
        /// <param name="nextState">The state for the preview control to 
        /// transition into.</param>
        /// 
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
                BeginNextTransition();
        }

        /// <summary>
        /// Bind a mirror data to the preview control for display, this call 
        /// unbinds the internal data structure from the view that it was 
        /// originally bound to and resets the data structure. If this call is 
        /// made while the preview control is in condensed or expanded state,
        /// the display will immediately be refreshed. Since this method deals 
        /// with UI elements internally, it must be called from the UI thread.
        /// </summary>
        /// <param name="mirrorData">The mirror data to bind the preview control
        /// to. This value can be null to reset the preview control to its 
        /// initial state.</param>
        /// 
        internal void BindToDataSource(MirrorData mirrorData)
        {
            // First detach the bound data from its view.
            ResetContentViews();

            this.mirrorData = mirrorData;
            this.cachedLargeContent = null; // Reset expanded content.
            this.cachedSmallContent = null; // Reset condensed content.

            // If at the time of data binding the preview control is within the 
            // following states, then its contents need to be updated immediately.
            if (this.IsCondensed)
            {
                RefreshCondensedDisplay();
                BeginViewSizeTransition(ComputeSmallContentSize());
            }
            else if (this.IsExpanded)
            {
                RefreshExpandedDisplay();
                BeginViewSizeTransition(ComputeLargeContentSize());
            }
        }

        #endregion

        #region Public Class Properties

        internal bool IsDataBound { get { return mirrorData != null; } }
        internal bool IsHidden { get { return currentState == State.Hidden; } }
        internal bool IsCondensed { get { return currentState == State.Condensed; } }
        internal bool IsExpanded { get { return currentState == State.Expanded; } }
        internal bool IsInTransition { get { return currentState == State.InTransition; } }

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

        private void BeginNextTransition()
        {
            if (this.IsInTransition || (queuedRequest.Count <= 0))
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

        private void ResetContentViews()
        {
            var smallContentView = smallContentGrid.Children[0] as TextBlock;
            smallContentView.Text = string.Empty;

            if (largeContentGrid.Children.Count <= 0)
                return; // No view to reset, return now.

            var watchTree = largeContentGrid.Children[0] as WatchTree;
            var rootDataContext = watchTree.DataContext as WatchViewModel;

            // Unbind the view from data context, then clear the data context.
            BindingOperations.ClearAllBindings(watchTree.treeView1);
            rootDataContext.Children.Clear();
        }

        private void RefreshCondensedDisplay()
        {
            // The preview control will not have its content refreshed unless 
            // the content is null. In order to perform real refresh, new data 
            // source needs to be rebound to the preview control by calling 
            // BindToDataSource method.
            // 
            if (cachedSmallContent != null)
                return;

            cachedSmallContent = "null";
            if (mirrorData != null)
            {
                if (mirrorData.IsCollection)
                {
                    // TODO(Ben): Can we display details of the array and 
                    // probably display the first element of the array (even 
                    // when it is multi-dimensional array)?
                    cachedSmallContent = "Array";
                }
                else if (mirrorData.Data == null && !mirrorData.IsNull && mirrorData.Class != null)
                {
                    cachedSmallContent = mirrorData.Class.ClassName;
                }
                else
                {
                    cachedSmallContent = mirrorData.StringData;
                }
            }

            var smallContentView = smallContentGrid.Children[0] as TextBlock;
            smallContentView.Text = cachedSmallContent; // Update displayed text.
        }

        private void RefreshExpandedDisplay()
        {
            // The preview control will not have its content refreshed unless 
            // the content is null. In order to perform real refresh, new data 
            // source needs to be rebound to the preview control by calling 
            // BindToDataSource method.
            // 
            if (this.cachedLargeContent != null)
                return;

            if (largeContentGrid.Children.Count <= 0)
            {
                var newWatchTree = new WatchTree();
                newWatchTree.DataContext = new WatchViewModel(nodeViewModel.DynamoViewModel.VisualizationManager);
                largeContentGrid.Children.Add(newWatchTree);
            }

            var watchTree = largeContentGrid.Children[0] as WatchTree;
            var rootDataContext = watchTree.DataContext as WatchViewModel;

            // Associate the data context to the view before binding.
            cachedLargeContent = nodeViewModel.DynamoViewModel.WatchHandler.GenerateWatchViewModelForData(
                mirrorData, null, string.Empty, false);

            rootDataContext.Children.Add(cachedLargeContent);

            // Establish data binding between data context and the view.
            watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty,
                new Binding("Children")
                {
                    Mode = BindingMode.TwoWay,
                    Source = rootDataContext
                });
        }

        private Size ComputeSmallContentSize()
        {
            Size maxSize = new Size(){
                Width = Configurations.MaxCondensedPreviewWidth,
                Height = Configurations.MaxCondensedPreviewHeight
            };

            this.smallContentGrid.Measure(maxSize);
            Size smallContentGridSize = this.smallContentGrid.DesiredSize;

            foreach (UIElement child in smallContentGrid.Children)
            {
                child.Measure(maxSize);
                if (child.DesiredSize.Width > smallContentGridSize.Width)
                {
                    smallContentGridSize.Width = child.DesiredSize.Width + 10;
                }
                if (child.DesiredSize.Height > smallContentGridSize.Height)
                {
                    smallContentGridSize.Height = child.DesiredSize.Height + 10;
                }
            }

            // Add padding since we are sizing the centralizedGrid.
            return ContentToControlSize(smallContentGridSize);
        }

        private Size ComputeLargeContentSize()
        {
            this.largeContentGrid.Measure(new Size()
            {
                Width = Configurations.MaxExpandedPreviewWidth,
                Height = Configurations.MaxExpandedPreviewHeight
            });

            // Add padding since we are sizing the centralizedGrid.
            return ContentToControlSize(this.largeContentGrid.DesiredSize);
        }

        private Size ContentToControlSize(Size size)
        {
            if (size.Width < Configurations.DefCondensedContentWidth)
                size.Width = Configurations.DefCondensedContentWidth;
            if (size.Height < Configurations.DefCondensedContentHeight)
                size.Height = Configurations.DefCondensedContentHeight;

            return size;
        }

        private void UpdateAnimatorTargetSize(SizeAnimator animator, Size targetSize)
        {
            string widthAnimator = string.Empty;
            string heightAnimator = string.Empty;

            switch (animator)
            {
                case SizeAnimator.PhaseIn:
                    widthAnimator = Element.PhaseInWidthAnimator;
                    heightAnimator = Element.PhaseInHeightAnimator;
                    break;

                case SizeAnimator.Expansion:
                    widthAnimator = Element.ExpandWidthAnimator;
                    heightAnimator = Element.ExpandHeightAnimator;
                    break;

                case SizeAnimator.Condensation:
                    widthAnimator = Element.CondenseWidthAnimator;
                    heightAnimator = Element.CondenseHeightAnimator;
                    break;

                case SizeAnimator.Resizing:
                    widthAnimator = Element.GridWidthAnimator;
                    heightAnimator = Element.GridHeightAnimator;
                    break;
            }

            sizeAnimators[widthAnimator].To = targetSize.Width;
            sizeAnimators[heightAnimator].To = targetSize.Height;
        }

        #endregion

        #region Private Class Methods - Transition Helpers

        private void BeginFadeInTransition()
        {
            if (this.IsHidden == false)
                throw new InvalidOperationException();

            CenterHorizontallyOnHostCanvas();
            RefreshCondensedDisplay(); // Bind data to the view, if needed.

            // Update size before fading in to view.
            var smallContentSize = ComputeSmallContentSize();
            UpdateAnimatorTargetSize(SizeAnimator.PhaseIn, smallContentSize);

            this.centralizedGrid.Opacity = 0.0;
            this.centralizedGrid.Visibility = System.Windows.Visibility.Visible;
            this.smallContentGrid.Visibility = System.Windows.Visibility.Visible;

            SetCurrentStateAndNotify(State.InTransition);
            phaseInStoryboard.Begin(this, true);
        }

        private void BeginFadeOutTransition()
        {
            if (this.IsCondensed == false)
                throw new InvalidOperationException();

            SetCurrentStateAndNotify(State.InTransition);
            phaseOutStoryboard.Begin(this, true);
        }

        private void BeginCondenseTransition()
        {
            if (this.IsExpanded == false)
                throw new InvalidOperationException();

            RefreshCondensedDisplay(); // Bind data to the view, if needed.

            this.smallContentGrid.Visibility = System.Windows.Visibility.Visible;
            SetCurrentStateAndNotify(State.InTransition);

            var smallContentSize = ComputeSmallContentSize();
            UpdateAnimatorTargetSize(SizeAnimator.Condensation, smallContentSize);
            this.condenseStoryboard.Begin(this, true);
        }

        private void BeginExpandTransition()
        {
            if (this.IsCondensed == false)
                throw new InvalidOperationException();

            RefreshExpandedDisplay(); // Bind data to the view, if needed.

            this.largeContentGrid.Visibility = System.Windows.Visibility.Visible;
            SetCurrentStateAndNotify(State.InTransition);

            var largeContentSize = ComputeLargeContentSize();
            UpdateAnimatorTargetSize(SizeAnimator.Expansion, largeContentSize);
            this.expandStoryboard.Begin(this, true);
        }

        private void BeginViewSizeTransition(Size targetSize)
        {
            UpdateAnimatorTargetSize(SizeAnimator.Resizing, targetSize);
            resizingStoryboard.Begin(this, true);
        }

        #endregion

        #region Private Event Handlers

        private void OnPreviewControlLoaded(object sender, RoutedEventArgs e)
        {
            phaseInStoryboard = this.Resources["phaseInStoryboard"] as Storyboard;
            phaseOutStoryboard = this.Resources["phaseOutStoryboard"] as Storyboard;
            expandStoryboard = this.Resources["expandStoryboard"] as Storyboard;
            condenseStoryboard = this.Resources["condenseStoryboard"] as Storyboard;
            resizingStoryboard = this.Resources["resizingStoryboard"] as Storyboard;

            var children = new List<Timeline>();
            children.AddRange(phaseInStoryboard.Children);
            children.AddRange(expandStoryboard.Children);
            children.AddRange(condenseStoryboard.Children);
            children.AddRange(resizingStoryboard.Children);

            this.sizeAnimators = new Dictionary<string, DoubleAnimation>();

            foreach (var child in children)
            {
                if (string.IsNullOrEmpty(child.Name))
                    continue;

                switch (child.Name)
                {
                    case Element.PhaseInWidthAnimator:
                    case Element.PhaseInHeightAnimator:
                    case Element.ExpandWidthAnimator:
                    case Element.ExpandHeightAnimator:
                    case Element.CondenseWidthAnimator:
                    case Element.CondenseHeightAnimator:
                    case Element.GridWidthAnimator:
                    case Element.GridHeightAnimator:
                        sizeAnimators.Add(child.Name, child as DoubleAnimation);
                        break;
                }
            }

            if (this.sizeAnimators.Count != 8)
            {
                var message = "One or more DoubleAnimation timeline not found";
                throw new InvalidOperationException(message);
            }

            // If there was a request queued before this control is loaded, 
            // then process the request as we now have the right width.
            BeginNextTransition();
        }

        private void OnPreviewControlPhasedIn(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Condensed);
            BeginNextTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlPhasedOut(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Hidden);
            BeginNextTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlExpanded(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Expanded);
            smallContentGrid.Visibility = System.Windows.Visibility.Hidden;
            BeginNextTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlCondensed(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Condensed);
            largeContentGrid.Visibility = System.Windows.Visibility.Hidden;
            BeginNextTransition(); // See if there's any more requests.
        }

        #endregion
    }
}
