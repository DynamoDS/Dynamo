//Copyright 2012 Ian Keough

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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Expression = Dynamo.FScheme.Expression;
using Grid = System.Windows.Controls.Grid;

namespace Dynamo.Elements
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary
    public enum ElementState { DEAD, ACTIVE, SELECTED, ERROR };
    public enum LacingType { SHORTEST, LONGEST, FULL };

    #region interfaces
    public interface IDynamic
    {
        void Draw();
        void Destroy();
    }
    #endregion

    public partial class dynNode : UserControl, IDynamic, INotifyPropertyChanged
    {
        #region delegates
        public delegate void dynElementUpdatedHandler(object sender, EventArgs e);
        public delegate void dynElementDestroyedHandler(object sender, EventArgs e);
        public delegate void dynElementReadyToBuildHandler(object sender, EventArgs e);
        public delegate void dynElementReadyToDestroyHandler(object sender, EventArgs e);
        public delegate void dynElementSelectedHandler(object sender, EventArgs e);
        public delegate void dynElementDeselectedHandler(object sender, EventArgs e);
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region private members
        //System.Windows.Shapes.Ellipse dirtyEllipse;
        List<dynPort> inPorts;
        dynPort outPort;
        List<dynPort> statePorts;
        List<PortData> inPortData;
        PortData outPortData;
        List<PortData> statePortData;
        Dictionary<dynPort, TextBlock> inPortTextBlocks;
        string nickName;
        Guid guid;
        //bool isSelected = false;
        ElementState state;
        bool elementsHaveBeenDeleted = false;
        SetStateDelegate stateSetter;
        LacingType lacingType = LacingType.SHORTEST;
        #endregion

        public delegate void SetToolTipDelegate(string message);
        public delegate void MarkConnectionStateDelegate(bool bad);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);
        public delegate void SetStateDelegate(dynNode el, ElementState state);

        #region public members
        private dynWorkspace _workspace;
        public dynWorkspace WorkSpace
        {
            get
            {
                return _workspace;
            }
            internal set
            {
                _workspace = value;
            }
        }

        public dynNode TopControl
        {
            get { return topControl; }
        }

        public string ToolTipText
        {
            get { return outPortData.ToolTipString; }
        }

        public List<PortData> InPortData
        {
            get { return inPortData; }
        }

        public PortData OutPortData
        {
            get { return outPortData; }
            set { outPortData = value; }
        }

        public List<PortData> StatePortData
        {
            get { return statePortData; }
        }

        public List<dynPort> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
            }
        }

        public dynPort OutPort
        {
            get { return outPort; }
            set { outPort = value; }
        }

        public List<dynPort> StatePorts
        {
            get { return statePorts; }
            set { statePorts = value; }
        }

        public string NickName
        {
            get { return nickName; }
            set
            {
                nickName = value;
                NotifyPropertyChanged("NickName");
            }
        }

        public Guid GUID
        {
            get { return guid; }
            set { guid = value; }
        }

        private List<List<ElementId>> elements;
        public List<ElementId> Elements
        {
            get
            {
                while (elements.Count <= runCount)
                    elements.Add(new List<ElementId>());
                return elements[runCount];
            }
            private set
            {
                elements[runCount] = value;
            }
        }

        public ElementState State
        {
            get { return state; }
            set
            {
                switch (value)
                {
                    case ElementState.ACTIVE:
                        elementRectangle.Fill = dynElementSettings.SharedInstance.ActiveBrush;
                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynElementSettings.SharedInstance.ActiveBrush;
                        }

                        if (outPort != null)
                            outPort.ellipse1.Fill = dynElementSettings.SharedInstance.ActiveBrush;

                        break;
                    case ElementState.DEAD:
                        elementRectangle.Fill = dynElementSettings.SharedInstance.DeadBrush;

                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynElementSettings.SharedInstance.DeadBrush;
                        }
                        if (outPort != null)
                            outPort.ellipse1.Fill = dynElementSettings.SharedInstance.DeadBrush;
                        break;
                    case ElementState.ERROR:
                        elementRectangle.Fill = dynElementSettings.SharedInstance.ErrorBrush;

                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynElementSettings.SharedInstance.ErrorBrush;
                        }

                        if (outPort != null)
                            outPort.ellipse1.Fill = dynElementSettings.SharedInstance.ErrorBrush;

                        break;
                    case ElementState.SELECTED:
                    default:
                        elementRectangle.Fill = dynElementSettings.SharedInstance.SelectedBrush;

                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynElementSettings.SharedInstance.SelectedBrush;
                        }

                        if (outPort != null)
                            outPort.ellipse1.Fill = dynElementSettings.SharedInstance.SelectedBrush;

                        break;
                }

                if (value != ElementState.ERROR)
                {
                    SetTooltip();
                }

                state = value;
            }
        }

        public bool ElementsHaveBeenDeleted
        {
            get { return elementsHaveBeenDeleted; }
            set { elementsHaveBeenDeleted = value; }
        }

        public LacingType LacingType
        {
            get { return lacingType; }
            set { lacingType = value; }
        }

        public Grid ContentGrid
        {
            get { return inputGrid; }
        }

        protected Autodesk.Revit.UI.UIDocument UIDocument
        {
            get { return dynElementSettings.SharedInstance.Doc; }
        }

        protected dynBench Bench
        {
            get { return dynElementSettings.SharedInstance.Bench; }
        }
        #endregion

        #region events
        //public event dynElementUpdatedHandler dynElementUpdated;
        public event dynElementDestroyedHandler dynElementDestroyed;
        //public event dynElementReadyToBuildHandler dynElementReadyToBuild;
        public event dynElementReadyToDestroyHandler dynElementReadyToDestroy;
        //public event dynElementSelectedHandler dynElementSelected;
        //public event dynElementDeselectedHandler dynElementDeselected;
        #endregion

        #region constructors
        /// <summary>
        /// dynElement constructor for use by workbench in creating dynElements
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nickName"></param>
        public dynNode()
        {
            InitializeComponent();
            //System.Uri resourceLocater = new System.Uri("/DynamoElements;component/dynElement.xaml", UriKind.Relative);
            //System.Uri resourceLocater = new System.Uri("/dynElement.xaml", UriKind.Relative);
            //System.Windows.Application.LoadComponent(this, resourceLocater);

            //set the main grid's data context to 
            //this element
            nickNameBlock.DataContext = this;

            inPorts = new List<dynPort>();
            inPortData = new List<PortData>();
            statePorts = new List<dynPort>();
            statePortData = new List<PortData>();
            elements = new List<List<ElementId>>() { new List<ElementId>() };
            inPortTextBlocks = new Dictionary<dynPort, TextBlock>();

            stateSetter = new SetStateDelegate(SetState);
            State = ElementState.DEAD;

            //Fetch the element name from the custom attribute.
            var nameArray = GetType().GetCustomAttributes(typeof(ElementNameAttribute), true);

            if (nameArray.Length > 0)
            {
                ElementNameAttribute elNameAttrib = nameArray[0] as ElementNameAttribute;
                if (elNameAttrib != null)
                {
                    NickName = elNameAttrib.ElementName;
                }
            }
            else
                NickName = "";

            //set the z index to 2
            Canvas.SetZIndex(this, 1);

            //Canvas.SetZIndex(this.gridLeft, 40);
            //Canvas.SetZIndex(this.gridRight, 40);

            //dirtyEllipse = new System.Windows.Shapes.Ellipse();
            //dirtyEllipse.Height = 20;
            //dirtyEllipse.Width = 20;
            //dirtyEllipse.Fill = Brushes.Red;
            //elementCanvas.Children.Add(dirtyEllipse);
            //Canvas.SetBottom(dirtyEllipse, 10);
            //Canvas.SetRight(dirtyEllipse, 10);
            //Canvas.SetZIndex(dirtyEllipse, 100);

            //dynElementReadyToBuild += new dynElementReadyToBuildHandler(Build);
        }
        #endregion

        protected virtual void OnDynElementDestroyed(EventArgs e)
        {
            if (dynElementDestroyed != null)
            {
                dynElementDestroyed(this, e);
            }
        }

        //protected virtual void OnDynElementReadyToBuild(EventArgs e)
        //{
        //    if (dynElementReadyToBuild != null)
        //    {
        //        dynElementReadyToBuild(this, e);
        //    }
        //}

        protected virtual void OnDynElementReadyToDestroy(EventArgs e)
        {
            if (dynElementReadyToDestroy != null)
            {
                dynElementReadyToDestroy(this, e);
            }
        }

        //protected virtual void OnDynElementSelected(EventArgs e)
        //{
        //    if (dynElementSelected != null)
        //    {
        //        dynElementSelected(this, e);
        //    }
        //}

        //protected virtual void OnDynElementDeselected(EventArgs e)
        //{
        //    if (dynElementDeselected != null)
        //    {
        //        dynElementDeselected(this, e);
        //    }
        //}

        public void RegisterInputsAndOutputs()
        {
            ResizeElementForInputs();
            SetupPortGrids();
            RegisterInputs();
            RegisterOutputs();

            UpdateLayout();

            SetToolTips();
            ValidateConnections();
        }

        public void ReregisterInputs()
        {
            ResizeElementForInputs();
            SetupPortGrids();
            RegisterInputs();

            UpdateLayout();

            SetToolTips();
            ValidateConnections();
            UpdateConnections();
        }

        void UpdateConnections()
        {
            foreach (var p in InPorts)
                p.Update();
            OutPort.Update();
        }

        private Dictionary<UIElement, bool> enabledDict = new Dictionary<UIElement, bool>();

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
        /// Creates a Scheme representation of this dynElement and all connected dynElements.
        /// </summary>
        /// <returns>S-Expression</returns>
        public virtual string PrintExpression()
        {
            var nick = NickName.Replace(' ', '_');

            if (!InPortData.Any() || !InPorts.Any(x => x.Connectors.Any()))
                return nick;

            string s = "";

            if (InPorts.All(x => x.Connectors.Any()))
            {
                s += "(" + nick;
                for (int i = 0; i < InPortData.Count; i++)
                {
                    var port = InPorts[i];
                    s += " " + port.Connectors[0].Start.Owner.PrintExpression();
                }
                s += ")";
            }
            else
            {
                s += "(lambda ("
                   + string.Join(" ", InPortData.Where((x, i) => !InPorts[i].Connectors.Any()).Select(x => x.NickName))
                   + ") (" + nick;
                for (int i = 0; i < InPortData.Count; i++)
                {
                    s += " ";
                    var port = InPorts[i];
                    if (port.Connectors.Any())
                        s += port.Connectors[0].Start.Owner.PrintExpression();
                    else
                        s += InPortData[i].NickName;
                }
                s += "))";
            }

            return s;
        }

        protected internal virtual bool RequiresManualTransaction()
        {
            return InPorts.Any(
               x =>
                  x.Connectors.Any() && x.Connectors[0].Start.Owner.RequiresManualTransaction()
            );
        }

        protected internal virtual bool RequiresTransaction()
        {
            object[] attribs = GetType().GetCustomAttributes(typeof(RequiresTransactionAttribute), false);

            return (attribs.Length > 0 && (attribs[0] as RequiresTransactionAttribute).RequiresTransaction)
               || InPorts.Any(
                     x =>
                        x.Connectors.Any() && x.Connectors[0].Start.Owner.RequiresTransaction()
                  );
        }

        /// <summary>
        /// Resize the control based on the number of inputs.
        /// </summary>
        public void ResizeElementForInputs()
        {
            //HACK
            //We don't want to remove these grids for the Instance Parameter mapper
            //because it will need them later
            //if (GetType() != typeof(dynInstanceParameterMapper))
            //{
            //size the height of the controller based on the 
            //whichever is larger the inport or the outport list
            topControl.Height = Math.Max(inPortData.Count, 1) * 20 + 10; //spacing for inputs + title space + bottom space
            //grid.Children.Remove(gridBottom);
            //grid.Children.Remove(portNamesBottom);

            Thickness leftGridThick = new Thickness(gridLeft.Margin.Left, gridLeft.Margin.Top, gridLeft.Margin.Right, 5);
            gridLeft.Margin = leftGridThick;
            //Thickness leftNamesThick = new Thickness(portNamesLeft.Margin.Left, portNamesLeft.Margin.Top, portNamesLeft.Margin.Right, 5);
            //portNamesLeft.Margin = leftNamesThick;

            //Thickness rightGridThick = new Thickness(gridRight.Margin.Left, gridRight.Margin.Top, gridRight.Margin.Right, 5);
            //gridRight.Margin = rightGridThick;
            //Thickness rightNamesThick = new Thickness(portNamesRight.Margin.Left, portNamesRight.Margin.Top, portNamesRight.Margin.Right, 5);
            //portNamesRight.Margin = rightNamesThick;

            Thickness inputGridThick = new Thickness(inputGrid.Margin.Left, inputGrid.Margin.Top, inputGrid.Margin.Right, 5);
            inputGrid.Margin = inputGridThick;

            grid.UpdateLayout();
            //}
            //else
            //{
            //   topControl.Height = Math.Max(inPortData.Count, outPortData.Count) * 20 + 10; //spacing for inputs + title space 
            //}

            if (inputGrid.Children.Count == 0)
            {
                //decrease the width of the node because
                //there's nothing in the middle grid
                topControl.Width = 100;
            }
            else
                topControl.Width = Math.Max(200, StatePortData.Count * 20) + 10;


        }

        /// <summary>
        /// Sets up the control's grids based on numbers of input and output ports.
        /// </summary>
        public void SetupPortGrids()
        {
            //add one column for inputs
            //ColumnDefinition cdIn = new ColumnDefinition();
            //cdIn.Width = GridLength.Auto;
            //gridLeft.ColumnDefinitions.Add(cdIn);

            //ColumnDefinition namesIn = new ColumnDefinition();
            //namesIn.Width = GridLength.Auto;
            //portNamesLeft.ColumnDefinitions.Add(namesIn);

            ////add one column for outputs
            //ColumnDefinition cdOut = new ColumnDefinition();
            //cdOut.Width = GridLength.Auto;
            //gridLeft.ColumnDefinitions.Add(cdOut);

            //ColumnDefinition namesOut = new ColumnDefinition();
            //namesOut.Width = GridLength.Auto;
            //portNamesRight.ColumnDefinitions.Add(namesOut);

            ////add one row for state ports
            //RowDefinition rdState = new RowDefinition();
            //rdState.Height = GridLength.Auto;
            //gridBottom.RowDefinitions.Add(rdState);

            //RowDefinition namesState = new RowDefinition();
            //namesState.Height = GridLength.Auto;
            //portNamesBottom.RowDefinitions.Add(namesState);

            int count = 0;
            int numRows = gridLeft.RowDefinitions.Count;
            foreach (object input in InPortData)
            {
                if (count++ < numRows)
                    continue;

                RowDefinition rd = new RowDefinition();
                gridLeft.RowDefinitions.Add(rd);

                //RowDefinition nameRd = new RowDefinition();
                //portNamesLeft.RowDefinitions.Add(nameRd);
            }

            if (count < numRows)
            {
                gridLeft.RowDefinitions.RemoveRange(count, numRows - count);
            }

            count = 0;
            numRows = gridRight.RowDefinitions.Count;
            if (numRows == 0)
            {
                gridRight.RowDefinitions.Add(new RowDefinition());
            }

            //foreach (object output in OutPortData)
            //{
            //   if (count++ < numRows)
            //      continue;

            //   RowDefinition rd = new RowDefinition();
            //   gridRight.RowDefinitions.Add(rd);

            //   //RowDefinition nameRd = new RowDefinition();
            //   //portNamesRight.RowDefinitions.Add(nameRd);
            //}

            if (numRows > 1)
            {
                gridRight.RowDefinitions.RemoveRange(count, numRows - 1);
            }

            //foreach(object state in StatePortData)
            //{
            //    ColumnDefinition cd = new ColumnDefinition();
            //    gridBottom.ColumnDefinitions.Add(cd);

            //    ColumnDefinition nameCd = new ColumnDefinition();
            //    portNamesBottom.ColumnDefinitions.Add(nameCd);
            //}
        }

        /// <summary>
        /// Reads inputs list and adds ports for each input.
        /// </summary>
        public void RegisterInputs()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in InPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                AddPort(PortType.INPUT, inPortData[count].NickName, count);
                count++;
            }

            if (inPorts.Count > count)
            {
                foreach (var inport in inPorts.Skip(count))
                {
                    RemovePort(inport);
                }

                inPorts.RemoveRange(count, inPorts.Count - count);
            }
        }

        private void RemovePort(dynPort inport)
        {
            if (inport.PortType == PortType.INPUT)
            {
                int index = inPorts.FindIndex(x => x == inport);

                gridLeft.Children.Remove(inport);
                gridLeft.Children.Remove(inPortTextBlocks[inport]);

                while (inport.Connectors.Any())
                {
                    inport.Connectors[0].Kill();
                }
            }
        }

        public void RegisterStatePorts()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in StatePortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                AddPort(PortType.STATE, statePortData[count].NickName, count);
                count++;
            }
        }

        /// <summary>
        /// Reads outputs list and adds ports for each output
        /// </summary>
        public void RegisterOutputs()
        {
            //outPorts.Clear();

            //read the outputs list and create a number of 
            //output ports
            //int count = 0;
            //foreach (PortData pd in OutPortData)
            //{
            //   //add a port for each output
            //   //pass in the y center of the port
            //   AddPort(pd.Object, PortType.OUTPUT, outPortData[count].NickName, count);
            //   count++;
            //}

            //AddPort(OutPortData.Object, PortType.OUTPUT, OutPortData.NickName, 0);

            dynPort p = new dynPort(0);

            //create a text block for the name of the port
            TextBlock tb = new TextBlock();

            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.FontSize = 12;
            tb.FontWeight = FontWeights.Normal;
            tb.Foreground = new SolidColorBrush(Colors.Black);
            tb.Text = OutPortData.NickName;

            tb.HorizontalAlignment = HorizontalAlignment.Right;

            ScaleTransform trans = new ScaleTransform(-1, 1, p.Width/2, p.Height/2);
            p.RenderTransform = trans;

            p.PortType = PortType.OUTPUT;
            outPort = p;
            gridRight.Children.Add(p);
            Grid.SetColumn(p, 1);
            Grid.SetRow(p, 0);

            //portNamesRight.Children.Add(tb);
            gridRight.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            p.Owner = this;

            //register listeners on the port
            p.PortConnected += new PortConnectedHandler(p_PortConnected);
            p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
        }

        public void SetToolTips()
        {
            //set all the tooltips
            int count = 0;
            foreach (dynPort p in inPorts)
            {
                //get the types from the types list
                p.toolTipText.Text = p.Owner.InPortData[count].ToolTipString;
                count++;
            }

            outPort.toolTipText.Text = outPort.Owner.OutPortData.ToolTipString;
        }

        /// <summary>
        /// Add a dynPort element to this control.
        /// </summary>
        /// <param name="isInput">Is the port an input?</param>
        /// <param name="index">The index of the port in the port list.</param>
        public void AddPort(PortType portType, string name, int index)
        {
            if (portType == PortType.INPUT)
            {
                if (inPorts.Count > index)
                {
                    inPortTextBlocks[inPorts[index]].Text = name;
                }
                else
                {
                    dynPort p = new dynPort(index);

                    //create a text block for the name of the port
                    TextBlock tb = new TextBlock();

                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.FontSize = 12;
                    tb.FontWeight = FontWeights.Normal;
                    tb.Foreground = new SolidColorBrush(Colors.Black);
                    tb.Text = name;

                    tb.HorizontalAlignment = HorizontalAlignment.Left;

                    Canvas.SetZIndex(tb, 200);

                    p.PortType = PortType.INPUT;
                    inPorts.Add(p);
                    inPortTextBlocks[p] = tb;
                    gridLeft.Children.Add(p);
                    Grid.SetColumn(p, 0);
                    Grid.SetRow(p, index);

                    //portNamesLeft.Children.Add(tb);
                    gridLeft.Children.Add(tb);
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, index);
                    
                    p.Owner = this;

                    //register listeners on the port
                    p.PortConnected += new PortConnectedHandler(p_PortConnected);
                    p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
                }
            }
        }

        Dictionary<dynPort, dynNode> previousEvalPortMappings = new Dictionary<dynPort, dynNode>();

        void CheckPortsForRecalc()
        {
            IsDirty = InPorts.Any(
               delegate(dynPort port)
               {
                   dynNode oldInput;
                   var connectors = port.Connectors;
                   return !previousEvalPortMappings.TryGetValue(port, out oldInput)
                      || (oldInput == null && connectors.Any())
                      || (connectors.Any() && oldInput != port.Connectors[0].Start.Owner);
               }
            );
        }

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
                CheckPortsForRecalc();
        }

        void p_PortDisconnected(object sender, EventArgs e)
        {
            ValidateConnections();

            var port = (dynPort)sender;
            if (port.PortType == PortType.INPUT)
                CheckPortsForRecalc();
        }

        /// <summary>
        /// Color the connection according to it's port connectivity
        /// if all ports are connected, color green, else color orange
        /// </summary>
        public void ValidateConnections()
        {
            bool flag = false;

            foreach (dynPort port in inPorts)
            {
                if (port.Connectors.Count == 0)
                {
                    flag = true;
                }
            }

            if (flag)
            {
                State = ElementState.DEAD;
                //Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.DEAD });
            }
            else
            {
                State = ElementState.ACTIVE;
                //Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ACTIVE });
            }
        }

        protected void MarkConnectionState(bool bad)
        {
            //don't change the color of the object
            //if it's selected
            if (state != ElementState.SELECTED)
            {
                if (bad)
                {
                    Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ERROR });
                }
                else
                {
                    Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ACTIVE });
                }
            }
        }

        /// <summary>
        /// Override this to implement custom save data for your Element. If overridden, you should also override
        /// LoadElement() in order to read the data back when loaded.
        /// </summary>
        /// <param name="xmlDoc">The XmlDocument representing the whole workspace containing this Element.</param>
        /// <param name="dynEl">The XmlElement representing this Element.</param>
        public virtual void SaveElement(System.Xml.XmlDocument xmlDoc, System.Xml.XmlElement dynEl)
        {

        }

        /// <summary>
        /// Override this to implement loading of custom data for your Element. If overridden, you should also override
        /// SaveElement() in order to write the data when saved.
        /// </summary>
        /// <param name="elNode">The XmlNode representing this Element.</param>
        public virtual void LoadElement(System.Xml.XmlNode elNode)
        {

        }

        /// <summary>
        /// Is this node an entry point to the program?
        /// </summary>
        public bool IsTopmost
        {
            get
            {
                return OutPort == null || !OutPort.Connectors.Any();
            }
        }

        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        protected internal int runCount;

        internal void ResetRuns()
        {
            if (runCount > 0)
            {
                PruneRuns();
                runCount = 0;
            }
        }

        void PruneRuns()
        {
            for (int i = elements.Count - 1; i >= runCount; i--)
            {
                var elems = elements[i];
                foreach (var e in elems)
                {
                    UIDocument.Document.Delete(e);
                }
                elems.Clear();
            }

            if (elements.Count > runCount)
            {
                elements.RemoveRange(
                   runCount,
                   elements.Count - runCount
                );
            }
        }

        protected internal static HashSet<string> _taggedSymbols = new HashSet<string>();
        protected internal static bool _startTag = false;
        //private bool __isDirty = true;
        protected internal bool _isDirty = true;
        //{
        //    get { return __isDirty; }
        //    set
        //    {
        //        __isDirty = value;
        //        Dispatcher.BeginInvoke(new Action(() => dirtyEllipse.Fill = __isDirty ? Brushes.Red : Brushes.Green));
        //    }
        //}
        ///<summary>
        ///Does this Element need to be regenerated? Setting this to true will trigger a modification event
        ///for the dynWorkspace containing it. If Automatic Running is enabled, setting this to true will
        ///trigger an evaluation.
        ///</summary>
        public virtual bool IsDirty
        {
            get
            {
                if (_isDirty)
                    return true;
                else
                {
                    //TODO: move this entirely to dynFunction?
                    bool start = _startTag;
                    _startTag = true;

                    bool dirty = InPorts.Any(x => x.Connectors.Any(y => y.Start.Owner.IsDirty));
                    _isDirty = dirty;

                    if (!start)
                    {
                        _startTag = false;
                        _taggedSymbols.Clear();
                    }

                    return dirty;
                }
            }
            set
            {
                _isDirty = value;
                if (value && _report && WorkSpace != null)
                    WorkSpace.Modified();
            }
        }

        private bool _report = true;

        protected Value oldValue;

        private bool _saveResult = false;
        /// <summary>
        /// Determines whether or not the output of this Element will be saved. If true, Evaluate() will not be called
        /// unless IsDirty is true. Otherwise, Evaluate will be called regardless of the IsDirty value.
        /// </summary>
        internal bool SaveResult
        {
            get
            {
                return _saveResult
                   && InPorts.All(x => x.Connectors.Any());
            }
            set
            {
                _saveResult = value;
            }
        }

        /// <summary>
        /// Forces the node to refresh it's dirty state by checking all inputs.
        /// </summary>
        public void MarkDirty()
        {
            bool dirty = false;
            foreach (var p in InPorts)
            {
                foreach (var c in p.Connectors)
                {
                    var input = c.Start.Owner;
                    input.MarkDirty();
                    if (c.Start.Owner.IsDirty)
                        dirty = true;
                }
            }
            if (!_isDirty)
                _isDirty = dirty;
            return;
        }

        /// <summary>
        /// Called when the node's workspace has been saved.
        /// </summary>
        protected internal virtual void OnSave() { }

        internal void onSave()
        {
            //Save all of the connection states, so we can check if this is dirty
            foreach (dynPort p in InPorts)
            {
                previousEvalPortMappings[p] = p.Connectors.Any()
                   ? p.Connectors[0].Start.Owner
                   : null;
            }
            OnSave();
        }

        internal virtual INode BuildExpression()
        {
            return Build(new Dictionary<dynNode, INode>());
        }

        /// <summary>
        /// Builds an INode out of this Element. Override this or Compile() if you want complete control over this Element's
        /// execution.
        /// </summary>
        /// <returns>The INode representation of this Element.</returns>
        protected internal virtual INode Build(Dictionary<dynNode, INode> preBuilt)
        {
            INode result;
            if (preBuilt.TryGetValue(this, out result))
                return result;

            //Fetch the names of input ports.
            var portNames = InPortData.Select(x => x.NickName);

            //Compile the procedure for this node.
            InputNode node = Compile(portNames);

            //Is this a partial application?
            var partial = false;

            //For each index in InPortData
            for (int i = 0; i < InPortData.Count; i++)
            {
                //Fetch the corresponding port
                var port = InPorts[i];

                //If this port has connectors...
                if (port.Connectors.Any())
                {
                    //Fetch the corresponding info for the port.
                    var data = InPortData[i];

                    //Compile input and connect it
                    node.ConnectInput(
                       data.NickName,
                       port.Connectors[0].Start.Owner.Build(preBuilt)
                    );
                }
                else //othwise, remember that this is a partial application
                    partial = true;
            }

            //If this is a partial application, then remember not to re-eval.
            if (partial)
            {
                IsDirty = false;
                OnEvaluate();
            }

            result = node;
            
            preBuilt[this] = result;

            //And we're done
            return result;
        }

        /// <summary>
        /// Compiles this Element into a InputNode. Override this instead of Build() if you don't want to set up all
        /// of the inputs for the InputNode.
        /// </summary>
        /// <param name="portNames">The names of the inputs to the node.</param>
        /// <returns>A InputNode which will then be processed recursively to be connected to its inputs.</returns>
        protected internal virtual InputNode Compile(IEnumerable<string> portNames)
        {
            //Return a Function that calls eval.
            return new ExternalFunctionNode(evalIfDirty, portNames);
        }

        /// <summary>
        /// Called right before Evaluate() is called. Useful for processing side-effects without touching Evaluate()
        /// </summary>
        protected virtual void OnEvaluate() { }

        protected internal ExecutionEnvironment macroEnvironment = null;

        private Value evalIfDirty(FSharpList<Value> args)
        {
            if (SaveResult)
            {
                //If this node requires a re-calc or if we haven't calc'd yet...
                if (IsDirty || oldValue == null)
                    //Evaluate arguments, then evaluate 
                    oldValue = evaluateNode(args);
                else //Otherwise, just increment the run counter.
                    runCount++;

                //We're done here
                return oldValue;
            }
            else
                return evaluateNode(args);
        }

        protected internal Value evaluateNode(FSharpList<Value> args)
        {
            //For convenience, store the bench.
            var bench = dynElementSettings.SharedInstance.Bench;

            if (SaveResult)
            {
                //Store the port mappings for this evaluate. We will compare later to see if it is dirty;
                foreach (dynPort p in InPorts)
                {
                    previousEvalPortMappings[p] = p.Connectors.Any()
                       ? p.Connectors[0].Start.Owner
                       : null;
                }
            }

            object[] rtAttribs = GetType().GetCustomAttributes(typeof(RequiresTransactionAttribute), false);
            bool useTransaction = rtAttribs.Length > 0 && ((RequiresTransactionAttribute)rtAttribs[0]).RequiresTransaction;

            object[] iaAttribs = GetType().GetCustomAttributes(typeof(IsInteractiveAttribute), false);
            bool isInteractive = iaAttribs.Length > 0 && ((IsInteractiveAttribute)iaAttribs[0]).IsInteractive;

            Value result = null;

            Action evaluation = delegate
            {
                if (bench.RunCancelled)
                    throw new CancelEvaluationException(false);

                bool debug = bench.RunInDebug;

                OnEvaluate();

                if (useTransaction)
                {
                    #region using transaction

                    if (!debug)
                    {
                        #region no debug

                        try
                        {
                            if (bench.TransMode == TransactionMode.Manual && !bench.IsTransactionActive())
                            {
                                var msg = "A Revit transaction is required in order evaluate this element.";
                                Error(msg);
                                throw new Exception(msg);
                            }

                            bench.InitTransaction();

                            result = Evaluate(args);

                            foreach (ElementId eid in deletedIds)
                            {
                                Bench.RegisterSuccessfulDeleteHook(
                                   eid,
                                   onSuccessfulDelete
                                );
                            }
                            deletedIds.Clear();

                            UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                            Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

                            elementsHaveBeenDeleted = false;
                            ValidateConnections();
                        }
                        catch (CancelEvaluationException ex)
                        {
                            OnRunCancelled();
                            throw ex;
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(
                               delegate
                               {
                                   Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                                   bench.Log(ex);
                                   bench.ShowElement(this);

                                   dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                                   dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                               }
                            ));

                            Error(ex.Message);
                        }

                        #endregion
                    }
                    else
                    {
                        #region debug

                        Dispatcher.Invoke(new Action(
                           () =>
                              bench.Log("Starting a debug transaction for element: " + NickName)
                        ));

                        result = IdlePromise<Value>.ExecuteOnIdle(
                           delegate
                           {
                               bench.InitTransaction();

                               try
                               {
                                   var exp = Evaluate(args);

                                   foreach (ElementId eid in deletedIds)
                                   {
                                       Bench.RegisterSuccessfulDeleteHook(
                                          eid,
                                          onSuccessfulDelete
                                       );
                                   }
                                   deletedIds.Clear();

                                   UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                                   Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

                                   elementsHaveBeenDeleted = false;

                                   bench.EndTransaction();

                                   Dispatcher.BeginInvoke(new Action(
                                       () => ValidateConnections()
                                   ));

                                   return exp;
                               }
                               catch (CancelEvaluationException ex)
                               {
                                   OnRunCancelled();
                                   throw ex;
                               }
                               catch (Exception ex)
                               {
                                   Dispatcher.Invoke(new Action(
                                      delegate
                                      {
                                          Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                                          bench.Log(ex);
                                          bench.ShowElement(this);
                                      }
                                   ));

                                   Error(ex.Message);

                                   bench.CancelTransaction();

                                   Dispatcher.Invoke(new Action(
                                      delegate
                                      {
                                          dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                                          dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                                      }
                                   ));

                                   return null;
                               }
                           }
                        );

                        #endregion
                    }

                    #endregion
                }
                else
                {
                    #region no transaction

                    try
                    {
                        result = Evaluate(args);

                        UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                        Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

                        elementsHaveBeenDeleted = false;

                        Dispatcher.BeginInvoke(new Action(
                            () => ValidateConnections()
                        ));
                    }
                    catch (CancelEvaluationException ex)
                    {
                        OnRunCancelled();
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(
                           delegate
                           {
                               Debug.WriteLine(ex.Message + " : " + ex.StackTrace);

                               bench.Log(ex);
                               bench.ShowElement(this);

                               dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                               dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                           }
                        ));

                        Error(ex.Message);
                    }

                    #endregion
                }

                #region Register Elements w/ DMU

                var del = new DynElementUpdateDelegate(onDeleted);

                foreach (ElementId id in Elements)
                    Bench.RegisterDeleteHook(id, del);

                #endregion

                //Increment the run counter
                runCount++;

                IsDirty = false;
            };

            if (isInteractive)
                Dispatcher.Invoke(evaluation);
            else
                evaluation();

            if (result != null)
                return result;
            else
                throw new Exception("");
        }

        protected virtual void OnRunCancelled()
        {
            
        }

        private List<ElementId> deletedIds = new List<ElementId>();
        protected void DeleteElement(ElementId id, bool hookOnly = false)
        {
            if (!hookOnly)
                UIDocument.Document.Delete(id);
            deletedIds.Add(id);
        }

        /// <summary>
        /// The dynElement's Evaluation Logic.
        /// </summary>
        /// <param name="args">Arguments to the node. You are guaranteed to have as many arguments as you have InPorts at the time it is run.</param>
        /// <returns>An expression that is the result of the Node's evaluation. It will be passed along to whatever the OutPort is connected to.</returns>
        public virtual Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }

        void onDeleted(List<ElementId> deleted)
        {
            int count = 0;
            foreach (var els in elements)
            {
                count += els.RemoveAll(x => deleted.Contains(x));
            }

            if (!_isDirty)
                _isDirty = count > 0;
        }

        /// <summary>
        /// Registers the given element id with the DMU such that any change in the element will
        /// trigger a workspace modification event (dynamic running and saving).
        /// </summary>
        /// <param name="id">ElementId of the element to watch.</param>
        public void RegisterEvalOnModified(ElementId id, Action modAction = null, Action delAction = null)
        {
            var u = Bench.Updater;
            u.RegisterChangeHook(
               id,
               ChangeTypeEnum.Modify,
               ReEvalOnModified(modAction)
            );
            u.RegisterChangeHook(
               id,
               ChangeTypeEnum.Delete,
               UnRegOnDelete(delAction)
            );
        }

        /// <summary>
        /// Unregisters the given element id with the DMU. Should not be called unless it has already
        /// been registered with RegisterEvalOnModified
        /// </summary>
        /// <param name="id">ElementId of the element to stop watching.</param>
        public void UnregisterEvalOnModified(ElementId id)
        {
            var u = Bench.Updater;
            u.UnRegisterChangeHook(
               id, ChangeTypeEnum.Modify
            );
            u.UnRegisterChangeHook(
               id, ChangeTypeEnum.Delete
            );
        }

        DynElementUpdateDelegate UnRegOnDelete(Action deleteAction)
        {
            return delegate(List<ElementId> deleted)
            {
                foreach (var d in deleted)
                {
                    var u = Bench.Updater;
                    u.UnRegisterChangeHook(d, ChangeTypeEnum.Delete);
                    u.UnRegisterChangeHook(d, ChangeTypeEnum.Modify);
                }
                if (deleteAction != null)
                    deleteAction();
            };
        }

        DynElementUpdateDelegate ReEvalOnModified(Action modifiedAction)
        {
            return delegate(List<ElementId> modified)
            {
                if (!IsDirty && !Bench.Running)
                {
                    if (modifiedAction != null)
                        modifiedAction();
                    IsDirty = true;
                }
            };
        }

        void onSuccessfulDelete(List<ElementId> deleted)
        {
            foreach (var els in elements)
                els.RemoveAll(x => deleted.Contains(x));
        }

        protected internal void SetColumnAmount(int amt)
        {
            int count = inputGrid.ColumnDefinitions.Count;
            if (count == amt)
                return;
            else if (count < amt)
            {
                int diff = amt - count;
                for (int i = 0; i < diff; i++)
                    inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            else
            {
                int diff = count - amt;
                inputGrid.ColumnDefinitions.RemoveRange(amt, diff);
            }
        }

        protected internal void SetRowAmount(int amt)
        {
            int count = inputGrid.RowDefinitions.Count;
            if (count == amt)
                return;
            else if (count < amt)
            {
                int diff = amt - count;
                for (int i = 0; i < diff; i++)
                    inputGrid.RowDefinitions.Add(new RowDefinition());
            }
            else
            {
                int diff = count - amt;
                inputGrid.RowDefinitions.RemoveRange(amt, diff);
            }
        }

        public void CallUpdateLayout(FrameworkElement el)
        {
            el.UpdateLayout();
        }

        public void SetTooltip(string message)
        {
            ToolTip = message;
        }

        void SetTooltip()
        {
            object[] rtAttribs = GetType().GetCustomAttributes(typeof(ElementDescriptionAttribute), false);
            if (rtAttribs.Length > 0)
            {
                string description = ((ElementDescriptionAttribute)rtAttribs[0]).ElementDescription;
                ToolTip = description;
            }
        }

        public virtual void Draw()
        {
        }

        /// <summary>
        /// Destroy all elements belonging to this dynElement
        /// </summary>
        public virtual void Destroy()
        {
            runCount = 0;
            foreach (var els in elements)
            {
                foreach (ElementId e in els)
                {
                    try
                    {
                        dynElementSettings.SharedInstance.Doc.Document.Delete(e);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        //TODO: Flesh out?
                    }
                }
                //els.Clear();
            }

            //clear out the array to avoid object initialization errors
            elements.Clear();

            //clear the data tree
            //dataTree.Clear();
        }

        public virtual void Update()
        {
        }

        public void UpdateOutputs()
        {
            //send the messages without updating
            foreach (dynConnector c in outPort.Connectors)
            {
                c.SendMessage();
            }

            //aggregate the unique output nodes
            //this avoids multiple updates of the same node
            //if a node has the same node connected to several of its outputs
            var uniqueNodes = new HashSet<dynNode>();
            foreach (dynConnector c in outPort.Connectors)
            {
                //if (!uniqueNodes.Contains(c.End.Owner))
                //{
                uniqueNodes.Add(c.End.Owner);
                //}
            }

            //update the unique nodes
            foreach (dynNode el in uniqueNodes)
            {
                el.Update();
            }
        }

        public void FindDownstreamElements(ref List<dynNode> downStream)
        {
            foreach (dynConnector c in outPort.Connectors)
            {
                if (!downStream.Contains(c.End.Owner))
                {
                    //don't add it if it's already there
                    downStream.Add(c.End.Owner);
                }

                //set a flag on the element to say 
                //that it has already processed its downstream geometry
                c.End.Owner.ElementsHaveBeenDeleted = true;
                c.End.Owner.FindDownstreamElements(ref downStream);
            }
        }

        public void ClearOutputs()
        {
            foreach (dynConnector c in outPort.Connectors)
            {
                c.Kill();
            }

            outPort = null;
            outPortData = null;
        }

        public bool CheckInputs()
        {
            var mcsd = new MarkConnectionStateDelegate(MarkConnectionState);

            foreach (PortData pd in InPortData)
            {
                //if the port data's object is null
                //or the port data's object type can not be matched
                Dispatcher.Invoke(
                   mcsd,
                   System.Windows.Threading.DispatcherPriority.Background,
                   new object[] { true }
                );

                SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
                Dispatcher.Invoke(
                   sttd, System.Windows.Threading.DispatcherPriority.Background,
                   new object[] { "One or more connections is null." }
                );

                return false;
            }


            Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
               new object[] { false });

            return true;
        }

        public void Select()
        {
            Dispatcher.Invoke(
               stateSetter,
               System.Windows.Threading.DispatcherPriority.Background,
               new object[] { this, ElementState.SELECTED }
            );
        }

        public void Deselect()
        {
            ValidateConnections();
        }

        void SetState(dynNode el, ElementState state)
        {
            el.State = state;
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

        private void longestList_cm_Click(object sender, RoutedEventArgs e)
        {
            lacingType = LacingType.LONGEST;
            //fullLace_cm.IsChecked = false;
            //shortestList_cm.IsChecked = false;
            //longestList_cm.IsChecked = true;
        }

        private void shortestList_cm_Click(object sender, RoutedEventArgs e)
        {
            lacingType = LacingType.SHORTEST;
            //fullLace_cm.IsChecked = false;
            //shortestList_cm.IsChecked = true;
            //longestList_cm.IsChecked = false;
        }

        private void fullLace_cm_Click(object sender, RoutedEventArgs e)
        {
            lacingType = LacingType.FULL;
            //fullLace_cm.IsChecked = true;
            //shortestList_cm.IsChecked = false;
            //longestList_cm.IsChecked = false;
        }

        private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void deleteElem_cm_Click(object sender, RoutedEventArgs e)
        {
            DisableReporting();
            var bench = dynElementSettings.SharedInstance.Bench;

            IdlePromise.ExecuteOnIdle(
               delegate
               {
                   bench.InitTransaction();
                   try
                   {
                       Destroy();
                   }
                   catch (Exception ex)
                   {
                       bench.Log("Error deleting elements:");
                       bench.Log(ex);
                   }
                   bench.EndTransaction();

                   bench.DeleteElement(this);
                   WorkSpace.Modified();
               },
               true
            );
        }

        internal void DisableReporting()
        {
            _report = false;
        }

        internal void EnableReporting()
        {
            _report = true;
        }

        internal void Error(string p)
        {
            MarkConnectionState(true);

            SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
            Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                new object[] { p });
        }

        private void topControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("I am clicking on the node. Beatch.");
        }
    }

    [ValueConversion(typeof(double), typeof(Thickness))]
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double height = (double)value;
            return new Thickness(0, -1 * height - 3, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    #region class attributes
    [AttributeUsage(AttributeTargets.Class)]
    public class IsConstantAttribute : System.Attribute
    {
        public bool IsConstant;

        public IsConstantAttribute(bool constant)
        {
            IsConstant = constant;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ElementNameAttribute : System.Attribute
    {
        public string ElementName { get; set; }

        public ElementNameAttribute(string elementName)
        {
            ElementName = elementName;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ElementCategoryAttribute : System.Attribute
    {
        public string ElementCategory { get; set; }

        public ElementCategoryAttribute(string category)
        {
            ElementCategory = category;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ElementSearchTagsAttribute : System.Attribute
    {
        public List<string> Tags { get; set; }

        public ElementSearchTagsAttribute(params string[] tags)
        {
            Tags = tags.ToList();
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class IsInteractiveAttribute : System.Attribute
    {
        public bool IsInteractive { get; set; }

        public IsInteractiveAttribute(bool isInteractive)
        {
            IsInteractive = isInteractive;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class RequiresTransactionAttribute : System.Attribute
    {
        public bool RequiresTransaction { get; set; }

        public RequiresTransactionAttribute(bool required)
        {
            RequiresTransaction = required;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ElementDescriptionAttribute : System.Attribute
    {
        public string ElementDescription { get; set; }

        public ElementDescriptionAttribute(string description)
        {
            ElementDescription = description;
        }
    }
    #endregion
}
