//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Prompts;
using Dynamo.Selection;
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

        public dynNodeView TopControl
        {
            get { return topControl; }
        }

        public Grid ContentGrid
        {
            get { return inputGrid; }
        }

        public NodeViewModel ViewModel { get; set; }

        #region constructors

        public dynNodeView()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(dynNodeView_Loaded);
            inputGrid.Loaded += new RoutedEventHandler(inputGrid_Loaded);
            this.LayoutUpdated += OnLayoutUpdated;

            Canvas.SetZIndex(this, 1);
        }

        #endregion

        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            if (ViewModel != null)
            {
                //Debug.WriteLine("Node layout updated.");
                if (ViewModel.NodeLogic.Height != this.ActualHeight ||
                    ViewModel.NodeLogic.Width != this.ActualWidth)
                {
                    ViewModel.NodeLogic.Height = this.ActualHeight;
                    ViewModel.NodeLogic.Width = this.ActualWidth;
                }
            }

        }

        void dynNodeView_Loaded(object sender, RoutedEventArgs e)
        {
            //This is an annoying bug in WPF for .net 4.0
            //You need to cache a reference to the data context or
            //when switching back and forth between tabs, the element's
            //data context will come back as disconnected.
            ViewModel = this.DataContext as NodeViewModel;

            ViewModel.NodeLogic.DispatchedToUI += new DispatchedToUIThreadHandler(NodeLogic_DispatchedToUI);
            ViewModel.RequestShowNodeHelp += new NodeViewModel.NodeHelpEventHandler(ViewModel_RequestShowNodeHelp);
            ViewModel.RequestShowNodeRename += new EventHandler(ViewModel_RequestShowNodeRename);
            ViewModel.RequestsSelection += new EventHandler(ViewModel_RequestsSelection);

            ViewModel.NodeLogic.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(NodeLogic_PropertyChanged);
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
            var editWindow = new EditWindow { DataContext = ViewModel };

            var bindingVal = new Binding("NickName")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = ViewModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            editWindow.editText.SetBinding(TextBox.TextProperty, bindingVal);

            editWindow.Title = "Edit Node Name";

            if (editWindow.ShowDialog() != true)
            {
                return;
            }
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
            if (ViewModel.SetStateCommand.CanExecute(ElementState.DEAD))
                ViewModel.SetStateCommand.Execute(ElementState.DEAD);
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

        public void CallUpdateLayout(FrameworkElement el)
        {
            el.UpdateLayout();
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

            dynSettings.ReturnFocusToSearch();
            //dynSettings.Bench.mainGrid.Focus();
            var view = WPF.FindUpVisualTree<DynamoView>(this);
            view.mainGrid.Focus();

            Guid nodeGuid = this.ViewModel.NodeModel.GUID;
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers));
        }

        private void topControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Node right selected.");
            e.Handled = true;
        }

        //private void dynNodeView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var viewModel = DataContext as dynNodeViewModel;
        //    if (viewModel != null)
        //        viewModel.ViewCustomNodeWorkspaceCommand.Execute();
        //}

        private void NickNameBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.CollapseTooltipCommand.Execute(null);
            if (e.ClickCount > 1)
            {
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
            string tooltipContent = ViewModel.NickName + '\n' + ViewModel.Description;
            UIElement containingWorkspace = WPF.FindUpVisualTree<TabControl>(this);
            Point topLeft = textBlock.TranslatePoint(new Point(0, 0), containingWorkspace);
            double actualWidth = textBlock.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            double actualHeight = textBlock.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            Point botRight = new Point(topLeft.X + actualWidth, topLeft.Y + actualHeight);
            ViewModel.ShowTooltipCommand.Execute(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip, topLeft, botRight, tooltipContent, InfoBubbleViewModel.Direction.Bottom, Guid.Empty));
        }

        private void NickNameBlock_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.FadeOutTooltipCommand.Execute(null);
        }

        private void InputPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ContentPresenter inputPort = sender as ContentPresenter;
            string content = (inputPort.Content as PortViewModel).ToolTipContent;
            UIElement containingWorkspace = WPF.FindUpVisualTree<TabControl>(this);
            Point topLeft = inputPort.TranslatePoint(new Point(0, 0), containingWorkspace);
            double actualWidth = inputPort.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            double actualHeight = inputPort.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            Point botRight = new Point(topLeft.X + actualWidth, topLeft.Y + actualHeight);
            ViewModel.ShowTooltipCommand.Execute(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip, topLeft, botRight, content, InfoBubbleViewModel.Direction.Right, Guid.Empty));
        }

        private void InputPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.FadeOutTooltipCommand.Execute(null);
        }

        private void InputPort_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.CollapseTooltipCommand.Execute(null);
        }

        private void OutputPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ContentPresenter outputPort = sender as ContentPresenter;
            string content = (outputPort.Content as PortViewModel).ToolTipContent;
            UIElement containingWorkspace = WPF.FindUpVisualTree<TabControl>(this);
            Point topLeft = outputPort.TranslatePoint(new Point(0, 0), containingWorkspace);
            double actualWidth = outputPort.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            double actualHeight = outputPort.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
            Point botRight = new Point(topLeft.X + actualWidth, topLeft.Y + actualHeight);
            ViewModel.ShowTooltipCommand.Execute(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip, topLeft, botRight, content, InfoBubbleViewModel.Direction.Left, Guid.Empty));
        }

        private void OutputPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.FadeOutTooltipCommand.Execute(null);
        }

        private void OutputPort_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.CollapseTooltipCommand.Execute(null);
        }
    }
}
