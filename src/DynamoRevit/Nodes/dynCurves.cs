using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;


namespace Dynamo.Nodes
{

    [NodeName("Line by Endpoints")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric line.")]
    [NodeSearchTags("curve", "two point", "line")]
    public class LineBound : GeometryBase
    {
        public LineBound()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(Value.Container)));
            //InPortData.Add(new PortData("bound?", "Boolean: Is this line bounded?", typeof(bool)));

            OutPortData.Add(new PortData("line", "Line", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var ptB = ((Value.Container)args[1]).Item;

            Line line = null;

            if (ptA is XYZ)
            {

                line = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)ptA, (XYZ)ptB
                  );


            }
            else if (ptA is ReferencePoint)
            {
                line = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position
               );

            }

            return Value.NewContainer(line);
        }
    }

    [NodeName("Transform Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_APPLY)]
    [NodeDescription("Returns the curve (c) transformed by the transform (t).")]
    [NodeSearchTags("move", "transform", "curve", "line")]
    public class CurveTransformed : GeometryBase
    {
        public CurveTransformed()
        {
            InPortData.Add(new PortData("cv", "Curve(Curve)", typeof(Value.Container)));
            InPortData.Add(new PortData("t", "Transform(Transform)", typeof(Value.Container)));
            OutPortData.Add(new PortData("tcv", "Transformed Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var curve = (Curve)((Value.Container)args[0]).Item;
            var trans = (Transform)((Value.Container)args[1]).Item;

            var crvTrans = curve.get_Transformed(trans);

            return Value.NewContainer(crvTrans);
        }
    }

    [NodeName("Circle")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric circle.")]
    public class Circle : GeometryBase
    {
        public Circle()
        {
            InPortData.Add(new PortData("center", "Start XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("radius", "Radius", typeof(Value.Number)));
            OutPortData.Add(new PortData("circle", "Circle CurveLoop", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public const double RevitPI = 3.14159265358979;

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radius = (double)((Value.Number)args[1]).Item;

            Curve circle = null;

            if (ptA is XYZ)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                circle = dynRevitSettings.Doc.Application.Application.Create.NewArc((XYZ)ptA, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);

            }
            else if (ptA is ReferencePoint)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                circle = dynRevitSettings.Doc.Application.Application.Create.NewArc((XYZ)((ReferencePoint)ptA).Position, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);
            }

            return Value.NewContainer(circle);
        }
    }

    [NodeName("Arc by Start, Middle, End")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric arc given start, middle and end points in XYZ.")]
    [NodeSearchTags("arc", "circle", "start", "middle", "end", "3 point", "three")]
    public class ArcStartMiddleEnd : GeometryBase
    {
        public ArcStartMiddleEnd()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("mid", "XYZ on Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("arc", "Arc", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            Arc a = null;

            var ptA = ((Value.Container)args[0]).Item;//start
            var ptB = ((Value.Container)args[1]).Item;//middle
            var ptC = ((Value.Container)args[2]).Item;//end

            if (ptA is XYZ)
            {

                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)ptA, (XYZ)ptC, (XYZ)ptB //start, end, middle 
                );


            }
            else if (ptA is ReferencePoint)
            {
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position, (XYZ)((ReferencePoint)ptC).Position //start, end, middle 
                );

            }

            return Value.NewContainer(a);
        }
    }

    [NodeName("Arc by Center, Normal, Radius")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric arc given a center point and two end parameters. Start and End Values may be between 0 and 2*PI in Radians")]
    [NodeSearchTags("arc", "circle", "center", "radius")]
    public class ArcCenter : GeometryBase
    {
        public ArcCenter()
        {
            InPortData.Add(new PortData("center", "center XYZ or Coordinate System", typeof(Value.Container)));
            InPortData.Add(new PortData("radius", "Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("start", "Start Param", typeof(Value.Number)));
            InPortData.Add(new PortData("end", "End Param", typeof(Value.Number)));
            OutPortData.Add(new PortData("arc", "Arc", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radius = (double)((Value.Number)args[1]).Item;
            var start = (double)((Value.Number)args[2]).Item;
            var end = (double)((Value.Number)args[3]).Item;

            Arc a = null;

            if (ptA is XYZ)
            {
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)ptA, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is ReferencePoint)
            {
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                             center, radius, start, end, trf.BasisX, trf.BasisY
                );
            }

            return Value.NewContainer(a);
        }
    }

    [NodeName("Ellipse")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric ellipse.")]
    public class Ellipse : GeometryBase
    {
        public Ellipse()
        {
            InPortData.Add(new PortData("center", "Center XYZ or Coordinate System", typeof(Value.Container)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(Value.Number)));
            OutPortData.Add(new PortData("ell", "Ellipse", typeof(Value.Container)));

            RegisterAllPorts();
        }

        const double RevitPI = 3.14159265358979;

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radX = (double)((Value.Number)args[1]).Item;
            var radY = (double)((Value.Number)args[2]).Item;

            Autodesk.Revit.DB.Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                     center, radX, radY, trf.BasisX, trf.BasisY, 0, 2 * RevitPI
                  );
            }

            return Value.NewContainer(ell);
        }
    }

    [NodeName("Ellipse Arc")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric elliptical arc. Start and End Values may be between 0 and 2*PI in Radians")]
    public class EllipticalArc : GeometryBase
    {
        public EllipticalArc()
        {
            InPortData.Add(new PortData("center", "Center XYZ or Coordinate System", typeof(Value.Container)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("start", "Start Param", typeof(Value.Number)));
            InPortData.Add(new PortData("end", "End Param", typeof(Value.Number)));
            OutPortData.Add(new PortData("ell", "Ellipse", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radX = (double)((Value.Number)args[1]).Item;
            var radY = (double)((Value.Number)args[2]).Item;
            var start = (double)((Value.Number)args[3]).Item;
            var end = (double)((Value.Number)args[4]).Item;

            Autodesk.Revit.DB.Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                     center, radX, radY, trf.BasisX, trf.BasisY, start, end
                  );
            }

            return Value.NewContainer(ell);
        }
    }

    [NodeName("Line by Origin and Direction")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a line in the direction of an XYZ normal.")]
    public class LineVectorfromXyz : NodeWithOneOutput
    {
        public LineVectorfromXyz()
        {
            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("curve", "Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = (XYZ)((Value.Container)args[0]).Item;
            var ptB = (XYZ)((Value.Container)args[1]).Item;

            // CurveElement c = MakeLine(this.UIDocument.Document, ptA, ptB);
            CurveElement c = MakeLineCBP(dynRevitSettings.Doc.Document, ptA, ptB);

            return FScheme.Value.NewContainer(c);
        }


        public Autodesk.Revit.DB.ModelCurve MakeLine(Document doc, XYZ ptA, XYZ ptB)
        {
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            // Create plane by the points
            Line line = app.Create.NewLine(ptA, ptB, true);
            XYZ norm = ptA.CrossProduct(ptB);
            double length = norm.GetLength();
            if (length == 0) norm = XYZ.BasisZ;
            Autodesk.Revit.DB.Plane plane = app.Create.NewPlane(norm, ptB);
            Autodesk.Revit.DB.SketchPlane skplane = doc.FamilyCreate.NewSketchPlane(plane);
            // Create line here
            Autodesk.Revit.DB.ModelCurve modelcurve = doc.FamilyCreate.NewModelCurve(line, skplane);
            return modelcurve;
        }

        public Autodesk.Revit.DB.CurveByPoints MakeLineCBP(Document doc, XYZ ptA, XYZ ptB)
        {
            ReferencePoint sunRP = doc.FamilyCreate.NewReferencePoint(ptA);
            ReferencePoint originRP = doc.FamilyCreate.NewReferencePoint(ptB);
            ReferencePointArray sunRPArray = new ReferencePointArray();
            sunRPArray.Append(sunRP);
            sunRPArray.Append(originRP);
            Autodesk.Revit.DB.CurveByPoints sunPath = doc.FamilyCreate.NewCurveByPoints(sunRPArray);
            return sunPath;
        }
    }

    [NodeName("Hermite Spline")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric hermite spline.")]
    [NodeSearchTags("curve through points", "interpolate", "spline")]
    public class HermiteSpline : GeometryBase
    {
        Autodesk.Revit.DB.HermiteSpline hs;

        public HermiteSpline()
        {
            InPortData.Add(new PortData("xyzs", "List of pts.(List XYZ)", typeof(Value.List)));
            OutPortData.Add(new PortData("spline", "Spline", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = ((Value.List)args[0]).Item;

            hs = null;

            var containers = Utils.SequenceToFSharpList(pts);

            var ctrlPts = new List<XYZ>();
            foreach (Value e in containers)
            {
                if (e.IsContainer)
                {
                    XYZ pt = (XYZ)((Value.Container)(e)).Item;
                    ctrlPts.Add(pt);
                }
            }
            if (pts.Count() > 0)
            {
                hs = dynRevitSettings.Doc.Application.Application.Create.NewHermiteSpline(ctrlPts, false);
            }

            return Value.NewContainer(hs);
        }
    }

    [NodeName("Polyline")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Create a series of lines through a list of points.")]
    [NodeSearchTags("line", "through", "passing", "thread")]
    public class CurvesThroughPoints : GeometryBase
    {
        public CurvesThroughPoints()
        {
            InPortData.Add(new PortData("xyzs", "List of points (xyz) through which to create lines.", typeof(Value.List)));
            OutPortData.Add(new PortData("lines", "Lines created through points.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Build a sequence that unwraps the input list from it's Value form.
            var pts = ((Value.List)args[0]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );

            var results = FSharpList<Value>.Empty;

            var enumerable = pts as XYZ[] ?? pts.ToArray();
            for (var i = 1; i < enumerable.Count(); i++)
            {
                var l = dynRevitSettings.Revit.Application.Create.NewLineBound(enumerable.ElementAt(i), enumerable.ElementAt(i - 1));

                results = FSharpList<Value>.Cons(Value.NewContainer(l), results);
            }

            return Value.NewList(results);
        }
    }

    [NodeName("Rectangle Curve Loop")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Create a rectangle by specifying the center, width, height, and normal.  Outputs a CurveLoop object directed counter-clockwise from upper right.")]
    public class Rectangle : GeometryBase
    {
        public Rectangle()
        {
            InPortData.Add(new PortData("transform", "The a transform for the rectangle.", typeof(Value.Container)));
            InPortData.Add(new PortData("width", "The width of the rectangle.", typeof(Value.Number)));
            InPortData.Add(new PortData("height", "The height of the rectangle.", typeof(Value.Number)));
            OutPortData.Add(new PortData("geometry", "The curve loop representing the rectangle.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var t = (Transform)((Value.Container)args[0]).Item;
            double width = ((Value.Number)args[1]).Item;
            double height = ((Value.Number)args[2]).Item;

            //ccw from upper right
            var p0 = new XYZ(width / 2, height / 2, 0);
            var p3 = new XYZ(-width / 2, height / 2, 0);
            var p2 = new XYZ(-width / 2, -height / 2, 0);
            var p1 = new XYZ(width / 2, -height / 2, 0);

            p0 = t.OfPoint(p0);
            p1 = t.OfPoint(p1);
            p2 = t.OfPoint(p2);
            p3 = t.OfPoint(p3);

            var l1 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p0, p1);
            var l2 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p1, p2);
            var l3 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p2, p3);
            var l4 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p3, p0);

            var cl = new Autodesk.Revit.DB.CurveLoop();
            cl.Append(l1);
            cl.Append(l2);
            cl.Append(l3);
            cl.Append(l4);

            return Value.NewContainer(cl);
        }
    }

    [NodeName("Element Geometry Objects")]
    [NodeCategory(BuiltinNodeCategories.REVIT_BAKE)]
    [NodeDescription("Creates list of geometry object references in the element.")]
    public class ElementGeometryObjects : NodeWithOneOutput
    {
        List<GeometryObject> instanceGeometryObjects;

        public ElementGeometryObjects()
        {
            InPortData.Add(new PortData("element", "element to create geometrical references to", typeof(Value.Container)));
            OutPortData.Add(new PortData("list", "Geometry objects of the element", typeof(Value.List)));

            RegisterAllPorts();

            instanceGeometryObjects = null;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Element thisElement = (Element)((Value.Container)args[0]).Item;

            instanceGeometryObjects = new List<GeometryObject>();

            var result = FSharpList<Value>.Empty;

            Autodesk.Revit.DB.Options geoOptionsOne = new Autodesk.Revit.DB.Options();
            geoOptionsOne.ComputeReferences = true;

            GeometryObject geomObj = thisElement.get_Geometry(geoOptionsOne);
            GeometryElement geomElement = geomObj as GeometryElement;

            if ((thisElement is GenericForm) && (geomElement.Count() < 1))
            {
                GenericForm gF = (GenericForm)thisElement;
                if (!gF.Combinations.IsEmpty)
                {
                    Autodesk.Revit.DB.Options geoOptionsTwo = new Autodesk.Revit.DB.Options();
                    geoOptionsTwo.IncludeNonVisibleObjects = true;
                    geoOptionsTwo.ComputeReferences = true;
                    geomObj = thisElement.get_Geometry(geoOptionsTwo);
                    geomElement = geomObj as GeometryElement;
                }
            }

            foreach (GeometryObject geob in geomElement)
            {
                GeometryInstance ginsta = geob as GeometryInstance;
                if (ginsta != null)
                {
                    GeometryElement instanceGeom = ginsta.GetInstanceGeometry();
                    instanceGeometryObjects.Add(instanceGeom);
                    foreach (GeometryObject geobInst in instanceGeom)
                    {
                        result = FSharpList<Value>.Cons(Value.NewContainer(geobInst), result);
                    }
                }
                else
                {
                    result = FSharpList<Value>.Cons(Value.NewContainer(geob), result);
                }
            }

            return Value.NewList(result);
        }
    }

    [NodeName("Model Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a model curve.")]
    public class ModelCurve : RevitTransactionNodeWithOneOutput
    {
        public ModelCurve()
        {
            InPortData.Add(new PortData("c", "A Geometric Curve.", typeof(Value.Container)));
            OutPortData.Add(new PortData("mc", "Model Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        static bool hasMethodSetCurve = true;

        static public void setCurveMethod(Autodesk.Revit.DB.ModelCurve mc, Curve c)
        {
            bool foundMethod = false;

            if (hasMethodSetCurve)
            {
                Type CurveElementType = typeof(Autodesk.Revit.DB.CurveElement);
                MethodInfo[] curveElementMethods = CurveElementType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                System.String nameOfMethodSetCurve = "SetGeometryCurveOverridingJoins";

                foreach (MethodInfo m in curveElementMethods)
                {
                    if (m.Name == nameOfMethodSetCurve)
                    {
                        object[] argsM = new object[1];
                        argsM[0] = c;

                        foundMethod = true;
                        m.Invoke(mc, argsM);
                        break;
                    }
                }
            }
            if (!foundMethod)
            {
                hasMethodSetCurve = false;
                mc.GeometryCurve = c;
            }
        }

        static bool hasMethodResetSketchPlane = true;
        //returns unused sketch plane id
        static public ElementId resetSketchPlaneMethod(Autodesk.Revit.DB.ModelCurve mc, Curve c, Autodesk.Revit.DB.Plane flattenedOnPlane, out bool needsSketchPlaneReset)
        {
            //do we need to reset?
            needsSketchPlaneReset = false;
            Autodesk.Revit.DB.Plane newPlane = flattenedOnPlane != null ? flattenedOnPlane : dynRevitUtils.GetPlaneFromCurve(c, false);

            Autodesk.Revit.DB.Plane curPlane = mc.SketchPlane.Plane;

            bool resetPlane = false;
 
            {
                double llSqCur = curPlane.Normal.DotProduct(curPlane.Normal);
                double llSqNew = newPlane.Normal.DotProduct(newPlane.Normal);
                double dotP = newPlane.Normal.DotProduct(curPlane.Normal);
                double dotSqNormalized = (dotP / llSqCur) * (dotP / llSqNew);
                double angleTol = Math.PI / 1800.0;
                if (dotSqNormalized < 1.0 - angleTol * angleTol)
                   resetPlane = true;
            }
            Autodesk.Revit.DB.SketchPlane sp = null;

            if (!resetPlane)
            {
                double originDiff = curPlane.Normal.DotProduct(curPlane.Origin - newPlane.Origin);
                double tolerance = 0.000001;
                if (originDiff > tolerance || originDiff < -tolerance)
                {
                    sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                    mc.SketchPlane = dynRevitUtils.GetSketchPlaneFromCurve(c);
                }
                return (sp == null || mc.SketchPlane.Id == sp.Id) ? ElementId.InvalidElementId : sp.Id;
            }
           
            //do reset if method is available

            bool foundMethod = false;

            if (hasMethodResetSketchPlane)
            {
                Type CurveElementType = typeof(Autodesk.Revit.DB.CurveElement);
                MethodInfo[] curveElementMethods = CurveElementType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                System.String nameOfMethodSetCurve = "ResetSketchPlaneAndCurve";
                System.String nameOfMethodSetCurveAlt = "SetSketchPlaneAndCurve";

                foreach (MethodInfo m in curveElementMethods)
                {
                    if (m.Name == nameOfMethodSetCurve || m.Name == nameOfMethodSetCurveAlt)
                    {
                        object[] argsM = new object[2];
                        sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                        argsM[0] = sp;
                        argsM[1] = null; 

                        foundMethod = true;
                        m.Invoke(mc, argsM);
                        break;
                    }
                }
            }
            if (!foundMethod)
            {
                //sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                hasMethodResetSketchPlane = false;
                needsSketchPlaneReset = true;
                //expect exception, so try to keep old plane?
                //mc.SketchPlane = sp;
                return ElementId.InvalidElementId;
            }

            if (sp != null && mc.SketchPlane.Id != sp.Id)
                return sp.Id;
            
            return ElementId.InvalidElementId;
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (Curve)((Value.Container)args[0]).Item;

            Autodesk.Revit.DB.ModelCurve mc = null;
            Autodesk.Revit.DB.Plane plane = dynRevitUtils.GetPlaneFromCurve(c, false);

            Curve flattenCurve = null;

            //instead of changing Revit curve keep it "as is"
            //user might have trouble modifying curve in Revit if it is off the sketch plane

            if (this.Elements.Any())
            {
                bool needsRemake = false;
                if (dynUtils.TryGetElement(this.Elements[0], out mc))
                {
                    ElementId idSpUnused = ModelCurve.resetSketchPlaneMethod(mc, c, plane, out needsRemake);

                    if (idSpUnused != ElementId.InvalidElementId)
                    {
                        this.DeleteElement(idSpUnused);
                    }
                    if (!needsRemake)
                    {
                        if (!mc.GeometryCurve.IsBound && c.IsBound)
                        {
                            c = c.Clone();
                            c.MakeUnbound();
                        }
                        ModelCurve.setCurveMethod(mc, c); // mc.GeometryCurve = c;
                    }

                }
                else
                    needsRemake = true;
                if (needsRemake)
                {
                    var sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                    if (dynRevitUtils.GetPlaneFromCurve(c, true) == null)
                    {
                        flattenCurve = dynRevitUtils.Flatten3dCurveOnPlane(c, plane);
                        mc = this.UIDocument.Document.IsFamilyDocument
                             ? this.UIDocument.Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                                : this.UIDocument.Document.Create.NewModelCurve(flattenCurve, sp);

                        ModelCurve.setCurveMethod(mc, c);
                    }
                    else
                    {
                        mc = this.UIDocument.Document.IsFamilyDocument
                           ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                           : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                    }
                    this.Elements[0] = mc.Id;
                    if (mc.SketchPlane.Id != sp.Id)
                    {
                        //THIS BIZARRE as Revit could use different existing SP, so if Revit had found better plane  this sketch plane has no use
                        this.DeleteElement(sp.Id);
                    }
                    this.Elements[0] = mc.Id;
                }
            }
            else
            {
                var sp = dynRevitUtils.GetSketchPlaneFromCurve(c);

                if (dynRevitUtils.GetPlaneFromCurve(c, true) == null)
                {
                    flattenCurve = dynRevitUtils.Flatten3dCurveOnPlane(c, plane);
                    mc = this.UIDocument.Document.IsFamilyDocument
                         ? this.UIDocument.Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                            : this.UIDocument.Document.Create.NewModelCurve(flattenCurve, sp);

                    ModelCurve.setCurveMethod(mc, c);
                }
                else
                {
                    mc = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                       : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                }
                this.Elements.Add(mc.Id);
                if (mc.SketchPlane.Id != sp.Id)
                {
                    //found better plane
                    this.DeleteElement(sp.Id);
                }
            }

            return Value.NewContainer(mc);
        }
    }

    [NodeName("Reference Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates a reference curve.")]
    public class ReferenceCurve : RevitTransactionNodeWithOneOutput
    {
        public ReferenceCurve()
        {
            InPortData.Add(new PortData("c", "A Geometric Curve.", typeof(Value.Container)));
            OutPortData.Add(new PortData("rc", "Reference Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (Curve)((Value.Container)args[0]).Item;
            Autodesk.Revit.DB.ModelCurve mc = null;

            Curve flattenCurve = null;
            Autodesk.Revit.DB.Plane plane = dynRevitUtils.GetPlaneFromCurve(c, false);

            //instead of changing Revit curve keep it "as is"
            //user might have trouble modifying curve in Revit if it is off the sketch plane

            if (this.Elements.Any())
            {
                bool needsRemake = false;
                if (dynUtils.TryGetElement(this.Elements[0], out mc))
                {
                    ElementId idSpUnused = ModelCurve.resetSketchPlaneMethod(mc, c, plane, out needsRemake);

                    if (idSpUnused != ElementId.InvalidElementId)
                    {
                        this.DeleteElement(idSpUnused);
                    }
                    //mc.SketchPlane = sp;
                    if (!needsRemake)
                    {
                        if (!mc.GeometryCurve.IsBound && c.IsBound)
                        {
                            c = c.Clone();
                            c.MakeUnbound();
                        }
                        ModelCurve.setCurveMethod(mc, c);  //mc.GeometryCurve = c;
                    }
                }
                else
                    needsRemake = true;
                if (needsRemake)
                {
                    var sp = dynRevitUtils.GetSketchPlaneFromCurve(c);

                    if (dynRevitUtils.GetPlaneFromCurve(c, true) == null)
                    {
                        flattenCurve = dynRevitUtils.Flatten3dCurveOnPlane(c, plane);
                        mc = this.UIDocument.Document.IsFamilyDocument
                             ? this.UIDocument.Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                                : this.UIDocument.Document.Create.NewModelCurve(flattenCurve, sp);

                        ModelCurve.setCurveMethod(mc, c); 
                    }
                    else
                        mc = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                       : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                    mc.ChangeToReferenceLine();
                    this.Elements[0] = mc.Id;
                    //mc.SketchPlane = sp;
                    if (mc.SketchPlane.Id != sp.Id)
                    {
                        //THIS BIZARRE as Revit could use different existing SP, so if Revit had found better plane  this sketch plane has no use
                        this.DeleteElement(sp.Id);
                    }
                    this.Elements[0] = mc.Id;
                }
            }
            else
            {
                var sp = dynRevitUtils.GetSketchPlaneFromCurve(c);

                if (dynRevitUtils.GetPlaneFromCurve(c, true) == null)
                {
                    flattenCurve = dynRevitUtils.Flatten3dCurveOnPlane(c, plane);
                    mc = this.UIDocument.Document.IsFamilyDocument
                         ? this.UIDocument.Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                            : this.UIDocument.Document.Create.NewModelCurve(flattenCurve, sp);

                    ModelCurve.setCurveMethod(mc, c);
                }
                else
                {
                    mc = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                       : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                }
                this.Elements.Add(mc.Id);
                mc.ChangeToReferenceLine();
                //mc.SketchPlane = sp;
                if (mc.SketchPlane.Id != sp.Id)
                {
                    //found better plane
                    this.DeleteElement(sp.Id);
                }
            }

            return Value.NewContainer(mc);
        }
    }

    [NodeName("Curve By Points")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Create a new Curve by Points by passing in a list of Reference Points")]
    public class CurveByPoints : RevitTransactionNodeWithOneOutput
    {
        //Our eventual output.
        Autodesk.Revit.DB.CurveByPoints c;

        public CurveByPoints()
        {
            InPortData.Add(new PortData("refPts", "List of reference points", typeof(Value.List)));
            InPortData.Add(new PortData("isRef", "Boolean indicating whether the resulting curve is a reference curve.", typeof(Value.Number)));
            OutPortData.Add(new PortData("curve", "Curve from ref points", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            bool isRefCurve = Convert.ToBoolean(((Value.Number)args[1]).Item);

            //Build a sequence that unwraps the input list from it's Value form.
            IEnumerable<ReferencePoint> refPts = ((Value.List)args[0]).Item.Select(
               x => (ReferencePoint)((Value.Container)x).Item
            );

            //Add all of the elements in the sequence to a ReferencePointArray.
            ReferencePointArray refPtArr = new ReferencePointArray();
            foreach (var refPt in refPts)
            {
                refPtArr.Append(refPt);
            }

            //Standard logic for updating an old result, if it exists.
            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(this.Elements[0], out c))
                {
                    c.SetPoints(refPtArr);
                }
                else
                {
                    //TODO: This method of handling bad elements may cause problems. Instead of overwriting
                    //      index in Elements, might be better to just add it the Elements and then do
                    //      this.DeleteElement(id, true) on the old index.
                    c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                    this.Elements[0] = c.Id;
                }
            }
            else
            {
                c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                this.Elements.Add(c.Id);
            }

            c.IsReferenceLine = isRefCurve;

            return Value.NewContainer(c);
        }
    }

    [NodeName("Curve by Points by Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Create a new Curve by Points by passing in a geometry line in 3d space")]
    [AlsoKnownAsAttribute("Curve By Points By Line")]
    public class CurveByPointsByLine : RevitTransactionNodeWithOneOutput
    {
        public CurveByPointsByLine()
        {
            InPortData.Add(new PortData("curve", "geometry curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("curve", "Curve from ref points", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Our eventual output.
            Autodesk.Revit.DB.CurveByPoints c = null;

            var input = args[0];

            Curve gc = (Curve)((Value.Container)args[0]).Item;
            XYZ start = gc.get_EndPoint(0);
            XYZ end = gc.get_EndPoint(1);

            //If we've made any elements previously...
            if (this.Elements.Any())
            {
                bool replaceElement = true;
                //...try to get the first one...
                if (dynUtils.TryGetElement(this.Elements[0], out c))
                {
                    //..and if we do, update it's position.
                    ReferencePointArray existingPts = c.GetPoints();

                    //update the points on the curve to match
                    if (gc.GetType() == typeof(Line) && 
                        existingPts.Size == 2)
                    {
                        existingPts.get_Item(0).Position = start;
                        existingPts.get_Item(1).Position = end;
                        replaceElement = false;
                    }
                    // NOTE: there is no way I found in REVITAPI to tell if existing curve by points is arc
                    else if (gc.GetType() == typeof(Arc) &&  existingPts.Size == 3)
                    {
                        if  (existingPts.Size != 3)
                        {
                            var newPts = new ReferencePointArray();
                            newPts.Append(existingPts.get_Item(0));
                            if (existingPts.Size < 3)
                               newPts.Append(this.UIDocument.Document.FamilyCreate.NewReferencePoint(gc.Evaluate(0.5, true)));
                            else
                               newPts.Append(existingPts.get_Item(1));
                            newPts.Append(existingPts.get_Item(existingPts.Size - 1));
                            c.SetPoints(newPts);
                            existingPts = c.GetPoints();
                        }
                       
                        existingPts.get_Item(0).Position = start;
                        existingPts.get_Item(2).Position = end;
                        existingPts.get_Item(1).Position = gc.Evaluate(0.5, true);
                        replaceElement = false;

                    }
                    else if (gc.GetType() != typeof(Arc))
                    {
                        int nPoints = existingPts.Size;
                        IList<XYZ> xyzList = gc.Tessellate();
                        int numPoints = xyzList.Count;

                        if (nPoints != numPoints)
                        {
                            var newPts = new ReferencePointArray();
                            newPts.Append(existingPts.get_Item(0));
                            newPts.get_Item(0).Position = xyzList[0];
                            for (int iPoint = 1; iPoint < numPoints; iPoint++)
                            {
                                if (iPoint == numPoints - 1)
                                {
                                    newPts.Append(existingPts.get_Item(nPoints - 1));
                                    newPts.get_Item(iPoint).Position = xyzList[iPoint];
                                }
                                else if (iPoint < nPoints - 1)
                                {
                                    newPts.Append(existingPts.get_Item(iPoint));
                                    newPts.get_Item(iPoint).Position = xyzList[iPoint];
                                }
                                else
                                {
                                    newPts.Append(this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyzList[iPoint]));
                                }
                            }
                            if (nPoints > numPoints)
                            {
                                //have to delete as API call to SetPoints leaves points in the doc
                                for (int iPoint = numPoints - 1; iPoint < nPoints - 1; iPoint++)
                                {
                                    this.DeleteElement(existingPts.get_Item(iPoint).Id);
                                }
                            }
                            c.SetPoints(newPts);
                            existingPts = c.GetPoints();
                        }
                        else
                        {
                           for (int iPoint = 0; iPoint < numPoints; iPoint++)
                           {
                               if (iPoint == 0)
                                  existingPts.get_Item(iPoint).Position = start;
                               else if (iPoint == nPoints - 1)
                                  existingPts.get_Item(iPoint).Position = end;
                               else
                                  existingPts.get_Item(iPoint).Position = xyzList[iPoint];
                           }
                        }
                        replaceElement = false;
                    }
                    if (replaceElement)
                    {
                        IList<XYZ> xyzList = gc.Tessellate();
                        int numPoint = xyzList.Count;
                        double step = 1.0/(numPoint - 1.0);
                        double tolerance = 0.0000001;
                        replaceElement = false;
                        for (int index = 0; index < numPoint; index++)
                        {
                            IntersectionResult projXYZ = c.GeometryCurve.Project(xyzList[index]);
                            if (projXYZ.XYZPoint.DistanceTo(xyzList[index]) > tolerance)
                            {
                                replaceElement = true;
                                break;
                            }
                        }
                    }
                }
                if (replaceElement)
                {
                    this.DeleteElement(this.Elements[0]);

                    ReferencePointArray existingPts = c.GetPoints();

                    c = null;

                    c = CreateCurveByPoints(c, gc, start, end);
                    this.Elements[0] = c.Id;
                }
            }
            else
            {    
                c = CreateCurveByPoints(c, gc, start, end);
                this.Elements.Add(c.Id);
            }

            return Value.NewContainer(c);
        }

        static bool foundCreateArcThroughPoints = true;

        private Autodesk.Revit.DB.CurveByPoints CreateCurveByPoints(Autodesk.Revit.DB.CurveByPoints c, Curve gc, XYZ start, XYZ end)
        {
            //Add the geometry curves start and end points to a ReferencePointArray.
            ReferencePointArray refPtArr = new ReferencePointArray();
            if (gc.GetType() == typeof(Line))
            {
                ReferencePoint refPointStart = this.UIDocument.Document.FamilyCreate.NewReferencePoint(start);
                ReferencePoint refPointEnd = this.UIDocument.Document.FamilyCreate.NewReferencePoint(end);
                refPtArr.Append(refPointStart);
                refPtArr.Append(refPointEnd);
                c = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(refPtArr);
            }
            else if (gc.GetType() == typeof(Arc) && foundCreateArcThroughPoints)
            {
                Type CurveByPointsUtilsType = typeof(Autodesk.Revit.DB.CurveByPointsUtils);
                MethodInfo[] curveElementMethods = CurveByPointsUtilsType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                System.String nameOfMethodSetCurve = "CreateArcThroughPoints";


                foreach (MethodInfo m in curveElementMethods)
                {
                    if (m.Name == nameOfMethodSetCurve)
                    {
                        object[] argsM = new object[4];

                        ReferencePoint refPointStart = this.UIDocument.Document.FamilyCreate.NewReferencePoint(start);
                        ReferencePoint refPointEnd = this.UIDocument.Document.FamilyCreate.NewReferencePoint(end);
                        XYZ midPoint = gc.Evaluate(0.5, true);
                        ReferencePoint refMidPoint = this.UIDocument.Document.FamilyCreate.NewReferencePoint(midPoint);

                        argsM[0] = this.UIDocument.Document;
                        argsM[1] = refPointStart;
                        argsM[2] = refPointEnd;
                        argsM[3] = refMidPoint;

                        c = (Autodesk.Revit.DB.CurveByPoints)m.Invoke(null, argsM);
                        if (c != null && c.GeometryCurve.GetType() == typeof(Arc))
                           return c;
                        if (c != null)
                            this.DeleteElement(c.Id);
                        break;
                    }
                }
                foundCreateArcThroughPoints = false;
            }
            if (gc.GetType() != typeof(Line))
            {
                IList <XYZ> xyzList = gc.Tessellate();
                int numPoints = xyzList.Count;
                for (int ii = 0; ii < numPoints; ii++)
                {
                    ReferencePoint refPoint = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyzList[ii]);
                    refPtArr.Append(refPoint);
                }
            }
            c = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(refPtArr);
            return c;
        }
    }

    [NodeName("Curve Reference")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Takes in a Model Curve or Geometry Curve, returns a Curve Reference")]
    public class CurveRef : RevitTransactionNodeWithOneOutput
    {
        public CurveRef()
        {
            InPortData.Add(new PortData("curve", "Model Curve Element or Geometry Curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("curveRef", "Curve Reference", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private Value makeCurveRef(object c, int count)
        {
            Reference r = c is CurveElement
               ? (c as CurveElement).GeometryCurve.Reference // curve element
               : (c as Curve).Reference; // geometry curve

            return Value.NewContainer(r);
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                int count = 0;
                var result = Value.NewList(
                   Utils.SequenceToFSharpList(
                      (input as Value.List).Item.Select(
                         x =>
                                this.makeCurveRef(
                                ((Value.Container)x).Item,
                                count++
                            )
                      )
                   )
                );
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.makeCurveRef(
                       ((Value.Container)input).Item,
                       0

                    );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }

    }

    [NodeName("Geometry Curve From Model Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Takes in a model curve and extracts a geometry curve")]
    [NodeSearchTags("Convert", "Extract", "Geometry", "Curve", "Model", "Reference")]
    public class CurveFromModelCurve : RevitTransactionNodeWithOneOutput
    {
        public CurveFromModelCurve()
        {
            InPortData.Add(new PortData("mc", "Model Curve Element", typeof(Value.Container)));
            OutPortData.Add(new PortData("curve", "Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private Value extractCurve(object c, int count)
        {
            Curve curve = ((CurveElement)c).GeometryCurve;

            return Value.NewContainer(curve);
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                int count = 0;
                var result = Value.NewList(
                   Utils.SequenceToFSharpList(
                      (input as Value.List).Item.Select(
                         x =>
                                this.extractCurve(
                                ((Value.Container)x).Item,
                                count++
                            )
                      )
                   )
                );
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.extractCurve(
                       ((Value.Container)input).Item,
                       0

                    );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }

    }

    [NodeName("Nurbs Spline Model Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Node to create a planar nurbs spline model curve.")]
    public class ModelCurveNurbSpline : RevitTransactionNodeWithOneOutput
    {
        public ModelCurveNurbSpline()
        {
            InPortData.Add(new PortData("pts", "The points from which to create the nurbs curve", typeof(Value.List)));
            OutPortData.Add(new PortData("cv", "The nurbs spline model curve created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = ((Value.List)args[0]).Item.Select(
               x => ((ReferencePoint)((Value.Container)x).Item).Position
            ).ToList();

            if (pts.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }

            var ns = UIDocument.Application.Application.Create.NewNurbSpline(
                    pts, Enumerable.Repeat(1.0, pts.Count).ToList());

            ModelNurbSpline c;

            if (Elements.Any() && dynUtils.TryGetElement(Elements[0], out c))
            {
                ModelCurve.setCurveMethod(c, ns); //c.GeometryCurve = ns;
            }
            else
            {
                Elements.Clear();

                double rawParam = ns.ComputeRawParameter(.5);
                Transform t = ns.ComputeDerivatives(rawParam, false);

                XYZ norm = t.BasisZ;

                if (norm.GetLength() == 0)
                {
                    norm = XYZ.BasisZ;
                }

                Autodesk.Revit.DB.Plane p = new Autodesk.Revit.DB.Plane(norm, t.Origin);
                Autodesk.Revit.DB.SketchPlane sp = this.UIDocument.Document.FamilyCreate.NewSketchPlane(p);
                //sps.Add(sp);

                c = UIDocument.Document.FamilyCreate.NewModelCurve(ns, sp) as ModelNurbSpline;

                Elements.Add(c.Id);
            }

            return Value.NewContainer(c);
        }
    }

    [NodeName("Nurbs Spline")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Node to create a planar nurbs spline curve.")]
    public class GeometryCurveNurbSpline : GeometryBase
    {
        public GeometryCurveNurbSpline()
        {
            InPortData.Add(new PortData("xyzs", "The xyzs from which to create the nurbs curve", typeof(Value.List)));
            OutPortData.Add(new PortData("cv", "The nurbs spline curve created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

         public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = ((Value.List)args[0]).Item.Select(
               x => ((XYZ)((Value.Container)x).Item)).ToList();

            if (pts.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }

            var ns = dynRevitSettings.Revit.Application.Create.NewNurbSpline(
                    pts, Enumerable.Repeat(1.0, pts.Count).ToList());

            return Value.NewContainer(ns);
        }
    }
     
    [NodeName("Curve Loop")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates Curve Loop")]
    public class CurveLoop : GeometryBase
    {
        public CurveLoop()
        {
            InPortData.Add(new PortData("curves", "Geometry curves to make curve loop", typeof(Value.List)));
            OutPortData.Add(new PortData("CurveLoop", "CurveLoop", typeof(Value.Container)));
            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            var curves = ((Value.List)args[0]).Item.Select(
               x => ((Curve)((Value.Container)x).Item)).ToList();

            List<Curve> curvesWithFlip = new List<Curve>();

            bool bStart = true;
            XYZ prevEnd = new XYZ();

            double tolMax = 0.0001;
            double tolMin = 0.00001;
            
            foreach (Curve c in curves)
            {
                if (!bStart)
                {
                    XYZ thisEnd = c.Evaluate(1.0, true);
                    XYZ thisStart = c.Evaluate(0.0, true);
                    double thisDist = thisStart.DistanceTo(prevEnd);
                    if (thisDist > tolMax &&  thisEnd.DistanceTo(prevEnd) < tolMin && (c is Line))
                    {
                        prevEnd = thisStart;
                        Curve flippedCurve = /* Line.CreateBound */ dynRevitSettings.Revit.Application.Create.NewLineBound(thisEnd, thisStart);
                        curvesWithFlip.Add(flippedCurve);
                        continue;
                    }
                }
                else
                {
                    bStart = false;
                    prevEnd = c.Evaluate(1.0, true);
                    if (curves.Count > 1)
                    {
                        XYZ nextStart = curves[1].Evaluate(0.0, true);
                        double thisDist = prevEnd.DistanceTo(nextStart);
                        if (thisDist > tolMax)
                        {
                            XYZ nextEnd = curves[1].Evaluate(1.0, true);
                            if (nextEnd.DistanceTo(prevEnd) > tolMax)
                            {
                                XYZ thisStart = c.Evaluate(0.0, true);
                                if (thisStart.DistanceTo(nextEnd) < tolMin || thisStart.DistanceTo(nextStart) < tolMin)
                                {
                                    if (c is Line)
                                    {
                                        Curve flippedCurve = /* Line.CreateBound */ dynRevitSettings.Revit.Application.Create.NewLineBound(prevEnd, thisStart);
                                        prevEnd = thisStart;
                                        curvesWithFlip.Add(flippedCurve);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                prevEnd = c.Evaluate(1.0, true);
                curvesWithFlip.Add(c);
            }

            Autodesk.Revit.DB.CurveLoop result = Autodesk.Revit.DB.CurveLoop.Create(curvesWithFlip);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Thicken Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates Curve Loop by thickening a curve")]
    public class ThickenCurveLoop : GeometryBase
    {
        public ThickenCurveLoop()
        {
            InPortData.Add(new PortData("Curve", "Curve to thicken, cannot be closed.", typeof(Value.Container)));
            InPortData.Add(new PortData("Thickness", "Thickness value.", typeof(Value.Number)));
            InPortData.Add(new PortData("Normal", "The normal vector to the plane used for thickening.", typeof(Value.Container)));
            OutPortData.Add(new PortData("CurveLoop", "CurveLoop which is the result of thickening.", typeof(Value.Container)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;
            double thickness = ((Value.Number)args[1]).Item;
            XYZ normal = (XYZ)((Value.Container)args[2]).Item;

            Autodesk.Revit.DB.CurveLoop result = Autodesk.Revit.DB.CurveLoop.CreateViaThicken(curve.Clone(), thickness, normal);
            if (result == null)
                throw new Exception("Could not thicken curve");

            return Value.NewContainer(result);
        }
    }

    [NodeName("Curve Loop Components")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Extract a list of curves in the Curve Loop")]
    public class ListCurveLoop : RevitTransactionNodeWithOneOutput
    {
        public ListCurveLoop()
        {
            InPortData.Add(new PortData("CurveLoop", "Curve to thicken.", typeof(Value.Container)));
            OutPortData.Add(new PortData("Curve List", "List of curves in the curve loop.", typeof(Value.List)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            Autodesk.Revit.DB.CurveLoop curveLoop = (Autodesk.Revit.DB.CurveLoop)((Value.Container)args[0]).Item;

            CurveLoopIterator CLiter = curveLoop.GetCurveLoopIterator();

            List<Curve> listCurves = new List<Curve>();
            for (; CLiter.MoveNext(); )
            {
                listCurves.Add(CLiter.Current.Clone());
            }

            var result = FSharpList<Value>.Empty;
            for (int indexCurve = listCurves.Count - 1; indexCurve > -1; indexCurve--)
            {
                result = FSharpList<Value>.Cons(Value.NewContainer(listCurves[indexCurve]), result);
            }

            return Value.NewList(result);
        }
    }
     
    [NodeName("Offset Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates curve by offseting curve")]
    public class OffsetCrv : GeometryBase
    {
        public OffsetCrv()
        {
            InPortData.Add(new PortData("Curve", "Curve to offest, cannot be closed.", typeof(Value.Container)));
            InPortData.Add(new PortData("Offset", "Offset value.", typeof(Value.Number)));
            InPortData.Add(new PortData("Normal", "The normal vector to the plane used for offset.", typeof(Value.Number)));
            OutPortData.Add(new PortData("Curve", "Curve which is the result of offset.", typeof(Value.Container)));
            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;

            double thickness = ((Value.Number)args[1]).Item;
            XYZ normal = (XYZ)((Value.Container)args[2]).Item;

            Autodesk.Revit.DB.CurveLoop thickenLoop = Autodesk.Revit.DB.CurveLoop.CreateViaThicken(curve.Clone(), thickness, normal);

            if (thickenLoop == null)
                throw new Exception("Could not offset curve");

            CurveLoopIterator CLiter = thickenLoop.GetCurveLoopIterator();

            Curve result = null;

            //relying heavily on the order of curves in the resulting curve loop, based on internal implemen
            for (int index = 0; CLiter.MoveNext(); index++)
            {
                if (index == 2)
                    result = CLiter.Current.Clone();
            }

            if (result == null)
                throw new Exception("Could not offset curve");

            return Value.NewContainer(result);
        }
    }

    [NodeName("Bound Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates Curve by bounding original by two points")]
    public class BoundCurve : RevitTransactionNodeWithOneOutput
    {
        public BoundCurve()
        {
            InPortData.Add(new PortData("Curve", "Curve to bound.", typeof(object)));
            InPortData.Add(new PortData("New Start Point", "Start point should be within bounded curve, anywhere on unbounded curve.", typeof(object)));
            InPortData.Add(new PortData("New End Point", "End point should be within bounded curve, anywhere on unbounded curve.", typeof(object)));
            OutPortData.Add(new PortData("Result", "Resulting curve.", typeof(object)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {

            Curve curve = (Curve)((Value.Container)args[0]).Item;
            XYZ newStart = (XYZ) ((Value.Container)args[1]).Item;
            XYZ newEnd = (XYZ) ((Value.Container)args[2]).Item;

            IntersectionResult projectStart = curve.Project(newStart);
            IntersectionResult projectEnd = curve.Project(newEnd);

            double sParam = projectStart.Parameter;
            double eParam = projectEnd.Parameter;


            bool closed = XyzOnCurveOrEdge.curveIsReallyUnbound(curve);
            if (closed)
            {
                double period = curve.Period;
                while (eParam < sParam)
                {
                    eParam += period;
                }
                while (eParam >= sParam + period)
                {
                    eParam -= period;
                }
                if (eParam < sParam + 0.000000001 || eParam > sParam + period - 0.000000001)
                    throw new Exception(" bounded curve results into curve of full period");
            }
            
            Curve result = curve.Clone();
                
            result.MakeBound(sParam, eParam);
            return Value.NewContainer(result);
        }
    }

    [NodeName("Bisector Line")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates bisector of two lines")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class Bisector : RevitTransactionNodeWithOneOutput
    {
        public Bisector()
        {
            InPortData.Add(new PortData("line1", "First Line", typeof(Value.Container)));
            InPortData.Add(new PortData("line2", "Second Line", typeof(Value.Container)));
            OutPortData.Add(new PortData("bisector", "Bisector Line", typeof(Value.Container)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            Line line1 = (Line)((Value.Container)args[0]).Item;
            Line line2 = (Line)((Value.Container)args[1]).Item;

            Type LineType = typeof(Autodesk.Revit.DB.Line);

            MethodInfo[] lineInstanceMethods = LineType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreateBisector = "CreateBisector";
            Line result = null;

            foreach (MethodInfo m in lineInstanceMethods)
            {
                if (m.Name == nameOfMethodCreateBisector)
                {
                    object[] argsM = new object[1];
                    argsM[0] = line2;

                    result = (Line)m.Invoke(line1, argsM);

                    break;
                }
            }

            return Value.NewContainer(result);
        }
    }

    [NodeName("Equal Distanced XYZs On Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_DIVIDE)]
    [NodeDescription("Creates a list of equal distanced XYZs along a curve.")]
    public class EqualDistXyzAlongCurve : GeometryBase
    {
            public EqualDistXyzAlongCurve()
            {
            InPortData.Add(new PortData("curve", "Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("count", "Number", typeof(Value.Number))); // just divide equally for now, dont worry about spacing and starting point
            OutPortData.Add(new PortData("XYZs", "List of equal distanced XYZs", typeof(Value.List)));

            RegisterAllPorts();
            }

        public override Value Evaluate(FSharpList<Value> args)
        {

            double xi;//, x0, xs;
            xi = ((Value.Number)args[1]).Item;// Number
            xi = Math.Round(xi);
            if (xi < System.Double.Epsilon)
                throw new Exception("The point count must be larger than 0.");

            //x0 = ((Value.Number)args[2]).Item;// Starting Coord
            //xs = ((Value.Number)args[3]).Item;// Spacing


            var result = FSharpList<Value>.Empty;

            Curve crvRef = null;

            if (((Value.Container)args[0]).Item is CurveElement)
            {
                var c = (CurveElement)((Value.Container)args[0]).Item; // Curve 
                crvRef = c.GeometryCurve;
            }
            else
            {
                crvRef = (Curve)((Value.Container)args[0]).Item; // Curve 
            }

            double t = 0.0;

            XYZ startPoint  = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                
            result = FSharpList<Value>.Cons(Value.NewContainer(startPoint), result);

            t = 1.0;
            XYZ endPoint = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);

            if (xi > 2.0 +  System.Double.Epsilon)
            {
                int numParams = Convert.ToInt32(xi - 2.0);

                var curveParams = new List<double>();

                for (int ii = 0; ii < numParams; ii++)
                {
                    curveParams.Add((ii + 1.0)/(xi - 1.0));
                }

                int maxIterNum = 15;
                bool bUnbound = XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef);

                int iterNum = 0;
                for (; iterNum < maxIterNum; iterNum++)
                {
                    XYZ prevPoint = startPoint;
                    XYZ thisXYZ = null;
                    XYZ nextXYZ = null;

                    Vector<double> distValues = DenseVector.Create(numParams, (c) => 0.0);

                    Matrix<double> iterMat = DenseMatrix.Create(numParams, numParams, (r, c) => 0.0);
                    double maxDistVal = -1.0;
                    for (int iParam = 0; iParam < numParams; iParam++)
                    {
                        t = curveParams[iParam];

                        if (nextXYZ != null)
                            thisXYZ = nextXYZ;
                        else
                            thisXYZ = !bUnbound ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
 
                        double tNext = (iParam == numParams - 1) ?  1.0 : curveParams[iParam + 1];
                        nextXYZ = (iParam == numParams - 1) ? endPoint :
                                   !bUnbound ? crvRef.Evaluate(tNext, true) : crvRef.Evaluate(tNext * crvRef.Period, false);

                        distValues[iParam] = thisXYZ.DistanceTo(prevPoint) - thisXYZ.DistanceTo(nextXYZ);

                        if (Math.Abs(distValues[iParam]) > maxDistVal)
                            maxDistVal = Math.Abs(distValues[iParam]);
                        Transform thisDerivTrf = !bUnbound ? crvRef.ComputeDerivatives(t, true) : crvRef.ComputeDerivatives(t * crvRef.Period, false);
                        XYZ derivThis = thisDerivTrf.BasisX;
                        if (bUnbound)
                            derivThis = derivThis.Multiply(crvRef.Period);
                        double distPrev = thisXYZ.DistanceTo(prevPoint);
                        if (distPrev  > System.Double.Epsilon)
                        {
                           double valDeriv = (thisXYZ - prevPoint).DotProduct(derivThis) / distPrev;
                           iterMat[iParam, iParam] += valDeriv;
                           if (iParam > 0)
                           {
                               iterMat[iParam - 1, iParam] -= valDeriv;
                           }
                        }
                        double distNext = thisXYZ.DistanceTo(nextXYZ);
                        if (distNext> System.Double.Epsilon)
                        {
                            double valDeriv = (thisXYZ - nextXYZ).DotProduct(derivThis) / distNext;

                           iterMat[iParam, iParam] -= valDeriv;
                           if (iParam < numParams - 1)
                           {
                               iterMat[iParam + 1, iParam] += valDeriv;
                           }
                        }
                        prevPoint = thisXYZ;
                    }

                    Matrix<double> iterMatInvert = iterMat.Inverse();
                    Vector<double> changeValues = iterMatInvert.Multiply(distValues);

                    double dampingFactor = 1.0;

                    for (int iParam = 0; iParam < numParams; iParam++)
                    {
                        curveParams[iParam] -= dampingFactor * changeValues[iParam];

                        if (iParam == 0 && curveParams[iParam] < 0.000000001)
                        {
                            double oldValue = dampingFactor * changeValues[iParam] + curveParams[iParam];
                            while (curveParams[iParam] < 0.000000001)
                               curveParams[iParam] = 0.5 * (dampingFactor * changeValues[iParam] + curveParams[iParam]);
                            changeValues[iParam] = (oldValue - curveParams[iParam]) / dampingFactor;
                        }
                        else if (iParam > 0 &&  curveParams[iParam] < 0.000000001 + curveParams[iParam - 1])
                        {
                            for (; iParam > -1; iParam--)
                                curveParams[iParam] = dampingFactor * changeValues[iParam] + curveParams[iParam];

                            dampingFactor *= 0.5;
                            continue;
                        }
                        else if (iParam == numParams - 1 && curveParams[iParam] > 1.0 - 0.000000001)
                        {
                            double oldValue = dampingFactor * changeValues[iParam] + curveParams[iParam];
                            while (curveParams[iParam] > 1.0 - 0.000000001)
                                 curveParams[iParam] = 0.5 * (1.0 + dampingFactor * changeValues[iParam] + curveParams[iParam]);
                            changeValues[iParam] = (oldValue - curveParams[iParam]) / dampingFactor;
                        }
                    }
                    if (maxDistVal < 0.000000001)
                    {
                        for (int iParam = 0; iParam < numParams; iParam++)
                        {
                            t = curveParams[iParam];
                            thisXYZ = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                            result = FSharpList<Value>.Cons(Value.NewContainer(thisXYZ), result);

                        }
                        break;
                    }
                }
          
                if (iterNum == maxIterNum)
                    throw new Exception("could not solve for equal distances");

            }

            if (xi > 1.0 + System.Double.Epsilon)
            {
                result = FSharpList<Value>.Cons(Value.NewContainer(endPoint), result);
            }
            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("Evaluate Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Evaluates curve or edge at parameter.")]
    public class XyzOnCurveOrEdge : GeometryBase
    {
        public XyzOnCurveOrEdge()
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

            return Value.NewContainer(result);
        }
    }

}

