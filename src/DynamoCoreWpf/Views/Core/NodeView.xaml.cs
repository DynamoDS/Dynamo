using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Prompts;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System.Windows.Threading;

using DynCmd = Dynamo.Models.DynamoModel;

using Dynamo.UI.Controls;
using Dynamo.Nodes;

namespace Dynamo.Controls
{
    public partial class NodeView : IViewModelView<NodeViewModel>
    {
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);
        private NodeViewModel viewModel = null;
        private PreviewControl previewControl = null;
        private const int previewDelay = 1000;

        /// <summary>
        /// If false - hides preview control until it will be explicitly shown.
        /// If true -preview control is shown and hidden on mouse enter/leave events.
        /// </summary>
        private bool previewEnabled = true;

        /// <summary>
        /// Old ZIndex of node. It's set, when mouse leaves node.
        /// </summary>
        private int oldZIndex;

        private bool nodeWasClicked;

        public NodeView TopControl
        {
            get { return topControl; }
        }

        public Grid ContentGrid
        {
            get { return inputGrid; }
        }

        public NodeViewModel ViewModel
        {
            get { return viewModel; }
            private set
            {
                viewModel = value;
                if (viewModel.PreviewPinned)
                {
                    CreatePreview(viewModel);
                }                
            }
        }

        private void NodeView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (viewModel!=null && viewModel.OnMouseLeave != null)
                viewModel.OnMouseLeave();
        }

        internal PreviewControl PreviewControl
        {
            get
            {
                CreatePreview(ViewModel);

                return previewControl;
            }
        }

        private void CreatePreview(NodeViewModel vm)
        {
            if (previewControl == null)
            {
                previewControl = new PreviewControl(vm);
                previewControl.StateChanged += OnPreviewControlStateChanged;
                previewControl.bubbleTools.MouseEnter += OnPreviewControlMouseEnter;
                previewControl.bubbleTools.MouseLeave += OnPreviewControlMouseLeave;
                expansionBay.Children.Add(previewControl);
            }
        }

        /// <summary>
        /// Returns a boolean value of whether this node view already has its PreviewControl field
        /// constructed (not null), in order to avoid calling the PreviewControl constructor
        /// whenever the accessor property is queried.
        /// </summary>
        internal bool HasPreviewControl
        {
            get
            {
                return previewControl != null;
            }
        }

        #region constructors

        public NodeView()
        {
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.PortsDictionary);

            InitializeComponent();

            Loaded += OnNodeViewLoaded;
            Unloaded += OnNodeViewUnloaded;
            inputGrid.Loaded += NodeViewReady;

            nodeBorder.SizeChanged += OnSizeChanged;
            DataContextChanged += OnDataContextChanged;


            Panel.SetZIndex(this, 1);
        }

        private void OnNodeViewUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NodeLogic.DispatchedToUI -= NodeLogic_DispatchedToUI;
            ViewModel.RequestShowNodeHelp -= ViewModel_RequestShowNodeHelp;
            ViewModel.RequestShowNodeRename -= ViewModel_RequestShowNodeRename;
            ViewModel.RequestsSelection -= ViewModel_RequestsSelection;
            ViewModel.NodeLogic.PropertyChanged -= NodeLogic_PropertyChanged;
            ViewModel.NodeModel.ConnectorAdded -= NodeModel_ConnectorAdded;
            MouseLeave -= NodeView_MouseLeave;

            if (previewControl != null)
            {
                previewControl.StateChanged -= OnPreviewControlStateChanged;
                previewControl.MouseEnter -= OnPreviewControlMouseEnter;
                previewControl.MouseLeave -= OnPreviewControlMouseLeave;
                expansionBay.Children.Remove(previewControl);
                previewControl = null;
            }
        }

        #endregion

        /// <summary>
        /// Called when the size of the node changes. Communicates changes down to the view model 
        /// then to the model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            if (ViewModel == null || ViewModel.PreferredSize.HasValue) return;

            var size = new[] { ActualWidth, nodeBorder.ActualHeight };
            if (ViewModel.SetModelSizeCommand.CanExecute(size))
            {
                ViewModel.SetModelSizeCommand.Execute(size);
            }
        }

        /// <summary>
        /// This event handler is called soon as the NodeViewModel is bound to this 
        /// NodeView, which happens way before OnNodeViewLoaded event is sent. 
        /// There is a known bug in WPF 4.0 where DataContext becomes DisconnectedItem 
        /// when actions such as tab switching happens (that is when the View becomes 
        /// disconnected from the underlying ViewModel/Model that it was bound to). So 
        /// it is more reliable for NodeView to cache the NodeViewModel it is bound 
        /// to when it first becomes available, and refer to the cached value at a later
        /// time.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If this is the first time NodeView is bound to the NodeViewModel, 
            // cache the DataContext (i.e. NodeViewModel) locally and start 
            // referecing it from this point onwards. Note that this notification 
            // can be sent as a result of DataContext becoming DisconnectedItem too,
            // but ViewModel should not be updated in that case (hence the null-check).
            // 
            if (null != ViewModel) return;

            ViewModel = e.NewValue as NodeViewModel;
            if (!ViewModel.PreferredSize.HasValue) return;

            var size = ViewModel.PreferredSize.Value;
            nodeBorder.Width = size.Width;
            nodeBorder.Height = size.Height;
            nodeBorder.RenderSize = size;
        }

        private void OnNodeViewLoaded(object sender, RoutedEventArgs e)
        {
            // We no longer cache the DataContext (NodeViewModel) here because 
            // OnNodeViewLoaded gets called at a much later time and we need the 
            // ViewModel to be valid earlier (e.g. OnSizeChanged is called before
            // OnNodeViewLoaded, and it needs ViewModel for size computation).
            // 
            // ViewModel = this.DataContext as NodeViewModel;
            ViewModel.NodeLogic.DispatchedToUI += NodeLogic_DispatchedToUI;
            ViewModel.RequestShowNodeHelp += ViewModel_RequestShowNodeHelp;
            ViewModel.RequestShowNodeRename += ViewModel_RequestShowNodeRename;
            ViewModel.RequestsSelection += ViewModel_RequestsSelection;
            ViewModel.NodeLogic.PropertyChanged += NodeLogic_PropertyChanged;
            ViewModel.NodeModel.ConnectorAdded += NodeModel_ConnectorAdded;
            MouseLeave += NodeView_MouseLeave;
        }

        private void NodeModel_ConnectorAdded(Graph.Connectors.ConnectorModel obj)
        {
            // If the mouse does not leave the node after the connnector is added,
            // try to show the preview bubble without new mouse enter event. 
            if (IsMouseOver)
            {
                Dispatcher.BeginInvoke(new Action(TryShowPreviewBubbles), DispatcherPriority.Loaded);
            }
        }

        void NodeLogic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ArgumentLacing":
                    ViewModel.SetLacingTypeCommand.RaiseCanExecuteChanged();
                    break;

                case "CachedValue":
                    CachedValueChanged();
                    break;

                case "IsSetAsInput":
                    (this.DataContext as NodeViewModel).DynamoViewModel.CurrentSpace.HasUnsavedChanges = true;
                    break;

                case "IsSetAsOutput":
                    (this.DataContext as NodeViewModel).DynamoViewModel.CurrentSpace.HasUnsavedChanges = true;
                    break;
            }
        }

        /// <summary>
        /// Called when the NodeModel's CachedValue property is updated
        /// </summary>
        private void CachedValueChanged()
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                // There is no preview control or the preview control is 
                // currently in transition state (it can come back to handle
                // the new data later on when it is ready).
                // If node is frozen, we shouldn't update cached value.
                // We keep value, that was before freezing. 
                if ((previewControl == null) || ViewModel.IsFrozen)
                {
                    return;
                }

                // Enqueue an update of the preview control once it has completed its 
                // transition
                if (previewControl.IsInTransition)
                {
                    previewControl.RequestForRefresh();
                    return;
                }

                if (previewControl.IsHidden) // The preview control is hidden.
                {
                    previewControl.IsDataBound = false;
                    return;
                }

                previewControl.BindToDataSource();
            }));
        }

        void ViewModel_RequestsSelection(object sender, EventArgs e)
        {
            if (!ViewModel.NodeLogic.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }

                DynamoSelection.Instance.Selection.AddUnique(ViewModel.NodeLogic);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(ViewModel.NodeLogic);
                }
            }
        }

        void ViewModel_RequestShowNodeRename(object sender, NodeDialogEventArgs e)
        {
            if (e.Handled) return;

            e.Handled = true;

            var editWindow = new EditWindow(viewModel.DynamoViewModel, false, true)
            {
                DataContext = ViewModel,
                Title = Dynamo.Wpf.Properties.Resources.EditNodeWindowTitle
            };

            editWindow.Owner = Window.GetWindow(this);

            editWindow.BindToProperty(null, new Binding("Name")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = ViewModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        void ViewModel_RequestShowNodeHelp(object sender, NodeDialogEventArgs e)
        {
            if (e.Handled) return;

            e.Handled = true;

            var helpDialog = new NodeHelpPrompt(e.Model);
            helpDialog.Owner = Window.GetWindow(this);

            helpDialog.Show();

        }

        void NodeLogic_DispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            Dispatcher.Invoke(e.ActionToDispatch);
        }

        private bool nodeViewReadyCalledOnce = false;
        private void NodeViewReady(object sender, RoutedEventArgs e)
        {
            if (nodeViewReadyCalledOnce) return;

            nodeViewReadyCalledOnce = true;
            ViewModel.DynamoViewModel.OnNodeViewReady(this);
        }

        private Dictionary<UIElement, bool> enabledDict
            = new Dictionary<UIElement, bool>();

        internal void DisableInteraction()
        {
            enabledDict.Clear();

            foreach (UIElement e in inputGrid.Children)
            {
                enabledDict[e] = e.IsEnabled;

                e.IsEnabled = false;
            }

            //set the state using the view model's command
            if (ViewModel.SetStateCommand.CanExecute(ElementState.Dead))
                ViewModel.SetStateCommand.Execute(ElementState.Dead);
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            nodeWasClicked = true;
            BringToFront();
        }

        /// <summary>
        /// If Zindex is more then max value of int, it should be set back to 0 to all elements.
        /// </summary>
        private void PrepareZIndex()
        {
            NodeViewModel.StaticZIndex = Configurations.NodeStartZIndex;

            var parent = TemplatedParent as ContentPresenter;
            if (parent == null) return;

            foreach (var child in parent.ChildrenOfType<NodeView>())
            {
                child.ViewModel.ZIndex = Configurations.NodeStartZIndex;
            }
        }

        private void topControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null || Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control) return;

            var view = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            ViewModel.DynamoViewModel.OnRequestReturnFocusToView();
            view.mainGrid.Focus();

            Guid nodeGuid = ViewModel.NodeModel.GUID;
            ViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers.AsDynamoType()));
            if (e.ClickCount == 2)
            {
                if (ViewModel.GotoWorkspaceCommand.CanExecute(null))
                {
                    e.Handled = true;
                    ViewModel.GotoWorkspaceCommand.Execute(null);
                }
            }
        }

        private void topControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Guid nodeGuid = ViewModel.NodeModel.GUID;
            ViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers.AsDynamoType()));
            //Debug.WriteLine("Node right selected.");
            e.Handled = true;
        }

        private void NameBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Debug.WriteLine("Name double clicked!");
                if (ViewModel != null && ViewModel.RenameCommand.CanExecute(null))
                {
                    ViewModel.RenameCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        #region Preview Control Related Event Handlers

        private void OnNodeViewMouseEnter(object sender, MouseEventArgs e)
        {
            // if the node is located under "Hide preview bubbles" menu item and the item is clicked,
            // ViewModel.DynamoViewModel.ShowPreviewBubbles will be updated AFTER node mouse enter event occurs
            // so, wait while ShowPreviewBubbles binding updates value
            Dispatcher.BeginInvoke(new Action(TryShowPreviewBubbles), DispatcherPriority.Loaded);
        }

        private void TryShowPreviewBubbles()
        {
            nodeWasClicked = false;

            // Always set old ZIndex to the last value, even if mouse is not over the node.
            oldZIndex = NodeViewModel.StaticZIndex;

            // There is no need run further.
            if (IsPreviewDisabled()) return;

            if (PreviewControl.IsInTransition) // In transition state, come back later.
                return;

            if (PreviewControl.IsHidden)
            {
                if (!previewControl.IsDataBound)
                    PreviewControl.BindToDataSource();

                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }

            Dispatcher.DelayInvoke(previewDelay, BringToFront);
        }

        private bool IsPreviewDisabled()
        {
            // True if preview bubbles are turned off globally 
            // Or a connector is being created now
            // Or the user is selecting nodes
            // Or preview is disabled for this node
            // Or preview shouldn't be shown for some nodes (e.g. number sliders, watch nodes etc.)
            // Or node is frozen.
            return !ViewModel.DynamoViewModel.ShowPreviewBubbles ||
                ViewModel.WorkspaceViewModel.IsConnecting ||
                ViewModel.WorkspaceViewModel.IsSelecting || !previewEnabled ||
                !ViewModel.IsPreviewInsetVisible || ViewModel.IsFrozen;
        }

        private void OnNodeViewMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.ZIndex = oldZIndex;

            // If mouse in over node/preview control or preview control is pined, we can not hide preview control.
            if (IsMouseOver || PreviewControl.IsMouseOver || PreviewControl.StaysOpen ||
                (Mouse.Captured is DragCanvas && IsMouseInsideNodeOrPreview(e.GetPosition(this)))) return;

            // If it's expanded, then first condense it.
            if (PreviewControl.IsExpanded)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }
            // If it's condensed, then try to hide it.
            if (PreviewControl.IsCondensed && Mouse.Captured == null)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Hidden);
            }
        }

        /// <summary>
        /// This event fires right after preview's state has been changed.
        /// This event is necessary, it handles some preview's manipulations, 
        /// that we can't handle in mouse enter/leave events.
        /// E.g. When mouse leaves preview control, it should be first condesed, after that hidden.
        /// </summary>
        /// <param name="sender">PreviewControl</param>
        /// <param name="e">Event arguments</param>
        private void OnPreviewControlStateChanged(object sender, EventArgs e)
        {
            var preview = sender as PreviewControl;
            // If the preview is in a transition, return directly to avoid another
            // transition
            if (preview == null || preview.IsInTransition || DynCmd.IsTestMode)
            {
                return;
            }

            switch (preview.CurrentState)
            {
                case PreviewControl.State.Hidden:
                    {
                        if (IsMouseOver && previewEnabled)
                        {
                            preview.TransitionToState(PreviewControl.State.Condensed);
                        }
                        break;
                    }
                case PreviewControl.State.Condensed:
                    {
                        if (preview.bubbleTools.IsMouseOver || preview.StaysOpen)
                        {
                            preview.TransitionToState(PreviewControl.State.Expanded);
                        }

                        if (!IsMouseOver)
                        {
                            // If mouse is captured by DragCanvas and mouse is still over node, preview should stay open.
                            if (!(Mouse.Captured is DragCanvas && IsMouseInsideNodeOrPreview(Mouse.GetPosition(this))))
                            {
                                preview.TransitionToState(PreviewControl.State.Hidden);
                            }
                        }
                        break;
                    }
                case PreviewControl.State.Expanded:
                    {
                        if (!preview.bubbleTools.IsMouseOver && !preview.StaysOpen)
                        {
                            preview.TransitionToState(PreviewControl.State.Condensed);
                        }
                        break;
                    }
            };
        }

        /// <summary>
        /// Sets ZIndex of node the maximum value.
        /// </summary>
        private void BringToFront()
        {
            if (!IsMouseOver && !PreviewControl.IsMouseOver && !DynCmd.IsTestMode) return;

            if (NodeViewModel.StaticZIndex == int.MaxValue)
            {
                PrepareZIndex();
            }

            var index = ++NodeViewModel.StaticZIndex;

            // increment all Notes to ensure that they are always above any Node
            NoteViewModel.StaticZIndex = index + 1;

            foreach (var note in ViewModel.WorkspaceViewModel.Notes)
            {
                note.ZIndex = NoteViewModel.StaticZIndex;
            }

            oldZIndex = nodeWasClicked ? index : ViewModel.ZIndex;
            ViewModel.ZIndex = index;
        }

        private void OnPreviewControlMouseEnter(object sender, MouseEventArgs e)
        {
            if (PreviewControl.IsCondensed)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Expanded);
            }
        }

        private void OnPreviewControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (!PreviewControl.StaysOpen)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }
        }

        private void OnNodeViewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == null) return;

            bool isInside = IsMouseInsideNodeOrPreview(e.GetPosition(this));

            if (!isInside && PreviewControl != null && previewControl.IsCondensed)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Hidden);
            }
        }

        /// <summary>
        /// When Mouse is captured, all mouse events are handled by element, that captured it.
        /// So we can't use MouseLeave/MouseEnter events.
        /// In this case, when we want to ensure, that mouse really left node, we use HitTest.
        /// </summary>
        /// <param name="mousePosition">Currect position of mouse</param>
        /// <returns>bool</returns>
        private bool IsMouseInsideNodeOrPreview(Point mousePosition)
        {
            bool isInside = false;
            VisualTreeHelper.HitTest(
                this,
                d =>
                {
                    if (d == this)
                    {
                        isInside = true;
                    }

                    return HitTestFilterBehavior.Stop;
                },
                ht => HitTestResultBehavior.Stop,
                new PointHitTestParameters(mousePosition));
            return isInside;
        }

        /// <summary>
        /// Enables/disables preview control. 
        /// </summary>
        internal void TogglePreviewControlAllowance()
        {
            previewEnabled = !previewEnabled;

            if (previewEnabled == false && !PreviewControl.StaysOpen)
            {
                if (PreviewControl.IsExpanded)
                {
                    PreviewControl.TransitionToState(PreviewControl.State.Condensed);
                    PreviewControl.TransitionToState(PreviewControl.State.Hidden);
                } else if (PreviewControl.IsCondensed)
                {
                    PreviewControl.TransitionToState(PreviewControl.State.Hidden);
                }
            }
        }

        #endregion

    }
}
