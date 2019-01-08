using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Event to be sent when state of PreviewControl is changed
    /// </summary>
    /// <param name="sender"><cref name="PreviewControl"/> instance whose state has changed</param>
    /// <param name="e">The event data</param>
    public delegate void StateChangedEventHandler(object sender, EventArgs e);

    public partial class PreviewControl
    {
        #region Class Data Members and Configurations

        public enum State
        {
            Hidden, Condensed, Expanded, InTransition, PreTransition
        }

        private readonly IScheduler scheduler;
        private readonly NodeViewModel nodeViewModel;

        private State currentState = State.Hidden;
        private readonly Queue<State> queuedRequest = new Queue<State>();

        // Data source and display.
        private CompactBubbleViewModel cachedSmallContent;
        private WatchViewModel cachedLargeContent;

        public event StateChangedEventHandler StateChanged;

        // Queued data refresh
        private bool queuedRefresh;

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
            Loaded += PreviewControl_Loaded;
            SizeChanged += UpdateMargin;
            Unloaded += PreviewControl_Unloaded;
            if (this.nodeViewModel.PreviewPinned)
            {
                StaysOpen = true;
                BindToDataSource();
                TransitionToState(State.Condensed);
                TransitionToState(State.Expanded);
            }
            else
            {
                StaysOpen = false;
            }
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
            if (IsLoaded || DynamoModel.IsTestMode)
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
        internal void BindToDataSource()
        {
            // First detach the bound data from its view.
            ResetContentViews();
            
            // Reset expanded content.
            cachedLargeContent = null;
            // Reset condensed content.
            cachedSmallContent = null;

            // If at the time of data binding the preview control is within the 
            // following states, then its contents need to be updated immediately.
            if (IsCondensed)
            {
                RefreshCondensedDisplay(null);
            }
            else if (IsExpanded)
            {
                RefreshExpandedDisplay(RefreshExpandedDisplayAction);
            }

            IsDataBound = true;
        }

        /// <summary>
        /// Hides preview bubble if it is not pinned
        /// </summary>
        internal void HidePreviewBubble()
        {
            if (StaysOpen) return;

            if (IsExpanded)
            {
                TransitionToState(State.Condensed);
            }

            if (IsCondensed)
            {
                TransitionToState(State.Hidden);
            }
        }

        /// <summary>
        /// It is possible for a run to complete while the preview display is
        /// in transition.  In these situations, we can store the MirrorData and
        /// set a flag to refresh the display.
        /// </summary>
        internal void RequestForRefresh()
        {
            queuedRefresh = true;
        }

        #endregion

        #region Public Class Properties

        internal bool IsDataBound { get; set;}
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
                BindToDataSource();
                return;
            }

            // Nothing else to do.
            if (IsInTransition || queuedRequest.Count <= 0) return; 

            var requestedState = queuedRequest.Dequeue();
            while (requestedState == currentState)
            {
                // There's no more request for now.
                if (queuedRequest.Count <= 0) return;

                requestedState = queuedRequest.Dequeue();
            }

            if (requestedState == State.Hidden)
            {
                BeginFadeOutTransition();
            }
            else if (requestedState == State.Condensed)
            {
                if (IsHidden)
                {
                    BeginFadeInTransition();
                }
                else if (IsExpanded)
                {
                    BeginCondenseTransition();
                }
            }
            else if (requestedState == State.Expanded)
            {
                BeginExpandTransition();
            }
        }

        private void SetCurrentStateAndNotify(State newState)
        {
            currentState = newState;

            if (StateChanged != null)
            {
                StateChanged(this, EventArgs.Empty);
            }

            UpdateMargin(this, null);
        }

        private void ResetContentViews()
        {            
            if (smallContentGrid.Children.Count <= 0) return;

            var smallContentView = smallContentGrid.Children[0] as PreviewCompactView;
            BindingOperations.ClearAllBindings(smallContentView);

            // No view to reset, return now.
            if (largeContentGrid.Children.Count <= 0) return;

            var watchTree = largeContentGrid.Children[0] as WatchTree;
            var rootDataContext = watchTree.DataContext as WatchViewModel;

            // Unbind the view from data context, then clear the data context.
            BindingOperations.ClearAllBindings(watchTree.treeView1);
            BindingOperations.ClearAllBindings(watchTree.ListLevelsDisplay);
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
            task.ThenPost(h, SynchronizationContext.Current);
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

            CompactBubbleViewModel newContent = null;

            RunOnSchedulerSync(
                () =>
                {
                    var mirrorData = nodeViewModel.NodeModel.CachedValue;
                    newContent = CompactBubbleHandler.Process(mirrorData);
                },
                (m) =>
                {
                    cachedSmallContent = newContent;
                    var smallContentView = smallContentGrid.Children[0] as PreviewCompactView;
                    smallContentView.DataContext = cachedSmallContent;

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

            RunOnSchedulerSync(
                () =>
                {
                    var preferredDictionaryOrdering = 
                    nodeViewModel.NodeModel.OutPorts.Select(p => p.Name).Where(n => !String.IsNullOrEmpty(n));
                    newViewModel = nodeViewModel.DynamoViewModel.WatchHandler.GenerateWatchViewModelForData(
                        nodeViewModel.NodeModel.CachedValue, preferredDictionaryOrdering,
                        null, nodeViewModel.NodeModel.AstIdentifierForPreview.Name, false);

                },
                (m) =>
                {
                    //If newViewModel is not set then no point continuing.
                    if (newViewModel == null) return;

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
                    if (watchTree != null)
                    {
                        var rootDataContext = watchTree.DataContext as WatchViewModel;

                        cachedLargeContent = newViewModel;

                        if (rootDataContext != null)
                        {
                            rootDataContext.IsOneRowContent = cachedLargeContent.Children.Count == 0;
                            rootDataContext.Children.Clear();
                            rootDataContext.Children.Add(cachedLargeContent);
                            rootDataContext.CountNumberOfItems(); //count the total number of items in the list
                            if (!rootDataContext.IsOneRowContent)
                            {
                                rootDataContext.CountLevels();
                                watchTree.listLevelsView.ItemsSource = rootDataContext.Levels; // add listLevelList to the ItemsSource of listlevelsView in WatchTree
                                rootDataContext.Children[0].IsTopLevel = true;
                            }

                            watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty,
                                new Binding("Children")
                                {
                                    Mode = BindingMode.TwoWay,
                                    Source = rootDataContext
                                });

                        }
                    }
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
            Dispatcher.DelayInvoke(50, () => RefreshExpandedDisplay(RefreshExpandedDisplayAction));
        }

        private Size ComputeSmallContentSize()
        {
            var maxSize = new Size
            {
                Width = Configurations.MaxCondensedPreviewWidth,
                Height = Configurations.MaxCondensedPreviewHeight
            };

            smallContentGrid.Measure(maxSize);
            var smallContentGridSize = smallContentGrid.DesiredSize;

            // Don't make it smaller then min width.
            smallContentGridSize.Width = smallContentGridSize.Width < smallContentGrid.MinWidth
                ? smallContentGrid.MinWidth
                : smallContentGridSize.Width;

            // Add padding since we are sizing the centralizedGrid.
            return ContentToControlSize(smallContentGridSize);
        }

        private Size ComputeLargeContentSize()
        {
            largeContentGrid.UpdateLayout();
            largeContentGrid.Measure(new Size
            {
                Width = Configurations.MaxExpandedPreviewWidth,
                Height = Configurations.MaxExpandedPreviewHeight
            });

            var largeContentGridSize = largeContentGrid.DesiredSize;

            // Don't make it smaller then min width.
            largeContentGridSize.Width = largeContentGridSize.Width < largeContentGrid.MinWidth
                ? largeContentGrid.MinWidth
                : largeContentGridSize.Width;

            // Add two times width of scroll bar (5) and right margin(5), refer to infoBubbleView.xaml
            largeContentGridSize.Width += 20;

            // Add padding since we are sizing the centralizedGrid.
            return ContentToControlSize(largeContentGridSize);
        }

        private static Size ContentToControlSize(Size size)
        {
            if (size.Width < Configurations.DefCondensedContentWidth)
                size.Width = Configurations.DefCondensedContentWidth;
            if (size.Height < Configurations.DefCondensedContentHeight)
                size.Height = Configurations.DefCondensedContentHeight;

            return size;
        }

        #endregion

        #region Private Class Methods - Transition Helpers

        private async void BeginFadeInTransition()
        {
            if (!IsHidden)
            {
                return;
            }

            // do not use delay in tests
            if (DynamoModel.IsTestMode)
            {
                ProcessFadeIn();
                return;
            }

            // show preview bubble only if mouse stays over node longer than delay time
            // so that during quick mouse moving preview bubbles won't be shown
            var delayTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            var onMouseLeave = new Action(delayTimer.Stop);
            nodeViewModel.OnMouseLeave += onMouseLeave;
            delayTimer.Tick += (obj, e) =>
            {
                Dispatcher.Invoke(ProcessFadeIn);
                nodeViewModel.OnMouseLeave -= onMouseLeave;
                delayTimer.Stop();
            };

            await Task.Run(() => delayTimer.Start());
        }

        private void ProcessFadeIn()
        {
            // To prevent another transition from being started and
            // indicate a new transition is about to be started
            SetCurrentStateAndNotify(State.PreTransition);

            RefreshCondensedDisplay(() =>
            {
                // Update size before fading in to view.
                var smallContentSize = ComputeSmallContentSize();

                centralizedGrid.Visibility = Visibility.Visible;
                smallContentGrid.Visibility = Visibility.Visible;
                largeContentGrid.Visibility = Visibility.Collapsed;
                thisPreviewControl.Visibility = Visibility.Visible;

                // The real transition starts
                SetCurrentStateAndNotify(State.InTransition);

                centralizedGrid.Width = smallContentSize.Width;
                centralizedGrid.Height = smallContentSize.Height;
                SetCurrentStateAndNotify(State.Condensed);
                BeginNextTransition(); // See if there's any more requests.
            });
        }

        private void BeginFadeOutTransition()
        {
            if (StaysOpen) return;
            if (!IsCondensed) return;

            SetCurrentStateAndNotify(State.InTransition);

            thisPreviewControl.Visibility = Visibility.Collapsed;
            SetCurrentStateAndNotify(State.Hidden);
            BeginNextTransition(); // See if there's any more requests.
        }

        private void BeginCondenseTransition()
        {
            if (!IsExpanded) return;

            // To prevent another transition from being started and
            // indicate a new transition is about to be started
            SetCurrentStateAndNotify(State.PreTransition);

            RefreshCondensedDisplay(() =>
            {
                smallContentGrid.Visibility = Visibility.Visible;
                largeContentGrid.Visibility = Visibility.Collapsed;

                // The real transition starts
                SetCurrentStateAndNotify(State.InTransition);
                var smallContentSize = ComputeSmallContentSize();

                centralizedGrid.Width = smallContentSize.Width;
                centralizedGrid.Height = smallContentSize.Height;
                SetCurrentStateAndNotify(State.Condensed);
                BeginNextTransition(); // See if there's any more requests.
            });
        }

        private void BeginExpandTransition()
        {
            if (!IsCondensed)
            {
                return;
            }

            // To prevent another transition from being started and
            // indicate a new transition is about to be started
            SetCurrentStateAndNotify(State.PreTransition);

            RefreshExpandedDisplay(RefreshExpandedDisplayAction);
        }

        private void RefreshExpandedDisplayAction()
        {
            smallContentGrid.Visibility = Visibility.Collapsed;
            largeContentGrid.Visibility = Visibility.Visible;

            // The real transition starts
            SetCurrentStateAndNotify(State.InTransition);
            try
            {
                var largeContentSize = ComputeLargeContentSize();
                centralizedGrid.Width = largeContentSize.Width;
                centralizedGrid.Height = largeContentSize.Height;
            }
            catch (DivideByZeroException ex)
            {
                //Log this exception as non-fatal exception.
                Logging.Analytics.TrackException(ex, false);

                //MAGN-10528, thrown from UpdateLayout call while measuring the size.
                //Most likely it doesn't have any content, so use condensed content size.
                centralizedGrid.Width = Configurations.DefCondensedContentWidth;
                centralizedGrid.Height = Configurations.DefCondensedContentHeight;
            }

            SetCurrentStateAndNotify(State.Expanded);
            BeginNextTransition(); // See if there's any more requests.
        }

        #endregion

        #region Private Event Handlers

        private void PreviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            BeginNextTransition();
            Loaded -= PreviewControl_Loaded;
        }

        private void OnMapPinMouseClick(object sender, MouseButtonEventArgs e)
        {
            StaysOpen = !StaysOpen;
            nodeViewModel.PreviewPinned = StaysOpen;

            // Select node.
            nodeViewModel.DynamoViewModel.ExecuteCommand(
               new DynamoModel.SelectModelCommand(nodeViewModel.NodeModel.GUID, Keyboard.Modifiers.AsDynamoType()));

            // Handle event here is order not to bubble it to parent control like DragCanvas.
            e.Handled = true;
        }

        private void PreviewControl_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= UpdateMargin;
            Unloaded -= PreviewControl_Unloaded;
        }

        private void UpdateMargin(object sender, SizeChangedEventArgs e)
        {
            // Compute margin for the preview bubble, 
            // so that it is centered relatively to the node
            //       ------
            //       |node|
            //       ------
            //    ------------
            //    |  bubble  |
            //    ------------
            // and margin for pin icon, so that it is under node's right bottom 
            // independently from preview bubble width     
            var nodeWidth = smallContentGrid.MinWidth;
            var previewWidth = Math.Max(centralizedGrid.ActualWidth, nodeWidth);
            var margin = (previewWidth - nodeWidth) / 2;
            Margin = new System.Windows.Thickness { Left = -margin };
            bubbleTools.Margin = new System.Windows.Thickness
            {
                Right = margin
            };
        }

        private void PreviewControl_MouseEnter(object sender, MouseEventArgs e)
        {
            bubbleTools.Visibility = Visibility.Visible;
        }

        private void PreviewControl_MouseLeave(object sender, MouseEventArgs e)
        {
            bubbleTools.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
