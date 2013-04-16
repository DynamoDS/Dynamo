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
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;

using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Controls;

namespace Dynamo.Connectors
{
    /// <summary>
    /// Interaction logic for dynPort.xaml
    /// </summary>
    public delegate void PortConnectedHandler(object sender, EventArgs e);
    public delegate void PortDisconnectedHandler(object sender, EventArgs e);
    public enum PortType { INPUT, OUTPUT };

    public partial class dynPort : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region events
        public event PortConnectedHandler PortConnected;
        public event PortConnectedHandler PortDisconnected;

        protected virtual void OnPortConnected(EventArgs e)
        {
            if (PortConnected != null)
                PortConnected(this, e);
        }
        protected virtual void OnPortDisconnected(EventArgs e)
        {
            if (PortDisconnected != null)
                PortDisconnected(this, e);
        }

        #endregion

        #region private members

        List<dynConnector> connectors = new List<dynConnector>();
        Point center;
        bool isConnected;
        dynNodeUI owner;
        int index;
        PortType portType;
        string name;
        #endregion

        #region public members

        public Point Center
        {
            get { return UpdateCenter(); }
            set { center = value; }
        }

        public List<dynConnector> Connectors
        {
            get { return connectors; }
            set { connectors = value; }
        }

        public string ToolTipContent
        {
            get
            {
                if (Owner != null)
                {
                    if (PortType == Dynamo.Connectors.PortType.INPUT)
                    {
                        return Owner.NodeLogic.InPortData[index].ToolTipString;
                    }
                    else
                    {
                        return Owner.NodeLogic.OutPortData[index].ToolTipString;
                    }
                }
                return "";
            }
        }

