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
    public partial class dynNodeView
    {
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);

        #region private members

        Dictionary<dynPort, PortData> portDataDict = new Dictionary<dynPort, PortData>();
        private dynNodeViewModel vm;

        #endregion

        #region public members

        public dynNodeView TopControl
        {
            get { return topControl; }
        }

        public Grid ContentGrid
        {
            get { return inputGrid; }
        }

        #endregion

        #region constructors
        /// <summary>
        /// dynElement constructor for use by workbench in creating dynElements
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nickName"></param>
        public dynNodeView()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(dynNodeView_Loaded);
            inputGrid.Loaded += new RoutedEventHandler(inputGrid_Loaded);

            //set the main grid's data context to 
            //this element
            //nickNameBlock.DataContext = this;
            //elementRectangle.DataContext = this;
            //topControl.DataContext = this;
            //elementRectangle.DataContext = this;

            //set the z index to 2
            Canvas.SetZIndex(this, 1);

        }

        void dynNodeView_Loaded(object sender, RoutedEventArgs e)
        {
            vm = DataContext as dynNodeViewModel;
            vm.NodeLogic.DispatchedToUI += new DispatchedToUIThreadHandler(NodeLogic_DispatchedToUI);
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
            vm.SetupCustomUIElementsCommand.Execute(this);
        }

        #endregion

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
            if (vm.SetStateCommand.CanExecute(ElementState.DEAD))
                vm.SetStateCommand.Execute(ElementState.DEAD);
        }

        internal void EnableInteraction()
        {
            foreach (UIElement e in inputGrid.Children)
            {
                if (enabledDict.ContainsKey(e))
                    e.IsEnabled = enabledDict[e];
            }

            //MVVM: converted to command on view model
            //ValidateConnections();
            vm.ValidateConnectionsCommand.Execute();
        }

        public void CallUpdateLayout(FrameworkElement el)
        {
            el.UpdateLayout();
        }

        private void topControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        //for information about routed events see:
        //http://msdn.microsoft.com/en-us/library/ms742806.aspx

        //tunneling event
        //from MSDN "...Tunneling routed events are often used or handled as part of the compositing for a 
        //control, such that events from composite parts can be deliberately suppressed or replaced by 
        //events that are specific to the complete control.
        //starts at parent and climbs down children to element
        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            //e.Handled = true;
        }

        //bubbling event
        //from MSDN "...Bubbling routed events are generally used to report input or state changes 
        //from distinct controls or other UI elements."
        //starts at element and climbs up parents to root
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
            dynSettings.Bench.mainGrid.Focus();
            //dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(vm.SelectCommand, this));
            //dynSettings.Controller.ProcessCommandQueue();
            vm.SelectCommand.Execute();
            //e.Handled = true;
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
    }
}
