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
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.FSharp.Collections;
using System.IO.Ports;
using Dynamo.Connectors;
using Expression = Dynamo.FScheme.Expression;
using HelixToolkit.Wpf;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;

namespace Dynamo.Elements
{
    [ElementName("Watch 3D")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Shows a dynamic preview of geometry.")]
    [RequiresTransaction(false)]
    public class dyn3DPreview : dynNode, INotifyPropertyChanged
    {
        HelixViewport3D view;
        PointsVisual3D points;
        PointsVisual3D fixedPoints;
        List<LinesVisual3D> linesList;
        System.Windows.Point rightMousePoint;

        bool isDrawingPoints;
        ParticleSystem ps;

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
            OutPortData = new PortData("OUT", "Watch contents, passed through", typeof(object));

            base.RegisterInputsAndOutputs();

            //get rid of right click delete
            this.MainContextMenu.Items.Clear();

            MenuItem mi = new MenuItem();
            mi.Header = "Zoom to Fit";
            mi.Click +=new RoutedEventHandler(mi_Click);

            this.MainContextMenu.Items.Add(mi);

            //take out the left and right margins
            //and make this so it's not so wide
            this.inputGrid.Margin = new Thickness(10, 5, 10, 5);
            this.topControl.Width = 400;
            this.topControl.Height = 300;
            this.elementShine.Visibility = System.Windows.Visibility.Hidden;
            this.elementRectangle.Visibility = System.Windows.Visibility.Hidden;

            //add a 3D viewport to the input grid
            //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
            view = new HelixViewport3D();
            view.DataContext = this;
            view.CameraRotationMode = CameraRotationMode.Turntable;
            view.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            view.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //view.IsHitTestVisible = true;
            view.ShowFrameRate = true;

            view.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            view.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            fixedPoints = new PointsVisual3D { Color = Colors.Red, Size = 8 };
            view.Children.Add(fixedPoints);

            points = new PointsVisual3D { Color = Colors.Black, Size = 4 };
            view.Children.Add(points);

            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(Colors.LightGray); //01
            colors.Add(Colors.LightBlue); //02
            colors.Add(Colors.Blue); //03
            colors.Add(Colors.Purple); //04
            colors.Add(Colors.LightGreen); //05
            colors.Add(Colors.GreenYellow); //06
            colors.Add(Colors.Yellow); //07
            colors.Add(Colors.Orange); //08
            colors.Add(Colors.OrangeRed); //09
            colors.Add(Colors.Red); //10

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

            this.inputGrid.Children.Add(view);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void view_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            rightMousePoint = e.GetPosition(this.topControl);
        }

        void view_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right
            //click, we assume rotation. handle the event
            //so we don't show the context menu
            if (e.GetPosition(this.topControl) != rightMousePoint)
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
                if (isDrawingPoints)
                {
                    points.Points = Points[0];
                }
                else
                {
                    for(int i=0; i<linesList.Count(); i++)
                    {
                        linesList[i].Points = Points[i];
                    }

                    fixedPoints.Points = FixedPoints;
                }
                
            }
        }                                                                                                                                                                                                         

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];

            this.Dispatcher.Invoke(new Action(
               delegate
               {
                    //If we are receiving a list, we test to see if they are XYZs or curves and then make Preview3d elements.
                    if (input.IsList)
                    {
                        #region points and curves
                        //FSharpList<Expression> list = ((Expression.List)args[0]).Item;
                        var inList = (input as Expression.List).Item;

                        DetachVisuals();
                        ClearPointsCollections();

                        //test the first item in the list.
                        //if it's an XYZ, assume XYZs for the list
                        //create points. otherwise, create curves
                        XYZ ptTest = (inList.First() as Expression.Container).Item as XYZ;
                        Curve cvTest = (inList.First() as Expression.Container).Item as Curve;

                        if (ptTest != null) isDrawingPoints = true;

                        foreach (Expression e in inList)
                        {
                            if (isDrawingPoints)
                            {
                                XYZ pt = (e as Expression.Container).Item as XYZ;
                                var ptVis = new Point3D(pt.X, pt.Y, pt.Z);
                                Points[0].Add(ptVis);
                            }
                            else
                            {
                                Curve c = (e as Expression.Container).Item as Curve;
                                if (c.GetType() == typeof(Line))

                                {
                                    XYZ start = c.get_EndPoint(0);
                                    XYZ end = c.get_EndPoint(1);
                                    var ptVis1 = new Point3D(start.X, start.Y, start.Z);
                                    var ptVis2 = new Point3D(end.X, end.Y, end.Z);
                                    Points[0].Add(ptVis1);
                                    Points[0].Add(ptVis2);
                                }
                                else if (c.GetType() == typeof(HermiteSpline))
                                {
                                    //TODO:
                                }
                            }
                        }
                        RaisePropertyChanged("Points");
                        #endregion
                    }
                    else if (input.IsContainer) //if not a list, presume it's a a particle system
                    {
                        var psTest = input as Expression.Container;

                        if ((ParticleSystem)(psTest).Item is ParticleSystem)
                        {
                            ps = (ParticleSystem)(psTest).Item;

                            try
                            {

                                UpdateVisualsFromParticleSystem();

                                RaisePropertyChanged("Points");

                            }
                            catch (Exception e)
                            {
                                dynElementSettings.SharedInstance.Bench.Log("Something wrong drawing 3d preview. " + e.ToString());

                            }
                        }
                        else
                        {
                            //please pass in a list of XYZs, Curves or a single particle system
                        }
                    }
               }));

            //return Expression.NewContainer(input); //watch 3d should be a 'pass through' node 
            return input; //watch 3d should be a 'pass through' node
            
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
            int maxColors = linesList.Count();
            double forceNormalized = force / ps.getMaxResidualForce();
            int forceGroup = (int)(forceNormalized * 9.0);
            if (forceGroup > maxColors) //maxColors  - points and colors array arrays are sized at 10, somehow we are going out of bounds here, clamp it to prevent hard crash
            {
                dynElementSettings.SharedInstance.Bench.Log("Had to clamp forces for display. Original Value: " + forceGroup.ToString());
                forceGroup = 9; //clamp

            }
            //int forceGroup = 9;
            Points[forceGroup].Add(pt1);
            Points[forceGroup].Add(pt2);
        }
    }
}
