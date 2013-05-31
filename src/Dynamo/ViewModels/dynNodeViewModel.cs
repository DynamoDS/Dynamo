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
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary>
    
    public class dynNodeViewModel : dynViewModelBase
    {
        #region delegates
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);
        #endregion

        #region private members

        ObservableCollection<dynPortViewModel> inPorts = new ObservableCollection<dynPortViewModel>();
        ObservableCollection<dynPortViewModel> outPorts = new ObservableCollection<dynPortViewModel>();
        
        dynNodeModel nodeLogic;
        public dynNodeModel NodeModel { get { return nodeLogic; } private set { nodeLogic = value; }}
        
        int preferredHeight = 30;
        private bool isFullyConnected = false;
        private double dropShadowOpacity = 0;
        
        #endregion

        #region public members

        public bool IsFullyConnected
        {
            get { return isFullyConnected; }
            set
            {
                isFullyConnected = value;
                RaisePropertyChanged("IsFullyConnected");
            }
        }
        
        public LacingStrategy ArgumentLacing
        {
            get { return nodeLogic.ArgumentLacing; }
            set
            {
                nodeLogic.ArgumentLacing = value;
                RaisePropertyChanged("ArgumentLacing");
            }
        }

        public dynNodeModel NodeLogic
        {
            get { return nodeLogic; }
        }

        public string ToolTipText
        {
            get { return nodeLogic.ToolTipText; }
        }
        
        public ObservableCollection<dynPortViewModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

        public ObservableCollection<dynPortViewModel> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
                RaisePropertyChanged("OutPorts");
            }
        }

        public bool IsSelected
        {
            get { return nodeLogic.IsSelected; }
        }

        public string NickName
        {
            get { return nodeLogic.NickName; }
        }

        public ElementState State
        {
            get { return nodeLogic.State; }
        }
        
        public double DropShadowOpacity
        {
            get
            {
                return nodeLogic.IsCustomFunction? 1:0;
            }
        }

        /// <summary>
        /// Element's left position is two-way bound to this value
        /// </summary>
        public double Left
        {
            get { return nodeLogic.X; }
            set
            {
                nodeLogic.X = value;
                RaisePropertyChanged("Left");
            }
        }

        /// <summary>
        /// Element's top position is two-way bound to this value
        /// </summary>
        public double Top
        {
            get { return nodeLogic.Y; }
            set
            {
                nodeLogic.Y = value;
                RaisePropertyChanged("Top");
            }
        }

        public double ZIndex
        {
            get { return 3; }
        }

        /// <summary>
        /// Input grid's enabled state is now bound to this property
        /// which tracks the node model's InteractionEnabled property
        /// </summary>
        public bool IsInteractionEnabled
        {
            get { return nodeLogic.InteractionEnabled; }
        }

        public bool IsVisible
        {
            get
            {
                return nodeLogic.IsVisible;
            }
            set
            {
                nodeLogic.IsVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public bool IsUpstreamVisible
        {
            get
            {
                return nodeLogic.IsUpstreamVisible;
            }
            set
            {
                nodeLogic.IsUpstreamVisible = value;
                RaisePropertyChanged("IsUpstreamVisible");
            }
        }

        #endregion

        #region commands

        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand<string> SetLacingTypeCommand { get; set; }
        public DelegateCommand<object> SetStateCommand { get; set; }
        public DelegateCommand SelectCommand { get; set; }
        public DelegateCommand ViewCustomNodeWorkspaceCommand { get; set; }
        public DelegateCommand<object> SetLayoutCommand { get; set; }
        public DelegateCommand<dynNodeView> SetupCustomUIElementsCommand { get; set; }
        public DelegateCommand ValidateConnectionsCommand { get; set; }
        public DelegateCommand ToggleIsVisibleCommand { get; set; }
        public DelegateCommand ToggleIsUpstreamVisibleCommand { get; set; }
        #endregion

        #region constructors

        public dynNodeViewModel(dynNodeModel logic)
        {
            nodeLogic = logic;

            //respond to collection changed events to sadd
            //and remove port model views
            logic.InPorts.CollectionChanged += inports_collectionChanged;
            logic.OutPorts.CollectionChanged += outports_collectionChanged;
            
            logic.PropertyChanged += logic_PropertyChanged;
            dynSettings.Controller.DynamoViewModel.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);

            DeleteCommand = new DelegateCommand(DeleteNodeAndItsConnectors, CanDeleteNode);
            SetLacingTypeCommand = new DelegateCommand<string>(new Action<string>(SetLacingType), CanSetLacingType);
            SetStateCommand = new DelegateCommand<object>(SetState, CanSetState);
            SelectCommand = new DelegateCommand(Select, CanSelect);
            ViewCustomNodeWorkspaceCommand = new DelegateCommand(ViewCustomNodeWorkspace, CanViewCustomNodeWorkspace);
            SetLayoutCommand = new DelegateCommand<object>(SetLayout, CanSetLayout);
            SetupCustomUIElementsCommand = new DelegateCommand<dynNodeView>(SetupCustomUIElements, CanSetupCustomUIElements);
            ValidateConnectionsCommand = new DelegateCommand(ValidateConnections, CanValidateConnections);
            ToggleIsVisibleCommand = new DelegateCommand(ToggleIsVisible, CanVisibilityBeToggled);
            ToggleIsUpstreamVisibleCommand = new DelegateCommand(ToggleIsUpstreamVisible, CanUpstreamVisibilityBeToggled);



            //Do a one time setup of the initial ports on the node
            //we can not do this automatically because this constructor
            //is called after the node's constructor where the ports
            //are initially registered
            SetupInitialPortViewModels();

            dynSettings.Controller.RequestNodeSelect += new NodeEventHandler(Controller_RequestNodeSelect);
        }

        void Controller_RequestNodeSelect(object sender, EventArgs e)
        {
            dynNodeModel n = (e as NodeEventArgs).Node;
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(SelectCommand, n));
            dynSettings.Controller.ProcessCommandQueue();
        }

        #endregion

        /// <summary>
        /// Do a one setup of the ports 
        /// </summary>
        private void SetupInitialPortViewModels()
        {
            foreach (var item in nodeLogic.InPorts)
            {
                InPorts.Add(new dynPortViewModel(item as dynPortModel, nodeLogic));
            }

            foreach (var item in nodeLogic.OutPorts)
            {
                OutPorts.Add(new dynPortViewModel(item as dynPortModel, nodeLogic));
            }
        }

        /// <summary>
        /// Respond to property changes on the model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSpace":
                    RaisePropertyChanged("NodeVisibility");
                    break;
                    
            }
        }

        /// <summary>
        /// Respond to property changes on the node model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void logic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NickName":
                    RaisePropertyChanged("NickName");
                    break;
                case "X":
                    RaisePropertyChanged("Left");
                    break;
                case "Y":
                    RaisePropertyChanged("Top");
                    break;
                case "InteractionEnabled":
                    RaisePropertyChanged("IsInteractionEnabled");
                    break;
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    break;
                case "State":
                    RaisePropertyChanged("State");
                    break;
                case "ArgumentLacing":
                    SetLacingTypeCommand.RaiseCanExecuteChanged();
                    break;
            }
        }

        private bool CanDeleteNode()
        {
            return true;
        }

        private void DeleteNodeAndItsConnectors()
        {
            dynSettings.Controller.DynamoViewModel.DeleteCommand.Execute(this.nodeLogic);
        }

        void SetLacingType(string parameter)
        {
            if (parameter == "First")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.First;
            }
            else if (parameter == "Longest")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Longest;
            }
            else if (parameter == "Shortest")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Shortest;
            }
            else if (parameter == "CrossProduct")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.CrossProduct;
            }
            else
                NodeLogic.ArgumentLacing = LacingStrategy.Disabled;

            RaisePropertyChanged("Lacing");
        }

        bool CanSetLacingType(string parameter)
        {
            if (this.ArgumentLacing == LacingStrategy.Disabled)
                return false;

            return true;
        }

        private void ViewCustomNodeWorkspace()
        {
            var f = (nodeLogic as dynFunction);
            if(f!= null)
                dynSettings.Controller.DynamoViewModel.ViewCustomNodeWorkspace(f.Definition);
        }

        private bool CanViewCustomNodeWorkspace()
        {
            if (nodeLogic.IsCustomFunction)
                return true;
            return false;
        }

        private void SetLayout(object parameters)
        {
            var dict = parameters as Dictionary<string,
            double >;
            nodeLogic.X = dict["X"];
            nodeLogic.Y = dict["Y"];
            nodeLogic.Height = dict["Height"];
            nodeLogic.Width = dict["Width"];
        }

        private bool CanSetLayout(object parameters)
        {
            var dict = parameters as Dictionary<string,
            double>;
            if (dict == null)
                return false;
            return true;
        }

        void inports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            //PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    InPorts.Add(new dynPortViewModel(item as dynPortModel,nodeLogic));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item
                //is the one passed in
                foreach (var item in e.OldItems)
                {
                    InPorts.Remove(InPorts.ToList().First(x => x.PortModel == item));
                }
            }
        }

        void outports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            //PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    OutPorts.Add(new dynPortViewModel(item as dynPortModel, nodeLogic));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item is the
                //one passed in
                foreach (var item in e.OldItems)
                {
                    OutPorts.Remove(OutPorts.ToList().First(x => x.PortModel == item));
                }
            }
        }

        private void ToggleIsVisible()
        {
            this.nodeLogic.IsVisible = !this.nodeLogic.IsVisible;
            RaisePropertyChanged("IsVisible");
        }

        private void ToggleIsUpstreamVisible()
        {
            this.nodeLogic.IsUpstreamVisible = !this.nodeLogic.IsUpstreamVisible;
            RaisePropertyChanged("IsUpstreamVisible");
        }

        private bool CanVisibilityBeToggled() 
        {
            return true;
        }

        private bool CanUpstreamVisibilityBeToggled()
        {
            return true;
        }

        private void ValidateConnections()
        {
            nodeLogic.ValidateConnections();
        }

        private bool CanValidateConnections()
        {
            return true;
        }

        private void SetupCustomUIElements(dynNodeView NodeUI)
        {
            nodeLogic.SetupCustomUIElements(NodeUI);
        }

        private bool CanSetupCustomUIElements(dynNodeView NodeUI)
        {
            return true;
        }

        private void SetState(object parameter)
        {
            nodeLogic.State = (ElementState)parameter;
        }

        private bool CanSetState(object parameter)
        {
            if(parameter is ElementState)
                return true;
            return false;
        }

        private void Select()
        {
            if (!nodeLogic.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }

                if (!DynamoSelection.Instance.Selection.Contains(nodeLogic))
                    DynamoSelection.Instance.Selection.Add(nodeLogic);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(nodeLogic);
                }
            }
        }

        private bool CanSelect()
        {
            return true;
        }

    }
}

