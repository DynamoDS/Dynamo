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
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.FSharp.Collections;
using System.IO.Ports;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;
using HelixToolkit.Wpf;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;

namespace Dynamo.Nodes
{
    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.DEBUG)]
    [NodeDescription("Shows a dynamic preview of geometry.")]
    public class dynWatch3D : dynNodeWithOneOutput, INotifyPropertyChanged
    {
        WatchControl _watchView;

        private PointsVisual3D _points;
        private LinesVisual3D _lines;
        private List<MeshVisual3D> _meshes = new List<MeshVisual3D>();

        public Point3DCollection Points { get; set; }
        public Point3DCollection Lines { get; set; }
        public List<Mesh3D> Meshes { get; set; }

        System.Windows.Point _rightMousePoint;
        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
        
        private bool _requiresRedraw = false;

        bool _isScreenShot = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public dynWatch3D()
        {
            InPortData.Add(new PortData("IN", "Incoming geometry objects.", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Watch contents, passed through", typeof(object)));

            NodeUI.RegisterAllPorts();

            MenuItem mi = new MenuItem();
            mi.Header = "Zoom to Fit";
            mi.Click += new RoutedEventHandler(mi_Click);

            NodeUI.MainContextMenu.Items.Add(mi);

            //take out the left and right margins and make this so it's not so wide
            NodeUI.inputGrid.Margin = new Thickness(10, 10, 10, 10);

            _isScreenShot = true;

            if (_isScreenShot)
            {
                NodeUI.topControl.Width = 800;
                NodeUI.topControl.Height = 500;
            }
            else
            {
                NodeUI.topControl.Width = 400;
                NodeUI.topControl.Height = 300;
            }

            //add a 3D viewport to the input grid
            //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
            _watchView = new WatchControl();
            _watchView.watch_view.DataContext = this;

            RenderOptions.SetEdgeMode(_watchView, EdgeMode.Unspecified);

            _watchView.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            _watchView.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            Points = new Point3DCollection();
            Lines = new Point3DCollection();

            _points = new PointsVisual3D{Color = Colors.Red, Size=6};
            _lines = new LinesVisual3D{Color = Colors.Blue, Thickness = 1};
            
            _points.Points = Points;
            _lines.Points = Lines;

            _watchView.watch_view.Children.Add(_lines);
            _watchView.watch_view.Children.Add(_points);

            _watchView.watch_view.Children.Add(new DefaultLights());

            System.Windows.Shapes.Rectangle backgroundRect = new System.Windows.Shapes.Rectangle();
            backgroundRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            backgroundRect.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            backgroundRect.RadiusX = 10;
            backgroundRect.RadiusY = 10;
            backgroundRect.IsHitTestVisible = false;
            BrushConverter bc = new BrushConverter();
            Brush strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 216));
            backgroundRect.Fill = backgroundBrush;
            NodeUI.inputGrid.Children.Add(backgroundRect);
            NodeUI.inputGrid.Children.Add(_watchView);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void view_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _rightMousePoint = e.GetPosition(NodeUI.topControl);
        }

        void view_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right
            //click, we assume rotation. handle the event
            //so we don't show the context menu
            if (e.GetPosition(NodeUI.topControl) != _rightMousePoint)
            {
                e.Handled = true;
            }
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            _watchView.watch_view.ZoomExtents();
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!_requiresRedraw)
                return;

            Points = null;
            Lines = null;
            _lines.Points = null;
            _points.Points = null;

            Points = new Point3DCollection();
            Lines = new Point3DCollection();
            Meshes = new List<Mesh3D>();

            var descendants = Inputs.Values.SelectMany(x => x.Item2.DescendantsAndSelf());
            var guids = descendants.Where(x => x is IDrawable).Select(x => x.NodeUI.GUID);

            var drawable = descendants.Where(x => (x is IDrawable));
            Debug.WriteLine(string.Format("Drawing {0} elements in the watch.", drawable.Count()));

            foreach (var d in drawable)
            {
                RenderDescription rd = (d as IDrawable).Draw();

                foreach (Point3D p in rd.points)
                {
                    Points.Add(p);
                }

                foreach (Point3D p in rd.lines)
                {
                    Lines.Add(p);
                }

                foreach (Mesh3D mesh in rd.meshes)
                {
                    Meshes.Add(mesh);
                }
            }

            _lines.Points = Lines;
            _points.Points = Points;

            // remove old meshes from the renderer
            foreach (MeshVisual3D mesh in _meshes)
            {
                _watchView.watch_view.Children.Remove(mesh);
            }

            _meshes.Clear();

            foreach (Mesh3D mesh in Meshes)
            {
                MeshVisual3D vismesh = MakeMeshVisual3D(mesh);
                _watchView.watch_view.Children.Add(vismesh);
                _meshes.Add(vismesh);
            }

            RaisePropertyChanged("Points");
            RaisePropertyChanged("Lines");
            RaisePropertyChanged("Meshes");

            _requiresRedraw = false;
        }

        MeshVisual3D MakeMeshVisual3D(Mesh3D mesh)
        {
            MeshBuilder mesh_builder = new MeshBuilder();

            IList<Point3D> points = mesh.Vertices;
            IList<int[]> faces = mesh.Faces;

            foreach (int[] triangle in faces)
            {
                mesh_builder.AddTriangle(points[triangle[0]], points[triangle[1]], points[triangle[2]]);
                mesh_builder.AddTriangle(points[triangle[2]], points[triangle[1]], points[triangle[0]]);
            }

            MeshVisual3D vismesh = new MeshVisual3D { Content = new GeometryModel3D { Geometry = mesh_builder.ToMesh(), Material = Materials.White } };
            return vismesh;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            _requiresRedraw = true;

            return input;
        }
    }
}
