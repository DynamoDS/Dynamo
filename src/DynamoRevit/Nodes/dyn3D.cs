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
using Autodesk.Revit.DB;
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
    public class dyn3DPreview: dynNodeWithOneOutput, INotifyPropertyChanged
    {
        HelixViewport3D view;
        PointsVisual3D points;
        PointsVisual3D fixedPoints;
        List<LinesVisual3D> linesList;
        System.Windows.Point rightMousePoint;
        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();

        bool isScreenShot= false;
        //bool isDrawingPoints;
        ParticleSystem ps;
        Curve c;
        XYZ pt;

        public List<Point3DCollection> Points{get;set;}

        public Point3DCollection FixedPoints { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public dyn3DPreview()
        {
            InPortData.Add(new PortData("IN", "Incoming geometry objects.", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Watch contents, passed through", typeof(object)));

            NodeUI.RegisterAllPorts();

            //get rid of right click delete
            //this.MainContextMenu.Items.Clear();

            MenuItem mi = new MenuItem();
            mi.Header = "Zoom to Fit";
            mi.Click +=new RoutedEventHandler(mi_Click);

            NodeUI.MainContextMenu.Items.Add(mi);

            //take out the left and right margins
            //and make this so it's not so wide
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

            //this.elementShine.Visibility = System.Windows.Visibility.Hidden;
            //this.elementRectangle.Visibility = System.Windows.Visibility.Hidden;

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

            fixedPoints = new PointsVisual3D { Color = Colors.Red, Size = 8 };
            view.Children.Add(fixedPoints);

            points = new PointsVisual3D { Color = Colors.Black, Size = 4 };
            view.Children.Add(points);

            // a list of 10 colors to be used for 
            // mapping analysis results
            colors.Add(Colors.Black); //01
            colors.Add(Colors.LightBlue); //02
            colors.Add(Colors.Blue); //03
            colors.Add(Colors.Purple); //04
            colors.Add(Colors.LightGreen); //05
            colors.Add(Colors.GreenYellow); //06
            colors.Add(Colors.Yellow); //07
            colors.Add(Colors.Orange); //08
            colors.Add(Colors.OrangeRed); //09
            colors.Add(Colors.Red); //10
            colors.Add(Colors.Black);//11 for naked lines and curves

            FixedPoints = new Point3DCollection();
            Points = new List<Point3DCollection>();
            for (int i = 0; i < colors.Count(); i++)
            {
                Points.Add(new Point3DCollection());
            }

            linesList = new List<LinesVisual3D>();
            for (int i = 0; i < colors.Count(); i++)
            {
                LinesVisual3D lines = new LinesVisual3D { Color = colors[i], Thickness = 1 };

                linesList.Add(lines);
                view.Children.Add(lines);
            }

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
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250,250,216));
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
            if (points != null)
            {
                //hook up the points collection to the visual
                //if (isDrawingPoints)
                //{
                    points.Points = Points[0];
                //}
                //else
                //{
                    for(int i=0; i<linesList.Count(); i++)
                    {
                        linesList[i].Points = Points[i];
                    }

                    fixedPoints.Points = FixedPoints;
                //}
                
            }
        }                                                                                                                                                                                                         

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            NodeUI.Dispatcher.Invoke(new Action(
               delegate
               {
                    //If we are receiving a list, we test to see if they are XYZs or curves and then make Preview3d elements.
                    if (input.IsList)
                    {
                        DetachVisuals();
                        ClearPointsCollections();

                        DrawList(input);
                    }
                    else if (input.IsContainer) //if not a list, try to cast to either a particle system, curve, or xyz
                    {
                        var test = ((Value.Container)(input)).Item;

                        if (test is ParticleSystem)
                        {
                            ps = (ParticleSystem)test;

                            try
                            {
                                UpdateVisualsFromParticleSystem();
                                RaisePropertyChanged("Points");
                            }
                            catch (Exception e)
                            {
                                Bench.Log("Something wrong drawing 3d preview. " + e.ToString());
                            }
                        }
                        else if (test is Curve)
                        {
                            DetachVisuals();
                            ClearPointsCollections();

                            c = (Curve)test;
                            DrawCurve(c);
                            RaisePropertyChanged("Points");
                        }
                        else if (test is XYZ)
                        {

                            DetachVisuals();
                            ClearPointsCollections();

                            pt = (XYZ)test;
                            DrawCurve(c);
                            RaisePropertyChanged("Points");
                        }
                    }
               }));

            return input; //watch 3d should be a 'pass through' node
            
        }

        private void DrawList(Value input)
        {
            #region points and curves
            //FSharpList<Value> list = ((Value.List)args[0]).Item;
            var inList = ((Value.List)input).Item;

            foreach (Value e in inList)
            {
                if (e.IsList)
                {
                    DrawList(e);
                }
                else if (e.IsContainer)
                {
                    XYZ ptTest = (inList.First() as Value.Container).Item as XYZ;
                    Curve cvTest = (inList.First() as Value.Container).Item as Curve;

                    if (ptTest != null)
                    {
                        pt = (e as Value.Container).Item as XYZ;
                        DrawPoint(pt);
                    }
                    else if (cvTest != null)
                    {
                        c = (e as Value.Container).Item as Curve;
                        DrawCurve(c);
                    }
                }
            }
            
            RaisePropertyChanged("Points");
            #endregion
        }

        private void DrawPoint(XYZ pt)
        {
            int lastPointColor = Points.Count() - 1;//master Point list for color assignment
            var ptVis = new Point3D(pt.X, pt.Y, pt.Z);
            //Points[lastPointColor].Add(ptVis);
            FixedPoints.Add(ptVis);
        }

        private void DrawCurve(Curve c)
        {
            List<XYZ> points;

            points = c.Tessellate() as List<XYZ>;
            XYZ pt1;
            XYZ pt2;
            Point3D ptVis1;
            Point3D ptVis2;
            int lastPointColor = Points.Count() - 1;//master Point list for color assignment
            for (int i = 0; i < points.Count - 1; i++)
            {
                pt1 = points[i] as XYZ;
                pt2 = points[i + 1] as XYZ;
                ptVis1 = new Point3D(pt1.X, pt1.Y, pt1.Z);
                ptVis2 = new Point3D(pt2.X, pt2.Y, pt2.Z);
                Points[lastPointColor].Add(ptVis1);
                Points[lastPointColor].Add(ptVis2);
            }
        }

        private void DrawSurface(Face f)
        {
            Mesh mesh;
            //Mesh3D helixMesh;

            mesh = f.Triangulate() as Mesh;
            XYZ vertex0;
            XYZ vertex1;
            XYZ vertex2;

            Point3D ptVis0;
            Point3D ptVis1;
            Point3D ptVis2;
            int lastPointColor = Points.Count() - 1;//master Point list for color assignment
            for (int i = 0; i < mesh.NumTriangles - 1; i++)
            {
                vertex0 =  mesh.get_Triangle(i).get_Vertex(0);
                vertex1 =  mesh.get_Triangle(i).get_Vertex(1);
                vertex2 = mesh.get_Triangle(i).get_Vertex(2);
                ptVis0 = new Point3D(vertex0.X, vertex0.Y, vertex0.Z);
                ptVis1 = new Point3D(vertex1.X, vertex1.Y, vertex1.Z);
                ptVis2 = new Point3D(vertex2.X, vertex2.Y, vertex2.Z);
               // TriOrQuadFacet
               // helixMesh.AddFace(

            }
        }

        private void UpdateVisualsFromParticleSystem()
        {
            DetachVisuals();
            ClearPointsCollections();

            ParticleSpring s;

            Particle springEnd1;
            Particle springEnd2;
            
            //draw springs as geometry curves
            for (int i = 0; i < ps.numberOfSprings(); i++)
            {
                s = ps.getSpring(i);
                springEnd1 = s.getOneEnd();
                springEnd2 = s.getTheOtherEnd();
                
                var ptVis1 = new Point3D(springEnd1.getPosition().X, springEnd1.getPosition().Y, springEnd1.getPosition().Z);
                var ptVis2 = new Point3D(springEnd2.getPosition().X, springEnd2.getPosition().Y, springEnd2.getPosition().Z);

                if (!springEnd1.isFree())
                    FixedPoints.Add(ptVis1);
                if (!springEnd2.isFree())
                    FixedPoints.Add(ptVis2);

                AddPointToCorrectCollection(s.getResidualForce(), ptVis1, ptVis2);
            }

            view.DebugInfo = string.Format("Max. Residual Force = {0:0.##}\n Max. Nodal Velocity = {1:0.##}", ps.getMaxResidualForce(), ps.getMaxNodalVelocity());
        }

        private void ClearPointsCollections()
        {
            foreach (Point3DCollection pts in Points)
            {
                pts.Clear();
            }

            FixedPoints.Clear();
        }

        private void DetachVisuals()
        {
            points.Points = null;
            fixedPoints.Points = null;

            foreach (LinesVisual3D lines in linesList)
            {
                lines.Points = null;
            }
        }

        private void AddPointToCorrectCollection(double force, Point3D pt1, Point3D pt2)
        {

            int maxColorIndex = colors.Count()-1;

            double forceNormalized = force / ps.getMaxResidualForce();
            int forceGroup = (int)Math.Round(forceNormalized * 9.0, 0, MidpointRounding.AwayFromZero);
            if (forceGroup > maxColorIndex || forceGroup < 0)
            {
                Points[0].Add(pt1);
                Points[0].Add(pt2);
            }
            else
            {
                Points[forceGroup].Add(pt1);
                Points[forceGroup].Add(pt2);
            }
        }
    }
}