        public string PortName
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("PortName");
            }
                
        }
        
        public PortType PortType
        {
            get { return portType; }
            set { portType = value; }
        }

        public dynNodeUI Owner
        {
            get { return owner; }
            set 
            { 
                owner = value;
                NotifyPropertyChanged("Owner");
            }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public bool IsConnected
        {
            get
            { return isConnected; }
            set
            {
                isConnected = value;
                NotifyPropertyChanged("IsConnected");
            }
        }

        #endregion

        #region constructors

        public dynPort(int index, PortType portType, dynNodeUI owner, string name)
        {
            InitializeComponent();

            Index = index;

            this.MouseEnter += delegate { foreach (var c in connectors) c.Highlight(); };
            this.MouseLeave += delegate { foreach (var c in connectors) c.Unhighlight(); };

            IsConnected = false;

            PortType = portType;
            Owner = owner;
            PortName = name;

            portGrid.DataContext = this;
            portNameTb.DataContext = this;
            toolTipText.DataContext = this;
            ellipse1Dot.DataContext = this;
            ellipse1.DataContext = Owner;

            portGrid.Loaded += new RoutedEventHandler(portGrid_Loaded);
        }

        void portGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //flip the output ports so they show up on the 
            //right hand side of the node with text on the left
            //do this after the port is loaded so we can get
            //its ActualWidth
            //if (PortType == Dynamo.Connectors.PortType.OUTPUT)
            //{
            //    ScaleTransform trans = new ScaleTransform(-1, 1, ActualWidth/2, Height / 2);
            //    portGrid.RenderTransform = trans;
            //    portNameTb.Margin = new Thickness(0, 0, 15, 0);
            //    portNameTb.TextAlignment = TextAlignment.Right;
            //}
        }
        #endregion constructors

        #region public methods
        public void Connect(dynConnector connector)
        {
            connectors.Add(connector);

            //throw the event for a connection
            OnPortConnected(EventArgs.Empty);

            IsConnected = true;
        }

        public void Disconnect(dynConnector connector)
        {
            //throw the event for a connection
            OnPortDisconnected(EventArgs.Empty);

            if (connectors.Contains(connector))
            {
                connectors.Remove(connector);
            }
            
            //don't set back to white if
            //there are still connectors on this port
            if (connectors.Count == 0)
            {
                IsConnected = false;
            }

            if (connectors.Count == 0)
                Owner.State = ElementState.DEAD;

            
        }

        public void Update()
        {
            foreach (dynConnector c in connectors)
            {
                //calling this with null will have
                //no effect
                c.Redraw();
            }
        }
        #endregion

        #region private methods
        Point UpdateCenter()
        {

            GeneralTransform transform = portCircle.TransformToAncestor(dynSettings.Workbench);
            Point rootPoint = transform.Transform(new Point(portCircle.Width / 2, portCircle.Height / 2));

            //double x = rootPoint.X;
            //double y = rootPoint.Y;

            //if(portType == Dynamo.Connectors.PortType.INPUT)
            //{
            //    x += ellipse1.Width / 2;
            //}
            //y += ellipse1.Height / 2;

            //return new Point(x, y);
            return new Point(rootPoint.X, rootPoint.Y);

        }
        #endregion

        private void OnOpened(object sender, RoutedEventArgs e)
        {
            //do some stuff when opening
        }

        private void OnClosed(object sender, RoutedEventArgs e)
        {
            //do some stuff when closing
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(string.Format("Port {0} selected.", this.Index));

            #region test for a port

            dynBench bench = dynSettings.Bench;
            
            if (!bench.WorkBench.IsConnecting)
            {
                //test if port already has a connection if so grab it
                //and begin connecting to somewhere else
                //don't allow the grabbing of the start connector
                if (this.Connectors.Count > 0 && this.Connectors[0].Start != this)
                {
                    bench.ActiveConnector = this.Connectors[0];
                    bench.ActiveConnector.Disconnect(this);
                    bench.WorkBench.IsConnecting = true;
                    dynSettings.Controller.CurrentSpace.Connectors.Remove(bench.ActiveConnector);
                }
                else
                {
                    try
                    {
                        //you've begun creating a connector
                        dynConnector c = new dynConnector(this, bench.WorkBench, e.GetPosition(bench.WorkBench));
                        bench.ActiveConnector = c;
                        bench.WorkBench.IsConnecting = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                //attempt a connection between the port
                //and the connector
                if (!bench.ActiveConnector.Connect(this))
                {
                    bench.ActiveConnector.Kill();
                    bench.WorkBench.IsConnecting = false;
                    bench.ActiveConnector = null;
                }
                else
                {
                    //you've already started connecting
                    //now you're going to stop
                    dynSettings.Controller.CurrentSpace.Connectors.Add(bench.ActiveConnector);
                    bench.WorkBench.IsConnecting = false;
                    bench.ActiveConnector = null;
                }
            }

            //set the handled flag so that the element doesn't get dragged
            e.Handled = true;

            #endregion
        }

        internal void KillAllConnectors()
        {
            foreach (var c in connectors.ToList())
                c.Kill();
        }
    }

    public class PortData
    {
        string nickName;
        string toolTip;
        Type portType;

        public string NickName
        {
            get { return nickName; }
            internal set { nickName = value; }
        }

        public string ToolTipString
        {
            get { return toolTip; }
            internal set { toolTip = value; }
        }

        public Type PortType
        {
            get { return portType; }
            set { portType = value; }
        }

        public PortData(string nickName, string tip, Type portType)
        {
            this.nickName = nickName;
            this.toolTip = tip;
            this.portType = portType;
        }

        //public override bool Equals(object obj)
        //{
        //    var other = obj as PortData;

        //    return other != null
        //        && other.nickName.Equals(nickName)
        //        && other.portType.Equals(portType)
        //        && other.toolTip == toolTip;
        //}

        //public override int GetHashCode()
        //{
        //    return nickName.GetHashCode() * 7 
        //        + portType.GetHashCode() * 11 
        //        + toolTip.GetHashCode() * 3;
        //}
    }
}
