﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
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
                if (previewControl == null)
                {
                    previewControl = new PreviewControl(ViewModel);
                    previewControl.StateChanged += OnPreviewControlStateChanged;
                    expansionBay.Children.Add(previewControl);
                }

                return previewControl;
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

            var size = new [] { ActualWidth, nodeBorder.ActualHeight };
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
                if ((previewControl == null))
                {
                    return;
                }

                // Enqueue an update of the preview control once it has completed its 
                // transition
                if (previewControl.IsInTransition)
                {
                    previewControl.EnqueueBindToDataSource(ViewModel.NodeModel.CachedValue);
                    return;
                }

                if (previewControl.IsHidden) // The preview control is hidden.
                {
                    // Invalidate the previously bound data, if any.
                    if (previewControl.IsDataBound)
                        previewControl.BindToDataSource(null);
                    return;
                }

                previewControl.BindToDataSource(ViewModel.NodeModel.CachedValue);
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

            var editWindow = new EditWindow(viewModel.DynamoViewModel)
            {
                DataContext = ViewModel,
                Title = Dynamo.Wpf.Properties.Resources.EditNodeWindowTitle 
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
            ViewModel.DynamoViewModel.ReturnFocusToSearch();
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

        private void NickNameBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Debug.WriteLine("Nickname double clicked!");
                if (ViewModel != null && ViewModel.RenameCommand.CanExecute(null))
                {
                    ViewModel.RenameCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        #region Preview Control Related Event Handlers

        private void OnPreviewIconMouseEnter(object sender, MouseEventArgs e)
        {
            previewInnerRect.Visibility = Visibility.Visible;
            previewOuterRect.Fill = FrozenResources.PreviewIconHoverBrush;

            if (PreviewControl.IsInTransition) // In transition state, come back later.
                return;

            if (PreviewControl.IsHidden)
            {
                if (PreviewControl.IsDataBound == false)
                    PreviewControl.BindToDataSource(ViewModel.NodeModel.CachedValue);

                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }
        }

        private void OnPreviewIconMouseLeave(object sender, MouseEventArgs e)
        {
            RefreshPreviewIconDisplay();
            previewInnerRect.Visibility = Visibility.Hidden;

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

            var preview = sender as PreviewControl;
            // If the preview is in a transition, return directly to avoid another
            // transition
            if (preview == null || preview.IsInTransition)
            {
                return;
            }

            if (previewIcon.IsMouseOver)
            {
                // The mouse is currently over the preview icon, so if the 
                // preview control is hidden, bring it into condensed state.
                if (preview.IsHidden != false)
                    preview.TransitionToState(PreviewControl.State.Condensed);
            }
            else
            {
                // The mouse is no longer over the preview icon, if the preview 
                // control is currently in condensed state, hide it from view.
                if (preview.IsCondensed != false)
                    preview.TransitionToState(PreviewControl.State.Hidden);
            }
        }

        private void RefreshPreviewIconDisplay()
        {
            if (previewControl == null)
                return;

            if (previewControl.IsHidden)
                previewOuterRect.Fill = FrozenResources.PreviewIconNormalBrush;
            else if (previewControl.IsCondensed)
                previewOuterRect.Fill = FrozenResources.PreviewIconHoverBrush;
            else if (previewControl.IsExpanded)
                previewOuterRect.Fill = FrozenResources.PreviewIconPinnedBrush;
            else if (previewControl.IsInTransition)
            {
                // No changes, those will come after transition is done.
            }
        }

        #endregion
      
    }
}
