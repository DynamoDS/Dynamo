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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using System.Collections;

namespace Dynamo.Utilities
{
    public static class dynSettings
    {
        //colors taken from:
        //http://cloford.com/resources/colours/500col.htm
        //http://linaker-wall.net/Colour/Dynamic_Fmt/Swatch_rgb_numbers.htm
        static System.Windows.Media.Color colorGreen1 = System.Windows.Media.Color.FromRgb(193, 255, 193);
        static System.Windows.Media.Color colorGreen2 = System.Windows.Media.Color.FromRgb(155, 250, 155);
        static System.Windows.Media.Color colorRed1 = System.Windows.Media.Color.FromRgb(255, 64, 64);
        static System.Windows.Media.Color colorRed2 = System.Windows.Media.Color.FromRgb(205, 51, 51);
        //System.Windows.Media.Color colorOrange1 = System.Windows.Media.Color.FromRgb(255, 193, 37);
        //System.Windows.Media.Color colorOrange2 = System.Windows.Media.Color.FromRgb(238, 180, 34);
        static System.Windows.Media.Color colorOrange1 = System.Windows.Media.Color.FromRgb(255, 207, 98);
        static System.Windows.Media.Color colorOrange2 = System.Windows.Media.Color.FromRgb(235, 187, 78);
        static System.Windows.Media.Color colorGray1 = System.Windows.Media.Color.FromRgb(220, 220, 220);
        static System.Windows.Media.Color colorGray2 = System.Windows.Media.Color.FromRgb(192, 192, 192);

        public static Dynamo.Controls.DragCanvas Workbench
        {
            get;
            internal set;
        }

        public static dynCollection Collection
        {
            get;
            internal set;
        }

        public static LinearGradientBrush ErrorBrush
        {
            get;
            internal set;
        }

        public static LinearGradientBrush ActiveBrush
        {
            get;
            internal set;
        }

        public static LinearGradientBrush SelectedBrush
        {
            get;
            internal set;
        }

        public static LinearGradientBrush DeadBrush
        {
            get;
            internal set;
        }

        public static dynBench Bench
        {
            get;
            internal set;
        }

        public static TextWriter Writer
        {
            get;
            set;
        }

        /*
        public dynElementSettings(Autodesk.Revit.UI.UIApplication app, Autodesk.Revit.UI.UIDocument doc, Level defaultLevel, DynamoWarningSwallower warningSwallower, Transaction t)
       {

           this.revit = app;
           this.doc = doc;
           this.defaultLevel = defaultLevel;
           this.warningSwallower = warningSwallower;
           this.trans = t;

            SetupBrushes();

       }
        */

        static dynSettings()
        {
            SetupBrushes();
        }

        static void SetupBrushes()
        {
            ErrorBrush = new LinearGradientBrush();
            ErrorBrush.StartPoint = new System.Windows.Point(0.5, 0);
            ErrorBrush.EndPoint = new System.Windows.Point(0.5, 1);
            ErrorBrush.GradientStops.Add(new GradientStop(colorRed1, 0.0));
            ErrorBrush.GradientStops.Add(new GradientStop(colorRed2, .25));
            ErrorBrush.GradientStops.Add(new GradientStop(colorRed2, 1.0));

            ActiveBrush = new LinearGradientBrush();
            ActiveBrush.StartPoint = new System.Windows.Point(0.5, 0);
            ActiveBrush.EndPoint = new System.Windows.Point(0.5, 1);
            ActiveBrush.GradientStops.Add(new GradientStop(colorOrange1, 0.0));
            ActiveBrush.GradientStops.Add(new GradientStop(colorOrange2, .25));
            ActiveBrush.GradientStops.Add(new GradientStop(colorOrange2, 1.0));

            SelectedBrush = new LinearGradientBrush();
            SelectedBrush.StartPoint = new System.Windows.Point(0.5, 0);
            SelectedBrush.EndPoint = new System.Windows.Point(0.5, 1);
            SelectedBrush.GradientStops.Add(new GradientStop(colorGreen1, 0.0));
            SelectedBrush.GradientStops.Add(new GradientStop(colorGreen2, .25));
            SelectedBrush.GradientStops.Add(new GradientStop(colorGreen2, 1.0));

            DeadBrush = new LinearGradientBrush();
            DeadBrush.StartPoint = new System.Windows.Point(0.5, 0);
            DeadBrush.EndPoint = new System.Windows.Point(0.5, 1);
            DeadBrush.GradientStops.Add(new GradientStop(colorGray1, 0.0));
            DeadBrush.GradientStops.Add(new GradientStop(colorGray2, .25));
            DeadBrush.GradientStops.Add(new GradientStop(colorGray2, 1.0));
        }
        
        public static DynamoController Controller { get; internal set; }

        public static PackageManagerClient PackageManagerClient { get; internal set; }

        public static Dictionary<Guid, FunctionDefinition> FunctionDict = 
            new Dictionary<Guid, FunctionDefinition>();

        public static HashSet<FunctionDefinition> FunctionWasEvaluated =
            new HashSet<FunctionDefinition>();
    }
}