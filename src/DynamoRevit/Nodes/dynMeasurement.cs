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
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Data;

using Autodesk.Revit.DB;

using Microsoft.FSharp.Collections;

using Dynamo.Utilities;
using Dynamo.Revit;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Controls;

namespace Dynamo.Nodes
{
    public abstract class dynMeasurementBase:dynNodeWithOneOutput
    {
        protected dynMeasurementBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Surface Area")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("An element which measures the surface area of a face (f)")]
    public class dynSurfaceArea : dynMeasurementBase
    {
        public dynSurfaceArea()
        {
            InPortData.Add(new PortData("f", "The face whose surface area you wish to calculate (Reference).", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("a", "The surface area of the face (Number).", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double area = 0.0;

            object arg0 = ((Value.Container)args[0]).Item;

            Face f;

            Reference faceRef = arg0 as Reference;
            if (faceRef != null)
                f = dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
            else
                f = arg0 as Face;

            if (f != null)
            {
                area = f.Area;
            }

            //Fin
            return Value.NewNumber(area);
        }
    }

    [NodeName("Surface Domain")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("An element which measures the domain of a surface in U and V.")]
    public class dynSurfaceDomain : dynRevitTransactionNodeWithOneOutput
    {
        public dynSurfaceDomain()
        {
            InPortData.Add(new PortData("f", "The surface whose domain you wish to calculate (Reference).", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("d", "The min, max, and dimensions of the surface domain. (List)", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> result = FSharpList<Value>.Empty;
            BoundingBoxUV bbox = null;

            object arg0 = ((Value.Container)args[0]).Item;

            Face f;

            Reference faceRef = arg0 as Reference;
            if (faceRef != null)
                f = this.UIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
            else
                f = arg0 as Face;

            if (f != null)
            {
                bbox = f.GetBoundingBox();
            }

            result = FSharpList<Value>.Cons(
                           Value.NewNumber(bbox.Max.V - bbox.Min.V),
                           result);
            result = FSharpList<Value>.Cons(
                           Value.NewNumber(bbox.Max.U - bbox.Min.U),
                           result);
            result = FSharpList<Value>.Cons(
                           Value.NewContainer(bbox.Max),
                           result);
            result = FSharpList<Value>.Cons(
                           Value.NewContainer(bbox.Min),
                           result);
            
            //Fin
            return Value.NewList(result);
        }
    }

    [NodeName("XYZ Distance")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Returns the distance between a(XYZ) and b(XYZ).")]
    public class dynXYZDistance : dynMeasurementBase
    {
        public dynXYZDistance()
        {
            InPortData.Add(new PortData("a", "Start (XYZ).", typeof(Value.Container)));//Ref to a face of a form
            InPortData.Add(new PortData("b", "End (XYZ)", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("d", "The distance between the two XYZs (Number).", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var a = (XYZ)((Value.Container)args[0]).Item;
            var b = (XYZ)((Value.Container)args[1]).Item;

            return Value.NewNumber(a.DistanceTo(b));
        }
    }

    [NodeName("Height")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Returns the height in z of an element.")]
    public class dynHeight : dynMeasurementBase
    {
        public dynHeight()
        {
            InPortData.Add(new PortData("elem", "Level, Family Instance, RefPoint, XYZ", typeof(Value.Container)));//add elements here when adding switch statements 
            OutPortData.Add(new PortData("h", "The height of an element in z relative to project 0.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        private static double getHeight(object elem)
        {
            double h = 0;

            if (elem is Level)
            {
                h = ((Level)elem).Elevation;
                return h;
            }
            else if (elem is ReferencePoint)
            {
                h = ((ReferencePoint)elem).Position.Z;
                return h;
            }
            else if (elem is FamilyInstance)
            {
                LocationPoint loc =  (LocationPoint)((FamilyInstance)elem).Location;
                h = loc.Point.Z;
                return h;
            }
            else if (elem is XYZ)
            {
                h = ((XYZ)elem).Z;
                return h;
            }
            else
            {
                return h;
            }

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var a = ((Value.Container)args[0]).Item;

            return Value.NewNumber(getHeight(a));
        }
    }

    [NodeName("Ref Point Dist")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Measures a distance between point(s).")]
    public class dynDistanceBetweenPoints : dynMeasurementBase
    {
        public dynDistanceBetweenPoints()
        {
            InPortData.Add(new PortData("ptA", "Element to measure to.", typeof(Value.Container)));
            InPortData.Add(new PortData("ptB", "A Reference point.", typeof(Value.Container)));

            OutPortData.Add(new PortData("dist", "Distance between points.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        private XYZ getXYZ(object arg)
        {
            if (arg is ReferencePoint)
            {
                return (arg as ReferencePoint).Position;
            }
            else if (arg is FamilyInstance)
            {
                return ((arg as FamilyInstance).Location as LocationPoint).Point;
            }
            else if (arg is XYZ)
            {
                return arg as XYZ;
            }
            else
            {
                throw new Exception("Cannot cast argument to ReferencePoint or FamilyInstance or XYZ.");
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Grab our inputs and turn them into XYZs.
            XYZ ptA = this.getXYZ(((Value.Container)args[0]).Item);
            XYZ ptB = this.getXYZ(((Value.Container)args[1]).Item);

            //Return the calculated distance.
            return Value.NewNumber(ptA.DistanceTo(ptB));
        }
    }

    /*
    [NodeName("Length")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Enter a length in project units.")]
    public class dynLengthInput : dynDouble
    {
        public dynLengthInput()
        {
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView NodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = false;
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new RevitProjectUnitsConverter(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, bindingVal);

            tb.Text = "0.0";
        }

        public override double Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (base.Value == value)
                    return;

                base.Value = value;
                //RaisePropertyChanged("Value");
            }
        }

        protected override double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val);
            }
            catch
            {
                return 0;
            }
        }
    }*/
}


