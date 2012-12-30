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
        LinesVisual3D lines;
        bool isDrawingPoints;
        ParticleSystem ps;

        public Point3DCollection Points{get;set;}
 
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
            view.DebugInfo = "This is some debug info.";
            
            points = new PointsVisual3D { Color = Colors.Black, Size = 6 };
            lines = new LinesVisual3D { Color = Colors.LightGray, Thickness = 2 };

            view.Children.Add(points);
            view.Children.Add(lines);

            this.inputGrid.Children.Add(view);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
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
                    points.Points = Points;
                }
                else
                {
                    lines.Points = Points;
                    //points.Points = Points;
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

                        //initialize the point collection
                        if (Points == null)
                        {
                            Points = new Point3DCollection();
                        }
                        else
                            Points.Clear();

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
                                Points.Add(ptVis);
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
                                    Points.Add(ptVis1);
                                    Points.Add(ptVis2);
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
                            ps = (ParticleSystem)(psTest).Item; //XYZ xyz = (XYZ)((Expression.Container)input).Item;

                            UpdateVisualsFromParticleSystem();

                            RaisePropertyChanged("Points");
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
            points.Points = null;
            lines.Points = null;

            if (Points == null)
            {
                //initialize the point collection
                Points = new Point3DCollection();
            }
            else
            {
                Points.Clear();
            }

            Particle p;
            ParticleSpring s;

            Particle springEnd1;
            Particle springEnd2;
            
            //draw curves as geometry curves
            for (int i = 0; i < ps.numberOfSprings(); i++)
            {
                s = ps.getSpring(i);
                springEnd1 = s.getOneEnd();
                springEnd2 = s.getTheOtherEnd();

                var ptVis1 = new Point3D(springEnd1.getPosition().X, springEnd1.getPosition().Y, springEnd1.getPosition().Z);
                var ptVis2 = new Point3D(springEnd2.getPosition().X, springEnd2.getPosition().Y, springEnd2.getPosition().Z);
                Points.Add(ptVis1);
                Points.Add(ptVis2);
            }
        }

    }
}
