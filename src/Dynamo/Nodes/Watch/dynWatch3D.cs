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
        HelixViewport3D view;

        //LinesVisual3D axis;

        PointsVisual3D helixChildPoints;
        LinesVisual3D helixChildLines;
        List<MeshVisual3D> helixChildMeshes;

        Point3DCollection watchPoints;
        Point3DCollection watchLines;
        List<Mesh3D> watchMeshes;

        System.Windows.Point rightMousePoint;
        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();

        bool isScreenShot = false;

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

            isScreenShot = true;

            if (isScreenShot)
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
            view = new HelixViewport3D();
            view.DataContext = this;
            view.CameraRotationMode = CameraRotationMode.Turntable;
            view.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            view.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //RenderOptions.SetEdgeMode(view,EdgeMode.Aliased);
            RenderOptions.SetEdgeMode(view, EdgeMode.Unspecified);
            view.ShowViewCube = false;

            //view.IsHitTestVisible = true;
            view.ShowFrameRate = true;

            view.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            view.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            helixChildPoints = new PointsVisual3D { Color = Colors.Red, Size = 8 };
            view.Children.Add(helixChildPoints);

            helixChildLines = new LinesVisual3D
            {
                Color = Colors.Black,
                Thickness = 1
            };

            view.Children.Add(helixChildLines);

            helixChildMeshes = new List<MeshVisual3D>();

            watchPoints = new Point3DCollection();
            watchLines = new Point3DCollection();

            watchMeshes = new List<Mesh3D>();

            //axis = new LinesVisual3D
            //{
            //    Color = Colors.Gray,
            //    Thickness = 0.1
            //};
            
            //axis.Points.Add(new Point3D(1000, 0, 0));
            //axis.Points.Add(new Point3D(-1000, 0, 0));

            //axis.Points.Add(new Point3D(0, 1000, 0));
            //axis.Points.Add(new Point3D(0, -1000, 0));

            //axis.Points.Add(new Point3D(0, 0, 1000));
            //axis.Points.Add(new Point3D(0, 0, -1000));

            //view.Children.Add(axis);

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
            NodeUI.inputGrid.Children.Add(view);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void view_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            rightMousePoint = e.GetPosition(NodeUI.topControl);
        }

        void view_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right
            //click, we assume rotation. handle the event
            //so we don't show the context menu
            if (e.GetPosition(NodeUI.topControl) != rightMousePoint)
            {
                e.Handled = true;
            }
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            view.ZoomExtents();
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            helixChildPoints.Points = watchPoints;

            helixChildLines.Points = watchLines;

            foreach (MeshVisual3D m in helixChildMeshes) 
            {
                view.Children.Remove(m);
            }

            helixChildMeshes.Clear();

            foreach (Mesh3D mesh in watchMeshes) 
            {
                MeshVisual3D visual_mesh = new MeshVisual3D();
                visual_mesh.Mesh = mesh;

                view.Children.Add(visual_mesh);
                helixChildMeshes.Add(visual_mesh);
            }
        }

        //private void PredrawNode(dynNode node)
        //{
        //    IDrawable d = node as IDrawable;

        //    if (d != null)
        //        PredrawIDrawable(d);

        //    foreach (KeyValuePair<int, Tuple<int, dynNode>> entry in node.Inputs)
        //    {
        //        PredrawNode(entry.Value.Item2);
        //    }
        //}

        //private void PredrawIDrawable(IDrawable drawable)
        //{
        //    RenderDescription description = drawable.Draw();

        //    if (description.points != null)
        //    {
        //        foreach (Point3D p in description.points)
        //        {
        //            watchPoints.Add(p);
        //        }
        //    }

        //    if (description.lines != null)
        //    {
        //        foreach (Point3D p in description.lines)
        //        {
        //            watchLines.Add(p);
        //        }
        //    }

        //    if (description.meshes != null)
        //    {
        //        foreach (Mesh3D mesh in description.meshes)
        //        {
        //            watchMeshes.Add(mesh);
        //        }
        //    }
        //}

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            NodeUI.Dispatcher.Invoke(new Action(delegate {
                ClearPointsCollections();

                var descendants = Inputs.Values.SelectMany(x=>x.Item2.DescendantsAndSelf());
                var guids = descendants.Where(x => x is IDrawable).Select(x=>x.NodeUI.GUID);
                var renderDataLists = dynSettings.Controller.CurrentSpace.RenderData.
                    Where(x => guids.Contains(x.Key)).
                    Select(x=>x.Value);
                
                var points = renderDataLists.SelectMany(x=>x).Select(x=>x.points);
                var lines = renderDataLists.SelectMany(x => x).Select(x => x.lines);
                var meshes = renderDataLists.SelectMany(x => x).Select(x => x.meshes);

                foreach (Point3DCollection pColl in points)
                {
                    foreach (Point3D p in pColl)
                    {
                        watchPoints.Add(p);
                    }
                }
                foreach (Point3DCollection lColl in lines)
                {
                    foreach (Point3D p in lColl)
                    {
                        watchLines.Add(p);
                    }
                }

                //foreach (Point3D p in points)
                //{
                //    watchPoints.Add(p);
                //}

                //foreach (KeyValuePair<int, Tuple<int, dynNode>> entry in Inputs)
                //{
                //    PredrawNode(entry.Value.Item2);
                //}
            }));

            return input;
        }

        private void ClearPointsCollections()
        {
            watchPoints.Clear();
            watchLines.Clear();
            watchMeshes.Clear();
        }

        // Patrick: I don't understand what this does. Can someone add a comment
        private void DetachVisuals()
        {
            helixChildPoints.Points = null;
            helixChildLines.Points = null;

            foreach (MeshVisual3D m in helixChildMeshes)
            {
                m.Mesh = null;
            }
        }
    }
}
