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
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Connectors
{
    public partial class dynPortView : UserControl, IViewModelView<PortViewModel>
    {
        private Dynamo.Controls.DragCanvas canvas;

        #region constructors

        public dynPortView()
        {
            InitializeComponent();
            this.Loaded += dynPort_Loaded;
        }

        void dynPort_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Port loaded.");
            canvas = WPF.FindUpVisualTree<Dynamo.Controls.DragCanvas>(this);

            if (ViewModel != null)
                ViewModel.UpdateCenter(CalculateCenter());

        }

        #endregion constructors

        public bool Visible
        {
            get
            {
                throw new NotImplementedException("Implement port Visibility parameter getter.");
            }
            set
            {
                throw new NotImplementedException("Implement port Visibility parameter setter.");
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dynSettings.ReturnFocusToSearch();

            if (ViewModel != null)
                ViewModel.ConnectCommand.Execute(null);
    
            //set the handled flag so that the element doesn't get dragged
            e.Handled = true;
        }

        Point CalculateCenter()
        {
            if (canvas != null && portRect.IsDescendantOf(canvas))
            {
                var transform = portRect.TransformToAncestor(canvas); // need to check if it is an ancestor first
                var rootPoint = transform.Transform(new Point(portRect.ActualWidth / 2, portRect.ActualHeight / 2));
                
                //put the "center" at one edge of the port
                if(ViewModel.PortType == PortType.INPUT)
                    return new Point(rootPoint.X - portRect.ActualWidth / 2 - 3, rootPoint.Y);
                return new Point(rootPoint.X + portRect.ActualWidth/2 + 3, rootPoint.Y);
            }

            return new Point();
        }

        private void DynPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.HighlightCommand.Execute(null);
        }

        private void DynPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.UnHighlightCommand.Execute(null);
        }

        public PortViewModel ViewModel
        {
            get
            {
                if (this.DataContext is PortViewModel)
                    return (PortViewModel) this.DataContext;
                else
                    return null;
            }
        }

        private void DynPortView_OnLayoutUpdated(object sender, EventArgs e)
        {
            if (ViewModel == null)
                return;

            Point p = CalculateCenter();
            if (p != ViewModel.Center)
            {
                //Debug.WriteLine("Port layout updated.");
                ViewModel.UpdateCenter(p);
            }
        }
    }

}
