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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.FSharp.Collections;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Dynamo.Commands;

using Expression = Dynamo.FScheme.Expression;
using Grid = System.Windows.Controls.Grid;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary>
    public enum ElementState { DEAD, ACTIVE, ERROR };

    public partial class dynNodeUI : UserControl, INotifyPropertyChanged, ISelectable
    {
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
        ObservableCollection<dynPort> inPorts;
        ObservableCollection<dynPort> outPorts;
        Dictionary<dynPort, PortData> portDataDict = new Dictionary<dynPort, PortData>();
        string nickName;
        Guid guid;
        string toolTipText = "";
        ElementState state;
        dynNode nodeLogic;
        bool isSelected = false;
        int preferredHeight = 30;
        #endregion

        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);

        #region public members

        public dynNode NodeLogic
        {
            get { return nodeLogic; }
        }

        public dynNodeUI TopControl
        {
            get { return topControl; }
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
                NotifyPropertyChanged("ToolTipText");
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
                if (value != ElementState.ERROR)
                {
                    SetTooltip();
                }

                state = value;
                NotifyPropertyChanged("State");
            }
        }

        public Grid ContentGrid
        {
            get { return inputGrid; }
        }

        public bool IsSelected
        {
            get 
            { 
                return isSelected; 
            }
            set 
            { 
                isSelected = value;
                NotifyPropertyChanged("IsSelected");

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
                    foreach (dynConnector c in inPorts.SelectMany(x=>x.Connectors)
                        .Concat(outPorts.SelectMany(x=>x.Connectors)))
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
                NotifyPropertyChanged("PreferredHeight");
            }
        }
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

            nodeLogic = logic;

            //set the main grid's data context to 
            //this element
            nickNameBlock.DataContext = this;
            elementRectangle.DataContext = this;
            topControl.DataContext = this;
            elementRectangle.DataContext = this;

            inPorts = new ObservableCollection<dynPort>();
            outPorts = new ObservableCollection<dynPort>();
            inPorts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ports_collectionChanged);
            outPorts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ports_collectionChanged);
            this.IsSelected = false;

            Binding heightBinding = new Binding("PreferredHeight");
            topControl.SetBinding(UserControl.HeightProperty, heightBinding);
            
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

            //set the z index to 2
            Canvas.SetZIndex(this, 1); 
        }

        void ports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space
        }

        #endregion

        public void RegisterAllPorts()
        {
            ResizeElementForPorts();
            SetupPortGrids();
            RegisterInputs();
            RegisterOutputs();

            UpdateLayout();

            SetToolTips();
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
        /// Resize the control based on the number of inputs.
        /// </summary>
        public void ResizeElementForPorts()
        {
            Thickness leftGridThick = new Thickness(gridLeft.Margin.Left, gridLeft.Margin.Top, gridLeft.Margin.Right, 5);
            gridLeft.Margin = leftGridThick;

            Thickness rightGridThick = new Thickness(gridRight.Margin.Left, gridRight.Margin.Top, gridRight.Margin.Right, 5);
            gridRight.Margin = rightGridThick;

            Thickness inputGridThick = new Thickness(inputGrid.Margin.Left, inputGrid.Margin.Top, inputGrid.Margin.Right, 5);
            inputGrid.Margin = inputGridThick;

            grid.UpdateLayout();

            if (inputGrid.Children.Count == 0)
            {
                //decrease the width of the node because
                //there's nothing in the middle grid
                topControl.Width = 100;
            }
        }

        /// <summary>
        /// Sets up the control's grids based on numbers of input and output ports.
        /// </summary>
        public void SetupPortGrids()
        {
            int count = 0;
            int numRows = gridLeft.RowDefinitions.Count;
            foreach (var input in nodeLogic.InPortData)
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
            foreach (var input in nodeLogic.OutPortData)
            {
                if (count++ < numRows)
                    continue;

                RowDefinition rd = new RowDefinition();
                gridRight.RowDefinitions.Add(rd);
            }

            if (count < numRows)
            {
                gridRight.RowDefinitions.RemoveRange(count, numRows - count);
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

        public void SetToolTips()
        {
            //set all the tooltips
            int count = 0;
            foreach (dynPort p in InPorts)
            {
                //get the types from the types list
                p.toolTipText.Text = nodeLogic.InPortData[count].ToolTipString;
                count++;
            }

            count = 0;
            foreach (dynPort p in OutPorts)
            {
                p.toolTipText.Text = nodeLogic.OutPortData[count].ToolTipString;
                count++;
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
                    dynPort p = new dynPort(index);

                    p.PortType = PortType.INPUT;
                    InPorts.Add(p);
                    gridLeft.Children.Add(p);
                    Grid.SetColumn(p, 0);
                    Grid.SetRow(p, index);

                    p.Owner = this;
                    p.PortName = name;
                    
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
                    dynPort p = new dynPort(index);

                    p.PortType = PortType.OUTPUT;
                    OutPorts.Add(p);
                    //portTextBlocks[p] = tb;
                    gridRight.Children.Add(p);
                    Grid.SetColumn(p, 1);
                    Grid.SetRow(p, index);

                    p.Owner = this;
                    p.PortName = name;

                    //register listeners on the port
                    p.PortConnected += new PortConnectedHandler(p_PortConnected);
                    p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);

                    //flip the right hand ports
                    ScaleTransform trans = new ScaleTransform(-1, 1, 25, p.Height / 2);
                    p.portGrid.RenderTransform = trans;
                    p.portNameTb.Margin = new Thickness(0, 0, 15, 0);
                    p.portNameTb.TextAlignment = TextAlignment.Right;
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
            var portsWithoutConnectors = inPorts.Select(x => x).Where(x => x.Connectors.Count == 0).Count();

            // if there are inputs without connections
            // mark as dead
            if(inPorts.Select(x => x).Where(x => x.Connectors.Count == 0).Count() > 0)
            {
                State = ElementState.DEAD;
            }
            else
            {
                //don't override state if it's in error
                //if (State != ElementState.ERROR)
                //{
                    State = ElementState.ACTIVE;
                //}
            }
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

        public void SetRowAmount(int amt)
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
            //ToolTip = message;
            this.ToolTipText = message;
        }

        public string Description { get {
            Type t = NodeLogic.GetType();
            object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true); 
            return ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription; 
        }}

        public List<string> Tags
        {
            get
            {
                Type t = NodeLogic.GetType();
                object[] rtAttribs = t.GetCustomAttributes(typeof(NodeSearchTagsAttribute), true);

                if ( rtAttribs.Length > 0 )
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

        public void Select()
        {
            this.IsSelected = true;
        }

        public void SelectNeighbors()
        {
            var outConnectors = this.outPorts.SelectMany(x => x.Connectors);
            var inConnectors = this.inPorts.SelectMany(x => x.Connectors);

            foreach(dynConnector c in outConnectors)
            {
                if(!dynSettings.Workbench.Selection.Contains(c.End.Owner))
                    dynSettings.Workbench.Selection.Add(c.End.Owner);
            }

            foreach(dynConnector c in inConnectors)
            {
                if(!dynSettings.Workbench.Selection.Contains(c.Start.Owner))
                    dynSettings.Workbench.Selection.Add(c.Start.Owner);
            }
        }

        public void Deselect()
        {
            ValidateConnections();
            this.IsSelected = false;
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

        private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void deleteElem_cm_Click(object sender, RoutedEventArgs e)
        {
            nodeLogic.DisableReporting();
            nodeLogic.Destroy();

            DynamoCommands.DeleteCmd.Execute(this);
        }

        public void Error(string p)
        {
            State = ElementState.ERROR;
            ToolTipText = p;
        }

        private void topControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dynSettings.Bench.mainGrid.Focus();
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SelectCmd, this));
            dynSettings.Controller.ProcessCommandQueue();
        }

        private void topControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Node right selected.");
            e.Handled = true;
        }
    }

    //[ValueConversion(typeof(double), typeof(Thickness))]
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

    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool condition = (bool)value;
            if (condition)
            {
                return new SolidColorBrush(Colors.Cyan);
            }
            else
            {
                return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class StateToColorConverter : IValueConverter
    {
        //http://stackoverflow.com/questions/3238590/accessing-colors-in-a-resource-dictionary-from-a-value-converter

        public LinearGradientBrush DeadBrush { get; set; }
        public LinearGradientBrush ActiveBrush { get; set; }
        public LinearGradientBrush ErrorBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ElementState state = (ElementState)value;
            switch (state)
            {
                case ElementState.ACTIVE:
                    return ActiveBrush;
                case ElementState.DEAD:
                    return DeadBrush;
                case ElementState.ERROR:
                    return ErrorBrush;
            }

            return DeadBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class PortCountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<dynPort> ports = (ObservableCollection<dynPort>)value;
            return Math.Max(30, ports.Count * 20 + 10); //spacing for inputs + title space + bottom space
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ListHasItemsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<object> list = (List<object>)value;
            return list.Count > 0; //spacing for inputs + title space + bottom space
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    //[ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ShowHideConsoleMenuItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return "Hide Console";
            }
            else
            {
                return "Show Console";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ShowHideClassicNavigatorMenuItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return "Hide Classic Node Library";
            }
            else
            {
                return "Show Classic Node Library";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ZoomStatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return string.Format("Zoom : {0}", value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class TransformOriginStatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            Point p = (Point)value;
            return string.Format("Transform origin X: {0}, Y: {1}", p.X, p.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class CurrentOffsetStatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            Point p = (Point)value;
            return string.Format("Current offset X: {0}, Y: {1}", p.X, p.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // You could also directly pass an enum value using {x:Static},
            // then there is no need to parse
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
