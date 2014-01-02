using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Dynamo.Nodes
{
    [NodeName("Transform Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_APPLY)]
    [NodeDescription("Returns the curve (c) transformed by the transform (t).")]
    [NodeSearchTags("move", "line")]
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

    [NodeName("Lines Through Points")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Create a series of lines through a list of points.")]
    [NodeSearchTags("passing", "thread", "polyline", "straight")]
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
                var l = DocumentManager.GetInstance().CurrentUIApplication.Application.Create.NewLineBound(enumerable.ElementAt(i), enumerable.ElementAt(i - 1));

                results = FSharpList<Value>.Cons(Value.NewContainer(l), results);
            }

            return Value.NewList(results);
        }
    }

    [NodeName("Curve By Points")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Create a new Curve by Points by passing in a list of Reference Points")]
    [NodeSearchTags("spline", "points", "reference", "hermite", "interpolate")]
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
    [NodeSearchTags("spline", "points", "reference", "hermite", "interpolate", "Approximate")]
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
                //c = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(refPtArr);
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
            c = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
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
    [NodeSearchTags("Convert", "Extract", "Geometry", "Curve", "Model", "Reference", "line")]
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
     
    [NodeName("Curve Loop")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates Curve Loop")]
    [NodeSearchTags("closed", "join", "profile")]
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
                        Curve flippedCurve = /* Line.CreateBound */ DocumentManager.GetInstance().CurrentUIApplication.Application.Create.NewLineBound(thisEnd, thisStart);
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
                                        Curve flippedCurve = /* Line.CreateBound */ DocumentManager.GetInstance().CurrentUIApplication.Application.Create.NewLineBound(prevEnd, thisStart);
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
    [NodeSearchTags("offset", "loop")]
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
    [NodeSearchTags("explode", "decompose", "profile")]
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
    [NodeSearchTags("copy", "move", "curve", "line", "perpendicular")]
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
    [NodeSearchTags("shatter", "subdivide", "divide", "domain")]
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

    [NodeName("Curve Derivatives")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Returns the derivatives of the curve at the parameter.")]
    [NodeSearchTags("tangent", "normal")]
    public class ComputeCurveDerivatives : GeometryBase
    {
        public ComputeCurveDerivatives()
        {
            InPortData.Add(new PortData("crv", "The curve to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("p", "The parameter to evaluate", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform describing the curve at the parameter(Transform)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve crv = (Curve)((Value.Container)args[0]).Item;
            double p = (double)((Value.Number)args[1]).Item;

            Transform t = Transform.Identity;

            if (crv != null)
            {
                t = crv.ComputeDerivatives(p, true);
                t.BasisX = t.BasisX.Normalize();
                t.BasisZ = t.BasisZ.Normalize();
                t.BasisY = t.BasisX.CrossProduct(t.BasisZ);
            }

            return Value.NewContainer(t);
        }

    }

    [NodeName("Transform on Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Evaluates tangent vector of curve or edge at parameter.")]
    [NodeSearchTags("tangent", "normal")]
    public class TangentTransformOnCurveOrEdge : GeometryBase
    {
        public TangentTransformOnCurveOrEdge()
        {
            InPortData.Add(new PortData("parameter", "The normalized parameter to evaluate at within 0..1 range except for closed curve", typeof(Value.Number)));
            InPortData.Add(new PortData("curve or edge", "The geometry curve or edge to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("transform", "tangent transform at parameter.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var parameter = ((Value.Number)args[0]).Item;

            var thisCurve = ((Value.Container)args[1]).Item as Curve;
            var thisEdge = (thisCurve != null) ? null : (((Value.Container)args[1]).Item as Edge);

            if (thisCurve == null && thisEdge == null && ((Value.Container)args[1]).Item is Reference)
            {
                var r = (Reference)((Value.Container)args[1]).Item;
                if (r != null)
                {
                    var refElem = DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(r.ElementId);
                    if (refElem != null)
                    {
                        GeometryObject geob = refElem.GetGeometryObjectFromReference(r);
                        thisEdge = geob as Edge;
                        if (thisEdge == null)
                            thisCurve = geob as Curve;
                    }
                }
            }

            var result = (thisCurve != null) ?
                (!XyzOnCurveOrEdge.curveIsReallyUnbound(thisCurve) ? thisCurve.ComputeDerivatives(parameter, true) : thisCurve.ComputeDerivatives(parameter, false))
                :
                (thisEdge == null ? null : thisEdge.ComputeDerivatives(parameter));




            return Value.NewContainer(result);
        }
    }

    [NodeName("Approximate By Tangent Arcs")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_FIT)]
    [NodeDescription("Appoximates curve by sequence of tangent arcs.")]
    [NodeSearchTags("rationalize", "subdivide")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class ApproximateByTangentArcs : RevitTransactionNodeWithOneOutput
    {
        public ApproximateByTangentArcs()
        {
            InPortData.Add(new PortData("curve", "Curve to Approximate by Tangent Arcs", typeof(Value.Container)));
            OutPortData.Add(new PortData("arcs", "List of Approximating Arcs", typeof(Value.List)));

            RegisterAllPorts();
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve thisCurve = (Curve)((Value.Container)args[0]).Item;

            if (thisCurve == null)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }


            Type CurveType = typeof(Autodesk.Revit.DB.Curve);

            MethodInfo[] curveInstanceMethods = CurveType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodApproximateByTangentArcs = "ApproximateByTangentArcs";
            List<Curve> resultArcs = null;
            var result = FSharpList<Value>.Empty;

            foreach (MethodInfo m in curveInstanceMethods)
            {
                if (m.Name == nameOfMethodApproximateByTangentArcs)
                {
                    object[] argsM = new object[0];

                    resultArcs = (List<Curve>)m.Invoke(thisCurve, argsM);

                    break;
                }
            }
            for (int indexCurve = resultArcs.Count - 1; indexCurve > -1; indexCurve--)
            {
                result = FSharpList<Value>.Cons(Value.NewContainer(resultArcs[indexCurve]), result);
            }

            return Value.NewList(result);
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

            var el = ((Value.Container)args[0]).Item as CurveElement;
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
                                DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(curveRef.ElementId)
                                                .GetGeometryObjectFromReference(curveRef);
            }


            var start = curve.get_EndParameter(0);
            var end = curve.get_EndParameter(1);

            return Value.NewContainer(DSCoreNodes.Domain.ByMinimumAndMaximum(start, end));
        }
    }
}

