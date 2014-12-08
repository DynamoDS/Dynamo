using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Dynamo.Models;
using Dynamo.Prompts;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynCmd = Dynamo.Models.DynamoModel;

using Dynamo.UI.Controls;

namespace Dynamo.Controls
{
    public partial class NodeView : IViewModelView<NodeViewModel>
    {
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);       
        private NodeViewModel viewModel = null;
        private PreviewControl previewControl = null;

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
            private set { viewModel = value; }
        }

        private PreviewControl PreviewControl
        {
            get
            {
                if (this.previewControl == null)
                {
                    this.previewControl = new PreviewControl(this.ViewModel);
                    this.previewControl.StateChanged += OnPreviewControlStateChanged;
                    this.expansionBay.Children.Add(this.previewControl);
                }

                return this.previewControl;
            }
        }

        #region constructors

        public NodeView()
        {
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.PortsDictionary);

            InitializeComponent();

            Loaded += OnNodeViewLoaded;
            Unloaded += OnNodeViewUnloaded;
            inputGrid.Loaded += NodeViewReady;

            this.nodeBorder.SizeChanged += OnSizeChanged;
            this.DataContextChanged += OnDataContextChanged;

            Canvas.SetZIndex(this, 1);

        }

        private void OnNodeViewUnloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Node view unloaded.");

            ViewModel.NodeLogic.DispatchedToUI -= NodeLogic_DispatchedToUI;
            ViewModel.RequestShowNodeHelp -= ViewModel_RequestShowNodeHelp;
            ViewModel.RequestShowNodeRename -= ViewModel_RequestShowNodeRename;
            ViewModel.RequestsSelection -= ViewModel_RequestsSelection;
            ViewModel.NodeLogic.PropertyChanged -= NodeLogic_PropertyChanged;
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
            if (ViewModel != null)
            {
                var size = new double[] { ActualWidth, nodeBorder.ActualHeight };
                if (ViewModel.SetModelSizeCommand.CanExecute(size))
                {
                    //Debug.WriteLine(string.Format("Updating {2} node size {0}:{1}", size[0], size[1], ViewModel.NodeLogic.GetType().ToString()));
                    ViewModel.SetModelSizeCommand.Execute(size);
                }
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
            if (null == this.ViewModel)
                this.ViewModel = e.NewValue as NodeViewModel;
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
           
        }
      
        void NodeLogic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ArgumentLacing":
                    ViewModel.SetLacingTypeCommand.RaiseCanExecuteChanged();
                    break;

                case "IsUpdated":
                    HandleCacheValueUpdated();
                    break;
            }
        }

        /// <summary>
        /// Whenever property "NodeModel.IsUpdated" is set to true, this method 
        /// is invoked. It will result in preview control updated, if the control 
        /// is currently visible. Otherwise this call will be ignored.
        /// </summary>
        /// 
        private void HandleCacheValueUpdated()
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                // There is no preview control or the preview control is 
                // currently in transition state (it can come back to handle
                // the new data later on when it is ready).
                if ((previewControl == null) || previewControl.IsInTransition)
                    return;

                if (previewControl.IsHidden) // The preview control is hidden.
                {
                    // Invalidate the previously bound data, if any.
                    if (previewControl.IsDataBound)
                        previewControl.BindToDataSource(null);
                    return;
                }

                previewControl.BindToDataSource(ViewModel.NodeLogic.CachedValue);
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

                if (!DynamoSelection.Instance.Selection.Contains(ViewModel.NodeLogic))
                    DynamoSelection.Instance.Selection.Add(ViewModel.NodeLogic);
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

            var editWindow = new EditWindow(viewModel.DynamoViewModel)
            {
                DataContext = ViewModel,
                Title = "Edit Node Name"
            };

            editWindow.Owner = Window.GetWindow(this);

            editWindow.BindToProperty(null, new Binding("NickName")
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
            this.ViewModel.DynamoViewModel.OnNodeViewReady(this);
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

        internal void EnableInteraction()
        {
            foreach (UIElement e in inputGrid.Children)
            {
                if (enabledDict.ContainsKey(e))
                    e.IsEnabled = enabledDict[e];
            }

            ViewModel.ValidateConnectionsCommand.Execute(null);
        }

        private void topControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            //e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //set handled to avoid triggering key press
            //events on the workbench
            //e.Handled = true;
        }

        private void topControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            var view = WpfUtilities.FindUpVisualTree<DynamoView>(this);

            this.ViewModel.DynamoViewModel.ReturnFocusToSearch();

            view.mainGrid.Focus();

            var node = this.ViewModel.NodeModel;
            if (node.Workspace.Nodes.Contains(node))
            {
                Guid nodeGuid = this.ViewModel.NodeModel.GUID;
                this.ViewModel.DynamoViewModel.ExecuteCommand(
                    new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers.AsDynamoType()));
            }
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
            //Debug.WriteLine("Node right selected.");
            e.Handled = true;
        }

        private void NickNameBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Debug.WriteLine("Nickname double clicked!");
                if (this.ViewModel != null && this.ViewModel.RenameCommand.CanExecute(null))
                {
                    this.ViewModel.RenameCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        #region Preview Control Related Event Handlers

        private void OnPreviewIconMouseEnter(object sender, MouseEventArgs e)
        {
            previewInnerRect.Visibility = System.Windows.Visibility.Visible;
            previewOuterRect.Fill = FrozenResources.PreviewIconHoverBrush;

            if (PreviewControl.IsInTransition) // In transition state, come back later.
                return;

            if (PreviewControl.IsHidden)
            {
                if (PreviewControl.IsDataBound == false)
                    PreviewControl.BindToDataSource(ViewModel.NodeLogic.CachedValue);

                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }
        }

        private void OnPreviewIconMouseLeave(object sender, MouseEventArgs e)
        {
            RefreshPreviewIconDisplay();
            previewInnerRect.Visibility = System.Windows.Visibility.Hidden;

            if (PreviewControl.IsInTransition) // In transition state, come back later.
                return;

            if (PreviewControl.IsCondensed)
                PreviewControl.TransitionToState(PreviewControl.State.Hidden);
        }

        private void OnPreviewIconMouseClicked(object sender, MouseEventArgs e)
        {
            if (PreviewControl.IsInTransition) // In transition state, come back later.
                return;

            if (PreviewControl.IsCondensed)
                PreviewControl.TransitionToState(PreviewControl.State.Expanded);
            else if (PreviewControl.IsExpanded)
                PreviewControl.TransitionToState(PreviewControl.State.Condensed);

            previewOuterRect.Fill = FrozenResources.PreviewIconClickedBrush;
        }

        private void OnPreviewControlStateChanged(object sender, EventArgs e)
        {
            RefreshPreviewIconDisplay();

            if (this.previewIcon.IsMouseOver)
            {
                // The mouse is currently over the preview icon, so if the 
                // preview control is hidden, bring it into condensed state.
                var preview = sender as PreviewControl;
                if (preview.IsHidden != false)
                    preview.TransitionToState(PreviewControl.State.Condensed);
            }
            else
            {
                // The mouse is no longer over the preview icon, if the preview 
                // control is currently in condensed state, hide it from view.
                var preview = sender as PreviewControl;
                if (preview.IsCondensed != false)
                    preview.TransitionToState(PreviewControl.State.Hidden);
            }
        }

        private void RefreshPreviewIconDisplay()
        {
            if (this.previewControl == null)
                return;

            if (this.previewControl.IsHidden)
                previewOuterRect.Fill = FrozenResources.PreviewIconNormalBrush;
            else if (this.previewControl.IsCondensed)
                previewOuterRect.Fill = FrozenResources.PreviewIconHoverBrush;
            else if (this.previewControl.IsExpanded)
                previewOuterRect.Fill = FrozenResources.PreviewIconPinnedBrush;
            else if (this.previewControl.IsInTransition)
            {
                // No changes, those will come after transition is done.
            }
        }

        #endregion
    }
}
