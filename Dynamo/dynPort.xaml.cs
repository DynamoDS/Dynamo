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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Connectors
{
   /// <summary>
   /// Interaction logic for dynPort.xaml
   /// </summary>
   public delegate void PortConnectedHandler(object sender, EventArgs e);
   public delegate void PortDisconnectedHandler(object sender, EventArgs e);
   public enum PortType { INPUT, OUTPUT, STATE };

   public partial class dynPort : UserControl
   {
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

      List<dynConnector> connectors;
      Point center;

      dynNodeUI owner;
      int index;
      PortType portType;

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

      //public bool IsInputPort
      //{
      //    get { return isInputPort; }
      //    set { isInputPort = value; }
      //}

      public PortType PortType
      {
         get { return portType; }
         set { portType = value; }
      }

      public dynNodeUI Owner
      {
         get { return owner; }
         set { owner = value; }
      }

      public int Index
      {
         get { return index; }
         set { index = value; }
      }
      #endregion

      #region constructors

      public dynPort(int index)
      {
         connectors = new List<dynConnector>();
         //this.workBench = workBench;
         this.index = index;
         InitializeComponent();

         this.MouseEnter += delegate { foreach (var c in connectors) c.Highlight(); };
         this.MouseLeave += delegate { foreach (var c in connectors) c.Unhighlight(); };
      }
      #endregion constructors

      #region public methods
      public void Connect(dynConnector connector)
      {
         connectors.Add(connector);

         ellipse1Dot.Fill = System.Windows.Media.Brushes.Black;

         //throw the event for a connection
         OnPortConnected(EventArgs.Empty);

      }

      public void Disconnect(dynConnector connector)
      {
         if (connectors.Contains(connector))
         {
            connectors.Remove(connector);
         }

         //don't set back to white if
         //there are still connectors on this port
         if (connectors.Count == 0)
            ellipse1Dot.Fill = System.Windows.Media.Brushes.White;

         //throw the event for a connection
         OnPortDisconnected(EventArgs.Empty);
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
         GeneralTransform transform = this.TransformToAncestor(dynSettings.Instance.Workbench);
         Point rootPoint = transform.Transform(new Point(0, 0));

         double x = rootPoint.X + this.Width / 2;
         double y = rootPoint.Y + this.Width / 2;
         return new Point(x, y);

      }
      #endregion

      private void ellipse1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
      {
         //show the contextual menu
      }

      private void OnOpened(object sender, RoutedEventArgs e)
      {
         //do some stuff when opening
      }

      private void OnClosed(object sender, RoutedEventArgs e)
      {
         //do some stuff when closing
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
      }
      public string ToolTipString
      {
         get { return toolTip; }
      }
      public Type PortType
      {
         get { return portType; }
      }

      public PortData(string nickName, string tip, Type portType)
      {
         this.nickName = nickName;
         this.toolTip = tip;
         this.portType = portType;
      }
   }
}
