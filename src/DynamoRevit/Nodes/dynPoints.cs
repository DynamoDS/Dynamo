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
using System.Linq;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Dynamo.FSchemeInterop;

using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Reference Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates a reference point.")]
    [NodeSearchTags("pt","ref")]
    public class dynReferencePointByXYZ : dynRevitTransactionNodeWithOneOutput
    {
        public dynReferencePointByXYZ()
        {
            InPortData.Add(new PortData("xyz", "The point(s) from which to create reference points.", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "The Reference Point(s) created from this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var xyz = (XYZ)((Value.Container)input).Item;

            ReferencePoint pt;

            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0],typeof(ReferencePoint), out e))
                {
                    //..and if we do, update it's position.
                    pt = (ReferencePoint)e;
                    pt.Position = xyz;
                }
                else
                {
                    pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                    this.Elements[0] = pt.Id;
                }
            }
            else
            {
                pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                this.Elements.Add(pt.Id);
            }

            return Value.NewContainer(pt);
        }
    }

    [NodeName("Reference Point On Edge")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an element which owns a reference point on a selected edge.")]
    [NodeSearchTags("ref", "pt")]
    public class dynPointOnEdge : dynRevitTransactionNodeWithOneOutput
    {
        public dynPointOnEdge()
        {
            InPortData.Add(new PortData("curve", "ModelCurve", typeof(Value.Container)));
            InPortData.Add(new PortData("t", "Parameter on edge.", typeof(Value.Number)));
            OutPortData.Add(new PortData("pt", "PointOnEdge", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Reference r = ((CurveElement)((Value.Container)args[0]).Item).GeometryCurve.Reference;

            double t = ((Value.Number)args[1]).Item;
            //Autodesk.Revit.DB..::.PointElementReference
            //Autodesk.Revit.DB..::.PointOnEdge
            //Autodesk.Revit.DB..::.PointOnEdgeEdgeIntersection
            //Autodesk.Revit.DB..::.PointOnEdgeFaceIntersection
            //Autodesk.Revit.DB..::.PointOnFace
            //Autodesk.Revit.DB..::.PointOnPlane
            PointLocationOnCurve plc = new PointLocationOnCurve(PointOnCurveMeasurementType.NormalizedCurveParameter, t, PointOnCurveMeasureFrom.Beginning);
            PointElementReference edgePoint = this.UIDocument.Application.Application.Create.NewPointOnEdge(r, plc);

            ReferencePoint p;

            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0],typeof(ReferencePoint), out e))
                {
                    p = e as ReferencePoint;
                    p.SetPointElementReference(edgePoint);
                }
                else
                {
                    p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                    this.Elements[0] = p.Id;
                }
            }
            else
            {
                p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                this.Elements.Add(p.Id);
            }
            
            return Value.NewContainer(p);
        }
    }

    [NodeName("Reference Point On Face by UV")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an element which owns a reference point on a selected face.")]
    [NodeSearchTags("ref", "pt")]
    public class dynPointOnFaceUV : dynRevitTransactionNodeWithOneOutput
    {
        public dynPointOnFaceUV()
        {
            InPortData.Add(new PortData("face", "ModelFace", typeof(Value.Container)));
            InPortData.Add(new PortData("UV", "UV Parameter on face.", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "PointOnFace", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            UV uv = ((Value.Container)args[1]).Item as UV;

            object arg0 = ((Value.Container)args[0]).Item;

            Face f;
            var r = arg0 as Reference;
            if (r != null)
                f = (Face)dynRevitSettings.Doc.Document.GetElement(r.ElementId).GetGeometryObjectFromReference(r);
            else
                f = (Face)arg0;

            var facePoint = f.Evaluate(uv);

            ReferencePoint pt = null;

            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0],typeof(ReferencePoint), out e))
                {
                    pt = (ReferencePoint)e;
                    pt.Position = facePoint;
                }
                else
                {
                    if (this.UIDocument.Document.IsFamilyDocument)
                    {
                        pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(facePoint);
                        this.Elements[0] = pt.Id;
                    }
                }
            }
            else
            {
                if (this.UIDocument.Document.IsFamilyDocument)
                {
                    pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(facePoint);
                    this.Elements.Add(pt.Id);
                }
            }

            return Value.NewContainer(pt);
        }
    }

    [NodeName("Reference Point By Normal")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Owns a reference point which is projected from a point by normal and distance.")]
    [NodeSearchTags("normal", "ref")]
    public class dynPointNormalDistance : dynRevitTransactionNodeWithOneOutput
    {
        public dynPointNormalDistance()
        {
            InPortData.Add(new PortData("pt", "The point to reference", typeof(Value.Container)));
            InPortData.Add(new PortData("norm", "The normal", typeof(Value.Container)));
            InPortData.Add(new PortData("d", "The offset distance", typeof(Value.Number)));
            OutPortData.Add(new PortData("pt", "Point", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pt = (ReferencePoint)((Value.Container)args[0]).Item;
            var norm = (XYZ)((Value.Container)args[1]).Item;
            double dist = ((Value.Number)args[2]).Item;

            ReferencePoint p;

            var newLocation = pt.Position + norm.Normalize().Multiply(dist);

            if (Elements.Any())
            {
                Element el;
                if (dynUtils.TryGetElement(Elements[0], typeof (ReferencePoint), out el))
                {
                    //move the point to the new offset
                    p = (ReferencePoint) el;
                    p.Position = newLocation;
                    Elements[0] = p.Id;
                }
                else
                {
                    p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(newLocation);
                    Elements[0] = p.Id;
                }
            }
            else
            {
                p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(newLocation);
                this.Elements.Add(p.Id);
            }

            return Value.NewContainer(p);
        }

    }

    [NodeName("Plane from Reference Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Extracts one of the primary Reference Planes from a Reference Point.")]
    [NodeSearchTags("ref")]
    public class dynPlaneFromRefPoint : dynRevitTransactionNodeWithOneOutput
    {
        ComboBox combo;

        public dynPlaneFromRefPoint()
        {
            InPortData.Add(new PortData("pt", "The point to extract the plane from", typeof(Value.Container)));
            OutPortData.Add(new PortData("r", "Reference", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
        {
            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.RequiresRecalc = true;
            };

            PopulateComboBox();
        }

        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateComboBox();
        }
        public enum RefPointReferencePlanes { XY, YZ, XZ };

        private void PopulateComboBox()
        {

            combo.Items.Clear();

            foreach (var plane in Enum.GetValues(typeof(RefPointReferencePlanes)))
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = plane.ToString();
                combo.Items.Add(cbi);
            }

        }


        public static XYZ TransformPoint(XYZ point, Transform transform)
        {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            //transform basis of the old coordinate system in the new coordinate // system
            XYZ b0 = transform.get_Basis(0);
            XYZ b1 = transform.get_Basis(1);
            XYZ b2 = transform.get_Basis(2);
            XYZ origin = transform.Origin;

            //transform the origin of the old coordinate system in the new 
            //coordinate system
            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            foreach (ElementId el in this.Elements)
            {
                Element e;
                if (dynUtils.TryGetElement(el,typeof(object), out e))
                {
                    this.UIDocument.Document.Delete(el);
                }
            }
            
            //Plane p = null;
            Reference r = null;
            ReferencePoint pt = ((Value.Container)args[0]).Item as ReferencePoint;

            int n = combo.SelectedIndex;
            switch (n)
            {
                case 0: //combo.SelectedValue == "XY"
                    r = pt.GetCoordinatePlaneReferenceXY();
                    break;
                case 1: //combo.SelectedValue == "XZ"
                    r = pt.GetCoordinatePlaneReferenceXZ();
                    break;
                case 2: //combo.SelectedValue == "YZ"
                    r = pt.GetCoordinatePlaneReferenceYZ();
                    break;
                default:
                    r = pt.GetCoordinatePlaneReferenceXY();
                    break;
            }
            return Value.NewContainer(r);
        }

    }

    [NodeName("Evaluate curve or edge")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Evaluates curve or edge at parameter.")]
    public class dynXYZOnCurveOrEdge : dynXYZBase
    {
        public dynXYZOnCurveOrEdge()
        {
            InPortData.Add(new PortData("parameter", "The normalized parameter to evaluate at within 0..1 range, any for closed curve.", typeof(Value.Number)));
            InPortData.Add(new PortData("curve or edge", "The curve or edge to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZ", "XYZ at parameter.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public static bool curveIsReallyUnbound(Curve curve)
        {
            if (!curve.IsBound)
                return true;
            if (!curve.IsCyclic)
                return false;
            double period = curve.Period;
            if (curve.get_EndParameter(1) > curve.get_EndParameter(0) + period - 0.000000001)
                return true;
            return false;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double parameter = ((Value.Number)args[0]).Item;

            Curve thisCurve = null;
            Edge thisEdge = null;
            if (((Value.Container)args[1]).Item is Curve)
                thisCurve = ((Value.Container)args[1]).Item as Curve;
            else if (((Value.Container)args[1]).Item is Edge)
                thisEdge = ((Value.Container)args[1]).Item as Edge;
            else if (((Value.Container)args[1]).Item is Reference)
            {
                Reference r = (Reference)((Value.Container)args[1]).Item;
                if (r != null)
                {
                    Element refElem = dynRevitSettings.Doc.Document.GetElement(r.ElementId);
                    if (refElem != null)
                    {
                        GeometryObject geob = refElem.GetGeometryObjectFromReference(r);
                        thisEdge = geob as Edge;
                        if (thisEdge == null)
                            thisCurve = geob as Curve;
                    }
                    else
                        throw new Exception("Could not accept second in-port for Evaluate curve or edge node");
                }
            }
            else if (((Value.Container)args[1]).Item is CurveElement)
            {
                CurveElement cElem = ((Value.Container)args[1]).Item as CurveElement;
                if (cElem != null)
                {
                    thisCurve = cElem.GeometryCurve;
                }
                else
                    throw new Exception("Could not accept second in-port for Evaluate curve or edge node");

            }
            else
                throw new Exception("Could not accept second in-port for Evaluate curve or edge node");

            XYZ result = (thisCurve != null) ? (!curveIsReallyUnbound(thisCurve) ? thisCurve.Evaluate(parameter, true) : thisCurve.Evaluate(parameter, false))
                :  
                (thisEdge == null ? null : thisEdge.Evaluate(parameter));

            pts.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Evaluate tangent transform of curve or edge")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Evaluates tangent vector of curve or edge at parameter.")]
    public class dynTangentTransformOnCurveOrEdge : dynTransformBase
    {
        public dynTangentTransformOnCurveOrEdge()
        {
            InPortData.Add(new PortData("parameter", "The normalized parameter to evaluate at within 0..1 range except for closed curve", typeof(Value.Number)));
            InPortData.Add(new PortData("curve or edge", "The curve or edge to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("tangent transform", "tangent transform at parameter.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double parameter = ((Value.Number)args[0]).Item;

            Curve thisCurve = ((Value.Container)args[1]).Item as Curve;
            Edge thisEdge = (thisCurve != null) ? null : (((Value.Container)args[1]).Item as Edge);

            if (thisCurve == null && thisEdge == null && ((Value.Container)args[1]).Item is Reference)
            {
                Reference r = (Reference)((Value.Container)args[1]).Item;
                if (r != null)
                {
                    Element refElem = dynRevitSettings.Doc.Document.GetElement(r.ElementId);
                    if (refElem != null)
                    {
                        GeometryObject geob = refElem.GetGeometryObjectFromReference(r);
                        thisEdge = geob as Edge;
                        if (thisEdge == null)
                            thisCurve = geob as Curve;
                    }
                }
            }

            Transform result = (thisCurve != null) ?
                (!dynXYZOnCurveOrEdge.curveIsReallyUnbound(thisCurve) ? thisCurve.ComputeDerivatives(parameter, true) : thisCurve.ComputeDerivatives(parameter, false))
                : 
                (thisEdge == null ? null : thisEdge.ComputeDerivatives(parameter));

            transforms.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Ref Point By Length")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an ref point element on curve located by length from the start or end of the curve.")]
    [NodeSearchTags("ref", "pt", "curve")]
    public class dynPointOnCurveByLength : dynRevitTransactionNodeWithOneOutput
    {
        public dynPointOnCurveByLength()
        {
            InPortData.Add(new PortData("curve", "Model Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("len", "measured length or percent of overall length", typeof(Value.Number)));
            InPortData.Add(new PortData("normalized?", "if true len is the percent of overall curve length, else the actual length", typeof(Value.Container)));
            InPortData.Add(new PortData("beginning?", "if true measured from Beginning, else from End", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "PointOnCurve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var inputItem = ((Value.Container)args[0]).Item;
            Reference r = (inputItem is Reference) ?
                                (Reference)inputItem : ((CurveElement)inputItem).GeometryCurve.Reference;

            double len = ((Value.Number)args[1]).Item;

            bool isNormalized = ((Value.Number)args[2]).Item == 1;
            bool isBeginning = ((Value.Number)args[3]).Item == 1;

            PointLocationOnCurve plc = new PointLocationOnCurve(isNormalized ? PointOnCurveMeasurementType.NormalizedSegmentLength : PointOnCurveMeasurementType.SegmentLength,
                                                            len,
                                                    isBeginning ? PointOnCurveMeasureFrom.Beginning : PointOnCurveMeasureFrom.End);

            PointElementReference edgePoint = this.UIDocument.Application.Application.Create.NewPointOnEdge(r, plc);

            ReferencePoint p;

            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], typeof(ReferencePoint), out e))
                {
                    p = e as ReferencePoint;
                    p.SetPointElementReference(edgePoint);
                }
                else
                {
                    p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                    this.Elements[0] = p.Id;
                }
            }
            else
            {
                p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                this.Elements.Add(p.Id);
            }

            return Value.NewContainer(p);
        }
    }
}
