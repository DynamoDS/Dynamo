using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using Microsoft.FSharp.Collections;
using HelixToolkit.Wpf;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using Dynamo.Controls;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class WatchViewFullscreen : UserControl
    {
        System.Windows.Point _rightMousePoint;

        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();

        public WatchViewFullscreen()
        {
            InitializeComponent();
            
            MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            MenuItem mi = new MenuItem();
            mi.Header = "Zoom to Fit";
            mi.Click += new RoutedEventHandler(mi_Click);

            MainContextMenu.Items.Add(mi);

            //System.Windows.Shapes.Rectangle backgroundRect = new System.Windows.Shapes.Rectangle();
            //Canvas.SetZIndex(backgroundRect, -10);
            //backgroundRect.IsHitTestVisible = false;
            //BrushConverter bc = new BrushConverter();
            //Brush strokeBrush = (Brush)bc.ConvertFrom("#313131");
            //backgroundRect.Stroke = strokeBrush;
            //backgroundRect.StrokeThickness = 1;
            //SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 216));
            //backgroundRect.Fill = backgroundBrush;

            //inputGrid.Children.Add(backgroundRect);
        }

        protected void mi_Click(object sender, RoutedEventArgs e)
        {
            watch_view.ZoomExtents();
        }

        private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        void view_MouseButtonIgnore(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        void view_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _rightMousePoint = e.GetPosition(topControl);
        }

        void view_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(topControl) != _rightMousePoint)
            {
                e.Handled = true;
            }
        }

        public Watch3DFullscreenViewModel ViewModel
        {
            get
            {
                if (this.DataContext is Watch3DFullscreenViewModel)
                    return (Watch3DFullscreenViewModel)this.DataContext;
                else
                    return null;
            }
        }

    }
}
