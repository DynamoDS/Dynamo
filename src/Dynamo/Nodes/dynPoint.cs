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
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Linq;
using Brush = System.Windows.Media.Brush;

using Dynamo.FSchemeInterop;
using Dynamo.Connectors;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

using HelixToolkit.Wpf;

namespace Dynamo.Nodes
{
    [NodeName("Drawable Point")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("An example of a point that can be drawn.")]
    class dynPoint3D : dynNodeWithOneOutput, IDrawable
    {
        Point3D p;
        Point3DCollection points = new Point3DCollection();

        public dynPoint3D()
        {
            InPortData.Add(new PortData("x", "The X component.", typeof(double)));
            InPortData.Add(new PortData("y", "The Y component", typeof(double)));
            InPortData.Add(new PortData("z", "The Z component", typeof(double)));
            OutPortData.Add(new PortData("n", "An output value.", typeof(double)));

            NodeUI.RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x = (double)((Value.Number)args[0]).Item;
            double y = (double)((Value.Number)args[1]).Item;
            double z = (double)((Value.Number)args[2]).Item;

            p = new Point3D(x, y, z);

            NodeUI.Dispatcher.Invoke(new Action(
               delegate
               {
                   points.Clear();
                   points.Add(p);
               }));

            return Value.NewContainer(this);
        }

        public virtual Point3DCollection Points()
        {
            return points;
        }

        public virtual Point3DCollection Lines()
        {
            return null;
        }

        public virtual Mesh3D[] Meshes()
        {
            return null;
        }

        public RenderDescription Draw()
        {
            throw new NotImplementedException();
        }
    }

    //[NodeName("Watch 3D Accumulator")]
    //[NodeCategory(BuiltinNodeCategories.DEBUG)]
    //[NodeDescription("Shows a dynamic preview of geometry.")]
    //public class dyn3DPreview : dynNodeWithOneOutput, INotifyPropertyChanged
    //{
    //    HelixViewport3D view;
    //    PointsVisual3D points;
    //    System.Windows.Point rightMousePoint;

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged(string property)
    //    {
    //        var handler = PropertyChanged;
    //        if (handler != null)
    //        {
    //            handler(this, new PropertyChangedEventArgs(property));
    //        }
    //    }

    //    public dyn3DPreview()
    //    {
    //        InPortData.Add(new PortData("IN", "Incoming geometry objects.", typeof(object)));
    //        OutPortData.Add(new PortData("OUT", "Watch contents, passed through", typeof(object)));

    //        NodeUI.RegisterAllPorts();

    //        //get rid of right click delete
    //        //this.MainContextMenu.Items.Clear();

    //        MenuItem mi = new MenuItem();
    //        mi.Header = "Zoom to Fit";
    //        mi.Click += new RoutedEventHandler(mi_Click);

    //        NodeUI.MainContextMenu.Items.Add(mi);

    //        //take out the left and right margins
    //        //and make this so it's not so wide
    //        NodeUI.inputGrid.Margin = new Thickness(10, 10, 10, 10);

    //        NodeUI.topControl.Width = 400;
    //        NodeUI.topControl.Height = 300;

    //        //add a 3D viewport to the input grid
    //        //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
    //        view = new HelixViewport3D();
    //        view.DataContext = this;
    //        view.CameraRotationMode = CameraRotationMode.Turntable;
    //        view.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
    //        view.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
    //        //RenderOptions.SetEdgeMode(view,EdgeMode.Aliased);
    //        RenderOptions.SetEdgeMode(view, EdgeMode.Unspecified);
    //        view.ShowViewCube = false;
    //        view.ShowFrameRate = true;

    //        view.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
    //        view.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

    //        points = new PointsVisual3D { Color = Colors.Black, Size = 4 };
    //        view.Children.Add(points);

    //        System.Windows.Shapes.Rectangle backgroundRect = new System.Windows.Shapes.Rectangle();
    //        backgroundRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
    //        backgroundRect.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
    //        backgroundRect.RadiusX = 10;
    //        backgroundRect.RadiusY = 10;
    //        backgroundRect.IsHitTestVisible = false;
    //        BrushConverter bc = new BrushConverter();
    //        Brush strokeBrush = (Brush)bc.ConvertFrom("#313131");
    //        backgroundRect.Stroke = strokeBrush;
    //        backgroundRect.StrokeThickness = 1;
    //        SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 216));
    //        backgroundRect.Fill = backgroundBrush;
    //        NodeUI.inputGrid.Children.Add(backgroundRect);
    //        NodeUI.inputGrid.Children.Add(view);

    //        CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
    //    }

    //    void view_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    //    {
    //        rightMousePoint = e.GetPosition(NodeUI.topControl);
    //    }

    //    void view_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    //    {
    //        //if the mouse has moved, and this is a right
    //        //click, we assume rotation. handle the event
    //        //so we don't show the context menu
    //        if (e.GetPosition(NodeUI.topControl) != rightMousePoint)
    //        {
    //            e.Handled = true;
    //        }
    //    }

    //    void mi_Click(object sender, RoutedEventArgs e)
    //    {
    //        view.ZoomExtents();
    //    }

    //    void CompositionTarget_Rendering(object sender, EventArgs e)
    //    {
    //        //if (points != null)
    //        //{
    //        //    points.Points = Points[0];
    //        //}
    //    }

    //    public override Value Evaluate(FSharpList<Value> args)
    //    {
    //        var input = args[0];
    //        NodeUI.Dispatcher.Invoke(new Action(
    //           delegate
    //           {
    //               var descendants = Inputs.Values.SelectMany(x=>x.Item2.DescendantsAndSelf());
    //               var drawableNodes = descendants.Where(x => x is IDrawable);
    //               var newPoints = drawableNodes.SelectMany(x => (x as IDrawable).Draw()).ToList();
                   
    //               //detach the visuals
    //               points.Points.Clear();
    //               points.Points = null;

    //               points.Points = newPoints;
    //           }));
          
    //        return input; //watch 3d should be a 'pass through' node
    //    }

        
    //}
}
