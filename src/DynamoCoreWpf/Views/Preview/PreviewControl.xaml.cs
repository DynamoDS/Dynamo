using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using ProtoCore.Mirror;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Dynamo.Configuration;
using Dynamo.Extensions;
using Dynamo.Models;
using System.Windows.Media;
using Dynamo.Graph.Nodes;

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
            Hidden, Condensed, Expanded, InTransition, PreTransition
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

        /// <summary>
        /// TextBlock, which is shown during graph calculation.
        /// TODO: this should be replaced with some animated text. 
        /// http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-9584
        /// </summary>
        private readonly TextBlock busyTextBlock = new TextBlock
        {
            Text = Configurations.BusyString,
            FontFamily = new FontFamily("Consolas"),
            Opacity = 0.5,
            Margin = new System.Windows.Thickness(12, 6, 0, 10)
        };

        private readonly IScheduler scheduler;
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

        // Queued data refresh
        private bool queuedRefresh;
        private MirrorData queuedMirrorData;

        private static readonly DependencyProperty StaysOpenProperty =
            DependencyProperty.Register("StaysOpen", typeof(bool), typeof(PreviewControl));

        /// <summary>
        ///     Indicates whether preview should stay open, when mouse leaves control.
        /// </summary>
        internal bool StaysOpen
        {
            get
            {
                return (bool)GetValue(StaysOpenProperty);
            }
            private set
            {
                SetValue(StaysOpenProperty, value);
            }
        }

        #endregion

        #region Public Class Operational Methods

        public PreviewControl(NodeViewModel nodeViewModel)
        {
            this.scheduler = nodeViewModel.DynamoViewModel.Model.Scheduler;
            this.nodeViewModel = nodeViewModel;
            InitializeComponent();
            Loaded += OnPreviewControlLoaded;
            StaysOpen = false;
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
            // If Dynamo is in test mode, preview control is not loaded.
            // You should RaiseEvent inside test manually in order to test this control.
            if (this.IsLoaded || DynamoModel.IsTestMode)
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
                RefreshCondensedDisplay(delegate { BeginViewSizeTransition(ComputeSmallContentSize()); });
            }
            else if (this.IsExpanded)
            {
                RefreshExpandedDisplay(delegate { BeginViewSizeTransition(ComputeLargeContentSize()); });
            }
        }

        /// <summary>
        /// It is possible for a run to complete while the preview display is
        /// in transition.  In these situations, we can store the MirrorData and
        /// set a flag to refresh the display.
        /// </summary>
        internal void EnqueueBindToDataSource(MirrorData mirrorData)
        {
            this.queuedMirrorData = mirrorData;
            this.queuedRefresh = true;
        }

        #endregion

        #region Public Class Properties

        internal bool IsDataBound { get { return mirrorData != null; } }
        internal bool IsHidden { get { return currentState == State.Hidden; } }
        internal bool IsCondensed { get { return currentState == State.Condensed; } }
        internal bool IsExpanded { get { return currentState == State.Expanded; } }
        internal bool IsInTransition
        {
            get
            {
                return currentState == State.InTransition ||
                        currentState == State.PreTransition;
            }
        }

        internal State CurrentState { get { return currentState; } }

        #endregion

        #region Private Class Methods - Generic Helpers

        private void BeginNextTransition()
        {
            // A run completed while in transition, we must refresh
            if (queuedRefresh)
            {
                queuedRefresh = false;
                BindToDataSource(queuedMirrorData);
                this.queuedMirrorData = null;
                return;
            }

            if (this.IsInTransition || queuedRequest.Count <= 0)
                return; // Nothing else to do.

            State requestedState = queuedRequest.Dequeue();
            while (requestedState == this.currentState)
            {
                if (queuedRequest.Count <= 0)
                    return; // There's no more request for now.
                requestedState = queuedRequest.Dequeue();
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
            if (largeContentGrid.Children.Count <= 0)
                return; // No view to reset, return now.

            var watchTree = largeContentGrid.Children[0] as WatchTree;

            // Watch tree can be null, when 0-element of largeContentGrid busyTextBlock.
            if (watchTree == null) return;

            var rootDataContext = watchTree.DataContext as WatchViewModel;

            // Unbind the view from data context, then clear the data context.
            BindingOperations.ClearAllBindings(watchTree.treeView1);
            rootDataContext.Children.Clear();
        }

        /// <summary>
        /// Add a new task to the scheduler to be executed and after that run a
        /// follow-up task.
        /// </summary>
        /// <param name="a">The task to be run in the scheduler</param>
        /// <param name="h">The follow up handler to be executed</param>
        private void RunOnSchedulerSync(Action a, AsyncTaskCompletedHandler h)
        {
            var task = new DelegateBasedAsyncTask(scheduler, a);
            task.ThenPost(h, DispatcherSynchronizationContext.Current);
            scheduler.ScheduleForExecution(task);
        }

        /// <summary>
        /// Obtain the condensed preview values for this control.  Must not be called from 
        /// Scheduler thread or this could cause a live-lock.
        /// </summary>
        /// <param name="refreshDisplay">The action to refresh the UI</param>
        private void RefreshCondensedDisplay(Action refreshDisplay)
        {
            // The preview control will not have its content refreshed unless 
            // the content is null. In order to perform real refresh, new data 
            // source needs to be rebound to the preview control by calling 
            // BindToDataSource method.
            // 
            if (cachedSmallContent != null)
            {
                // If there are cached contents, simply update the UI and return
                if (refreshDisplay != null)
                {
                    refreshDisplay();
                }
                return;
            }

            string newContent = "null";

            if (mirrorData is ProtoCore.MirrorDataInProgress
                // When CBN is created, its cached value is set to MirrorDataInProgress.
                // And it's not updated after update graph task is complited.
                && !((nodeViewModel.NodeLogic is CodeBlockNodeModel) && (nodeViewModel.NodeLogic as CodeBlockNodeModel).Code == String.Empty))
            {
                var smallContentView = smallContentGrid.Children[0] as TextBlock;
                smallContentView.Text = Configurations.BusyString;

                if (refreshDisplay != null)
                {
                    refreshDisplay();
                }

                return;
            }

            RunOnSchedulerSync(
                () =>
                {
                    if (mirrorData != null)
                    {
                        if (mirrorData.IsCollection)
                        {
                            // TODO(Ben): Can we display details of the array and 
                            // probably display the first element of the array (even 
                            // when it is multi-dimensional array)?
                            newContent = Wpf.Properties.Resources.PreviewListLabel;
                        }
                        else if (mirrorData.Data == null && !mirrorData.IsNull && mirrorData.Class != null)
                        {
                            newContent = mirrorData.Class.ClassName;
                        }
                        else if (mirrorData.Data is Enum)
                        {
                            newContent = ((Enum)mirrorData.Data).GetDescription();
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(mirrorData.StringData))
                            {
                                newContent = String.Empty;
                                return;
                            }

                            int index = mirrorData.StringData.IndexOf('(');
                            newContent = index != -1 ? mirrorData.StringData.Substring(0, index) : mirrorData.StringData;
                        }
                    }
                },
                (m) =>
                {
                    cachedSmallContent = newContent;
                    var smallContentView = smallContentGrid.Children[0] as TextBlock;
                    smallContentView.Text = cachedSmallContent; // Update displayed text.
                    if (refreshDisplay != null)
                    {
                        refreshDisplay();
                    }
                }
            );
        }

        /// <summary>
        ///     Obtain the expanded preview values for this control.  Must not be called from 
        ///     Scheduler thread or this could cause a live-lock.
        /// </summary>
        /// 
        private void RefreshExpandedDisplay(Action refreshDisplay)
        {
            // The preview control will not have its content refreshed unless 
            // the content is null. In order to perform real refresh, new data 
            // source needs to be rebound to the preview control by calling 
            // BindToDataSource method.
            // 
            if (this.cachedLargeContent != null)
            {
                // If there are cached contents, simply update the UI and return
                if (refreshDisplay != null)
                {
                    refreshDisplay();
                }
                return;
            }

            WatchViewModel newViewModel = null;

            if (mirrorData is ProtoCore.MirrorDataInProgress
                // When CBN is created, its cached value is set to MirrorDataInProgress.
                // And it's not updated after update graph task is complited.
                && !((nodeViewModel.NodeLogic is CodeBlockNodeModel) && (nodeViewModel.NodeLogic as CodeBlockNodeModel).Code == String.Empty))
            {
                if (!largeContentGrid.Children.Contains(busyTextBlock))
                {
                    largeContentGrid.Children.Add(busyTextBlock);
                }

                if (refreshDisplay != null)
                {
                    refreshDisplay();
                }

                return;
            }

            RunOnSchedulerSync(
                () =>
                {
                    newViewModel = nodeViewModel.DynamoViewModel.WatchHandler.GenerateWatchViewModelForData(
                        mirrorData, null, nodeViewModel.NodeModel.AstIdentifierForPreview.Name, false);
                },
                (m) =>
                {
                    largeContentGrid.Children.Remove(busyTextBlock);

                    if (largeContentGrid.Children.Count == 0)
                    {
                        var tree = new WatchTree
                        {
                            DataContext = new WatchViewModel(nodeViewModel.DynamoViewModel.BackgroundPreviewViewModel.AddLabelForPath)
                        };
                        tree.treeView1.ItemContainerGenerator.StatusChanged += WatchContainer_StatusChanged;

                        largeContentGrid.Children.Add(tree);
                    }

                    var watchTree = largeContentGrid.Children[0] as WatchTree;
                    var rootDataContext = watchTree.DataContext as WatchViewModel;


                    cachedLargeContent = newViewModel;

                    rootDataContext.IsOneRowContent = cachedLargeContent.Children.Count == 0;
                    rootDataContext.Children.Clear();
                    rootDataContext.Children.Add(cachedLargeContent);

                    watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty,
                        new Binding("Children")
                        {
                            Mode = BindingMode.TwoWay,
                            Source = rootDataContext
                        });
                    if (refreshDisplay != null)
                    {
                        refreshDisplay();
                    }
                }
            );
        }

        /// <summary>
        /// It's used to apply Collapsed and Expanded events for TreeViewItems.
        /// </summary>
        /// <param name="sender">TreeView</param>
        private void WatchContainer_StatusChanged(object sender, EventArgs e)
        {
            var generator = sender as ItemContainerGenerator;
            if (generator == null || generator.Status != GeneratorStatus.ContainersGenerated) return;

            int i = 0;
            while (true)
            {
                var container = generator.ContainerFromIndex(i);
                if (container == null)
                    break;

                var tvi = container as TreeViewItem;
                if (tvi != null)
                {
                    tvi.Collapsed += ComputeWatchContentSize;
                    tvi.Expanded += ComputeWatchContentSize;
                }

                i++;
            }
        }

        /// <summary>
        /// When item in WatchTree is collapsed or expanded, we should compute new large content size.
        /// </summary>
        private void ComputeWatchContentSize(object sender, RoutedEventArgs e)
        {
            if (!IsExpanded) return;

            SetCurrentStateAndNotify(State.PreTransition);

            // Used delay invoke, because TreeView hasn't changed its'appearance with usual Dispatcher call.
            Dispatcher.DelayInvoke(50, () => RefreshExpandedDisplay(() =>
            {
                largeContentGrid.Visibility = Visibility.Visible;
                bubbleTools.Visibility = Visibility.Visible;

                // The real transition starts
                SetCurrentStateAndNotify(State.InTransition);
                var largeContentSize = ComputeLargeContentSize();
                UpdateAnimatorTargetSize(SizeAnimator.Expansion, largeContentSize);

                // If it's test mode - skip storyboard.
                if (!DynamoModel.IsTestMode)
                {
                    expandStoryboard.Begin(this, true);
                }
                else
                {
                    largeContentGrid.Opacity = 1.0;
                    smallContentGrid.Opacity = 0.0;
                    centralizedGrid.Width = largeContentSize.Width;
                    centralizedGrid.Height = largeContentSize.Height;
                    OnPreviewControlExpanded(null, null);
                }
            }));
        }

        private Size ComputeSmallContentSize()
        {
            var maxSize = new Size()
            {
                Width = Configurations.MaxCondensedPreviewWidth,
                Height = Configurations.MaxCondensedPreviewHeight
            };

            smallContentGrid.Measure(maxSize);
            Size smallContentGridSize = smallContentGrid.DesiredSize;

            // Don't make it smaller then min width.
            smallContentGridSize.Width = smallContentGridSize.Width < smallContentGrid.MinWidth
                ? smallContentGrid.MinWidth
                : smallContentGridSize.Width;

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

            Size largeContentGridSize = largeContentGrid.DesiredSize;

            // Don't make it smaller then min width.
            largeContentGridSize.Width = largeContentGridSize.Width < largeContentGrid.MinWidth
                ? largeContentGrid.MinWidth
                : largeContentGridSize.Width;

            // Add padding since we are sizing the centralizedGrid.
            return ContentToControlSize(largeContentGridSize);
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

            // To prevent another transition from being started and
            // indicate a new transition is about to be started
            SetCurrentStateAndNotify(State.PreTransition);

            RefreshCondensedDisplay(() =>
                {
                    // Update size before fading in to view.
                    var smallContentSize = ComputeSmallContentSize();
                    UpdateAnimatorTargetSize(SizeAnimator.PhaseIn, smallContentSize);

                    centralizedGrid.Opacity = 0.0;
                    centralizedGrid.Visibility = Visibility.Visible;
                    smallContentGrid.Visibility = Visibility.Visible;

                    // The real transition starts
                    SetCurrentStateAndNotify(State.InTransition);

                    // If it's test mode - skip storyboard.
                    if (!DynamoModel.IsTestMode)
                    {
                        phaseInStoryboard.Begin(this, true);
                    }
                    else
                    {
                        centralizedGrid.Opacity = 1.0;
                        centralizedGrid.Width = smallContentSize.Width;
                        centralizedGrid.Height = smallContentSize.Height;
                        OnPreviewControlPhasedIn(null, null);
                    }
                }
            );
        }

        private void BeginFadeOutTransition()
        {
            if (this.IsCondensed == false)
                throw new InvalidOperationException();

            bubbleTools.Visibility = Visibility.Collapsed;

            SetCurrentStateAndNotify(State.InTransition);

            // If it's test mode - skip storyboard.
            if (!DynamoModel.IsTestMode)
            {
                phaseOutStoryboard.Begin(this, true);
            }
            else
            {
                centralizedGrid.Opacity = 0;
                thisPreviewControl.Visibility = Visibility.Collapsed;
                OnPreviewControlPhasedOut(null, null);
            }
        }

        private void BeginCondenseTransition()
        {
            if (this.IsExpanded == false)
                throw new InvalidOperationException();

            // To prevent another transition from being started and
            // indicate a new transition is about to be started
            SetCurrentStateAndNotify(State.PreTransition);

            RefreshCondensedDisplay(() =>
                {
                    smallContentGrid.Visibility = Visibility.Visible;
                    bubbleTools.Visibility = Visibility.Collapsed;

                    // The real transition starts
                    SetCurrentStateAndNotify(State.InTransition);
                    var smallContentSize = ComputeSmallContentSize();
                    UpdateAnimatorTargetSize(SizeAnimator.Condensation, smallContentSize);                    

                    // If it's test mode - skip storyboard.
                    if (!DynamoModel.IsTestMode)
                    {
                        condenseStoryboard.Begin(this, true);
                    }
                    else
                    {
                        largeContentGrid.Opacity = 0.0;
                        smallContentGrid.Opacity = 1.0;
                        centralizedGrid.Width = smallContentSize.Width;
                        centralizedGrid.Height = smallContentSize.Height;
                        OnPreviewControlCondensed(null, null);
                    }
                }
            );
        }

        private void BeginExpandTransition()
        {
            if (this.IsCondensed == false)
                throw new InvalidOperationException();

            // To prevent another transition from being started and
            // indicate a new transition is about to be started
            SetCurrentStateAndNotify(State.PreTransition);

            RefreshExpandedDisplay(() =>
                {
                    largeContentGrid.Visibility = Visibility.Visible;
                    bubbleTools.Visibility = Visibility.Visible;

                    // The real transition starts
                    SetCurrentStateAndNotify(State.InTransition);
                    var largeContentSize = ComputeLargeContentSize();
                    UpdateAnimatorTargetSize(SizeAnimator.Expansion, largeContentSize);                    

                    // If it's test mode - skip storyboard.
                    if (!DynamoModel.IsTestMode)
                    {
                        expandStoryboard.Begin(this, true);
                    }
                    else
                    {
                        largeContentGrid.Opacity = 1.0;
                        smallContentGrid.Opacity = 0.0;
                        centralizedGrid.Width = largeContentSize.Width;
                        centralizedGrid.Height = largeContentSize.Height;
                        OnPreviewControlExpanded(null, null);
                    }
                }
            );
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
            BeginNextTransition(); // See if there's any more requests.
        }

        private void OnPreviewControlCondensed(object sender, EventArgs e)
        {
            SetCurrentStateAndNotify(State.Condensed);
            BeginNextTransition(); // See if there's any more requests.
        }

        private void OnMapPinMouseClick(object sender, MouseButtonEventArgs e)
        {
            StaysOpen = !StaysOpen;
        }

        #endregion

    }
}
