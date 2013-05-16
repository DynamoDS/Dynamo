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
using System.Windows.Input;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Grid = System.Windows.Controls.Grid;

namespace Dynamo.Controls
{
    public partial class dynNodeView : IViewModelView<dynNodeViewModel>
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
                ViewModel.NodeLogic.Height = this.ActualHeight;
                ViewModel.NodeLogic.Width = this.ActualWidth;
            }
  
        }

        void dynNodeView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NodeLogic.DispatchedToUI += new DispatchedToUIThreadHandler(NodeLogic_DispatchedToUI);
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

            ViewModel.ValidateConnectionsCommand.Execute();
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
            dynSettings.ReturnFocusToSearch();
            dynSettings.Bench.mainGrid.Focus();
            ViewModel.SelectCommand.Execute();
        }

        private void topControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Node right selected.");
            e.Handled = true;
        }

        private void dynNodeView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as dynNodeViewModel;
            if (viewModel != null)
                viewModel.ViewCustomNodeWorkspaceCommand.Execute();
        }

        public dynNodeViewModel ViewModel
        {
            get
            {
                if (this.DataContext is dynNodeViewModel)
                    return (dynNodeViewModel)this.DataContext;
                else
                    return null;
            }
        }
    }
}
