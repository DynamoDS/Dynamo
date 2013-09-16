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
using System.Collections.ObjectModel;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary>
    
    public partial class NodeViewModel : ViewModelBase
    {
        #region delegates
        public delegate void SetToolTipDelegate(string message);
        public delegate void NodeHelpEventHandler(object sender, NodeHelpEventArgs e);
        #endregion

        #region private members

        ObservableCollection<PortViewModel> inPorts = new ObservableCollection<PortViewModel>();
        ObservableCollection<PortViewModel> outPorts = new ObservableCollection<PortViewModel>();
        
        NodeModel nodeLogic;
        public NodeModel NodeModel { get { return nodeLogic; } private set { nodeLogic = value; }}
        
        private bool isFullyConnected = false;
        
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

        public NodeModel NodeLogic
        {
            get { return nodeLogic; }
        }

        public string ToolTipText
        {
            get { return nodeLogic.ToolTipText; }
        }
        
        public ObservableCollection<PortViewModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

        public ObservableCollection<PortViewModel> OutPorts
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
            set { nodeLogic.NickName = value; }
        }

        public string OldValue
        {
            get { 
                if (this.nodeLogic.WorkSpace is FuncWorkspace)
                {
                    return "Not available in custom nodes";
                }
                return NodeModel.BuildValueString(nodeLogic.OldValue, 0, 3, 0, 2).TrimEnd('\n');
            }
        }

        public ElementState State
        {
            get { return nodeLogic.State; }
        }

        public string Description
        {
            get { return nodeLogic.Description; }
        }
        
        //public double DropShadowOpacity
        //{
        //    get
        //    {
        //        return nodeLogic.IsCustomFunction? 1:0;
        //    }
        //}

        public bool IsCustomFunction
        {
            get { return nodeLogic.IsCustomFunction ? true : false; }
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

        #region events
        public event NodeHelpEventHandler RequestShowNodeHelp;
        public virtual void OnRequestShowNodeHelp(Object sender, NodeHelpEventArgs e)
        {
            if (RequestShowNodeHelp != null)
            {
                RequestShowNodeHelp(this, e);
            }
        }

        public event EventHandler RequestShowNodeRename;
        public virtual void OnRequestShowNodeRename(Object sender, EventArgs e)
        {
            if (RequestShowNodeRename != null)
            {
                RequestShowNodeRename(this, e);
            }
        }

        public event EventHandler RequestsSelection;
        public virtual void OnRequestsSelection(Object sender, EventArgs e)
        {
            if (RequestsSelection != null)
            {
                RequestsSelection(this, e);
            }
        }

        #endregion

        #region constructors

        public NodeViewModel(NodeModel logic)
        {
            nodeLogic = logic;

            //respond to collection changed events to sadd
            //and remove port model views
            logic.InPorts.CollectionChanged += inports_collectionChanged;
            logic.OutPorts.CollectionChanged += outports_collectionChanged;
            
            logic.PropertyChanged += logic_PropertyChanged;
            dynSettings.Controller.DynamoViewModel.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);

            //Do a one time setup of the initial ports on the node
            //we can not do this automatically because this constructor
            //is called after the node's constructor where the ports
            //are initially registered
            SetupInitialPortViewModels();

            dynSettings.Controller.RequestNodeSelect += new NodeEventHandler(Controller_RequestNodeSelect);
        }

        void Controller_RequestNodeSelect(object sender, EventArgs e)
        {
            ModelBase n = (e as ModelEventArgs).Model;

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(n);
        }

        #endregion

        /// <summary>
        /// Do a one setup of the ports 
        /// </summary>
        private void SetupInitialPortViewModels()
        {
            foreach (var item in nodeLogic.InPorts)
            {
                InPorts.Add(new PortViewModel(item as PortModel, nodeLogic));
            }

            foreach (var item in nodeLogic.OutPorts)
            {
                OutPorts.Add(new PortViewModel(item as PortModel, nodeLogic));
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
                case "CurrentWorkspace":
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
                case "OldValue":
                    RaisePropertyChanged("OldValue");
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
                    RaisePropertyChanged("ArgumentLacing");
                    break;
                case "IsVisible":
                    RaisePropertyChanged("IsVisible");
                    break;
                case "IsUpstreamVisible":
                    RaisePropertyChanged("IsUpstreamVisible");
                    break;
            }
        }

        private void ShowHelp(object parameter)
        {
            //var helpDialog = new NodeHelpPrompt(this.NodeModel);
            //helpDialog.Show();

            OnRequestShowNodeHelp(this, new NodeHelpEventArgs(NodeModel));
        }

        private bool CanShowHelp(object parameter)
        {
            return true;
        }

        private void ShowRename(object parameter)
        {
            //var editWindow = new dynEditWindow { DataContext = this };

            //var bindingVal = new Binding("NickName")
            //{
            //    Mode = BindingMode.TwoWay,
            //    NotifyOnValidationError = false,
            //    Source = this,
            //    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            //};
            //editWindow.editText.SetBinding(TextBox.TextProperty, bindingVal);

            //editWindow.Title = "Edit Node Name";

            //if (editWindow.ShowDialog() != true)
            //{
            //    return;
            //}

            OnRequestShowNodeRename(this, EventArgs.Empty);
        }

        private bool CanShowRename(object parameter)
        {
            return true;
        }

        private bool CanDeleteNode(object parameter)
        {
            return true;
        }

        private void DeleteNodeAndItsConnectors(object parameter)
        {
            dynSettings.Controller.DynamoModel.Delete(nodeLogic);
        }

        void SetLacingType(object param)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            LacingStrategy strategy = LacingStrategy.Disabled;
            if (!System.Enum.TryParse(param.ToString(), out strategy))
                strategy = LacingStrategy.Disabled;

            NodeLogic.ArgumentLacing = strategy;

            RaisePropertyChanged("ArgumentLacing");
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        bool CanSetLacingType(object param)
        {
            // Only allow setting of lacing strategy when it is not disabled.
            return (this.ArgumentLacing != LacingStrategy.Disabled);
        }

        private void ViewCustomNodeWorkspace(object parameter)
        {
            var f = (nodeLogic as Function);
            if(f!= null)
                dynSettings.Controller.DynamoViewModel.ViewCustomNodeWorkspace(f.Definition);
        }

        private bool CanViewCustomNodeWorkspace(object parameter)
        {
            return nodeLogic.IsCustomFunction;
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
                    InPorts.Add(new PortViewModel(item as PortModel,nodeLogic));
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
                    OutPorts.Add(new PortViewModel(item as PortModel, nodeLogic));
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

        private void ToggleIsVisible(object parameter)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            this.nodeLogic.IsVisible = !this.nodeLogic.IsVisible;

            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private void ToggleIsUpstreamVisible(object parameter)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            this.nodeLogic.IsUpstreamVisible = !this.nodeLogic.IsUpstreamVisible;

            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private bool CanVisibilityBeToggled(object parameter) 
        {
            return true;
        }

        private bool CanUpstreamVisibilityBeToggled(object parameter)
        {
            return true;
        }

        private void ValidateConnections(object parameter)
        {
            nodeLogic.ValidateConnections();
        }

        private bool CanValidateConnections(object parameter)
        {
            return true;
        }

        private void SetupCustomUIElements(object NodeUI)
        {
            nodeLogic.SetupCustomUIElements(NodeUI);
        }

        private bool CanSetupCustomUIElements(object NodeUI)
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

        private void Select(object parameter)
        {
            //this logic has been moved to the view
            //because it depends on Keyboard modifiers.

            //if (!nodeLogic.IsSelected)
            //{
            //    if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            //    {
            //        DynamoSelection.Instance.ClearSelection();
            //    }

            //    if (!DynamoSelection.Instance.Selection.Contains(nodeLogic))
            //        DynamoSelection.Instance.Selection.Add(nodeLogic);
            //}
            //else
            //{
            //    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            //    {
            //        DynamoSelection.Instance.Selection.Remove(nodeLogic);
            //    }
            //}

            //if the node is not already selected
            //then clear the selection

            OnRequestsSelection(this, EventArgs.Empty);
        }

        private bool CanSelect(object parameter)
        {
            return true;
        }

    }

    public class NodeHelpEventArgs : EventArgs
    {
        public NodeModel Model { get; set; }
        public NodeHelpEventArgs(NodeModel model)
        {
            Model = model;
        }
    }
}

