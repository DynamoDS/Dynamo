﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Prompts;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System.Windows.Media;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Controls
{
    public partial class dynNodeView : IViewModelView<NodeViewModel>
    {
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);

        private NodeViewModel viewModel;

        public dynNodeView TopControl
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

        #region constructors

        public dynNodeView()
        {
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.PortsDictionary);

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(OnNodeViewLoaded);
            inputGrid.Loaded += new RoutedEventHandler(inputGrid_Loaded);

            this.SizeChanged += OnSizeChanged;
            this.DataContextChanged += OnDataContextChanged;

            Canvas.SetZIndex(this, 1);
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
                var size = new double[] { ActualWidth, ActualHeight };
                if (ViewModel.SetModelSizeCommand.CanExecute(size))
                {
                    Debug.WriteLine(string.Format("Updating {2} node size {0}:{1}", size[0], size[1], ViewModel.NodeLogic.GetType().ToString()));
                    ViewModel.SetModelSizeCommand.Execute(size);
                }
            }
        }

        /// <summary>
        /// This event handler is called soon as the NodeViewModel is bound to this 
        /// dynNodeView, which happens way before OnNodeViewLoaded event is sent. 
        /// There is a known bug in WPF 4.0 where DataContext becomes DisconnectedItem 
        /// when actions such as tab switching happens (that is when the View becomes 
        /// disconnected from the underlying ViewModel/Model that it was bound to). So 
        /// it is more reliable for dynNodeView to cache the NodeViewModel it is bound 
        /// to when it first becomes available, and refer to the cached value at a later
        /// time.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If this is the first time dynNodeView is bound to the NodeViewModel, 
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
            }
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

        void ViewModel_RequestShowNodeRename(object sender, EventArgs e)
        {
            var editWindow = new EditWindow
            {
                DataContext = ViewModel,
                Title = "Edit Node Name"
            };

            editWindow.BindToProperty(null, new Binding("NickName")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = ViewModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        void ViewModel_RequestShowNodeHelp(object sender, NodeHelpEventArgs e)
        {
            var helpDialog = new NodeHelpPrompt(e.Model);
            helpDialog.Show();
        }

        void NodeLogic_DispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            Dispatcher.Invoke(e.ActionToDispatch);
        }

        void inputGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //once the input grid is loaded, send a command
            //to the view model, which will be pushed down
            //to the model to ask for types to load custom UI elements
            ViewModel.SetupCustomUIElementsCommand.Execute(this);
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

        private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void topControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            var view = WPF.FindUpVisualTree<DynamoView>(this);

            dynSettings.ReturnFocusToSearch();

            view.mainGrid.Focus();

            var node = this.ViewModel.NodeModel;
            if (node.WorkSpace.Nodes.Contains(node))
            {
                Guid nodeGuid = this.ViewModel.NodeModel.GUID;
                dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                    new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers));
            }
            if (e.ClickCount == 2)
            {
                if (ViewModel.GotoWorkspaceCommand.CanExecute(null))
                {
                    ViewModel.GotoWorkspaceCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        private void topControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Node right selected.");
            e.Handled = true;
        }

        private void NickNameBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.CollapseTooltipCommand.Execute(null);
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

        private void NickNameBlock_OnMouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            string tooltipContent = ViewModel.Description;
            UIElement containingWorkspace = WPF.FindUpVisualTree<TabControl>(this);
            Point topLeft = textBlock.TranslatePoint(new Point(0, 0), containingWorkspace);
            double actualWidth = textBlock.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            double actualHeight = textBlock.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            Point botRight = new Point(topLeft.X + actualWidth, topLeft.Y + actualHeight);
            ViewModel.ShowTooltipCommand.Execute(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip, topLeft,
                botRight, tooltipContent, InfoBubbleViewModel.Direction.Bottom));
        }

        private void NickNameBlock_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.FadeOutTooltipCommand.Execute(null);
            else if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.HideInfoBubble(null);
        }

        private void InputPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ContentPresenter inputPort = sender as ContentPresenter;
            string content = (inputPort.Content as PortViewModel).ToolTipContent;
            if (string.IsNullOrWhiteSpace(content))
                return;

            UIElement containingWorkspace = WPF.FindUpVisualTree<TabControl>(this);
            Point topLeft = inputPort.TranslatePoint(new Point(0, 0), containingWorkspace);
            double actualWidth = inputPort.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            double actualHeight = inputPort.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            Point botRight = new Point(topLeft.X + actualWidth, topLeft.Y + actualHeight);
            ViewModel.ShowTooltipCommand.Execute(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip, topLeft,
                botRight, content, InfoBubbleViewModel.Direction.Right));
        }

        private void InputPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.FadeOutTooltipCommand.Execute(null);
            else if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.HideInfoBubble(null);
        }

        private void InputPort_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.CollapseTooltipCommand.Execute(null);
        }

        private void OutputPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ContentPresenter outputPort = sender as ContentPresenter;
            string content = (outputPort.Content as PortViewModel).ToolTipContent;
            if (string.IsNullOrWhiteSpace(content))
                return;

            UIElement containingWorkspace = WPF.FindUpVisualTree<TabControl>(this);
            Point topLeft = outputPort.TranslatePoint(new Point(0, 0), containingWorkspace);
            double actualWidth = outputPort.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            double actualHeight = outputPort.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            Point botRight = new Point(topLeft.X + actualWidth, topLeft.Y + actualHeight);
            ViewModel.ShowTooltipCommand.Execute(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip, topLeft,
                botRight, content, InfoBubbleViewModel.Direction.Left));
        }

        private void OutputPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.FadeOutTooltipCommand.Execute(null);
            else if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.HideInfoBubble(null);
        }

        private void OutputPort_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.CollapseTooltipCommand.Execute(null);
        }

        private void PreviewArrow_MouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = sender as UIElement;
            if (uiElement.Visibility == System.Windows.Visibility.Visible)
                ViewModel.ShowPreviewCommand.Execute(null);
        }
    }
}
