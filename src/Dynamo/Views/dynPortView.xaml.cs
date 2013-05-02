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

        #region constructors

        public dynPortView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(dynPort_Loaded);
        }

        void dynPort_Loaded(object sender, RoutedEventArgs e)
        {
            canvas = FindUpVisualTree<Dynamo.Controls.DragCanvas>(this);
            vm = DataContext as dynPortViewModel;

            vm.UpdateCenter(CalculateCenter());
        }

        #endregion constructors

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
            vm.ConnectCommand.Execute();
    
            //set the handled flag so that the element doesn't get dragged
            e.Handled = true;

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
            
            if (canvas != null)
            {
                var transform = portCircle.TransformToAncestor(canvas); // need to check if it is an ancestor first
                var rootPoint = transform.Transform(new Point(portCircle.Width/2, portCircle.Height/2));
                return new Point(rootPoint.X, rootPoint.Y);
            }

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
