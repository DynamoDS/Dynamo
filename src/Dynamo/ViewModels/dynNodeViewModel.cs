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
using System.Windows.Media;
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
    public enum ElementState { DEAD, ACTIVE, ERROR };

    public partial class dynNodeViewModel : dynViewModelBase, ISelectable
    {
        #region delegates
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);
        #endregion

        #region private members

        ObservableCollection<dynPortViewModel> inPorts = new ObservableCollection<dynPortViewModel>();
        ObservableCollection<dynPortViewModel> outPorts = new ObservableCollection<dynPortViewModel>();
        Dictionary<dynPort, PortData> portDataDict = new Dictionary<dynPort, PortData>();
        string nickName;
        string toolTipText = "";
        ElementState state;
        dynNode nodeLogic;
        bool isSelected = false;
        int preferredHeight = 30;
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
        }

        public dynNode NodeLogic
        {
            get { return nodeLogic; }
        }

        public string ToolTipText
        {
            get
            {
                return toolTipText;
            }
            set
            {
                toolTipText = value;
                RaisePropertyChanged("ToolTipText");
            }
        }

        public ObservableCollection<dynPortViewModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
            }
        }

        public ObservableCollection<dynPortViewModel> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
            }
        }

        public string NickName
        {
            get { return nodeLogic.NickName; }
        }

        public ElementState State
        {
            get { return state; }
            set
            {
                if (value != ElementState.ERROR)
                {
                    SetTooltip();
                }

                state = value;
                RaisePropertyChanged("State");
            }
        }

        public bool IsSelected
        {
            //TODO:Remove brush setting from here
            //brushes should be controlled by a converter

            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");

                if (isSelected)
                {
                    var inConnectors = inPorts.SelectMany(x => x.Connectors);
                    var outConnectors = outPorts.SelectMany(x => x.Connectors);

                    foreach (dynConnector c in inConnectors)
                    {
                        if (c.Start != null && c.Start.Owner.IsSelected)
                        {
                            c.StrokeBrush = new LinearGradientBrush(Colors.Cyan, Colors.Cyan, 0);
                        }
                        else
                        {
                            c.StrokeBrush = new LinearGradientBrush(Color.FromRgb(31, 31, 31), Colors.Cyan, 0);
                        }
                    }
                    foreach (dynConnector c in outConnectors)
                    {
                        if (c.End != null & c.End.Owner.IsSelected)
                        {
                            c.StrokeBrush = new LinearGradientBrush(Colors.Cyan, Colors.Cyan, 0);
                        }
                        else
                        {
                            c.StrokeBrush = new LinearGradientBrush(Colors.Cyan, Color.FromRgb(31, 31, 31), 0);
                        }
                    }
                }
                else
                {
                    foreach (dynConnector c in inPorts.SelectMany(x => x.Connectors)
                        .Concat(outPorts.SelectMany(x => x.Connectors)))
                    {
                        c.StrokeBrush = new SolidColorBrush(Color.FromRgb(31, 31, 31));
                    }

                }
            }
        }

        public int PreferredHeight
        {
            get
            {
                return preferredHeight;
            }
            set
            {
                preferredHeight = value;
                RaisePropertyChanged("PreferredHeight");
            }
        }
        #endregion

        #region commands

        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand<string> SetLacingTypeCommand { get; set; }
        public DelegateCommand SetStateCommand { get; set; }
        public DelegateCommand SelectCommand { get; set; }

        #endregion

        #region constructors
        /// <summary>
        /// dynElement constructor for use by workbench in creating dynElements
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nickName"></param>
        public dynNodeViewModel(dynNode logic)
        {
            nodeLogic = logic;

            //respond to collection changed events to add
            //and remove port model views
            logic.InPorts.CollectionChanged += inports_collectionChanged;
            logic.OutPorts.CollectionChanged += outports_collectionChanged;

            this.IsSelected = false;

            State = ElementState.DEAD;

            logic.PropertyChanged += logic_PropertyChanged;

            DeleteCommand = new DelegateCommand(new Action(DeleteNode()), CanDeleteNode);
            SetLacingTypeCommand = new DelegateCommand<string>(new Action<string>(SetLacingType), CanSetLacingType);
        }

        void logic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NickName":
                    RaisePropertyChanged("NickName");
                    break;
                case "ArgumentLacing":
                    RaisePropertyChanged("ArgumentLacing");
                    break;
            }
        }

        private bool CanDeleteNode()
        {
            return true;
        }

        private Action DeleteNode()
        {
            foreach (var port in OutPorts)
            {
                for (int j = port.Connectors.Count - 1; j >= 0; j--)
                {
                    port.Connectors[j].Kill();
                }
            }

            foreach (dynPort p in InPorts)
            {
                for (int j = p.Connectors.Count - 1; j >= 0; j--)
                {
                    p.Connectors[j].Kill();
                }
            }

            NodeLogic.DisableReporting();
            NodeLogic.Destroy();
            NodeLogic.Cleanup();

            dynSettings.Workbench.Selection.Remove(this);
            dynSettings.Controller.Nodes.Remove(NodeLogic);
            //dynSettings.Workbench.Children.Remove(node);
        }

        void SetLacingType(string parameter)
        {
            if (parameter == "Single")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Single;
            }
            else if (parameter == "Longest")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Longest;
            }
            else if (parameter == "Shortest")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Shortest;
            }
            else
                NodeLogic.ArgumentLacing = LacingStrategy.Single;

            ReportPropertyChanged("Lacing");
        }

        bool CanSetLacingType(string parameter)
        {
            return true;
        }

        void inports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    InPorts.Add(new dynPortViewModel(item));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item
                //is the one passed in
                foreach (var item in e.OldItems)
                {
                    InPorts.Remove(InPorts.ToList().Where(x => x.PortModel == item));
                }
            }
        }

        void outports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    OutPorts.Add(new dynPortViewModel(item));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item is the
                //one passed in
                foreach (var item in e.OldItems)
                {
                    OutPorts.Remove(OutPorts.ToList().Where(x => x.PortModel == item));
                }
            }
        }

        #endregion

        public void RegisterAllPorts()
        {
            RegisterInputs();
            RegisterOutputs();

            //UpdateLayout();

            ValidateConnections();
        }

        public void UpdateConnections()
        {
            foreach (var p in InPorts.Concat(OutPorts))
                p.Update();
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
            State = ElementState.DEAD;
        }

        internal void EnableInteraction()
        {
            foreach (UIElement e in inputGrid.Children)
            {
                if (enabledDict.ContainsKey(e))
                    e.IsEnabled = enabledDict[e];
            }
            ValidateConnections();
        }

        /// <summary>
        /// Is this node an entry point to the program?
        /// </summary>
        public bool IsTopmost
        {
            get
            {
                return OutPorts == null
                    || OutPorts.All(x => !x.Connectors.Any());
            }
        }

        /// <summary>
        /// Reads inputs list and adds ports for each input.
        /// </summary>
        public void RegisterInputs()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in nodeLogic.InPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                var port = AddPort(PortType.INPUT, nodeLogic.InPortData[count].NickName, count);

                port.DataContext = this;

                portDataDict[port] = pd;
                count++;
            }

            if (inPorts.Count > count)
            {
                foreach (var inport in inPorts.Skip(count))
                {
                    RemovePort(inport);
                }

                for (int i = inPorts.Count - 1; i >= count; i--)
                {
                    inPorts.RemoveAt(i);
                }
                //InPorts.RemoveRange(count, inPorts.Count - count);
            }
        }

        /// <summary>
        /// Reads outputs list and adds ports for each output
        /// </summary>
        public void RegisterOutputs()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in nodeLogic.OutPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                var port = AddPort(PortType.OUTPUT, pd.NickName, count);

                port.DataContext = this;

                portDataDict[port] = pd;
                count++;
            }

            if (outPorts.Count > count)
            {
                foreach (var outport in outPorts.Skip(count))
                {
                    RemovePort(outport);
                }

                for (int i = outPorts.Count - 1; i >= count; i--)
                {
                    outPorts.RemoveAt(i);
                }

                //OutPorts.RemoveRange(count, outPorts.Count - count);
            }
        }

        //TODO: call connect and disconnect for dynNode

        /// <summary>
        /// When a port is connected, register a listener for the dynElementUpdated event
        /// and tell the object to build
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void p_PortConnected(object sender, EventArgs e)
        {
            ValidateConnections();

            var port = (dynPort)sender;
            if (port.PortType == PortType.INPUT)
            {
                var data = InPorts.IndexOf(port);
                var startPort = port.Connectors[0].Start;
                var outData = startPort.Owner.OutPorts.IndexOf(startPort);
                nodeLogic.ConnectInput(
                    data,
                    outData,
                    startPort.Owner.nodeLogic);
                startPort.Owner.nodeLogic.ConnectOutput(
                    outData,
                    data,
                    nodeLogic
                );
            }
        }

        void p_PortDisconnected(object sender, EventArgs e)
        {
            var port = (dynPort)sender;
            if (port.PortType == PortType.INPUT)
            {
                var data = InPorts.IndexOf(port);
                var startPort = port.Connectors[0].Start;
                nodeLogic.DisconnectInput(data);
                startPort.Owner.nodeLogic.DisconnectOutput(
                    startPort.Owner.OutPorts.IndexOf(startPort),
                    data);
            }
        }

        /// <summary>
        /// Color the connection according to it's port connectivity
        /// if all ports are connected, color green, else color orange
        /// </summary>
        public void ValidateConnections()
        {
            // if there are inputs without connections
            // mark as dead
            if (inPorts.Select(x => x).Where(x => x.PortModel.Connectors.Count == 0).Count() > 0)
            {
                State = ElementState.DEAD;
            }
            else
            {
                State = ElementState.ACTIVE;
            }
        }

        public string Description
        {
            get
            {
                Type t = NodeLogic.GetType();
                object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true);
                return ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription;
            }
        }

        public List<string> Tags
        {
            get
            {
                Type t = NodeLogic.GetType();
                object[] rtAttribs = t.GetCustomAttributes(typeof(NodeSearchTagsAttribute), true);

                if (rtAttribs.Length > 0)
                    return ((NodeSearchTagsAttribute)rtAttribs[0]).Tags;
                else
                    return new List<string>();

            }
        }

        void SetTooltip()
        {
            Type t = NodeLogic.GetType();
            object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true);
            if (rtAttribs.Length > 0)
            {
                string description = ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription;
                this.ToolTipText = description;
            }
        }

        public void SelectNeighbors()
        {
            var outConnectors = this.outPorts.SelectMany(x => x.Connectors);
            var inConnectors = this.inPorts.SelectMany(x => x.Connectors);

            foreach (dynConnector c in outConnectors)
            {
                if (!dynSettings.Workbench.Selection.Contains(c.End.Owner))
                    dynSettings.Workbench.Selection.Add(c.End.Owner);
            }

            foreach (dynConnector c in inConnectors)
            {
                if (!dynSettings.Workbench.Selection.Contains(c.Start.Owner))
                    dynSettings.Workbench.Selection.Add(c.Start.Owner);
            }
        }

        private void SetState(dynNodeUI el, ElementState state)
        {
            el.State = state;
        }

        public void Error(string p)
        {
            State = ElementState.ERROR;
            ToolTipText = p;
        }

        #region ISelectable Interface
        public void Select()
        {
            IsSelected = true;
        }

        public void Deselect()
        {
            ValidateConnections();
            this.IsSelected = false;
        }
        #endregion

        #region junk
        //public void CallUpdateLayout(FrameworkElement el)
        //{
        //    el.UpdateLayout();
        //}

        //public void SetTooltip(string message)
        //{
        //    this.ToolTipText = message;
        //}
        #endregion
    }
}

