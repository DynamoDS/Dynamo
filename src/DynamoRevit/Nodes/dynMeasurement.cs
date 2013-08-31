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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Xml;
using Autodesk.Revit.DB;
using DSCoreNodes;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

using Dynamo.Utilities;
using Value = Dynamo.FScheme.Value;
using Dynamo.Controls;
using Curve = Autodesk.Revit.DB.Curve;
using Vector = Autodesk.LibG.Vector;

namespace Dynamo.Nodes
{
    public abstract class MeasurementBase:NodeWithOneOutput
    {
        protected MeasurementBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Surface Area")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("An element which measures the surface area of a face (f)")]
    public class SurfaceArea : MeasurementBase
    {
        public SurfaceArea()
        {
            InPortData.Add(new PortData("f", "The face whose surface area you wish to calculate (Reference).", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("a", "The surface area of the face (Number).", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double area = 0.0;

            object arg0 = ((Value.Container)args[0]).Item;

            Autodesk.Revit.DB.Face f;

            Reference faceRef = arg0 as Reference;
            if (faceRef != null)
                f = dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;
            else
                f = arg0 as Autodesk.Revit.DB.Face;

            if (f != null)
            {
                area = f.Area;
            }

            //Fin
            return Value.NewNumber(area);
        }
    }

    [NodeName("Get Surface Domain")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Measure the domain of a surface in U and V.")]
    public class SurfaceDomain : NodeWithOneOutput
    {
        public SurfaceDomain()
        {
            InPortData.Add(new PortData("f", "The surface whose domain you wish to calculate (Reference).", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("domain", "The surface's domain.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            BoundingBoxUV bbox = null;

            object arg0 = ((Value.Container)args[0]).Item;

            Autodesk.Revit.DB.Face f;

            var faceRef = arg0 as Reference;
            if (faceRef != null)
                f = dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;
            else
                f = arg0 as Autodesk.Revit.DB.Face;

            if (f != null)
            {
                bbox = f.GetBoundingBox();
            }

            var min = Vector.by_coordinates(bbox.Min.U, bbox.Min.V);
            var max = Vector.by_coordinates(bbox.Max.U, bbox.Max.V);

            return Value.NewContainer(DSCoreNodes.Domain2D.ByMinimumAndMaximum(min, max));
        }
    }

    [NodeName("Get Curve Domain")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Measure the domain of a curve.")]
    public class CurveDomain : NodeWithOneOutput
    {
        public CurveDomain()
        {
            InPortData.Add(new PortData("curve", "The curve whose domain you wish to calculate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("domain", "The curve's domain.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var curveRef = ((Value.Container)args[0]).Item as Reference;

            Curve curve = null;

            var el = ((Value.Container) args[0]).Item as CurveElement;
            if (el != null)
            {
                var crvEl = el;
                curve = crvEl.GeometryCurve;
            }
            else
            {
                curve = curveRef == null
                              ? (Curve)((Value.Container)args[0]).Item
                              : (Curve)
                                dynRevitSettings.Doc.Document.GetElement(curveRef.ElementId)
                                                .GetGeometryObjectFromReference(curveRef);
            }
            

            var start = curve.get_EndParameter(0);
            var end = curve.get_EndParameter(1);

            return Value.NewContainer(DSCoreNodes.Domain.ByMinimumAndMaximum(start, end));
        }
    }

    [NodeName("XYZ Distance")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Returns the distance between a(XYZ) and b(XYZ).")]
    public class XyzDistance : MeasurementBase
    {
        public XyzDistance()
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
    public class Height : MeasurementBase
    {
        public Height()
        {
            InPortData.Add(new PortData("elem", "Level, Family Instance, RefPoint, XYZ", typeof(Value.Container)));//add elements here when adding switch statements 
            OutPortData.Add(new PortData("h", "The height of an element in z relative to project 0.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        private static double getHeight(object elem)
        {
            double h = 0;

            if (elem is Autodesk.Revit.DB.Level)
            {
                h = ((Autodesk.Revit.DB.Level)elem).Elevation;
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

    [NodeName("Reference Point Distance")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeSearchTags("Distance", "dist", "norm")]
    [NodeDescription("Measures a distance between point(s).")]
    public class DistanceBetweenPoints : MeasurementBase
    {
        public DistanceBetweenPoints()
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

    [NodeName("Length")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Enter a length in project units.")]
    [NodeSearchTags("Imperial", "Metric", "Length", "Project", "units")]
    public class LengthInput : NodeWithOneOutput
    {
        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }

        public LengthInput()
        {
            OutPortData.Add(new PortData("length", "The length. Stored internally as decimal feet.", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewNumber(Value);
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem();
            editWindowItem.Header = "Edit...";
            editWindowItem.IsCheckable = false;

            nodeUI.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += new RoutedEventHandler(editWindowItem_Click);
            //add a text box to the input grid of the control
            var tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = false;
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            
            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new RevitProjectUnitsConverter(),
                //ConverterParameter = Measure,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, bindingVal);
            
            tb.OnChangeCommitted += delegate { RequiresRecalc = true; };
        }

        private void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new dynEditWindow();

            editWindow.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new RevitProjectUnitsConverter(),
                //ConverterParameter = Measure,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            editWindow.editText.SetBinding(System.Windows.Controls.TextBox.TextProperty, bindingVal);

            if (editWindow.ShowDialog() != true)
            {
                return;
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            dynEl.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                // this node now stores a double, having previously stored a measure type
                // by checking for the measure type as well we allow for loading of older files.
                if (subNode.Name.Equals(typeof(double).FullName) || subNode.Name.Equals("Dynamo.Measure.Foot"))
                {
                    Value = DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        protected double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }
    }
}


