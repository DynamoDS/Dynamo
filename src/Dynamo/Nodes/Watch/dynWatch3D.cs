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
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;
using HelixToolkit.Wpf;
using Dynamo.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace Dynamo.Nodes
{
    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Shows a dynamic preview of geometry.")]
    public class dynWatch3D : dynNodeWithOneOutput
    {
        WatchView _watchView;

        private PointsVisual3D _points;
        private LinesVisual3D _lines;
        private List<MeshVisual3D> _meshes = new List<MeshVisual3D>();

        public Point3DCollection Points { get; set; }
        public Point3DCollection Lines { get; set; }
        public List<Mesh3D> Meshes { get; set; }

        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
        
        private bool _requiresRedraw = false;
        private bool _isRendering = false;

        public dynWatch3D()
        {
            InPortData.Add(new PortData("IN", "Incoming geometry objects.", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Watch contents, passed through", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
        {
            MenuItem mi = new MenuItem();
            mi.Header = "Zoom to Fit";
            mi.Click += new RoutedEventHandler(mi_Click);

            NodeUI.MainContextMenu.Items.Add(mi);

            //take out the left and right margins and make this so it's not so wide
            //NodeUI.inputGrid.Margin = new Thickness(10, 10, 10, 10);

            //add a 3D viewport to the input grid
            //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
            _watchView = new WatchView();
            _watchView.watch_view.DataContext = this;

            RenderOptions.SetEdgeMode(_watchView, EdgeMode.Unspecified);

            Points = new Point3DCollection();
            Lines = new Point3DCollection();

            _points = new PointsVisual3D { Color = Colors.Red, Size = 6 };
            _lines = new LinesVisual3D { Color = Colors.Blue, Thickness = 1 };

            _points.Points = Points;
            _lines.Points = Lines;

            _watchView.watch_view.Children.Add(_lines);
            _watchView.watch_view.Children.Add(_points);

            _watchView.watch_view.Children.Add(new DefaultLights());

            _watchView.Width = 400;
            _watchView.Height = 300;

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

        void mi_Click(object sender, RoutedEventArgs e)
        {
            _watchView.watch_view.ZoomExtents();
        }

        private void GetUpstreamIDrawable(List<IDrawable> drawables, Dictionary<int, Tuple<int, dynNodeModel>> inputs)
        {
            foreach (KeyValuePair<int, Tuple<int, dynNodeModel>> pair in inputs)
            {
                if (pair.Value == null)
                    continue;

                dynNodeModel node = pair.Value.Item2;
                IDrawable drawable = node as IDrawable;

                if (drawable != null)
                    drawables.Add(drawable);

                GetUpstreamIDrawable(drawables, node.Inputs);
            }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_isRendering)
                return;

            if (!_requiresRedraw)
                return;

            _isRendering = true;

            Points = null;
            Lines = null;
            _lines.Points = null;
            _points.Points = null;

            Points = new Point3DCollection();
            Lines = new Point3DCollection();
            Meshes = new List<Mesh3D>();

            // a list of all the upstream IDrawable nodes
            List<IDrawable> drawables = new List<IDrawable>();

            GetUpstreamIDrawable(drawables, Inputs);

            foreach (IDrawable d in drawables)
            {
                RenderDescription rd = d.Draw();

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

            _requiresRedraw = false;
            _isRendering = false;
        }

        MeshVisual3D MakeMeshVisual3D(Mesh3D mesh)
        {
            MeshVisual3D vismesh = new MeshVisual3D { Content = new GeometryModel3D { Geometry = mesh.ToMeshGeometry3D(), Material = Materials.White } };
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
