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
            OutPortData = new PortData("OUT", "3D output", typeof(object));

            base.RegisterInputsAndOutputs();

            //take out the left and right margins
            //and make this so it's not so wide
            this.inputGrid.Margin = new Thickness(10, 5, 10, 5);
            this.topControl.Width = 400;
            this.topControl.Height = 300;

            //add a 3D viewport to the input grid
            //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
            view = new HelixViewport3D();
            view.DataContext = this;
            view.CameraRotationMode = CameraRotationMode.Trackball;
            view.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            view.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //view.IsHitTestVisible = true;

            points = new PointsVisual3D { Color = Colors.Red, Size = 6 };
            view.Children.Add(points);

            this.inputGrid.Children.Add(view);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (points != null)
            {
                //hook up the points collection to the visual
                points.Points = Points;
            }
        }                                                                                                                                                                                                         

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FSharpList<Expression> list = ((Expression.List)args[0]).Item;
            
            var input = args[0];

            this.Dispatcher.Invoke(new Action(
               delegate
               {
                    //If we are receiving a list, we must create reference points for each XYZ in the list.
                    if (input.IsList)
                    { 
                        var xyzList = (input as Expression.List).Item;

                        //initialize the point collection
                        if (Points == null)
                        {
                            Points = new Point3DCollection();
                        }
                        else
                            Points.Clear();

                        foreach (Expression e in xyzList)
                        {
                            XYZ pt = (XYZ)(e as Expression.Container).Item;
                            var ptVis = new Point3D(pt.X, pt.Y, pt.Z);
                            Points.Add(ptVis);
                        }
                        RaisePropertyChanged("Points");
                        view.ZoomExtents();
                    }
               }));

            return Expression.NewContainer(null);
        }

    }
}
