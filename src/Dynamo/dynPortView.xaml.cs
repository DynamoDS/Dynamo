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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dynamo.Connectors
{
    /// <summary>
    /// Interaction logic for dynPort.xaml
    /// </summary>
    public delegate void PortConnectedHandler(object sender, EventArgs e);
    public delegate void PortDisconnectedHandler(object sender, EventArgs e);
    public enum PortType { INPUT, OUTPUT };

    public partial class dynPortView : UserControl
    {
        private Dynamo.Controls.DragCanvas canvas;
        private dynPortViewModel vm;

#warning dynPort view no longer needs to notify of property changes
        /*
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }*/

        #region constructors

        //MVVM:make a default constructor
        //public dynPort(int index, PortType portType, dynNodeView owner, string name)
        public dynPortView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(dynPort_Loaded);
            //this.MouseEnter += delegate { foreach (var c in connectors) c.Highlight(); };
            //this.MouseLeave += delegate { foreach (var c in connectors) c.Unhighlight(); };

#warning don't set data contexts to the view
            /*portGrid.DataContext = this;
            portNameTb.DataContext = this;
            toolTipText.DataContext = this;
            ellipse1Dot.DataContext = this;
            ellipse1.DataContext = Owner;*/

            //portGrid.Loaded += new RoutedEventHandler(portGrid_Loaded);
        }

        void dynPort_Loaded(object sender, RoutedEventArgs e)
        {
            canvas = FindUpVisualTree<Dynamo.Controls.DragCanvas>(this);
            vm = DataContext as dynPortViewModel;

            vm.UpdateCenter(CalculateCenter());
        }

        #endregion constructors

        /*void portGrid_Loaded(object sender, RoutedEventArgs e)
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
        }*/

        public bool Visible
        {
            get
            {
                throw new NotImplementedException("Implement port Visibility parameter getter.");
                //return connector.Opacity > 0;
            }
            set
            {
                throw new NotImplementedException("Implement port Visibility parameter setter.");
                /*if (value)
                {
                    connector.Opacity = STROKE_OPACITY;
                    plineConnector.Opacity = STROKE_OPACITY;
                    endDot.Opacity = STROKE_OPACITY;
                }
                else
                {
                    connector.Opacity = 0;
                    plineConnector.Opacity = 0;
                    endDot.Opacity = 0;
                }*/
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine(string.Format("Port {0} selected.", this.Index));

            vm.ConnectCommand.Execute();

#warning logic moved to command on the view model
            
            #region test for a port
            /*
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
                    DynamoModel.Instance.CurrentSpace.Connectors.Remove(bench.ActiveConnector);
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
            */

            //set the handled flag so that the element doesn't get dragged
            e.Handled = true;

            #endregion
        }

        private void Ellipse1Dot_OnLayoutUpdated(object sender, EventArgs e)
        {
            if (vm != null)
            {
                //set the center property on the view model
                vm.UpdateCenter(CalculateCenter());
            }
        }

        Point CalculateCenter()
        {
            //if (canvas != null)
            //{
            //    GeneralTransform transform = portCircle.TransformToAncestor(canvas);
            //    Point rootPoint = transform.Transform(new Point(portCircle.Width / 2, portCircle.Height / 2));
            //    return new Point(rootPoint.X, rootPoint.Y);
            //}

            return new Point();
        }

        private void DynPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            (DataContext as dynPortViewModel).HighlightCommand.Execute();
        }

        private void DynPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            (DataContext as dynPortViewModel).UnHighlightCommand.Execute();
        }

        // walk up the visual tree to find object of type T, starting from initial object
        public static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;

            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }

}
