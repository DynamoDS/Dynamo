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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
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
        ObservableCollection<dynPort> inPorts;
        ObservableCollection<dynPort> outPorts;
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
        
        public LacingStrategy ArgumentLacing { get; set; }

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

        public ObservableCollection<dynPort> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
            }
        }

        public ObservableCollection<dynPort> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
            }
        }

        public string NickName
        {
            get { return nickName; }
            set
            {
                nickName = value;
                RaisePropertyChanged("NickName");
            }
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
            ArgumentLacing = NodeLogic.ArgumentLacing;
            nodeLogic.ArgumentLacingUpdated += new Nodes.LacingTypeChangedHandler(nodeLogic_ArgumentLacingUpdated);

            inPorts = new ObservableCollection<dynPort>();
            outPorts = new ObservableCollection<dynPort>();
            inPorts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ports_collectionChanged);
            outPorts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ports_collectionChanged);
            this.IsSelected = false;

            //Binding heightBinding = new Binding("PreferredHeight");
            //topControl.SetBinding(UserControl.HeightProperty, heightBinding);

            State = ElementState.DEAD;

            //Fetch the element name from the custom attribute.
            var nameArray = nodeLogic.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true);

            if (nameArray.Length > 0)
            {
                NodeNameAttribute elNameAttrib = nameArray[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    NickName = elNameAttrib.Name;
                }
            }
            else
                NickName = "";

            DeleteCommand = new DelegateCommand(new Action(DeleteNode()), CanDeleteNode);
            SetLacingTypeCommand = new DelegateCommand<string>(new Action<string>(SetLacingType), CanSetLacingType);
        }

        void nodeLogic_ArgumentLacingUpdated(object sender, EventArgs e)
        {
            ArgumentLacing = (sender as dynNode).ArgumentLacing;
            RaisePropertyChanged("ArgumentLacing");
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

        void ports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space
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

        private void RemovePort(dynPort inport)
        {
            if (inport.PortType == PortType.INPUT)
            {
                //int index = inPorts.FindIndex(x => x == inport);
                int index = inPorts.IndexOf(inport);
                gridLeft.Children.Remove(inport);

                while (inport.Connectors.Any())
                {
                    inport.Connectors[0].Kill();
                }
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

        /// <summary>
        /// Add a dynPort element to this control.
        /// </summary>
        /// <param name="isInput">Is the port an input?</param>
        /// <param name="index">The index of the port in the port list.</param>
        public dynPort AddPort(PortType portType, string name, int index)
        {
            if (portType == PortType.INPUT)
            {
                if (inPorts.Count > index)
                {
                    return inPorts[index];
                }
                else
                {
                    dynPort p = new dynPort(index, portType, this, name);

                    InPorts.Add(p);

                    //register listeners on the port
                    p.PortConnected += new PortConnectedHandler(p_PortConnected);
                    p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);

                    return p;
                }
            }
            else if (portType == PortType.OUTPUT)
            {
                if (outPorts.Count > index)
                {
                    return outPorts[index];
                }
                else
                {
                    dynPort p = new dynPort(index, portType, this, name);

                    OutPorts.Add(p);

                    //register listeners on the port
                    p.PortConnected += new PortConnectedHandler(p_PortConnected);
                    p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);

                    return p;
                }
            }
            return null;
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
            if (inPorts.Select(x => x).Where(x => x.Connectors.Count == 0).Count() > 0)
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

