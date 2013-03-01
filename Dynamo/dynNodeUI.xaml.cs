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
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Grid = System.Windows.Controls.Grid;
using System.Windows.Threading;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary
    public enum ElementState { DEAD, ACTIVE, SELECTED, ERROR };
    public enum LacingType { SHORTEST, LONGEST, FULL };

    public partial class dynNodeUI : UserControl, INotifyPropertyChanged
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
        Dictionary<dynPort, TextBlock> inPortTextBlocks;
        Dictionary<dynPort, PortData> portDataDict = new Dictionary<dynPort, PortData>();
        string nickName;
        Guid guid;
        ElementState state;
        SetStateDelegate stateSetter;
        LacingType lacingType = LacingType.SHORTEST;
        dynNode nodeLogic;

        #endregion

        public delegate void SetToolTipDelegate(string message);
        public delegate void MarkConnectionStateDelegate(bool bad);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);
        public delegate void SetStateDelegate(dynNodeUI el, ElementState state);

        #region public members

        public dynNode NodeLogic
        {
            get { return nodeLogic; }
        }

        public dynNodeUI TopControl
        {
            get { return this.topControl; }
        }

        public string ToolTipText
        {
            get
            {
                try
                {
                    return portDataDict[OutPort].ToolTipString;
                }
                catch
                {
                    return "";
                }
            }
        }

        public List<dynPort> InPorts
        {
            get { return inPorts; }
            set { inPorts = value; }
        }

        public dynPort OutPort
        {
            get { return outPort; }
            set { outPort = value; }
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

        public ElementState State
        {
            get { return state; }
            set
            {
                switch (value)
                {
                    case ElementState.ACTIVE:
                        elementRectangle.Fill = dynSettings.ActiveBrush;
                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynSettings.ActiveBrush;
                        }

                        if (outPort != null)
                            outPort.ellipse1.Fill = dynSettings.ActiveBrush;

                        break;
                    case ElementState.DEAD:
                        elementRectangle.Fill = dynSettings.DeadBrush;
                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynSettings.DeadBrush;
                        }

                        if (outPort != null)
                            outPort.ellipse1.Fill = dynSettings.DeadBrush;

                        break;
                    case ElementState.ERROR:
                        elementRectangle.Fill = dynSettings.ErrorBrush;
                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynSettings.ErrorBrush;
                        }

                        if (outPort != null)
                            outPort.ellipse1.Fill = dynSettings.ErrorBrush;

                        break;
                    case ElementState.SELECTED:
                    default:
                        elementRectangle.Fill = dynSettings.SelectedBrush;
                        foreach (dynPort p in inPorts)
                        {
                            p.ellipse1.Fill = dynSettings.SelectedBrush;
                        }

                        if (outPort != null)
                            outPort.ellipse1.Fill = dynSettings.SelectedBrush;
                        break;
                }

                if (value != ElementState.ERROR)
                {
                    SetTooltip();
                }

                state = value;
            }
        }

        public LacingType LacingType
        {
            get { return lacingType; }
            set { lacingType = value; }
        }

        public Grid ContentGrid
        {
            get { return this.inputGrid; }
        }
        #endregion

        #region events
        //public event dynElementUpdatedHandler dynElementUpdated;
        public event dynElementDestroyedHandler dynNodeDestroyed;
        public event dynElementReadyToBuildHandler dynElementReadyToBuild;
        public event dynElementReadyToDestroyHandler dynNodeReadyToDestroy;
        public event dynElementSelectedHandler dynElementSelected;
        public event dynElementDeselectedHandler dynElementDeselected;
        #endregion

        #region constructors
        /// <summary>
        /// dynElement constructor for use by workbench in creating dynElements
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nickName"></param>
        public dynNodeUI(dynNode logic)
        {
            InitializeComponent();

            this.nodeLogic = logic;

            //set the main grid's data context to 
            //this element
            nickNameBlock.DataContext = this;

            inPorts = new List<dynPort>();
            //inPortData = new List<PortData>();
            inPortTextBlocks = new Dictionary<dynPort, TextBlock>();

            stateSetter = new SetStateDelegate(SetState);
            this.State = ElementState.DEAD;

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

            //set the z index to 2
            Canvas.SetZIndex(this, 1);

            //dirtyEllipse = new System.Windows.Shapes.Ellipse();
            //dirtyEllipse.Height = 20;
            //dirtyEllipse.Width = 20;
            //dirtyEllipse.Fill = Brushes.Red;
            //this.elementCanvas.Children.Add(dirtyEllipse);
            //Canvas.SetBottom(dirtyEllipse, 10);
            //Canvas.SetRight(dirtyEllipse, 10);
            //Canvas.SetZIndex(dirtyEllipse, 100);

            //dynElementReadyToBuild += new dynElementReadyToBuildHandler(Build);
        }
        #endregion

        protected virtual void OnDynElementDestroyed(EventArgs e)
        {
            if (dynNodeDestroyed != null)
            {
                dynNodeDestroyed(this, e);
            }
        }

        protected virtual void OnDynElementReadyToBuild(EventArgs e)
        {
            if (dynElementReadyToBuild != null)
            {
                dynElementReadyToBuild(this, e);
            }
        }

        protected virtual void OnDynElementReadyToDestroy(EventArgs e)
        {
            if (dynNodeReadyToDestroy != null)
            {
                dynNodeReadyToDestroy(this, e);
            }
        }

        protected virtual void OnDynElementSelected(EventArgs e)
        {
            if (dynElementSelected != null)
            {
                dynElementSelected(this, e);
            }
        }

        protected virtual void OnDynElementDeselected(EventArgs e)
        {
            if (dynElementDeselected != null)
            {
                dynElementDeselected(this, e);
            }
        }

        public void RegisterInputsAndOutput()
        {
            ResizeElementForInputs();
            SetupPortGrids();
            RegisterInputs();
            RegisterOutput();

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
            foreach (var p in this.InPorts)
                p.Update();
            this.OutPort.Update();
        }

        private Dictionary<UIElement, bool> enabledDict 
            = new Dictionary<UIElement, bool>();

        internal void DisableInteraction()
        {
            enabledDict.Clear();

            foreach (UIElement e in this.inputGrid.Children)
            {
                enabledDict[e] = e.IsEnabled;

                e.IsEnabled = false;
            }
            this.State = ElementState.DEAD;
        }

        internal void EnableInteraction()
        {
            foreach (UIElement e in this.inputGrid.Children)
            {
                if (enabledDict.ContainsKey(e))
                    e.IsEnabled = enabledDict[e];
            }
            this.ValidateConnections();
        }

        /// <summary>
        /// Is this node an entry point to the program?
        /// </summary>
        public bool IsTopmost
        {
            get
            {
                return this.OutPort == null || !this.OutPort.Connectors.Any();
            }
        }


        /// <summary>
        /// Resize the control based on the number of inputs.
        /// </summary>
        public void ResizeElementForInputs()
        {
            //size the height of the controller based on the 
            //whichever is larger the inport or the outport list
            this.topControl.Height = Math.Max(this.nodeLogic.InPortData.Count, 1) * 20 + 10; //spacing for inputs + title space + bottom space

            Thickness leftGridThick = new Thickness(gridLeft.Margin.Left, gridLeft.Margin.Top, gridLeft.Margin.Right, 5);
            gridLeft.Margin = leftGridThick;

            Thickness inputGridThick = new Thickness(inputGrid.Margin.Left, inputGrid.Margin.Top, inputGrid.Margin.Right, 5);
            inputGrid.Margin = inputGridThick;

            grid.UpdateLayout();

            //this.elementShine.Height = this.topControl.Height / 2;

            if (inputGrid.Children.Count == 0)
            {
                //decrease the width of the node because
                //there's nothing in the middle grid
                this.topControl.Width = 100;
            }
        }

        /// <summary>
        /// Sets up the control's grids based on numbers of input and output ports.
        /// </summary>
        public void SetupPortGrids()
        {
            int count = 0;
            int numRows = gridLeft.RowDefinitions.Count;
            foreach (object input in this.nodeLogic.InPortData)
            {
                if (count++ < numRows)
                    continue;

                RowDefinition rd = new RowDefinition();
                gridLeft.RowDefinitions.Add(rd);
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

            if (numRows > 1)
            {
                gridRight.RowDefinitions.RemoveRange(count, numRows - 1);
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
            foreach (PortData pd in this.nodeLogic.InPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                var port = AddPort(PortType.INPUT, this.nodeLogic.InPortData[count].NickName, count);
                this.portDataDict[port] = pd;
                count++;
            }

            if (this.inPorts.Count > count)
            {
                foreach (var inport in this.inPorts.Skip(count))
                {
                    RemovePort(inport);
                }

                this.inPorts.RemoveRange(count, this.inPorts.Count - count);
            }
        }

        private void RemovePort(dynPort inport)
        {
            if (inport.PortType == PortType.INPUT)
            {
                int index = this.inPorts.FindIndex(x => x == inport);

                gridLeft.Children.Remove(inport);
                gridLeft.Children.Remove(this.inPortTextBlocks[inport]);

                while (inport.Connectors.Any())
                {
                    inport.Connectors[0].Kill();
                }
            }
        }

        /// <summary>
        /// Reads outputs list and adds ports for each output
        /// </summary>
        public void RegisterOutput()
        {
            dynPort p = new dynPort(0);

            //create a text block for the name of the port
            TextBlock tb = new TextBlock();

            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.FontSize = 12;
            tb.FontWeight = FontWeights.Normal;
            tb.Foreground = new SolidColorBrush(Colors.Black);
            tb.Text = this.nodeLogic.OutPortData.NickName;

            tb.HorizontalAlignment = HorizontalAlignment.Right;

            ScaleTransform trans = new ScaleTransform(-1, 1, p.Width / 2, p.Height / 2);
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
            foreach (dynPort p in this.InPorts)
            {
                //get the types from the types list
                p.toolTipText.Text = this.nodeLogic.InPortData[count].ToolTipString;
                count++;
            }

            outPort.toolTipText.Text = this.nodeLogic.OutPortData.ToolTipString;
        }

        /// <summary>
        /// Add a dynPort element to this control.
        /// </summary>
        /// <param name="isInput">Is the port an input?</param>
        /// <param name="index">The index of the port in the port list.</param>
        public dynPort AddPort(PortType portType, string name, int index)
        {
            if (portType != PortType.INPUT)
                return null;

            if (inPorts.Count > index)
            {
                this.inPortTextBlocks[this.inPorts[index]].Text = name;
                return this.inPorts[index];
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

                return p;
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
                var data = this.portDataDict[port];
                this.nodeLogic.Connect(data, port.Connectors[0].Start.Owner.nodeLogic);
            }
        }

        void p_PortDisconnected(object sender, EventArgs e)
        {
            ValidateConnections();

            var port = (dynPort)sender;
            if (port.PortType == PortType.INPUT)
            {
                var data = this.portDataDict[port];
                this.nodeLogic.Disconnect(data);
            }
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
                this.State = ElementState.DEAD;
                //Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.DEAD });
            }
            else
            {
                this.State = ElementState.ACTIVE;
                //Dispatcher.Invoke(stateSetter, System.Windows.Threading.DispatcherPriority.Background, new object[] { this, ElementState.ACTIVE });
            }
        }

        protected void MarkConnectionState(bool bad)
        {
            //don't change the color of the object
            //if it's selected
            if (state != ElementState.SELECTED)
            {
                Dispatcher.Invoke(
                    stateSetter,
                    DispatcherPriority.Background,
                    new object[] {
                        this,
                        bad ? ElementState.ERROR : ElementState.ACTIVE
                    });
            }
        }

        protected internal void SetColumnAmount(int amt)
        {
            int count = this.inputGrid.ColumnDefinitions.Count;
            if (count == amt)
                return;
            else if (count < amt)
            {
                int diff = amt - count;
                for (int i = 0; i < diff; i++)
                    this.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            else
            {
                int diff = count - amt;
                this.inputGrid.ColumnDefinitions.RemoveRange(amt, diff);
            }
        }

        public void SetRowAmount(int amt)
        {
            int count = this.inputGrid.RowDefinitions.Count;
            if (count == amt)
                return;
            else if (count < amt)
            {
                int diff = amt - count;
                for (int i = 0; i < diff; i++)
                    this.inputGrid.RowDefinitions.Add(new RowDefinition());
            }
            else
            {
                int diff = count - amt;
                this.inputGrid.RowDefinitions.RemoveRange(amt, diff);
            }
        }

        public void CallUpdateLayout(FrameworkElement el)
        {
            el.UpdateLayout();
        }

        public void SetTooltip(string message)
        {
            this.ToolTip = message;
        }

        void SetTooltip()
        {
            object[] rtAttribs = this.GetType().GetCustomAttributes(typeof(NodeDescriptionAttribute), false);
            if (rtAttribs.Length > 0)
            {
                string description = ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription;
                this.ToolTip = description;
            }
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
            this.ValidateConnections();
        }

        void SetState(dynNodeUI el, ElementState state)
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
            var bench = dynSettings.Bench;
            this.nodeLogic.DisableReporting();
            this.nodeLogic.Destroy();
            bench.DeleteElement(this);
        }

        public void Error(string p)
        {
            MarkConnectionState(true);

            SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
            Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                new object[] { p });
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
}
